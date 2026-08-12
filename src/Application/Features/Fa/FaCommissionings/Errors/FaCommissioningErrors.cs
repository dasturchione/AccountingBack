using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaCommissionings;

public static class FaCommissioningErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaCommissioning.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan foydalanishga topshirish hujjati topilmadi.",
            LanguageIdConst.RU => $"Документ ввода в эксплуатацию с id {id} не найден.",
            _ => $"Commissioning document with id {id} was not found."
        });

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("FaCommissioning.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kamida bitta asosiy vosita kiritilishi shart.",
            LanguageIdConst.RU => "Необходимо добавить хотя бы одно основное средство.",
            _ => "At least one fixed asset is required."
        });

    public static Error DuplicateAsset(long assetId, short? languageId = null) =>
        Error.Conflict("FaCommissioning.DuplicateAsset", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vosita hujjatda takrorlangan.",
            LanguageIdConst.RU => $"Основное средство с id {assetId} повторяется в документе.",
            _ => $"Fixed asset with id {assetId} is duplicated in the document."
        });

    public static Error AssetNotFound(long assetId, short? languageId = null) =>
        Error.NotFound("FaCommissioning.AssetNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.RU => $"Основное средство с id {assetId} не найдено.",
            _ => $"Fixed asset with id {assetId} was not found."
        });

    public static Error AssetUnavailable(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.AssetUnavailable", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vositani foydalanishga topshirib bo'lmaydi.",
            LanguageIdConst.RU => $"Основное средство с id {assetId} нельзя ввести в эксплуатацию.",
            _ => $"Fixed asset with id {assetId} cannot be commissioned."
        });

    public static Error AccountingNotFound(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.AccountingNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vositaning hisob ma'lumotlari topilmadi.",
            LanguageIdConst.RU => $"Учетные данные основного средства с id {assetId} не найдены.",
            _ => $"Accounting data for fixed asset with id {assetId} was not found."
        });

    public static Error ReceiptNotPosted(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.ReceiptNotPosted", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vositaning qabul hujjati o'tkazilmagan.",
            LanguageIdConst.RU => $"Поступление основного средства с id {assetId} не проведено.",
            _ => $"The receipt for fixed asset with id {assetId} is not posted."
        });

    public static Error CapitalInvestmentAccountMissing(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.CapitalInvestmentAccountMissing", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vosita uchun kapital qo'yilma hisobi topilmadi.",
            LanguageIdConst.RU => $"Для основного средства с id {assetId} не найден счет капитальных вложений.",
            _ => $"Capital investment account for fixed asset with id {assetId} was not found."
        });

    public static Error ReferenceNotFound(string referenceName, long id, short? languageId = null) =>
        Error.NotFound("FaCommissioning.ReferenceNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"{referenceName} id {id} topilmadi.",
            LanguageIdConst.RU => $"{referenceName} с id {id} не найден.",
            _ => $"{referenceName} with id {id} was not found."
        });

    public static Error SalvageValueTooHigh(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.SalvageValueTooHigh", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vositaning tugatish qiymati boshlang'ich qiymatdan katta.",
            LanguageIdConst.RU => $"Ликвидационная стоимость ОС с id {assetId} превышает первоначальную.",
            _ => $"Salvage value exceeds initial cost for fixed asset with id {assetId}."
        });

    public static Error PlannedUnitsRequired(long assetId, short? languageId = null) =>
        Error.Business("FaCommissioning.PlannedUnitsRequired", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {assetId} bo'lgan asosiy vosita uchun reja birliklari majburiy.",
            LanguageIdConst.RU => $"Для основного средства с id {assetId} необходимо указать плановые единицы.",
            _ => $"Planned units are required for fixed asset with id {assetId}."
        });

    public static Error CannotUpdate(long id, short statusId, short? languageId = null) =>
        Error.Business("FaCommissioning.CannotUpdate", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tahrirlab bo'lmaydi.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя изменить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotConfirm(long id, short statusId, short? languageId = null) =>
        Error.Business("FaCommissioning.CannotConfirm", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя провести в статусе {statusId}.",
            _ => $"Document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancel(long id, short statusId, short? languageId = null) =>
        Error.Business("FaCommissioning.CannotCancel", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatni {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Документ с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error PostedDependenciesExist(short? languageId = null) =>
        Error.Conflict("FaCommissioning.PostedDependenciesExist", languageId switch
        {
            LanguageIdConst.UZ => "Foydalanishga topshirishni bekor qilib bo'lmaydi: keyingi o'tkazilgan FA hujjatlari mavjud.",
            LanguageIdConst.RU => "Нельзя отменить ввод в эксплуатацию: существуют последующие проведённые документы ОС.",
            _ => "Commissioning cannot be cancelled because later posted fixed-asset documents exist."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaCommissioning.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hujjatning faol posting batch'i topilmadi.",
            LanguageIdConst.RU => $"Для документа с id {id} не найден активный пакет проводок.",
            _ => $"Active posting batch for document with id {id} was not found."
        });
}
