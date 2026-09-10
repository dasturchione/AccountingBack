using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Hr.Calendar;
using Application.Features.Pay.Periods;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.Timesheets;

public sealed class PayrollTimesheetService : BaseService, IPayrollTimesheetService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<PayTimesheet> _query;
    private readonly ICommandRepository<PayTimesheet> _command;
    private readonly ICommandRepository<PayTimesheetLine> _lineCommand;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<PayPayrollDoc> _payrollQuery;
    private readonly IQueryRepository<HrAbsenceType> _absenceTypeQuery;
    private readonly IHrEmployeeCalendarService _calendarService;
    private readonly IPayrollPeriodLock _periodLock;

    public PayrollTimesheetService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<PayTimesheet> query,
        ICommandRepository<PayTimesheet> command,
        ICommandRepository<PayTimesheetLine> lineCommand,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<PayPayrollDoc> payrollQuery,
        IQueryRepository<HrAbsenceType> absenceTypeQuery,
        IHrEmployeeCalendarService calendarService,
        IPayrollPeriodLock periodLock,
        ILogger<PayrollTimesheetService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _periodQuery = periodQuery;
        _employeeQuery = employeeQuery;
        _payrollQuery = payrollQuery;
        _absenceTypeQuery = absenceTypeQuery;
        _calendarService = calendarService;
        _periodLock = periodLock;
    }

    public Task<Result<PagedResponse<PayrollTimesheetListDto>>> GetAllAsync(
        PayrollTimesheetListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var specification = _queryBuilder.For<PayTimesheet>()
                .Where(x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    (!filter.PeriodId.HasValue || x.PeriodId == filter.PeriodId.Value) &&
                    (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.DocNumber.ToLower().Contains(search) ||
                     (x.Note != null && x.Note.ToLower().Contains(search))))
                .As(x => new PayrollTimesheetListDto
                {
                    Id = x.Id,
                    DocNumber = x.DocNumber,
                    DocDate = x.DocDate,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    EmployeeCount = x.Lines.Count,
                    TotalWorkedDays = x.Lines.Sum(line => line.WorkedDays),
                    TotalWorkedHours = x.Lines.Sum(line => line.WorkedHours),
                    Note = x.Note
                })
                .OrderBy(x => x.OrderByDescending(y => y.DocDate).ThenByDescending(y => y.Id))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollTimesheetDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            if (dto is null)
                return Result.Failure<PayrollTimesheetDto>(
                    PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));

            var calendarResult = await BuildDocumentCalendarAsync(dto, ct);
            if (!calendarResult.IsSuccess)
                return Result.Failure<PayrollTimesheetDto>(calendarResult.Error);

            dto.Calendar = calendarResult.Value;
            return Result.Success(dto);
        });

    public Task<Result<HrEmployeeCalendarDto>> GetEmployeeCalendarAsync(
        long periodId,
        long employeeId,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetEmployeeCalendarAsync), async () =>
        {
            var period = await GetPeriodAsync(periodId, ct);
            if (period is null)
                return Result.Failure<HrEmployeeCalendarDto>(
                    PayrollErrors.NotFound("Period", periodId, _userContext.LanguageId));

            return await _calendarService.GetAsync(employeeId, period.StartDate, period.EndDate, ct);
        });

    public Task<Result<IReadOnlyList<PayrollAttendanceStatusOptionDto>>> GetAttendanceStatusOptionsAsync(
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAttendanceStatusOptionsAsync), async () =>
        {
            var absenceOptions = await GetActiveAbsenceStatusOptionsAsync(ct);
            IReadOnlyList<PayrollAttendanceStatusOptionDto> options =
                PayrollAttendanceStatusOptions.GetFixed(_userContext.LanguageId)
                    .Concat(absenceOptions)
                    .ToList();
            return Result.Success(options);
        });

    public Task<Result<PayrollTimesheetCalendarDto>> GetCalendarTableAsync(
        long periodId,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetCalendarTableAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PayrollTimesheetCalendarDto>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var period = await GetPeriodAsync(periodId, ct);
            if (period is null)
                return Result.Failure<PayrollTimesheetCalendarDto>(
                    PayrollErrors.NotFound("Period", periodId, _userContext.LanguageId));

            var employeeIds = await GetActiveEmployeeIdsAsync(
                _userContext.OrganizationId.Value,
                period,
                ct);
            var calendarsResult = await _calendarService.GetManyAsync(
                employeeIds,
                period.StartDate,
                period.EndDate,
                ct);
            if (!calendarsResult.IsSuccess)
                return Result.Failure<PayrollTimesheetCalendarDto>(calendarsResult.Error);

            var periodCalendars = calendarsResult.Value
                .Select(calendar => PayrollPeriodCalendarOverlay.Apply(calendar, period))
                .ToList();

            return Result.Success(PayrollTimesheetCalendarBuilder.Build(
                null,
                period.Id,
                GetPeriodName(period),
                period.StartDate,
                period.EndDate,
                periodCalendars));
        });

    public Task<Result<PayrollTimesheetCalendarDto>> GetDocumentCalendarAsync(
        long id,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDocumentCalendarAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            if (dto is null)
                return Result.Failure<PayrollTimesheetCalendarDto>(
                    PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));

            return await BuildDocumentCalendarAsync(dto, ct);
        });

    public Task<Result<long>> CreateAsync(PayrollTimesheetCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            await _periodLock.AcquireAsync(dto.PeriodId, ct);
            var period = await GetOpenPeriodAsync(dto.PeriodId, ct);
            if (period is null)
                return Result.Failure<long>(PayrollErrors.PeriodClosed(dto.PeriodId, _userContext.LanguageId));

            if (await _query.AnyAsync(x =>
                    x.PeriodId == dto.PeriodId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure<long>(PayrollErrors.Conflict("TimesheetConflict", $"Ushbu davr uchun faol tabel allaqachon mavjud (davr ID: {dto.PeriodId}).", _userContext.LanguageId));

            var linesResult = await BuildLinesAsync(dto.Lines, period, organizationId, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.PAYROLLTIMESHEET,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var entity = new PayTimesheet
            {
                OrganizationId = organizationId,
                PeriodId = dto.PeriodId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                StatusId = DocumentStatusIdConst.DRAFT,
                Note = dto.Note,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                Lines = linesResult.Value
            };
            await _command.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(entity.Id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PayrollTimesheetUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entityQuery = _queryBuilder.For<PayTimesheet>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(entityQuery, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.InvalidStatus("Timesheet", id, entity.StatusId, "updated", _userContext.LanguageId));

            if (dto.PeriodId != entity.PeriodId)
                return Result.Failure(PayrollErrors.Business(
                    "TimesheetPeriodImmutable",
                    "Tabelning oylik davrini o'zgartirib bo'lmaydi.",
                    _userContext.LanguageId));

            var existingDto = await GetDtoInternalAsync(id, ct);
            if (existingDto is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));
            if (existingDto.Lines.Any(line => line.IsLegacy))
                return Result.Failure(PayrollErrors.Business(
                    "TimesheetDaysInitializationRequired",
                    "Eski tabel qatorlarini tahrirlashdan oldin kunlarni boshlang'ich holatga keltiring.",
                    _userContext.LanguageId));

            var period = await GetOpenPeriodAsync(entity.PeriodId, ct);
            if (period is null)
                return Result.Failure(PayrollErrors.PeriodClosed(entity.PeriodId, _userContext.LanguageId));

            var existingSnapshots = existingDto.Lines.ToDictionary(
                line => line.EmployeeId,
                line => (IReadOnlyDictionary<DateOnly, PayrollTimesheetDayDto>)line.Days.ToDictionary(day => day.Date));
            var existingNorms = existingDto.Lines.ToDictionary(
                line => line.EmployeeId,
                line => (line.NormWorkDays, line.NormWorkHours));

            var linesResult = await BuildLinesAsync(
                dto.Lines,
                period,
                entity.OrganizationId,
                ct,
                existingSnapshots,
                existingNorms);
            if (!linesResult.IsSuccess)
                return linesResult;

            _auditLogService.SetOldValues(existingDto);
            await _lineCommand.DeleteAsync(x => x.TimesheetId == id, ct);
            foreach (var line in linesResult.Value)
                line.TimesheetId = id;
            await _lineCommand.CreateAsync(linesResult.Value, ct);

            entity.DocDate = dto.DocDate;
            entity.Note = dto.Note;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, id.ToString(), AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> InitializeDaysAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(InitializeDaysAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var existingDto = await GetDtoInternalAsync(id, ct);
            if (existingDto is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));
            if (existingDto.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.InvalidStatus(
                    "Timesheet",
                    id,
                    existingDto.StatusId,
                    "updated",
                    _userContext.LanguageId));
            if (existingDto.Lines.Count == 0 || existingDto.Lines.Any(line => !line.IsLegacy))
                return Result.Failure(PayrollErrors.Business(
                    "TimesheetDaysAlreadyInitialized",
                    "Kunlik qatorlar faqat eski tabel uchun boshlang'ich holatga keltiriladi.",
                    _userContext.LanguageId));

            var period = await GetOpenPeriodAsync(existingDto.PeriodId, ct);
            if (period is null)
                return Result.Failure(PayrollErrors.PeriodClosed(existingDto.PeriodId, _userContext.LanguageId));

            var employeeIds = existingDto.Lines.Select(line => line.EmployeeId).ToList();
            var calendarsResult = await _calendarService.GetManyAsync(
                employeeIds,
                period.StartDate,
                period.EndDate,
                ct);
            if (!calendarsResult.IsSuccess)
                return Result.Failure(calendarsResult.Error);

            var calendars = calendarsResult.Value
                .Select(calendar => PayrollPeriodCalendarOverlay.Apply(calendar, period))
                .ToDictionary(calendar => calendar.EmployeeId);
            var lineInputs = new List<PayrollTimesheetLineSaveDto>();
            foreach (var line in existingDto.Lines)
            {
                if (!calendars.TryGetValue(line.EmployeeId, out var calendar))
                    return Result.Failure(PayrollErrors.NoActiveEmployment(line.EmployeeId, _userContext.LanguageId));

                lineInputs.Add(new PayrollTimesheetLineSaveDto
                {
                    EmployeeId = line.EmployeeId,
                    OvertimeHours = line.OvertimeHours,
                    Note = line.Note,
                    Days = calendar.Days.Select(day => new PayrollTimesheetDaySaveDto
                    {
                        Date = day.Date,
                        StatusCode = day.StatusCode,
                        AbsenceTypeId = day.AbsenceTypeId,
                        PlannedHours = day.PlannedHours
                    }).ToList()
                });
            }

            var linesResult = await BuildLinesAsync(
                lineInputs,
                period,
                existingDto.OrganizationId,
                ct,
                employeeCalendars: calendars);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var entityQuery = _queryBuilder.For<PayTimesheet>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(entityQuery, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));

            _auditLogService.SetOldValues(existingDto);
            await _lineCommand.DeleteAsync(line => line.TimesheetId == id, ct);
            foreach (var line in linesResult.Value)
                line.TimesheetId = id;
            await _lineCommand.CreateAsync(linesResult.Value, ct);

            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PayTimesheet,
                id.ToString(),
                AuditLogOperationTypeConst.Update,
                "Initialized daily snapshots");
            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            var entity = await GetAggregateAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();
            if (entity.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
                return Result.Failure(PayrollErrors.InvalidStatus("Timesheet", id, entity.StatusId, "confirmed", _userContext.LanguageId));
            if (entity.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(entity.PeriodId, _userContext.LanguageId));
            if (entity.Lines.Count == 0)
                return Result.Failure(PayrollErrors.Business("EmptyTimesheet", "Tabelda kamida bitta xodim bo‘lishi kerak.", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.PostedAt = DateTime.Now;
            entity.PostedByUserId = _userContext.Id;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var entity = await GetAggregateAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId));
            if (entity.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (entity.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(entity.PeriodId, _userContext.LanguageId));
            if (await _payrollQuery.AnyAsync(x =>
                    x.PeriodId == entity.PeriodId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure(PayrollErrors.Conflict("TimesheetUsedByPayroll", "Ushbu davr uchun faol oylik hisoblash hujjati mavjudligi sababli tabelni bekor qilib bo‘lmaydi.", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            entity.CancelledAt = DateTime.Now;
            entity.CancelledByUserId = _userContext.Id;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            return Result.Success();
        }, ct);

    private async Task<Result<List<PayTimesheetLine>>> BuildLinesAsync(
        List<PayrollTimesheetLineSaveDto> dtos,
        PayPeriod period,
        int organizationId,
        CancellationToken ct,
        IReadOnlyDictionary<long, IReadOnlyDictionary<DateOnly, PayrollTimesheetDayDto>>? existingSnapshots = null,
        IReadOnlyDictionary<long, (decimal NormWorkDays, decimal NormWorkHours)>? existingNorms = null,
        IReadOnlyDictionary<long, HrEmployeeCalendarDto>? employeeCalendars = null)
    {
        var duplicateEmployee = dtos.GroupBy(x => x.EmployeeId).FirstOrDefault(x => x.Count() > 1);
        if (duplicateEmployee is not null)
            return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Conflict("DuplicateTimesheetEmployee", $"Xodim tabelda takroran kiritilgan (xodim ID: {duplicateEmployee.Key}).", _userContext.LanguageId));

        var expectedDates = PayrollTimesheetDayCoverage.CreateExpected(period.StartDate, period.EndDate);
        var incompleteCoverage = dtos.FirstOrDefault(dto =>
            !PayrollTimesheetDayCoverage.IsComplete(expectedDates, dto.Days.Select(day => day.Date)));
        if (incompleteCoverage is not null)
            return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                "TimesheetDayCoverageIncomplete",
                $"Tabel har bir sana uchun bitta kunlik qatorni o'z ichiga olishi kerak (xodim ID: {incompleteCoverage.EmployeeId}).",
                _userContext.LanguageId));

        var employeeIds = dtos.Select(x => x.EmployeeId).Distinct().ToList();
        var employeesQuery = _queryBuilder.For<PayEmployee>()
            .Where(x =>
                employeeIds.Contains(x.Id) &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.Employments.Any(e =>
                    e.StateId == StateIdConst.ACTIVE &&
                    e.StartDate <= period.EndDate &&
                    (!e.EndDate.HasValue || e.EndDate.Value >= period.StartDate)))
            .As(x => x.Id)
            .Build();
        var validEmployeeIds = await _employeeQuery.GetAllAsync(employeesQuery, ct);
        var missingEmployeeId = employeeIds.Except(validEmployeeIds).FirstOrDefault();
        if (missingEmployeeId > 0)
            return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.NoActiveEmployment(missingEmployeeId, _userContext.LanguageId));

        var employeeIdsNeedingCalendar = dtos
            .Where(dto => existingSnapshots is null || !existingSnapshots.ContainsKey(dto.EmployeeId))
            .Select(dto => dto.EmployeeId)
            .Distinct()
            .ToList();
        IReadOnlyDictionary<long, HrEmployeeCalendarDto> calendars = employeeCalendars ??
            new Dictionary<long, HrEmployeeCalendarDto>();
        if (employeeCalendars is null && employeeIdsNeedingCalendar.Count > 0)
        {
            var calendarsResult = await _calendarService.GetManyAsync(
                employeeIdsNeedingCalendar,
                period.StartDate,
                period.EndDate,
                ct);
            if (!calendarsResult.IsSuccess)
                return Result.Failure<List<PayTimesheetLine>>(calendarsResult.Error);

            calendars = calendarsResult.Value.ToDictionary(calendar => calendar.EmployeeId);
        }

        var absenceOptions = await GetActiveAbsenceStatusOptionsAsync(ct);
        // Existing snapshots must remain editable even if an HR absence type was
        // later deactivated. New selections still come only from active options.
        var activeAbsenceIds = absenceOptions
            .Where(option => option.AbsenceTypeId.HasValue)
            .Select(option => option.AbsenceTypeId!.Value)
            .ToHashSet();
        var snapshotAbsenceIds = (existingSnapshots is null
                ? Enumerable.Empty<IReadOnlyDictionary<DateOnly, PayrollTimesheetDayDto>>()
                : existingSnapshots.Values)
            .SelectMany(days => days.Values)
            .Where(day => day.AbsenceTypeId.HasValue)
            .Select(day => day.AbsenceTypeId!.Value)
            .Concat(dtos.SelectMany(dto => dto.Days)
                .Where(day => day.AbsenceTypeId.HasValue)
                .Select(day => day.AbsenceTypeId!.Value))
            .Where(id => !activeAbsenceIds.Contains(id))
            .Distinct()
            .ToList();
        if (snapshotAbsenceIds.Count > 0)
            absenceOptions.AddRange(await GetAbsenceStatusOptionsByIdsAsync(snapshotAbsenceIds, ct));
        var lines = new List<PayTimesheetLine>();
        foreach (var dto in dtos)
        {
            IReadOnlyDictionary<DateOnly, PayrollTimesheetDayDto>? existingDays = null;
            if (existingSnapshots is not null)
                existingSnapshots.TryGetValue(dto.EmployeeId, out existingDays);

            IReadOnlyDictionary<DateOnly, HrEmployeeCalendarDayDto>? sourceDays = null;
            HrEmployeeCalendarDto? employeeCalendar = null;
            if (existingDays is not null)
            {
                if (!PayrollTimesheetDayCoverage.IsComplete(expectedDates, existingDays.Keys))
                    return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                        "TimesheetSnapshotCoverageIncomplete",
                        $"Saqlangan kunlik tabel surati to'liq emas (xodim ID: {dto.EmployeeId}).",
                        _userContext.LanguageId));
            }
            else
            {
                if (!calendars.TryGetValue(dto.EmployeeId, out var calendar))
                    return Result.Failure<List<PayTimesheetLine>>(
                        PayrollErrors.NoActiveEmployment(dto.EmployeeId, _userContext.LanguageId));

                employeeCalendar = PayrollPeriodCalendarOverlay.Apply(calendar, period);
                sourceDays = employeeCalendar.Days.ToDictionary(day => day.Date);
                if (!PayrollTimesheetDayCoverage.IsComplete(expectedDates, sourceDays.Keys))
                    return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                        "TimesheetSourceCoverageIncomplete",
                        $"Xodim uchun HR kalendari to'liq emas (xodim ID: {dto.EmployeeId}).",
                        _userContext.LanguageId));
            }

            var days = new List<PayTimesheetLineDay>();
            foreach (var submittedDay in dto.Days.OrderBy(day => day.Date))
            {
                PayrollTimesheetDayDto? existingDay = null;
                if (existingDays is not null)
                    existingDays.TryGetValue(submittedDay.Date, out existingDay);
                var sourceDay = existingDay is null ? sourceDays![submittedDay.Date] : null;

                PayrollAttendanceStatusOptionDto selectedStatus;
                try
                {
                    selectedStatus = PayrollAttendanceStatusResolver.Resolve(
                        submittedDay.StatusCode,
                        submittedDay.AbsenceTypeId,
                        absenceOptions);
                }
                catch (ArgumentException)
                {
                    return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                        "InvalidTimesheetAttendanceStatus",
                        $"Kunlik holat yoki yo'qlik turi noto'g'ri (xodim ID: {dto.EmployeeId}, sana: {submittedDay.Date:yyyy-MM-dd}).",
                        _userContext.LanguageId));
                }

                var workedHours = submittedDay.WorkedHours ?? existingDay?.WorkedHours ?? period.DailyWorkHours;
                var plannedHours = submittedDay.PlannedHours ?? existingDay?.PlannedHours ?? sourceDay?.PlannedHours ?? period.DailyWorkHours;
                if (selectedStatus.Code == HrCalendarStatusConst.Worked &&
                    (workedHours < 1m || workedHours > period.DailyWorkHours))
                    return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                        "TimesheetWorkedHoursRange",
                        $"Ishlagan kun soati 1 dan {period.DailyWorkHours} gacha bo‘lishi kerak (xodim ID: {dto.EmployeeId}, sana: {submittedDay.Date:yyyy-MM-dd}).",
                        _userContext.LanguageId));

                var specialHoursLimit = selectedStatus.Code == HrCalendarStatusConst.Worked ? workedHours : 0m;
                if (submittedDay.OvertimeHours < 0m || submittedDay.NightHours < 0m ||
                    submittedDay.HolidayHours < 0m || submittedDay.WeekendHours < 0m ||
                    submittedDay.OvertimeHours > specialHoursLimit || submittedDay.NightHours > specialHoursLimit ||
                    submittedDay.HolidayHours > specialHoursLimit || submittedDay.WeekendHours > specialHoursLimit)
                    return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business(
                        "TimesheetSpecialHoursRange",
                        $"Maxsus soatlar manfiy yoki ishlangan soatdan katta bo‘lishi mumkin emas (xodim ID: {dto.EmployeeId}, sana: {submittedDay.Date:yyyy-MM-dd}).",
                        _userContext.LanguageId));

                days.Add(new PayTimesheetLineDay
                {
                    OrganizationId = organizationId,
                    WorkDate = submittedDay.Date,
                    SourceStatusCode = existingDay?.SourceStatusCode ?? sourceDay!.StatusCode,
                    SourceAbsenceId = existingDay?.SourceAbsenceId ?? sourceDay!.AbsenceId,
                    SourceScheduleId = existingDay?.SourceScheduleId ?? sourceDay!.ScheduleId,
                    SourceAbsenceTypeId = existingDay?.SourceAbsenceTypeId ?? sourceDay!.AbsenceTypeId,
                    StatusCode = selectedStatus.Code,
                    AbsenceTypeId = selectedStatus.AbsenceTypeId,
                    // Keep the category captured when the snapshot was first
                    // created; HR type edits must not recalculate history.
                    TimesheetCategory = existingDay?.TimesheetCategory ?? selectedStatus.TimesheetCategory,
                    WorkedHours = selectedStatus.Code == HrCalendarStatusConst.Worked
                        ? decimal.Round(workedHours, 2)
                        : 0m,
                    PlannedHours = selectedStatus.Code == HrCalendarStatusConst.PlannedWork
                        ? decimal.Round(plannedHours, 2)
                        : 0m,
                    OvertimeHours = selectedStatus.Code == HrCalendarStatusConst.Worked ? decimal.Round(submittedDay.OvertimeHours, 2) : 0m,
                    NightHours = selectedStatus.Code == HrCalendarStatusConst.Worked ? decimal.Round(submittedDay.NightHours, 2) : 0m,
                    HolidayHours = selectedStatus.Code == HrCalendarStatusConst.Worked ? decimal.Round(submittedDay.HolidayHours, 2) : 0m,
                    WeekendHours = selectedStatus.Code == HrCalendarStatusConst.Worked ? decimal.Round(submittedDay.WeekendHours, 2) : 0m
                });
            }

            var totals = PayrollTimesheetDayCalculator.Calculate(
                period.DailyWorkHours,
                days.Select(day => new PayrollTimesheetDayValue(
                    day.StatusCode,
                    day.TimesheetCategory,
                    day.WorkedHours,
                    day.AbsenceTypeId.HasValue && absenceOptions.FirstOrDefault(x => x.AbsenceTypeId == day.AbsenceTypeId)?.IsPaid == true,
                    day.PlannedHours,
                    day.OvertimeHours,
                    day.NightHours,
                    day.HolidayHours,
                    day.WeekendHours)));

            (decimal NormWorkDays, decimal NormWorkHours)? snapshotNorms = null;
            if (existingNorms is not null && existingNorms.TryGetValue(dto.EmployeeId, out var existingNorm))
                snapshotNorms = existingNorm;
            var norms = PayrollTimesheetNormCalculator.Resolve(
                period,
                employeeCalendar,
                snapshotNorms?.NormWorkDays,
                snapshotNorms?.NormWorkHours,
                period.WorkDays
                    .Where(workDay => workDay.IsWorkDay && workDay.WorkHours > 0m)
                    .Select(workDay => workDay.WorkDate)
                    .ToArray());

            lines.Add(new PayTimesheetLine
            {
                OrganizationId = organizationId,
                EmployeeId = dto.EmployeeId,
                WorkedDays = totals.WorkedDays,
                WorkedHours = totals.WorkedHours,
                NormWorkDays = norms.NormWorkDays,
                NormWorkHours = norms.NormWorkHours,
                LeaveDays = totals.LeaveDays,
                SickDays = totals.SickDays,
                PaidLeaveDays = totals.PaidLeaveDays,
                PaidSickDays = totals.PaidSickDays,
                AbsentDays = totals.AbsentDays,
                OvertimeHours = dto.OvertimeHours > 0m ? decimal.Round(dto.OvertimeHours, 2) : totals.OvertimeHours,
                NightHours = totals.NightHours,
                HolidayHours = totals.HolidayHours,
                WeekendHours = totals.WeekendHours,
                Note = dto.Note,
                Days = days
            });
        }

        return Result.Success(lines);
    }

    private async Task<List<PayrollAttendanceStatusOptionDto>> GetActiveAbsenceStatusOptionsAsync(
        CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsenceType>()
            .Where(x => x.StateId == StateIdConst.ACTIVE)
            .As(x => new PayrollAttendanceStatusOptionDto
            {
                Code = x.Code,
                Name = x.Name,
                Kind = PayrollAttendanceStatusKindConst.Absence,
                AbsenceTypeId = x.Id,
                TimesheetCategory = x.TimesheetCategory,
                IsPaid = x.IsPaid
            })
            .Build();
        var options = await _absenceTypeQuery.GetAllAsync(query, ct);
        return options
            .OrderBy(option => option.Name)
            .ThenBy(option => option.Code)
            .ToList();
    }

    private async Task<List<PayrollAttendanceStatusOptionDto>> GetAbsenceStatusOptionsByIdsAsync(
        IReadOnlyCollection<short> ids,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsenceType>()
            .Where(x => ids.Contains(x.Id))
            .As(x => new PayrollAttendanceStatusOptionDto
            {
                Code = x.Code,
                Name = x.Name,
                Kind = PayrollAttendanceStatusKindConst.Absence,
                AbsenceTypeId = x.Id,
                TimesheetCategory = x.TimesheetCategory,
                IsPaid = x.IsPaid
            })
            .Build();
        return await _absenceTypeQuery.GetAllAsync(query, ct);
    }

    private async Task<PayPeriod?> GetOpenPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>()
            .Where(x => x.Id == id && x.Status == PayrollPeriodStatusConst.Open)
            .Build();
        query.AddIncludes(x => x.Include(period => period.WorkDays));
        return await _periodQuery.GetAsync(query, ct);
    }

    private async Task<PayPeriod?> GetPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(period => period.WorkDays));
        return await _periodQuery.GetAsync(query, ct);
    }

    private async Task<List<long>> GetActiveEmployeeIdsAsync(
        int organizationId,
        PayPeriod period,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployee>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.Employments.Any(employment =>
                    employment.StateId == StateIdConst.ACTIVE &&
                    employment.StartDate <= period.EndDate &&
                    (!employment.EndDate.HasValue || employment.EndDate.Value >= period.StartDate)))
            .As(x => x.Id)
            .Build();
        return await _employeeQuery.GetAllAsync(query, ct);
    }

    private async Task<Result<PayrollTimesheetCalendarDto>> BuildDocumentCalendarAsync(
        PayrollTimesheetDto dto,
        CancellationToken ct)
    {
        var period = await GetPeriodAsync(dto.PeriodId, ct);
        if (period is null)
            return Result.Failure<PayrollTimesheetCalendarDto>(
                PayrollErrors.NotFound("Period", dto.PeriodId, _userContext.LanguageId));

        var employeeIds = dto.Lines.Select(x => x.EmployeeId).Distinct().ToList();
        var calendarsResult = await _calendarService.GetManyAsync(
            employeeIds,
            period.StartDate,
            period.EndDate,
            ct);
        if (!calendarsResult.IsSuccess)
            return Result.Failure<PayrollTimesheetCalendarDto>(calendarsResult.Error);

        var lines = dto.Lines.ToDictionary(x => x.EmployeeId);
        var periodCalendars = calendarsResult.Value
            .Select(calendar => PayrollPeriodCalendarOverlay.Apply(calendar, period))
            .ToList();

        return Result.Success(PayrollTimesheetCalendarBuilder.Build(
            dto.Id,
            period.Id,
            dto.PeriodName,
            period.StartDate,
            period.EndDate,
            periodCalendars,
            lines));
    }

    private static string GetPeriodName(PayPeriod period) =>
        period.PeriodYear + "-" + period.PeriodMonth;

    private async Task<PayTimesheet?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayTimesheet>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(t => t.Period));
        query.AddIncludes(x => x.Include(t => t.Lines));
        return await _query.GetAsync(query, ct);
    }

    private async Task<PayrollTimesheetDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayTimesheet>()
            .Where(x => x.Id == id)
            .As(x => new PayrollTimesheetDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                DocNumber = x.DocNumber,
                DocDate = x.DocDate,
                PeriodId = x.PeriodId,
                PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                StatusId = x.StatusId,
                StatusName = x.Status.Name,
                EmployeeCount = x.Lines.Count,
                TotalWorkedDays = x.Lines.Sum(line => line.WorkedDays),
                TotalWorkedHours = x.Lines.Sum(line => line.WorkedHours),
                Note = x.Note,
                StateId = x.StateId,
                CreatedDate = x.CreatedDate,
                PostedAt = x.PostedAt,
                CancelledAt = x.CancelledAt,
                Lines = x.Lines
                    .OrderBy(line => line.Employee.LastName)
                    .ThenBy(line => line.Employee.FirstName)
                    .Select(line => new PayrollTimesheetLineDto
                    {
                        Id = line.Id,
                        EmployeeId = line.EmployeeId,
                        EmployeeNumber = line.Employee.EmployeeNumber,
                        EmployeeName = line.Employee.LastName + " " + line.Employee.FirstName,
                        WorkedDays = line.WorkedDays,
                        WorkedHours = line.WorkedHours,
                        NormWorkDays = line.NormWorkDays,
                        NormWorkHours = line.NormWorkHours,
                        LeaveDays = line.LeaveDays,
                        SickDays = line.SickDays,
                        PaidLeaveDays = line.PaidLeaveDays,
                        PaidSickDays = line.PaidSickDays,
                        AbsentDays = line.AbsentDays,
                        OvertimeHours = line.OvertimeHours,
                        NightHours = line.NightHours,
                        HolidayHours = line.HolidayHours,
                        WeekendHours = line.WeekendHours,
                        Note = line.Note,
                        IsLegacy = !line.Days.Any(),
                        Days = line.Days
                            .OrderBy(day => day.WorkDate)
                            .Select(day => new PayrollTimesheetDayDto
                            {
                                Date = day.WorkDate,
                                SourceStatusCode = day.SourceStatusCode,
                                SourceAbsenceId = day.SourceAbsenceId,
                                SourceScheduleId = day.SourceScheduleId,
                                SourceAbsenceTypeId = day.SourceAbsenceTypeId,
                                StatusCode = day.StatusCode,
                                StatusName = day.StatusCode,
                                AbsenceTypeId = day.AbsenceTypeId,
                                TimesheetCategory = day.TimesheetCategory,
                                WorkedHours = day.StatusCode == HrCalendarStatusConst.Worked
                                    ? day.WorkedHours
                                    : 0m,
                                PlannedHours = day.StatusCode == HrCalendarStatusConst.PlannedWork
                                    ? day.PlannedHours
                                    : 0m,
                                OvertimeHours = day.OvertimeHours,
                                NightHours = day.NightHours,
                                HolidayHours = day.HolidayHours,
                                WeekendHours = day.WeekendHours,
                                IsOverridden = day.StatusCode != day.SourceStatusCode ||
                                               day.AbsenceTypeId != day.SourceAbsenceTypeId
                            }).ToList()
                    }).ToList()
            })
            .Build();
        var dto = await _query.GetAsync(query, ct);
        if (dto is not null)
        {
            await PopulateDayDisplayValuesAsync(dto, ct);
            var period = await GetPeriodAsync(dto.PeriodId, ct);
            if (period is not null)
                ApplyPeriodPlannedHours(dto, period);
        }
        return dto;
    }

    private static void ApplyPeriodPlannedHours(PayrollTimesheetDto dto, PayPeriod period)
    {
        var workHoursByDate = period.WorkDays
            .GroupBy(day => day.WorkDate)
            .ToDictionary(group => group.Key, group => group.Last().WorkHours);

        foreach (var day in dto.Lines.SelectMany(line => line.Days))
        {
            if (day.StatusCode != HrCalendarStatusConst.PlannedWork || day.PlannedHours > 0m)
                continue;

            day.PlannedHours = workHoursByDate.TryGetValue(day.Date, out var hours) && hours > 0m
                ? hours
                : period.DailyWorkHours;
        }
    }

    private async Task PopulateDayDisplayValuesAsync(PayrollTimesheetDto dto, CancellationToken ct)
    {
        var absenceTypeIds = dto.Lines
            .SelectMany(line => line.Days)
            .Where(day => day.AbsenceTypeId.HasValue)
            .Select(day => day.AbsenceTypeId!.Value)
            .Distinct()
            .ToList();
        var absenceTypes = new Dictionary<short, PayrollAttendanceStatusOptionDto>();
        if (absenceTypeIds.Count > 0)
        {
            var query = _queryBuilder.For<HrAbsenceType>()
                .Where(type => absenceTypeIds.Contains(type.Id))
                .As(type => new PayrollAttendanceStatusOptionDto
                {
                    Code = type.Code,
                    Name = type.Name,
                    Kind = PayrollAttendanceStatusKindConst.Absence,
                    AbsenceTypeId = type.Id,
                    TimesheetCategory = type.TimesheetCategory,
                    IsPaid = type.IsPaid
                })
                .Build();
            absenceTypes = (await _absenceTypeQuery.GetAllAsync(query, ct))
                .ToDictionary(type => type.AbsenceTypeId!.Value);
        }

        foreach (var day in dto.Lines.SelectMany(line => line.Days))
        {
            if (PayrollAttendanceStatusOptions.TryGetFixed(
                    day.StatusCode,
                    out var fixedStatus,
                    _userContext.LanguageId))
            {
                day.StatusName = fixedStatus.Name;
                day.AbsenceTypeId = null;
                day.AbsenceTypeCode = null;
                day.AbsenceTypeName = null;
                day.TimesheetCategory = null;
                continue;
            }

            if (day.AbsenceTypeId.HasValue && absenceTypes.TryGetValue(day.AbsenceTypeId.Value, out var absenceType))
            {
                day.StatusName = absenceType.Name;
                day.AbsenceTypeCode = absenceType.Code;
                day.AbsenceTypeName = absenceType.Name;
                continue;
            }

            day.StatusName = day.StatusCode;
        }
    }

    private async Task<PayrollTimesheetDto> GetRequiredDtoInternalAsync(long id, CancellationToken ct) =>
        await GetDtoInternalAsync(id, ct)
        ?? throw new InvalidOperationException("Payroll timesheet audit snapshot is unavailable.");
}
