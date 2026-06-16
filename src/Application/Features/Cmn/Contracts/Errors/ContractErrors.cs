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
}
