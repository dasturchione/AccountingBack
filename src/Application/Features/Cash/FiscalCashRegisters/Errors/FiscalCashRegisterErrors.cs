using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FiscalCashRegisters;

public static class FiscalCashRegisterErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("FiscalCashRegister.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan fiskal kassa topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган фискал касса топилмади.",
            LanguageIdConst.RU => $"Фискальный кассовый регистр с id {id} не найден.",
            _ => $"Fiscal cash register with id {id} was not found."
        });

    public static Error OrganizationRequired(short? languageId = null) =>
        Error.Business("FiscalCashRegister.OrganizationRequired", languageId switch
        {
            LanguageIdConst.UZ => "Fiskal kassani yaratish uchun tashkilotni tanlang.",
            LanguageIdConst.UZ_CYRL => "Фискал кассани яратиш учун ташкилотни танланг.",
            LanguageIdConst.RU => "Для создания фискального кассового регистра выберите организацию.",
            _ => "Select an organization before creating a fiscal cash register."
        });

    public static Error WarehouseNotFound(int id, short? languageId = null) =>
        Error.NotFound("FiscalCashRegister.WarehouseNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan ombor tashkilotga tegishli emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган омбор ташкилотга тегишли эмас.",
            LanguageIdConst.RU => $"Склад с id {id} не принадлежит организации.",
            _ => $"Warehouse with id {id} does not belong to the organization."
        });

    public static Error RegisterTypeNotFound(short id, short? languageId = null) =>
        Error.NotFound("FiscalCashRegister.RegisterTypeNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan fiskal kassa turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган фискал касса тури топилмади.",
            LanguageIdConst.RU => $"Тип фискального кассового регистра с id {id} не найден.",
            _ => $"Fiscal cash register type with id {id} was not found."
        });

    public static Error ExternalRegisterIdConflict(string value, short? languageId = null) =>
        Error.Conflict("FiscalCashRegister.ExternalRegisterIdConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Tashqi kassa ID '{value}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ташқи касса ID '{value}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Внешний идентификатор кассы '{value}' уже существует в этой организации.",
            _ => $"External cash register ID '{value}' already exists in this organization."
        });

    public static Error SerialNumberConflict(string value, short? languageId = null) =>
        Error.Conflict("FiscalCashRegister.SerialNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Seriya raqami '{value}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Серия рақами '{value}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Серийный номер '{value}' уже существует в этой организации.",
            _ => $"Serial number '{value}' already exists in this organization."
        });

    public static Error FiscalModuleNumberConflict(string value, short? languageId = null) =>
        Error.Conflict("FiscalCashRegister.FiscalModuleNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Fiskal modul raqami '{value}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Фискал модул рақами '{value}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Номер фискального модуля '{value}' уже существует в этой организации.",
            _ => $"Fiscal module number '{value}' already exists in this organization."
        });
}
