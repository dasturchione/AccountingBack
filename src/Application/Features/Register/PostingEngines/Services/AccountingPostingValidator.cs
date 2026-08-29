using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public class AccountingPostingValidator(IUserContext userContext) : IAccountingPostingValidator
{
    public Result Validate(IReadOnlyCollection<AccountingRegisterEntry> entries)
    {
        if (entries.Count == 0)
            return Result.Failure(AccountingPostingErrors.Empty(userContext.LanguageId));

        foreach (var entry in entries)
        {
            if (entry.OrganizationId <= 0 || entry.DocumentTypeId <= 0 || entry.DocumentId <= 0)
                return Result.Failure(AccountingPostingErrors.InvalidDocument(userContext.LanguageId));

            if (entry.CurrencyId <= 0)
                return Result.Failure(AccountingPostingErrors.InvalidCurrency(userContext.LanguageId));

            if (entry.Amount <= 0)
                return Result.Failure(AccountingPostingErrors.InvalidAmount(userContext.LanguageId));

            if (entry.DocDate == default || entry.CreatedDate == default)
                return Result.Failure(AccountingPostingErrors.InvalidDate(userContext.LanguageId));

            if (!entry.DebitAccountId.HasValue || !entry.CreditAccountId.HasValue ||
                entry.DebitAccountId.Value <= 0 || entry.CreditAccountId.Value <= 0)
                return Result.Failure(AccountingPostingErrors.MissingAccount(userContext.LanguageId));

            if (entry.DebitAccountId == entry.CreditAccountId &&
                !IsCashBoxTransferEntry(entry.RegisterEntrySubkontos))
                return Result.Failure(AccountingPostingErrors.SameAccount(userContext.LanguageId));

            if (entry.DebitQuantity.HasValue && entry.DebitQuantity.Value <= 0 ||
                entry.CreditQuantity.HasValue && entry.CreditQuantity.Value <= 0)
                return Result.Failure(AccountingPostingErrors.InvalidQuantity(userContext.LanguageId));
        }

        foreach (var entry in entries)
        {
            if (entry.DebitQuantity.HasValue &&
                entry.CreditQuantity.HasValue &&
                entry.DebitQuantity.Value != entry.CreditQuantity.Value)
            {
                return Result.Failure(AccountingPostingErrors.BalanceMismatch(userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private static bool IsCashBoxTransferEntry(ICollection<RegisterEntrySubkonto> subkontos)
    {
        var debitCashBoxIds = subkontos
            .Where(x => x.Side == SubkontoSideConst.DEBIT && x.SubkontoTypeId == SubkontoTypeIdConst.OrganizationCashDesks)
            .Select(x => x.EntityId)
            .Where(x => x.HasValue)
            .Distinct()
            .ToList();

        var creditCashBoxIds = subkontos
            .Where(x => x.Side == SubkontoSideConst.CREDIT && x.SubkontoTypeId == SubkontoTypeIdConst.OrganizationCashDesks)
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
