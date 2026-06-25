using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Register.AccountingRegisterEntries.Services;
using Domain.Entities;
using LinqKit;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Text.Json;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class PurchaseAccountingHandler : IAccountingDocumentHandler<PurchaseDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IPurchaseSubkontoNamesResolver _resolver;
        private readonly IQueryRepository<PostingRule> _postingRuleQuery;
        public PurchaseAccountingHandler(IUserContext userContext,
                                         IQueryBuilder queryBuilder,
                                         IPurchaseSubkontoNamesResolver resolver,
                                         IQueryRepository<PostingRule> postingRuleQuery)
        {
            _resolver = resolver;
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _postingRuleQuery = postingRuleQuery;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
        {
            var rule = await GetRuleAsync(ct);
            if (rule is null)
                return Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.PostingRuleNotFound(_userContext.LanguageId));

            var entries = new List<AccountingRegisterEntry>();

            var subkontoContext = await _resolver.FillSubkontoContext(purchase);

            foreach (var productContext in subkontoContext.Products)
            {
                foreach (var ruleLine in rule.PostingRuleLines.OrderBy(x => x.SortOrder))
                {
                    entries.Add(BuildEntry(subkontoContext, productContext, ruleLine));
                }
            }

            return Result.Success(entries);
        }

        private AccountingRegisterEntry BuildEntry(PurchaseSubkontoContext context, ProductPurchaseSubkontoContext productContext, PostingRuleLine ruleLine)
        {
            var amount = GetAmount(productContext, ruleLine.AmountSource);
            var quantity = GetQuantity(productContext, ruleLine.QuantitySource);

            return new AccountingRegisterEntry
            {
                Amount = amount,

                OrganizationId = context.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                OperationTypeId = OperationTypeIdConst.IN,

                CreatedDate = DateTime.Now,
                DocDate = context.DocDate,

                DebitAccountId = ruleLine.DebitAccountId,
                CreditAccountId = ruleLine.CreditAccountId,

                DebitQuantity = quantity,
                CreditQuantity = quantity,

                CurrencyId = context.CurrencyId,
                DocumentId = context.Id,

                Content = ruleLine.ContentTemplate,

                RegisterEntrySubkontos = BuildSubkontos(context, productContext, ruleLine)
            };
        }

        private List<RegisterEntrySubkonto> BuildSubkontos(PurchaseSubkontoContext context, ProductPurchaseSubkontoContext productContext, PostingRuleLine ruleLine)
        {
            var list = new List<RegisterEntrySubkonto>();

            if (ruleLine.AmountSource == PostingAmountFields.Amount)
            {
                list.Add(new RegisterEntrySubkonto
                {
                    SortOrder = list.Count + 1,
                    EntityId = productContext.ProductId,
                    DisplayValue = productContext.ProductName,
                    SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                    Side = SubkontoSideConst.DEBIT,
                    CreatedDate = DateTime.Now,
                });

                list.Add(new RegisterEntrySubkonto
                {
                    SortOrder = list.Count + 1,
                    EntityId = context.WarehouseId,
                    DisplayValue = context.WarehouseName,
                    SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                    Side = SubkontoSideConst.DEBIT,
                    CreatedDate = DateTime.Now,
                });
            }

            if (ruleLine.AmountSource == PostingAmountFields.VatAmount)
            {
                list.Add(new RegisterEntrySubkonto
                {
                    SortOrder = list.Count + 1,
                    EntityId = context.CounterpartyId,
                    DisplayValue = context.CounterpartyName,
                    SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                    Side = SubkontoSideConst.DEBIT,
                    CreatedDate = DateTime.Now,
                });
            }

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = context.Id,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    purchase = new
                    {
                        number = context.DocNumber,
                        date = context.DocDate
                    }
                }),
                SubkontoTypeId = SubkontoTypeIdConst.PURCHASE,
                Side = SubkontoSideConst.DEBIT,
                CreatedDate = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = context.CounterpartyId,
                DisplayValue = context.CounterpartyName,
                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                Side = SubkontoSideConst.CREDIT,
                CreatedDate = DateTime.Now,
            });

            if (context.ContractId is not null)
                list.Add(new RegisterEntrySubkonto
                {
                    SortOrder = list.Count + 1,
                    EntityId = context.ContractId,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        contract = new
                        {
                            number = context.ContractNumber,
                            date = context.ContractDate
                        }
                    }),
                    SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
                    Side = SubkontoSideConst.CREDIT,
                    CreatedDate = DateTime.Now,
                });

            return list;
        }

        private decimal GetAmount(ProductPurchaseSubkontoContext productContext, string source)
        {
            return source switch
            {
                PostingAmountFields.Amount => productContext.Amount,
                PostingAmountFields.VatAmount => productContext.VatAmount,
                _ => 0
            };
        }

        private decimal? GetQuantity(ProductPurchaseSubkontoContext productContext, string? source)
        {
            if (source is null)
                return null;

            return source switch
            {
                PostingQuantityFields.Quantity => productContext.Quantity,
                _ => null
            };
        }

        private async Task<PostingRule?> GetRuleAsync(CancellationToken ct = default)
        {
            var query = _queryBuilder
                .For<PostingRule>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE)
                .Build();

            query.AddIncludes(e => e.Include(i => i.PostingRuleLines));

            return await _postingRuleQuery.GetAsync(query, ct);
        }
    }
}
