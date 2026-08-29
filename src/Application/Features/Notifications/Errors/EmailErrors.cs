using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Notifications;

public static class EmailErrors
{
    public static Error NoRecipient(short? languageId = null) =>
        Error.Validation("Email.NoRecipient", Message(languageId,
            "Kamida bitta qabul qiluvchi ko'rsatilishi kerak.", "Камида битта қабул қилувчи кўрсатилиши керак.",
            "Необходимо указать хотя бы одного получателя.", "At least one recipient is required."));

    public static Error BuildFailed(short? languageId = null) =>
        Error.Validation("Email.BuildFailed", Message(languageId,
            "Elektron xat ma'lumotlari noto'g'ri.", "Электрон хат маълумотлари нотўғри.",
            "Данные электронного письма указаны неверно.", "The email message data is invalid."));

    public static Error SendFailed(short? languageId = null) =>
        Error.Problem("Email.SendFailed", Message(languageId,
            "Elektron xatni yuborib bo'lmadi.", "Электрон хатни юбориб бўлмади.",
            "Не удалось отправить электронное письмо.", "Email could not be sent."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
