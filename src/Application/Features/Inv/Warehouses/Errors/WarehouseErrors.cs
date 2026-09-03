using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public static class WarehouseErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Warehouse.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan ombor topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган омбор топилмади.",
            LanguageIdConst.RU      => $"Склад с id {id} не найден.",
            _                       => $"Warehouse with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Warehouse.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan ombor allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган омбор аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Склад с кодом '{code}' уже существует.",
            _                       => $"Warehouse with code '{code}' already exists."
        });

    public static Error BranchNotFound(int id, short? languageId = null) =>
        Error.NotFound("Warehouse.BranchNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan filial joriy tashkilotda topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган филиал жорий ташкилотда топилмади.",
            LanguageIdConst.RU      => $"Филиал с id {id} не найден в текущей организации.",
            _                       => $"Branch with id {id} was not found in the current organization."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("Warehouse.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan mas'ul foydalanuvchi joriy tashkilotda topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган масъул фойдаланувчи жорий ташкилотда топилмади.",
            LanguageIdConst.RU      => $"Ответственный пользователь с id {id} не найден в текущей организации.",
            _                       => $"Responsible user with id {id} was not found in the current organization."
        });
}
