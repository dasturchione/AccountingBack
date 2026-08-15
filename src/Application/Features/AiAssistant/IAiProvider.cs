namespace Application.Features.AiAssistant;

public interface IAiProvider
{
    Task<AiProviderChatResult> CompleteAsync(
        AiProviderChatRequest request,
        CancellationToken ct = default);
}

public sealed record AiProviderChatRequest(
    IReadOnlyList<AiChatMessage> Messages,
    AiLanguageContext LanguageContext);

public sealed record AiChatMessage(
    AiChatMessageRole Role,
    string Content);

public enum AiChatMessageRole : short
{
    System = 1,
    User = 2,
    Assistant = 3
}

public sealed record AiProviderChatResponse(
    string Content);

public sealed record AiProviderError(
    AiProviderErrorKind Kind,
    string SafeMessage);

public enum AiProviderErrorKind : short
{
    NotConfigured = 1,
    Unavailable = 2,
    Cancelled = 3
}

public sealed record AiProviderChatResult(
    AiProviderChatStatus Status,
    AiProviderChatResponse? Response,
    AiProviderError? Error)
{
    public static AiProviderChatResult Success(AiProviderChatResponse response) =>
        new(AiProviderChatStatus.Success, response, null);

    public static AiProviderChatResult NotConfigured() =>
        new(
            AiProviderChatStatus.NotConfigured,
            null,
            new AiProviderError(
                AiProviderErrorKind.NotConfigured,
                "AI provider is not configured."));

    public static AiProviderChatResult Unavailable() =>
        new(
            AiProviderChatStatus.Unavailable,
            null,
            new AiProviderError(
                AiProviderErrorKind.Unavailable,
                "AI provider is temporarily unavailable."));

    public static AiProviderChatResult Cancelled() =>
        new(
            AiProviderChatStatus.Cancelled,
            null,
            new AiProviderError(
                AiProviderErrorKind.Cancelled,
                "AI request was cancelled."));
}

public enum AiProviderChatStatus : short
{
    Success = 1,
    NotConfigured = 2,
    Unavailable = 3,
    Cancelled = 4
}
