using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Pay.Timesheets;

public sealed class PayrollTimesheetService : BaseService, IPayrollTimesheetService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocNumberGenerator _docNumberGenerator;
    private readonly IQueryRepository<PayTimesheet> _query;
    private readonly ICommandRepository<PayTimesheet> _command;
    private readonly ICommandRepository<PayTimesheetLine> _lineCommand;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<PayPayrollDoc> _payrollQuery;

    public PayrollTimesheetService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocNumberGenerator docNumberGenerator,
        IQueryRepository<PayTimesheet> query,
        ICommandRepository<PayTimesheet> command,
        ICommandRepository<PayTimesheetLine> lineCommand,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<PayPayrollDoc> payrollQuery,
        ILogger<PayrollTimesheetService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _docNumberGenerator = docNumberGenerator;
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _periodQuery = periodQuery;
        _employeeQuery = employeeQuery;
        _payrollQuery = payrollQuery;
    }

    public Task<Result<PagedResponse<PayrollTimesheetListDto>>> GetAllAsync(
        PayrollTimesheetListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var specification = new PagedQuerySpecification<PayTimesheet, PayrollTimesheetListDto>
            {
                Criteria = x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    (!filter.PeriodId.HasValue || x.PeriodId == filter.PeriodId.Value) &&
                    (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.DocNumber.ToLower().Contains(search) ||
                     (x.Note != null && x.Note.ToLower().Contains(search))),
                Selector = x => new PayrollTimesheetListDto
                {
                    Id = x.Id,
                    DocNumber = x.DocNumber,
                    DocDate = x.DocDate,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    EmployeeCount = x.Lines.Count,
                    Note = x.Note
                },
                OrderBy = x => x.OrderByDescending(y => y.DocDate).ThenByDescending(y => y.Id),
                Skip = (page - 1) * take,
                Take = take
            };
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollTimesheetDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<PayrollTimesheetDto>(PayrollErrors.NotFound("Timesheet", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PayrollTimesheetCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var period = await GetOpenPeriodAsync(dto.PeriodId, ct);
            if (period is null)
                return Result.Failure<long>(PayrollErrors.PeriodClosed(dto.PeriodId));

            if (await _query.AnyAsync(x =>
                    x.PeriodId == dto.PeriodId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure<long>(PayrollErrors.Conflict("TimesheetConflict", $"An active timesheet already exists for period {dto.PeriodId}."));

            var linesResult = await BuildLinesAsync(dto.Lines, period, organizationId, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var now = DateTime.Now;
            var entity = new PayTimesheet
            {
                OrganizationId = organizationId,
                PeriodId = dto.PeriodId,
                DocNumber = await _docNumberGenerator.GenerateAsync(organizationId, "TSH", dto.DocDate, ct),
                DocDate = dto.DocDate,
                StatusId = DocumentStatusIdConst.DRAFT,
                Note = dto.Note,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                Lines = linesResult.Value
            };
            await _command.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(entity.Id, ct));
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
                return Result.Failure(PayrollErrors.InvalidStatus("Timesheet", id, entity.StatusId, "updated"));

            var period = await GetOpenPeriodAsync(dto.PeriodId, ct);
            if (period is null)
                return Result.Failure(PayrollErrors.PeriodClosed(dto.PeriodId));

            if (dto.PeriodId != entity.PeriodId &&
                await _query.AnyAsync(x =>
                    x.Id != id &&
                    x.PeriodId == dto.PeriodId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure(PayrollErrors.Conflict("TimesheetConflict", $"An active timesheet already exists for period {dto.PeriodId}."));

            var linesResult = await BuildLinesAsync(dto.Lines, period, entity.OrganizationId, ct);
            if (!linesResult.IsSuccess)
                return linesResult;

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            await _lineCommand.DeleteAsync(x => x.TimesheetId == id, ct);
            foreach (var line in linesResult.Value)
                line.TimesheetId = id;
            await _lineCommand.CreateAsync(linesResult.Value, ct);

            entity.PeriodId = dto.PeriodId;
            entity.DocDate = dto.DocDate;
            entity.Note = dto.Note;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, id.ToString(), AuditLogOperationTypeConst.Update);
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
                return Result.Failure(PayrollErrors.InvalidStatus("Timesheet", id, entity.StatusId, "confirmed"));
            if (entity.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(entity.PeriodId));
            if (entity.Lines.Count == 0)
                return Result.Failure(PayrollErrors.Business("EmptyTimesheet", "Timesheet must contain at least one employee."));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.PostedAt = DateTime.Now;
            entity.PostedByUserId = _userContext.Id;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
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
                return Result.Failure(PayrollErrors.PeriodClosed(entity.PeriodId));
            if (await _payrollQuery.AnyAsync(x =>
                    x.PeriodId == entity.PeriodId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure(PayrollErrors.Conflict("TimesheetUsedByPayroll", "Timesheet cannot be cancelled while an active payroll document exists for the period."));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            entity.CancelledAt = DateTime.Now;
            entity.CancelledByUserId = _userContext.Id;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayTimesheet, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            return Result.Success();
        }, ct);

    private async Task<Result<List<PayTimesheetLine>>> BuildLinesAsync(
        List<PayrollTimesheetLineSaveDto> dtos,
        PayPeriod period,
        int organizationId,
        CancellationToken ct)
    {
        var duplicateEmployee = dtos.GroupBy(x => x.EmployeeId).FirstOrDefault(x => x.Count() > 1);
        if (duplicateEmployee is not null)
            return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Conflict("DuplicateTimesheetEmployee", $"Employee {duplicateEmployee.Key} is duplicated in the timesheet."));

        foreach (var dto in dtos)
        {
            var totalDays = dto.WorkedDays + dto.LeaveDays + dto.SickDays + dto.AbsentDays;
            if (totalDays > period.NormWorkDays)
                return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.Business("TimesheetDaysExceeded", $"Employee {dto.EmployeeId} has {totalDays} days while period norm is {period.NormWorkDays}."));
        }

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
            return Result.Failure<List<PayTimesheetLine>>(PayrollErrors.NoActiveEmployment(missingEmployeeId));

        return Result.Success(dtos.Select(dto => new PayTimesheetLine
        {
            OrganizationId = organizationId,
            EmployeeId = dto.EmployeeId,
            WorkedDays = dto.WorkedDays,
            WorkedHours = dto.WorkedHours,
            LeaveDays = dto.LeaveDays,
            SickDays = dto.SickDays,
            AbsentDays = dto.AbsentDays,
            OvertimeHours = dto.OvertimeHours,
            Note = dto.Note
        }).ToList());
    }

    private async Task<PayPeriod?> GetOpenPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>()
            .Where(x => x.Id == id && x.Status == PayrollPeriodStatusConst.Open)
            .Build();
        return await _periodQuery.GetAsync(query, ct);
    }

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
                        LeaveDays = line.LeaveDays,
                        SickDays = line.SickDays,
                        AbsentDays = line.AbsentDays,
                        OvertimeHours = line.OvertimeHours,
                        Note = line.Note
                    }).ToList()
            })
            .Build();
        return await _query.GetAsync(query, ct);
    }
}
