using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features;
using Application.Features.AuditLogs;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Integration.Edo;

public sealed class EdoSigningSessionCleanupService(
    IUserContext userContext,
    IEdoAuthSigningSessionStore authSessionStore,
    IEdoDocumentSigningSessionStore documentSessionStore,
    IAuditLogService auditLogService,
    ILogger<EdoSigningSessionCleanupService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoSigningSessionCleanupService
{
    private const string AuthSessionTable = "edo_auth_signing_session";
    private const string DocumentSessionTable = "edo_document_signing_session";

    public async Task<EdoSigningSessionCleanupResultDto> CleanupAsync(
        int organizationId,
        EdoProviderCode? providerCode,
        TimeSpan consumedRetention,
        CancellationToken ct = default)
    {
        if (userContext.OrganizationId is not int currentOrganizationId)
            throw new EdoOrganizationScopeRequiredException();

        if (currentOrganizationId != organizationId)
            throw new InvalidOperationException(
                "Signing-session cleanup is outside the current organization scope.");

        if (providerCode is not null && !Enum.IsDefined(providerCode.Value))
            throw new EdoProviderNotFoundException(providerCode.Value.ToString());

        if (consumedRetention < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(consumedRetention));

        var now = DateTime.UtcNow;
        var cutoff = now - consumedRetention;
        var authRemoved = 0;
        var documentRemoved = 0;
        await ExecuteInTransactionAsync("CleanupSigningSessions", async () =>
        {
            authRemoved = await authSessionStore.CleanupAsync(
                organizationId, providerCode, now, cutoff, ct);
            documentRemoved = await documentSessionStore.CleanupAsync(
                organizationId, providerCode, now, cutoff, ct);

            if (authRemoved > 0)
            {
                auditLogService.SetOldValues(new
                {
                    operation = "AUTH_SIGNING_SESSION_CLEANUP",
                    organizationId,
                    providerCode = providerCode?.ToString() ?? "ALL",
                    status = "DELETED",
                    removedCount = authRemoved
                });
                await auditLogService.CreateAsync(
                    AuthSessionTable,
                    organizationId.ToString(),
                    AuditLogOperationTypeConst.Delete);
            }

            if (documentRemoved > 0)
            {
                auditLogService.SetOldValues(new
                {
                    operation = "DOCUMENT_SIGNING_SESSION_CLEANUP",
                    organizationId,
                    providerCode = providerCode?.ToString() ?? "ALL",
                    status = "DELETED",
                    removedCount = documentRemoved
                });
                await auditLogService.CreateAsync(
                    DocumentSessionTable,
                    organizationId.ToString(),
                    AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

        return new EdoSigningSessionCleanupResultDto
        {
            OrganizationId = organizationId,
            ProviderCode = providerCode,
            AuthSessionsRemoved = authRemoved,
            DocumentSessionsRemoved = documentRemoved,
            ExecutedAt = DateTimeOffset.UtcNow
        };
    }
}
