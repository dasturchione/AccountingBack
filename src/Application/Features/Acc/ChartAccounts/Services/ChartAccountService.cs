using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.ChartAccounts;
using Domain.Entities;
using LinqKit;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public class ChartAccountService : IChartAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ChartAccount> _query;
    private readonly ICommandRepository<ChartAccount> _command;
    private readonly IQueryRepository<ChartAccountPresetAccount> _presetAccountQuery;
    private readonly IQueryRepository<ChartAccountPresetAccountSubkonto> _presetAccountSubkontoQuery;
    private readonly IQueryRepository<ChartAccountSubkonto> _chartAccountSubkontoQuery;
    private readonly ICommandRepository<ChartAccountSubkonto> _chartAccountSubkontoCommand;

    public ChartAccountService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ChartAccount> query, 
                               ICommandRepository<ChartAccount> command,
                               IQueryRepository<ChartAccountPresetAccount> presetAccountQuery,
                               IQueryRepository<ChartAccountPresetAccountSubkonto> presetAccountSubkontoQuery,
                               IQueryRepository<ChartAccountSubkonto> chartAccountSubkontoQuery,
                               ICommandRepository<ChartAccountSubkonto> chartAccountSubkontoCommand)
    {
        _query = query; 
        _command = command; 
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
        _presetAccountQuery = presetAccountQuery;
        _presetAccountSubkontoQuery = presetAccountSubkontoQuery;
        _chartAccountSubkontoQuery = chartAccountSubkontoQuery;
        _chartAccountSubkontoCommand = chartAccountSubkontoCommand;
    }

    public async Task<Result<int>> CreateAsync(ChartAccountCreateDto dto, CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!string.IsNullOrEmpty(dto.Code) && await _query.AnyAsync(x => x.Code == dto.Code && x.OrganizationId == _userContext.OrganizationId.Value, ct))
            return Result.Failure<int>(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        if (await _query.AnyAsync(x => x.Number == dto.Number && x.OrganizationId == _userContext.OrganizationId.Value, ct))
            return Result.Failure<int>(ChartAccountErrors.NumberConflict(dto.Number, _userContext.LanguageId));

        var entity = new ChartAccount
        {
            ParentId = dto.ParentId,
            Code = dto.Code,
            Name = dto.Name,
            Number = dto.Number,
            IsGroup = dto.IsGroup,
            IsTaxAccounting = dto.IsTaxAccounting,
            IsQuantity = dto.IsQuantity,
            IsCurrency = dto.IsCurrency,
            IsDepartment = dto.IsDepartment,
            IsOffBalance = dto.IsOffBalance,
            AccountTypeId = dto.AccountTypeId,
            OrganizationId = _userContext.OrganizationId.Value,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result<List<ChartAccountImportFromPresetResultDto>>> ImportFromPresetAsync(
        List<ChartAccountImportFromPresetRequestDto> dto,
        CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure<List<ChartAccountImportFromPresetResultDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var requestedIds = dto
            .Select(x => x.PresetAccountId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        if (requestedIds.Count == 0)
            return Result.Failure<List<ChartAccountImportFromPresetResultDto>>(ChartAccountErrors.EmptyPresetAccounts(_userContext.LanguageId));

        var presetAccounts = await GetPresetAccountClosureAsync(requestedIds, ct);
        foreach (var requestedId in requestedIds)
        {
            if (!presetAccounts.ContainsKey(requestedId))
                return Result.Failure<List<ChartAccountImportFromPresetResultDto>>(ChartAccountErrors.PresetAccountNotFound(requestedId, _userContext.LanguageId));
        }

        var organizationId = _userContext.OrganizationId.Value;
        var presetAccountIds = presetAccounts.Keys.ToList();
        var childParentIds = await GetPresetAccountIdsWithChildrenAsync(presetAccountIds, ct);
        var chartAccountsByNumber = await GetChartAccountsByNumberAsync(presetAccounts.Values.Select(x => x.Number).Distinct().ToList(), organizationId, ct);
        var subkontosByPresetAccountId = await GetSubkontosByPresetAccountIdAsync(presetAccountIds, ct);
        var resultsByPresetAccountId = new Dictionary<int, ChartAccountImportFromPresetResultDto>();

        foreach (var presetAccount in SortByHierarchy(presetAccounts.Values.ToList(), presetAccounts))
        {
            if (!chartAccountsByNumber.TryGetValue(presetAccount.Number, out var chartAccount))
            {
                int? parentId = null;
                if (presetAccount.ParentPresetAccountId.HasValue &&
                    presetAccounts.TryGetValue(presetAccount.ParentPresetAccountId.Value, out var parentPresetAccount))
                {
                    if (!chartAccountsByNumber.TryGetValue(parentPresetAccount.Number, out var parentChartAccount))
                        return Result.Failure<List<ChartAccountImportFromPresetResultDto>>(ChartAccountErrors.PresetAccountNotFound(parentPresetAccount.Id, _userContext.LanguageId));

                    parentId = parentChartAccount.Id;
                }

                chartAccount = new ChartAccount
                {
                    ParentId = parentId,
                    Code = presetAccount.Code,
                    Name = presetAccount.Name,
                    Number = presetAccount.Number,
                    IsGroup = presetAccount.IsGroup,
                    IsTaxAccounting = presetAccount.IsTaxAccounting,
                    IsQuantity = presetAccount.IsQuantity,
                    IsCurrency = presetAccount.IsCurrency,
                    IsDepartment = presetAccount.IsDepartment,
                    IsOffBalance = presetAccount.IsOffBalance,
                    AccountTypeId = presetAccount.AccountTypeId,
                    OrganizationId = organizationId,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                };

                await _command.CreateAsync(chartAccount, ct);
                chartAccountsByNumber[chartAccount.Number] = chartAccount;

                resultsByPresetAccountId[presetAccount.Id] = new ChartAccountImportFromPresetResultDto
                {
                    PresetAccountId = presetAccount.Id,
                    ChartAccountId = chartAccount.Id,
                    Number = chartAccount.Number,
                    WasCreated = true
                };
            }
            else
            {
                resultsByPresetAccountId[presetAccount.Id] = new ChartAccountImportFromPresetResultDto
                {
                    PresetAccountId = presetAccount.Id,
                    ChartAccountId = chartAccount.Id,
                    Number = chartAccount.Number,
                    WasCreated = false
                };
            }

            await EnsureChartAccountSubkontosAsync(
                chartAccount,
                subkontosByPresetAccountId.GetValueOrDefault(presetAccount.Id) ?? new List<PresetAccountSubkontoSnapshot>(),
                organizationId,
                ct);
        }

        return requestedIds
            .Select(id => resultsByPresetAccountId[id])
            .ToList();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ChartAccountListDto>>> GetAllAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ChartAccount, ChartAccountListDto, ChartAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PagedResponse<ChartAccountGroupedListDto>>> GetGroupedListAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ChartAccount, ChartAccountGroupedListDto, ChartAccountListFilter>(filter);
        query.Criteria.And(x => x.ParentId == null);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ChartAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).As<ChartAccountDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure<ChartAccountDto>(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ChartAccountUpdateDto dto, CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        if(entity.Code != dto.Code && !string.IsNullOrEmpty(dto.Code))
        {
            var codeConflict = await _query.AnyAsync(x => x.Code == dto.Code && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (codeConflict)
                return Result.Failure(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        if (entity.Number != dto.Number)
        {
            var numberConflict = await _query.AnyAsync(x => x.Number == dto.Number && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (numberConflict)
                return Result.Failure(ChartAccountErrors.NumberConflict(dto.Number, _userContext.LanguageId));
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Number = dto.Number;
        entity.ParentId = dto.ParentId;
        entity.IsGroup = dto.IsGroup;
        entity.IsCurrency = dto.IsCurrency;
        entity.IsQuantity = dto.IsQuantity;
        entity.IsDepartment = dto.IsDepartment;
        entity.IsOffBalance = dto.IsOffBalance;
        entity.AccountTypeId = dto.AccountTypeId;
        entity.IsTaxAccounting = dto.IsTaxAccounting;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<Dictionary<int, PresetAccountImportSnapshot>> GetPresetAccountClosureAsync(
        List<int> presetAccountIds,
        CancellationToken ct)
    {
        var languageId = _userContext.LanguageId;
        var result = new Dictionary<int, PresetAccountImportSnapshot>();
        var pendingIds = presetAccountIds.Distinct().ToList();

        while (pendingIds.Count > 0)
        {
            var currentIds = pendingIds
                .Where(id => !result.ContainsKey(id))
                .Distinct()
                .ToList();

            if (currentIds.Count == 0)
                break;

            var query = new QuerySpecification<ChartAccountPresetAccount, PresetAccountImportSnapshot>
            {
                Criteria = x => currentIds.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE,
                Selector = x => new PresetAccountImportSnapshot
                {
                    Id = x.Id,
                    PresetId = x.PresetId,
                    Code = x.Code,
                    Number = x.Number,
                    Name = x.ChartAccountPresetAccountTranslations
                        .Where(t => t.LanguageId == languageId)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? x.Number,
                    ParentPresetAccountId = x.ParentPresetAccountId,
                    AccountTypeId = x.AccountTypeId,
                    IsQuantity = x.IsQuantity,
                    IsCurrency = x.IsCurrency,
                    IsGroup = x.IsGroup,
                    IsDepartment = x.IsDepartment,
                    IsTaxAccounting = x.IsTaxAccounting,
                    IsOffBalance = x.IsOffBalance,
                    DisplayOrder = x.DisplayOrder
                }
            };

            var items = await _presetAccountQuery.GetAllAsync(query, ct);
            foreach (var item in items)
                result[item.Id] = item;

            pendingIds = items
                .Where(x => x.ParentPresetAccountId.HasValue && !result.ContainsKey(x.ParentPresetAccountId.Value))
                .Select(x => x.ParentPresetAccountId!.Value)
                .Distinct()
                .ToList();
        }

        return result;
    }

    private async Task<HashSet<int>> GetPresetAccountIdsWithChildrenAsync(List<int> presetAccountIds, CancellationToken ct)
    {
        if (presetAccountIds.Count == 0)
            return new HashSet<int>();

        var query = new QuerySpecification<ChartAccountPresetAccount, int>
        {
            Criteria = x => x.ParentPresetAccountId.HasValue && presetAccountIds.Contains(x.ParentPresetAccountId.Value),
            Selector = x => x.ParentPresetAccountId!.Value
        };

        var ids = await _presetAccountQuery.GetAllAsync(query, ct);
        return ids.ToHashSet();
    }

    private async Task<Dictionary<string, ChartAccount>> GetChartAccountsByNumberAsync(
        List<string> numbers,
        int organizationId,
        CancellationToken ct)
    {
        if (numbers.Count == 0)
            return new Dictionary<string, ChartAccount>(StringComparer.OrdinalIgnoreCase);

        var query = new QuerySpecification<ChartAccount>
        {
            Criteria = x => x.OrganizationId == organizationId && numbers.Contains(x.Number)
        };

        var accounts = await _query.GetAllAsync(query, ct);
        return accounts
            .GroupBy(x => x.Number, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<int, List<PresetAccountSubkontoSnapshot>>> GetSubkontosByPresetAccountIdAsync(
        List<int> presetAccountIds,
        CancellationToken ct)
    {
        if (presetAccountIds.Count == 0)
            return new Dictionary<int, List<PresetAccountSubkontoSnapshot>>();

        var query = new QuerySpecification<ChartAccountPresetAccountSubkonto, PresetAccountSubkontoSnapshot>
        {
            Criteria = x => presetAccountIds.Contains(x.PresetAccountId),
            Selector = x => new PresetAccountSubkontoSnapshot
            {
                PresetAccountId = x.PresetAccountId,
                SubkontoTypeId = x.SubkontoTypeId,
                SortOrder = x.SortOrder
            },
            OrderBy = q => q.OrderBy(x => x.SortOrder)
        };

        var items = await _presetAccountSubkontoQuery.GetAllAsync(query, ct);
        return items
            .GroupBy(x => x.PresetAccountId)
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    private async Task EnsureChartAccountSubkontosAsync(
        ChartAccount chartAccount,
        List<PresetAccountSubkontoSnapshot> presetSubkontos,
        int organizationId,
        CancellationToken ct)
    {
        if (presetSubkontos.Count == 0)
            return;

        var subkontoTypeIds = presetSubkontos.Select(x => x.SubkontoTypeId).Distinct().ToList();
        var existingQuery = new QuerySpecification<ChartAccountSubkonto, short>
        {
            Criteria = x => x.OrganizationId == organizationId &&
                            x.AccountId == chartAccount.Id &&
                            subkontoTypeIds.Contains(x.SubkontoTypeId),
            Selector = x => x.SubkontoTypeId
        };

        var existingTypeIds = (await _chartAccountSubkontoQuery.GetAllAsync(existingQuery, ct)).ToHashSet();
        var missing = presetSubkontos
            .Where(x => !existingTypeIds.Contains(x.SubkontoTypeId))
            .Select(x => new ChartAccountSubkonto
            {
                OrganizationId = organizationId,
                AccountId = chartAccount.Id,
                SubkontoTypeId = x.SubkontoTypeId,
                SortOrder = x.SortOrder,
                IsRequired = true,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            })
            .ToList();

        if (missing.Count > 0)
            await _chartAccountSubkontoCommand.CreateAsync(missing, ct);
    }

    private static List<PresetAccountImportSnapshot> SortByHierarchy(
        List<PresetAccountImportSnapshot> items,
        Dictionary<int, PresetAccountImportSnapshot> byId)
    {
        return items
            .OrderBy(x => GetDepth(x, byId))
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Number)
            .ToList();
    }

    private static int GetDepth(PresetAccountImportSnapshot item, Dictionary<int, PresetAccountImportSnapshot> byId)
    {
        var depth = 0;
        var current = item;

        while (current.ParentPresetAccountId.HasValue &&
               byId.TryGetValue(current.ParentPresetAccountId.Value, out var parent))
        {
            depth++;
            current = parent;
        }

        return depth;
    }

    private sealed class PresetAccountImportSnapshot
    {
        public int Id { get; set; }
        public short PresetId { get; set; }
        public string? Code { get; set; }
        public string Number { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int? ParentPresetAccountId { get; set; }
        public short AccountTypeId { get; set; }
        public bool IsGroup { get; set; }
        public bool IsQuantity { get; set; }
        public bool IsCurrency { get; set; }
        public bool IsDepartment { get; set; }
        public bool IsTaxAccounting { get; set; }
        public bool IsOffBalance { get; set; }
        public int DisplayOrder { get; set; }
    }

    private sealed class PresetAccountSubkontoSnapshot
    {
        public int PresetAccountId { get; set; }
        public short SubkontoTypeId { get; set; }
        public int SortOrder { get; set; }
    }
}
