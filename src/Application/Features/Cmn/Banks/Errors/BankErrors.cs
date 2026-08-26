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

    public static Error BranchNotFound(string mfo, short? languageId = null) =>
        Error.NotFound("BankBranch.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"MFOsi '{mfo}' bo'lgan bank filiali topilmadi.",
            LanguageIdConst.RU => $"Bank branch with MFO '{mfo}' was not found.",
            _ => $"Bank branch with MFO '{mfo}' was not found."
        });
}
