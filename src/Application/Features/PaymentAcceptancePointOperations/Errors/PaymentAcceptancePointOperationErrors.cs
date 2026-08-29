using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public static class PaymentAcceptancePointOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan to'lov qabul qilish nuqtasi operatsiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган тўлов қабул қилиш нуқтаси операцияси топилмади.",
            LanguageIdConst.RU => $"Операция точки приёма платежей с id {id} не найдена.",
            _ => $"Payment acceptance point operation with id {id} was not found."
        });

    public static Error PointNotFound(int id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.PointNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan faol to'lov qabul qilish nuqtasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган фаол тўлов қабул қилиш нуқтаси топилмади.",
            LanguageIdConst.RU => $"Активная точка приёма платежей с id {id} не найдена.",
            _ => $"Active payment acceptance point with id {id} was not found."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan faol valyuta topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган фаол валюта топилмади.",
            LanguageIdConst.RU => $"Активная валюта с id {id} не найдена.",
            _ => $"Active currency with id {id} was not found."
        });

    public static Error OrganizationMismatch(long id, short? languageId = null) =>
        Error.Forbidden("PaymentAcceptancePointOperation.OrganizationMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"{id} operatsiya boshqa tashkilotga tegishli.",
            LanguageIdConst.UZ_CYRL => $"{id} операция бошқа ташкилотга тегишли.",
            LanguageIdConst.RU => $"Операция {id} принадлежит другой организации.",
            _ => $"Operation {id} belongs to another organization."
        });

    public static Error InvalidStatus(long id, short statusId, string action, short? languageId = null) =>
        Error.Business(
            "PaymentAcceptancePointOperation.InvalidStatus",
            Message(languageId,
                $"{id}-operatsiyani {statusId} holatida {Action(action, LanguageIdConst.UZ)} mumkin emas.",
                $"{id}-операцияни {statusId} ҳолатида {Action(action, LanguageIdConst.UZ_CYRL)} мумкин эмас.",
                $"Операцию {id} в статусе {statusId} нельзя {Action(action, LanguageIdConst.RU)}.",
                $"Operation {id} with status {statusId} cannot be {action}."));

    public static Error InvalidValues(short? languageId = null) => Error.Business(
        "PaymentAcceptancePointOperation.InvalidConfiguration", Message(languageId,
            "Yo'nalish IN yoki OUT bo'lishi, summa va valyuta kursi noldan katta bo'lishi kerak.",
            "Йўналиш IN ёки OUT бўлиши, сумма ва валюта курси нолдан катта бўлиши керак.",
            "Направление должно быть IN или OUT, а сумма и курс валюты — больше нуля.",
            "Direction must be IN or OUT; amount and exchange rate must be greater than zero."));

    public static Error PointInvalid(short? languageId = null) => Error.Business(
        "PaymentAcceptancePointOperation.InvalidConfiguration", Message(languageId,
            "To'lov qabul qilish nuqtasi faol emas yoki boshqa tashkilotga tegishli.",
            "Тўлов қабул қилиш нуқтаси фаол эмас ёки бошқа ташкилотга тегишли.",
            "Точка приёма платежей неактивна или относится к другой организации.",
            "Payment acceptance point is inactive or belongs to another organization."));

    public static Error InsufficientBalance(decimal available, decimal requested, short? languageId = null) =>
        Error.Business(
            "PaymentAcceptancePointOperation.InsufficientBalance",
            Message(languageId,
                $"To'lov qabul qilish nuqtasining mavjud qoldig'i {available}, chiqim summasi {requested}.",
                $"Тўлов қабул қилиш нуқтасининг мавжуд қолдиғи {available}, чиқим суммаси {requested}.",
                $"Доступный остаток точки приёма платежей: {available}; сумма расхода: {requested}.",
                $"Available payment acceptance point balance is {available}; requested outgoing amount is {requested}."));

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.BusinessEffectsAlreadyExist",
            Message(languageId, $"{id}-operatsiyada faol pul registri harakatlari mavjud.",
                $"{id}-операцияда фаол пул регистри ҳаракатлари мавжуд.",
                $"Операция {id} уже имеет активные движения денежного регистра.",
                $"Operation {id} already has active money register effects."));

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.MissingPostingBatch",
            Message(languageId, $"O'tkazilgan {id}-operatsiya uchun faol o'tkazmalar paketi topilmadi.",
                $"Ўтказилган {id}-операция учун фаол ўтказмалар пакети топилмади.",
                $"Для проведённой операции {id} не найден активный пакет проводок.",
                $"Posted operation {id} has no active posting batch."));

    public static Error MissingMoneyEntries(long id, short? languageId = null) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.MissingMoneyEntries",
            Message(languageId, $"O'tkazilgan {id}-operatsiyada bekor qilish uchun pul registri yozuvlari yo'q.",
                $"Ўтказилган {id}-операцияда бекор қилиш учун пул регистри ёзувлари йўқ.",
                $"У проведённой операции {id} нет записей денежного регистра для сторнирования.",
                $"Posted operation {id} has no money register entries to reverse."));

    private static string Action(string action, short languageId) => (action, languageId) switch
    {
        ("updated", LanguageIdConst.UZ) => "o'zgartirish",
        ("deleted", LanguageIdConst.UZ) => "o'chirish",
        ("confirmed", LanguageIdConst.UZ) => "tasdiqlash",
        ("cancelled", LanguageIdConst.UZ) => "bekor qilish",
        ("updated", LanguageIdConst.UZ_CYRL) => "ўзгартириш",
        ("deleted", LanguageIdConst.UZ_CYRL) => "ўчириш",
        ("confirmed", LanguageIdConst.UZ_CYRL) => "тасдиқлаш",
        ("cancelled", LanguageIdConst.UZ_CYRL) => "бекор қилиш",
        ("updated", LanguageIdConst.RU) => "изменить",
        ("deleted", LanguageIdConst.RU) => "удалить",
        ("confirmed", LanguageIdConst.RU) => "подтвердить",
        ("cancelled", LanguageIdConst.RU) => "отменить",
        _ => action
    };

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
