using SharedKernel.Results;

namespace Application.Features.CashFiscalTransfers;

public static class CashFiscalTransferErrors
{
    public static Error NotFound(long id) => Error.NotFound("CashFiscalTransfer.NotFound", $"Cash fiscal transfer {id} was not found.");
    public static Error InvalidStatus(long id, short statusId) => Error.Conflict("CashFiscalTransfer.InvalidStatus", $"Cash fiscal transfer {id} cannot be processed in status {statusId}.");
    public static Error AlreadyCancelled(long id) => Error.Conflict("CashFiscalTransfer.AlreadyCancelled", $"Cash fiscal transfer {id} is already cancelled.");
    public static Error InvalidConfiguration(string message) => Error.Business("CashFiscalTransfer.InvalidConfiguration", message);
    public static Error InsufficientBalance(string source, decimal available, decimal required) => Error.Business("CashFiscalTransfer.InsufficientBalance", $"{source} balance {available} is less than required amount {required}.");
    public static Error BusinessEffectsAlreadyExist(long id) => Error.Conflict("CashFiscalTransfer.BusinessEffectsAlreadyExist", $"Cash fiscal transfer {id} already has business effects.");
    public static Error MissingPostingBatch(long id) => Error.Conflict("CashFiscalTransfer.MissingPostingBatch", $"Posting batch for cash fiscal transfer {id} was not found.");
    public static Error MissingAccountingEntries(long id) => Error.Conflict("CashFiscalTransfer.MissingAccountingEntries", $"Accounting entries for cash fiscal transfer {id} were not found.");
    public static Error MissingMoneyEntries(long id) => Error.Conflict("CashFiscalTransfer.MissingMoneyEntries", $"Money entries for cash fiscal transfer {id} were not found.");
}
