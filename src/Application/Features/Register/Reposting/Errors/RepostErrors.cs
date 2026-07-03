using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Reposting;

public static class RepostErrors
{
    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("Repost.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "DateFrom DateTo dan katta bo'lishi mumkin emas.",
            LanguageIdConst.UZ_CYRL => "DateFrom DateTo дан катта бўлиши мумкин эмас.",
            LanguageIdConst.RU => "Дата начала не может быть больше даты окончания.",
            _ => "DateFrom cannot be greater than DateTo."
        });

    public static Error PeriodNotFound(int id, short? languageId = null) =>
        Error.NotFound("Repost.PeriodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob davri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб даври топилмади.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error DateRangeOutsidePeriod(int periodId, short? languageId = null) =>
        Error.Business("Repost.DateRangeOutsidePeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Tanlangan sana oralig'i {periodId} davr chegarasidan tashqariga chiqmoqda.",
            LanguageIdConst.UZ_CYRL => $"Танланган сана оралиғи {periodId} давр чегарасидан ташқарига чиқмоқда.",
            LanguageIdConst.RU => $"Выбранный диапазон дат выходит за границы периода {periodId}.",
            _ => $"Selected date range falls outside accounting period {periodId}."
        });

    public static Error ClosedPeriod(int periodId, short? languageId = null) =>
        Error.Conflict("Repost.ClosedPeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Accounting period {periodId} yopilgan. Reposting ruxsat etilmaydi.",
            LanguageIdConst.UZ_CYRL => $"Accounting period {periodId} ёпилган. Reposting рухсат этилмайди.",
            LanguageIdConst.RU => $"Учётный период {periodId} закрыт. Перепроведение недоступно.",
            _ => $"Accounting period {periodId} is closed. Reposting is not allowed."
        });

    public static Error DocumentTypeRequired(short? languageId = null) =>
        Error.Business("Repost.DocumentTypeRequired", languageId switch
        {
            LanguageIdConst.UZ => "DocumentId yuborilganda DocumentType ham yuborilishi shart.",
            LanguageIdConst.UZ_CYRL => "DocumentId юборилганда DocumentType ҳам юборилиши шарт.",
            LanguageIdConst.RU => "При передаче DocumentId необходимо указать и DocumentType.",
            _ => "DocumentType is required when DocumentId is specified."
        });

    public static Error UnsupportedDocumentType(short documentType, short? languageId = null) =>
        Error.Business("Repost.UnsupportedDocumentType", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentType {documentType} reposting uchun qo'llab-quvvatlanmaydi.",
            LanguageIdConst.UZ_CYRL => $"DocumentType {documentType} reposting учун қўллаб-қувватланмайди.",
            LanguageIdConst.RU => $"DocumentType {documentType} не поддерживается для перепроведения.",
            _ => $"DocumentType {documentType} is not supported for reposting."
        });

    public static Error InvalidDocument(short documentType, long documentId, short? languageId = null) =>
        Error.NotFound("Repost.InvalidDocument", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentType {documentType}, DocumentId {documentId} uchun repost qilinadigan posted document topilmadi.",
            LanguageIdConst.UZ_CYRL => $"DocumentType {documentType}, DocumentId {documentId} учун repost қилинадиган posted document топилмади.",
            LanguageIdConst.RU => $"Не найден проведённый документ для перепроведения: DocumentType {documentType}, DocumentId {documentId}.",
            _ => $"Posted document for reposting was not found: DocumentType {documentType}, DocumentId {documentId}."
        });

    public static Error AlreadyReposting(short documentType, long documentId, short? languageId = null) =>
        Error.Conflict("Repost.AlreadyReposting", languageId switch
        {
            LanguageIdConst.UZ => $"DocumentType {documentType}, DocumentId {documentId} hozir repost qilinmoqda.",
            LanguageIdConst.UZ_CYRL => $"DocumentType {documentType}, DocumentId {documentId} ҳозир repost қилиняпти.",
            LanguageIdConst.RU => $"DocumentType {documentType}, DocumentId {documentId} уже перепроводится.",
            _ => $"DocumentType {documentType}, DocumentId {documentId} is already being reposted."
        });
}
