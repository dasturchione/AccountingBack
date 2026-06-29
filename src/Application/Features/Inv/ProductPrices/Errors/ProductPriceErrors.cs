using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductPrices;

public static class ProductPriceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("ProductPrice.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tovar narxi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган товар нархи топилмади.",
            LanguageIdConst.RU      => $"Цена товара с id {id} не найдена.",
            _                       => $"Product price with id {id} was not found."
        });
}
