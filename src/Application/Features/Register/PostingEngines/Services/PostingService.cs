using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Register.PostingEngines
{
    public class PostingService : IPostingService
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<PostingRule> _ruleQuery;
        private readonly IQueryRepository<AccountResolveRule> _accountResolveRuleQuery;
        private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
        public PostingService(IQueryBuilder queryBuilder,
                              IQueryRepository<PostingRule> ruleQuery,
                              IQueryRepository<AccountResolveRule> accountResolveRuleQuery,
                              IQueryRepository<ChartAccount> chartAccountQuery)
        {
            _ruleQuery = ruleQuery;
            _queryBuilder = queryBuilder;
            _accountResolveRuleQuery = accountResolveRuleQuery;
            _chartAccountQuery = chartAccountQuery;
        }

        public async Task<List<AccountingRegisterEntry>> BuildEntriesAsync(List<PostingContext> contexts)
        {
            var postingRuleIds = contexts.Select(s => s.RuleId);

            var postingRuleQuery = _queryBuilder.For<PostingRule>().Where(x => postingRuleIds.Contains(x.Id)).Build();
            postingRuleQuery.AddIncludes(x => x.Include(i => i.PostingRuleLines).ThenInclude(l => l.DebitAlias));
            postingRuleQuery.AddIncludes(x => x.Include(i => i.PostingRuleLines).ThenInclude(l => l.CreditAlias));

            var postingRules = await _ruleQuery.GetAllAsync(postingRuleQuery);

            var resolveRules = await GetResolveRulesAsync(postingRules);
            var quantityAccountIds = await GetQuantityAccountIdsAsync(resolveRules);

            var result = new List<AccountingRegisterEntry>();

            foreach (var context in contexts)
            {
                var postingRule = postingRules.FirstOrDefault(f => f.Id == context.RuleId);
                if (postingRule == null)
                    throw new ArgumentException($"Шаблон проводки не найден в acc_posting_template.");

                var entriesCountBeforeContext = result.Count;

                foreach (var line in postingRule.PostingRuleLines)
                {
                    if (ShouldSkipOptionalLine(context, line))
                        continue;

                    if (string.IsNullOrWhiteSpace(line.AmountSource))
                    {
                        if (line.IsOptional)
                            continue;

                        throw new ArgumentException("Для шаблона не указан источник суммы.");
                    }

                    if (IsSkippedAmountSource(context, line.AmountSource))
                        continue;

                    if (!context.Amounts.TryGetValue(line.AmountSource, out var amount))
                    {
                        if (line.IsOptional)
                            continue;

                        throw new ArgumentException($"Для шаблона не передана сумма источника {line.AmountSource}.");
                    }

                    if (amount == 0m)
                        continue;

                    var debitAlias = line.DebitAlias.Code;
                    var creditAlias = line.CreditAlias.Code;

                    var debitAccountId = GetAccountId(debitAlias, context, resolveRules);
                    var creditAccountId = GetAccountId(creditAlias, context, resolveRules);

                    var entry = new AccountingRegisterEntry
                    {
                        OrganizationId = context.OrganizationId,
                        DocumentTypeId = context.DocumentTypeId,
                        DocumentId = context.DocumentId,
                        DebitAccountId = debitAccountId,
                        CreditAccountId = creditAccountId,
                        CurrencyId = context.CurrencyId,
                        Amount = amount,
                        DocDate = context.DocDate,
                        CreatedDate = DateTime.Now,
                        DebitQuantity = quantityAccountIds.Contains(debitAccountId) ? context.DebitQuantity : null,
                        CreditQuantity = quantityAccountIds.Contains(creditAccountId) ? context.CreditQuantity : null,
                        Content = postingRule.Name,
                        JournalNumber = context.JournalNumber,
                        SourceLineId = context.SourceLineId,
                    };

                    // Субконто: применяем те, что относятся к DT, к дебетовой стороне,
                    // те, что к CT — к кредитовой, а без AppliesTo — к обеим сторонам.

                    var ctSubkontos = GetSubkontos(creditAlias, SubkontoSideConst.CREDIT, context.Subkontos);
                    var dtSubkontos = GetSubkontos(debitAlias, SubkontoSideConst.DEBIT, context.Subkontos);

                    foreach (var subkonto in ctSubkontos)
                    {
                        entry.RegisterEntrySubkontos.Add(subkonto);
                    }

                    foreach (var subkonto in dtSubkontos)
                    {
                        entry.RegisterEntrySubkontos.Add(subkonto);
                    }

                    result.Add(entry);
                }

                if (HasRequiredAliases(context) && result.Count == entriesCountBeforeContext)
                {
                    throw new ArgumentException(
                        $"В шаблоне проводки не найдена строка для DebitAlias='{context.RequiredDebitAlias}' и CreditAlias='{context.RequiredCreditAlias}'.");
                }
            }

            return result;
        }

        private async Task<HashSet<int>> GetQuantityAccountIdsAsync(List<AccountResolveRule> resolveRules)
        {
            var accountIds = resolveRules
                .Select(x => x.AccountId)
                .Distinct()
                .ToList();

            if (accountIds.Count == 0)
                return [];

            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => accountIds.Contains(x.Id) && x.IsQuantity)
                .As(x => x.Id)
                .Build();

            var ids = await _chartAccountQuery.GetAllAsync(query);
            return ids.ToHashSet();
        }

        private async Task<List<AccountResolveRule>> GetResolveRulesAsync(List<PostingRule> postingRules)
        {
            var aliases = postingRules
                            .SelectMany(t => t.PostingRuleLines.Select(l => l.CreditAlias.Code))
                            .Concat(postingRules.SelectMany(t => t.PostingRuleLines.Select(l => l.DebitAlias.Code)))
                            .Distinct()
                            .ToList();

            var query = _queryBuilder.For<AccountResolveRule>().Where(x => aliases.Contains(x.Alias)).Build();
            var rules = await _accountResolveRuleQuery.GetAllAsync(query);

            return rules;
        }

        private int GetAccountId(string alias, PostingContext context, List<AccountResolveRule> rules)
        {
            var rule = GetRule(alias, context, rules);
            return rule.AccountId;
        }

        private AccountResolveRule GetRule(string alias, PostingContext context, List<AccountResolveRule> rules)
        {
            var dimensionValue = alias switch
            {
                AliasConst.Inventory or AliasConst.CostOfGoods or AliasConst.SalesRevenue =>
                    context.ProductCategory ?? RegisterDefaultsConst.DefaultDimensionValue,
                AliasConst.Expense or AliasConst.CostOfService or AliasConst.ServiceRevenue =>
                    context.ServiceType ?? RegisterDefaultsConst.DefaultDimensionValue,
                AliasConst.AssetWriteOff => context.AssetType ?? RegisterDefaultsConst.DefaultDimensionValue,
                AliasConst.PaymentAccount => context.PaymentMethod ?? RegisterDefaultsConst.DefaultDimensionValue,
                AliasConst.VATIn => context.VatKind ?? RegisterDefaultsConst.DefaultDimensionValue,
                _ => RegisterDefaultsConst.DefaultDimensionValue
            };

            var aliasRules = rules.Where(x => x.Alias == alias).ToList();

            // Policy-aware resolution: prefer rules configured for the document's accounting
            // policy; if none exist for that policy, fall back to any rule for the alias
            // (backward compatible with data that predates policy-scoped resolve rules).
            var policyRules = aliasRules.Where(x => x.PolicyId == context.AccountingPolicyId).ToList();
            var datas = policyRules.Count > 0 ? policyRules : aliasRules;

            if (datas.Count == 0)
                throw new ArgumentException(
                    $"Для alias '{alias}' не найдено правило разрешения счёта (acc_account_resolve_rule).");

            if (datas.Any(a => a.DimensionValue == dimensionValue))
                return datas.First(f => f.DimensionValue == dimensionValue);

            return datas.FirstOrDefault(f => f.DimensionValue == RegisterDefaultsConst.DefaultDimensionValue)
                ?? datas.OrderBy(o => o.Priority).First();
        }

        private List<RegisterEntrySubkonto> GetSubkontos(string alias, string side, List<SubkontoValue> subkontos)
        {
            var items = GetSubkontoValues(alias, subkontos);

            var result = items.Select(s => new RegisterEntrySubkonto()
            {
                EntityId = s.EntityId,
                DisplayValue = s.DisplayValue,
                Side = side,
                SortOrder = s.SortOrder,
                SubkontoTypeId = s.SubkontoTypeId,
                CreatedDate = DateTime.Now
            }).ToList();

            return result;
        }

        private List<SubkontoValue> GetSubkontoValues(string alias, List<SubkontoValue> subkontos)
        {
            var result = new List<SubkontoValue>();

            switch (alias)
            {
                case AliasConst.VATIn:
                case AliasConst.VATOut:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.PURCHASE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.SALE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY)
                             .ToList();
                    break;

                case AliasConst.Supplier:
                case AliasConst.Customer:
                case AliasConst.SupplierAdvance:
                case AliasConst.CustomerAdvance:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.CONTRACT ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY)
                             .ToList();
                    break;

                case AliasConst.Inventory:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.PRODUCT ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.WAREHOUSE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.PURCHASE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.SALE)
                             .ToList();
                    break;
                case AliasConst.CashBoxSource:
                case AliasConst.CashBoxDestination:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.CASH_BOX)
                             .ToList();
                    break;
                case AliasConst.PaymentAccount:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.BANK_ACCOUNT ||
                                                  x.SubkontoTypeId == SubkontoTypeIdConst.CASH_BOX)
                             .ToList();
                    break;

                case AliasConst.FixedAsset:
                case AliasConst.FixedAssetInProgress:
                case AliasConst.FixedAssetDepreciation:
                    result = subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.FIXED_ASSET)
                             .ToList();
                    break;
            };

            return result;
        }

        private static bool ShouldSkipOptionalLine(PostingContext context, PostingRuleLine line)
        {
            if (!line.IsOptional || context.AllowedAliases.Length == 0)
                return false;

            if (string.Equals(line.DebitAlias.Code, AliasConst.PaymentAccount, StringComparison.OrdinalIgnoreCase))
                return !context.AllowedAliases.Contains(line.CreditAlias.Code, StringComparer.OrdinalIgnoreCase);

            if (string.Equals(line.CreditAlias.Code, AliasConst.PaymentAccount, StringComparison.OrdinalIgnoreCase))
                return !context.AllowedAliases.Contains(line.DebitAlias.Code, StringComparer.OrdinalIgnoreCase);

            return !context.AllowedAliases.Contains(line.DebitAlias.Code, StringComparer.OrdinalIgnoreCase) &&
                   !context.AllowedAliases.Contains(line.CreditAlias.Code, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsSkippedAmountSource(PostingContext context, string amountSource)
        {
            return context.SkippedAmountSources is { Length: > 0 } &&
                   context.SkippedAmountSources.Any(source => string.Equals(source, amountSource, StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasRequiredAliases(PostingContext context) =>
            !string.IsNullOrWhiteSpace(context.RequiredDebitAlias) ||
            !string.IsNullOrWhiteSpace(context.RequiredCreditAlias);
    }
}
