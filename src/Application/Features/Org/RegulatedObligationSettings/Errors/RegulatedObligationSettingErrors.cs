using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.RegulatedObligationSettings;

public static class RegulatedObligationSettingErrors
{
    public static Error OrganizationRequired(short? languageId) =>
        Error.Validation("RegulatedObligationSetting.OrganizationRequired", Translate(
            languageId,
            "Tashkilot tanlanishi shart.",
            "Требуется выбрать организацию.",
            "Organization is required."));

    public static Error NotFound(int id, short? languageId) =>
        Error.NotFound("RegulatedObligationSetting.NotFound", Translate(
            languageId,
            $"Id-si {id} bo'lgan majburiyat sozlamasi topilmadi.",
            $"Настройка обязательства с id {id} не найдена.",
            $"Regulated obligation setting with id {id} was not found."));

    public static Error ObligationNotFound(short id, short? languageId) =>
        Error.NotFound("RegulatedObligationSetting.ObligationNotFound", Translate(
            languageId,
            $"Id-si {id} bo'lgan majburiyat topilmadi.",
            $"Обязательство с id {id} не найдено.",
            $"Regulated obligation with id {id} was not found."));

    public static Error ObligationNotFound(string code, short? languageId) =>
        Error.NotFound("RegulatedObligationSetting.ObligationNotFound", Translate(
            languageId,
            $"{code} kodli majburiyat topilmadi.",
            $"Обязательство с кодом {code} не найдено.",
            $"Regulated obligation with code {code} was not found."));

    public static Error PeriodicityNotFound(short id, short? languageId) =>
        Error.NotFound("RegulatedObligationSetting.PeriodicityNotFound", Translate(
            languageId,
            $"Id-si {id} bo'lgan davriylik topilmadi.",
            $"Периодичность с id {id} не найдена.",
            $"Periodicity with id {id} was not found."));

    public static Error ChartAccountNotFound(int id, short? languageId) =>
        Error.NotFound("RegulatedObligationSetting.ChartAccountNotFound", Translate(
            languageId,
            $"Id-si {id} bo'lgan buxgalteriya hisobvarag'i topilmadi.",
            $"Счёт бухгалтерского учёта с id {id} не найден.",
            $"Chart account with id {id} was not found."));

    public static Error InvalidPeriod(short? languageId) =>
        Error.Validation("RegulatedObligationSetting.InvalidPeriod", Translate(
            languageId,
            "Amal qilishning tugash sanasi boshlanish sanasidan oldin bo'lishi mumkin emas.",
            "Дата окончания действия не может быть раньше даты начала.",
            "Effective-to date cannot be earlier than effective-from date."));

    public static Error InvalidRate(short? languageId) =>
        Error.Validation("RegulatedObligationSetting.InvalidRate", Translate(
            languageId,
            "Stavka 0 dan 100 gacha bo'lishi kerak.",
            "Ставка должна находиться в диапазоне от 0 до 100.",
            "Rate must be between 0 and 100."));

    public static Error PeriodOverlap(short? languageId) =>
        Error.Conflict("RegulatedObligationSetting.PeriodOverlap", Translate(
            languageId,
            "Ushbu majburiyat uchun amal qilish davrlari kesishmasligi kerak.",
            "Периоды действия настроек одного обязательства не должны пересекаться.",
            "Effective periods for the same obligation must not overlap."));

    private static string Translate(short? languageId, string uz, string ru, string en) =>
        languageId switch
        {
            LanguageIdConst.UZ => uz,
            LanguageIdConst.UZ_CYRL => uz,
            LanguageIdConst.RU => ru,
            _ => en
        };
}
