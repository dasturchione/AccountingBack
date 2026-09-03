using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashDocuments;

public static class CashBookErrors
{
    public static Error CashBoxRequired(short? languageId = null) =>
        Error.Business("CashBook.CashBoxRequired", languageId switch
        {
            LanguageIdConst.UZ => "Kassa tanlanishi kerak.",
            LanguageIdConst.UZ_CYRL => "Касса танланиши керак.",
            LanguageIdConst.RU => "Необходимо выбрать кассу.",
            _ => "Cash box is required."
        });

    public static Error InvalidPagination(short? languageId = null) =>
        Error.Business("CashBook.InvalidPagination", languageId switch
        {
            LanguageIdConst.UZ => "Sahifalash parametrlari noto'g'ri.",
            LanguageIdConst.UZ_CYRL => "Саҳифалаш параметрлари нотўғри.",
            LanguageIdConst.RU => "Параметры пагинации указаны неверно.",
            _ => "Invalid pagination parameters."
        });

    public static Error InvalidDateRange(short? languageId = null) =>
        Error.Business("CashBook.InvalidDateRange", languageId switch
        {
            LanguageIdConst.UZ => "Boshlanish sanasi tugash sanasidan kech bo'lmasligi kerak.",
            LanguageIdConst.UZ_CYRL => "Бошланиш санаси тугаш санасидан кеч бўлмаслиги керак.",
            LanguageIdConst.RU => "Дата начала не должна быть позже даты окончания.",
            _ => "DateFrom must be earlier than or equal to DateTo."
        });

    public static Error CashBoxNotFound(int cashBoxId, short? languageId = null) =>
        Error.NotFound("CashBook.CashBoxNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {cashBoxId} bo'lgan kassa topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {cashBoxId} бўлган касса топилмади.",
            LanguageIdConst.RU => $"Касса с id {cashBoxId} не найдена.",
            _ => $"Cash box {cashBoxId} was not found."
        });
}
