using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Acc.OpeningBalances;

public partial class OpeningBalanceService
{
    private async Task PopulateSubkontoNamesAsync(
        OpeningBalanceDetailDto account,
        int organizationId,
        CancellationToken ct)
    {
        var subkontoGroups = account.Details
            .SelectMany(x => x.Subkontos)
            .GroupBy(x => x.SubkontoTypeId)
            .ToList();

        foreach (var group in subkontoGroups)
        {
            var ids = group.Select(x => x.SubkontoId).Distinct().ToList();
            var names = await GetSubkontoNamesAsync(group.Key, ids, organizationId, ct);

            foreach (var subkonto in group)
                subkonto.SubkontoName = names.GetValueOrDefault(subkonto.SubkontoId, string.Empty);
        }
    }

    private Task<Dictionary<long, string>> GetSubkontoNamesAsync(
        short subkontoTypeId,
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct) =>
        subkontoTypeId switch
        {
            SubkontoTypeIdConst.FixedAssets or SubkontoTypeIdConst.FixedAssetsTurnover =>
                GetFaAssetNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.Counterparties or SubkontoTypeIdConst.CounterpartiesTurnover =>
                GetCounterpartyNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.Contracts => GetContractNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.InventoryItems or SubkontoTypeIdConst.InventoryItemsTurnover or SubkontoTypeIdConst.ProductsTurnover =>
                GetProductNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.Batches or SubkontoTypeIdConst.BatchesTurnover =>
                GetPostingBatchNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.Warehouses => GetWarehouseNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.OrganizationEmployees or SubkontoTypeIdConst.OrganizationEmployeesTurnover =>
                GetUserNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.ProductGroups or SubkontoTypeIdConst.ProductGroupsTurnover =>
                GetProductGroupNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.VatRates or SubkontoTypeIdConst.VatRatesTurnover =>
                GetVatRateNamesAsync(ids, ct),
            SubkontoTypeIdConst.TaxTypes => GetTaxTypeNamesAsync(ids, ct),
            SubkontoTypeIdConst.SalesDocumentsTurnover => GetSaleDocumentNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.CounterpartySettlementDocuments =>
                GetSettlementDocumentNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.OrganizationCashDesks => GetCashBoxNamesAsync(ids, organizationId, ct),
            SubkontoTypeIdConst.BankAccounts => GetBankAccountNamesAsync(ids, organizationId, ct),
            _ => Task.FromResult(new Dictionary<long, string>())
        };

    private async Task<Dictionary<long, string>> GetFaAssetNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaAsset>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _faAssetQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetCounterpartyNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, Name = x.FullName ?? x.ShortName })
            .Build();
        var items = await _counterpartyCardQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetContractNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<Contract>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.ContractNumber })
            .Build();
        var items = await _contractQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => x.Id, x => x.ContractNumber);
    }

    private async Task<Dictionary<long, string>> GetProductNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<Product>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _productQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetPostingBatchNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Comment })
            .Build();
        var items = await _postingBatchQuery.GetAllAsync(query, ct);
        return items.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace(x.Comment) ? $"Posting batch #{x.Id}" : x.Comment);
    }

    private async Task<Dictionary<long, string>> GetWarehouseNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<Warehouse>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _warehouseQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetUserNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<User>()
            .Where(x => entityIds.Contains(x.Id) &&
                        x.UserOrganizations.Any(membership =>
                            membership.OrganizationId == organizationId &&
                            membership.StateId == StateIdConst.ACTIVE))
            .As(x => new { x.Id, x.FirstName, x.LastName, x.UserName })
            .Build();
        var items = await _userQuery.GetAllAsync(query, ct);
        return items.ToDictionary(
            x => (long)x.Id,
            x => string.IsNullOrWhiteSpace($"{x.FirstName} {x.LastName}") ? x.UserName : $"{x.FirstName} {x.LastName}".Trim());
    }

    private async Task<Dictionary<long, string>> GetProductGroupNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<ProductGroup>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _productGroupQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetVatRateNamesAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        var entityIds = ToShortIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<VatRate>()
            .Where(x => entityIds.Contains(x.Id))
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _vatRateQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetTaxTypeNamesAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        var entityIds = ToShortIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<TaxType>()
            .Where(x => entityIds.Contains(x.Id))
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _taxTypeQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetSaleDocumentNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<SaleDoc>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.DocNumber })
            .Build();
        var items = await _saleDocQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => x.Id, x => x.DocNumber);
    }

    private async Task<Dictionary<long, string>> GetSettlementDocumentNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var purchaseQuery = _queryBuilder.For<PurchaseDoc>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.DocNumber })
            .Build();
        var saleQuery = _queryBuilder.For<SaleDoc>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.DocNumber })
            .Build();
        var bankQuery = _queryBuilder.For<BankOperation>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.DocNumber })
            .Build();
        var cashQuery = _queryBuilder.For<CashOperation>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.DocNumber })
            .Build();

        var namesById = new Dictionary<long, List<string>>();
        AddDocumentNames(namesById, "Purchase", (await _purchaseDocQuery.GetAllAsync(purchaseQuery, ct)).Select(x => (x.Id, x.DocNumber)));
        AddDocumentNames(namesById, "Sale", (await _saleDocQuery.GetAllAsync(saleQuery, ct)).Select(x => (x.Id, x.DocNumber)));
        AddDocumentNames(namesById, "Bank", (await _bankOperationQuery.GetAllAsync(bankQuery, ct)).Select(x => (x.Id, x.DocNumber)));
        AddDocumentNames(namesById, "Cash", (await _cashOperationQuery.GetAllAsync(cashQuery, ct)).Select(x => (x.Id, x.DocNumber)));

        return namesById.ToDictionary(x => x.Key, x => string.Join(" / ", x.Value.Distinct()));
    }

    private async Task<Dictionary<long, string>> GetCashBoxNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<CashBox>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name })
            .Build();
        var items = await _cashBoxQuery.GetAllAsync(query, ct);
        return items.ToDictionary(x => (long)x.Id, x => x.Name);
    }

    private async Task<Dictionary<long, string>> GetBankAccountNamesAsync(
        IReadOnlyCollection<long> ids,
        int organizationId,
        CancellationToken ct)
    {
        var entityIds = ToIntIds(ids);
        if (entityIds.Count == 0)
            return [];

        var query = _queryBuilder.For<BankAccount>()
            .Where(x => entityIds.Contains(x.Id) && x.OrganizationId == organizationId)
            .As(x => new { x.Id, x.Name, x.AccountNumber })
            .Build();
        var items = await _bankAccountQuery.GetAllAsync(query, ct);
        return items.ToDictionary(
            x => (long)x.Id,
            x => string.IsNullOrWhiteSpace(x.Name) ? x.AccountNumber : x.Name);
    }

    private static void AddDocumentNames(
        Dictionary<long, List<string>> namesById,
        string prefix,
        IEnumerable<(long Id, string DocNumber)> documents)
    {
        foreach (var document in documents)
        {
            if (!namesById.TryGetValue(document.Id, out var names))
            {
                names = [];
                namesById[document.Id] = names;
            }

            names.Add($"{prefix}: {document.DocNumber}");
        }
    }

    private static List<int> ToIntIds(IReadOnlyCollection<long> ids) =>
        ids.Where(x => x is >= int.MinValue and <= int.MaxValue).Select(x => (int)x).ToList();

    private static List<short> ToShortIds(IReadOnlyCollection<long> ids) =>
        ids.Where(x => x is >= short.MinValue and <= short.MaxValue).Select(x => (short)x).ToList();
}
