using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public static class ChartAccountErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("ChartAccount.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisoblar rejasi topilmadi.",
            LanguageIdConst.RU => $"План счетов с id {id} не найден.",
            _ => $"Chart of accounts with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("ChartAccount.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Kodi '{code}' bo'lgan hisoblar rejasi allaqachon mavjud.",
            LanguageIdConst.RU => $"План счетов с кодом '{code}' уже существует.",
            _ => $"Chart of accounts with code '{code}' already exists."
        });

    public static Error NumberConflict(string number, short? languageId = null) =>
        Error.Conflict("ChartAccount.NumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Raqami '{number}' bo'lgan hisoblar rejasi allaqachon mavjud.",
            LanguageIdConst.RU => $"План счетов с номером '{number}' уже существует.",
            _ => $"Chart of accounts with number '{number}' already exists."
        });

    public static Error EmptyPresetAccounts(short? languageId = null) =>
        Error.Business("ChartAccount.EmptyPresetAccounts", languageId switch
        {
            LanguageIdConst.UZ => "Import qilish uchun kamida bitta shablon hisobi tanlanishi kerak.",
            LanguageIdConst.RU => "Для импорта нужно выбрать хотя бы один шаблонный счёт.",
            _ => "At least one preset account must be selected for import."
        });

    public static Error PresetAccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("ChartAccount.PresetAccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan shablon hisobi topilmadi.",
            LanguageIdConst.RU => $"Шаблонный счёт с id {id} не найден.",
            _ => $"Preset account with id {id} was not found."
        });

    public static Error DuplicateSubkontoType(short subkontoTypeId, short? languageId = null) =>
        Error.Conflict("ChartAccount.DuplicateSubkontoType", languageId switch
        {
            LanguageIdConst.UZ => $"Subkonto turi {subkontoTypeId} bir hisobda bir martadan ko'p ishlatilmasligi kerak.",
            LanguageIdConst.RU => $"Тип субконто {subkontoTypeId} нельзя добавлять к одному счёту больше одного раза.",
            _ => $"Subkonto type {subkontoTypeId} cannot be added to the same account more than once."
        });
}
