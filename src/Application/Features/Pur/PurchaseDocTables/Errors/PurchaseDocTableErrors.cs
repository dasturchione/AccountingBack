using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public static class PurchaseDocTableErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjati qatori topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжати қатори топилмади.",
            LanguageIdConst.RU      => $"Строка документа закупки с id {id} не найдена.",
            _                       => $"Purchase document line with id {id} was not found."
        });

    public static Error OwnerNotFound(long ownerId, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.OwnerNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan xarid hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган харид ҳужжати топилмади.",
            LanguageIdConst.RU      => $"Документ закупки с id {ownerId} не найден.",
            _                       => $"Purchase document with id {ownerId} was not found."
        });

    public static Error OwnerAlreadyPosted(long ownerId, short? languageId = null) =>
        Error.Conflict("PurchaseDocTable.OwnerAlreadyPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan xarid hujjati o'tkazilgan, qator qo'shish, o'zgartirish yoki o'chirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган харид ҳужжати ўтказилган, қатор қўшиш, ўзгартириш ёки ўчириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ закупки с id {ownerId} уже проведён, добавление, изменение или удаление строк невозможно.",
            _                       => $"Purchase document with id {ownerId} is already posted. Lines cannot be added, modified or deleted."
        });

    public static Error VatRateNotFound(short vatRateId, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.VatRateNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {vatRateId} bo'lgan QQS stavkasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {vatRateId} бўлган ҚҚС ставкаси топилмади.",
            LanguageIdConst.RU      => $"Ставка НДС с id {vatRateId} не найдена.",
            _                       => $"VAT rate with id {vatRateId} was not found."
        });

    public static Error DirectTableCreateUnsupported(short? languageId = null) =>
        Error.Business("PurchaseDocTable.DirectTableCreateUnsupported", languageId switch
        {
            LanguageIdConst.UZ => "Xarid item qatorlari hujjat product line ichida yaratiladi.",
            _ => "Purchase item rows are created through purchase document product lines."
        });

    public static Error DirectTableMutationUnsupported(short? languageId = null) =>
        Error.Business("PurchaseDocTable.DirectTableMutationUnsupported", languageId switch
        {
            LanguageIdConst.UZ => "Xarid item qatorlari faqat xarid hujjati aggregate orqali o'zgartiriladi.",
            _ => "Purchase item rows can only be changed through the purchase document aggregate."
        });

    public static Error InvalidProductQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("PurchaseDocTable.InvalidProductQuantity", languageId switch
        {
            LanguageIdConst.UZ => $"Mahsulot id-si {productId} uchun miqdor noto'g'ri: {quantity}.",
            _ => $"Invalid quantity {quantity} for product id {productId}."
        });

    public static Error InvalidProductUnitPrice(int productId, decimal unitPrice, short? languageId = null) =>
        Error.Business("PurchaseDocTable.InvalidProductUnitPrice", languageId switch
        {
            LanguageIdConst.UZ => $"Mahsulot id-si {productId} uchun narx noto'g'ri: {unitPrice}.",
            _ => $"Invalid unit price {unitPrice} for product id {productId}."
        });

    public static Error ProductItemsRequired(int productId, short? languageId = null) =>
        Error.Business("PurchaseDocTable.ProductItemsRequired", languageId switch
        {
            LanguageIdConst.UZ => $"Mahsulot id-si {productId} uchun marking itemlar kiritilishi kerak.",
            _ => $"Items with marking numbers are required for product id {productId}."
        });

    public static Error ProductQuantityItemsMismatch(int productId, decimal quantity, int itemCount, short? languageId = null) =>
        Error.Business("PurchaseDocTable.ProductQuantityItemsMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"Mahsulot id-si {productId} uchun quantity ({quantity}) va items soni ({itemCount}) mos emas.",
            _ => $"Quantity ({quantity}) and item count ({itemCount}) do not match for product id {productId}."
        });

    public static Error DuplicateMarkingNumber(string markingNumber, short? languageId = null) =>
        Error.Business("PurchaseDocTable.DuplicateMarkingNumber", languageId switch
        {
            LanguageIdConst.UZ => $"Marking raqami takrorlangan: {markingNumber}.",
            _ => $"Duplicate marking number: {markingNumber}."
        });

    public static Error MarkingNumberRequired(int productId, short? languageId = null) =>
        Error.Business("PurchaseDocTable.MarkingNumberRequired", languageId switch
        {
            LanguageIdConst.UZ => $"Mahsulot id-si {productId} uchun marking raqami majburiy.",
            _ => $"Marking number is required for product id {productId}."
        });
}
