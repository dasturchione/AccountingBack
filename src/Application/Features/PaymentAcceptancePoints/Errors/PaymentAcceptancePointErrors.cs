using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePoints;

public static class PaymentAcceptancePointErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePoint.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan payment acceptance pointi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган тўлов қабул қилиш нуқтаси топилмади.",
            LanguageIdConst.RU => $"Точка приёма платежей с id {id} не найдена.",
            _ => $"Payment acceptance point with id {id} was not found."
        });

    public static Error OrganizationRequired(short? languageId = null) =>
        Error.Business("PaymentAcceptancePoint.OrganizationRequired", languageId switch
        {
            LanguageIdConst.UZ => "Payment acceptance pointini yaratish uchun tashkilotni tanlang.",
            LanguageIdConst.UZ_CYRL => "Тўлов қабул қилиш нуқтасини яратиш учун ташкилотни танланг.",
            LanguageIdConst.RU => "Для создания точки приёма платежей выберите организацию.",
            _ => "Select an organization before creating a payment acceptance point."
        });

    public static Error BankAccountNotFound(int id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePoint.BankAccountNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan tashkilotning bank hisobi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ташкилотнинг банк ҳисоби топилмади.",
            LanguageIdConst.RU => $"Банковский счёт организации с id {id} не найден.",
            _ => $"Organization bank account with id {id} was not found."
        });

    public static Error TypeNotFound(short id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePoint.TypeNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan to'lov qabul qilish nuqtasi turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган тўлов қабул қилиш нуқтаси тури топилмади.",
            LanguageIdConst.RU => $"Тип точки приёма платежей с id {id} не найден.",
            _ => $"Payment acceptance point type with id {id} was not found."
        });

    public static Error ExternalIdConflict(string externalId, short? languageId = null) =>
        Error.Conflict("PaymentAcceptancePoint.ExternalIdConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Tashqi ID '{externalId}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ташқи ID '{externalId}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Внешний ID '{externalId}' уже существует в этой организации.",
            _ => $"External ID '{externalId}' already exists in this organization."
        });

    public static Error SerialNumberConflict(string serialNumber, short? languageId = null) =>
        Error.Conflict("PaymentAcceptancePoint.SerialNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Seriya raqami '{serialNumber}' ushbu tashkilotda allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Серия рақами '{serialNumber}' ушбу ташкилотда аллақачон мавжуд.",
            LanguageIdConst.RU => $"Серийный номер '{serialNumber}' уже существует в этой организации.",
            _ => $"Serial number '{serialNumber}' already exists in this organization."
        });
}
