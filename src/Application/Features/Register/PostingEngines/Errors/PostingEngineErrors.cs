using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public static class PostingEngineErrors
{
    public static Error EmptyEntries(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Business("PostingEngine.EmptyEntries", Message(languageId,
            $"{documentTypeId}/{documentId} hujjati uchun o'tkazma satrlari yaratilmagan.",
            $"{documentTypeId}/{documentId} ҳужжати учун ўтказма сатрлари яратилмаган.",
            $"Для документа {documentTypeId}/{documentId} не созданы строки проводок.",
            $"No posting entries were created for document {documentTypeId}/{documentId}."));

    public static Error DebitAccountMissing(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Business("PostingEngine.DebitAccountMissing", Message(languageId,
            $"{documentTypeId}/{documentId} hujjati uchun debet hisobvarag'i ko'rsatilmagan.",
            $"{documentTypeId}/{documentId} ҳужжати учун дебет ҳисобварағи кўрсатилмаган.",
            $"Для документа {documentTypeId}/{documentId} не указан дебетовый счёт.",
            $"Debit account is missing for document {documentTypeId}/{documentId}."));

    public static Error CreditAccountMissing(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Business("PostingEngine.CreditAccountMissing", Message(languageId,
            $"{documentTypeId}/{documentId} hujjati uchun kredit hisobvarag'i ko'rsatilmagan.",
            $"{documentTypeId}/{documentId} ҳужжати учун кредит ҳисобварағи кўрсатилмаган.",
            $"Для документа {documentTypeId}/{documentId} не указан кредитовый счёт.",
            $"Credit account is missing for document {documentTypeId}/{documentId}."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
