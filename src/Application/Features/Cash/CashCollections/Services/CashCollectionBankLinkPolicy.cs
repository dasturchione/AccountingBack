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
        decimal amount)
    {
        if (document.OrganizationId != organizationId)
            return Result.Failure(CashCollectionErrors.OrganizationMismatch(document.Id));

        if (document.StateId != StateIdConst.ACTIVE || document.StatusId != DocumentStatusIdConst.IN_TRANSIT)
            return Result.Failure(CashCollectionErrors.NotInTransit(document.Id));

        if (document.BankAccountId != bankAccountId)
            return Result.Failure(CashCollectionErrors.BankAccountMismatch(document.Id));

        if (directionId != MovementDirectionIdConst.IN)
            return Result.Failure(CashCollectionErrors.DirectionMustBeIn());

        if (document.CurrencyId != currencyId)
            return Result.Failure(CashCollectionErrors.CurrencyMismatch(document.Id));

        if (document.Amount != amount)
            return Result.Failure(CashCollectionErrors.AmountMismatch(document.Id));

        if (!document.CashInTransitAccountId.HasValue || !document.BankChartAccountId.HasValue)
            return Result.Failure(CashCollectionErrors.MissingAccounts(document.Id));

        return Result.Success();
    }

    public static Result ValidateCancellation(CashCollectionDoc document, bool hasActiveBankOperation)
    {
        return hasActiveBankOperation
            ? Result.Failure(CashCollectionErrors.ActiveBankOperation(document.Id))
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
