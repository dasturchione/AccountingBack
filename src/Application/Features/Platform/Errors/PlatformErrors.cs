using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Platform;

public static class PlatformErrors
{
    public static Error GlobalAccessRequired(short? languageId = null) =>
        Error.Forbidden("Platform.GlobalAccessRequired", Message(languageId,
            "Platformani boshqarish API-laridan faqat global ruxsatli foydalanuvchilar foydalanishi mumkin.",
            "Платформани бошқариш API-ларидан фақат глобал рухсатли фойдаланувчилар фойдаланиши мумкин.",
            "API администрирования платформы доступны только пользователям с глобальным доступом.",
            "Only users with global access can use platform administration endpoints."));

    public static Error TenantNotFound(int id, short? languageId = null) =>
        Error.NotFound("PlatformTenant.NotFound", NotFoundMessage("tenant", "tenant", "тенант", id, languageId));

    public static Error TenantSlugConflict(string slug, short? languageId = null) =>
        Error.Conflict("PlatformTenant.SlugConflict", Message(languageId,
            $"'{slug}' slugli tenant allaqachon mavjud.", $"'{slug}' slugли tenant аллақачон мавжуд.",
            $"Тенант со slug '{slug}' уже существует.", $"Tenant with slug '{slug}' already exists."));

    public static Error UserNotFound(int id, short? languageId = null) =>
        Error.NotFound("PlatformUser.NotFound", NotFoundMessage("foydalanuvchi", "фойдаланувчи", "пользователь", id, languageId));

    public static Error UserNameConflict(string userName, short? languageId = null) =>
        Error.Conflict("PlatformUser.UserNameConflict", Message(languageId,
            $"'{userName}' nomli foydalanuvchi allaqachon mavjud.", $"'{userName}' номли фойдаланувчи аллақачон мавжуд.",
            $"Пользователь с именем '{userName}' уже существует.", $"User with username '{userName}' already exists."));

    public static Error RoleNotFound(int id, short? languageId = null) =>
        Error.NotFound("PlatformRole.NotFound", NotFoundMessage("rol", "рол", "роль", id, languageId));

    public static Error UserKindNotFound(short id, short? languageId = null) =>
        Error.NotFound("PlatformUserKind.NotFound", NotFoundMessage("foydalanuvchi turi", "фойдаланувчи тури", "тип пользователя", id, languageId));

    public static Error OrganizationNotFound(int id, short? languageId = null) =>
        Error.NotFound("PlatformOrganization.NotFound", NotFoundMessage("tashkilot", "ташкилот", "организация", id, languageId));

    public static Error OrganizationInnConflict(string inn, short? languageId = null) =>
        Error.Conflict("PlatformOrganization.InnConflict", Message(languageId,
            $"STIR '{inn}' bo'lgan tashkilot allaqachon mavjud.", $"СТИР '{inn}' бўлган ташкилот аллақачон мавжуд.",
            $"Организация с ИНН '{inn}' уже существует.", $"Organization with INN '{inn}' already exists."));

    public static Error UserOrganizationConflict(int userId, int organizationId, short? languageId = null) =>
        Error.Conflict("PlatformUserOrganization.Conflict", Message(languageId,
            $"{userId}-foydalanuvchi {organizationId}-tashkilotga allaqachon biriktirilgan.",
            $"{userId}-фойдаланувчи {organizationId}-ташкилотга аллақачон бириктирилган.",
            $"Пользователь {userId} уже привязан к организации {organizationId}.",
            $"User {userId} is already attached to organization {organizationId}."));

    public static Error UserOrganizationNotFound(int userId, int organizationId, short? languageId = null) =>
        Error.NotFound("PlatformUserOrganization.NotFound", Message(languageId,
            $"{userId}-foydalanuvchi {organizationId}-tashkilotga biriktirilmagan.",
            $"{userId}-фойдаланувчи {organizationId}-ташкилотга бириктирилмаган.",
            $"Пользователь {userId} не привязан к организации {organizationId}.",
            $"User {userId} is not attached to organization {organizationId}."));

    public static Error InvalidInventoryValuationMethod(string method, short? languageId = null) =>
        Error.Business("Platform.InvalidInventoryValuationMethod", Message(languageId,
            $"'{method}' zaxiralarni baholash usuli qo'llab-quvvatlanmaydi.",
            $"'{method}' захираларни баҳолаш усули қўллаб-қувватланмайди.",
            $"Метод оценки запасов '{method}' не поддерживается.",
            $"Inventory valuation method '{method}' is not supported."));

    private static string NotFoundMessage(string uzName, string uzCyrlName, string ruName, int id, short? languageId) =>
        Message(languageId, $"Id-si {id} bo'lgan {uzName} topilmadi.", $"Id-си {id} бўлган {uzCyrlName} топилмади.",
            $"{ruName} с id {id} не найден.", $"Entity with id '{id}' was not found.");

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
