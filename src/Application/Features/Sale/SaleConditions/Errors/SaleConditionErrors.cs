using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleConditions;

public static class SaleConditionErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("SaleCondition.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Sale condition with id {id} was not found.",
            LanguageIdConst.RU => $"Sale condition with id {id} was not found.",
            _ => $"Sale condition with id {id} was not found."
        });

    public static Error CurrentNotFound(short? languageId = null) =>
        Error.NotFound("SaleCondition.CurrentNotFound", languageId switch
        {
            LanguageIdConst.UZ => "Hozirgi vaqt uchun sotuv sharti topilmadi.",
            LanguageIdConst.UZ_CYRL => "Sale condition for current time was not found.",
            LanguageIdConst.RU => "Sale condition for current time was not found.",
            _ => "Sale condition for current time was not found."
        });
}
