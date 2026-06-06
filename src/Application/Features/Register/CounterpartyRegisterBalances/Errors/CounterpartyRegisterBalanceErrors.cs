using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public static class CounterpartyRegisterBalanceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("CounterpartyRegisterBalance.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan kontragent balansi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган контрагент баланси топилмади.",
            LanguageIdConst.RU      => $"Баланс контрагента с id {id} не найден.",
            _                       => $"Counterparty register balance with id {id} was not found."
        });
}
