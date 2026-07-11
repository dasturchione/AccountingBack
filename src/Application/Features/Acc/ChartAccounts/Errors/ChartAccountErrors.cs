using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public static class ChartAccountErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("ChartAccount.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan hisoblar rejasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоблар режаси топилмади.",
            LanguageIdConst.RU      => $"План счетов с id {id} не найден.",
            _                       => $"Chart of accounts with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("ChartAccount.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan hisoblar rejasi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган ҳисоблар режаси аллақачон мавжуд.",
            LanguageIdConst.RU      => $"План счетов с кодом '{code}' уже существует.",
            _                       => $"Chart of accounts with code '{code}' already exists."
        });

    public static Error NumberConflict(string number, short? languageId = null) =>
        Error.Conflict("ChartAccount.NumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi '{number}' bo'lgan hisoblar rejasi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{number}' бўлган ҳисоблар режаси аллақачон мавжуд.",
            LanguageIdConst.RU => $"План счетов с кодом '{number}' уже существует.",
            _ => $"Chart of accounts with code '{number}' already exists."
        });
}
