using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public static class AccountingRegisterEntryErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("AccountingRegisterEntry.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan buxgalteriya o'tkazmasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган бухгалтерия ўтказмаси топилмади.",
            LanguageIdConst.RU      => $"Бухгалтерская проводка с id {id} не найдена.",
            _                       => $"Accounting register entry with id {id} was not found."
        });
}
