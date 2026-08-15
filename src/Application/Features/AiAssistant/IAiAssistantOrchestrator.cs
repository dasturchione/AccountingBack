namespace Application.Features.AiAssistant;

public interface IAiAssistantOrchestrator
{
    Task<GroundedAiResponse> ExecuteAsync(
        AiAssistantRequest request,
        CancellationToken ct = default);
}

public sealed record AiAssistantRequest(
    string UserText,
    string? ToolName,
    object? ToolInput = null,
    AiOrganizationContextRequest? Organization = null,
    string? LanguageCode = null);

public sealed class AiAssistantOrchestrator(
    IAiOrganizationContextResolver organizationContextResolver,
    ILanguageContextResolver languageContextResolver,
    IAiToolExecutionGuard toolExecutionGuard,
    IAiReadOnlyToolRegistry toolRegistry,
    IAiProvider provider) : IAiAssistantOrchestrator
{
    public async Task<GroundedAiResponse> ExecuteAsync(
        AiAssistantRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var language = languageContextResolver.Resolve(request.LanguageCode, request.UserText);
        var organization = await organizationContextResolver.ResolveAsync(request.Organization, ct);
        if (!organization.IsSuccess)
            return GroundedAiResponse.CreateSafeFallback(
                language,
                organization: null,
                kind: AiSafeFallbackKind.UnauthorizedOrganization);

        if (!toolRegistry.IsAllowed(request.ToolName))
            return GroundedAiResponse.CreateSafeFallback(
                language,
                organization: null,
                AiSafeFallbackKind.ToolUnavailable);

        if (!toolRegistry.TryGet(request.ToolName!, out var executor) || executor is null)
            return GroundedAiResponse.CreateSafeFallback(
                language,
                organization: null,
                AiSafeFallbackKind.ToolUnavailable);

        var authorizedOrganization = await toolExecutionGuard.ValidateAsync(organization.Value, ct);
        if (!authorizedOrganization.IsSuccess)
            return GroundedAiResponse.CreateSafeFallback(
                language,
                organization: null,
                kind: AiSafeFallbackKind.UnauthorizedOrganization);

        AiToolExecutionOutcome toolResult;
        try
        {
            toolResult = await executor.ExecuteAsync(
                new AiToolInvocationRequest(
                    request.ToolInput,
                    authorizedOrganization.Value,
                    language),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.RequiresVerification);
        }
        catch
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.RequiresVerification);
        }

        var evidence = toolResult.Evidence
            .Select(evidenceItem => new GroundedAiEvidence(
                evidenceItem.Source,
                evidenceItem.Description,
                evidenceItem.Period,
                evidenceItem.RetrievedAtUtc))
            .ToArray();

        if (toolResult.Status == AiToolResultStatus.NoData)
            return GroundedAiResponse.NoData(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                evidence,
                [GetNoDataNextAction(language.Language)]);

        if (toolResult.Status == AiToolResultStatus.Error
            || toolResult.Status == AiToolResultStatus.IncompleteData
            || !toolResult.IsComplete)
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                toolResult.Error?.Kind == AiToolErrorKind.UnauthorizedOrganization
                    ? AiSafeFallbackKind.UnauthorizedOrganization
                    : AiSafeFallbackKind.RequiresVerification,
                evidence,
                [GetIncompleteDataNextAction(language.Language)]);
        }

        if (evidence.Length == 0)
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.RequiresVerification);

        if (!string.IsNullOrWhiteSpace(toolResult.Answer))
        {
            return new GroundedAiResponse(
                toolResult.Answer,
                ToGroundedOrganization(authorizedOrganization.Value),
                language,
                AiEvidenceState.Grounded,
                AiConfidenceLevel.High,
                evidence,
                [],
                null,
                null);
        }

        AiProviderChatResult providerResult;
        try
        {
            var evidenceContext = string.Join(
                Environment.NewLine,
                evidence.Select(item => $"{item.Source}: {item.Observation}"));

            providerResult = await provider.CompleteAsync(
                new AiProviderChatRequest(
                    [
                        new AiChatMessage(
                            AiChatMessageRole.System,
                            "Answer only from the supplied accounting evidence. Do not disclose internal identifiers, technical details, credentials, or provider payloads."),
                        new AiChatMessage(AiChatMessageRole.System, $"Accounting evidence:{Environment.NewLine}{evidenceContext}"),
                        new AiChatMessage(AiChatMessageRole.User, request.UserText)
                    ],
                    language),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.ProviderUnavailable,
                evidence);
        }
        catch
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.ProviderUnavailable,
                evidence);
        }

        if (providerResult.Status != AiProviderChatStatus.Success
            || string.IsNullOrWhiteSpace(providerResult.Response?.Content))
        {
            return GroundedAiResponse.CreateSafeFallback(
                language,
                ToGroundedOrganization(authorizedOrganization.Value),
                AiSafeFallbackKind.ProviderUnavailable,
                evidence);
        }

        return new GroundedAiResponse(
            providerResult.Response.Content,
            ToGroundedOrganization(authorizedOrganization.Value),
            language,
            AiEvidenceState.Grounded,
            AiConfidenceLevel.Medium,
            evidence,
            [],
            null,
            null);
    }

    private static GroundedOrganizationContext ToGroundedOrganization(
        AiOrganizationContext organization) =>
        new(organization.Name, organization.Inn);

    private static string GetNoDataNextAction(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Filtrlarni tekshiring yoki boshqa davrni tanlang.",
        AiLanguage.UzbekCyrillic => "Филтрларни текширинг ёки бошқа даврни танланг.",
        AiLanguage.Russian => "Проверьте фильтры или выберите другой период.",
        _ => "Check the filters or select another period."
    };

    private static string GetIncompleteDataNextAction(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Aniq javob uchun maʼlumotlarni toʻliq tekshiring.",
        AiLanguage.UzbekCyrillic => "Аниқ жавоб учун маълумотларни тўлиқ текширинг.",
        AiLanguage.Russian => "Проверьте полноту данных перед использованием ответа.",
        _ => "Verify data completeness before using the answer."
    };
}
