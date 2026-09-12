using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Pay.Employees;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.HrOrders;

public sealed class PayrollHrOrderService : BaseService, IPayrollHrOrderService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IPayrollEmployeeService _employeeService;
    private readonly IQueryRepository<PayHrOrder> _orderQuery;
    private readonly ICommandRepository<PayHrOrder> _orderCommand;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly ICommandRepository<PayEmployee> _employeeCommand;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly ICommandRepository<PayEmployment> _employmentCommand;
    private readonly IQueryRepository<PayPayrollLine> _payrollLineQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<Position> _positionQuery;

    public PayrollHrOrderService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IPayrollEmployeeService employeeService,
        IQueryRepository<PayHrOrder> orderQuery,
        ICommandRepository<PayHrOrder> orderCommand,
        IQueryRepository<PayEmployee> employeeQuery,
        ICommandRepository<PayEmployee> employeeCommand,
        IQueryRepository<PayEmployment> employmentQuery,
        ICommandRepository<PayEmployment> employmentCommand,
        IQueryRepository<PayPayrollLine> payrollLineQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<Position> positionQuery,
        ILogger<PayrollHrOrderService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _employeeService = employeeService;
        _orderQuery = orderQuery;
        _orderCommand = orderCommand;
        _employeeQuery = employeeQuery;
        _employeeCommand = employeeCommand;
        _employmentQuery = employmentQuery;
        _employmentCommand = employmentCommand;
        _payrollLineQuery = payrollLineQuery;
        _departmentQuery = departmentQuery;
        _positionQuery = positionQuery;
    }

    public Task<Result<PagedResponse<PayrollHrOrderListDto>>> GetAllAsync(
        PayrollHrOrderListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();

            var specification = _queryBuilder.For<PayHrOrder>()
                .Where(x =>
                    (!filter.EmployeeId.HasValue || x.EmployeeId == filter.EmployeeId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.OrderType) || x.OrderType == filter.OrderType) &&
                    (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                    (!filter.DateFrom.HasValue || x.OrderDate >= filter.DateFrom.Value) &&
                    (!filter.DateTo.HasValue || x.OrderDate <= filter.DateTo.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.OrderNumber.ToLower().Contains(search) ||
                     x.Employee.LastName.ToLower().Contains(search) ||
                     x.Employee.FirstName.ToLower().Contains(search)))
                .As(x => new PayrollHrOrderListDto
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    OrderDate = x.OrderDate,
                    OrderType = x.OrderType,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName +
                                   (x.Employee.MiddleName != null ? " " + x.Employee.MiddleName : ""),
                    EffectiveDate = x.EffectiveDate,
                    StatusId = x.StatusId,
                    PositionId = x.PositionId,
                    PositionName = x.Position != null ? x.Position.Name : null,
                    MonthlySalary = x.MonthlySalary
                })
                .OrderBy(x => x.OrderByDescending(y => y.OrderDate).ThenByDescending(y => y.Id))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();

            var paged = await _orderQuery.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollHrOrderDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<PayrollHrOrderDto>(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PayrollHrOrderSaveDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var organizationId = _userContext.OrganizationId.Value;

            var orderType = dto.OrderType?.Trim().ToUpperInvariant();
            if (!PayrollHrOrderTypeConst.All.Contains(orderType))
                return Result.Failure<long>(PayrollErrors.HrOrderTypeInvalid(dto.OrderType, _userContext.LanguageId));

            if (!await _employeeQuery.AnyAsync(x => x.Id == dto.EmployeeId && x.OrganizationId == organizationId, ct))
                return Result.Failure<long>(PayrollErrors.NotFound("Employee", dto.EmployeeId, _userContext.LanguageId));

            var numberResult = await _documentNumberService.GetNextAsync(
                organizationId, DocumentTypeIdConst.HRORDER, dto.OrderDate.ToDateTime(TimeOnly.MinValue), ct);
            if (!numberResult.IsSuccess)
                return Result.Failure<long>(numberResult.Error);

            var entity = new PayHrOrder
            {
                OrganizationId = organizationId,
                OrderNumber = "K-" + numberResult.Value.DocumentNumber.PadLeft(6, '0'),
                StatusId = DocumentStatusIdConst.DRAFT,
                CreatedDate = DateTime.Now,
                CreatedByUserId = _userContext.Id
            };
            ApplyOrder(entity, dto, orderType!);
            await _orderCommand.CreateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(entity.Id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayHrOrder, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PayrollHrOrderSaveDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var entity = await GetOrderEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.HrOrderNotEditable(id, entity.StatusId, _userContext.LanguageId));

            var orderType = dto.OrderType?.Trim().ToUpperInvariant();
            if (!PayrollHrOrderTypeConst.All.Contains(orderType))
                return Result.Failure(PayrollErrors.HrOrderTypeInvalid(dto.OrderType, _userContext.LanguageId));
            if (!await _employeeQuery.AnyAsync(x => x.Id == dto.EmployeeId && x.OrganizationId == entity.OrganizationId, ct))
                return Result.Failure(PayrollErrors.NotFound("Employee", dto.EmployeeId, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            ApplyOrder(entity, dto, orderType!);
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _orderCommand.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayHrOrder, id.ToString(), AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entity = await GetOrderEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.HrOrderNotEditable(id, entity.StatusId, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            await _orderCommand.DeleteAsync(entity, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.PayHrOrder, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            var entity = await GetOrderEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.InvalidStatus("HrOrder", id, entity.StatusId, "confirm", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));

            // Delegate the personnel action to the shared employee service; the nested
            // transaction (UnitOfWork depth) keeps everything atomic with the order status.
            long? employmentId;
            switch (entity.OrderType)
            {
                case PayrollHrOrderTypeConst.Hire:
                {
                    var r = await _employeeService.AddEmploymentAsync(entity.EmployeeId, BuildEmploymentSaveDto(entity), ct);
                    if (!r.IsSuccess) return Result.Failure(r.Error);
                    employmentId = r.Value;
                    break;
                }
                case PayrollHrOrderTypeConst.Transfer:
                {
                    var r = await _employeeService.TransferAsync(entity.EmployeeId, BuildTransferDto(entity), ct);
                    if (!r.IsSuccess) return Result.Failure(r.Error);
                    employmentId = r.Value;
                    break;
                }
                case PayrollHrOrderTypeConst.PayChange:
                {
                    var r = await _employeeService.ChangePayAsync(entity.EmployeeId, BuildPayChangeDto(entity), ct);
                    if (!r.IsSuccess) return Result.Failure(r.Error);
                    employmentId = r.Value;
                    break;
                }
                case PayrollHrOrderTypeConst.Dismissal:
                {
                    var current = await GetCurrentEmploymentAsync(entity.EmployeeId, ct);
                    var r = await _employeeService.DismissAsync(entity.EmployeeId, BuildDismissDto(entity), ct);
                    if (!r.IsSuccess) return Result.Failure(r.Error);
                    employmentId = current?.Id;
                    break;
                }
                default:
                    return Result.Failure(PayrollErrors.HrOrderTypeInvalid(entity.OrderType, _userContext.LanguageId));
            }

            entity.EmploymentId = employmentId;
            entity.StatusId = DocumentStatusIdConst.POSTED;
            entity.ConfirmedDate = DateTime.Now;
            entity.ConfirmedByUserId = _userContext.Id;
            await _orderCommand.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayHrOrder, id.ToString(), AuditLogOperationTypeConst.Update, "HR order confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var entity = await GetOrderEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(PayrollErrors.InvalidStatus("HrOrder", id, entity.StatusId, "cancel", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));

            var reversal = await ReverseAsync(entity, ct);
            if (!reversal.IsSuccess)
                return reversal;

            entity.StatusId = DocumentStatusIdConst.CANCELLED;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _orderCommand.UpdateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayHrOrder, id.ToString(), AuditLogOperationTypeConst.Update, "HR order cancelled");
            return Result.Success();
        }, ct);

    public Task<Result<PayrollHrOrderPrintDto>> GetPrintAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetPrintAsync), async () =>
        {
            var dto = await GetPrintInternalAsync(id, ct);
            if (dto is null)
                return Result.Failure<PayrollHrOrderPrintDto>(PayrollErrors.NotFound("HrOrder", id, _userContext.LanguageId));

            // Eski holat: tasdiqlangan bo'lsa hosil bo'lgan intervalning predecessori;
            // aks holda joriy ochiq interval.
            var current = await GetCurrentEmploymentAsync(dto.EmployeeId, ct);
            var basis = current;
            if (dto.StatusId == DocumentStatusIdConst.POSTED)
            {
                var predecessor = (await _employmentQuery.GetAllAsync(
                    _queryBuilder.For<PayEmployment>()
                        .Where(x => x.EmployeeId == dto.EmployeeId && x.EndDate == dto.EffectiveDate.AddDays(-1))
                        .Build(), ct))
                    .OrderByDescending(x => x.StartDate).FirstOrDefault();
                basis = predecessor ?? current;
            }

            if (basis is not null)
            {
                dto.FromMonthlySalary = basis.MonthlySalary;
                dto.FromDepartmentName = await GetDepartmentNameAsync(basis.DepartmentId, ct);
                dto.FromPositionName = await GetPositionNameAsync(basis.PositionId, ct);
            }
            return Result.Success(dto);
        });

    // ── Reversal ────────────────────────────────────────────────────────────
    private async Task<Result> ReverseAsync(PayHrOrder order, CancellationToken ct)
    {
        switch (order.OrderType)
        {
            case PayrollHrOrderTypeConst.Hire:
            case PayrollHrOrderTypeConst.Transfer:
            case PayrollHrOrderTypeConst.PayChange:
            {
                if (order.EmploymentId is null)
                    return Result.Success();
                if (await _payrollLineQuery.AnyAsync(x =>
                        x.EmploymentId == order.EmploymentId.Value &&
                        x.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED, ct))
                    return Result.Failure(PayrollErrors.HrOrderCancelLocked(order.Id, _userContext.LanguageId));

                var created = await GetEmploymentByIdAsync(order.EmploymentId.Value, ct);
                if (order.OrderType != PayrollHrOrderTypeConst.Hire)
                {
                    var predecessor = (await _employmentQuery.GetAllAsync(
                        _queryBuilder.For<PayEmployment>()
                            .Where(x => x.EmployeeId == order.EmployeeId &&
                                        x.EndDate == order.EffectiveDate.AddDays(-1) &&
                                        x.StateId == StateIdConst.ACTIVE)
                            .Build(), ct))
                        .OrderByDescending(x => x.StartDate).FirstOrDefault();
                    if (predecessor is not null)
                    {
                        predecessor.EndDate = null;
                        predecessor.UpdatedDate = DateTime.Now;
                        predecessor.UpdatedByUserId = _userContext.Id;
                        await _employmentCommand.UpdateAsync(predecessor, ct);
                    }
                }

                // FK: null the order link before deleting the produced interval.
                order.EmploymentId = null;
                await _orderCommand.UpdateAsync(order, ct);
                if (created is not null)
                    await _employmentCommand.DeleteAsync(created, ct);
                return Result.Success();
            }
            case PayrollHrOrderTypeConst.Dismissal:
            {
                // Reopening extends coverage past the dismissal date — block if a posted
                // payroll already covers a period beyond it.
                if (await _payrollLineQuery.AnyAsync(x =>
                        x.EmployeeId == order.EmployeeId &&
                        x.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED &&
                        x.PayrollDoc.Period.EndDate > order.EffectiveDate, ct))
                    return Result.Failure(PayrollErrors.HrOrderCancelLocked(order.Id, _userContext.LanguageId));

                if (order.EmploymentId is not null)
                {
                    var closed = await GetEmploymentByIdAsync(order.EmploymentId.Value, ct);
                    if (closed is not null)
                    {
                        closed.EndDate = null;
                        closed.UpdatedDate = DateTime.Now;
                        closed.UpdatedByUserId = _userContext.Id;
                        await _employmentCommand.UpdateAsync(closed, ct);
                    }
                }
                var employee = await GetEmployeeByIdAsync(order.EmployeeId, ct);
                if (employee is not null)
                {
                    employee.StateId = StateIdConst.ACTIVE;
                    employee.UpdatedDate = DateTime.Now;
                    employee.UpdatedByUserId = _userContext.Id;
                    await _employeeCommand.UpdateAsync(employee, ct);
                }
                return Result.Success();
            }
            default:
                return Result.Failure(PayrollErrors.HrOrderTypeInvalid(order.OrderType, _userContext.LanguageId));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void ApplyOrder(PayHrOrder entity, PayrollHrOrderSaveDto dto, string orderType)
    {
        entity.OrderDate = dto.OrderDate;
        entity.OrderType = orderType;
        entity.EmployeeId = dto.EmployeeId;
        entity.EffectiveDate = dto.EffectiveDate;
        entity.Basis = string.IsNullOrWhiteSpace(dto.Basis) ? null : dto.Basis.Trim();
        entity.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
        entity.DepartmentId = dto.DepartmentId;
        entity.PositionId = dto.PositionId;
        entity.EmploymentType = string.IsNullOrWhiteSpace(dto.EmploymentType) ? null : dto.EmploymentType.Trim().ToUpperInvariant();
        entity.MonthlySalary = dto.MonthlySalary;
        entity.EmploymentRate = dto.EmploymentRate;
        entity.WeeklyHours = dto.WeeklyHours;
        entity.CurrencyId = dto.CurrencyId;
        entity.ExpenseAccountId = dto.ExpenseAccountId;
        entity.AdvanceMethod = string.IsNullOrWhiteSpace(dto.AdvanceMethod) ? null : dto.AdvanceMethod.Trim().ToUpperInvariant();
        entity.AdvanceValue = dto.AdvanceValue;
    }

    private static PayrollEmploymentSaveDto BuildEmploymentSaveDto(PayHrOrder o) => new()
    {
        DepartmentId = o.DepartmentId,
        PositionId = o.PositionId,
        EmploymentType = string.IsNullOrWhiteSpace(o.EmploymentType) ? PayrollEmploymentTypeConst.Primary : o.EmploymentType,
        StartDate = o.EffectiveDate,
        EndDate = null,
        MonthlySalary = o.MonthlySalary ?? 0m,
        EmploymentRate = o.EmploymentRate ?? 1m,
        WeeklyHours = o.WeeklyHours ?? 40m,
        CurrencyId = o.CurrencyId ?? 0,
        ExpenseAccountId = o.ExpenseAccountId,
        AdvanceMethod = o.AdvanceMethod ?? PayrollAdvanceMethodConst.Percent,
        AdvanceValue = o.AdvanceValue ?? 0m,
        Note = o.Note
    };

    private static PayrollEmploymentTransferDto BuildTransferDto(PayHrOrder o) => new()
    {
        EffectiveDate = o.EffectiveDate,
        DepartmentId = o.DepartmentId,
        PositionId = o.PositionId,
        EmploymentType = o.EmploymentType,
        MonthlySalary = o.MonthlySalary,
        EmploymentRate = o.EmploymentRate,
        WeeklyHours = o.WeeklyHours,
        CurrencyId = o.CurrencyId,
        ExpenseAccountId = o.ExpenseAccountId,
        AdvanceMethod = o.AdvanceMethod,
        AdvanceValue = o.AdvanceValue,
        Note = o.Note
    };

    private static PayrollEmploymentPayChangeDto BuildPayChangeDto(PayHrOrder o) => new()
    {
        EffectiveDate = o.EffectiveDate,
        MonthlySalary = o.MonthlySalary ?? 0m,
        EmploymentRate = o.EmploymentRate,
        Note = o.Note
    };

    private static PayrollEmploymentDismissDto BuildDismissDto(PayHrOrder o) => new()
    {
        EffectiveDate = o.EffectiveDate,
        Note = o.Note
    };

    private async Task<PayHrOrder?> GetOrderEntityAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayHrOrder>().Where(x => x.Id == id).Build();
        return await _orderQuery.GetAsync(query, ct);
    }

    private async Task<PayEmployment?> GetEmploymentByIdAsync(long id, CancellationToken ct) =>
        await _employmentQuery.GetAsync(_queryBuilder.For<PayEmployment>().Where(x => x.Id == id).Build(), ct);

    private async Task<PayEmployee?> GetEmployeeByIdAsync(long id, CancellationToken ct) =>
        await _employeeQuery.GetAsync(_queryBuilder.For<PayEmployee>().Where(x => x.Id == id).Build(), ct);

    private async Task<PayEmployment?> GetCurrentEmploymentAsync(long employeeId, CancellationToken ct)
    {
        var open = await _employmentQuery.GetAllAsync(
            _queryBuilder.For<PayEmployment>()
                .Where(x => x.EmployeeId == employeeId && x.StateId == StateIdConst.ACTIVE && x.EndDate == null)
                .Build(), ct);
        return open.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault();
    }

    private async Task<string?> GetDepartmentNameAsync(int? departmentId, CancellationToken ct) =>
        departmentId is null ? null : await _departmentQuery.GetAsync(
            _queryBuilder.For<Department>().Where(x => x.Id == departmentId).As(x => x.Name).Build(), ct);

    private async Task<string?> GetPositionNameAsync(int? positionId, CancellationToken ct) =>
        positionId is null ? null : await _positionQuery.GetAsync(
            _queryBuilder.For<Position>().Where(x => x.Id == positionId).As(x => x.Name).Build(), ct);

    private async Task<PayrollHrOrderDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayHrOrder>()
            .Where(x => x.Id == id)
            .As(x => new PayrollHrOrderDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                OrderType = x.OrderType,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName +
                               (x.Employee.MiddleName != null ? " " + x.Employee.MiddleName : ""),
                EffectiveDate = x.EffectiveDate,
                StatusId = x.StatusId,
                Basis = x.Basis,
                Note = x.Note,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.Name : null,
                PositionId = x.PositionId,
                PositionName = x.Position != null ? x.Position.Name : null,
                EmploymentType = x.EmploymentType,
                MonthlySalary = x.MonthlySalary,
                EmploymentRate = x.EmploymentRate,
                WeeklyHours = x.WeeklyHours,
                CurrencyId = x.CurrencyId,
                CurrencyName = x.Currency != null ? x.Currency.Name : null,
                ExpenseAccountId = x.ExpenseAccountId,
                ExpenseAccountNumber = x.ExpenseAccount != null ? x.ExpenseAccount.Number : null,
                AdvanceMethod = x.AdvanceMethod,
                AdvanceValue = x.AdvanceValue,
                EmploymentId = x.EmploymentId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                ConfirmedDate = x.ConfirmedDate
            })
            .Build();
        return await _orderQuery.GetAsync(query, ct);
    }

    private async Task<PayrollHrOrderDto> GetRequiredDtoInternalAsync(long id, CancellationToken ct) =>
        await GetDtoInternalAsync(id, ct)
        ?? throw new InvalidOperationException("HR order audit snapshot is unavailable.");

    private async Task<PayrollHrOrderPrintDto?> GetPrintInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayHrOrder>()
            .Where(x => x.Id == id)
            .As(x => new PayrollHrOrderPrintDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                OrderType = x.OrderType,
                EffectiveDate = x.EffectiveDate,
                StatusId = x.StatusId,
                Basis = x.Basis,
                Note = x.Note,
                OrganizationName = x.Organization.FullName,
                EmployeeId = x.EmployeeId,
                EmployeeNumber = x.Employee.EmployeeNumber,
                EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName +
                               (x.Employee.MiddleName != null ? " " + x.Employee.MiddleName : ""),
                Pinfl = x.Employee.Pinfl,
                ToDepartmentName = x.Department != null ? x.Department.Name : null,
                ToPositionName = x.Position != null ? x.Position.Name : null,
                ToMonthlySalary = x.MonthlySalary,
                CurrencyName = x.Currency != null ? x.Currency.Name : null,
                EmploymentRate = x.EmploymentRate,
                ConfirmedByName = x.ConfirmedByUser != null ? x.ConfirmedByUser.LastName + " " + x.ConfirmedByUser.FirstName : null,
                ConfirmedDate = x.ConfirmedDate
            })
            .Build();
        return await _orderQuery.GetAsync(query, ct);
    }
}
