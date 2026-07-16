using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public static class ProductStockErrors
{
    public static Error NotFoundByMarkingNumber(string markingNumber, short? languageId = null) =>
        Error.NotFound("ProductTable.NotFoundByMarkingNumber", languageId switch
        {
            LanguageIdConst.UZ      => $"Markirovka raqami '{markingNumber}' bo'lgan tovar topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Маркировка рақами '{markingNumber}' бўлган товар топилмади.",
            LanguageIdConst.RU      => $"Товар с маркировочным номером '{markingNumber}' не найден.",
            _                       => $"Product with marking number '{markingNumber}' was not found."
        });

    public static Error NotAvailableByMarkingNumber(string markingNumber, short? languageId = null) =>
        Error.NotFound("ProductTable.NotAvailableByMarkingNumber", languageId switch
        {
            LanguageIdConst.UZ      => $"Markirovka raqami '{markingNumber}' bo'lgan tovar omborda mavjud emas (sotilgan yoki band).",
            LanguageIdConst.UZ_CYRL => $"Маркировка рақами '{markingNumber}' бўлган товар омборда мавжуд эмас (сотилган ёки банд).",
            LanguageIdConst.RU      => $"Товар с маркировочным номером '{markingNumber}' недоступен на складе (продан или зарезервирован).",
            _                       => $"Product with marking number '{markingNumber}' is not available in stock (sold or reserved)."
        });
    public static Error WarehouseRequired(short? languageId = null) =>
        Error.Business("ProductStock.WarehouseRequired", languageId switch
        {
            LanguageIdConst.UZ => "Omborni tanlang.",
            LanguageIdConst.RU => "Выберите склад.",
            _ => "Warehouse must be selected."
        });
}
