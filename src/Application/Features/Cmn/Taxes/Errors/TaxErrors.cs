using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public static class TaxErrors
{
    public static Error ProviderNotFound(string providerCode, short? languageId = null) =>
        Error.NotFound("Tax.ProviderNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"'{providerCode}' soliq provayderi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"'{providerCode}' солиқ провайдери топилмади.",
            LanguageIdConst.RU => $"Налоговый провайдер '{providerCode}' не найден.",
            _ => $"{providerCode} provider was not found."
        });

    public static Error NotFound(short id, short? languageId = null) =>
        Error.NotFound("Tax.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan soliq topilmadi.",
            LanguageIdConst.RU => $"Tax with id {id} was not found.",
            _ => $"Tax with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Tax.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi '{code}' bo'lgan soliq allaqachon mavjud.",
            LanguageIdConst.RU => $"Tax with code '{code}' already exists.",
            _ => $"Tax with code '{code}' already exists."
        });

    public static Error NameConflict(string name, short? languageId = null) =>
        Error.Conflict("Tax.NameConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Nomi '{name}' bo'lgan soliq allaqachon mavjud.",
            LanguageIdConst.RU => $"Tax with name '{name}' already exists.",
            _ => $"Tax with name '{name}' already exists."
        });

    public static Error InvalidRate(short? languageId = null) =>
        Error.Business("Tax.InvalidRate", languageId switch
        {
            LanguageIdConst.UZ => "Soliq foizi 0 dan katta va 100 dan kichik yoki teng bo'lishi kerak.",
            LanguageIdConst.RU => "Tax rate must be greater than 0 and less than or equal to 100.",
            _ => "Tax rate must be greater than 0 and less than or equal to 100."
        });

    public static Error InvalidState(short? languageId = null) =>
        Error.Business("Tax.InvalidState", languageId switch
        {
            LanguageIdConst.UZ => "Soliq holati noto'g'ri.",
            LanguageIdConst.RU => "Tax state is invalid.",
            _ => "Tax state is invalid."
        });

    public static Error OrganizationRequired(short? languageId = null) =>
        CommonErrors.UserHasNoOrganization(languageId);
}
