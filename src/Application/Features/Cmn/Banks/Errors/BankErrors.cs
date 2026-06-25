using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Banks;

public static class BankErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Bank.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bank topilmadi.",
            LanguageIdConst.RU => $"Bank with id {id} was not found.",
            _ => $"Bank with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Bank.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi '{code}' bo'lgan bank allaqachon mavjud.",
            LanguageIdConst.RU => $"Bank with code '{code}' already exists.",
            _ => $"Bank with code '{code}' already exists."
        });
}
