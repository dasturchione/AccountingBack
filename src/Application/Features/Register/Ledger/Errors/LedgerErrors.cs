using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Ledger;

public static class LedgerErrors
{
    public static Error AccountRequired(short? languageId = null) =>
        Error.Business("Ledger.AccountRequired", languageId switch
        {
            LanguageIdConst.UZ => "Ledger uchun hisob tanlanishi shart.",
            LanguageIdConst.UZ_CYRL => "Леджер учун ҳисоб танланиши шарт.",
            LanguageIdConst.RU => "Для главной книги необходимо выбрать счёт.",
            _ => "Account is required for ledger."
        });

    public static Error AccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("Ledger.AccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб топилмади.",
            LanguageIdConst.RU => $"Счёт с id {id} не найден.",
            _ => $"Account with id {id} was not found."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("Ledger.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган валюта топилмади.",
            LanguageIdConst.RU => $"Валюта с id {id} не найдена.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error PeriodNotFound(int id, short? languageId = null) =>
        Error.NotFound("Ledger.PeriodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob davri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб даври топилмади.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("Ledger.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "DateFrom DateTo dan katta bo'lishi mumkin emas.",
            LanguageIdConst.UZ_CYRL => "DateFrom DateTo дан катта бўлиши мумкин эмас.",
            LanguageIdConst.RU => "Дата начала не может быть больше даты окончания.",
            _ => "DateFrom cannot be greater than DateTo."
        });

    public static Error InvalidPagination(short? languageId = null) =>
        Error.Business("Ledger.InvalidPagination", languageId switch
        {
            LanguageIdConst.UZ => "Page va PageSize musbat qiymat bo'lishi kerak.",
            LanguageIdConst.UZ_CYRL => "Page ва PageSize мусбат қиймат бўлиши керак.",
            LanguageIdConst.RU => "Page и PageSize должны быть положительными значениями.",
            _ => "Page and PageSize must be positive values."
        });

    public static Error DateRangeOutsidePeriod(int periodId, short? languageId = null) =>
        Error.Business("Ledger.DateRangeOutsidePeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Tanlangan sana oralig'i {periodId} davr chegarasidan tashqariga chiqmoqda.",
            LanguageIdConst.UZ_CYRL => $"Танланган сана оралиғи {periodId} давр чегарасидан ташқарига чиқмоқда.",
            LanguageIdConst.RU => $"Выбранный диапазон дат выходит за границы периода {periodId}.",
            _ => $"Selected date range falls outside accounting period {periodId}."
        });
}
