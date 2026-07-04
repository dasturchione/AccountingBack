using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public static class AccountingPeriodErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("AccountingPeriod.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan accounting period topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган accounting period топилмади.",
            LanguageIdConst.RU => $"Учётный период с id {id} не найден.",
            _ => $"Accounting period with id {id} was not found."
        });

    public static Error AlreadyClosed(int id, short? languageId = null) =>
        Error.Conflict("AccountingPeriod.AlreadyClosed", languageId switch
        {
            LanguageIdConst.UZ => $"Accounting period {id} allaqachon yopilgan.",
            LanguageIdConst.UZ_CYRL => $"Accounting period {id} аллақачон ёпилган.",
            LanguageIdConst.RU => $"Учётный период {id} уже закрыт.",
            _ => $"Accounting period {id} is already closed."
        });

    public static Error NotClosed(int id, short? languageId = null) =>
        Error.Conflict("AccountingPeriod.NotClosed", languageId switch
        {
            LanguageIdConst.UZ => $"Accounting period {id} hali yopilmagan.",
            LanguageIdConst.UZ_CYRL => $"Accounting period {id} ҳали ёпилмаган.",
            LanguageIdConst.RU => $"Учётный период {id} ещё не закрыт.",
            _ => $"Accounting period {id} is not closed yet."
        });

    public static Error PreviousPeriodsMustBeClosed(short? languageId = null) =>
        Error.Business("AccountingPeriod.PreviousPeriodsMustBeClosed", languageId switch
        {
            LanguageIdConst.UZ => "Oldingi accounting periodlar yopilmasdan turib joriy periodni yopib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => "Олдинги accounting periodлар ёпилмасдан туриб жорий periodни ёпиб бўлмайди.",
            LanguageIdConst.RU => "Нельзя закрыть текущий период, пока предыдущие периоды не закрыты.",
            _ => "Current accounting period cannot be closed while previous periods are still open."
        });

    public static Error LaterClosedPeriodsExist(short? languageId = null) =>
        Error.Business("AccountingPeriod.LaterClosedPeriodsExist", languageId switch
        {
            LanguageIdConst.UZ => "Keyingi yopilgan periodlar mavjud bo'lsa, bu periodni qayta ochib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => "Кейинги ёпилган periodлар мавжуд бўлса, бу periodни қайта очиб бўлмайди.",
            LanguageIdConst.RU => "Нельзя переоткрыть период, если существуют более поздние закрытые периоды.",
            _ => "Period cannot be reopened while later closed periods exist."
        });

    public static Error DraftDocumentsExist(int count, short? languageId = null) =>
        Error.Business("AccountingPeriod.DraftDocumentsExist", languageId switch
        {
            LanguageIdConst.UZ => $"Period ichida {count} ta tasdiqlanmagan accounting document mavjud.",
            LanguageIdConst.UZ_CYRL => $"Period ичида {count} та тасдиқланмаган accounting document мавжуд.",
            LanguageIdConst.RU => $"В периоде найдено {count} неподтверждённых учётных документов.",
            _ => $"There are {count} unconfirmed accounting documents within the period."
        });

    public static Error InvalidPostingBatchState(short? languageId = null) =>
        Error.Conflict("AccountingPeriod.InvalidPostingBatchState", languageId switch
        {
            LanguageIdConst.UZ => "Posting batch holati noto'g'ri. Periodni yopishdan oldin accounting posting consistency tiklanishi kerak.",
            LanguageIdConst.UZ_CYRL => "Posting batch ҳолати нотўғри. Periodни ёпишдан олдин accounting posting consistency тикланиши керак.",
            LanguageIdConst.RU => "Состояние posting batch некорректно. Перед закрытием периода нужно восстановить согласованность проводок.",
            _ => "Posting batch state is invalid. Accounting posting consistency must be restored before closing the period."
        });
}
