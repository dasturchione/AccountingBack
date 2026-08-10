using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankTerminals;

public static class BankTerminalErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("BankTerminal.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bank terminali topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган банк терминали топилмади.",
            LanguageIdConst.RU => $"Банковский терминал с id {id} не найден.",
            _ => $"Bank terminal with id {id} was not found."
        });

    public static Error OrganizationRequired(short? languageId = null) =>
        Error.Business("BankTerminal.OrganizationRequired", languageId switch
        {
            LanguageIdConst.UZ => "Bank terminalini yaratish uchun tashkilotni tanlang.",
            LanguageIdConst.UZ_CYRL => "Банк терминалини яратиш учун ташкилотни танланг.",
            LanguageIdConst.RU => "Для создания банковского терминала выберите организацию.",
            _ => "Select an organization before creating a bank terminal."
        });

    public static Error BankAccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("BankTerminal.BankAccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan tashkilotning bank hisobi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ташкилотнинг банк ҳисоби топилмади.",
            LanguageIdConst.RU => $"Банковский счёт организации с id {id} не найден.",
            _ => $"Organization bank account with id {id} was not found."
        });

    public static Error ExternalTerminalIdConflict(string externalTerminalId, short? languageId = null) =>
        Error.Conflict("BankTerminal.ExternalTerminalIdConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Terminal ID '{externalTerminalId}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Terminal ID '{externalTerminalId}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Terminal ID '{externalTerminalId}' уже существует в этой организации.",
            _ => $"Terminal ID '{externalTerminalId}' already exists in this organization."
        });

    public static Error SerialNumberConflict(string serialNumber, short? languageId = null) =>
        Error.Conflict("BankTerminal.SerialNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Seriya raqami '{serialNumber}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Серия рақами '{serialNumber}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Серийный номер '{serialNumber}' уже существует в этой организации.",
            _ => $"Serial number '{serialNumber}' already exists in this organization."
        });
}
