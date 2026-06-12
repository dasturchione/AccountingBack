using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Domain.Entities;
using LinqKit;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class PurchaseAccountingHandler : IAccountingDocumentHandler<PurchaseDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<PostingRule> _postingRuleQuery;
        public PurchaseAccountingHandler(IUserContext userContext,
                                         IQueryBuilder queryBuilder,
                                         IQueryRepository<PostingRule> postingRuleQuery)
        {
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

            var groupedByProduct = purchase.Lines.GroupBy(g => g.ProductTable.ProductId);

            foreach (var productLine in groupedByProduct)
            {
                foreach (var ruleLine in rule.PostingRuleLines.OrderBy(x => x.SortOrder))
                {
                    entries.Add(BuildEntry(purchase, productLine.ToList(), ruleLine));
                }
            }

            return Result.Success(entries);
        }

        private AccountingRegisterEntry BuildEntry(PurchaseDoc purchase, List<PurchaseDocTable> productLine, PostingRuleLine ruleLine)
        {
            var amount = GetAmount(productLine, ruleLine.AmountSource);
            var quantity = GetQuantity(productLine, ruleLine.QuantitySource);

            return new AccountingRegisterEntry
            {
                Amount = amount,

                OrganizationId = purchase.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                OperationTypeId = OperationTypeIdConst.IN,

                CreatedDate = DateTime.UtcNow,
                DocDate = purchase.DocDate,

                DebitAccountId = ruleLine.DebitAccountId,
                CreditAccountId = ruleLine.CreditAccountId,

                DebitQuantity = quantity,
                CreditQuantity = quantity,

                CurrencyId = purchase.CurrencyId,
                DocumentId = purchase.Id,

                Content = ruleLine.ContentTemplate,

                RegisterEntrySubkontos = BuildSubkontos(purchase, productLine)
            };
        }

        private List<RegisterEntrySubkonto> BuildSubkontos(PurchaseDoc purchase, List<PurchaseDocTable> productLine)
        {
            var list = new List<RegisterEntrySubkonto>();

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = productLine.First().ProductTable.ProductId,
                DisplayValue = productLine.First().ProductTable.Product.Name,
                SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                Side = SubkontoSideConst.DEBIT,
                CreatedDate = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = purchase.Warehouse.Id,
                DisplayValue = purchase.Warehouse.Name,
                SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                Side = SubkontoSideConst.DEBIT,
                CreatedDate = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = purchase.Id,
                DisplayValue = $"number: {purchase.DocNumber}; date: {purchase.DocDate}",
                SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                Side = SubkontoSideConst.DEBIT,
                CreatedDate = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder = list.Count + 1,
                EntityId = purchase.CounterpartyId,
                DisplayValue = purchase.Counterparty.FullName,
                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                Side = SubkontoSideConst.CREDIT,
                CreatedDate = DateTime.Now,
            });

            //list.Add(new RegisterEntrySubkonto
            //{
            //    SortOrder = list.Count + 1,
            //    EntityId = purchase.CounterpartyId,
            //    DisplayValue = purchase.Counterparty.FullName,
            //    SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
            //    Side = SubkontoSideConst.CREDIT,
            //    CreatedDate = DateTime.Now,
            //});

            return list;
        }

        private decimal GetAmount(List<PurchaseDocTable> productLine, string source)
        {
            return source switch
            {
                "amount" => productLine.Sum(s => s.Amount),
                "total_amount" => productLine.Sum(s => s.TotalAmount),
                "vat_amount" => productLine.Sum(s => s.VatAmount),
                _ => 0
            };
        }

        private decimal? GetQuantity(List<PurchaseDocTable> productLine, string? source)
        {
            if (source is null)
                return null;

            return source switch
            {
                "quantity" => productLine.Sum(s => s.Quantity),
                "received_qty" => productLine.Sum(s => s.Quantity),
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
