using System.Collections.Concurrent;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration;

/// <summary>
/// Shared local idempotency/reconciliation gate for provider side effects. It delegates all
/// persistence to the no-save operation store and never performs a provider call or a retry.
/// The keyed lock closes the read-then-add race inside one process; the database unique scope
/// remains the authority across instances.
/// </summary>
public sealed class ProviderOperationGate(IProviderOperationStore store) : IProviderOperationGate
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new(StringComparer.Ordinal);

    public Task<Result<ProviderOperation>> BeginAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string requestHash,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.BeginAsync(scope, operation, clientRequestId, requestHash, ct), ct);

    public Task<Result<ProviderOperation>> MarkSentAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? externalOperationId = null,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.MarkSentAsync(scope, operation, clientRequestId, externalOperationId, ct), ct);

    public Task<Result<ProviderOperation>> MarkConfirmedAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastExternalStatus = null,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.MarkConfirmedAsync(scope, operation, clientRequestId, lastExternalStatus, ct), ct);

    public Task<Result<ProviderOperation>> MarkFailedAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastErrorCode = null,
        string? lastExternalStatus = null,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.MarkFailedAsync(scope, operation, clientRequestId, lastErrorCode, lastExternalStatus, ct), ct);

    public Task<Result<ProviderOperation>> MarkUnknownAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastExternalStatus = null,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.MarkUnknownAsync(scope, operation, clientRequestId, lastExternalStatus, ct), ct);

    public Task<Result<ProviderOperation>> MarkReconcileRequiredAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? lastErrorCode = null,
        CancellationToken ct = default) =>
        WithLockAsync(scope, provider, operation, clientRequestId,
            () => store.MarkReconcileRequiredAsync(scope, operation, clientRequestId, lastErrorCode, ct), ct);

    public async Task<Result<bool>> CanResendAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        CancellationToken ct = default)
    {
        var validation = ValidateProvider(scope, provider);
        if (!validation.IsSuccess)
            return Result.Failure<bool>(validation.Error);

        var key = LockKey(scope, provider, operation, clientRequestId);
        var gate = Locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var existing = await store.GetAsync(scope, operation, clientRequestId, ct);
            if (!existing.IsSuccess)
                return Result.Failure<bool>(existing.Error);

            return Result.Success(existing.Value is
                { State: ProviderOperationState.ReconcileRequired, ReconcileRequired: true });
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<Result<ProviderOperation>> ResendAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        string? externalOperationId = null,
        CancellationToken ct = default)
    {
        var validation = ValidateProvider(scope, provider);
        if (!validation.IsSuccess)
            return Result.Failure<ProviderOperation>(validation.Error);

        var key = LockKey(scope, provider, operation, clientRequestId);
        var gate = Locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var existing = await store.GetAsync(scope, operation, clientRequestId, ct);
            if (!existing.IsSuccess)
                return Result.Failure<ProviderOperation>(existing.Error);

            if (existing.Value is not
                { State: ProviderOperationState.ReconcileRequired, ReconcileRequired: true })
                return Result.Failure<ProviderOperation>(ProviderOperationErrors.ResendNotAllowed);

            return await store.ResendAsync(scope, operation, clientRequestId, externalOperationId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<Result<ProviderOperation>> WithLockAsync(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId,
        Func<Task<Result<ProviderOperation>>> action,
        CancellationToken ct)
    {
        var validation = ValidateProvider(scope, provider);
        if (!validation.IsSuccess)
            return Result.Failure<ProviderOperation>(validation.Error);

        var key = LockKey(scope, provider, operation, clientRequestId);
        var gate = Locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            return await action();
        }
        finally
        {
            gate.Release();
        }
    }

    private static Result ValidateProvider(OrganizationScope scope, Provider provider)
    {
        if (scope is null || scope.OrganizationId <= 0)
            return Result.Failure(ProviderOperationErrors.ScopeInvalid);

        if (!Enum.IsDefined(provider))
            return Result.Failure(ProviderOperationErrors.ProviderInvalid);

        return scope.Provider == provider
            ? Result.Success()
            : Result.Failure(ProviderOperationErrors.ScopeProviderMismatch);
    }

    private static string LockKey(
        OrganizationScope scope,
        Provider provider,
        string operation,
        string clientRequestId) =>
        $"{scope.OrganizationId}:{provider}:{operation?.Trim() ?? string.Empty}:{clientRequestId?.Trim() ?? string.Empty}";
}
