using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public sealed class RentalAccrualContextBuilder(
    IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    : IPostingContextBuilder<RentalAccrualDoc>
{
    public async Task<List<PostingContext>> BuildAsync(RentalAccrualDoc document)
    {
        var accountingPolicyId = await accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var contexts = new List<PostingContext>(document.Items.Count);

        foreach (var item in document.Items)
        {
            var entries = new List<PostingEntryContext>(2);
            if (item.PayableAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = item.ExpenseAccountId,
                    CreditAccountId = document.LessorPayableAccountId,
                    Amount = item.PayableAmount,
                    SourceLineId = item.Id,
                    Content = $"Rental payable: {document.Contract.LessorFullName}"
                });
            }

            if (item.TaxAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = item.ExpenseAccountId,
                    CreditAccountId = document.TaxPayableAccountId,
                    Amount = item.TaxAmount,
                    SourceLineId = item.Id,
                    Content = $"Rental personal income tax: {document.Contract.LessorFullName}"
                });
            }

            if (entries.Count == 0)
                continue;

            contexts.Add(new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.RENTAL_ACCRUAL,
                DocumentId = document.Id,
                CurrencyId = document.CurrencyId,
                DocDate = document.DocDate,
                JournalNumber = document.DocNumber,
                SourceLineId = item.Id,
                AccountingPolicyId = accountingPolicyId,
                Entries = entries
            });
        }

        return contexts;
    }
}
