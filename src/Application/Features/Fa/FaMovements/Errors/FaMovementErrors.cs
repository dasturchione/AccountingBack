using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaMovements;

public static class FaMovementErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaMovement.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita ko'chirish hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита кўчириш ҳужжати топилмади.",
            LanguageIdConst.RU => $"Документ перемещения основных средств с id {id} не найден.",
            _ => $"Fixed asset movement document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaMovement.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta asset qatori kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Камида битта асосий восита қатори киритилиши шарт.",
            LanguageIdConst.RU => "Необходимо добавить хотя бы одну строку основного средства.",
            _ => "At least one asset line is required."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaMovement.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита топилмади.",
            LanguageIdConst.RU => $"Основное средство с id {id} не найдено.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaMovement.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита фаол ҳолатда эмас.",
            LanguageIdConst.RU => $"Основное средство с id {id} не активно.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetDisposed(long id, short? languageId = null) =>
        Error.Business("FaMovement.AssetDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisobdan chiqarilgan asosiy vositani ko'chirib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисобдан чиқарилган асосий воситани кўчириб бўлмайди.",
            LanguageIdConst.RU => $"Выбывшее основное средство с id {id} нельзя переместить.",
            _ => $"Disposed fixed asset with id {id} cannot be moved."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaMovement.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита ҳужжатда такрорланган.",
            LanguageIdConst.RU => $"Основное средство с id {id} повторяется в документе.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error DepartmentNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaMovement.DepartmentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган бўлим топилмади.",
            LanguageIdConst.RU => $"Подразделение с id {id} не найдено.",
            _ => $"Department with id {id} was not found."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaMovement.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mas'ul foydalanuvchi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган масъул фойдаланувчи топилмади.",
            LanguageIdConst.RU => $"Ответственный пользователь с id {id} не найден.",
            _ => $"Responsible user with id {id} was not found."
        });

    public static Error NoTargetChange(short? languageId = null) =>
        Error.Business("FaMovement.NoTargetChange", languageId switch
        {
            LanguageIdConst.UZ => "Ko'chirish uchun bo'lim yoki mas'ul shaxsda kamida bitta o'zgarish bo'lishi shart.",
            LanguageIdConst.UZ_CYRL => "Кўчириш учун бўлим ёки масъул шахсда камида битта ўзгариш бўлиши шарт.",
            LanguageIdConst.RU => "Для перемещения должно измениться подразделение или ответственное лицо.",
            _ => "Movement requires a change in department or responsible user."
        });

    public static Error MixedSourceOwnership(short? languageId = null) =>
        Error.Business("FaMovement.MixedSourceOwnership", languageId switch
        {
            LanguageIdConst.UZ => "Bitta movement hujjatidagi barcha assetlar bir xil joriy bo'lim va mas'ul shaxsga tegishli bo'lishi shart.",
            LanguageIdConst.UZ_CYRL => "Битта кўчириш ҳужжатидаги барча асосий воситалар бир хил жорий бўлим ва масъул шахсга тегишли бўлиши шарт.",
            LanguageIdConst.RU => "Все основные средства в одном документе должны иметь одинаковое текущее подразделение и ответственное лицо.",
            _ => "All assets in one movement document must share the same current department and responsible user."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида таҳрирлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя изменять в статусе {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида тасдиқлаб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя подтвердить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжатни {statusId} ҳолатида бекор қилиб бўлмайди.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Business("FaMovement.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjat allaqachon bekor qilingan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳужжат аллақачон бекор қилинган.",
            LanguageIdConst.RU => $"Документ с id {id} уже отменён.",
            _ => $"Document with id {id} is already cancelled."
        });
}
