using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public static class OrganizationSetupErrors
{
    public static Error OrganizationContextRequired(short? languageId = null) => Business(
        "OrganizationSetup.OrganizationContextRequired", languageId,
        "Tashkilot konteksti talab qilinadi. X-OrganizationId sarlavhasini yuboring yoki standart tashkilotli tokendan foydalaning.",
        "Ташкилот контексти талаб қилинади. X-OrganizationId сарлавҳасини юборинг ёки стандарт ташкилотли токендан фойдаланинг.",
        "Требуется контекст организации. Передайте заголовок X-OrganizationId или токен с организацией по умолчанию.",
        "Organization context is required. Send X-OrganizationId header or use a token with default organization.");

    public static Error Forbidden(short? languageId = null) => Error.Forbidden(
        "OrganizationSetup.Forbidden", Message(languageId,
            "Sizda ushbu tashkilot sozlamalarini boshqarish huquqi yo'q.",
            "Сизда ушбу ташкилот созламаларини бошқариш ҳуқуқи йўқ.",
            "У вас нет доступа к управлению настройкой этой организации.",
            "You do not have access to manage this organization setup."));

    public static Error OrganizationNotFound(int id, short? languageId = null) => NotFound(
        "OrganizationSetup.OrganizationNotFound", id, languageId,
        "tashkilot", "ташкилот", "организация", "Organization");

    public static Error InnConflict(string inn, short? languageId = null) => Error.Conflict(
        "OrganizationSetup.InnConflict", Message(languageId,
            $"STIR '{inn}' bo'lgan tashkilot allaqachon mavjud.", $"СТИР '{inn}' бўлган ташкилот аллақачон мавжуд.",
            $"Организация с ИНН '{inn}' уже существует.", $"Organization with INN '{inn}' already exists."));

    public static Error TaxTypeNotFound(short id, short? languageId = null) => NotFound(
        "OrganizationSetup.TaxTypeNotFound", id, languageId,
        "soliq turi", "солиқ тури", "тип налога", "Tax type");

    public static Error AccountingPolicyNotFound(short id, short? languageId = null) => NotFound(
        "OrganizationSetup.AccountingPolicyNotFound", id, languageId,
        "hisob siyosati", "ҳисоб сиёсати", "учётная политика", "Accounting policy");

    public static Error CurrencyNotFound(short id, short? languageId = null) => NotFound(
        "OrganizationSetup.CurrencyNotFound", id, languageId,
        "valyuta", "валюта", "валюта", "Currency");

    public static Error InvalidInventoryValuationMethod(string method, short? languageId = null) => Business(
        "OrganizationSetup.InvalidInventoryValuationMethod", languageId,
        $"'{method}' zaxiralarni baholash usuli qo'llab-quvvatlanmaydi.",
        $"'{method}' захираларни баҳолаш усули қўллаб-қувватланмайди.",
        $"Метод оценки запасов '{method}' не поддерживается.",
        $"Inventory valuation method '{method}' is not supported.");

    public static Error BranchNotFound(int id, short? languageId = null) => ScopedNotFound(
        "OrganizationSetup.BranchNotFound", id, languageId, "filial", "филиал", "филиал", "Branch");

    public static Error WarehouseNotFound(int id, short? languageId = null) => ScopedNotFound(
        "OrganizationSetup.WarehouseNotFound", id, languageId, "ombor", "омбор", "склад", "Warehouse");

    public static Error CashBoxNotFound(int id, short? languageId = null) => ScopedNotFound(
        "OrganizationSetup.CashBoxNotFound", id, languageId, "kassa", "касса", "касса", "Cash box");

    public static Error BankAccountNotFound(int id, short? languageId = null) => ScopedNotFound(
        "OrganizationSetup.BankAccountNotFound", id, languageId, "bank hisobvarag'i", "банк ҳисобварағи", "банковский счёт", "Bank account");

    public static Error ChartAccountNotFound(int id, short? languageId = null) => Error.NotFound(
        "OrganizationSetup.ChartAccountNotFound", Message(languageId,
            $"Joriy tashkilotda id-si {id} bo'lgan buxgalteriya hisobvarag'i topilmadi.",
            $"Жорий ташкилотда id-си {id} бўлган бухгалтерия ҳисобварағи топилмади.",
            $"Бухгалтерский счёт с id {id} не найден в текущей организации.",
            $"Chart account with id {id} was not found in the current organization."));

    public static Error SetupNotReady(string missingStep, short? languageId = null) => Business(
        "OrganizationSetup.NotReady", languageId,
        $"Sozlashni yakunlab bo'lmaydi. Bajarilmagan bosqich: {missingStep}.",
        $"Созлашни якунлаб бўлмайди. Бажарилмаган босқич: {missingStep}.",
        $"Настройку нельзя завершить. Пропущенный шаг: {missingStep}.",
        $"Setup cannot be completed. Missing step: {missingStep}.");

    private static Error NotFound(string code, int id, short? languageId,
        string uzName, string uzCyrlName, string ruName, string enName) =>
        Error.NotFound(code, Message(languageId,
            $"Id-si {id} bo'lgan {uzName} topilmadi.", $"Id-си {id} бўлган {uzCyrlName} топилмади.",
            $"{ruName} с id {id} не найдена.", $"{enName} with id {id} was not found."));

    private static Error ScopedNotFound(string code, int id, short? languageId,
        string uzName, string uzCyrlName, string ruName, string enName) =>
        Error.NotFound(code, Message(languageId,
            $"Joriy tashkilotda id-si {id} bo'lgan {uzName} topilmadi.",
            $"Жорий ташкилотда id-си {id} бўлган {uzCyrlName} топилмади.",
            $"{ruName} с id {id} не найден в текущей организации.",
            $"{enName} with id {id} was not found in current organization."));

    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
