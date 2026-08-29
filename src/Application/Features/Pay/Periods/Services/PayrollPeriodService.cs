using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
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
    private readonly IQueryRepository<PayTimesheet> _timesheetQuery;
    private readonly IQueryRepository<PayPayrollDoc> _payrollQuery;
    private readonly IQueryRepository<PayPaymentBatch> _paymentQuery;

    public PayrollPeriodService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<PayPeriod> query,
        ICommandRepository<PayPeriod> command,
        IQueryRepository<PayTimesheet> timesheetQuery,
        IQueryRepository<PayPayrollDoc> payrollQuery,
        IQueryRepository<PayPaymentBatch> paymentQuery,
        ILogger<PayrollPeriodService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _timesheetQuery = timesheetQuery;
        _payrollQuery = payrollQuery;
        _paymentQuery = paymentQuery;
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
                .As(DtoSelector)
                .OrderBy(x => x.OrderByDescending(y => y.Year).ThenByDescending(y => y.Month))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollPeriodDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).As(DtoSelector).Build();
            var dto = await _query.GetAsync(query, ct);
            return dto is null
                ? Result.Failure<PayrollPeriodDto>(PayrollErrors.NotFound("Period", id, _userContext.LanguageId))
                : Result.Success(dto);
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

            var start = new DateOnly(dto.Year, dto.Month, 1);
            var entity = new PayPeriod
            {
                OrganizationId = organizationId,
                PeriodYear = dto.Year,
                PeriodMonth = dto.Month,
                StartDate = start,
                EndDate = start.AddMonths(1).AddDays(-1),
                NormWorkDays = dto.NormWorkDays,
                NormWorkHours = dto.NormWorkHours,
                Status = PayrollPeriodStatusConst.Open,
                CreatedDate = DateTime.Now
            };
            await _command.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPeriod, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
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

            _auditLogService.SetOldValues(MapDto(entity));
            entity.Status = PayrollPeriodStatusConst.Closed;
            entity.ClosedDate = DateTime.Now;
            entity.ClosedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity));
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

            _auditLogService.SetOldValues(MapDto(entity));
            entity.Status = PayrollPeriodStatusConst.Open;
            entity.ClosedDate = null;
            entity.ClosedByUserId = null;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(MapDto(entity));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPeriod, id.ToString(), AuditLogOperationTypeConst.Update, "Reopened");
            return Result.Success();
        }, ct);

    private async Task<PayPeriod?> GetEntityAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        return await _query.GetAsync(query, ct);
    }

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

    private static PayrollPeriodDto MapDto(PayPeriod x) =>
        new()
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Year = x.PeriodYear,
            Month = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            NormWorkDays = x.NormWorkDays,
            NormWorkHours = x.NormWorkHours,
            Status = x.Status,
            CreatedDate = x.CreatedDate,
            ClosedDate = x.ClosedDate
        };

    private static readonly Expression<Func<PayPeriod, PayrollPeriodDto>> DtoSelector = x =>
        new PayrollPeriodDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Year = x.PeriodYear,
            Month = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            NormWorkDays = x.NormWorkDays,
            NormWorkHours = x.NormWorkHours,
            Status = x.Status,
            CreatedDate = x.CreatedDate,
            ClosedDate = x.ClosedDate
        };
}
