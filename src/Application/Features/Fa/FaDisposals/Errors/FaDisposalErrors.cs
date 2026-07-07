using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public static class FaDisposalErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDisposal.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan disposal hujjati topilmadi.",
            LanguageIdConst.RU => $"Dokument disposal s id {id} ne nayden.",
            _ => $"Fixed asset disposal document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaDisposal.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta disposal qatori kiritilishi shart.",
            LanguageIdConst.RU => "Nuzhno dobavit khotya by odnu stroku disposal.",
            _ => "At least one disposal line is required."
        });

    public static Error InvalidDisposalType(short? languageId = null) =>
        Error.Business("FaDisposal.InvalidDisposalType", languageId switch
        {
            LanguageIdConst.UZ => "Disposal turi faqat SALE, WRITEOFF yoki BREAKDOWN bo'lishi mumkin.",
            LanguageIdConst.RU => "Tip disposal dolzhen byt SALE, WRITEOFF ili BREAKDOWN.",
            _ => "Disposal type must be SALE, WRITEOFF, or BREAKDOWN."
        });

    public static Error AssetNotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDisposal.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne naydeno.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error AssetInactive(long id, short? languageId = null) =>
        Error.Business("FaDisposal.AssetInactive", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita aktiv holatda emas.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne aktivno.",
            _ => $"Fixed asset with id {id} is not active."
        });

    public static Error AssetAlreadyDisposed(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.AssetAlreadyDisposed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita allaqachon hisobdan chiqarilgan.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} uzhe spisano.",
            _ => $"Fixed asset with id {id} is already disposed."
        });

    public static Error DuplicateAsset(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} povtoryayetsya v dokumente.",
            _ => $"Fixed asset with id {id} is duplicated in the document."
        });

    public static Error SaleAmountRequired(short? languageId = null) =>
        Error.Business("FaDisposal.SaleAmountRequired", languageId switch
        {
            LanguageIdConst.UZ => "SALE disposal turi uchun kamida bitta qatorda sale amount 0 dan katta bo'lishi shart.",
            LanguageIdConst.RU => "Dlya disposal tipa SALE nuzhna summа prodazhi bolshe 0 khotya by v odnoy stroke.",
            _ => "SALE disposal requires a positive sale amount on at least one line."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya redaktirovat v statuse {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya podtverdit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDisposal.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya otmenit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaDisposal.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan disposal hujjati uchun posting batch topilmadi.",
            LanguageIdConst.RU => $"Dlya disposal dokumenta s id {id} ne nayden posting batch.",
            _ => $"Posting batch was not found for disposal document with id {id}."
        });
}
