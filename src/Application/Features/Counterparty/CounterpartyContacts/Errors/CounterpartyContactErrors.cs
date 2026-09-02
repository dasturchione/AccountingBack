using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyContacts;

public static class CounterpartyContactErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CounterpartyContact.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan kontragent kontakti topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган контрагент контакти топилмади.",
            LanguageIdConst.RU      => $"Контакт контрагента с id {id} не найден.",
            _                       => $"Counterparty contact with id {id} was not found."
        });

    public static Error CounterpartyNotFound(int counterpartyId, short? languageId = null) =>
        Error.NotFound("CounterpartyContact.CounterpartyNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {counterpartyId} bo'lgan kontragent joriy tashkilotda topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {counterpartyId} бўлган контрагент жорий ташкилотда топилмади.",
            LanguageIdConst.RU      => $"Контрагент с id {counterpartyId} не найден в текущей организации.",
            _                       => $"Counterparty with id {counterpartyId} was not found in the current organization."
        });
}
