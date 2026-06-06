using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public static class InventoryRegisterBalanceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("InventoryRegisterBalance.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan ombor registri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган омбор регистри топилмади.",
            LanguageIdConst.RU      => $"Складской регистр с id {id} не найден.",
            _                       => $"Inventory register balance with id {id} was not found."
        });
}
