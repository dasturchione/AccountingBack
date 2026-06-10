using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Domain.Entities;
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

            foreach (var line in purchase.Lines)
            {
                foreach (var ruleLine in rule.PostingRuleLines.OrderBy(x => x.SortOrder))
                {
                    entries.Add(BuildEntry(purchase, line, ruleLine));
                }
            }

            return Result.Success(entries);
        }

        private AccountingRegisterEntry BuildEntry(PurchaseDoc purchase, PurchaseDocTable line, PostingRuleLine ruleLine)
        {
            var amount = GetAmount(line, ruleLine.AmountSource);
            var quantity = GetQuantity(line, ruleLine.QuantitySource);

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

                Content = ruleLine.ContentTemplate
            };
        }

        private decimal GetAmount(PurchaseDocTable line, string source)
        {
            return source switch
            {
                "amount" => line.Amount,
                "total_amount" => line.TotalAmount,
                "vat_amount" => line.VatAmount,
                _ => 0
            };
        }

        private decimal? GetQuantity(PurchaseDocTable line, string? source)
        {
            if (source is null)
                return null;

            return source switch
            {
                "quantity" => line.Quantity,
                "received_qty" => line.Quantity,
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
