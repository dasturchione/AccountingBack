using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public static class FaReceiptErrors
{
    public static Error VatAccountRequired(short? languageId = null) =>
        Error.Business("FaReceipt.VatAccountRequired", languageId switch
        {
            LanguageIdConst.UZ => "Qatorda QQS mavjud bo'lsa, QQS hisobvarag'i ko'rsatilishi kerak.",
            LanguageIdConst.UZ_CYRL => "Қаторда ҚҚС мавжуд бўлса, ҚҚС ҳисобварағи кўрсатилиши керак.",
            LanguageIdConst.RU => "Если в строке есть НДС, необходимо указать счёт НДС.",
            _ => "VAT account is required when the receipt line has VAT."
        });

    public static Error PostedDependenciesExist(short? languageId = null) =>
        Error.Conflict("FaReceipt.PostedDependenciesExist", languageId switch
        {
            LanguageIdConst.UZ => "Qabul hujjatini bekor qilib bo'lmaydi: unga bog'liq o'tkazilgan asosiy vosita hujjatlari mavjud.",
            LanguageIdConst.UZ_CYRL => "Қабул ҳужжатини бекор қилиб бўлмайди: унга боғлиқ ўтказилган асосий восита ҳужжатлари мавжуд.",
            LanguageIdConst.RU => "Нельзя отменить поступление: существуют проведённые зависимые документы основных средств.",
            _ => "The receipt cannot be cancelled because posted dependent fixed-asset documents exist."
        });

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaReceipt.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita qabul hujjati topilmadi.",
            LanguageIdConst.RU => $"Dokument priyoma osnovnykh sredstv s id {id} ne nayden.",
            _ => $"Fixed asset receipt document with id {id} was not found."
        });

    public static Error CounterpartyNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaReceipt.CounterpartyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan yetkazib beruvchi topilmadi.",
            LanguageIdConst.RU => $"Postavshchik s id {id} ne nayden.",
            _ => $"Counterparty with id {id} was not found."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaReceipt.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.RU => $"Valyuta s id {id} ne naydena.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error VatRateNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaReceipt.VatRateNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan QQS stavkasi topilmadi.",
            LanguageIdConst.RU => $"Stavka NDS s id {id} ne naydena.",
            _ => $"VAT rate with id {id} was not found."
        });

    public static Error FaGroupNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaReceipt.FaGroupNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita guruhi topilmadi.",
            LanguageIdConst.RU => $"Gruppa osnovnykh sredstv s id {id} ne naydena.",
            _ => $"Fixed asset group with id {id} was not found."
        });

    public static Error OkofNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaReceipt.OkofNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan OKOF kodi topilmadi.",
            LanguageIdConst.RU => $"Kod OKOF s id {id} ne nayden.",
            _ => $"OKOF with id {id} was not found."
        });

    public static Error DepreciationMethodNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaReceipt.DepreciationMethodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya usuli topilmadi.",
            LanguageIdConst.RU => $"Metod amortizatsii s id {id} ne nayden.",
            _ => $"Depreciation method with id {id} was not found."
        });

    public static Error DepartmentNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaReceipt.DepartmentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.RU => $"Otdel s id {id} ne nayden.",
            _ => $"Department with id {id} was not found."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaReceipt.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mas'ul foydalanuvchi topilmadi.",
            LanguageIdConst.RU => $"Otvetstvennyy polzovatel s id {id} ne nayden.",
            _ => $"Responsible user with id {id} was not found."
        });

    public static Error InventoryNumberConflict(string inventoryNumber, short? languageId = null) =>
        Error.Conflict("FaReceipt.InventoryNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Inventar raqami '{inventoryNumber}' bo'lgan asosiy vosita allaqachon mavjud.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s inventarnym nomerom '{inventoryNumber}' uzhe sushchestvuyet.",
            _ => $"Fixed asset with inventory number '{inventoryNumber}' already exists."
        });

    public static Error DuplicateInventoryNumber(string inventoryNumber, short? languageId = null) =>
        Error.Conflict("FaReceipt.DuplicateInventoryNumber", languageId switch
        {
            LanguageIdConst.UZ => $"Inventar raqami '{inventoryNumber}' hujjat ichida takrorlangan.",
            LanguageIdConst.RU => $"Inventarnyy nomer '{inventoryNumber}' povtoryayetsya v dokumente.",
            _ => $"Inventory number '{inventoryNumber}' is duplicated within the document."
        });

    public static Error InvalidReceiptType(short receiptType, short? languageId = null) =>
        Error.Business("FaReceipt.InvalidReceiptType", languageId switch
        {
            LanguageIdConst.UZ => $"'{receiptType}' qabul turi qo'llab-quvvatlanmaydi.",
            LanguageIdConst.RU => $"Tip priyoma '{receiptType}' ne podderzhivayetsya.",
            _ => $"Receipt type '{receiptType}' is not supported."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaReceipt.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta qator kiritilishi shart.",
            LanguageIdConst.RU => "Nuzhno dobavit khotya by odnu stroku.",
            _ => "At least one line is required."
        });

    public static Error AssetLinesRequired(string lineName, short? languageId = null) =>
        Error.Business("FaReceipt.AssetLinesRequired", languageId switch
        {
            LanguageIdConst.UZ => $"'{lineName}' qatori uchun kamida bitta asset kartasi kiritilishi shart.",
            LanguageIdConst.RU => $"Dlya stroki '{lineName}' nuzhno ukazat khotya by odnu kartochku OS.",
            _ => $"At least one asset card is required for line '{lineName}'."
        });

    public static Error LineQuantityMismatch(string lineName, decimal quantity, int assetCount, short? languageId = null) =>
        Error.Business("FaReceipt.LineQuantityMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"'{lineName}' qatorida son ({quantity}) asset kartalari soniga ({assetCount}) teng bo'lishi kerak.",
            LanguageIdConst.RU => $"V stroke '{lineName}' kolichestvo ({quantity}) dolzhno sovpadat s kolichestvom kartochek ({assetCount}).",
            _ => $"Line '{lineName}' quantity ({quantity}) must match asset count ({assetCount})."
        });

    public static Error LineAmountMismatch(string lineName, decimal amount, decimal assetTotal, short? languageId = null) =>
        Error.Business("FaReceipt.LineAmountMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"'{lineName}' qator summasi ({amount}) asset boshlang'ich qiymatlari yig'indisiga ({assetTotal}) teng emas.",
            LanguageIdConst.RU => $"Summa stroki '{lineName}' ({amount}) ne ravna summarnoy pervonachalnoy stoimosti OS ({assetTotal}).",
            _ => $"Line '{lineName}' amount ({amount}) does not match summed asset initial cost ({assetTotal})."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaReceipt.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya redaktirovat v statuse {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaReceipt.CannotDeleteInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida o'chirib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya udalit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be deleted in status {statusId}."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaReceipt.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya provesti v statuse {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaReceipt.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument s id {id} nelzya otmenit v statuse {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaReceipt.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan qabul hujjati uchun faol o'tkazmalar paketi topilmadi.",
            LanguageIdConst.RU => $"Для документа поступления с id {id} не найден активный пакет проводок.",
            _ => $"Active posting batch for fixed asset receipt {id} was not found."
        });
    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Business("FaReceipt.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjat allaqachon bekor qilingan.",
            LanguageIdConst.RU => $"Dokument s id {id} uzhe otmenyon.",
            _ => $"Document with id {id} is already cancelled."
        });

    public static Error PlannedUnitsRequired(string inventoryNumber, short? languageId = null) =>
        Error.Business("FaReceipt.PlannedUnitsRequired", languageId switch
        {
            LanguageIdConst.UZ => $"'{inventoryNumber}' inventar raqami uchun ishlab chiqarish hajmi bo'yicha usulda reja birliklari majburiy.",
            LanguageIdConst.RU => $"Dlya inventarnogo nomera '{inventoryNumber}' pri metode po obyemu proizvodstva nuzhno ukazat planovoye kolichestvo edinits.",
            _ => $"Planned units total is required for inventory number '{inventoryNumber}' when units of production depreciation is used."
        });
}
