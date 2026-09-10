using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace Application.Features.Pay.Periods;

public sealed class PayrollPeriodService : BaseService, IPayrollPeriodService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PayPeriod> _query;
    private readonly ICommandRepository<PayPeriod> _command;
    private readonly ICommandRepository<PayPeriodWorkDay> _workDayCommand;
    private readonly IQueryRepository<PayTimesheet> _timesheetQuery;
    private readonly IQueryRepository<PayPayrollDoc> _payrollQuery;
    private readonly IQueryRepository<PayPayrollRecalculation> _recalculationQuery;
    private readonly IQueryRepository<PayPaymentBatch> _paymentQuery;
    private readonly IPayrollPeriodLock _periodLock;

    public PayrollPeriodService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<PayPeriod> query,
        ICommandRepository<PayPeriod> command,
        ICommandRepository<PayPeriodWorkDay> workDayCommand,
        IQueryRepository<PayTimesheet> timesheetQuery,
        IQueryRepository<PayPayrollDoc> payrollQuery,
        IQueryRepository<PayPayrollRecalculation> recalculationQuery,
        IQueryRepository<PayPaymentBatch> paymentQuery,
        IPayrollPeriodLock periodLock,
        ILogger<PayrollPeriodService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _workDayCommand = workDayCommand;
        _timesheetQuery = timesheetQuery;
        _payrollQuery = payrollQuery;
        _recalculationQuery = recalculationQuery;
        _paymentQuery = paymentQuery;
        _periodLock = periodLock;
    }

    public Task<Result<PagedResponse<PayrollPeriodDto>>> GetAllAsync(PayrollPeriodListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var specification = _queryBuilder.For<PayPeriod>()
                .Where(x =>
                    (!filter.Year.HasValue || x.PeriodYear == filter.Year.Value) &&
                    (string.IsNullOrWhiteSpace(filter.Status) || x.Status == filter.Status))
                .As(ListDtoSelector)
                .OrderBy(x => x.OrderByDescending(y => y.Year).ThenByDescending(y => y.Month))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            ApplyEditPolicy(paged.Items);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollPeriodDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).As(DetailDtoSelector).Build();
            var dto = await _query.GetAsync(query, ct);
            if (dto is null)
                return Result.Failure<PayrollPeriodDto>(PayrollErrors.NotFound("Period", id, _userContext.LanguageId));

            ApplyEditPolicy(dto);
            return Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PayrollPeriodCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            if (await _query.AnyAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.PeriodYear == dto.Year &&
                    x.PeriodMonth == dto.Month, ct))
                return Result.Failure<long>(PayrollErrors.Conflict("PeriodConflict", $"{dto.Year:D4}-{dto.Month:D2} uchun oylik hisoblash davri allaqachon mavjud.", _userContext.LanguageId));

            var calendar = CalculateCalendar(dto);
            var start = new DateOnly(dto.Year, dto.Month, 1);
            var entity = new PayPeriod
            {
                OrganizationId = organizationId,
                PeriodYear = dto.Year,
                PeriodMonth = dto.Month,
                StartDate = start,
                EndDate = start.AddMonths(1).AddDays(-1),
                DailyWorkHours = dto.DailyWorkHours,
                NormWorkDays = calendar.NormWorkDays,
                NormWorkHours = calendar.NormWorkHours,
                Status = PayrollPeriodStatusConst.Open,
                CreatedDate = DateTime.Now,
                WorkDays = BuildWorkDays(organizationId, calendar.CalendarDays)
            };
            await _command.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity, false));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPeriod, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PayrollPeriodUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            await _periodLock.AcquireAsync(id, ct);

            var entityQuery = _queryBuilder.For<PayPeriod>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build();
            entityQuery.AddIncludes(x => x.Include(period => period.WorkDays));
            var entity = await _query.GetAsync(entityQuery, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Period", id, _userContext.LanguageId));

            if (entity.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(id, _userContext.LanguageId));

            if (await IsUsedInTimesheetAsync(id, ct))
                return Result.Failure(PayrollErrors.Conflict(
                    "PeriodUsedInTimesheet",
                    "Oylik davri tabelda ishlatilgan va uni o‘zgartirib bo‘lmaydi.",
                    _userContext.LanguageId));

            if (await _query.AnyAsync(x =>
                    x.Id != id &&
                    x.OrganizationId == organizationId &&
                    x.PeriodYear == dto.Year &&
                    x.PeriodMonth == dto.Month, ct))
                return Result.Failure(PayrollErrors.Conflict(
                    "PeriodConflict",
                    $"{dto.Year:D4}-{dto.Month:D2} uchun oylik hisoblash davri allaqachon mavjud.",
                    _userContext.LanguageId));

            var calendar = CalculateCalendar(dto);
            var start = new DateOnly(dto.Year, dto.Month, 1);

            _auditLogService.SetOldValues(MapDto(entity, false));
            if (entity.WorkDays.Count > 0)
                await _workDayCommand.DeleteAsync(entity.WorkDays, ct);

            entity.PeriodYear = dto.Year;
            entity.PeriodMonth = dto.Month;
            entity.StartDate = start;
            entity.EndDate = start.AddMonths(1).AddDays(-1);
            entity.DailyWorkHours = dto.DailyWorkHours;
            entity.NormWorkDays = calendar.NormWorkDays;
            entity.NormWorkHours = calendar.NormWorkHours;
            entity.WorkDays = BuildWorkDays(organizationId, calendar.CalendarDays, id);
            await _command.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues(MapDto(entity, false));
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PayPeriod,
                id.ToString(),
                AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> CloseAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CloseAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Period", id, _userContext.LanguageId));
            if (entity.Status == PayrollPeriodStatusConst.Closed)
                return Result.Success();

            if (await HasUnfinishedDocumentsAsync(id, ct))
                return Result.Failure(PayrollErrors.Business("PeriodHasDraftDocuments", "Oylik davrida yakunlanmagan tabel, oylik hisoblash yoki to‘lov hujjatlari mavjud.", _userContext.LanguageId));

            if (await HasBlockingRecalculationAsync(id, ct))
                return Result.Failure(PayrollErrors.RecalculationBlocksPeriodClose(id, _userContext.LanguageId));

            var isUsedInTimesheet = await IsUsedInTimesheetAsync(id, ct);
            _auditLogService.SetOldValues(MapDto(entity, isUsedInTimesheet));
            entity.Status = PayrollPeriodStatusConst.Closed;
            entity.ClosedDate = DateTime.Now;
            entity.ClosedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity, isUsedInTimesheet));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPeriod, id.ToString(), AuditLogOperationTypeConst.Update, "Closed");
            return Result.Success();
        }, ct);

    public Task<Result> ReopenAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ReopenAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Period", id, _userContext.LanguageId));
            if (entity.Status == PayrollPeriodStatusConst.Open)
                return Result.Success();

            var isUsedInTimesheet = await IsUsedInTimesheetAsync(id, ct);
            _auditLogService.SetOldValues(MapDto(entity, isUsedInTimesheet));
            entity.Status = PayrollPeriodStatusConst.Open;
            entity.ClosedDate = null;
            entity.ClosedByUserId = null;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity, isUsedInTimesheet));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPeriod, id.ToString(), AuditLogOperationTypeConst.Update, "Reopened");
            return Result.Success();
        }, ct);

    private async Task<PayPeriod?> GetEntityAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(period => period.WorkDays));
        return await _query.GetAsync(query, ct);
    }

    private Task<bool> IsUsedInTimesheetAsync(long id, CancellationToken ct) =>
        _timesheetQuery.AnyAsync(PayrollPeriodEditPolicy.UsagePredicate(id), ct);

    private async Task<bool> HasUnfinishedDocumentsAsync(long id, CancellationToken ct) =>
        await _timesheetQuery.AnyAsync(x =>
            x.PeriodId == id && x.StateId == StateIdConst.ACTIVE &&
            (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING), ct)
        || await _payrollQuery.AnyAsync(x =>
            x.PeriodId == id && x.StateId == StateIdConst.ACTIVE &&
            (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING), ct)
        || await _paymentQuery.AnyAsync(x =>
            x.PeriodId == id && x.StateId == StateIdConst.ACTIVE &&
            (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING), ct);

    private async Task<bool> HasBlockingRecalculationAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPayrollRecalculation>()
            .Where(x => x.PayrollDoc.PeriodId == id)
            .As(x => new { x.Id, x.PayrollDocId, x.Status })
            .Build();
        var requests = await _recalculationQuery.GetAllAsync(query, ct);

        // A failed attempt is resolved by a later successful attempt for the
        // same source document. Only the latest request per document controls
        // period close; this preserves the failed row for audit without
        // permanently locking the period.
        return requests
            .GroupBy(x => x.PayrollDocId)
            .Select(group => group.OrderByDescending(x => x.Id).First().Status)
            .Any(PayrollPeriodClosePolicy.HasBlockingRecalculation);
    }

    private static PayrollPeriodCalendarResult CalculateCalendar(PayrollPeriodSaveDto dto) =>
        dto.CalendarDays is { Count: > 0 }
            ? PayrollPeriodCalendarCalculator.CalculateFromCalendarDays(dto.Year, dto.Month, dto.DailyWorkHours, dto.CalendarDays)
            : PayrollPeriodCalendarCalculator.Calculate(dto.Year, dto.Month, dto.DailyWorkHours, dto.WorkDates);

    private static List<PayPeriodWorkDay> BuildWorkDays(
        int organizationId,
        IEnumerable<PayrollPeriodCalendarDaySaveDto> calendarDays,
        long periodId = 0) =>
        calendarDays
            .Select(day => new PayPeriodWorkDay
            {
                OrganizationId = organizationId,
                PeriodId = periodId,
                WorkDate = day.Date,
                DayType = day.DayType,
                IsWorkDay = day.DayType != PayrollPeriodDayTypeConst.Holiday && day.WorkHours > 0m,
                WorkHours = day.WorkHours
            })
            .ToList();

    private static void ApplyEditPolicy(IEnumerable<PayrollPeriodDto> periods)
    {
        foreach (var period in periods)
            ApplyEditPolicy(period);
    }

    private static void ApplyEditPolicy(PayrollPeriodDto period)
    {
        period.CanEdit = PayrollPeriodEditPolicy.CanEdit(period.Status, period.IsUsedInTimesheet);
        period.EditBlockedReason = PayrollPeriodEditPolicy.GetBlockedReason(period.Status, period.IsUsedInTimesheet);
    }

    private static PayrollPeriodDto MapDto(PayPeriod x, bool isUsedInTimesheet) =>
        new()
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Year = x.PeriodYear,
            Month = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            DailyWorkHours = x.DailyWorkHours,
            WorkDates = x.WorkDays.Where(day => day.IsWorkDay).OrderBy(day => day.WorkDate).Select(day => day.WorkDate).ToList(),
            CalendarDays = x.WorkDays.OrderBy(day => day.WorkDate).Select(day => new PayrollPeriodCalendarDaySaveDto
            {
                Date = day.WorkDate,
                DayType = day.DayType,
                WorkHours = day.WorkHours
            }).ToList(),
            NormWorkDays = x.NormWorkDays,
            NormWorkHours = x.NormWorkHours,
            Status = x.Status,
            IsUsedInTimesheet = isUsedInTimesheet,
            CanEdit = PayrollPeriodEditPolicy.CanEdit(x.Status, isUsedInTimesheet),
            EditBlockedReason = PayrollPeriodEditPolicy.GetBlockedReason(x.Status, isUsedInTimesheet),
            CreatedDate = x.CreatedDate,
            ClosedDate = x.ClosedDate
        };

    private static readonly Expression<Func<PayPeriod, PayrollPeriodDto>> ListDtoSelector = x =>
        new PayrollPeriodDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Year = x.PeriodYear,
            Month = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            DailyWorkHours = x.DailyWorkHours,
            NormWorkDays = x.NormWorkDays,
            NormWorkHours = x.NormWorkHours,
            Status = x.Status,
            IsUsedInTimesheet = x.Timesheets.Any(timesheet => timesheet.StateId == StateIdConst.ACTIVE),
            CreatedDate = x.CreatedDate,
            ClosedDate = x.ClosedDate
        };

    private static readonly Expression<Func<PayPeriod, PayrollPeriodDto>> DetailDtoSelector = x =>
        new PayrollPeriodDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Year = x.PeriodYear,
            Month = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            DailyWorkHours = x.DailyWorkHours,
            WorkDates = x.WorkDays.Where(day => day.IsWorkDay).OrderBy(day => day.WorkDate).Select(day => day.WorkDate).ToList(),
            CalendarDays = x.WorkDays.OrderBy(day => day.WorkDate).Select(day => new PayrollPeriodCalendarDaySaveDto
            {
                Date = day.WorkDate,
                DayType = day.DayType,
                WorkHours = day.WorkHours
            }).ToList(),
            NormWorkDays = x.NormWorkDays,
            NormWorkHours = x.NormWorkHours,
            Status = x.Status,
            IsUsedInTimesheet = x.Timesheets.Any(timesheet => timesheet.StateId == StateIdConst.ACTIVE),
            CreatedDate = x.CreatedDate,
            ClosedDate = x.ClosedDate
        };
}
