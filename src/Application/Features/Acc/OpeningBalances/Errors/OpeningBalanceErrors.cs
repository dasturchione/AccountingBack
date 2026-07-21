using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Acc.OpeningBalances;

public static class OpeningBalanceErrors
{
    public static Error NotFound(short? languageId = null) =>
        Error.NotFound("OpeningBalance.NotFound", Message(languageId, "Boshlang'ich qoldiq topilmadi.", "Начальный остаток не найден.", "Opening balance was not found."));

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("OpeningBalance.NotFound", Message(languageId, $"Id-si {id} bo'lgan boshlang'ich qoldiq topilmadi.", $"Начальный остаток с id {id} не найден.", $"Opening balance with id {id} was not found."));

    public static Error AlreadyExists(short? languageId = null) =>
        Error.Conflict("OpeningBalance.AlreadyExists", Message(languageId, "Tashkilot uchun boshlang'ich qoldiq allaqachon yaratilgan.", "Для организации уже создан начальный остаток.", "An opening balance already exists for the organization."));

    public static Error AccountNotFound(long id, short? languageId = null) =>
        Error.NotFound("OpeningBalance.AccountNotFound", Message(languageId, $"Id-si {id} bo'lgan boshlang'ich qoldiq hisobi topilmadi.", $"Счёт начального остатка с id {id} не найден.", $"Opening balance account with id {id} was not found."));

    public static Error ChartAccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("OpeningBalance.ChartAccountNotFound", Message(languageId, $"Id-si {id} bo'lgan hisob topilmadi.", $"Счёт с id {id} не найден.", $"Chart account with id {id} was not found."));

    public static Error ChartAccountIsGroup(int id, short? languageId = null) =>
        Error.Business("OpeningBalance.ChartAccountIsGroup", Message(languageId, $"Id-si {id} bo'lgan guruh hisobiga qoldiq kiritib bo'lmaydi.", $"Нельзя ввести остаток по группе счетов с id {id}.", $"An opening balance cannot be entered for group chart account {id}."));

    public static Error ChartAccountAlreadyAdded(int id, short? languageId = null) =>
        Error.Conflict("OpeningBalance.ChartAccountAlreadyAdded", Message(languageId, $"Id-si {id} bo'lgan hisob boshlang'ich qoldiqda allaqachon mavjud.", $"Счёт с id {id} уже добавлен в начальный остаток.", $"Chart account {id} is already added to the opening balance."));

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("OpeningBalance.CurrencyNotFound", Message(languageId, $"Id-si {id} bo'lgan faol valyuta topilmadi.", $"Активная валюта с id {id} не найдена.", $"Active currency with id {id} was not found."));

    public static Error DetailsRequired(short? languageId = null) =>
        Error.Business("OpeningBalance.DetailsRequired", Message(languageId, "Kamida bitta qoldiq satrini kiritish kerak.", "Необходимо указать хотя бы одну строку остатка.", "At least one opening balance detail is required."));

    public static Error DetailNotFound(long id, short? languageId = null) =>
        Error.NotFound("OpeningBalance.DetailNotFound", Message(languageId, $"Id-si {id} bo'lgan qoldiq satri topilmadi.", $"Строка остатка с id {id} не найдена.", $"Opening balance detail with id {id} was not found."));
    public static Error DuplicateDetail(long id, short? languageId = null) =>
        Error.Conflict("OpeningBalance.DuplicateDetail", Message(languageId, $"Id-si {id} bo'lgan qoldiq satri bir necha marta yuborildi.", $"Строка остатка с id {id} передана несколько раз.", $"Opening balance detail {id} was supplied more than once."));

    public static Error DuplicateSubkontoType(short? languageId = null) =>
        Error.Conflict("OpeningBalance.DuplicateSubkontoType", Message(languageId, "Bitta satrda subkonto turi takrorlanmasligi kerak.", "Тип субконто не должен повторяться в одной строке.", "A subkonto type cannot be repeated within one detail."));

    public static Error SubkontoConfigurationMismatch(int chartAccountId, short? languageId = null) =>
        Error.Business("OpeningBalance.SubkontoConfigurationMismatch", Message(languageId, $"Hisob {chartAccountId} uchun subkonto to'plami hisob sozlamasiga mos emas.", $"Набор субконто не соответствует настройке счёта {chartAccountId}.", $"The subkonto set does not match chart account {chartAccountId} configuration."));

    public static Error InvalidState(short stateId, short? languageId = null) =>
        Error.Business("OpeningBalance.InvalidState", Message(languageId, $"Holat {stateId} qo'llab-quvvatlanmaydi.", $"Статус {stateId} не поддерживается.", $"State {stateId} is not supported."));

    public static Error NotActive(long id, short? languageId = null) =>
        Error.Business("OpeningBalance.NotActive", Message(languageId, $"Id-si {id} bo'lgan boshlang'ich qoldiq faol emas.", $"Начальный остаток с id {id} не активен.", $"Opening balance {id} is not active."));

    private static string Message(short? languageId, string uz, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
