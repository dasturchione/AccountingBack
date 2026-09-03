using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CounterpartyBankAccounts;

public static class CounterpartyBankAccountErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CounterpartyBankAccount.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan kontragent bank hisobi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган контрагент банк ҳисоби топилмади.",
            LanguageIdConst.RU      => $"Банковский счёт контрагента с id {id} не найден.",
            _                       => $"Counterparty bank account with id {id} was not found."
        });

    public static Error AccountNumberConflict(string accountNumber, short? languageId = null) =>
        Error.Conflict("CounterpartyBankAccount.AccountNumberConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Hisob raqami '{accountNumber}' allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ҳисоб рақами '{accountNumber}' аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Номер счёта '{accountNumber}' уже существует.",
            _                       => $"Account number '{accountNumber}' already exists."
        });

    public static Error BankBranchMismatch(int bankId, int bankBranchId, short? languageId = null) =>
        Error.Business("CounterpartyBankAccount.BankBranchMismatch", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {bankBranchId} bo'lgan bank filiali id-si {bankId} bo'lgan bankka tegishli emas yoki faol emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {bankBranchId} бўлган банк филиали id-си {bankId} бўлган банкка тегишли эмас ёки фаол эмас.",
            LanguageIdConst.RU      => $"Филиал банка с id {bankBranchId} не относится к банку с id {bankId} или неактивен.",
            _                       => $"Bank branch {bankBranchId} does not belong to bank {bankId} or is inactive."
        });

    public static Error CounterpartyNotFound(int counterpartyId, short? languageId = null) =>
        Error.NotFound("CounterpartyBankAccount.CounterpartyNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {counterpartyId} bo'lgan kontragent joriy tashkilotda topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {counterpartyId} бўлган контрагент жорий ташкилотда топилмади.",
            LanguageIdConst.RU      => $"Контрагент с id {counterpartyId} не найден в текущей организации.",
            _                       => $"Counterparty with id {counterpartyId} was not found in the current organization."
        });
}
