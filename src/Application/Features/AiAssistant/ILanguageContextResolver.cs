using SharedKernel.Constants;

namespace Application.Features.AiAssistant;

public interface ILanguageContextResolver
{
    AiLanguageContext Resolve(
        string? requestLanguageCode,
        string? userText);
}

public sealed record AiLanguageContext(
    AiLanguage Language,
    string Code,
    bool IsExplicit,
    bool IsDetected)
{
    public static AiLanguageContext From(AiLanguage language, bool isExplicit, bool isDetected) =>
        new(language, language switch
        {
            AiLanguage.UzbekLatin => LanguageCodeConst.UZ,
            AiLanguage.UzbekCyrillic => LanguageCodeConst.UZ_CYRL,
            AiLanguage.Russian => LanguageCodeConst.RU,
            _ => LanguageCodeConst.EN
        }, isExplicit, isDetected);
}

public enum AiLanguage : short
{
    UzbekLatin = 1,
    UzbekCyrillic = 2,
    Russian = 3,
    English = 4
}
