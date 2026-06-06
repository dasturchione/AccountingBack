using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public static class BankOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("BankOperation.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan bank operatsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган банк операцияси топилмади.",
            LanguageIdConst.RU      => $"Банковская операция с id {id} не найдена.",
            _                       => $"Bank operation with id {id} was not found."
        });
}
