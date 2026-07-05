using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Notifications;

public static class NotificationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Notification.NotFound", GetNotFoundDescription(id, languageId));

    public static Error TypeRequired(short? languageId = null) =>
        Error.Business("Notification.TypeRequired", GetTypeRequiredDescription(languageId));

    public static Error TypeNotFound(short id, short? languageId = null) =>
        Error.NotFound("Notification.TypeNotFound", GetTypeNotFoundByIdDescription(id, languageId));

    public static Error TypeNotFound(string code, short? languageId = null) =>
        Error.NotFound("Notification.TypeCodeNotFound", GetTypeNotFoundByCodeDescription(code, languageId));

    public static Error TypeMismatch(short id, string code, short? languageId = null) =>
        Error.Business("Notification.TypeMismatch", GetTypeMismatchDescription(id, code, languageId));

    public static Error TitleRequired(short? languageId = null) =>
        Error.Business("Notification.TitleRequired", GetTitleRequiredDescription(languageId));

    public static Error BodyRequired(short? languageId = null) =>
        Error.Business("Notification.BodyRequired", GetBodyRequiredDescription(languageId));

    public static Error UnsupportedChannel(short channel, short? languageId = null) =>
        Error.Business("Notification.UnsupportedChannel", GetUnsupportedChannelDescription(channel, languageId));

    private static string GetNotFoundDescription(long id, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bildirishnoma topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-si {id} bo'lgan bildirishnoma topilmadi.",
            LanguageIdConst.RU => $"Uvedomleniye s id {id} ne naydeno.",
            _ => $"Notification with id {id} was not found."
        };

    private static string GetTypeRequiredDescription(short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => "Bildirishnoma turi ko'rsatilishi shart.",
            LanguageIdConst.UZ_CYRL => "Bildirishnoma turi ko'rsatilishi shart.",
            LanguageIdConst.RU => "Neobkhodimo ukazat tip uvedomleniya.",
            _ => "Notification type is required."
        };

    private static string GetTypeNotFoundByIdDescription(short id, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bildirishnoma turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-si {id} bo'lgan bildirishnoma turi topilmadi.",
            LanguageIdConst.RU => $"Tip uvedomleniya s id {id} ne nayden.",
            _ => $"Notification type with id {id} was not found."
        };

    private static string GetTypeNotFoundByCodeDescription(string code, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Kodni {code} bo'lgan bildirishnoma turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Kodni {code} bo'lgan bildirishnoma turi topilmadi.",
            LanguageIdConst.RU => $"Tip uvedomleniya s kodom {code} ne nayden.",
            _ => $"Notification type with code {code} was not found."
        };

    private static string GetTypeMismatchDescription(short id, string code, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"TypeId={id} va TypeCode={code} bir xil turga tegishli emas.",
            LanguageIdConst.UZ_CYRL => $"TypeId={id} va TypeCode={code} bir xil turga tegishli emas.",
            LanguageIdConst.RU => $"TypeId={id} i TypeCode={code} ne sootvetstvuyut odnomu tipu.",
            _ => $"TypeId={id} and TypeCode={code} do not point to the same notification type."
        };

    private static string GetTitleRequiredDescription(short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => "Bildirishnoma sarlavhasi bo'sh bo'lmasligi kerak.",
            LanguageIdConst.UZ_CYRL => "Bildirishnoma sarlavhasi bo'sh bo'lmasligi kerak.",
            LanguageIdConst.RU => "Zagolovok uvedomleniya ne dolzhen byt pustym.",
            _ => "Notification title is required."
        };

    private static string GetBodyRequiredDescription(short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => "Bildirishnoma matni bo'sh bo'lmasligi kerak.",
            LanguageIdConst.UZ_CYRL => "Bildirishnoma matni bo'sh bo'lmasligi kerak.",
            LanguageIdConst.RU => "Tekst uvedomleniya ne dolzhen byt pustym.",
            _ => "Notification body is required."
        };

    private static string GetUnsupportedChannelDescription(short channel, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ => $"Notification channel {channel} hozircha qo'llab-quvvatlanmaydi.",
            LanguageIdConst.UZ_CYRL => $"Notification channel {channel} hozircha qo'llab-quvvatlanmaydi.",
            LanguageIdConst.RU => $"Kanal uvedomleniya {channel} poka ne podderzhivayetsya.",
            _ => $"Notification channel {channel} is not supported yet."
        };
}
