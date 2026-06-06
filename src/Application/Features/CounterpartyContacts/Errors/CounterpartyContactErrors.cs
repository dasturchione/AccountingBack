using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyContacts;

public static class CounterpartyContactErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CounterpartyContact.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan kontragent kontakti topilmadi.",
            LanguageIdConst.RU => $"Контакт контрагента с id {id} не найден.",
            _ => $"Counterparty contact with id {id} was not found."
        });
}
