using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public static class BankOperationErrors
{
    public static Error ClassificationCategoryRequired() =>
        Error.Business("BankOperation.ClassificationCategoryRequired", "Classification category is required when a rule is specified.");

    public static Error ClassificationCategoryNotFound(short categoryId) =>
        Error.NotFound("BankOperation.ClassificationCategoryNotFound", $"Active classification category {categoryId} was not found.");

    public static Error ClassificationRuleNotFound(int ruleId) =>
        Error.NotFound("BankOperation.ClassificationRuleNotFound", $"Active classification rule {ruleId} was not found.");

    public static Error ClassificationBankAccountNotFound(int bankAccountId) =>
        Error.NotFound("BankOperation.ClassificationBankAccountNotFound", $"Active bank account {bankAccountId} was not found in the current organization.");

    public static Error ClassificationCategoryMismatch(int ruleId, short categoryId) =>
        Error.Business("BankOperation.ClassificationCategoryMismatch", $"Rule {ruleId} does not belong to category {categoryId}.");

    public static Error ClassificationBankMismatch(int ruleId, int bankId) =>
        Error.Business("BankOperation.ClassificationBankMismatch", $"Rule {ruleId} does not belong to bank {bankId}.");

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("BankOperation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bank operatsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-ÑÐ¸ {id} Ð±ÑžÐ»Ð³Ð°Ð½ Ð±Ð°Ð½Ðº Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸ÑÑÐ¸ Ñ‚Ð¾Ð¿Ð¸Ð»Ð¼Ð°Ð´Ð¸.",
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ°Ñ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ Ñ id {id} Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ð°.",
            _ => $"Bank operation with id {id} was not found."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("BankOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÑƒÑŽ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸ÑŽ Ñ id {id} Ð½ÐµÐ»ÑŒÐ·Ñ Ð¸Ð·Ð¼ÐµÐ½Ð¸Ñ‚ÑŒ Ð² ÑÑ‚Ð°Ñ‚ÑƒÑÐµ {statusId}.",
            _ => $"Bank operation with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("BankOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÑƒÑŽ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸ÑŽ Ñ id {id} Ð½ÐµÐ»ÑŒÐ·Ñ ÑƒÐ´Ð°Ð»Ð¸Ñ‚ÑŒ Ð² ÑÑ‚Ð°Ñ‚ÑƒÑÐµ {statusId}.",
            _ => $"Bank operation with id {id} cannot be deleted in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("BankOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÑƒÑŽ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸ÑŽ Ñ id {id} Ð½ÐµÐ»ÑŒÐ·Ñ Ð¿Ð¾Ð´Ñ‚Ð²ÐµÑ€Ð´Ð¸Ñ‚ÑŒ Ð² ÑÑ‚Ð°Ñ‚ÑƒÑÐµ {statusId}.",
            _ => $"Bank operation with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("BankOperation.InvalidStatus", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÑƒÑŽ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸ÑŽ Ñ id {id} Ð½ÐµÐ»ÑŒÐ·Ñ Ð¾Ñ‚Ð¼ÐµÐ½Ð¸Ñ‚ÑŒ Ð² ÑÑ‚Ð°Ñ‚ÑƒÑÐµ {statusId}.",
            _ => $"Bank operation with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error OrganizationMismatch(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.OrganizationMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ°Ñ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ {id} Ð½Ðµ Ð¿Ñ€Ð¸Ð½Ð°Ð´Ð»ÐµÐ¶Ð¸Ñ‚ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¹ Ð¾Ñ€Ð³Ð°Ð½Ð¸Ð·Ð°Ñ†Ð¸Ð¸.",
            _ => $"Bank operation {id} does not belong to current organization."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.RU => $"Ð‘Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ°Ñ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ {id} ÑƒÐ¶Ðµ Ð¾Ñ‚Ð¼ÐµÐ½ÐµÐ½Ð°.",
            _ => $"Bank operation {id} is already cancelled."
        });

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.BusinessEffectsAlreadyExist", languageId switch
        {
            LanguageIdConst.RU => $"ÐŸÐ¾ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} ÑƒÐ¶Ðµ ÑÐ¾Ð·Ð´Ð°Ð½Ñ‹ Ð¿Ñ€Ð¾Ð²Ð¾Ð´ÐºÐ¸ Ð¸Ð»Ð¸ Ñ€ÐµÐ³Ð¸ÑÑ‚Ñ€Ñ‹.",
            _ => $"Bank operation {id} already has business effects."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.RU => $"Ð”Ð»Ñ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½ posting batch.",
            _ => $"Posting batch was not found for bank operation {id}."
        });

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.MissingAccountingEntries", languageId switch
        {
            LanguageIdConst.RU => $"Ð”Ð»Ñ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ñ‹ Ð±ÑƒÑ…Ð³Ð°Ð»Ñ‚ÐµÑ€ÑÐºÐ¸Ðµ Ð¿Ñ€Ð¾Ð²Ð¾Ð´ÐºÐ¸.",
            _ => $"Accounting register entries were not found for bank operation {id}."
        });

    public static Error MissingMoneyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.MissingMoneyEntries", languageId switch
        {
            LanguageIdConst.RU => $"Ð”Ð»Ñ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ñ‹ Ð·Ð°Ð¿Ð¸ÑÐ¸ Ð´ÐµÐ½ÐµÐ¶Ð½Ð¾Ð³Ð¾ Ñ€ÐµÐ³Ð¸ÑÑ‚Ñ€Ð°.",
            _ => $"Money register entries were not found for bank operation {id}."
        });

    public static Error MissingCounterpartyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("BankOperation.MissingCounterpartyEntries", languageId switch
        {
            LanguageIdConst.RU => $"Ð”Ð»Ñ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½Ñ‹ Ð·Ð°Ð¿Ð¸ÑÐ¸ Ð¿Ð¾ ÐºÐ¾Ð½Ñ‚Ñ€Ð°Ð³ÐµÐ½Ñ‚Ñƒ.",
            _ => $"Counterparty register entries were not found for bank operation {id}."
        });

    public static Error InvalidAmount(long id, short? languageId = null) =>
        Error.Business("BankOperation.InvalidAmount", languageId switch
        {
            LanguageIdConst.RU => $"Ð£ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½ÐµÐºÐ¾Ñ€Ñ€ÐµÐºÑ‚Ð½Ð°Ñ ÑÑƒÐ¼Ð¼Ð°.",
            _ => $"Bank operation {id} has invalid amount."
        });

    public static Error InvalidDirection(short directionId, short? languageId = null) =>
        Error.Business("BankOperation.InvalidDirection", languageId switch
        {
            LanguageIdConst.RU => $"Неподдерживаемое направление банковской операции {directionId}.",
            _ => $"Unsupported bank movement direction {directionId}."
        });

    public static Error InvalidLineConfiguration(long id, short? languageId = null) =>
        Error.Business("BankOperation.InvalidLineConfiguration", languageId switch
        {
            LanguageIdConst.RU => $"Ð£ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð¹ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ð¸ {id} Ð½ÐµÐºÐ¾Ñ€Ñ€ÐµÐºÑ‚Ð½Ð°Ñ ÐºÐ¾Ð½Ñ„Ð¸Ð³ÑƒÑ€Ð°Ñ†Ð¸Ñ ÑÑ‚Ñ€Ð¾Ðº.",
            _ => $"Bank operation {id} has invalid line configuration."
        });

    public static Error InvalidOrganizationReference(string referenceName, short? languageId = null) =>
        Error.Business("BankOperation.InvalidOrganizationReference", languageId switch
        {
            LanguageIdConst.RU => $"Ð¡Ð²ÑÐ·Ð°Ð½Ð½Ñ‹Ð¹ ÑÐ¿Ñ€Ð°Ð²Ð¾Ñ‡Ð½Ð¸Ðº {referenceName} Ð½Ðµ Ð¿Ñ€Ð¸Ð½Ð°Ð´Ð»ÐµÐ¶Ð¸Ñ‚ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¹ Ð¾Ñ€Ð³Ð°Ð½Ð¸Ð·Ð°Ñ†Ð¸Ð¸ Ð¸Ð»Ð¸ Ð½ÐµÐ°ÐºÑ‚Ð¸Ð²ÐµÐ½.",
            _ => $"Reference {referenceName} does not belong to the current organization or is inactive."
        });

    public static Error InvalidCurrencyMismatch(short? languageId = null) =>
        Error.Business("BankOperation.InvalidCurrencyMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Ð’Ð°Ð»ÑŽÑ‚Ð° Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ð° Ð½Ðµ ÑÐ¾Ð¾Ñ‚Ð²ÐµÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚ Ð²Ð°Ð»ÑŽÑ‚Ðµ Ð±Ð°Ð½ÐºÐ¾Ð²ÑÐºÐ¾Ð³Ð¾ ÑÑ‡ÐµÑ‚Ð°.",
            _ => $"Document currency does not match the bank account currency."
        });
}
