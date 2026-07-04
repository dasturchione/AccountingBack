using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.Currencies;

public static class CurrencyErrors
{
    public static Error NotFound(short id, short? languageId = null) =>
        Error.NotFound("Currency.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.RU => $"Currency with id {id} was not found.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Currency.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi '{code}' bo'lgan valyuta allaqachon mavjud.",
            LanguageIdConst.RU => $"Currency with code '{code}' already exists.",
            _ => $"Currency with code '{code}' already exists."
        });
}
