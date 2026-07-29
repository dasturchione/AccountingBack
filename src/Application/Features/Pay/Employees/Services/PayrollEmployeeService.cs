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

namespace Application.Features.Pay.Employees;

public sealed class PayrollEmployeeService : BaseService, IPayrollEmployeeService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly ICommandRepository<PayEmployee> _employeeCommand;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly ICommandRepository<PayEmployment> _employmentCommand;
    private readonly IQueryRepository<PayEmployeeComponent> _assignmentQuery;
    private readonly ICommandRepository<PayEmployeeComponent> _assignmentCommand;
    private readonly IQueryRepository<PayComponent> _componentQuery;
    private readonly IQueryRepository<PayPayrollLine> _payrollLineQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<Position> _positionQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;

    public PayrollEmployeeService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<PayEmployee> employeeQuery,
        ICommandRepository<PayEmployee> employeeCommand,
        IQueryRepository<PayEmployment> employmentQuery,
        ICommandRepository<PayEmployment> employmentCommand,
        IQueryRepository<PayEmployeeComponent> assignmentQuery,
        ICommandRepository<PayEmployeeComponent> assignmentCommand,
        IQueryRepository<PayComponent> componentQuery,
        IQueryRepository<PayPayrollLine> payrollLineQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<Position> positionQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<ChartAccount> accountQuery,
        ILogger<PayrollEmployeeService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _employeeQuery = employeeQuery;
        _employeeCommand = employeeCommand;
        _employmentQuery = employmentQuery;
        _employmentCommand = employmentCommand;
        _assignmentQuery = assignmentQuery;
        _assignmentCommand = assignmentCommand;
        _componentQuery = componentQuery;
        _payrollLineQuery = payrollLineQuery;
        _departmentQuery = departmentQuery;
        _positionQuery = positionQuery;
        _currencyQuery = currencyQuery;
        _accountQuery = accountQuery;
    }

    public Task<Result<PagedResponse<PayrollEmployeeListDto>>> GetAllAsync(
        PayrollEmployeeListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var specification = new PagedQuerySpecification<PayEmployee, PayrollEmployeeListDto>
            {
                Criteria = x =>
                    (!filter.StateId.HasValue || x.StateId == filter.StateId.Value) &&
                    (!filter.DepartmentId.HasValue ||
                     x.Employments.Any(e => e.StateId == StateIdConst.ACTIVE && e.DepartmentId == filter.DepartmentId.Value)) &&
                    (!filter.PositionId.HasValue ||
                     x.Employments.Any(e => e.StateId == StateIdConst.ACTIVE && e.PositionId == filter.PositionId.Value)) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.EmployeeNumber.ToLower().Contains(search) ||
                     x.FirstName.ToLower().Contains(search) ||
                     x.LastName.ToLower().Contains(search) ||
                     (x.Pinfl != null && x.Pinfl.Contains(search))),
                Selector = x => new PayrollEmployeeListDto
                {
                    Id = x.Id,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.LastName + " " + x.FirstName + (x.MiddleName != null ? " " + x.MiddleName : ""),
                    Pinfl = x.Pinfl,
                    DepartmentId = x.Employments
                        .Where(e => e.StateId == StateIdConst.ACTIVE &&
                                    e.StartDate <= today &&
                                    (!e.EndDate.HasValue || e.EndDate.Value >= today))
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => e.DepartmentId)
                        .FirstOrDefault(),
                    DepartmentName = x.Employments
                        .Where(e => e.StateId == StateIdConst.ACTIVE &&
                                    e.StartDate <= today &&
                                    (!e.EndDate.HasValue || e.EndDate.Value >= today))
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => e.Department != null ? e.Department.Name : null)
                        .FirstOrDefault(),
                    PositionId = x.Employments
                        .Where(e => e.StateId == StateIdConst.ACTIVE &&
                                    e.StartDate <= today &&
                                    (!e.EndDate.HasValue || e.EndDate.Value >= today))
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => e.PositionId)
                        .FirstOrDefault(),
                    PositionName = x.Employments
                        .Where(e => e.StateId == StateIdConst.ACTIVE &&
                                    e.StartDate <= today &&
                                    (!e.EndDate.HasValue || e.EndDate.Value >= today))
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => e.Position != null ? e.Position.Name : null)
                        .FirstOrDefault(),
                    MonthlySalary = x.Employments
                        .Where(e => e.StateId == StateIdConst.ACTIVE &&
                                    e.StartDate <= today &&
                                    (!e.EndDate.HasValue || e.EndDate.Value >= today))
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => (decimal?)e.MonthlySalary)
                        .FirstOrDefault(),
                    StateId = x.StateId
                },
                OrderBy = x => x.OrderBy(y => y.FullName),
                Skip = (page - 1) * take,
                Take = take
            };

            var paged = await _employeeQuery.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollEmployeeDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<PayrollEmployeeDto>(PayrollErrors.NotFound("Employee", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PayrollEmployeeCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var uniqueness = await ValidateEmployeeUniquenessAsync(dto.EmployeeNumber, dto.Pinfl, organizationId, null, ct);
            if (!uniqueness.IsSuccess)
                return Result.Failure<long>(uniqueness.Error);

            var employmentValidation = await ValidateEmploymentAsync(0, dto.Employment, organizationId, null, ct);
            if (!employmentValidation.IsSuccess)
                return Result.Failure<long>(employmentValidation.Error);

            var now = DateTime.Now;
            var employee = new PayEmployee
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id
            };
            ApplyEmployee(employee, dto);
            employee.Employments.Add(BuildEmployment(organizationId, dto.Employment, now));

            await _employeeCommand.CreateAsync(employee, ct);
            var created = await GetDtoInternalAsync(employee.Id, ct);
            _auditLogService.SetNewValues(created);
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, employee.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(employee.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PayrollEmployeeUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await GetEmployeeEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Employee", id, _userContext.LanguageId));

            var uniqueness = await ValidateEmployeeUniquenessAsync(dto.EmployeeNumber, dto.Pinfl, entity.OrganizationId, id, ct);
            if (!uniqueness.IsSuccess)
                return uniqueness;

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            ApplyEmployee(entity, dto);
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;
            await _employeeCommand.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, id.ToString(), AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<PayEmployee>().Where(x => x.Id == id).Build();
            query.AddIncludes(x => x.Include(e => e.Employments));
            query.AddIncludes(x => x.Include(e => e.EmployeeComponents));
            var entity = await _employeeQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Employee", id, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            var now = DateTime.Now;
            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = now;
            entity.UpdatedByUserId = _userContext.Id;
            foreach (var employment in entity.Employments)
            {
                employment.StateId = StateIdConst.PASSIVE;
                employment.UpdatedDate = now;
            }
            foreach (var assignment in entity.EmployeeComponents)
            {
                assignment.StateId = StateIdConst.PASSIVE;
                assignment.UpdatedDate = now;
            }
            await _employeeCommand.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result<long>> AddEmploymentAsync(long employeeId, PayrollEmploymentSaveDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(AddEmploymentAsync), async () =>
        {
            var employee = await GetEmployeeEntityAsync(employeeId, ct);
            if (employee is null)
                return Result.Failure<long>(PayrollErrors.NotFound("Employee", employeeId, _userContext.LanguageId));

            var validation = await ValidateEmploymentAsync(employeeId, dto, employee.OrganizationId, null, ct);
            if (!validation.IsSuccess)
                return Result.Failure<long>(validation.Error);

            _auditLogService.SetOldValues(await GetDtoInternalAsync(employeeId, ct));
            var entity = BuildEmployment(employee.OrganizationId, dto, DateTime.Now);
            entity.EmployeeId = employeeId;
            await _employmentCommand.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(employeeId, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, employeeId.ToString(), AuditLogOperationTypeConst.Update, "Employment added");
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateEmploymentAsync(
        long employeeId,
        long employmentId,
        PayrollEmploymentSaveDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateEmploymentAsync), async () =>
        {
            var query = _queryBuilder.For<PayEmployment>()
                .Where(x => x.Id == employmentId && x.EmployeeId == employeeId)
                .Build();
            var entity = await _employmentQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Employment", employmentId, _userContext.LanguageId));

            if (await _payrollLineQuery.AnyAsync(x =>
                    x.EmploymentId == employmentId &&
                    x.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED, ct))
                return Result.Failure(PayrollErrors.Conflict("EmploymentLocked", "Employment used by posted payroll cannot be edited; close it and add a new employment record."));

            var validation = await ValidateEmploymentAsync(employeeId, dto, entity.OrganizationId, employmentId, ct);
            if (!validation.IsSuccess)
                return validation;

            _auditLogService.SetOldValues(await GetDtoInternalAsync(employeeId, ct));
            ApplyEmployment(entity, dto);
            entity.UpdatedDate = DateTime.Now;
            await _employmentCommand.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(employeeId, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, employeeId.ToString(), AuditLogOperationTypeConst.Update, "Employment updated");
            return Result.Success();
        }, ct);

    public Task<Result<long>> AssignComponentAsync(
        long employeeId,
        PayrollEmployeeComponentSaveDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(AssignComponentAsync), async () =>
        {
            var employee = await GetEmployeeEntityAsync(employeeId, ct);
            if (employee is null)
                return Result.Failure<long>(PayrollErrors.NotFound("Employee", employeeId, _userContext.LanguageId));

            if (!await _componentQuery.AnyAsync(x =>
                    x.Id == dto.ComponentId &&
                    x.OrganizationId == employee.OrganizationId &&
                    x.StateId == StateIdConst.ACTIVE, ct))
                return Result.Failure<long>(PayrollErrors.ReferencedRecordNotFound("Component", dto.ComponentId));

            if (await _assignmentQuery.AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.ComponentId == dto.ComponentId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    (!dto.EffectiveTo.HasValue || x.EffectiveFrom <= dto.EffectiveTo.Value) &&
                    (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= dto.EffectiveFrom), ct))
                return Result.Failure<long>(PayrollErrors.Conflict("EmployeeComponentOverlap", "The employee already has this component for an overlapping period."));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(employeeId, ct));
            var entity = new PayEmployeeComponent
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = employeeId,
                ComponentId = dto.ComponentId,
                Amount = dto.Amount,
                Rate = dto.Rate,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };
            await _assignmentCommand.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(employeeId, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, employeeId.ToString(), AuditLogOperationTypeConst.Update, "Payroll component assigned");
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> RemoveComponentAsync(long employeeId, long assignmentId, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RemoveComponentAsync), async () =>
        {
            var query = _queryBuilder.For<PayEmployeeComponent>()
                .Where(x => x.Id == assignmentId && x.EmployeeId == employeeId)
                .Build();
            var entity = await _assignmentQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("EmployeeComponent", assignmentId, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(employeeId, ct));
            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = DateTime.Now;
            await _assignmentCommand.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(employeeId, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayEmployee, employeeId.ToString(), AuditLogOperationTypeConst.Update, "Payroll component removed");
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateEmployeeUniquenessAsync(
        string employeeNumber,
        string? pinfl,
        int organizationId,
        long? currentId,
        CancellationToken ct)
    {
        var normalizedNumber = employeeNumber.Trim().ToUpperInvariant();
        if (await _employeeQuery.AnyAsync(x =>
                x.OrganizationId == organizationId &&
                x.Id != currentId &&
                x.EmployeeNumber == normalizedNumber, ct))
            return Result.Failure(PayrollErrors.DuplicateEmployeeNumber(normalizedNumber));

        var normalizedPinfl = string.IsNullOrWhiteSpace(pinfl) ? null : pinfl.Trim();
        if (normalizedPinfl is not null &&
            await _employeeQuery.AnyAsync(x =>
                x.OrganizationId == organizationId &&
                x.Id != currentId &&
                x.Pinfl == normalizedPinfl, ct))
            return Result.Failure(PayrollErrors.DuplicatePinfl(normalizedPinfl));

        return Result.Success();
    }

    private async Task<Result> ValidateEmploymentAsync(
        long employeeId,
        PayrollEmploymentSaveDto dto,
        int organizationId,
        long? currentEmploymentId,
        CancellationToken ct)
    {
        if (employeeId > 0 &&
            await _employmentQuery.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.Id != currentEmploymentId &&
                x.StateId == StateIdConst.ACTIVE &&
                (!dto.EndDate.HasValue || x.StartDate <= dto.EndDate.Value) &&
                (!x.EndDate.HasValue || x.EndDate.Value >= dto.StartDate), ct))
            return Result.Failure(PayrollErrors.EmploymentOverlap(employeeId));

        if (dto.DepartmentId.HasValue &&
            !await _departmentQuery.AnyAsync(x =>
                x.Id == dto.DepartmentId.Value &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("Department", dto.DepartmentId.Value));

        if (dto.PositionId.HasValue &&
            !await _positionQuery.AnyAsync(x =>
                x.Id == dto.PositionId.Value &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("Position", dto.PositionId.Value));

        if (!await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("Currency", dto.CurrencyId));

        if (dto.ExpenseAccountId.HasValue &&
            !await _accountQuery.AnyAsync(x =>
                x.Id == dto.ExpenseAccountId.Value &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("ChartAccount", dto.ExpenseAccountId.Value));

        return Result.Success();
    }

    private async Task<PayEmployee?> GetEmployeeEntityAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployee>().Where(x => x.Id == id).Build();
        return await _employeeQuery.GetAsync(query, ct);
    }

    private async Task<PayrollEmployeeDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployee>()
            .Where(x => x.Id == id)
            .As(x => new PayrollEmployeeDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                EmployeeNumber = x.EmployeeNumber,
                Pinfl = x.Pinfl,
                Tin = x.Tin,
                FirstName = x.FirstName,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                BirthDate = x.BirthDate,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                BankAccountNumber = x.BankAccountNumber,
                StateId = x.StateId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                Employments = x.Employments
                    .OrderByDescending(e => e.StartDate)
                    .Select(e => new PayrollEmploymentDto
                    {
                        Id = e.Id,
                        DepartmentId = e.DepartmentId,
                        DepartmentName = e.Department != null ? e.Department.Name : null,
                        PositionId = e.PositionId,
                        PositionName = e.Position != null ? e.Position.Name : null,
                        EmploymentType = e.EmploymentType,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        MonthlySalary = e.MonthlySalary,
                        EmploymentRate = e.EmploymentRate,
                        WeeklyHours = e.WeeklyHours,
                        CurrencyId = e.CurrencyId,
                        CurrencyName = e.Currency.Name,
                        ExpenseAccountId = e.ExpenseAccountId,
                        ExpenseAccountNumber = e.ExpenseAccount != null ? e.ExpenseAccount.Number : null,
                        StateId = e.StateId
                    }).ToList(),
                Components = x.EmployeeComponents
                    .OrderBy(e => e.Component.SortOrder)
                    .Select(e => new PayrollEmployeeComponentDto
                    {
                        Id = e.Id,
                        ComponentId = e.ComponentId,
                        ComponentCode = e.Component.Code,
                        ComponentName = e.Component.Name,
                        ComponentType = e.Component.ComponentType,
                        Amount = e.Amount,
                        Rate = e.Rate,
                        EffectiveFrom = e.EffectiveFrom,
                        EffectiveTo = e.EffectiveTo,
                        StateId = e.StateId
                    }).ToList()
            })
            .Build();
        return await _employeeQuery.GetAsync(query, ct);
    }

    private static PayEmployment BuildEmployment(int organizationId, PayrollEmploymentSaveDto dto, DateTime now)
    {
        var entity = new PayEmployment
        {
            OrganizationId = organizationId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = now
        };
        ApplyEmployment(entity, dto);
        return entity;
    }

    private static void ApplyEmployment(PayEmployment entity, PayrollEmploymentSaveDto dto)
    {
        entity.DepartmentId = dto.DepartmentId;
        entity.PositionId = dto.PositionId;
        entity.EmploymentType = dto.EmploymentType.Trim().ToUpperInvariant();
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.MonthlySalary = dto.MonthlySalary;
        entity.EmploymentRate = dto.EmploymentRate;
        entity.WeeklyHours = dto.WeeklyHours;
        entity.CurrencyId = dto.CurrencyId;
        entity.ExpenseAccountId = dto.ExpenseAccountId;
    }

    private static void ApplyEmployee(PayEmployee entity, PayrollEmployeeBaseDto dto)
    {
        entity.EmployeeNumber = dto.EmployeeNumber.Trim().ToUpperInvariant();
        entity.Pinfl = string.IsNullOrWhiteSpace(dto.Pinfl) ? null : dto.Pinfl.Trim();
        entity.Tin = string.IsNullOrWhiteSpace(dto.Tin) ? null : dto.Tin.Trim();
        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        entity.BirthDate = dto.BirthDate;
        entity.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        entity.BankAccountNumber = string.IsNullOrWhiteSpace(dto.BankAccountNumber) ? null : dto.BankAccountNumber.Trim();
    }
}
