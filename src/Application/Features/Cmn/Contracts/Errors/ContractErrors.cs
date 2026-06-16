using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public static class ContractErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Contract.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan shartnoma topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган шартнома топилмади.",
            LanguageIdConst.RU      => $"Договор с id {id} не найден.",
            _                       => $"Contract with id {id} was not found."
        });

    public static Error NumberConflict(string number, short? languageId = null) =>
        Error.Conflict("Contract.NumberConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Raqami '{number}' bo'lgan shartnoma allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Рақами '{number}' бўлган шартнома аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Договор с номером '{number}' уже существует.",
            _                       => $"Contract with number '{number}' already exists."
        });
}
