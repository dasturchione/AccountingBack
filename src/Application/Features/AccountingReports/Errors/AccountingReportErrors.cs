using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.AccountingReports;

public static class AccountingReportErrors
{
    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("AccountingReport.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "DateFrom DateTo dan katta bo'lishi mumkin emas.",
            LanguageIdConst.UZ_CYRL => "DateFrom DateTo дан катта бўлиши мумкин эмас.",
            LanguageIdConst.RU => "Дата начала не может быть больше даты окончания.",
            _ => "DateFrom cannot be greater than DateTo."
        });

    public static Error InvalidPagination(short? languageId = null) =>
        Error.Business("AccountingReport.InvalidPagination", languageId switch
        {
            LanguageIdConst.UZ => "Page va PageSize 0 dan katta bo'lishi kerak.",
            LanguageIdConst.UZ_CYRL => "Page ва PageSize 0 дан катта бўлиши керак.",
            LanguageIdConst.RU => "Page и PageSize должны быть больше 0.",
            _ => "Page and PageSize must be greater than 0."
        });

    public static Error PeriodNotFound(int id, short? languageId = null) =>
        Error.NotFound("AccountingReport.PeriodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob davri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб даври топилмади.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error DateRangeOutsidePeriod(int periodId, short? languageId = null) =>
        Error.Business("AccountingReport.DateRangeOutsidePeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Tanlangan sana oralig'i {periodId} davr chegarasidan tashqariga chiqmoqda.",
            LanguageIdConst.UZ_CYRL => $"Танланган сана оралиғи {periodId} давр чегарасидан ташқарига чиқмоқда.",
            LanguageIdConst.RU => $"Выбранный диапазон дат выходит за границы периода {periodId}.",
            _ => $"Selected date range falls outside accounting period {periodId}."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("AccountingReport.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган валюта топилмади.",
            LanguageIdConst.RU => $"Валюта с id {id} не найдена.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error AccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("AccountingReport.AccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб топилмади.",
            LanguageIdConst.RU => $"Счёт с id {id} не найден.",
            _ => $"Account with id {id} was not found."
        });
}
