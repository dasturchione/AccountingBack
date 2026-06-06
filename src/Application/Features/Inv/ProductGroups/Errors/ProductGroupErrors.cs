using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public static class ProductGroupErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("ProductGroup.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tovar guruhi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган товар гуруҳи топилмади.",
            LanguageIdConst.RU      => $"Группа товаров с id {id} не найдена.",
            _                       => $"Product group with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("ProductGroup.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan tovar guruhi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган товар гуруҳи аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Группа товаров с кодом '{code}' уже существует.",
            _                       => $"Product group with code '{code}' already exists."
        });
}
