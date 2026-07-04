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
            LanguageIdConst.RU => $"Tax type not found: {taxTypeId}.",
            _ => $"Tax type not found: {taxTypeId}."
        });

    public static Error InactiveTaxType(short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.InactiveTaxType", languageId switch
        {
            LanguageIdConst.UZ => $"Soliq turi faol emas: {taxTypeId}.",
            LanguageIdConst.RU => $"Tax type is inactive: {taxTypeId}.",
            _ => $"Tax type is inactive: {taxTypeId}."
        });

    public static Error MissingTaxConfiguration(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.MissingConfiguration", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi topilmadi.",
            LanguageIdConst.RU => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} was not found.",
            _ => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} was not found."
        });

    public static Error DuplicateTaxConfiguration(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Conflict("Tax.DuplicateConfiguration", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi takrorlangan.",
            LanguageIdConst.RU => $"Duplicate tax configuration found for organization {organizationId} and tax type {taxTypeId}.",
            _ => $"Duplicate tax configuration found for organization {organizationId} and tax type {taxTypeId}."
        });

    public static Error DisabledTax(int organizationId, short taxTypeId, short? languageId = null) =>
        Error.Business("Tax.Disabled", languageId switch
        {
            LanguageIdConst.UZ => $"Tashkilot {organizationId} uchun {taxTypeId} soliq konfiguratsiyasi faol emas.",
            LanguageIdConst.RU => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} is inactive.",
            _ => $"Tax configuration for organization {organizationId} and tax type {taxTypeId} is inactive."
        });

    public static Error InvalidCalculationMode(short? languageId = null) =>
        Error.Business("Tax.InvalidCalculationMode", languageId switch
        {
            LanguageIdConst.UZ => "Hisoblash rejimi noto'g'ri.",
            LanguageIdConst.RU => "Calculation mode is invalid.",
            _ => "Calculation mode is invalid."
        });

    public static Error InvalidPercentage(short? languageId = null) =>
        Error.Business("Tax.InvalidPercentage", languageId switch
        {
            LanguageIdConst.UZ => "Soliq foizi 0 dan katta va 100 dan kichik yoki teng bo'lishi kerak.",
            LanguageIdConst.RU => "Tax percentage must be greater than 0 and less than or equal to 100.",
            _ => "Tax percentage must be greater than 0 and less than or equal to 100."
        });

    public static Error InvalidAmount(short? languageId = null) =>
        Error.Business("Tax.InvalidAmount", languageId switch
        {
            LanguageIdConst.UZ => "Soliq summasi noto'g'ri.",
            LanguageIdConst.RU => "Tax amount is invalid.",
            _ => "Tax amount is invalid."
        });

    public static Error MissingOrganizationConfiguration(short? languageId = null) =>
        Error.Business("Tax.MissingOrganizationConfiguration", languageId switch
        {
            LanguageIdConst.UZ => "Soliq konfiguratsiyasi topilmadi.",
            LanguageIdConst.RU => "Tax configuration was not found.",
            _ => "Tax configuration was not found."
        });
}
