using Application.Abstractions.Authentication;
using SharedKernel.Constants;

namespace Application.Features.AiAssistant;

public sealed class LanguageContextResolver(IUserContext userContext) : ILanguageContextResolver
{
    public AiLanguageContext Resolve(string? requestLanguageCode, string? userText)
    {
        if (TryParse(requestLanguageCode, out var requestedLanguage))
            return AiLanguageContext.From(requestedLanguage, isExplicit: true, isDetected: false);

        var detectedLanguage = Detect(userText);
        if (detectedLanguage is not null)
            return AiLanguageContext.From(detectedLanguage.Value, isExplicit: false, isDetected: true);

        var contextLanguage = userContext.LanguageId switch
        {
            LanguageIdConst.UZ => AiLanguage.UzbekLatin,
            LanguageIdConst.UZ_CYRL => AiLanguage.UzbekCyrillic,
            LanguageIdConst.RU => AiLanguage.Russian,
            _ => AiLanguage.English
        };

        return AiLanguageContext.From(contextLanguage, isExplicit: false, isDetected: false);
    }

    private static bool TryParse(string? code, out AiLanguage language)
    {
        language = default;

        var normalized = code?.Trim().Replace('-', '_');
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        if (normalized.Equals(LanguageCodeConst.UZ, StringComparison.OrdinalIgnoreCase))
        {
            language = AiLanguage.UzbekLatin;
            return true;
        }

        if (normalized.Equals(LanguageCodeConst.UZ_CYRL, StringComparison.OrdinalIgnoreCase))
        {
            language = AiLanguage.UzbekCyrillic;
            return true;
        }

        if (normalized.Equals(LanguageCodeConst.RU, StringComparison.OrdinalIgnoreCase))
        {
            language = AiLanguage.Russian;
            return true;
        }

        if (normalized.Equals(LanguageCodeConst.EN, StringComparison.OrdinalIgnoreCase))
        {
            language = AiLanguage.English;
            return true;
        }

        return false;
    }

    private static AiLanguage? Detect(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (text.Any(character => "ўқғҳЎҚҒҲ".Contains(character)))
            return AiLanguage.UzbekCyrillic;

        if (text.Any(character => character is >= '\u0400' and <= '\u04ff'))
            return AiLanguage.Russian;

        var normalized = text.ToLowerInvariant();
        if (new[] { "shartnoma", "tashkilot", "uchun", "bo'yicha", "bo‘yicha", "hisobot" }
            .Any(normalized.Contains))
            return AiLanguage.UzbekLatin;

        return AiLanguage.English;
    }
}
