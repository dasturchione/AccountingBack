using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Banks;

public static class BankErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Bank.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bank topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган банк топилмади.",
            LanguageIdConst.RU => $"Банк с id {id} не найден.",
            _ => $"Bank with id {id} was not found."
        });

    public static Error BranchNotFound(string mfo, short? languageId = null) =>
        Error.NotFound("BankBranch.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"MFOsi '{mfo}' bo'lgan bank filiali topilmadi.",
            LanguageIdConst.UZ_CYRL => $"МФОси '{mfo}' бўлган банк филиали топилмади.",
            LanguageIdConst.RU => $"Филиал банка с МФО '{mfo}' не найден.",
            _ => $"Bank branch with MFO '{mfo}' was not found."
        });
}
