using Application.Abstractions;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Integration;

/// <summary>
/// Idempotency store for outbound provider operations, staged as change-tracker mutations. Like the
/// other integration stores it never calls <c>SaveChangesAsync</c>; the calling service owns the
/// unit-of-work transaction. Organization/provider always come from the trusted
/// <see cref="OrganizationScope"/>, and only request hashes / external ids are stored — never a token,
/// PKCS#7 body or secret.
/// </summary>
public sealed class ProviderOperationStore(
    IQueryRepository<ProviderOperation> operations,
    ITrackingRepository<ProviderOperation> tracking,
    TimeProvider timeProvider) : IProviderOperationStore
{
    public async Task<Result<ProviderOperation>> BeginAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string requestHash,
        CancellationToken ct = default)
    {
        if (ValidateKeys(scope, operation, clientRequestId) is { IsSuccess: false } invalid)
            return Result.Failure<ProviderOperation>(invalid.Error);

        if (!IsValidHash(requestHash))
            return Result.Failure<ProviderOperation>(ProviderOperationErrors.RequestHashInvalid);

        var existing = await FindAsync(scope, operation, clientRequestId, ct);
        if (existing is not null)
        {
            // Same idempotency key: identical request replays the prior operation, a different request
            // is a conflict (fail closed — never silently overwrite).
            return string.Equals(existing.RequestHash, requestHash.Trim(), StringComparison.Ordinal)
                ? Result.Success(existing)
                : Result.Failure<ProviderOperation>(ProviderOperationErrors.HashConflict);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var created = new ProviderOperation
        {
            OrganizationId = scope.OrganizationId,
            Provider = scope.Provider,
            Operation = operation.Trim(),
            ClientRequestId = clientRequestId.Trim(),
            RequestHash = requestHash.Trim(),
            ExternalOperationId = null,
            State = ProviderOperationState.Pending,
            ReconcileRequired = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await tracking.AddAsync(created, ct);
        return Result.Success(created);
    }

    public async Task<Result<ProviderOperation?>> GetAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        CancellationToken ct = default)
    {
        if (ValidateKeys(scope, operation, clientRequestId) is { IsSuccess: false } invalid)
            return Result.Failure<ProviderOperation?>(invalid.Error);

        var existing = await FindAsync(scope, operation, clientRequestId, ct);
        return Result.Success(existing);
    }

    public Task<Result<ProviderOperation>> MarkSentAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? externalOperationId,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.Pending],
            transitionError: ProviderOperationErrors.InvalidTransition,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.Sent;
                op.ExternalOperationId = Trim(externalOperationId) ?? op.ExternalOperationId;
                op.UpdatedAtUtc = now;
            });

    public Task<Result<ProviderOperation>> MarkConfirmedAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastExternalStatus,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.Sent, ProviderOperationState.Unknown, ProviderOperationState.ReconcileRequired],
            transitionError: ProviderOperationErrors.InvalidTransition,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.Confirmed;
                op.ReconcileRequired = false;
                op.LastExternalStatus = Trim(lastExternalStatus) ?? op.LastExternalStatus;
                op.CompletedAtUtc = now;
                op.UpdatedAtUtc = now;
            });

    public Task<Result<ProviderOperation>> MarkFailedAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastErrorCode,
        string? lastExternalStatus,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.Pending, ProviderOperationState.Sent, ProviderOperationState.Unknown, ProviderOperationState.ReconcileRequired],
            transitionError: ProviderOperationErrors.InvalidTransition,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.Failed;
                op.LastErrorCode = Trim(lastErrorCode) ?? op.LastErrorCode;
                op.LastExternalStatus = Trim(lastExternalStatus) ?? op.LastExternalStatus;
                op.CompletedAtUtc = now;
                op.UpdatedAtUtc = now;
            });

    public Task<Result<ProviderOperation>> MarkUnknownAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastExternalStatus,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.Pending, ProviderOperationState.Sent],
            transitionError: ProviderOperationErrors.InvalidTransition,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.Unknown;
                op.ReconcileRequired = true; // an inconclusive outcome always needs reconciliation
                op.LastExternalStatus = Trim(lastExternalStatus) ?? op.LastExternalStatus;
                op.UpdatedAtUtc = now;
            });

    public Task<Result<ProviderOperation>> MarkReconcileRequiredAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? lastErrorCode,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.Pending, ProviderOperationState.Sent, ProviderOperationState.Unknown, ProviderOperationState.Failed],
            transitionError: ProviderOperationErrors.InvalidTransition,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.ReconcileRequired;
                op.ReconcileRequired = true;
                op.LastErrorCode = Trim(lastErrorCode) ?? op.LastErrorCode;
                op.UpdatedAtUtc = now;
            });

    public Task<Result<ProviderOperation>> ResendAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        string? externalOperationId,
        CancellationToken ct = default) =>
        TransitionAsync(scope, operation, clientRequestId, ct,
            allowed: [ProviderOperationState.ReconcileRequired],
            transitionError: ProviderOperationErrors.ResendNotAllowed,
            apply: (op, now) =>
            {
                op.State = ProviderOperationState.Sent;
                op.ReconcileRequired = false;
                op.ExternalOperationId = Trim(externalOperationId) ?? op.ExternalOperationId;
                op.CompletedAtUtc = null;
                op.UpdatedAtUtc = now;
            });

    private async Task<Result<ProviderOperation>> TransitionAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        CancellationToken ct,
        ProviderOperationState[] allowed,
        Error transitionError,
        Action<ProviderOperation, DateTime> apply)
    {
        if (ValidateKeys(scope, operation, clientRequestId) is { IsSuccess: false } invalid)
            return Result.Failure<ProviderOperation>(invalid.Error);

        var existing = await FindAsync(scope, operation, clientRequestId, ct);
        if (existing is null)
            return Result.Failure<ProviderOperation>(ProviderOperationErrors.NotFound);

        if (Array.IndexOf(allowed, existing.State) < 0)
            return Result.Failure<ProviderOperation>(transitionError);

        apply(existing, timeProvider.GetUtcNow().UtcDateTime);
        await tracking.UpdateAsync(existing, ct);
        return Result.Success(existing);
    }

    // Exactly one row per (organization, provider, operation, client_request_id) — enforced by
    // ux_int_provider_operation_idempotency. The scope filter plus the ambient organization query
    // filter keep other organizations' operations invisible.
    private Task<ProviderOperation?> FindAsync(
        OrganizationScope scope,
        string operation,
        string clientRequestId,
        CancellationToken ct)
    {
        var op = operation.Trim();
        var requestId = clientRequestId.Trim();
        return operations.GetAsync(new QuerySpecification<ProviderOperation>
        {
            Criteria = x => x.OrganizationId == scope.OrganizationId
                && x.Provider == scope.Provider
                && x.Operation == op
                && x.ClientRequestId == requestId
        }, ct);
    }

    private static Result ValidateKeys(OrganizationScope scope, string operation, string clientRequestId)
    {
        if (scope is null || scope.OrganizationId <= 0)
            return Result.Failure(ProviderOperationErrors.ScopeInvalid);

        if (!Enum.IsDefined(scope.Provider))
            return Result.Failure(ProviderOperationErrors.ProviderInvalid);

        if (string.IsNullOrWhiteSpace(operation) || operation.Trim().Length > 100)
            return Result.Failure(ProviderOperationErrors.OperationInvalid);

        if (string.IsNullOrWhiteSpace(clientRequestId) || clientRequestId.Trim().Length > 200)
            return Result.Failure(ProviderOperationErrors.ClientRequestIdInvalid);

        return Result.Success();
    }

    private static bool IsValidHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (trimmed.Length is < 16 or > 128)
            return false;

        foreach (var ch in trimmed)
        {
            if (char.IsWhiteSpace(ch) || char.IsControl(ch))
                return false;
        }

        return true;
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
