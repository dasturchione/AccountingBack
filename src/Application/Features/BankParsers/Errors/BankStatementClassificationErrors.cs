using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankParsers;

public static class BankStatementClassificationErrors
{
    public static Error OrganizationRequired(short? languageId = null) =>
        Error.Business("BankStatement.OrganizationRequired", Message(languageId,
            "Tasniflash uchun joriy tashkilot kerak.", "Таснифлаш учун жорий ташкилот керак.",
            "Для классификации необходима текущая организация.", "Current organization is required for classification."));

    public static Error OrganizationNotFound(short? languageId = null) =>
        Error.NotFound("BankStatement.OrganizationNotFound", Message(languageId,
            "Joriy tashkilot topilmadi.", "Жорий ташкилот топилмади.",
            "Текущая организация не найдена.", "Current organization was not found."));

    public static Error OrganizationMismatch(short? languageId = null) =>
        Error.Business("BankStatement.OrganizationMismatch", Message(languageId,
            "Ko'chirmadagi STIR joriy tashkilot STIRiga mos emas.", "Кўчирмадаги СТИР жорий ташкилот СТИРига мос эмас.",
            "ИНН в выписке не совпадает с ИНН текущей организации.", "The statement taxpayer number does not match the current organization."));

    public static Error RuleSetNotFound(int bankId, short? languageId = null) =>
        Error.Problem("BankStatement.ClassificationRuleSetNotFound", Message(languageId,
            $"{bankId} banki uchun faol tasniflash qoidalari to'plami topilmadi.",
            $"{bankId} банки учун фаол таснифлаш қоидалари тўплами топилмади.",
            $"Для банка {bankId} не найден активный набор правил классификации операций.",
            $"No active operation classification rule set was found for bank {bankId}."));

    public static Error ReviewCategoryNotFound(short? languageId = null) =>
        Error.Problem("BankStatement.ReviewCategoryNotFound", Message(languageId,
            "Faol REVIEW_REQUIRED bank operatsiyasi toifasi topilmadi.", "Фаол REVIEW_REQUIRED банк операцияси тоифаси топилмади.",
            "Активная категория банковских операций REVIEW_REQUIRED не найдена.", "Active REVIEW_REQUIRED bank operation category was not found."));

    public static Error InvalidConfiguration(short? languageId = null) =>
        Error.Problem("BankStatement.ClassificationConfigurationInvalid", Message(languageId,
            "Bank operatsiyalarini tasniflash sozlamasi noto'g'ri.", "Банк операцияларини таснифлаш созламаси нотўғри.",
            "Конфигурация классификации банковских операций некорректна.", "Bank operation classification configuration is invalid."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
