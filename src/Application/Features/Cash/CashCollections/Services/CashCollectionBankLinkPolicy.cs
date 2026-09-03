using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public static class CashCollectionBankLinkPolicy
{
    public static Result Validate(
        CashCollectionDoc document,
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        short? languageId = null)
    {
        if (document.OrganizationId != organizationId)
            return Result.Failure(CashCollectionErrors.OrganizationMismatch(document.Id, languageId));

        if (document.StateId != StateIdConst.ACTIVE || document.StatusId != DocumentStatusIdConst.IN_TRANSIT)
            return Result.Failure(CashCollectionErrors.NotInTransit(document.Id, languageId));

        if (document.BankAccountId != bankAccountId)
            return Result.Failure(CashCollectionErrors.BankAccountMismatch(document.Id, languageId));

        if (directionId != MovementDirectionIdConst.IN)
            return Result.Failure(CashCollectionErrors.DirectionMustBeIn(languageId));

        if (document.CurrencyId != currencyId)
            return Result.Failure(CashCollectionErrors.CurrencyMismatch(document.Id, languageId));

        if (document.Amount != amount)
            return Result.Failure(CashCollectionErrors.AmountMismatch(document.Id, languageId));

        if (!document.CashInTransitAccountId.HasValue || !document.BankChartAccountId.HasValue)
            return Result.Failure(CashCollectionErrors.MissingAccounts(document.Id, languageId));

        return Result.Success();
    }

    public static Result ValidateCancellation(CashCollectionDoc document, bool hasActiveBankOperation, short? languageId = null)
    {
        return hasActiveBankOperation
            ? Result.Failure(CashCollectionErrors.ActiveBankOperation(document.Id, languageId))
            : Result.Success();
    }

    public static void Apply(
        BankOperation operation,
        DocumentRegistry registry,
        CashCollectionDoc document,
        short cashCollectionCategoryId)
    {
        operation.RelatedDocumentId = registry.Id;
        operation.BankChartAccountId = document.BankChartAccountId;
        operation.OffsetAccountId = document.CashInTransitAccountId;
        operation.PaymentTypeId = PaymentTypeIdConst.BANK;
        operation.ClassificationCategoryId = cashCollectionCategoryId;
        operation.CounterpartyId = null;
        operation.CounterpartyBankAccountId = null;
        operation.ContractId = null;
    }
}
