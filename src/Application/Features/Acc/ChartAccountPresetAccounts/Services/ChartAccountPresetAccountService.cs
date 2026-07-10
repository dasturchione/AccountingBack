using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.ChartAccountPresetAccounts;

public class ChartAccountPresetAccountService : IChartAccountPresetAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<ChartAccountPresetAccount> _presetAccountQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;

    public ChartAccountPresetAccountService(
        IUserContext userContext,
        IQueryRepository<ChartAccountPresetAccount> presetAccountQuery,
        IQueryRepository<ChartAccount> chartAccountQuery)
    {
        _userContext = userContext;
        _presetAccountQuery = presetAccountQuery;
        _chartAccountQuery = chartAccountQuery;
    }

    public async Task<Result<PagedResponse<ChartAccountPresetAccountListDto>>> GetGroupedListAsync(
        ChartAccountPresetAccountListFilter filter,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<PagedResponse<ChartAccountPresetAccountListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var languageId = _userContext.LanguageId;
        var presetQuery = new QuerySpecification<ChartAccountPresetAccount, PresetAccountSnapshot>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (!filter.PresetId.HasValue || x.PresetId == filter.PresetId.Value),
            Selector = x => new PresetAccountSnapshot
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
                ParentNumber = x.ChartAccountPresetAccountNavigation != null ? x.ChartAccountPresetAccountNavigation.Number : null,
                AccountTypeId = x.AccountTypeId,
                AccountTypeCode = x.AccountType.Code,
                AccountTypeName = x.AccountType.Name,
                IsQuantity = x.IsQuantity,
                IsCurrency = x.IsCurrency,
                IsDepartment = x.IsDepartment,
                IsTaxAccounting = x.IsTaxAccounting,
                IsOffBalance = x.IsOffBalance,
                DisplayOrder = x.DisplayOrder,
                StateId = x.StateId,
                StateName = x.State.FullName
            },
            OrderBy = q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Number)
        };

        var snapshots = await _presetAccountQuery.GetAllAsync(presetQuery, ct);
        var accountNumbers = snapshots.Select(x => x.Number).Distinct().ToList();
        var existingNumbers = await GetExistingChartAccountNumbersAsync(accountNumbers, ct);
        var childrenByParentId = snapshots
            .Where(x => x.ParentPresetAccountId.HasValue)
            .GroupBy(x => x.ParentPresetAccountId!.Value)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.DisplayOrder).ThenBy(y => y.Number).ToList());

        var rootParentId = filter.ParentPresetAccountId;
        var roots = snapshots
            .Where(x => x.ParentPresetAccountId == rootParentId)
            .Where(x => !filter.IsGroup.HasValue || childrenByParentId.ContainsKey(x.Id) == filter.IsGroup.Value)
            .Where(x => MatchesSearch(x, filter.Search))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Number)
            .ToList();

        var totalCount = roots.Count;
        var pageSize = filter.PageSize ?? 50;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var pagedRoots = roots.Skip(skip).Take(pageSize).ToList();

        var items = pagedRoots
            .Select(x => MapDto(x, childrenByParentId, existingNumbers))
            .ToList();

        return Result.Success(PagedResponseFactory.Create(
            new PagedList<ChartAccountPresetAccountListDto>(items, totalCount),
            filter.Page,
            filter.PageSize));
    }

    private async Task<HashSet<string>> GetExistingChartAccountNumbersAsync(List<string> accountNumbers, CancellationToken ct)
    {
        if (accountNumbers.Count == 0 || _userContext.OrganizationId is null)
            return new HashSet<string>();

        var organizationId = _userContext.OrganizationId.Value;
        var query = new QuerySpecification<ChartAccount, string>
        {
            Criteria = x => x.OrganizationId == organizationId && accountNumbers.Contains(x.Number),
            Selector = x => x.Number
        };

        var numbers = await _chartAccountQuery.GetAllAsync(query, ct);
        return numbers.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static ChartAccountPresetAccountListDto MapDto(
        PresetAccountSnapshot snapshot,
        Dictionary<int, List<PresetAccountSnapshot>> childrenByParentId,
        HashSet<string> existingNumbers)
    {
        childrenByParentId.TryGetValue(snapshot.Id, out var children);

        return new ChartAccountPresetAccountListDto
        {
            Id = snapshot.Id,
            PresetId = snapshot.PresetId,
            Code = snapshot.Code,
            Number = snapshot.Number,
            Name = snapshot.Name,
            ParentPresetAccountId = snapshot.ParentPresetAccountId,
            ParentNumber = snapshot.ParentNumber,
            AccountTypeId = snapshot.AccountTypeId,
            AccountTypeCode = snapshot.AccountTypeCode,
            AccountTypeName = snapshot.AccountTypeName,
            IsGroup = children is { Count: > 0 },
            IsQuantity = snapshot.IsQuantity,
            IsCurrency = snapshot.IsCurrency,
            IsDepartment = snapshot.IsDepartment,
            IsTaxAccounting = snapshot.IsTaxAccounting,
            IsOffBalance = snapshot.IsOffBalance,
            DisplayOrder = snapshot.DisplayOrder,
            StateId = snapshot.StateId,
            StateName = snapshot.StateName,
            HasChartAccount = existingNumbers.Contains(snapshot.Number),
            Lines = children is null
                ? new List<ChartAccountPresetAccountListDto>()
                : children.Select(child => MapDto(child, childrenByParentId, existingNumbers)).ToList()
        };
    }

    private static bool MatchesSearch(PresetAccountSnapshot snapshot, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        var value = search.Trim();
        return snapshot.Number.StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
               snapshot.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrWhiteSpace(snapshot.Code) &&
                snapshot.Code.StartsWith(value, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class PresetAccountSnapshot
    {
        public int Id { get; set; }
        public short PresetId { get; set; }
        public string? Code { get; set; }
        public string Number { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int? ParentPresetAccountId { get; set; }
        public string? ParentNumber { get; set; }
        public short AccountTypeId { get; set; }
        public string AccountTypeCode { get; set; } = null!;
        public string AccountTypeName { get; set; } = null!;
        public bool IsQuantity { get; set; }
        public bool IsCurrency { get; set; }
        public bool IsDepartment { get; set; }
        public bool IsTaxAccounting { get; set; }
        public bool IsOffBalance { get; set; }
        public int DisplayOrder { get; set; }
        public short StateId { get; set; }
        public string StateName { get; set; } = null!;
    }
}
