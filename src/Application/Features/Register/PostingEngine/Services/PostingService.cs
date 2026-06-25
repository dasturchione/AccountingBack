using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Register.PostingEngine
{
    public class PostingService
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<PostingTemplate> _templateQuery;
        private readonly IQueryRepository<AccountResolveRule> _accountResolveRuleQuery;
        public PostingService(IQueryBuilder queryBuilder,
                              IQueryRepository<PostingTemplate> templateQuery,
                              IQueryRepository<AccountResolveRule> accountResolveRuleQuery)
        {
            _queryBuilder = queryBuilder;
            _templateQuery = templateQuery;
            _accountResolveRuleQuery = accountResolveRuleQuery;
        }

        public async Task<List<AccountingRegisterEntry>> BuildEntriesAsync(List<PostingContext> contexts)
        {
            var documentTypeIds = contexts.Select(s => s.DocumentTypeId);
            var postingTemplateQuery = _queryBuilder.For<PostingTemplate>().Where(x => documentTypeIds.Contains(x.DocumentTypeId)).Build();
            postingTemplateQuery.AddIncludes(x => x.Include(i => i.PostingTemplateLines));
            var templates = await _templateQuery.GetAllAsync(postingTemplateQuery);

            var rules = await GetResolveRulesAsync(templates);

            var result = new List<AccountingRegisterEntry>();

            foreach (var context in contexts)
            {
                var template = templates.FirstOrDefault(f => f.DocumentTypeId == context.DocumentTypeId);
                if (template == null)
                    throw new ArgumentException($"Шаблон проводки не найден в acc_posting_template.");

                foreach (var line in template.PostingTemplateLines)
                {
                    if (!context.Amounts.TryGetValue(line.AmountSource, out var amount))
                    {
                        if (line.IsOptional)
                            continue;

                        throw new ArgumentException($"Для шаблона не передана сумма источника {line.AmountSource}.");
                    }

                    var debitAccountId = GetAccountId(line.DebitAlias, context, rules);
                    var creditAccountId = GetAccountId(line.CreditAlias, context, rules);

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
                        CreatedDate = DateTime.UtcNow,
                        DebitQuantity = context.DebitQuantity,
                        CreditQuantity = context.CreditQuantity,
                        Content = template.Name,
                        JournalNumber = context.JournalNumber,
                    };

                    // Субконто: применяем те, что относятся к DT, к дебетовой стороне,
                    // те, что к CT — к кредитовой, а без AppliesTo — к обеим сторонам.

                    var ctSubkontos = GetSubkontos(line.CreditAlias, SubkontoSideConst.CREDIT, context.Subkontos);
                    var dtSubkontos = GetSubkontos(line.CreditAlias, SubkontoSideConst.DEBIT, context.Subkontos);

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

        private async Task<List<AccountResolveRule>> GetResolveRulesAsync(List<PostingTemplate> templates)
        {
            var aliases = templates
                            .SelectMany(t => t.PostingTemplateLines.Select(l => l.CreditAlias))
                            .Concat(templates.SelectMany(t => t.PostingTemplateLines.Select(l => l.DebitAlias)))
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
                    subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.CONTRACT ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.COUNTER_PARTY)
                             .ToList();
                    break;

                case AliasConst.Inventory:
                    subkontos.Where(x => x.SubkontoTypeId == SubkontoTypeIdConst.PRODUCT ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.WAREHOUSE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.PURCHASE ||
                                         x.SubkontoTypeId == SubkontoTypeIdConst.SALE)
                             .ToList();
                    break;
            };

            return result;
        }
    }
}
