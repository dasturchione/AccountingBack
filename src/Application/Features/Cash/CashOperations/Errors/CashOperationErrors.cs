using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public static class CashOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("CashOperation.NotFound", languageId switch
        {
            LanguageIdConst.RU => $"Операция с id {id} не найдена.",
            _ => $"Cash operation with id {id} was not found."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Операцию с id {id} нельзя редактировать в текущем статусе {statusId}.",
            _ => $"Cash operation with id {id} cannot be updated in current status {statusId}."
        });

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Операцию с id {id} нельзя удалить в текущем статусе {statusId}.",
            _ => $"Cash operation with id {id} cannot be deleted in current status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Операцию с id {id} нельзя подтвердить в текущем статусе {statusId}.",
            _ => $"Cash operation with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("CashOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Операцию с id {id} нельзя отменить в текущем статусе {statusId}.",
            _ => $"Cash operation with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.BusinessEffectsAlreadyExist", languageId switch
        {
            LanguageIdConst.RU => $"По операции {id} уже существуют проводки.",
            _ => $"Cash operation with id {id} already has business effects."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.RU => $"Операция с id {id} уже отменена.",
            _ => $"Cash operation with id {id} is already cancelled."
        });

    public static Error InvalidAmount(long id, short? languageId = null) =>
        Error.Business("CashOperation.InvalidAmount", languageId switch
        {
            LanguageIdConst.RU => $"Сумма по операции {id} неверна.",
            _ => $"Cash operation with id {id} has invalid amount."
        });

    public static Error InvalidOperationType(short operationTypeId, short? languageId = null) =>
        Error.Business("CashOperation.InvalidOperationType", languageId switch
        {
            LanguageIdConst.RU => $"Неверный тип операции: {operationTypeId}.",
            _ => $"Unsupported cash operation type: {operationTypeId}."
        });

    public static Error InvalidCashBoxMismatch(long id, short? languageId = null) =>
        Error.Business("CashOperation.InvalidCashBox", languageId switch
        {
            LanguageIdConst.RU => $"Для операции {id} некорректные кассовые ячейки.",
            _ => $"Cash operation {id} has invalid cash box configuration."
        });

    public static Error OrganizationMismatch(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.OrganizationMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Документ {id} не принадлежит текущей организации.",
            _ => $"Cash operation {id} does not belong to current organization."
        });

    public static Error InsufficientBalance(int cashBoxId, decimal amount, short? languageId = null) =>
        Error.Business("CashOperation.InsufficientBalance", languageId switch
        {
            LanguageIdConst.RU => $"На кассе {cashBoxId} недостаточно средств для списания {amount}.",
            _ => $"Cash box {cashBoxId} has insufficient balance for amount {amount}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.RU => $"Для операции {id} не найден posting batch.",
            _ => $"Posting batch was not found for cash operation {id}."
        });

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingAccountingEntries", languageId switch
        {
            LanguageIdConst.RU => $"Не найдены проводки для операции {id}.",
            _ => $"Cash operation {id} has no accounting register entries."
        });

    public static Error MissingMoneyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("CashOperation.MissingMoneyEntries", languageId switch
        {
            LanguageIdConst.RU => $"Не найдены записи кассового регистра для операции {id}.",
            _ => $"Cash operation {id} has no money register entries."
        });
}
