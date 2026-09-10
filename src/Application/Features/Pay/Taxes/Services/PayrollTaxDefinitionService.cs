using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Pay.Components;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.Taxes;

public sealed class PayrollTaxDefinitionService : BaseService, IPayrollTaxDefinitionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PayTaxDefinition> _query;
    private readonly ICommandRepository<PayTaxDefinition> _command;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly IQueryRepository<PayPayrollTaxLine> _taxLineQuery;

    public PayrollTaxDefinitionService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<PayTaxDefinition> query,
        ICommandRepository<PayTaxDefinition> command,
        IQueryRepository<ChartAccount> accountQuery,
        IQueryRepository<PayPayrollTaxLine> taxLineQuery,
        ILogger<PayrollTaxDefinitionService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
        _accountQuery = accountQuery;
        _taxLineQuery = taxLineQuery;
    }

    public Task<Result<PagedResponse<PayrollTaxDefinitionListDto>>> GetAllAsync(
        PayrollTaxDefinitionListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<PagedResponse<PayrollTaxDefinitionListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var specification = _queryBuilder.For<PayTaxDefinition>()
                .Where(x => x.OrganizationId == organizationId &&
                    (!filter.StateId.HasValue || x.StateId == filter.StateId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.TaxType) || x.TaxType == filter.TaxType) &&
                    (!filter.EffectiveOn.HasValue ||
                     (x.EffectiveFrom <= filter.EffectiveOn.Value &&
                      (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= filter.EffectiveOn.Value))) &&
                    (string.IsNullOrWhiteSpace(search) || x.Code.ToLower().Contains(search) || x.Name.ToLower().Contains(search)))
                .As(x => new PayrollTaxDefinitionListDto
                {
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    Code = x.Code,
                    Name = x.Name,
                    TaxType = x.TaxType,
                    BaseType = x.BaseType,
                    Rate = x.Rate,
                    ExemptionAmount = x.ExemptionAmount,
                    LimitAmount = x.LimitAmount,
                    LiabilityAccountId = x.LiabilityAccountId,
                    LiabilityAccountNumber = x.LiabilityAccount.Number,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    StateId = x.StateId,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .OrderBy(x => x.OrderBy(y => y.Code).ThenByDescending(y => y.EffectiveFrom))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollTaxDefinitionDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<PayrollTaxDefinitionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var dto = await _query.GetAsync(_queryBuilder.For<PayTaxDefinition>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .As(x => new PayrollTaxDefinitionDto
                {
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    Code = x.Code,
                    Name = x.Name,
                    TaxType = x.TaxType,
                    BaseType = x.BaseType,
                    Rate = x.Rate,
                    ExemptionAmount = x.ExemptionAmount,
                    LimitAmount = x.LimitAmount,
                    LiabilityAccountId = x.LiabilityAccountId,
                    LiabilityAccountNumber = x.LiabilityAccount.Number,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    StateId = x.StateId,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                }).Build(), ct);
            return dto is null
                ? Result.Failure<PayrollTaxDefinitionDto>(PayrollErrors.NotFound("TaxDefinition", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<int>> CreateAsync(PayrollTaxDefinitionCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var validation = await ValidateAsync(dto, organizationId, null, ct);
            if (!validation.IsSuccess)
                return Result.Failure<int>(validation.Error);
            var entity = new PayTaxDefinition
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };
            Apply(entity, dto);
            await _command.CreateAsync(entity, ct);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(int id, PayrollTaxDefinitionUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var entity = await _query.GetAsync(_queryBuilder.For<PayTaxDefinition>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId).Build(), ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("TaxDefinition", id, _userContext.LanguageId));
            if (await _taxLineQuery.AnyAsync(x => x.TaxDefinitionId == id && x.PayrollLine.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED, ct))
                return Result.Failure(PayrollErrors.Business("TaxDefinitionAlreadyUsed", "Tasdiqlangan oylik hujjatida ishlatilgan soliq qoidasini o‘zgartirib bo‘lmaydi.", _userContext.LanguageId));
            var validation = await ValidateAsync(dto, organizationId, id, ct);
            if (!validation.IsSuccess)
                return validation;
            Apply(entity, dto);
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var entity = await _query.GetAsync(_queryBuilder.For<PayTaxDefinition>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId).Build(), ct);
            if (entity is null)
                return Result.Failure(PayrollErrors.NotFound("TaxDefinition", id, _userContext.LanguageId));
            if (await _taxLineQuery.AnyAsync(x => x.TaxDefinitionId == id, ct))
                return Result.Failure(PayrollErrors.Business("TaxDefinitionAlreadyUsed", "Ishlatilgan soliq qoidasini o‘chirish o‘rniga nofaol holatga o‘tkazing.", _userContext.LanguageId));
            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateAsync(PayrollTaxDefinitionBaseDto dto, int organizationId, int? currentId, CancellationToken ct)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(dto.Name))
            return Result.Failure(PayrollErrors.Business("TaxDefinitionRequired", "Soliq kodi va nomi majburiy.", _userContext.LanguageId));
        var taxType = dto.TaxType.Trim().ToUpperInvariant();
        var baseType = dto.BaseType.Trim().ToUpperInvariant();
        if (!PayrollTaxTypeConst.All.Contains(taxType) || !PayrollTaxBaseTypeConst.All.Contains(baseType))
            return Result.Failure(PayrollErrors.Business("TaxDefinitionTypeInvalid", "Soliq turi yoki soliq bazasi noto‘g‘ri.", _userContext.LanguageId));
        if (dto.Rate is < 0m or > 100m || dto.ExemptionAmount is < 0m || dto.LimitAmount is < 0m)
            return Result.Failure(PayrollErrors.Business("TaxDefinitionValueInvalid", "Soliq stavkasi 0–100 oralig‘ida, chegirma va limit esa manfiy bo‘lmasligi kerak.", _userContext.LanguageId));
        if (!PayrollComponentEffectiveDatePolicy.IsValidRange(dto.EffectiveFrom, dto.EffectiveTo))
            return Result.Failure(PayrollErrors.Business("TaxDefinitionDateRange", "Soliq qoidasining tugash sanasi boshlanish sanasidan oldin bo‘lishi mumkin emas.", _userContext.LanguageId));
        if (!await _accountQuery.AnyAsync(x => x.Id == dto.LiabilityAccountId && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("ChartAccount", dto.LiabilityAccountId, _userContext.LanguageId));
        if (await _query.AnyAsync(x => x.OrganizationId == organizationId && x.Id != currentId && x.Code == code &&
            x.EffectiveFrom <= (dto.EffectiveTo ?? DateOnly.MaxValue) &&
            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= dto.EffectiveFrom), ct))
            return Result.Failure(PayrollErrors.Conflict("TaxDefinitionEffectiveDateOverlap", $"'{code}' soliq qoidasining amal qilish sanalari kesishadi.", _userContext.LanguageId));
        return Result.Success();
    }

    private static void Apply(PayTaxDefinition entity, PayrollTaxDefinitionBaseDto dto)
    {
        entity.Code = dto.Code.Trim().ToUpperInvariant();
        entity.Name = dto.Name.Trim();
        entity.TaxType = dto.TaxType.Trim().ToUpperInvariant();
        entity.BaseType = dto.BaseType.Trim().ToUpperInvariant();
        entity.Rate = dto.Rate;
        entity.ExemptionAmount = dto.ExemptionAmount;
        entity.LimitAmount = dto.LimitAmount;
        entity.LiabilityAccountId = dto.LiabilityAccountId;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;
    }
}
