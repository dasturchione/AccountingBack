using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Integration.Edo.UnifiedImport;

public static class EdoUnifiedImportErrors
{
    public static Error ActiveProviderRequired(short? languageId = null) => Business("EDO_UNIFIED_ACTIVE_PROVIDER_REQUIRED", languageId,
        "Birlashtirilgan import uchun faol EDO provayderi EDOCS bo'lishi kerak.", "Бирлаштирилган импорт учун фаол EDO провайдери EDOCS бўлиши керак.",
        "Для объединённого импорта активным провайдером EDO должен быть EDOCS.", "The active EDO provider for unified import must be EDOCS.");

    public static Error ConfirmationRequired(short? languageId = null) => Business("EDO_UNIFIED_CONFIRM_REQUIRED", languageId,
        "Amalni aniq tasdiqlash kerak.", "Амални аниқ тасдиқлаш керак.", "Необходимо явно подтвердить операцию.", "Explicit confirmation is required.");

    public static Error InvalidPlanHash(short? languageId = null) => Conflict("EDO_UNIFIED_PLAN_HASH_INVALID", languageId,
        "Haqiqiy reja xeshi ko'rsatilishi kerak.", "Ҳақиқий режа хеши кўрсатилиши керак.", "Необходимо указать корректный хеш плана.", "A valid plan hash is required.");

    public static Error InvalidItems(short? languageId = null) => Business("EDO_UNIFIED_ITEMS_INVALID", languageId,
        "Paketda bittadan yuztagacha hujjat bo'lishi kerak.", "Пакетда биттадан юзтагача ҳужжат бўлиши керак.",
        "Пакет должен содержать от одного до ста документов.", "The batch must contain between one and one hundred documents.");

    public static Error StalePlan(short? languageId = null) => Conflict("EDO_UNIFIED_STALE_PLAN", languageId,
        "Birlashtirilgan EDO import rejasi eskirgan.", "Бирлаштирилган EDO импорт режаси эскирган.",
        "План объединённого импорта EDO устарел.", "The unified EDO import plan is stale.");

    public static Error DocumentNotInPlan(short? languageId = null) => Business("EDO_UNIFIED_DOCUMENT_NOT_IN_PLAN", languageId,
        "Hujjat joriy tashkilot rejasiga kirmaydi.", "Ҳужжат жорий ташкилот режасига кирмайди.",
        "Документ не входит в план текущей организации.", "The document is not part of the current organization plan.");

    public static Error BatchInitializationFailed(short? languageId = null) => Conflict("EDO_UNIFIED_BATCH_INITIALIZATION_FAILED", languageId,
        "Birlashtirilgan EDO import paketini yaratib bo'lmadi.", "Бирлаштирилган EDO импорт пакетини яратиб бўлмади.",
        "Не удалось инициализировать пакет объединённого импорта EDO.", "The unified EDO batch could not be initialized.");

    public static Error BatchNotFound(bool latest = false, short? languageId = null) =>
        Error.NotFound("EDO_UNIFIED_BATCH_NOT_FOUND", Message(languageId,
            latest ? "Birorta birlashtirilgan EDO import paketi topilmadi." : "Birlashtirilgan EDO import paketi topilmadi.",
            latest ? "Бирорта бирлаштирилган EDO импорт пакети топилмади." : "Бирлаштирилган EDO импорт пакети топилмади.",
            latest ? "Пакет объединённого импорта EDO не найден." : "Указанный пакет объединённого импорта EDO не найден.",
            latest ? "No unified EDO import batch was found." : "The unified EDO import batch was not found."));

    public static Error StatusRefreshFailed(short? languageId = null) =>
        Error.Problem("EDO_UNIFIED_STATUS_REFRESH_FAILED", Message(languageId,
            "Birlashtirilgan EDO holatini xavfsiz yangilab bo'lmadi.", "Бирлаштирилган EDO ҳолатини хавфсиз янгилаб бўлмади.",
            "Не удалось безопасно обновить статус объединённого импорта EDO.", "The unified EDO status refresh failed safely."));

    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));

    private static Error Conflict(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Conflict(code, Message(languageId, uz, uzCyrl, ru, en));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
