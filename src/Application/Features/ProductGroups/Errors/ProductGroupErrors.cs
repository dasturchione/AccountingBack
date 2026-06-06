using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.ProductGroups;

public static class ProductGroupErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("ProductGroup.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mahsulot guruhi topilmadi.",
            LanguageIdConst.RU => $"Группа товаров с id {id} не найдена.",
            _ => $"Product group with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("ProductGroup.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi {code} bo'lgan mahsulot guruhi allaqachon mavjud.",
            LanguageIdConst.RU => $"Группа товаров с кодом {code} уже существует.",
            _ => $"Product group with code {code} already exists."
        });
}
