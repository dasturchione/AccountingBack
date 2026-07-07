using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaMovements;

public static class FaMovementErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaMovement.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita ko'chirish hujjati topilmadi.",
            LanguageIdConst.RU => $"Dokument peremeshcheniya osnovnykh sredstv s id {id} ne nayden.",
            _ => $"Fixed asset movement document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaMovement.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta asset qatori kiritilishi shart.",
            LanguageIdConst.RU => "Nuzhno dobavit khotya by odnu stroku s osnovnym sredstvom.",
            _ => "At least one asset line is required."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaMovement.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne naydeno.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaMovement.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne aktivno.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetDisposed(long id, short? languageId = null) =>
        Error.Business("FaMovement.AssetDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisobdan chiqarilgan asosiy vositani ko'chirib bo'lmaydi.",
            LanguageIdConst.RU => $"Spisannoye osnovnoye sredstvo s id {id} nelzya peremestit.",
            _ => $"Disposed fixed asset with id {id} cannot be moved."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaMovement.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} povtoryayetsya v dokumente.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error DepartmentNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaMovement.DepartmentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.RU => $"Otdel s id {id} ne nayden.",
            _ => $"Department with id {id} was not found."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaMovement.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mas'ul foydalanuvchi topilmadi.",
            LanguageIdConst.RU => $"Otvetstvennyy polzovatel s id {id} ne nayden.",
            _ => $"Responsible user with id {id} was not found."
        });

    public static Error NoTargetChange(short? languageId = null) =>
        Error.Business("FaMovement.NoTargetChange", languageId switch
        {
            LanguageIdConst.UZ => "Ko'chirish uchun bo'lim yoki mas'ul shaxsda kamida bitta o'zgarish bo'lishi shart.",
            LanguageIdConst.RU => "Dlya peremeshcheniya dolzhno izmenitsya khotya by odno znachenie: otdel ili otvetstvennyy.",
            _ => "Movement requires a change in department or responsible user."
        });

    public static Error MixedSourceOwnership(short? languageId = null) =>
        Error.Business("FaMovement.MixedSourceOwnership", languageId switch
        {
            LanguageIdConst.UZ => "Bitta movement hujjatidagi barcha assetlar bir xil joriy bo'lim va mas'ul shaxsga tegishli bo'lishi shart.",
            LanguageIdConst.RU => "Vse osnovnyye sredstva v odnom dokumente dolzhny imet odinakovyy tekushchiy otdel i otvetstvennogo.",
            _ => "All assets in one movement document must share the same current department and responsible user."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya redaktirovat v statuse {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya podtverdit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaMovement.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya otmenit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Business("FaMovement.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjat allaqachon bekor qilingan.",
            LanguageIdConst.RU => $"Dokument s id {id} uzhe otmenyon.",
            _ => $"Document with id {id} is already cancelled."
        });
}
