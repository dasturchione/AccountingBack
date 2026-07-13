using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public class PostingService : IPostingService
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<ChartAccount> _chartAccountQuery;

        public PostingService(
            IQueryBuilder queryBuilder,
            IQueryRepository<ChartAccount> chartAccountQuery)
        {
            _queryBuilder = queryBuilder;
            _chartAccountQuery = chartAccountQuery;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> BuildEntriesAsync(List<PostingContext> contexts)
        {
            var quantityAccountIds = await GetQuantityAccountIdsAsync(contexts);
            var accountSubkontoMap = await GetAccountSubkontoMapAsync(contexts);
            var result = new List<AccountingRegisterEntry>();

            foreach (var context in contexts)
            {
                if (context.Entries.Count == 0)
                    return Result.Failure<List<AccountingRegisterEntry>>(Error.Business("PostingEngine.EmptyEntries", $"Для документа {context.DocumentTypeId}/{context.DocumentId} не созданы прямые строки проводок."));

                foreach (var line in context.Entries)
                {
                    if (line.Amount == 0m)
                        continue;

                    if (!line.DebitAccountId.HasValue || line.DebitAccountId.Value <= 0)
                        return Result.Failure<List<AccountingRegisterEntry>>(Error.Business("PostingEngine.DebitAccountMissing", $"Для документа {context.DocumentTypeId}/{context.DocumentId} не указан debit_account_id."));

                    if (!line.CreditAccountId.HasValue || line.CreditAccountId.Value <= 0)
                        return Result.Failure<List<AccountingRegisterEntry>>(Error.Business("PostingEngine.CreditAccountMissing", $"Для документа {context.DocumentTypeId}/{context.DocumentId} не указан credit_account_id."));

                    var debitAccountId = line.DebitAccountId.Value;
                    var creditAccountId = line.CreditAccountId.Value;

                    var entry = new AccountingRegisterEntry
                    {
                        OrganizationId = context.OrganizationId,
                        DocumentTypeId = context.DocumentTypeId,
                        DocumentId = context.DocumentId,
                        DebitAccountId = debitAccountId,
                        CreditAccountId = creditAccountId,
                        CurrencyId = context.CurrencyId,
                        Amount = line.Amount,
                        DocDate = context.DocDate,
                        CreatedDate = DateTime.Now,
                        DebitQuantity = quantityAccountIds.Contains(debitAccountId) ? line.DebitQuantity : null,
                        CreditQuantity = quantityAccountIds.Contains(creditAccountId) ? line.CreditQuantity : null,
                        Content = line.Content,
                        JournalNumber = context.JournalNumber,
                        SourceLineId = line.SourceLineId ?? context.SourceLineId,
                    };

                    foreach (var subkonto in GetSubkontos(creditAccountId, SubkontoSideConst.CREDIT, context.Subkontos, accountSubkontoMap))
                        entry.RegisterEntrySubkontos.Add(subkonto);

                    foreach (var subkonto in GetSubkontos(debitAccountId, SubkontoSideConst.DEBIT, context.Subkontos, accountSubkontoMap))
                        entry.RegisterEntrySubkontos.Add(subkonto);

                    result.Add(entry);
                }
            }

            return Result.Success(result);
        }

        private async Task<HashSet<int>> GetQuantityAccountIdsAsync(List<PostingContext> contexts)
        {
            var accountIds = GetContextAccountIds(contexts);
            if (accountIds.Count == 0)
                return [];

            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.IsQuantity)
                .As(x => x.Id)
                .Build();

            var ids = await _chartAccountQuery.GetAllAsync(query);
            return ids.ToHashSet();
        }

        private async Task<Dictionary<int, List<AccountSubkontoConfig>>> GetAccountSubkontoMapAsync(List<PostingContext> contexts)
        {
            var accountIds = GetContextAccountIds(contexts);
            if (accountIds.Count == 0)
                return new Dictionary<int, List<AccountSubkontoConfig>>();

            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id))
                .Build();
            query.AddIncludes(x => x.Include(i => i.ChartAccountSubkontos));

            var accounts = await _chartAccountQuery.GetAllAsync(query);

            return accounts.ToDictionary(
                account => account.Id,
                account => account.ChartAccountSubkontos
                    .Where(subkonto => subkonto.StateId == StateIdConst.ACTIVE)
                    .OrderBy(subkonto => subkonto.SortOrder)
                    .Select(subkonto => new AccountSubkontoConfig(
                        subkonto.SubkontoTypeId,
                        subkonto.SortOrder))
                    .ToList());
        }

        private static List<int> GetContextAccountIds(List<PostingContext> contexts) =>
            contexts
                .SelectMany(context => context.Entries)
                .SelectMany(entry => new[] { entry.DebitAccountId, entry.CreditAccountId })
                .Where(accountId => accountId.HasValue && accountId.Value > 0)
                .Select(accountId => accountId!.Value)
                .Distinct()
                .ToList();

        private static List<RegisterEntrySubkonto> GetSubkontos(
            int accountId,
            string side,
            List<SubkontoValue> subkontos,
            Dictionary<int, List<AccountSubkontoConfig>> accountSubkontoMap)
        {
            if (!accountSubkontoMap.TryGetValue(accountId, out var requiredSubkontos) || requiredSubkontos.Count == 0)
                return [];

            var availableSubkontos = subkontos
                .Where(subkonto => !subkonto.AppliesToAccountId.HasValue || subkonto.AppliesToAccountId.Value == accountId)
                .ToList();

            var result = new List<RegisterEntrySubkonto>();

            foreach (var requiredSubkonto in requiredSubkontos)
            {
                var value = FindSubkontoValue(requiredSubkonto.SubkontoTypeId, availableSubkontos);

                if (value == null)
                {
                    continue;
                }

                result.Add(new RegisterEntrySubkonto
                {
                    EntityId = value.EntityId,
                    DisplayValue = value.DisplayValue,
                    Side = side,
                    SortOrder = requiredSubkonto.SortOrder,
                    SubkontoTypeId = requiredSubkonto.SubkontoTypeId,
                    CreatedDate = DateTime.Now
                });
            }

            return result;
        }

        private static SubkontoValue? FindSubkontoValue(short requiredSubkontoTypeId, List<SubkontoValue> subkontos)
        {
            var exactValue = subkontos
                .Where(subkonto => subkonto.SubkontoTypeId == requiredSubkontoTypeId)
                .OrderBy(subkonto => subkonto.SortOrder)
                .FirstOrDefault();

            if (exactValue is not null)
                return exactValue;

            foreach (var fallbackTypeId in GetFallbackSubkontoTypeIds(requiredSubkontoTypeId))
            {
                var fallbackValue = subkontos
                    .Where(subkonto => subkonto.SubkontoTypeId == fallbackTypeId)
                    .OrderBy(subkonto => subkonto.SortOrder)
                    .FirstOrDefault();

                if (fallbackValue is not null)
                    return fallbackValue;
            }

            return null;
        }

        private static short[] GetFallbackSubkontoTypeIds(short requiredSubkontoTypeId) =>
            requiredSubkontoTypeId switch
            {
                SubkontoTypeIdConst.CounterpartiesTurnover => [SubkontoTypeIdConst.Counterparties],
                SubkontoTypeIdConst.InventoryItemsTurnover => [SubkontoTypeIdConst.InventoryItems],
                SubkontoTypeIdConst.ProductsTurnover => [SubkontoTypeIdConst.InventoryItems],
                SubkontoTypeIdConst.BatchesTurnover => [SubkontoTypeIdConst.Batches],
                SubkontoTypeIdConst.ProductGroupsTurnover => [SubkontoTypeIdConst.ProductGroups],
                SubkontoTypeIdConst.VatRatesTurnover => [SubkontoTypeIdConst.VatRates],
                SubkontoTypeIdConst.FixedAssetsTurnover => [SubkontoTypeIdConst.FixedAssets],
                SubkontoTypeIdConst.ReceivedInvoicesTurnover => [SubkontoTypeIdConst.CounterpartySettlementDocuments],
                _ => []
            };

        private sealed record AccountSubkontoConfig(short SubkontoTypeId, int SortOrder);
    }
}
