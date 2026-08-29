using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public static class ProviderContractReconciliationErrors
{
    public static Error ConfirmationRequired(short? languageId = null) => Conflict("EDO_CONTRACT_CONFIRMATION_REQUIRED", languageId,
        "Shartnomani solishtirishni aniq tasdiqlash kerak.", "Шартномани солиштиришни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить сверку договора.", "Explicit contract reconciliation confirmation is required.");

    public static Error CounterpartyRequired(short? languageId = null) => Validation("EDO_CONTRACT_COUNTERPARTY_REQUIRED", languageId,
        "Haqiqiy kontragent tanlanishi kerak.", "Ҳақиқий контрагент танланиши керак.",
        "Необходимо выбрать корректного контрагента.", "A valid counterparty is required.");

    public static Error ProviderInvalid(short? languageId = null) => Validation("EDO_CONTRACT_PROVIDER_INVALID", languageId,
        "Bu endpoint faqat faol EDOCS provayder identifikatorini qabul qiladi.", "Бу endpoint фақат фаол EDOCS провайдер идентификаторини қабул қилади.",
        "Этот endpoint принимает только идентификатор активного провайдера EDOCS.", "Only the active EDOCS provider identity is accepted by this endpoint.");

    public static Error ProviderNumberInvalid(short? languageId = null) => Validation("EDO_CONTRACT_PROVIDER_NUMBER_INVALID", languageId,
        "Provayder shartnoma raqami bo'sh bo'lmasligi kerak.", "Провайдер шартнома рақами бўш бўлмаслиги керак.",
        "Номер договора провайдера не может быть пустым.", "A non-empty provider contract number is required.");

    public static Error ProviderDateRequired(short? languageId = null) => Validation("EDO_CONTRACT_PROVIDER_DATE_REQUIRED", languageId,
        "Provayder shartnoma sanasi ko'rsatilishi kerak.", "Провайдер шартнома санаси кўрсатилиши керак.",
        "Необходимо указать дату договора провайдера.", "A provider contract date is required.");

    public static Error HeaderRequired(short? languageId = null) => Validation("EDO_CONTRACT_HEADER_REQUIRED", languageId,
        "Shartnoma turi va sanasi ko'rsatilishi kerak.", "Шартнома тури ва санаси кўрсатилиши керак.",
        "Необходимо указать тип и дату договора.", "Contract type and contract date are required.");

    public static Error DateIntervalInvalid(short? languageId = null) => Validation("EDO_CONTRACT_DATE_INTERVAL_INVALID", languageId,
        "Shartnoma sanalari oralig'i noto'g'ri.", "Шартнома саналари оралиғи нотўғри.",
        "Интервал дат договора указан неверно.", "The contract date interval is invalid.");

    public static Error CommentInvalid(short? languageId = null) => Validation("EDO_CONTRACT_COMMENT_INVALID", languageId,
        "Shartnoma izohi juda uzun.", "Шартнома изоҳи жуда узун.",
        "Комментарий к договору слишком длинный.", "The contract comment is too long.");

    public static Error CounterpartyNotFound(short? languageId = null) =>
        Error.NotFound("EDO_CONTRACT_COUNTERPARTY_NOT_FOUND", Message(languageId,
            "Kontragent joriy tashkilotda mavjud emas.", "Контрагент жорий ташкилотда мавжуд эмас.",
            "Контрагент недоступен в текущей организации.", "The counterparty is not available in the current organization."));

    public static Error ProviderIdentityInactive(short? languageId = null) => Conflict("EDO_CONTRACT_PROVIDER_IDENTITY_INACTIVE", languageId,
        "Faol bo'lmagan shartnoma ushbu provayder identifikatoriga ega; avval uni qayta faollashtiring.",
        "Фаол бўлмаган шартнома ушбу провайдер идентификаторига эга; аввал уни қайта фаоллаштиринг.",
        "Этот идентификатор провайдера уже принадлежит неактивному договору; сначала явно активируйте его.",
        "An inactive contract already owns this provider identity; reactivate it explicitly before reconciliation.");

    public static Error ProviderIdentityExists(short? languageId = null) => Conflict("EDO_CONTRACT_PROVIDER_IDENTITY_EXISTS", languageId,
        "Provayder shartnoma identifikatori boshqa shartnomaga tegishli.", "Провайдер шартнома идентификатори бошқа шартномага тегишли.",
        "Идентификатор договора провайдера уже принадлежит другому договору.",
        "The provider contract identity is already owned by another contract. Retry the same request to read its safe existing result.");

    private static Error Validation(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Validation(code, Message(languageId, uz, uzCyrl, ru, en));

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
