using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaRevaluations;

public static class FaRevaluationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaRevaluation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan revaluation hujjati topilmadi.",
            LanguageIdConst.RU => $"Dokument revaluation s id {id} ne nayden.",
            _ => $"Fixed asset revaluation document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaRevaluation.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta revaluation qatori kiritilishi shart.",
            LanguageIdConst.RU => "Nuzhno dobavit khotya by odnu stroku revaluation.",
            _ => "At least one revaluation line is required."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaRevaluation.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne naydeno.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaRevaluation.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne aktivno.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetDisposed(long id, short? languageId = null) =>
        Error.Business("FaRevaluation.AssetDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisobdan chiqarilgan asosiy vositani qayta baholab bo'lmaydi.",
            LanguageIdConst.RU => $"Spisannoye osnovnoye sredstvo s id {id} nelzya pereotsenit.",
            _ => $"Disposed fixed asset with id {id} cannot be revalued."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaRevaluation.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} povtoryayetsya v dokumente.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya redaktirovat v statuse {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya podtverdit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaRevaluation.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya otmenit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaRevaluation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan revaluation hujjati uchun posting batch topilmadi.",
            LanguageIdConst.RU => $"Dlya revaluation dokumenta s id {id} ne nayden posting batch.",
            _ => $"Posting batch was not found for revaluation document with id {id}."
        });
}
