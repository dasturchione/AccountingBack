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
    public class SaleAccountingHandler : IAccountingDocumentHandler<SaleDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<PostingRule> _postingRuleQuery;

        public SaleAccountingHandler(IUserContext userContext,
                                     IQueryBuilder queryBuilder,
                                     IQueryRepository<PostingRule> postingRuleQuery)
        {
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _postingRuleQuery = postingRuleQuery;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
        {
            var rule = await GetRuleAsync(ct);
            if (rule is null)
                return Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.PostingRuleNotFound(_userContext.LanguageId));

            var entries = new List<AccountingRegisterEntry>();

            var groupedByProduct = sale.SaleDocTables.GroupBy(g => g.ProductTable.ProductId);

            foreach (var productLine in groupedByProduct)
            {
                foreach (var ruleLine in rule.PostingRuleLines.OrderBy(x => x.SortOrder))
                {
                    entries.Add(BuildEntry(sale, productLine.ToList(), ruleLine));
                }
            }

            return Result.Success(entries);
        }

        private AccountingRegisterEntry BuildEntry(SaleDoc sale, List<SaleDocTable> productLine, PostingRuleLine ruleLine)
        {
            var amount = GetAmount(productLine, ruleLine.AmountSource);
            var quantity = GetQuantity(productLine, ruleLine.QuantitySource);

            return new AccountingRegisterEntry
            {
                Amount = amount,

                OrganizationId  = sale.OrganizationId,
                DocumentTypeId  = DocumentTypeIdConst.SALE,
                OperationTypeId = OperationTypeIdConst.OUT,

                CreatedDate = DateTime.UtcNow,
                DocDate     = sale.DocDate,

                DebitAccountId  = ruleLine.DebitAccountId,
                CreditAccountId = ruleLine.CreditAccountId,

                DebitQuantity  = quantity,
                CreditQuantity = quantity,

                CurrencyId = sale.CurrencyId,
                DocumentId = sale.Id,

                Content = ruleLine.ContentTemplate,

                RegisterEntrySubkontos = BuildSubkontos(sale, productLine)
            };
        }

        private List<RegisterEntrySubkonto> BuildSubkontos(SaleDoc sale, List<SaleDocTable> productLine)
        {
            var list = new List<RegisterEntrySubkonto>();

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder      = list.Count + 1,
                EntityId       = productLine.First().ProductTable.ProductId,
                DisplayValue   = productLine.First().ProductTable.Product.Name,
                SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                Side           = SubkontoSideConst.CREDIT,
                CreatedDate    = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder      = list.Count + 1,
                EntityId       = sale.Warehouse.Id,
                DisplayValue   = sale.Warehouse.Name,
                SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                Side           = SubkontoSideConst.CREDIT,
                CreatedDate    = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder      = list.Count + 1,
                EntityId       = sale.Id,
                DisplayValue   = $"number: {sale.DocNumber}; date: {sale.DocDate}",
                SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                Side           = SubkontoSideConst.CREDIT,
                CreatedDate    = DateTime.Now,
            });

            list.Add(new RegisterEntrySubkonto
            {
                SortOrder      = list.Count + 1,
                EntityId       = sale.CounterpartyId,
                DisplayValue   = sale.Counterparty.FullName,
                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                Side           = SubkontoSideConst.DEBIT,
                CreatedDate    = DateTime.Now,
            });

            return list;
        }

        private decimal GetAmount(List<SaleDocTable> productLine, string source)
        {
            return source switch
            {
                "amount"       => productLine.Sum(s => s.Amount),
                "total_amount" => productLine.Sum(s => s.TotalAmount),
                "vat_amount"   => productLine.Sum(s => s.VatAmount),
                _              => 0
            };
        }

        private decimal? GetQuantity(List<SaleDocTable> productLine, string? source)
        {
            if (source is null)
                return null;

            return source switch
            {
                "quantity" => productLine.Sum(s => s.Quantity),
                _          => null
            };
        }

        private async Task<PostingRule?> GetRuleAsync(CancellationToken ct = default)
        {
            var query = _queryBuilder
                .For<PostingRule>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE)
                .Build();

            query.AddIncludes(e => e.Include(i => i.PostingRuleLines));

            return await _postingRuleQuery.GetAsync(query, ct);
        }
    }
}
