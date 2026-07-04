using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Ledger;

public static class LedgerErrors
{
    public static Error AccountRequired(short? languageId = null) =>
        Error.Business("Ledger.AccountRequired", languageId switch
        {
            LanguageIdConst.UZ => "Ledger uchun hisob tanlanishi shart.",
            LanguageIdConst.UZ_CYRL => "Ð›ÐµÐ´Ð¶ÐµÑ€ ÑƒÑ‡ÑƒÐ½ Ò³Ð¸ÑÐ¾Ð± Ñ‚Ð°Ð½Ð»Ð°Ð½Ð¸ÑˆÐ¸ ÑˆÐ°Ñ€Ñ‚.",
            LanguageIdConst.RU => "Для ledger необходимо выбрать счёт.",
            _ => "Account is required for ledger."
        });

    public static Error AccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("Ledger.AccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-ÑÐ¸ {id} Ð±ÑžÐ»Ð³Ð°Ð½ Ò³Ð¸ÑÐ¾Ð± Ñ‚Ð¾Ð¿Ð¸Ð»Ð¼Ð°Ð´Ð¸.",
            LanguageIdConst.RU => $"Счёт с id {id} не найден.",
            _ => $"Account with id {id} was not found."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("Ledger.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-ÑÐ¸ {id} Ð±ÑžÐ»Ð³Ð°Ð½ Ð²Ð°Ð»ÑŽÑ‚Ð° Ñ‚Ð¾Ð¿Ð¸Ð»Ð¼Ð°Ð´Ð¸.",
            LanguageIdConst.RU => $"Валюта с id {id} не найдена.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error PeriodNotFound(int id, short? languageId = null) =>
        Error.NotFound("Ledger.PeriodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob davri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-ÑÐ¸ {id} Ð±ÑžÐ»Ð³Ð°Ð½ Ò³Ð¸ÑÐ¾Ð± Ð´Ð°Ð²Ñ€Ð¸ Ñ‚Ð¾Ð¿Ð¸Ð»Ð¼Ð°Ð´Ð¸.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("Ledger.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "DateFrom DateTo dan katta bo'lishi mumkin emas.",
            LanguageIdConst.UZ_CYRL => "DateFrom DateTo Ð´Ð°Ð½ ÐºÐ°Ñ‚Ñ‚Ð° Ð±ÑžÐ»Ð¸ÑˆÐ¸ Ð¼ÑƒÐ¼ÐºÐ¸Ð½ ÑÐ¼Ð°Ñ.",
            LanguageIdConst.RU => "Дата начала не может быть больше даты окончания.",
            _ => "DateFrom cannot be greater than DateTo."
        });

    public static Error InvalidPagination(short? languageId = null) =>
        Error.Business("Ledger.InvalidPagination", languageId switch
        {
            LanguageIdConst.UZ => "Page va PageSize musbat qiymat bo'lishi kerak.",
            LanguageIdConst.UZ_CYRL => "Page Ð²Ð° PageSize Ð¼ÑƒÑÐ±Ð°Ñ‚ Ò›Ð¸Ð¹Ð¼Ð°Ñ‚ Ð±ÑžÐ»Ð¸ÑˆÐ¸ ÐºÐµÑ€Ð°Ðº.",
            LanguageIdConst.RU => "Page и PageSize должны быть положительными значениями.",
            _ => "Page and PageSize must be positive values."
        });

    public static Error DateRangeOutsidePeriod(int periodId, short? languageId = null) =>
        Error.Business("Ledger.DateRangeOutsidePeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Tanlangan sana oralig'i {periodId} davr chegarasidan tashqariga chiqmoqda.",
            LanguageIdConst.UZ_CYRL => $"Ð¢Ð°Ð½Ð»Ð°Ð½Ð³Ð°Ð½ ÑÐ°Ð½Ð° Ð¾Ñ€Ð°Ð»Ð¸Ò“Ð¸ {periodId} Ð´Ð°Ð²Ñ€ Ñ‡ÐµÐ³Ð°Ñ€Ð°ÑÐ¸Ð´Ð°Ð½ Ñ‚Ð°ÑˆÒ›Ð°Ñ€Ð¸Ð³Ð° Ñ‡Ð¸Ò›Ð¼Ð¾Ò›Ð´Ð°.",
            LanguageIdConst.RU => $"Выбранный диапазон дат выходит за границы периода {periodId}.",
            _ => $"Selected date range falls outside accounting period {periodId}."
        });
}
