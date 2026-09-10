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

    public static Error AccountNotPostable(IReadOnlyCollection<int> accountIds, short? languageId = null) =>
        Error.Business("AccountingPosting.AccountNotPostable", GetAccountNotPostableDescription(accountIds, languageId));

    private static string GetAccountNotPostableDescription(IReadOnlyCollection<int> accountIds, short? languageId)
    {
        var ids = string.Join(", ", accountIds);
        return languageId switch
        {
            LanguageIdConst.UZ => $"Faol va quyi (leaf) hisobvaraqlargagina o'tkazma yozish mumkin: {ids}.",
            LanguageIdConst.UZ_CYRL => $"Фаол ва қуйи (leaf) ҳисобварақларгина ўтказма ёзиш мумкин: {ids}.",
            LanguageIdConst.RU => $"Проводки разрешены только по активным конечным счетам: {ids}.",
            _ => $"Only active leaf accounts can receive postings: {ids}."
        };
    }

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

    public static Error UnsupportedRebuildDocumentType(short documentTypeId, short? languageId = null) =>
        Error.Business("AccountingRegisterEntry.UnsupportedRebuildDocumentType", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentTypeId {documentTypeId} uchun buxgalteriya o'tkazmalarini qayta tuzish qo'llab-quvvatlanmaydi.",
            LanguageIdConst.UZ_CYRL => $"DocumentTypeId {documentTypeId} учун бухгалтерия ўтказмаларини қайта тузиш қўллаб-қувватланмайди.",
            LanguageIdConst.RU => $"Пересборка бухгалтерских проводок для DocumentTypeId {documentTypeId} не поддерживается.",
            _ => $"Accounting entry rebuild is not supported for DocumentTypeId {documentTypeId}."
        });

    public static Error PostedDocumentNotFound(short documentTypeId, long documentId, short? languageId = null) =>
        Error.NotFound("AccountingRegisterEntry.PostedDocumentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} bo'yicha o'tkazilgan hujjat topilmadi.",
            LanguageIdConst.UZ_CYRL => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} бўйича ўтказилган ҳужжат топилмади.",
            LanguageIdConst.RU => $"Проведённый документ с DocumentTypeId {documentTypeId} и DocumentId {documentId} не найден.",
            _ => $"Posted document with DocumentTypeId {documentTypeId} and DocumentId {documentId} was not found."
        });

    public static Error AlreadyRebuilding(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Conflict("AccountingRegisterEntry.AlreadyRebuilding", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} uchun o'tkazmalar hozir qayta tuzilmoqda.",
            LanguageIdConst.UZ_CYRL => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} учун ўтказмалар ҳозир қайта тузилмоқда.",
            LanguageIdConst.RU => $"Проводки для DocumentTypeId {documentTypeId}, DocumentId {documentId} уже пересобираются.",
            _ => $"Entries for DocumentTypeId {documentTypeId}, DocumentId {documentId} are already being rebuilt."
        });

    public static Error RebuildProducedNoEntries(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Business("AccountingRegisterEntry.RebuildProducedNoEntries", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} uchun yangi buxgalteriya o'tkazmalari yaratilmadi.",
            LanguageIdConst.UZ_CYRL => $"DocumentTypeId {documentTypeId}, DocumentId {documentId} учун янги бухгалтерия ўтказмалари яратилмади.",
            LanguageIdConst.RU => $"Для DocumentTypeId {documentTypeId}, DocumentId {documentId} не создано ни одной новой проводки.",
            _ => $"No accounting entries were produced for DocumentTypeId {documentTypeId}, DocumentId {documentId}."
        });
}
