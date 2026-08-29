using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public static class FaDisposalErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDisposal.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan disposal hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисобдан чиқариш ҳужжати топилмади.",
            LanguageIdConst.RU => $"Документ выбытия с id {id} не найден.",
            _ => $"Fixed asset disposal document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaDisposal.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta disposal qatori kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Камида битта ҳисобдан чиқариш қатори киритилиши шарт.",
            LanguageIdConst.RU => "Необходимо добавить хотя бы одну строку выбытия.",
            _ => "At least one disposal line is required."
        });

    public static Error InvalidDisposalType(short? languageId = null) =>
        Error.Business("FaDisposal.InvalidDisposalType", languageId switch
        {
            LanguageIdConst.UZ => "Disposal turi faqat SALE, WRITEOFF yoki BREAKDOWN bo'lishi mumkin.",
            LanguageIdConst.UZ_CYRL => "Ҳисобдан чиқариш тури фақат SALE, WRITEOFF ёки BREAKDOWN бўлиши мумкин.",
            LanguageIdConst.RU => "Тип выбытия должен быть SALE, WRITEOFF или BREAKDOWN.",
            _ => "Disposal type must be SALE, WRITEOFF, or BREAKDOWN."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDisposal.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита топилмади.",
            LanguageIdConst.RU => $"Основное средство с id {id} не найдено.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaDisposal.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита фаол ҳолатда эмас.",
            LanguageIdConst.RU => $"Основное средство с id {id} не активно.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetAlreadyDisposed(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.AssetAlreadyDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita allaqachon hisobdan chiqarilgan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита аллақачон ҳисобдан чиқарилган.",
            LanguageIdConst.RU => $"Основное средство с id {id} уже выбыло.",
            _ => $"Fixed asset with id {id} is already disposed."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита ҳужжатда такрорланган.",
            LanguageIdConst.RU => $"Основное средство с id {id} повторяется в документе.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error SaleAmountRequired(short? languageId = null) =>
        Error.Business("FaDisposal.SaleAmountRequired", languageId switch
        {
            LanguageIdConst.UZ => "SALE disposal turi uchun kamida bitta qatorda sale amount 0 dan katta bo'lishi shart.",
            LanguageIdConst.UZ_CYRL => "SALE турида камида битта қаторнинг сотув суммаси 0 дан катта бўлиши шарт.",
            LanguageIdConst.RU => "Для выбытия типа SALE хотя бы в одной строке сумма продажи должна быть больше нуля.",
            _ => "SALE disposal requires a positive sale amount on at least one line."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида таҳрирлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя изменять в статусе {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида тасдиқлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя подтвердить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида бекор қилиб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan disposal hujjati uchun posting batch topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисобдан чиқариш ҳужжати учун ўтказмалар пакети топилмади.",
            LanguageIdConst.RU => $"Для документа выбытия с id {id} не найден пакет проводок.",
            _ => $"Posting batch was not found for disposal document with id {id}."
        });
}
