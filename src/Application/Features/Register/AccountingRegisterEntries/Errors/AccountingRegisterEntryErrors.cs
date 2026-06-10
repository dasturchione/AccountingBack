using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public static class AccountingRegisterEntryErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("AccountingRegisterEntry.NotFound", GetNotFoundDescription(id, languageId));

    private static string GetNotFoundDescription(long id, short? languageId = null) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan buxgalteriya o'tkazmasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган бухгалтерия ўтказмаси топилмади.",
            LanguageIdConst.RU => $"Бухгалтерская проводка с id {id} не найдена.",
            _ => $"Accounting register entry with id {id} was not found."
        };

    public static Error PostingRuleNotFound(short? languageId = null) =>
        Error.Business("PostingRule.Business", GetPostingRuleNotFoundDescription(languageId));

    private static string GetPostingRuleNotFoundDescription(short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Buxgalteriya yozuvini to'ldirish qoidasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Бухгалтерия ёзувини тўлдириш қоидаси топилмади.",
            LanguageIdConst.RU => $"Правило для заполнения проводки не найдено",
            _ => $"Posting rule not found"
        };

    public static Error UnsupportedDocumentType(short? languageId = null) =>
        Error.Business("AccountingRegisterEntry.UnsupportedDocumentType", GetUnsupportedDocumentTypeDescription(languageId));

    private static string GetUnsupportedDocumentTypeDescription(short? languageId) =>
        languageId switch 
        {
            LanguageIdConst.UZ => "Hujjat turi qo'llab-quvvatlanmaydi.", 
            LanguageIdConst.UZ_CYRL => "Ҳужжат тури қўллаб-қувватланмайди.",
            LanguageIdConst.RU => "Тип документа не поддерживается.", 
            _ => "Document type is not supported."
        };
}
