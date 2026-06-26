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
        public PostingService(IQueryBuilder queryBuilder,
                              IQueryRepository<PostingRule> ruleQuery,
                              IQueryRepository<AccountResolveRule> accountResolveRuleQuery)
        {
            _ruleQuery = ruleQuery;
            _queryBuilder = queryBuilder;
            _accountResolveRuleQuery = accountResolveRuleQuery;
        }

        public async Task<List<AccountingRegisterEntry>> BuildEntriesAsync(List<PostingContext> contexts)
        {
            var postingRuleIds = contexts.Select(s => s.RuleId);

            var postingRuleQuery = _queryBuilder.For<PostingRule>().Where(x => postingRuleIds.Contains(x.Id)).Build();
            postingRuleQuery.AddIncludes(x => x.Include(i => i.PostingRuleLines));

            var postingRules = await _ruleQuery.GetAllAsync(postingRuleQuery);

            var resolveRules = await GetResolveRulesAsync(postingRules);

            var result = new List<AccountingRegisterEntry>();

            foreach (var context in contexts)
            {
                var postingRule = postingRules.FirstOrDefault(f => f.Id == context.RuleId);
                if (postingRule == null)
                    throw new ArgumentException($"Шаблон проводки не найден в acc_posting_template.");

                foreach (var line in postingRule.PostingRuleLines)
                {
                    if (string.IsNullOrWhiteSpace(line.AmountSource))
                    {
                        if (line.IsOptional)
                            continue;

                        throw new ArgumentException("Для шаблона не указан источник суммы.");
                    }

                    if (!context.Amounts.TryGetValue(line.AmountSource, out var amount))
                    {
                        if (line.IsOptional)
                            continue;

                        throw new ArgumentException($"Для шаблона не передана сумма источника {line.AmountSource}.");
                    }

                    if (amount == 0m)
                        continue;

                    var debitAccountId = GetAccountId(line.DebitAlias, context, resolveRules);
                    var creditAccountId = GetAccountId(line.CreditAlias, context, resolveRules);

                    var entry = new AccountingRegisterEntry
                    {
                        OrganizationId = context.OrganizationId,
                        DocumentTypeId = GetDocumentTypeId(context.RuleId),
                        DocumentId = context.DocumentId,
                        DebitAccountId = debitAccountId,
                        CreditAccountId = creditAccountId,
                        CurrencyId = context.CurrencyId,
                        Amount = amount,
                        DocDate = context.DocDate,
                        CreatedDate = DateTime.UtcNow,
                        DebitQuantity = context.DebitQuantity,
                        CreditQuantity = context.CreditQuantity,
                        Content = postingRule.Name,
                        JournalNumber = context.JournalNumber,
                    };

                    // Субконто: применяем те, что относятся к DT, к дебетовой стороне,
                    // те, что к CT — к кредитовой, а без AppliesTo — к обеим сторонам.

                    var ctSubkontos = GetSubkontos(line.CreditAlias, SubkontoSideConst.CREDIT, context.Subkontos);
                    var dtSubkontos = GetSubkontos(line.DebitAlias, SubkontoSideConst.DEBIT, context.Subkontos);

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
            }

            return result;
        }

        private async Task<List<AccountResolveRule>> GetResolveRulesAsync(List<PostingRule> postingRules)
        {
            var aliases = postingRules
                            .SelectMany(t => t.PostingRuleLines.Select(l => l.CreditAlias))
                            .Concat(postingRules.SelectMany(t => t.PostingRuleLines.Select(l => l.DebitAlias)))
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
                AliasConst.Inventory => context.ProductCategory ?? "_default",
                AliasConst.Expense => context.ServiceType ?? "_default",
                AliasConst.AssetWriteOff => context.AssetType ?? "_default",
                AliasConst.PaymentAccount => context.PaymentMethod ?? "_default",
                _ => "_default"
            };

            var datas = rules.Where(x => x.Alias == alias);

            if (datas.Any(a => a.DimensionValue == dimensionValue))
                return datas.First(f => f.DimensionValue == dimensionValue);

            return datas.FirstOrDefault(f => f.DimensionValue == "_default") ?? datas.OrderBy(o => o.Priority).First();
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
            };

            return result;
        }

        private static short GetDocumentTypeId(short ruleId) =>
            ruleId switch
            {
                PostingRuleIdConst.PURCHASE_GOODS or PostingRuleIdConst.PURCHASE_SERVICE => DocumentTypeIdConst.PURCHASE,
                PostingRuleIdConst.SALE_GOODS or PostingRuleIdConst.SALE_SERVICE => DocumentTypeIdConst.SALE,
                
                _ => ruleId
            };
    }
}
