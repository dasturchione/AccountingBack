namespace Application.Features.AiAssistant;

public sealed record GroundedAiResponse(
    string Answer,
    GroundedOrganizationContext? Organization,
    AiLanguageContext Language,
    AiEvidenceState EvidenceState,
    AiConfidenceLevel Confidence,
    IReadOnlyList<GroundedAiEvidence> Evidence,
    IReadOnlyList<string> MissingData,
    string? RecommendedNextAction,
    AiSafeFallback? SafeFallback)
{
    public static GroundedAiResponse NoData(
        AiLanguageContext language,
        GroundedOrganizationContext? organization,
        IReadOnlyList<GroundedAiEvidence>? evidence = null,
        IReadOnlyList<string>? missingData = null,
        string? recommendedNextAction = null) =>
        new(
            Answer: GetNoDataMessage(language.Language),
            Organization: organization,
            Language: language,
            EvidenceState: AiEvidenceState.NoData,
            Confidence: AiConfidenceLevel.NotAvailable,
            Evidence: evidence ?? [],
            MissingData: missingData ?? [],
            RecommendedNextAction: recommendedNextAction,
            SafeFallback: new AiSafeFallback(
                AiSafeFallbackKind.NoData,
                GetNoDataMessage(language.Language)));

    public static GroundedAiResponse CreateSafeFallback(
        AiLanguageContext language,
        GroundedOrganizationContext? organization,
        AiSafeFallbackKind kind = AiSafeFallbackKind.RequiresVerification,
        IReadOnlyList<GroundedAiEvidence>? evidence = null,
        IReadOnlyList<string>? missingData = null) =>
        new(
            Answer: GetFallbackMessage(language.Language, kind),
            Organization: organization,
            Language: language,
            EvidenceState: AiEvidenceState.Fallback,
            Confidence: AiConfidenceLevel.NotAvailable,
            Evidence: evidence ?? [],
            MissingData: missingData ?? [],
            RecommendedNextAction: null,
            SafeFallback: new AiSafeFallback(kind, GetFallbackMessage(language.Language, kind)));

    private static string GetNoDataMessage(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Bu so‘rov bo‘yicha ma’lumot mavjud emas.",
        AiLanguage.UzbekCyrillic => "Бу сўров бўйича маълумот мавжуд эмас.",
        AiLanguage.Russian => "По этому запросу данные отсутствуют.",
        _ => "No data is available for this request."
    };

    private static string GetFallbackMessage(AiLanguage language, AiSafeFallbackKind kind) =>
        (language, kind) switch
        {
            (AiLanguage.UzbekLatin, AiSafeFallbackKind.UnauthorizedOrganization) =>
                "Tashkilot ma’lumotlariga kirish ruxsati mavjud emas.",
            (AiLanguage.UzbekLatin, AiSafeFallbackKind.ToolUnavailable) =>
                "Bu hisob-kitob so‘rovi hozircha mavjud emas.",
            (AiLanguage.UzbekLatin, AiSafeFallbackKind.ProviderUnavailable) =>
                "AI xizmati hozircha mavjud emas. Ma’lumotlarni keyinroq tekshiring.",
            (AiLanguage.UzbekLatin, _) =>
                "Aniq javob berish uchun ma’lumotlarni tekshirish kerak.",
            (AiLanguage.UzbekCyrillic, AiSafeFallbackKind.UnauthorizedOrganization) =>
                "Ташкилот маълумотларига кириш учун рухсат мавжуд эмас.",
            (AiLanguage.UzbekCyrillic, AiSafeFallbackKind.ToolUnavailable) =>
                "Бу ҳисоб-китоб сўрови ҳозирча мавжуд эмас.",
            (AiLanguage.UzbekCyrillic, AiSafeFallbackKind.ProviderUnavailable) =>
                "AI хизмати ҳозирча мавжуд эмас. Маълумотларни кейинроқ текширинг.",
            (AiLanguage.UzbekCyrillic, _) =>
                "Аниқ жавоб бериш учун маълумотларни текшириш керак.",
            (AiLanguage.Russian, AiSafeFallbackKind.UnauthorizedOrganization) =>
                "Нет доступа к данным организации.",
            (AiLanguage.Russian, AiSafeFallbackKind.ToolUnavailable) =>
                "Этот учетный запрос пока недоступен.",
            (AiLanguage.Russian, AiSafeFallbackKind.ProviderUnavailable) =>
                "Сервис AI временно недоступен. Проверьте данные позже.",
            (AiLanguage.Russian, _) =>
                "Для точного ответа необходимо проверить данные.",
            (_, AiSafeFallbackKind.UnauthorizedOrganization) =>
                "Access to the organization data is not allowed.",
            (_, AiSafeFallbackKind.ToolUnavailable) =>
                "This accounting request is not currently available.",
            (_, AiSafeFallbackKind.ProviderUnavailable) =>
                "The AI service is currently unavailable. Please verify the data later.",
            _ => "The data must be verified before a reliable answer can be given."
        };
}

public sealed record GroundedOrganizationContext(
    string Name,
    string Inn);

public sealed record GroundedAiEvidence(
    string Source,
    string Observation,
    string? Period = null,
    DateTimeOffset? RetrievedAtUtc = null);

public sealed record AiSafeFallback(
    AiSafeFallbackKind Kind,
    string Message);

public enum AiEvidenceState : short
{
    Grounded = 1,
    NoData = 2,
    RequiresVerification = 3,
    Fallback = 4
}

public enum AiConfidenceLevel : short
{
    High = 1,
    Medium = 2,
    Low = 3,
    NotAvailable = 4
}

public enum AiSafeFallbackKind : short
{
    NoData = 1,
    RequiresVerification = 2,
    ProviderUnavailable = 3,
    ToolUnavailable = 4,
    UnauthorizedOrganization = 5
}
