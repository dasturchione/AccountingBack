using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public static class MoneyRegisterBalanceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("MoneyRegisterBalance.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan pul registri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган пул регистри топилмади.",
            LanguageIdConst.RU      => $"Денежный регистр с id {id} не найден.",
            _                       => $"Money register balance with id {id} was not found."
        });
}
