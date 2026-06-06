using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyBankAccounts;

public static class CounterpartyBankAccountErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CounterpartyBankAccount.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan kontragent bank hisobi topilmadi.",
            LanguageIdConst.RU => $"Банковский счёт контрагента с id {id} не найден.",
            _ => $"Counterparty bank account with id {id} was not found."
        });

    public static Error AccountNumberConflict(string accountNumber, short? languageId = null) =>
        Error.Conflict("CounterpartyBankAccount.AccountNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Hisob raqami '{accountNumber}' allaqachon mavjud.",
            LanguageIdConst.RU => $"Счёт с номером '{accountNumber}' уже существует.",
            _ => $"Account number '{accountNumber}' already exists."
        });
}
