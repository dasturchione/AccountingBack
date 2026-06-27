using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PricingConditions;

public static class PricingConditionErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PricingCondition.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan narxlash sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Pricing condition with id {id} was not found.",
            LanguageIdConst.RU => $"Pricing condition with id {id} was not found.",
            _ => $"Pricing condition with id {id} was not found."
        });

    public static Error CurrentNotFound(short? languageId = null) =>
        Error.NotFound("PricingCondition.CurrentNotFound", languageId switch
        {
            LanguageIdConst.UZ => "Hozirgi vaqt uchun narxlash sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => "Pricing condition for current time was not found.",
            LanguageIdConst.RU => "Pricing condition for current time was not found.",
            _ => "Pricing condition for current time was not found."
        });
}
