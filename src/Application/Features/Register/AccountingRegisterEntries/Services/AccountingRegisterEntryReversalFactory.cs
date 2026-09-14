using Domain.Entities;

namespace Application.Features.Register.AccountingRegisterEntries;

/// <summary>
/// Builds 1C-style storno (сторно) entries for a cancelled document.
/// <para>
/// The original debit/credit correspondence is KEPT and the amount is negated. Swapping
/// the accounts instead (the previous behaviour) leaves a correct closing balance but
/// inflates the period turnover on both accounts: cancelling a 10 000 000 payroll accrual
/// used to leave Дт 2010 Кт 6710 = 10 000 000 AND Дт 6710 Кт 2010 = 10 000 000, so the
/// trial balance reported 10 000 000 of turnover on each side for an operation that never
/// happened. A negated entry on the same correspondence cancels the turnover out.
/// </para>
/// <para>
/// The storno is dated on the ORIGINAL entry's <see cref="AccountingRegisterEntry.DocDate"/>,
/// so it lands in the same accounting period as the movement it removes. Dating it "now"
/// left the original period overstated and moved the correction into an unrelated month.
/// Callers must therefore verify that the original document's period is still open before
/// reversing.
/// </para>
/// </summary>
public static class AccountingRegisterEntryReversalFactory
{
    public static List<AccountingRegisterEntry> Create(
        IEnumerable<AccountingRegisterEntry> entries,
        long reversalBatchId)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var createdDate = DateTime.Now;
        return entries.Select(entry => new AccountingRegisterEntry
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            DebitAccountId = entry.DebitAccountId,
            CreditAccountId = entry.CreditAccountId,
            CurrencyId = entry.CurrencyId,
            Amount = -entry.Amount,
            DocDate = entry.DocDate,
            CreatedDate = createdDate,
            OperationTypeId = entry.OperationTypeId,
            DebitQuantity = Negate(entry.DebitQuantity),
            CreditQuantity = Negate(entry.CreditQuantity),
            Content = $"Reversal: {entry.Content}",
            JournalNumber = entry.JournalNumber,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id,
            // The accounts no longer swap, so each subkonto keeps the side it was
            // recorded on; swapping it here would detach the analytics from its account.
            RegisterEntrySubkontos = entry.RegisterEntrySubkontos.Select(subkonto => new RegisterEntrySubkonto
            {
                Side = subkonto.Side,
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = createdDate
            }).ToList()
        }).ToList();
    }

    private static decimal? Negate(decimal? quantity) =>
        quantity.HasValue ? -quantity.Value : null;
}
