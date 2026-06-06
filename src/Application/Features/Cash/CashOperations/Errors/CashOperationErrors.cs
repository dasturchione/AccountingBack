using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public static class CashOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("CashOperation.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan kassa operatsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган касса операцияси топилмади.",
            LanguageIdConst.RU      => $"Кассовая операция с id {id} не найдена.",
            _                       => $"Cash operation with id {id} was not found."
        });
}
