namespace Application.Features.AiAssistant;

public sealed class UnavailableAiProvider : IAiProvider
{
    public Task<AiProviderChatResult> CompleteAsync(
        AiProviderChatRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.FromResult(
            ct.IsCancellationRequested
                ? AiProviderChatResult.Cancelled()
                : AiProviderChatResult.NotConfigured());
    }
}
