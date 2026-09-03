using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public static class EdoSaleDraftErrors
{
    public static Error HistoricalNumberRequired(short? languageId = null) =>
        Error.Business("HISTORICAL_SALE_NUMBER_REQUIRED", Message(languageId,
            "Tarixiy sotuv hujjati uchun raqam yaratib bo'lmadi.", "Тарихий сотув ҳужжати учун рақам яратиб бўлмади.",
            "Не удалось сформировать номер исторического документа продажи.", "Historical sale document numbering is unavailable."));

    public static Error StalePlan(string code, short? languageId = null) =>
        Error.Conflict(code, Message(languageId,
            "EDO sotuv qoralamasi rejasi eskirgan yoki endi qo'llanilmaydi.", "EDO сотув қораламаси режаси эскирган ёки энди қўлланилмайди.",
            "План черновика продажи EDO устарел или больше неприменим.", "The EDO sale draft plan is stale or no longer applicable."));

    public static Error InvalidSelection(string code, short? languageId = null) =>
        Error.Business(code, Message(languageId,
            "EDO sotuv qoralamasi uchun tanlov noto'g'ri.", "EDO сотув қораламаси учун танлов нотўғри.",
            "Выбор для черновика продажи EDO некорректен.", "The EDO sale draft selection is invalid."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
