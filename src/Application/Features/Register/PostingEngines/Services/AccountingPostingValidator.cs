using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public class AccountingPostingValidator : IAccountingPostingValidator
{
    public Result Validate(IReadOnlyCollection<AccountingRegisterEntry> entries)
    {
        if (entries.Count == 0)
            return Result.Failure(Error.Business("AccountingPosting.Empty", "Posting template produced no accounting entries."));

        foreach (var entry in entries)
        {
            if (entry.OrganizationId <= 0 || entry.DocumentTypeId <= 0 || entry.DocumentId <= 0)
                return Result.Failure(Error.Business("AccountingPosting.InvalidDocument", "Accounting entry must be linked to a valid document."));

            if (entry.CurrencyId <= 0)
                return Result.Failure(Error.Business("AccountingPosting.InvalidCurrency", "Accounting entry must contain a valid currency."));

            if (entry.Amount <= 0)
                return Result.Failure(Error.Business("AccountingPosting.InvalidAmount", "Accounting entry amount must be greater than zero."));

            if (entry.DocDate == default || entry.CreatedDate == default)
                return Result.Failure(Error.Business("AccountingPosting.InvalidDate", "Accounting entry must contain valid document and created dates."));

            if (!entry.DebitAccountId.HasValue || !entry.CreditAccountId.HasValue ||
                entry.DebitAccountId.Value <= 0 || entry.CreditAccountId.Value <= 0)
                return Result.Failure(Error.Business("AccountingPosting.MissingAccount", "Accounting entry must contain both debit and credit accounts."));

            if (entry.DebitAccountId == entry.CreditAccountId &&
                !IsCashBoxTransferEntry(entry.RegisterEntrySubkontos))
                return Result.Failure(Error.Business("AccountingPosting.SameAccount", "Debit and credit accounts must be different."));

            if (entry.DebitQuantity.HasValue && entry.DebitQuantity.Value <= 0 ||
                entry.CreditQuantity.HasValue && entry.CreditQuantity.Value <= 0)
                return Result.Failure(Error.Business("AccountingPosting.InvalidQuantity", "Accounting entry quantity must be greater than zero when provided."));
        }

        foreach (var entry in entries)
        {
            if (entry.DebitQuantity.HasValue &&
                entry.CreditQuantity.HasValue &&
                entry.DebitQuantity.Value != entry.CreditQuantity.Value)
            {
                return Result.Failure(Error.Business("AccountingPosting.BalanceMismatch", "Accounting posting is not balanced: total debit does not match total credit."));
            }
        }

        return Result.Success();
    }

    private static bool IsCashBoxTransferEntry(ICollection<RegisterEntrySubkonto> subkontos)
    {
        var debitCashBoxIds = subkontos
            .Where(x => x.Side == SubkontoSideConst.DEBIT && x.SubkontoTypeId == SubkontoTypeIdConst.CASH_BOX)
            .Select(x => x.EntityId)
            .Where(x => x.HasValue)
            .Distinct()
            .ToList();

        var creditCashBoxIds = subkontos
            .Where(x => x.Side == SubkontoSideConst.CREDIT && x.SubkontoTypeId == SubkontoTypeIdConst.CASH_BOX)
            .Select(x => x.EntityId)
            .Where(x => x.HasValue)
            .Distinct()
            .ToList();

        if (!debitCashBoxIds.Any() || !creditCashBoxIds.Any())
            return false;

        return debitCashBoxIds.Count == 1 && creditCashBoxIds.Count == 1 &&
               debitCashBoxIds.Single() != creditCashBoxIds.Single();
    }
}
