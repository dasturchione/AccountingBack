using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.OrgBankAccounts;

public static class OrgBankAccountErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("OrgBankAccount.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tashkilot bank hisobi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ташкилот банк ҳисоби топилмади.",
            LanguageIdConst.RU      => $"Банковский счёт организации с id {id} не найден.",
            _                       => $"Organization bank account with id {id} was not found."
        });

    public static Error AccountNumberConflict(string accountNumber, short? languageId = null) =>
        Error.Conflict("OrgBankAccount.AccountNumberConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Hisob raqami '{accountNumber}' bo'lgan bank hisobi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ҳисоб рақами '{accountNumber}' бўлган банк ҳисоби аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Банковский счёт с номером '{accountNumber}' уже существует.",
            _                       => $"Bank account with number '{accountNumber}' already exists."
        });

    public static Error BankBranchMismatch(int bankId, int bankBranchId, short? languageId = null) =>
        Error.Business("OrgBankAccount.BankBranchMismatch", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {bankBranchId} bo'lgan bank filiali id-si {bankId} bo'lgan bankka tegishli emas yoki faol emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {bankBranchId} бўлган банк филиали id-си {bankId} бўлган банкка тегишли эмас ёки фаол эмас.",
            LanguageIdConst.RU      => $"Филиал банка с id {bankBranchId} не относится к банку с id {bankId} или неактивен.",
            _                       => $"Bank branch {bankBranchId} does not belong to bank {bankId} or is inactive."
        });
}
