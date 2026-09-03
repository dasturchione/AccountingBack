using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public static class TaxBusinessErrors
{
    public static Error OrganizationRequired(short? languageId = null) =>
        CommonErrors.UserHasNoOrganization(languageId);

    public static Error InvalidTaxType(short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.InvalidTaxType", languageId switch
        {
            LanguageIdConst.UZ => $"Soliq turi topilmadi: {taxTypeId}.",
            LanguageIdConst.UZ_CYRL => $"Солиқ тури топилмади: {taxTypeId}.",
            LanguageIdConst.RU => $"Тип налога не найден: {taxTypeId}.",
            _ => $"Tax type not found: {taxTypeId}."
        });

    public static Error InactiveTaxType(short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.InactiveTaxType", languageId switch
        {
            LanguageIdConst.UZ => $"Soliq turi faol emas: {taxTypeId}.",
            LanguageIdConst.UZ_CYRL => $"Солиқ тури фаол эмас: {taxTypeId}.",
            LanguageIdConst.RU => $"Тип налога неактивен: {taxTypeId}.",
            _ => $"Tax type is inactive: {taxTypeId}."
        });

    public static Error MissingTaxConfiguration(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.MissingConfiguration", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Ташкилот {organizationId} учун {taxTypeId} солиқ конфигурацияси топилмади.",
            LanguageIdConst.RU => $"Для организации {organizationId} не найдена настройка налога типа {taxTypeId}.",
            _ => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} was not found."
        });

    public static Error DuplicateTaxConfiguration(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Conflict("Tax.DuplicateConfiguration", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi takrorlangan.",
            LanguageIdConst.UZ_CYRL => $"Ташкилот {organizationId} учун {taxTypeId} солиқ конфигурацияси такрорланган.",
            LanguageIdConst.RU => $"Для организации {organizationId} найдена дублирующая настройка налога типа {taxTypeId}.",
            _ => $"Duplicate tax configuration found for organization {organizationId} and tax type {taxTypeId}."
        });

    public static Error DisabledTax(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.Disabled", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi faol emas.",
            LanguageIdConst.UZ_CYRL => $"Ташкилот {organizationId} учун {taxTypeId} солиқ конфигурацияси фаол эмас.",
            LanguageIdConst.RU => $"Настройка налога типа {taxTypeId} для организации {organizationId} неактивна.",
            _ => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} is inactive."
        });

    public static Error InvalidCalculationMode(short? languageId = null) =>
        Error.Business("Tax.InvalidCalculationMode", languageId switch
        {
            LanguageIdConst.UZ => "Hisoblash rejimi noto'g'ri.",
            LanguageIdConst.UZ_CYRL => "Ҳисоблаш режими нотўғри.",
            LanguageIdConst.RU => "Недопустимый режим расчёта.",
            _ => "Calculation mode is invalid."
        });

    public static Error InvalidPercentage(short? languageId = null) =>
        Error.Business("Tax.InvalidPercentage", languageId switch
        {
            LanguageIdConst.UZ => "Soliq foizi 0 dan katta va 100 dan kichik yoki teng bo'lishi kerak.",
            LanguageIdConst.UZ_CYRL => "Солиқ фоизи 0 дан катта ва 100 дан кичик ёки тенг бўлиши керак.",
            LanguageIdConst.RU => "Процент налога должен быть больше 0 и не больше 100.",
            _ => "Tax percentage must be greater than 0 and less than or equal to 100."
        });

    public static Error InvalidAmount(short? languageId = null) =>
        Error.Business("Tax.InvalidAmount", languageId switch
        {
            LanguageIdConst.UZ => "Soliq summasi noto'g'ri.",
            LanguageIdConst.UZ_CYRL => "Солиқ суммаси нотўғри.",
            LanguageIdConst.RU => "Недопустимая сумма налога.",
            _ => "Tax amount is invalid."
        });

    public static Error MissingOrganizationConfiguration(short? languageId = null) =>
        Error.Business("Tax.MissingOrganizationConfiguration", languageId switch
        {
            LanguageIdConst.UZ => "Soliq konfiguratsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => "Солиқ конфигурацияси топилмади.",
            LanguageIdConst.RU => "Настройка налога не найдена.",
            _ => "Tax configuration was not found."
        });
}
