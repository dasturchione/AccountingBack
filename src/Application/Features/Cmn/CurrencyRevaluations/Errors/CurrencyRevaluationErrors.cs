using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRevaluations;

public static class CurrencyRevaluationErrors
{
    public static Error NotFound(long id, short languageId) => N("CurrencyRevaluation.NotFound", languageId,
        $"Id-si {id} bo'lgan valyuta qayta baholash hujjati topilmadi.", $"Id-си {id} бўлган валюта қайта баҳолаш ҳужжати топилмади.",
        $"Переоценка валюты с id {id} не найдена.", $"Currency revaluation with id '{id}' not found.");
    public static Error InvalidDate(short languageId) => B("CurrencyRevaluation.InvalidDate", languageId,
        "Qayta baholash sanasi noto'g'ri.", "Қайта баҳолаш санаси нотўғри.", "Недопустимая дата переоценки.", "Revaluation date is invalid.");
    public static Error NoOrganization(short languageId) => B("CurrencyRevaluation.NoOrganization", languageId,
        "Joriy foydalanuvchida tashkilot yo'q.", "Жорий фойдаланувчида ташкилот йўқ.", "У текущего пользователя не выбрана организация.", "Current user has no organization.");
    public static Error AlreadyConfirmed(long id, short languageId) => C("CurrencyRevaluation.AlreadyConfirmed", languageId,
        $"{id}-valyuta qayta baholash allaqachon tasdiqlangan.", $"{id}-валюта қайта баҳолаш аллақачон тасдиқланган.",
        $"Переоценка валюты {id} уже подтверждена.", $"Currency revaluation '{id}' is already confirmed.");
    public static Error AlreadyCancelled(long id, short languageId) => C("CurrencyRevaluation.AlreadyCancelled", languageId,
        $"{id}-valyuta qayta baholash allaqachon bekor qilingan.", $"{id}-валюта қайта баҳолаш аллақачон бекор қилинган.",
        $"Переоценка валюты {id} уже отменена.", $"Currency revaluation '{id}' is already cancelled.");
    public static Error DuplicateForDate(short languageId) => C("CurrencyRevaluation.DuplicateForDate", languageId,
        "Ko'rsatilgan sana uchun valyuta qayta baholash allaqachon mavjud.", "Кўрсатилган сана учун валюта қайта баҳолаш аллақачон мавжуд.",
        "Переоценка валюты на указанную дату уже существует.", "Currency revaluation for the specified date already exists.");
    public static Error NoRevaluationLines(short languageId) => B("CurrencyRevaluation.NoLines", languageId,
        "Valyuta qayta baholash uchun mos qatorlar topilmadi.", "Валюта қайта баҳолаш учун мос қаторлар топилмади.",
        "Переоценка валюты не сформировала подходящих строк.", "Currency revaluation produced no eligible lines.");
    public static Error MissingPostingBatch(long id, short languageId) => C("CurrencyRevaluation.MissingPostingBatch", languageId,
        $"{id}-valyuta qayta baholash uchun o'tkazmalar paketi topilmadi.", $"{id}-валюта қайта баҳолаш учун ўтказмалар пакети топилмади.",
        $"Для переоценки валюты {id} не найден пакет проводок.", $"Posting batch was not found for currency revaluation '{id}'.");
    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short languageId) => C("CurrencyRevaluation.CannotConfirmInCurrentStatus", languageId,
        $"{id}-valyuta qayta baholashni {statusId} holatida tasdiqlab bo'lmaydi.", $"{id}-валюта қайта баҳолашни {statusId} ҳолатида тасдиқлаб бўлмайди.",
        $"Переоценку валюты {id} нельзя подтвердить в статусе {statusId}.", $"Currency revaluation '{id}' cannot be confirmed in status '{statusId}'.");
    public static Error CannotCancelInCurrentStatus(long id, short statusId, short languageId) => C("CurrencyRevaluation.CannotCancelInCurrentStatus", languageId,
        $"{id}-valyuta qayta baholashni {statusId} holatida bekor qilib bo'lmaydi.", $"{id}-валюта қайта баҳолашни {statusId} ҳолатида бекор қилиб бўлмайди.",
        $"Переоценку валюты {id} нельзя отменить в статусе {statusId}.", $"Currency revaluation '{id}' cannot be cancelled in status '{statusId}'.");
    public static Error BusinessEffectsAlreadyExist(long id, short languageId) => C("CurrencyRevaluation.BusinessEffectsAlreadyExist", languageId,
        $"{id}-valyuta qayta baholashda buxgalteriya harakatlari allaqachon mavjud.", $"{id}-валюта қайта баҳолашда бухгалтерия ҳаракатлари аллақачон мавжуд.",
        $"Переоценка валюты {id} уже имеет бухгалтерские движения.", $"Currency revaluation '{id}' already has accounting effects.");
    public static Error MissingAccountingRegisterEntries(long id, short languageId) => C("CurrencyRevaluation.MissingAccountingRegisterEntries", languageId,
        $"{id}-valyuta qayta baholash uchun buxgalteriya registri yozuvlari topilmadi.", $"{id}-валюта қайта баҳолаш учун бухгалтерия регистри ёзувлари топилмади.",
        $"Для переоценки валюты {id} не найдены записи бухгалтерского регистра.", $"Accounting register entries were not found for currency revaluation '{id}'.");
    public static Error NoAccountingEntries(long id, short languageId) => C("CurrencyRevaluation.NoAccountingEntries", languageId,
        $"{id}-valyuta qayta baholashda buxgalteriya yozuvlari yo'q.", $"{id}-валюта қайта баҳолашда бухгалтерия ёзувлари йўқ.",
        $"У переоценки валюты {id} нет бухгалтерских проводок.", $"Currency revaluation '{id}' has no accounting entries.");

    private static Error B(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Business(code, M(languageId, uz, uzCyrl, ru, en));
    private static Error C(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Conflict(code, M(languageId, uz, uzCyrl, ru, en));
    private static Error N(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.NotFound(code, M(languageId, uz, uzCyrl, ru, en));
    private static string M(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
