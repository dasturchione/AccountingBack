using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace Application.Features.Pay.Components;

public sealed class PayrollComponentService : BaseService, IPayrollComponentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PayComponent> _query;
    private readonly ICommandRepository<PayComponent> _command;
    private readonly IQueryRepository<PayPayrollCalcLine> _calcLineQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;

    public PayrollComponentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<PayComponent> query,
        ICommandRepository<PayComponent> command,
        IQueryRepository<PayPayrollCalcLine> calcLineQuery,
        IQueryRepository<ChartAccount> accountQuery,
        ILogger<PayrollComponentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _calcLineQuery = calcLineQuery;
        _accountQuery = accountQuery;
    }

    public Task<Result<PagedResponse<PayrollComponentListDto>>> GetAllAsync(
        PayrollComponentListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();

            var specification = new PagedQuerySpecification<PayComponent, PayrollComponentListDto>
            {
                Criteria = x =>
                    (!filter.StateId.HasValue || x.StateId == filter.StateId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.ComponentType) || x.ComponentType == filter.ComponentType) &&
                    (!filter.EffectiveOn.HasValue ||
                     (x.EffectiveFrom <= filter.EffectiveOn.Value &&
                      (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= filter.EffectiveOn.Value))) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.Code.ToLower().Contains(search) ||
                     x.Name.ToLower().Contains(search)),
                Selector = ListDtoSelector,
                OrderBy = x => x.OrderBy(y => y.SortOrder).ThenBy(y => y.Code),
                Skip = (page - 1) * take,
                Take = take
            };

            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollComponentDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PayComponent>()
                .Where(x => x.Id == id)
                .As(DtoSelector)
                .Build();
            var dto = await _query.GetAsync(query, ct);
            return dto is null
                ? Result.Failure<PayrollComponentDto>(PayrollErrors.NotFound("Component", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<int>> CreateAsync(PayrollComponentCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var validation = await ValidateAsync(dto, organizationId, null, ct);
            if (!validation.IsSuccess)
                return Result.Failure<int>(validation.Error);

            var entity = new PayComponent
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };
            Apply(entity, dto);
            await _command.CreateAsync(entity, ct);

            _auditLogService.SetNewValues(await GetDtoInternalAsync(entity.Id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayComponent, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(int id, PayrollComponentUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entityQuery = _queryBuilder.For<PayComponent>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(entityQuery, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Component", id, _userContext.LanguageId));

            if (await IsUsedByPostedPayrollAsync(id, ct))
                return Result.Failure(PayrollErrors.ComponentAlreadyUsed(id));

            var validation = await ValidateAsync(dto, entity.OrganizationId, id, ct);
            if (!validation.IsSuccess)
                return validation;

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            Apply(entity, dto);
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayComponent, id.ToString(), AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entityQuery = _queryBuilder.For<PayComponent>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(entityQuery, ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("Component", id, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetDtoInternalAsync(id, ct));
            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayComponent, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateAsync(PayrollComponentBaseDto dto, int organizationId, int? currentId, CancellationToken ct)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _query.AnyAsync(x =>
                x.OrganizationId == organizationId &&
                x.Id != currentId &&
                x.Code == code &&
                x.EffectiveFrom == dto.EffectiveFrom, ct))
            return Result.Failure(PayrollErrors.Conflict("ComponentConflict", $"'{code}' hisoblash komponenti {dto.EffectiveFrom} sanasi uchun allaqachon mavjud."));

        var accountIds = new[] { dto.ExpenseAccountId, dto.LiabilityAccountId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
        if (accountIds.Count > 0)
        {
            var accountQuery = _queryBuilder.For<ChartAccount>()
                .Where(x => x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE &&
                            accountIds.Contains(x.Id))
                .As(x => x.Id)
                .Build();
            var existing = await _accountQuery.GetAllAsync(accountQuery, ct);
            var missing = accountIds.Except(existing).FirstOrDefault();
            if (missing > 0)
                return Result.Failure(PayrollErrors.ReferencedRecordNotFound("ChartAccount", missing));
        }

        return Result.Success();
    }

    private async Task<bool> IsUsedByPostedPayrollAsync(int id, CancellationToken ct) =>
        await _calcLineQuery.AnyAsync(x =>
            x.ComponentId == id &&
            x.PayrollLine.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED, ct);

    private async Task<PayrollComponentDto?> GetDtoInternalAsync(int id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayComponent>().Where(x => x.Id == id).As(DtoSelector).Build();
        return await _query.GetAsync(query, ct);
    }

    private static void Apply(PayComponent entity, PayrollComponentBaseDto dto)
    {
        entity.Code = dto.Code.Trim().ToUpperInvariant();
        entity.Name = dto.Name.Trim();
        entity.ComponentType = dto.ComponentType;
        entity.CalculationMethod = dto.CalculationMethod;
        entity.DefaultAmount = dto.DefaultAmount;
        entity.DefaultRate = dto.DefaultRate;
        entity.IsMandatory = dto.IsMandatory;
        entity.ExpenseAccountId = dto.ExpenseAccountId;
        entity.LiabilityAccountId = dto.LiabilityAccountId;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;
        entity.SortOrder = dto.SortOrder;
    }

    private static PayrollComponentDto MapDto(PayComponent x) =>
        new()
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Code = x.Code,
            Name = x.Name,
            ComponentType = x.ComponentType,
            CalculationMethod = x.CalculationMethod,
            DefaultAmount = x.DefaultAmount,
            DefaultRate = x.DefaultRate,
            IsMandatory = x.IsMandatory,
            ExpenseAccountId = x.ExpenseAccountId,
            ExpenseAccountNumber = x.ExpenseAccount != null ? x.ExpenseAccount.Number : null,
            LiabilityAccountId = x.LiabilityAccountId,
            LiabilityAccountNumber = x.LiabilityAccount != null ? x.LiabilityAccount.Number : null,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            SortOrder = x.SortOrder,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };

    private static readonly Expression<Func<PayComponent, PayrollComponentDto>> DtoSelector = x =>
        new PayrollComponentDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Code = x.Code,
            Name = x.Name,
            ComponentType = x.ComponentType,
            CalculationMethod = x.CalculationMethod,
            DefaultAmount = x.DefaultAmount,
            DefaultRate = x.DefaultRate,
            IsMandatory = x.IsMandatory,
            ExpenseAccountId = x.ExpenseAccountId,
            ExpenseAccountNumber = x.ExpenseAccount != null ? x.ExpenseAccount.Number : null,
            LiabilityAccountId = x.LiabilityAccountId,
            LiabilityAccountNumber = x.LiabilityAccount != null ? x.LiabilityAccount.Number : null,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            SortOrder = x.SortOrder,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };

    private static readonly Expression<Func<PayComponent, PayrollComponentListDto>> ListDtoSelector = x =>
        new PayrollComponentListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            Code = x.Code,
            Name = x.Name,
            ComponentType = x.ComponentType,
            CalculationMethod = x.CalculationMethod,
            DefaultAmount = x.DefaultAmount,
            DefaultRate = x.DefaultRate,
            IsMandatory = x.IsMandatory,
            ExpenseAccountId = x.ExpenseAccountId,
            ExpenseAccountNumber = x.ExpenseAccount != null ? x.ExpenseAccount.Number : null,
            LiabilityAccountId = x.LiabilityAccountId,
            LiabilityAccountNumber = x.LiabilityAccount != null ? x.LiabilityAccount.Number : null,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            SortOrder = x.SortOrder,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };
}
