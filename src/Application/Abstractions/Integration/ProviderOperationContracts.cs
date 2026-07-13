using Domain.Entities;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Idempotency store for outbound provider operations. Every operation is keyed by the trusted
/// <see cref="OrganizationScope"/> organization/provider plus the caller's operation name and
/// idempotency key. The store never calls <c>SaveChangesAsync</c> — the calling service owns the
/// unit-of-work transaction — and only ever holds request hashes and provider ids, never tokens,
/// PKCS#7 bodies or secrets.
/// </summary>
public interface IProviderOperationStore
{
    /// <summary>
    /// Begins (or resumes) an idempotent operation. A first call creates a <c>Pending</c> row; a
    /// repeat with the same request hash returns the prior operation; a repeat with a different hash
    /// for the same idempotency key is a conflict.
    /// </summary>
    Task<Result<ProviderOperation>> BeginAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string requestHash,
        CancellationToken ct = default);

    Task<Result<ProviderOperation?>> GetAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkSentAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? externalOperationId,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkConfirmedAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastExternalStatus,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkFailedAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastErrorCode,
        string? lastExternalStatus,
        CancellationToken ct = default);

    /// <summary>Records an inconclusive outcome (e.g. timeout). The operation then requires reconcile before any resend.</summary>
    Task<Result<ProviderOperation>> MarkUnknownAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastExternalStatus,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkReconcileRequiredAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastErrorCode,
        CancellationToken ct = default);

    /// <summary>
    /// Allows a fresh send only after reconciliation has moved the operation to
    /// <c>ReconcileRequired</c>. Resending directly from <c>Unknown</c> (or any other state) is blocked.
    /// </summary>
    Task<Result<ProviderOperation>> ResendAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? externalOperationId,
        CancellationToken ct = default);
}

/// <summary>
/// Side-effect gate for provider operations. The gate is deliberately provider-agnostic: it
/// records idempotency and reconciliation state but never builds a provider request or performs
/// an HTTP call. Implementations must receive a trusted <see cref="OrganizationScope"/>.
/// </summary>
public interface IProviderOperationGate
{
    Task<Result<ProviderOperation>> BeginAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string requestHash,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkSentAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? externalOperationId = null,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkConfirmedAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastExternalStatus = null,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkFailedAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastErrorCode = null,
        string? lastExternalStatus = null,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkUnknownAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastExternalStatus = null,
        CancellationToken ct = default);

    Task<Result<ProviderOperation>> MarkReconcileRequiredAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastErrorCode = null,
        CancellationToken ct = default);

    Task<Result<bool>> CanResendAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        CancellationToken ct = default);

    /// <summary>Moves only a reconciled operation back to Sent; Unknown is never resent directly.</summary>
    Task<Result<ProviderOperation>> ResendAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? externalOperationId = null,
        CancellationToken ct = default);
}
