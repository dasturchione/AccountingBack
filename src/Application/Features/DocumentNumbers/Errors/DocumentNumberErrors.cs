using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.DocumentNumbers;

public static class DocumentNumberErrors
{
    public static Error InvalidOrganization(short? languageId = null) =>
        Error.Business("DocumentNumber.InvalidOrganization", languageId switch
        {
            LanguageIdConst.UZ => "Tashkilot noto'g'ri ko'rsatilgan.",
            LanguageIdConst.UZ_CYRL => "Ташкилот нотўғри кўрсатилган.",
            LanguageIdConst.RU => "Указана некорректная организация.",
            _ => "The organization is invalid."
        });

    public static Error DocumentTypeNotFound(short documentTypeId, short? languageId = null) =>
        Error.NotFound("DocumentNumber.DocumentTypeNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {documentTypeId} bo'lgan hujjat turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {documentTypeId} бўлган ҳужжат тури топилмади.",
            LanguageIdConst.RU => $"Тип документа с id {documentTypeId} не найден.",
            _ => $"Document type with id {documentTypeId} was not found."
        });

    public static Error InvalidDocumentDate(short? languageId = null) =>
        Error.Business("DocumentNumber.InvalidDocumentDate", languageId switch
        {
            LanguageIdConst.UZ => "Hujjat sanasi noto'g'ri ko'rsatilgan.",
            LanguageIdConst.UZ_CYRL => "Ҳужжат санаси нотўғри кўрсатилган.",
            LanguageIdConst.RU => "Указана некорректная дата документа.",
            _ => "The document date is invalid."
        });

    public static Error EarlierDocumentDate(DateTime lastDocumentDate, short? languageId = null) =>
        Error.Conflict("DocumentNumber.EarlierDocumentDate", languageId switch
        {
            LanguageIdConst.UZ => $"Hujjat sanasi oxirgi hujjat sanasidan oldin bo'lishi mumkin emas. Eng oxirgi sana: {lastDocumentDate:dd.MM.yyyy}.",
            LanguageIdConst.UZ_CYRL => $"Ҳужжат санаси охирги ҳужжат санасидан олдин бўлиши мумкин эмас. Энг охирги сана: {lastDocumentDate:dd.MM.yyyy}.",
            LanguageIdConst.RU => $"Дата документа не может быть раньше даты последнего документа. Последняя допустимая дата: {lastDocumentDate:dd.MM.yyyy}.",
            _ => $"The document date cannot be earlier than the latest document date. Latest allowed date: {lastDocumentDate:yyyy-MM-dd}."
        });

    public static Error TransactionRequired(short? languageId = null) =>
        Error.Problem("DocumentNumber.TransactionRequired", languageId switch
        {
            LanguageIdConst.UZ => "Hujjat raqamini yaratish faol tranzaksiyada bajarilishi kerak.",
            LanguageIdConst.UZ_CYRL => "Ҳужжат рақамини яратиш фаол транзакцияда бажарилиши керак.",
            LanguageIdConst.RU => "Генерация номера документа должна выполняться в активной транзакции.",
            _ => "Document number generation must run inside an active transaction."
        });

    public static Error SequenceConfigurationInvalid(short? languageId = null) =>
        Error.Problem("DocumentNumber.SequenceConfigurationInvalid", languageId switch
        {
            LanguageIdConst.UZ => "Hujjat raqamlari ketma-ketligi noto'g'ri sozlangan.",
            LanguageIdConst.UZ_CYRL => "Ҳужжат рақамлари кетма-кетлиги нотўғри созланган.",
            LanguageIdConst.RU => "Конфигурация последовательности номеров документов некорректна.",
            _ => "The document number sequence configuration is invalid."
        });

    public static Error CannotGenerate(short? languageId = null) =>
        Error.Problem("DocumentNumber.CannotGenerate", languageId switch
        {
            LanguageIdConst.UZ => "Keyingi hujjat raqamini olishning imkoni bo'lmadi.",
            LanguageIdConst.UZ_CYRL => "Кейинги ҳужжат рақамини олишнинг имкони бўлмади.",
            LanguageIdConst.RU => "Не удалось получить следующий номер документа.",
            _ => "Failed to obtain the next document number."
        });
}
