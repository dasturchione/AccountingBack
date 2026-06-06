using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Products;

public static class ProductErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Product.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tovar topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган товар топилмади.",
            LanguageIdConst.RU      => $"Товар с id {id} не найден.",
            _                       => $"Product with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Product.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan tovar allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган товар аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Товар с кодом '{code}' уже существует.",
            _                       => $"Product with code '{code}' already exists."
        });
}
