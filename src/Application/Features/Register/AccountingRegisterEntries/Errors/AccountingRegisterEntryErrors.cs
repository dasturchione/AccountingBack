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
    public static Error GroupAccountNotPostable(IReadOnlyCollection<int> accountIds, short? languageId = null) =>
        Error.Business("AccountingPosting.GroupAccountNotPostable", GetGroupAccountNotPostableDescription(accountIds, languageId));

    private static string GetGroupAccountNotPostableDescription(IReadOnlyCollection<int> accountIds, short? languageId)
    {
        var ids = string.Join(", ", accountIds);
        return languageId switch
        {
            LanguageIdConst.UZ => $"Guruh (jamlovchi) hisobga to'g'ridan-to'g'ri o'tkazma yozib bo'lmaydi: {ids}.",
            LanguageIdConst.UZ_CYRL => $"Гуруҳ (жамловчи) ҳисобга тўғридан-тўғри ўтказма ёзиб бўлмайди: {ids}.",
            LanguageIdConst.RU => $"Нельзя проводить проводку напрямую на групповой счёт: {ids}.",
            _ => $"Cannot post directly to a group (header) account: {ids}."
        };
    }

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
