using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Settings;

public static class SettingErrors
{
    public static Error NotFound(string code, short? languageId = null) =>
        Error.NotFound("Setting.NotFound", Message(languageId,
            $"'{code}' kodli sozlama topilmadi.", $"'{code}' кодли созлама топилмади.",
            $"Настройка с кодом '{code}' не найдена.", $"Setting with code '{code}' was not found."));

    public static Error ReadOnly(string code, short? languageId = null) =>
        Error.Forbidden("Setting.ReadOnly", Message(languageId,
            $"'{code}' sozlamasi faqat o'qish uchun.", $"'{code}' созламаси фақат ўқиш учун.",
            $"Настройка '{code}' доступна только для чтения.", $"Setting '{code}' is read-only."));

    public static Error InvalidValue(string code, short? languageId = null) =>
        Error.Business("Setting.InvalidValue", Message(languageId,
            $"'{code}' sozlamasida noto'g'ri qiymat mavjud.", $"'{code}' созламасида нотўғри қиймат мавжуд.",
            $"Настройка '{code}' содержит недопустимое значение.", $"Setting '{code}' contains an invalid value."));

    public static Error UnsupportedValueType(string code, short valueType, short? languageId = null) =>
        Error.Business("Setting.UnsupportedValueType", Message(languageId,
            $"'{code}' sozlamasida qo'llab-quvvatlanmaydigan '{valueType}' qiymat turi ishlatilgan.",
            $"'{code}' созламасида қўллаб-қувватланмайдиган '{valueType}' қиймат тури ишлатилган.",
            $"Настройка '{code}' использует неподдерживаемый тип значения '{valueType}'.",
            $"Setting '{code}' uses unsupported value type '{valueType}'."));

    public static Error TypeMismatch(string code, string requestedType, short? languageId = null) =>
        Error.Business("Setting.TypeMismatch", Message(languageId,
            $"'{code}' sozlamasini '{requestedType}' turi sifatida o'qib bo'lmaydi.",
            $"'{code}' созламасини '{requestedType}' тури сифатида ўқиб бўлмайди.",
            $"Настройку '{code}' нельзя прочитать как '{requestedType}'.",
            $"Setting '{code}' cannot be read as '{requestedType}'."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
