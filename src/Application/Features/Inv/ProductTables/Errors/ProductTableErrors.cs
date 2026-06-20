using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.ProductTables;

public static class ProductTableErrors
{
    public static Error NotFoundByMarkingNumber(string markingNumber, short? languageId = null) =>
        Error.NotFound("ProductTable.NotFoundByMarkingNumber", languageId switch
        {
            LanguageIdConst.UZ      => $"Markirovka raqami '{markingNumber}' bo'lgan tovar topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Маркировка рақами '{markingNumber}' бўлган товар топилмади.",
            LanguageIdConst.RU      => $"Товар с маркировочным номером '{markingNumber}' не найден.",
            _                       => $"Product with marking number '{markingNumber}' was not found."
        });
}
