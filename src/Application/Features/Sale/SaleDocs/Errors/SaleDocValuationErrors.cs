using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public static partial class SaleDocErrors
{
    public static Error EmptyProducts(long id, short? languageId = null) =>
        Error.Business("SaleDoc.EmptyProducts", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatida mahsulot qatorlari mavjud emas.",
            _ => $"Sale document with id {id} has no product lines."
        });

    public static Error InvalidProductQuantity(long saleDocProductId, decimal quantity, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidProductQuantity", languageId switch
        {
            LanguageIdConst.UZ => $"SaleDocProduct id-si {saleDocProductId}: miqdor musbat butun son bo'lishi kerak. Joriy miqdor: {quantity}.",
            _ => $"SaleDocProduct id {saleDocProductId}: quantity must be a positive whole number. Current quantity: {quantity}."
        });

    public static Error InsufficientStock(int productId, int required, int available, short? languageId = null) =>
        Error.Conflict("SaleDoc.InsufficientStock", languageId switch
        {
            LanguageIdConst.UZ => $"Tovar id-si {productId} uchun omborda yetarli qoldiq yo'q. Kerak: {required}, mavjud: {available}.",
            _ => $"Not enough stock for product id {productId}. Required: {required}, available: {available}."
        });

    public static Error InvalidInventoryValuationMethod(string method, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidInventoryValuationMethod", languageId switch
        {
            LanguageIdConst.UZ => $"InventoryValuationMethod noto'g'ri: '{method}'.",
            _ => $"Invalid InventoryValuationMethod: '{method}'."
        });

    public static Error InventoryReservationConflict(short? languageId = null) =>
        Error.Conflict("SaleDoc.InventoryReservationConflict", languageId switch
        {
            LanguageIdConst.UZ => "Tanlangan tovarlardan biri boshqa hujjat tomonidan band qilingan. Qayta urinib ko'ring.",
            _ => "One or more selected inventory items were reserved by another document. Please retry."
        });
}
