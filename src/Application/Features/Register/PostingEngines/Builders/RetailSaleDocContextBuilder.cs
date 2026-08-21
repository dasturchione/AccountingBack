using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public sealed class RetailSaleDocContextBuilder : IPostingContextBuilder<RetailSaleDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public RetailSaleDocContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(RetailSaleDoc document)
    {
        var policyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var contexts = new List<PostingContext>();

        foreach (var line in document.RetailSaleDocProducts)
        {
            var entries = new List<PostingEntryContext>();
            if (line.Amount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = document.ReceivableAccountId,
                    CreditAccountId = line.IncomeAccountId,
                    Amount = line.Amount,
                    Content = "Retail sale"
                });
            }

            if (line.VatAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = document.ReceivableAccountId,
                    CreditAccountId = document.VatAccountId,
                    Amount = line.VatAmount,
                    Content = "Retail sale VAT"
                });
            }

            if (!line.Product.IsService)
            {
                var costAmount = line.CostPrice * line.Quantity;
                if (costAmount != 0m)
                {
                    entries.Add(new PostingEntryContext
                    {
                        DebitAccountId = line.CostAccountId,
                        CreditAccountId = line.InventoryAccountId,
                        Amount = costAmount,
                        CreditQuantity = line.Quantity,
                        Content = "Retail sale cost"
                    });
                }
            }

            AddContext(contexts, document, policyId, line.Id, entries);
        }

        foreach (var payment in document.RetailSaleDocPayments)
        {
            AddContext(contexts, document, policyId, payment.Id,
            [
                new PostingEntryContext
                {
                    DebitAccountId = payment.DebitAccountId,
                    CreditAccountId = document.ReceivableAccountId,
                    Amount = payment.Amount,
                    Content = "Retail sale payment"
                }
            ]);
        }

        return contexts;
    }

    private static void AddContext(
        ICollection<PostingContext> contexts,
        RetailSaleDoc document,
        short policyId,
        long sourceLineId,
        List<PostingEntryContext> entries)
    {
        if (entries.Count == 0)
            return;

        contexts.Add(new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.RETAIL_SALE,
            AccountingPolicyId = policyId,
            DocumentId = document.Id,
            DocDate = document.DocDate,
            CurrencyId = document.CurrencyId,
            JournalNumber = document.DocNumber,
            SourceLineId = sourceLineId,
            Entries = entries
        });
    }
}
