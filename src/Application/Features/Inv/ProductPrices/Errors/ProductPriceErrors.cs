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

    public static Error ProductNotFound(int id, short? languageId = null) =>
        Error.NotFound("ProductPrice.ProductNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tovar joriy tashkilotda topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган товар жорий ташкилотда топилмади.",
            LanguageIdConst.RU      => $"Товар с id {id} не найден в текущей организации.",
            _                       => $"Product with id {id} was not found in the current organization."
        });
}
