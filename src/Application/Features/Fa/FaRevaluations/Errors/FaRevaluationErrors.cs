using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaRevaluations;

public static class FaRevaluationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaRevaluation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan revaluation hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган қайта баҳолаш ҳужжати топилмади.",
            LanguageIdConst.RU => $"Документ переоценки с id {id} не найден.",
            _ => $"Fixed asset revaluation document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaRevaluation.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta revaluation qatori kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Камида битта қайта баҳолаш қатори киритилиши шарт.",
            LanguageIdConst.RU => "Необходимо добавить хотя бы одну строку переоценки.",
            _ => "At least one revaluation line is required."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaRevaluation.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита топилмади.",
            LanguageIdConst.RU => $"Основное средство с id {id} не найдено.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaRevaluation.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита фаол ҳолатда эмас.",
            LanguageIdConst.RU => $"Основное средство с id {id} не активно.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetDisposed(long id, short? languageId = null) =>
        Error.Business("FaRevaluation.AssetDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisobdan chiqarilgan asosiy vositani qayta baholab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисобдан чиқарилган асосий воситани қайта баҳолаб бўлмайди.",
            LanguageIdConst.RU => $"Выбывшее основное средство с id {id} нельзя переоценить.",
            _ => $"Disposed fixed asset with id {id} cannot be revalued."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaRevaluation.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита ҳужжатда такрорланган.",
            LanguageIdConst.RU => $"Основное средство с id {id} повторяется в документе.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида таҳрирлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя изменять в статусе {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида тасдиқлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя подтвердить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида бекор қилиб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaRevaluation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan revaluation hujjati uchun posting batch topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган қайта баҳолаш ҳужжати учун ўтказмалар пакети топилмади.",
            LanguageIdConst.RU => $"Для документа переоценки с id {id} не найден пакет проводок.",
            _ => $"Posting batch was not found for revaluation document with id {id}."
        });
}
