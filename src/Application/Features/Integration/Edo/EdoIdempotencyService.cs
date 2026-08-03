using Application.Abstractions;
using Application.Abstractions.Integration.Edo;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Integration.Edo;

public interface IEdoIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(
        int organizationId,
        string idempotencyKey,
        CancellationToken ct = default);

    Task AddAsync(IdempotencyRecord record, CancellationToken ct = default);

    Task UpdateAsync(IdempotencyRecord record, CancellationToken ct = default);
}

public interface IEdoIdempotencyService
{
    Task<EdoIdempotencyDecision> TryCreateIdempotencyRecordAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string requestKey,
        string requestHash,
        string? continuationReference = null,
        CancellationToken ct = default);

    Task SetReferenceAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string reference,
        CancellationToken ct = default);

    Task CompleteIdempotencyAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string resultId,
        CancellationToken ct = default);

    Task MarkFailedAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string? resultId = null,
        CancellationToken ct = default);
}

public sealed record EdoIdempotencyDecision(
    string Key,
    bool IsReplay,
    bool IsContinuation,
    string? ResultDocumentId);

public sealed class EdoIdempotencyService(
    IEdoIdempotencyStore store,
    IAuditLogService auditLogService,
    ILogger<EdoIdempotencyService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoIdempotencyService
{
    private const string IdempotencyTable = "idempotency_record";

    public async Task<EdoIdempotencyDecision> TryCreateIdempotencyRecordAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string requestKey,
        string requestHash,
        string? continuationReference = null,
        CancellationToken ct = default)
    {
        var key = BuildKey(providerCode, operationType, requestKey);
        var existing = await store.GetAsync(organizationId, key, ct);

        if (existing is null)
        {
            try
            {
                await ExecuteInTransactionAsync("CreateIdempotencyRecord", async () =>
                {
                    var record = new IdempotencyRecord
                    {
                        OrganizationId = organizationId,
                        IdempotencyKey = key,
                        OperationType = operationType,
                        RequestHash = requestHash,
                        Status = "PENDING",
                        CreatedDate = DateTime.UtcNow
                    };

                    await store.AddAsync(record, ct);
                    await AuditAsync(
                        record,
                        providerCode,
                        "IDEMPOTENCY_CREATED",
                        null,
                        "PENDING");
                    return Result.Success();
                }, ct);

                return new EdoIdempotencyDecision(key, false, false, null);
            }
            catch (UniqueConstraintViolationException)
            {
                existing = await store.GetAsync(organizationId, key, ct)
                    ?? throw new InvalidOperationException("The concurrent EDO idempotency record could not be reloaded.");
            }
        }

        ValidateRequest(existing, operationType, requestHash);

        if (existing.Status == "COMPLETED")
            return new EdoIdempotencyDecision(key, true, false, existing.ResultDocumentId);

        if (existing.Status == "FAILED")
        {
            var oldStatus = existing.Status;
            await ExecuteInTransactionAsync("RetryFailedIdempotencyRecord", async () =>
            {
                existing.Status = "PENDING";
                existing.ResultDocumentId = null;
                existing.RequestReference = null;
                existing.UpdatedDate = DateTime.UtcNow;
                await store.UpdateAsync(existing, ct);
                await AuditAsync(
                    existing,
                    providerCode,
                    "IDEMPOTENCY_RETRY",
                    oldStatus,
                    existing.Status);
                return Result.Success();
            }, ct);

            return new EdoIdempotencyDecision(key, false, false, null);
        }

        if (existing.Status == "PENDING"
            && !string.IsNullOrWhiteSpace(continuationReference)
            && string.Equals(existing.RequestReference, continuationReference, StringComparison.Ordinal))
        {
            return new EdoIdempotencyDecision(key, false, true, existing.ResultDocumentId);
        }

        throw new InvalidOperationException("A request with this EDO idempotency key is already in progress.");
    }

    public Task SetReferenceAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string reference,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync("SetIdempotencyReference", async () =>
        {
            var record = await GetAndValidateAsync(
                organizationId,
                providerCode,
                operationType,
                idempotencyKey,
                ct);
            var oldStatus = record.Status;
            record.RequestReference = reference;
            record.UpdatedDate = DateTime.UtcNow;
            await store.UpdateAsync(record, ct);
            await AuditAsync(record, providerCode, "IDEMPOTENCY_REFERENCE_SET", oldStatus, record.Status);
            return Result.Success();
        }, ct);

    public Task CompleteIdempotencyAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string resultId,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync("CompleteIdempotencyRecord", async () =>
        {
            var record = await GetAndValidateAsync(
                organizationId,
                providerCode,
                operationType,
                idempotencyKey,
                ct);
            var oldStatus = record.Status;
            record.ResultDocumentId = resultId;
            record.Status = "COMPLETED";
            record.UpdatedDate = DateTime.UtcNow;
            await store.UpdateAsync(record, ct);
            await AuditAsync(record, providerCode, "IDEMPOTENCY_COMPLETED", oldStatus, record.Status);
            return Result.Success();
        }, ct);

    public Task MarkFailedAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        string? resultId = null,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync("FailIdempotencyRecord", async () =>
        {
            var record = await GetAndValidateAsync(
                organizationId,
                providerCode,
                operationType,
                idempotencyKey,
                ct);
            if (record.Status == "COMPLETED")
                return Result.Success();

            var oldStatus = record.Status;
            record.Status = "FAILED";
            if (!string.IsNullOrWhiteSpace(resultId))
                record.ResultDocumentId = resultId;
            record.UpdatedDate = DateTime.UtcNow;
            await store.UpdateAsync(record, ct);
            await AuditAsync(record, providerCode, "IDEMPOTENCY_FAILED", oldStatus, record.Status);
            return Result.Success();
        }, ct);

    private async Task<IdempotencyRecord> GetAndValidateAsync(
        int organizationId,
        EdoProviderCode providerCode,
        string operationType,
        string idempotencyKey,
        CancellationToken ct)
    {
        var record = await store.GetAsync(organizationId, idempotencyKey, ct)
            ?? throw new InvalidOperationException("The EDO idempotency record was not found.");
        ValidateRequest(record, operationType, record.RequestHash ?? string.Empty);
        return record;
    }

    private async Task AuditAsync(
        IdempotencyRecord record,
        EdoProviderCode providerCode,
        string operation,
        string? oldStatus,
        string newStatus)
    {
        var values = new
        {
            operation,
            organizationId = record.OrganizationId,
            providerCode = providerCode.ToString(),
            idempotencyKey = record.IdempotencyKey,
            operationType = record.OperationType,
            status = newStatus,
            resultDocumentId = record.ResultDocumentId
        };

        if (oldStatus is not null)
        {
            auditLogService.SetOldValues(new
            {
                operation,
                organizationId = record.OrganizationId,
                providerCode = providerCode.ToString(),
                idempotencyKey = record.IdempotencyKey,
                operationType = record.OperationType,
                status = oldStatus,
                resultDocumentId = record.ResultDocumentId
            });
        }

        auditLogService.SetNewValues(values);
        await auditLogService.CreateAsync(
            IdempotencyTable,
            record.Id.ToString(),
            oldStatus is null ? AuditLogOperationTypeConst.Create : AuditLogOperationTypeConst.Update);
    }

    private static void ValidateRequest(
        IdempotencyRecord record,
        string operationType,
        string requestHash)
    {
        if (!string.Equals(record.OperationType, operationType, StringComparison.Ordinal)
            || !string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The EDO idempotency key has already been used for a different request.");
        }
    }

    private static string BuildKey(
        EdoProviderCode providerCode,
        string operationType,
        string requestKey)
    {
        var source = $"EDO|{providerCode}|{operationType}|{requestKey}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
        return $"EDO_{providerCode}*{operationType}*{hash}";
    }
}
