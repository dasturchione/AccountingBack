using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Features.Inv.OpeningInventories;

public partial class OpeningInventoryService
{
    private async Task<Result> ApplyEffectsAsync(OpeningInventory document, CancellationToken ct)
    {
        var inventoryResult = await _inventoryDispatcher.ProcessAsync(document, ct);
        if (!inventoryResult.IsSuccess)
            return Result.Failure(inventoryResult.Error);

        return await SyncOpeningBalanceAsync(document, ct);
    }

    private async Task<Result> RemoveEffectsAsync(OpeningInventory document, CancellationToken ct)
    {
        var reverseResult = await _inventoryDispatcher.ReverseAsync(document, ct);
        if (!reverseResult.IsSuccess)
            return reverseResult;

        await DeleteWarehouseEffectRowsAsync(document.Id, ct);
        await RemoveOpeningBalanceDetailsAsync(document.Id, ct);
        return Result.Success();
    }

    private async Task<Result> EnsureEffectsAreRemovableAsync(
        OpeningInventory document,
        CancellationToken ct)
    {
        var movements = await _movementQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductMovement>()
                .Where(x => x.OrganizationId == document.OrganizationId &&
                            x.DocumentTypeId == DocumentTypeIdConst.OPENINGINVENTORY &&
                            x.DocumentId == document.Id)
                .Build(),
            ct);
        var receiptMovementIds = movements
            .Where(x => x.MovementSign == 1)
            .Select(x => x.Id)
            .ToList();
        var batches = receiptMovementIds.Count == 0
            ? []
            : await _batchQuery.GetAllAsync(
                _queryBuilder.For<WarehouseProductBatch>()
                    .Where(x => receiptMovementIds.Contains(x.ReceiptMovementId))
                    .Build(),
                ct);
        var batchIds = batches.Select(x => x.Id).ToList();

        if (batchIds.Count > 0 &&
            (await _allocationQuery.AnyAsync(x => batchIds.Contains(x.BatchId), ct) ||
             await _saleBatchQuery.AnyAsync(x => batchIds.Contains(x.WarehouseProductBatchId), ct) ||
             await _shipmentBatchQuery.AnyAsync(x => batchIds.Contains(x.BatchId), ct)))
            return Result.Failure(OpeningInventoryErrors.EffectsAlreadyUsed(document.Id));

        var productTableIds = document.OpeningInventoryProducts
            .SelectMany(x => x.OpeningInventoryTables)
            .Select(x => x.ProductTableId)
            .Distinct()
            .ToList();
        if (productTableIds.Count == 0)
            return Result.Success();

        var isReferenced =
            await _saleItemQuery.AnyAsync(x => productTableIds.Contains(x.ProductTableId), ct) ||
            await _shipmentItemQuery.AnyAsync(x => productTableIds.Contains(x.ProductTableId), ct) ||
            await _transferItemQuery.AnyAsync(x => productTableIds.Contains(x.ProductTableId), ct) ||
            await _adjustmentItemQuery.AnyAsync(
                x => x.ProductTableId.HasValue && productTableIds.Contains(x.ProductTableId.Value), ct) ||
            await _countItemQuery.AnyAsync(
                x => x.ProductTableId.HasValue && productTableIds.Contains(x.ProductTableId.Value), ct);

        return isReferenced
            ? Result.Failure(OpeningInventoryErrors.EffectsAlreadyUsed(document.Id))
            : Result.Success();
    }

    private async Task DeleteWarehouseEffectRowsAsync(long documentId, CancellationToken ct)
    {
        var movements = await _movementQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductMovement>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.OPENINGINVENTORY &&
                            x.DocumentId == documentId)
                .Build(),
            ct);
        var movementIds = movements.Select(x => x.Id).ToList();
        if (movementIds.Count == 0)
            return;

        var batches = await _batchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(x => movementIds.Contains(x.ReceiptMovementId))
                .Build(),
            ct);
        var batchIds = batches.Select(x => x.Id).ToList();

        await _allocationCommand.DeleteAsync(
            x => movementIds.Contains(x.IssueMovementId) || batchIds.Contains(x.BatchId), ct);
        if (batchIds.Count > 0)
        {
            await _batchTableCommand.DeleteAsync(x => batchIds.Contains(x.BatchId), ct);
            await _batchCommand.DeleteAsync(x => batchIds.Contains(x.Id), ct);
        }
        await _movementCommand.DeleteAsync(x => movementIds.Contains(x.Id), ct);
    }

    private async Task<Result> SyncOpeningBalanceAsync(
        OpeningInventory document,
        CancellationToken ct)
    {
        await RemoveOpeningBalanceDetailsAsync(document.Id, ct);

        var openingBalance = await _openingBalanceQuery.GetAsync(
            _queryBuilder.For<OpeningBalance>()
                .Where(x => x.OrganizationId == document.OrganizationId &&
                            x.StateId == StateIdConst.ACTIVE)
                .Build(),
            ct);
        if (openingBalance is null)
            return Result.Failure(OpeningInventoryErrors.OpeningBalanceNotFound(document.OrganizationId));

        var organizationConfig = await _organizationConfigQuery.GetAsync(
            _queryBuilder.For<OrganizationConfig>()
                .Where(x => x.OrganizationId == document.OrganizationId)
                .Build(),
            ct);
        if (organizationConfig?.BaseCurrencyId is null)
            return Result.Failure(OpeningInventoryErrors.BaseCurrencyNotConfigured(document.OrganizationId));
        var currencyId = organizationConfig.BaseCurrencyId.Value;

        // Zero-cost stock is valid in inventory, but the opening-balance detail
        // table explicitly forbids a row with both debit and credit equal to zero.
        var accountingLines = document.OpeningInventoryProducts
            .Where(x => x.Amount > 0m)
            .ToList();
        if (accountingLines.Count == 0)
            return Result.Success();

        var accountIds = accountingLines.Select(x => x.DebitAccountId).Distinct().ToList();
        var accounts = await _openingBalanceAccountQuery.GetAllAsync(
            _queryBuilder.For<OpeningBalanceAccount>()
                .Where(x => x.OpeningBalanceId == openingBalance.Id &&
                            accountIds.Contains(x.ChartAccountId))
                .Build(),
            ct);
        var accountByChartId = accounts.ToDictionary(x => x.ChartAccountId);

        var newAccounts = accountingLines
            .GroupBy(x => x.DebitAccountId)
            .Where(group => !accountByChartId.ContainsKey(group.Key))
            .Select(group => new OpeningBalanceAccount
            {
                OpeningBalanceId = openingBalance.Id,
                ChartAccountId = group.Key,
                DebitAmount = group.Sum(x => x.Amount),
                CreditAmount = 0m,
                CreatedDate = DateTime.Now
            })
            .ToList();
        if (newAccounts.Count > 0)
        {
            await _openingBalanceAccountCommand.CreateAsync(newAccounts, ct);
            foreach (var account in newAccounts)
                accountByChartId[account.ChartAccountId] = account;
        }

        var configurations = await _chartAccountSubkontoQuery.GetAllAsync(
            _queryBuilder.For<ChartAccountSubkonto>()
                .Where(x => accountIds.Contains(x.AccountId) &&
                            x.StateId == StateIdConst.ACTIVE &&
                            x.SubkontoType.StateId == StateIdConst.ACTIVE)
                .As(x => new OpeningInventorySubkontoConfig(
                    x.AccountId,
                    x.SubkontoTypeId,
                    x.SortOrder,
                    x.SubkontoType.SourceTable))
                .Build(),
            ct);
        var configurationsByAccount = configurations
            .GroupBy(x => x.AccountId)
            .ToDictionary(x => x.Key, x => x.OrderBy(item => item.SortOrder).ToList());

        var receiptBatches = await _batchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(x => x.ReceiptMovement.DocumentTypeId == DocumentTypeIdConst.OPENINGINVENTORY &&
                            x.ReceiptMovement.DocumentId == document.Id)
                .As(x => new OpeningInventoryBatchLink(x.ProductId, x.Id))
                .Build(),
            ct);
        var batchIdByProductId = receiptBatches
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Single().BatchId);

        var details = new List<OpeningBalanceAccountDetail>(accountingLines.Count);
        var lineByDetail = new Dictionary<OpeningBalanceAccountDetail, OpeningInventoryProduct>();
        var sortOrder = 1;
        foreach (var line in accountingLines)
        {
            var detail = new OpeningBalanceAccountDetail
            {
                OpeningBalanceAccountId = accountByChartId[line.DebitAccountId].Id,
                DebitAmount = line.Amount,
                CreditAmount = 0m,
                Quantity = line.Quantity,
                CurrencyId = currencyId,
                CurrencyAmount = line.Amount,
                ExchangeRate = 1m,
                Description = $"{document.DocNumber}: {line.Product.Name}",
                SortOrder = sortOrder++,
                SourceDocumentTypeId = DocumentTypeIdConst.OPENINGINVENTORY,
                SourceDocumentId = document.Id,
                SourceLineId = line.Id,
                CreatedDate = DateTime.Now
            };
            details.Add(detail);
            lineByDetail[detail] = line;
        }
        await _openingBalanceDetailCommand.CreateAsync(details, ct);

        var subkontos = new List<OpeningBalanceAccountDetailSubkonto>();
        foreach (var detail in details)
        {
            var line = lineByDetail[detail];
            foreach (var config in configurationsByAccount.GetValueOrDefault(line.DebitAccountId, []))
            {
                var value = ResolveSubkontoValue(
                    config,
                    document,
                    line,
                    batchIdByProductId.GetValueOrDefault(line.ProductId));
                if (!value.HasValue)
                    return Result.Failure(
                        OpeningInventoryErrors.SubkontoValueUnavailable(
                            line.DebitAccountId, config.SubkontoTypeId));

                subkontos.Add(new OpeningBalanceAccountDetailSubkonto
                {
                    OpeningBalanceAccountDetailId = detail.Id,
                    SubkontoTypeId = config.SubkontoTypeId,
                    SubkontoId = value.Value,
                    SortOrder = checked((short)config.SortOrder),
                    CreatedDate = DateTime.Now
                });
            }
        }

        if (subkontos.Count > 0)
            await _openingBalanceSubkontoCommand.CreateAsync(subkontos, ct);

        await RecalculateOpeningBalanceAccountsAsync(
            accountByChartId.Values.Select(x => x.Id).Distinct().ToList(), ct);
        return Result.Success();
    }

    private async Task RemoveOpeningBalanceDetailsAsync(long documentId, CancellationToken ct)
    {
        var details = await _openingBalanceDetailQuery.GetAllAsync(
            _queryBuilder.For<OpeningBalanceAccountDetail>()
                .Where(x => x.SourceDocumentTypeId == DocumentTypeIdConst.OPENINGINVENTORY &&
                            x.SourceDocumentId == documentId)
                .Build(),
            ct);
        if (details.Count == 0)
            return;

        var detailIds = details.Select(x => x.Id).ToList();
        var accountIds = details.Select(x => x.OpeningBalanceAccountId).Distinct().ToList();
        await _openingBalanceSubkontoCommand.DeleteAsync(
            x => detailIds.Contains(x.OpeningBalanceAccountDetailId), ct);
        await _openingBalanceDetailCommand.DeleteAsync(details, ct);
        await RecalculateOpeningBalanceAccountsAsync(accountIds, ct);
    }

    private async Task RecalculateOpeningBalanceAccountsAsync(
        IReadOnlyCollection<long> accountIds,
        CancellationToken ct)
    {
        if (accountIds.Count == 0)
            return;

        var accounts = await _openingBalanceAccountQuery.GetAllAsync(
            _queryBuilder.For<OpeningBalanceAccount>()
                .Where(x => accountIds.Contains(x.Id))
                .Build(),
            ct);
        var details = await _openingBalanceDetailQuery.GetAllAsync(
            _queryBuilder.For<OpeningBalanceAccountDetail>()
                .Where(x => accountIds.Contains(x.OpeningBalanceAccountId))
                .Build(),
            ct);
        var detailsByAccount = details
            .GroupBy(x => x.OpeningBalanceAccountId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var toDelete = new List<OpeningBalanceAccount>();
        var toUpdate = new List<OpeningBalanceAccount>();
        foreach (var account in accounts)
        {
            if (!detailsByAccount.TryGetValue(account.Id, out var accountDetails) ||
                accountDetails.Count == 0)
            {
                toDelete.Add(account);
                continue;
            }

            var debit = accountDetails.Sum(x => x.DebitAmount);
            var credit = accountDetails.Sum(x => x.CreditAmount);
            account.DebitAmount = debit > credit ? debit - credit : 0m;
            account.CreditAmount = credit > debit ? credit - debit : 0m;
            toUpdate.Add(account);
        }

        if (toUpdate.Count > 0)
            await _openingBalanceAccountCommand.UpdateAsync(toUpdate, ct);
        if (toDelete.Count > 0)
            await _openingBalanceAccountCommand.DeleteAsync(toDelete, ct);
    }

    private static long? ResolveSubkontoValue(
        OpeningInventorySubkontoConfig config,
        OpeningInventory document,
        OpeningInventoryProduct line,
        long? batchId)
    {
        var sourceTable = config.SourceTable;
        switch (config.SubkontoTypeId)
        {
            case SubkontoTypeIdConst.InventoryItems:
            case SubkontoTypeIdConst.InventoryItemsTurnover:
            case SubkontoTypeIdConst.ProductsTurnover:
                return line.ProductId;
            case SubkontoTypeIdConst.Warehouses:
                return document.WarehouseId;
            case SubkontoTypeIdConst.Batches:
            case SubkontoTypeIdConst.BatchesTurnover:
                return batchId;
            case SubkontoTypeIdConst.Counterparties:
            case SubkontoTypeIdConst.CounterpartiesTurnover:
                return document.CounterpartyId;
            case SubkontoTypeIdConst.Contracts:
                return document.ContractId;
            case SubkontoTypeIdConst.ProductGroups:
            case SubkontoTypeIdConst.ProductGroupsTurnover:
                return line.Product.ProductGroupId;
        }

        if (sourceTable.Equals(TableName<Product>(), StringComparison.OrdinalIgnoreCase))
            return line.ProductId;
        if (sourceTable.Equals(TableName<Warehouse>(), StringComparison.OrdinalIgnoreCase))
            return document.WarehouseId;
        if (sourceTable.Equals(TableName<CounterpartyCard>(), StringComparison.OrdinalIgnoreCase))
            return document.CounterpartyId;
        if (sourceTable.Equals(TableName<Contract>(), StringComparison.OrdinalIgnoreCase))
            return document.ContractId;
        if (sourceTable.Equals(TableName<WarehouseProductBatch>(), StringComparison.OrdinalIgnoreCase))
            return batchId;
        if (sourceTable.Equals(TableName<OpeningInventory>(), StringComparison.OrdinalIgnoreCase))
            return document.Id;

        return null;
    }

    private static string TableName<TEntity>() =>
        typeof(TEntity).GetCustomAttributes(typeof(TableAttribute), inherit: false)
            .OfType<TableAttribute>()
            .Single()
            .Name;

    private sealed record OpeningInventorySubkontoConfig(
        int AccountId,
        short SubkontoTypeId,
        int SortOrder,
        string SourceTable);

    private sealed record OpeningInventoryBatchLink(int ProductId, long BatchId);
}
