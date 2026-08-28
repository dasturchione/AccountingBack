using SharedKernel.Results;

namespace Application.Features.CashCollections;

public static class CashCollectionErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("CashCollection.NotFound", $"Cash collection document {id} was not found.");

    public static Error InvalidStatus(long id, short statusId) =>
        Error.Conflict("CashCollection.InvalidStatus", $"Cash collection document {id} cannot be processed in status {statusId}.");

    public static Error AlreadyCancelled(long id) =>
        Error.Conflict("CashCollection.AlreadyCancelled", $"Cash collection document {id} is already cancelled.");

    public static Error OrganizationMismatch(long id) =>
        Error.Business("CashCollection.OrganizationMismatch", $"Cash collection document {id} belongs to another organization.");

    public static Error NotInTransit(long id) =>
        Error.Business("CashCollection.NotInTransit", $"Cash collection document {id} is not in transit.");

    public static Error BankAccountMismatch(long id) =>
        Error.Business("CashCollection.BankAccountMismatch", $"Bank account does not match cash collection document {id}.");

    public static Error DirectionMustBeIn() =>
        Error.Business("CashCollection.DirectionMustBeIn", "A bank operation linked to cash collection must be incoming.");

    public static Error CurrencyMismatch(long id) =>
        Error.Business("CashCollection.CurrencyMismatch", $"Currency does not match cash collection document {id}.");

    public static Error AmountMismatch(long id) =>
        Error.Business("CashCollection.AmountMismatch", $"Amount does not match cash collection document {id}.");

    public static Error MissingAccounts(long id) =>
        Error.Business("CashCollection.MissingAccounts", $"Cash collection document {id} has incomplete accounting accounts.");

    public static Error AlreadyLinked(long id) =>
        Error.Conflict("CashCollection.AlreadyLinked", $"Cash collection document {id} is already linked to an active bank operation.");

    public static Error ActiveBankOperation(long id) =>
        Error.Conflict("CashCollection.ActiveBankOperation", $"Remove or cancel the active bank operation linked to cash collection document {id} first.");

    public static Error CategoryNotFound() =>
        Error.NotFound("CashCollection.CategoryNotFound", "Active CASH_COLLECTION bank operation category was not found.");

    public static Error CategoryMismatch(short categoryId) =>
        Error.Business("CashCollection.CategoryMismatch", $"Classification category {categoryId} is not CASH_COLLECTION.");

    public static Error InvalidConfiguration(string message) =>
        Error.Business("CashCollection.InvalidConfiguration", message);

    public static Error InsufficientBalance(decimal available, decimal required) =>
        Error.Business("CashCollection.InsufficientBalance", $"Cash box balance {available} is less than required amount {required}.");

    public static Error BusinessEffectsAlreadyExist(long id) =>
        Error.Conflict("CashCollection.BusinessEffectsAlreadyExist", $"Cash collection document {id} already has business effects.");

    public static Error MissingPostingBatch(long id) =>
        Error.Conflict("CashCollection.MissingPostingBatch", $"Posting batch for cash collection document {id} was not found.");

    public static Error MissingAccountingEntries(long id) =>
        Error.Conflict("CashCollection.MissingAccountingEntries", $"Accounting entries for cash collection document {id} were not found.");

    public static Error MissingMoneyEntries(long id) =>
        Error.Conflict("CashCollection.MissingMoneyEntries", $"Money entries for cash collection document {id} were not found.");

    public static Error CompletedBankOperationMustBeCancelled(long id) =>
        Error.Conflict("CashCollection.CancelBankOperationFirst", $"Cancel the posted bank operation linked to cash collection document {id} first.");
}
