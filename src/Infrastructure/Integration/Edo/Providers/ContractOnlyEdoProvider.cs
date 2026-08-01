using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Providers;

// Provider adapterlari keyingi bosqichlarda qo'shiladi. Hozirgi obyekt registry uchun
// provider identity va capability metadata beradi, operationlarni esa fail-fast qiladi.
public sealed class ContractOnlyEdoProvider : IEdoProvider
{
    public ContractOnlyEdoProvider(
        EdoProviderCode code,
        string displayName,
        IReadOnlyCollection<EdoAuthMode> authModes,
        IReadOnlyCollection<EdoSigningMode> signingModes)
    {
        Code = code;
        Capabilities = new EdoProviderCapabilityDto
        {
            ProviderCode = code,
            DisplayName = displayName,
            Capabilities = Enum.GetValues<EdoCapabilityKind>()
                .Select(kind => new EdoCapabilityDto
                {
                    Kind = kind,
                    Status = EdoCapabilityStatus.UNKNOWN
                })
                .ToList(),
            AuthModes = authModes,
            SigningModes = signingModes
        };
    }

    public EdoProviderCode Code { get; }

    public EdoProviderCapabilityDto Capabilities { get; }

    public Task<EdoAuthChallengeDto> GetAuthChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoAuthChallengeDto>(EdoCapabilityKind.AuthChallenge);

    public Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoAuthCompleteDto>(EdoCapabilityKind.AuthComplete);

    public Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoOutboxCreateDto>(EdoCapabilityKind.CreateFactura);

    public Task<EdoOutboxSignDto> SignOutboxAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoOutboxSignDto>(EdoCapabilityKind.SignOutbox);

    public Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoInboxListDto>(EdoCapabilityKind.ListInbox);

    public Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoInboxRejectDto>(EdoCapabilityKind.RejectInbox);

    public Task<EdoFileDto> GetFileAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoFileDto>(EdoCapabilityKind.GetFile);

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoDocumentStatusDto>(EdoCapabilityKind.GetOutboxStatus);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        ThrowUnavailable<EdoDocumentStatusDto>(EdoCapabilityKind.GetInboxStatus);

    public static ContractOnlyEdoProvider CreateDidox() =>
        new(EdoProviderCode.DIDOX, "Didox", [EdoAuthMode.EImzo], [EdoSigningMode.TimestampedSignature]);

    public static ContractOnlyEdoProvider CreateFaktura() =>
        new(EdoProviderCode.FAKTURA, "Faktura.uz", [EdoAuthMode.OAuthPassword, EdoAuthMode.Password], [EdoSigningMode.Unknown]);

    public static ContractOnlyEdoProvider CreateEdocs() =>
        new(EdoProviderCode.EDOCS, "Edocs", [EdoAuthMode.EImzo], [EdoSigningMode.PreparedPkcs7]);

    private Task<T> ThrowUnavailable<T>(EdoCapabilityKind capability)
    {
        var status = Capabilities.Capabilities
            .Single(item => item.Kind == capability)
            .Status;

        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            capability.ToString(),
            status.ToString());
    }
}
