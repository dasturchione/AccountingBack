using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.TrialBalance;

public static class TrialBalanceErrors
{
    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("TrialBalance.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan valyuta topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган валюта топилмади.",
            LanguageIdConst.RU => $"Валюта с id {id} не найдена.",
            _ => $"Currency with id {id} was not found."
        });

    public static Error PeriodNotFound(int id, short? languageId = null) =>
        Error.NotFound("TrialBalance.PeriodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan hisob davri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ҳисоб даври топилмади.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("TrialBalance.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "DateFrom DateTo dan katta bo'lishi mumkin emas.",
            LanguageIdConst.UZ_CYRL => "DateFrom DateTo дан катта бўлиши мумкин эмас.",
            LanguageIdConst.RU => "Дата начала не может быть больше даты окончания.",
            _ => "DateFrom cannot be greater than DateTo."
        });

    public static Error DateRangeOutsidePeriod(int periodId, short? languageId = null) =>
        Error.Business("TrialBalance.DateRangeOutsidePeriod", languageId switch
        {
            LanguageIdConst.UZ => $"Tanlangan sana oralig'i {periodId} davr chegarasidan tashqariga chiqmoqda.",
            LanguageIdConst.UZ_CYRL => $"Танланган сана оралиғи {periodId} давр чегарасидан ташқарига чиқмоқда.",
            LanguageIdConst.RU => $"Выбранный диапазон дат выходит за границы периода {periodId}.",
            _ => $"Selected date range falls outside accounting period {periodId}."
        });
}
