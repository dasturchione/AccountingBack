using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public static class WarehouseErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Warehouse.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan ombor topilmadi.",
            LanguageIdConst.RU => $"Склад с id {id} не найден.",
            _ => $"Warehouse with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Warehouse.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi {code} bo'lgan ombor allaqachon mavjud.",
            LanguageIdConst.RU => $"Склад с кодом {code} уже существует.",
            _ => $"Warehouse with code {code} already exists."
        });
}
