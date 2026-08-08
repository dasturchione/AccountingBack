using Application.Abstractions.Integration.Edo;
using Integration.Faktura.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Providers;

public sealed class FakturaEdoProvider(FakturaEdoOperations edoOperations) : IEdoProvider
{
    public EdoProviderCode Code => EdoProviderCode.FAKTURA;

    public EdoProviderCapabilityDto Capabilities { get; } = CreateCapabilities();

    public Task<EdoAuthChallengeDto> GetAuthChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoAuthChallengeDto>(EdoCapabilityKind.AuthChallenge);

    public Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.CompleteAuthAsync(request, ct);

    public Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.CreateFacturaAsync(request, ct);

    public Task<EdoOutboxSignDto> SignOutboxAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoOutboxSignDto>(EdoCapabilityKind.SignOutbox);

    public Task<EdoOutboxSignDto> SignOutboxAsync(
        string providerDocumentId,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.SignOutboxAsync(providerDocumentId, request, ct);

    public Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default) =>
        edoOperations.ListInboxAsync(request, ct);

    public Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default) =>
        edoOperations.ListDocumentsAsync(request, ct);

    public Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentType,
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default) =>
        edoOperations.RejectInboxAsync(providerDocumentId, providerDocumentType, ct);

    public Task<EdoFileDto> GetFileAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetFileAsync(providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetOutboxStatusAsync(providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        edoOperations.GetInboxStatusAsync(providerDocumentId, ct);

    private Task<T> ThrowUnavailable<T>(EdoCapabilityKind capability) =>
        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            capability.ToString(),
            EdoCapabilityStatus.UNKNOWN.ToString());

    private static EdoProviderCapabilityDto CreateCapabilities() =>
        new()
        {
            ProviderCode = EdoProviderCode.FAKTURA,
            DisplayName = "Faktura.uz",
            AuthModes = [EdoAuthMode.OAuthPassword, EdoAuthMode.Password],
            SigningModes = [EdoSigningMode.Unknown],
            Capabilities = Enum.GetValues<EdoCapabilityKind>()
                .Select(kind => new EdoCapabilityDto
                {
                    Kind = kind,
                    Status = kind switch
                    {
                        EdoCapabilityKind.AuthComplete
                            or EdoCapabilityKind.ListInbox
                            or EdoCapabilityKind.ListOutbox
                            or EdoCapabilityKind.GetFile
                            or EdoCapabilityKind.GetInboxStatus => EdoCapabilityStatus.SUPPORTED,
                        _ => EdoCapabilityStatus.UNKNOWN
                    }
                })
                .ToList()
        };
}
