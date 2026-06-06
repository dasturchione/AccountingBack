using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyCards;

public static class CounterpartyCardErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CounterpartyCard.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan kontragent topilmadi.",
            LanguageIdConst.RU => $"Контрагент с id {id} не найден.",
            _ => $"Counterparty with id {id} was not found."
        });

    public static Error ShortNameConflict(string shortName, short? languageId = null) =>
        Error.Conflict("CounterpartyCard.ShortNameConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Qisqa nomi '{shortName}' bo'lgan kontragent allaqachon mavjud.",
            LanguageIdConst.RU => $"Контрагент с кратким наименованием '{shortName}' уже существует.",
            _ => $"Counterparty with short name '{shortName}' already exists."
        });
}
