using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Banks;

public static class BankErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Bank.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bank topilmadi.",
            LanguageIdConst.RU => $"Банк с id {id} не найден.",
            _ => $"Bank with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Bank.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi {code} bo'lgan bank allaqachon mavjud.",
            LanguageIdConst.RU => $"Банк с кодом {code} уже существует.",
            _ => $"Bank with code {code} already exists."
        });
}
