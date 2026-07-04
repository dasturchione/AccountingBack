using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public static class CashOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("CashOperation.NotFound", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} was not found.",
            _ => $"Cash operation with id {id} was not found."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} cannot be updated in status {statusId}.",
            _ => $"Cash operation with id {id} cannot be updated in current status {statusId}."
        });

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} cannot be deleted in status {statusId}.",
            _ => $"Cash operation with id {id} cannot be deleted in current status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} cannot be confirmed in status {statusId}.",
            _ => $"Cash operation with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} cannot be cancelled in status {statusId}.",
            _ => $"Cash operation with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.BusinessEffectsAlreadyExist", languageId switch
        {
            LanguageIdConst.RU => $"Operation {id} already has postings.",
            _ => $"Cash operation with id {id} already has business effects."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.RU => $"Operation with id {id} is already cancelled.",
            _ => $"Cash operation with id {id} is already cancelled."
        });

    public static Error InvalidAmount(long id, short? languageId = null) =>
        Error.Business("CashOperation.InvalidAmount", languageId switch
        {
            LanguageIdConst.RU => $"Operation {id} has invalid amount.",
            _ => $"Cash operation with id {id} has invalid amount."
        });

    public static Error InvalidOperationType(short operationTypeId, short? languageId = null) =>
        Error.Business("CashOperation.InvalidOperationType", languageId switch
        {
            LanguageIdConst.RU => $"Unsupported operation type: {operationTypeId}.",
            _ => $"Unsupported cash operation type: {operationTypeId}."
        });

    public static Error InvalidCashBoxMismatch(long id, short? languageId = null) =>
        Error.Business("CashOperation.InvalidCashBox", languageId switch
        {
            LanguageIdConst.RU => $"Operation {id} has invalid cash box configuration.",
            _ => $"Cash operation {id} has invalid cash box configuration."
        });

    public static Error InvalidPaymentPurpose(long id, short? languageId = null) =>
        Error.Business("CashOperation.InvalidPaymentPurpose", languageId switch
        {
            LanguageIdConst.RU => $"Operation {id} has invalid payment purpose configuration.",
            _ => $"Cash operation {id} has invalid payment purpose configuration."
        });

    public static Error OrganizationMismatch(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.OrganizationMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Document {id} does not belong to the current organization.",
            _ => $"Cash operation {id} does not belong to current organization."
        });

    public static Error InsufficientBalance(int cashBoxId, decimal amount, short? languageId = null) =>
        Error.Business("CashOperation.InsufficientBalance", languageId switch
        {
            LanguageIdConst.RU => $"Cash box {cashBoxId} has insufficient balance for amount {amount}.",
            _ => $"Cash box {cashBoxId} has insufficient balance for amount {amount}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.RU => $"Posting batch was not found for operation {id}.",
            _ => $"Posting batch was not found for cash operation {id}."
        });

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingAccountingEntries", languageId switch
        {
            LanguageIdConst.RU => $"Accounting entries were not found for operation {id}.",
            _ => $"Cash operation {id} has no accounting register entries."
        });

    public static Error MissingMoneyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingMoneyEntries", languageId switch
        {
            LanguageIdConst.RU => $"Money register entries were not found for operation {id}.",
            _ => $"Cash operation {id} has no money register entries."
        });

    public static Error MissingCounterpartyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingCounterpartyEntries", languageId switch
        {
            LanguageIdConst.RU => $"Counterparty register entries were not found for operation {id}.",
            _ => $"Cash operation {id} has no counterparty register entries."
        });
}
