using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PricingConditions;

public static class PricingConditionErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PricingCondition.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan narxlash sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => $"ID рақами {id} бўлган нархлаш шарти топилмади.",
            LanguageIdConst.RU => $"Условие ценообразования с id {id} не найдено.",
            _ => $"Pricing condition with id {id} was not found."
        });

    public static Error CurrentNotFound(short? languageId = null) =>
        Error.NotFound("PricingCondition.CurrentNotFound", languageId switch
        {
            LanguageIdConst.UZ => "Hozirgi vaqt uchun narxlash sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => "Жорий вақт учун нархлаш шарти топилмади.",
            LanguageIdConst.RU => "Условие ценообразования для текущего времени не найдено.",
            _ => "Pricing condition for current time was not found."
        });
}
