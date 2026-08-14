using Application.Abstractions;
using Application.Abstractions.Integration.Edo;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.PurchaseDocs;

public sealed class EdoImportPreflightProcessor : IEdoImportPreflightProcessor
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(30);
    private const int MaxProviderAttempts = 5;

    private readonly IEdoImportStore _store;
    private readonly IEdoProviderRegistry _providerRegistry;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EdoImportPreflightProcessor> _logger;

    public EdoImportPreflightProcessor(
        IEdoImportStore store,
        IEdoProviderRegistry providerRegistry,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        TimeProvider timeProvider,
        ILogger<EdoImportPreflightProcessor> logger)
    {
        _store = store;
        _providerRegistry = providerRegistry;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<long>> GetRunnableJobIdsAsync(CancellationToken ct = default) =>
        _store.GetRunnableJobIdsAsync(UtcNow(), ct);

    public async Task ProcessAsync(long jobId, string leaseOwner, CancellationToken ct = default)
    {
        var job = await _store.GetJobForProcessingAsync(jobId, ct);
        if (job is null || !EdoImportJobStatus.IsActive(job.Status))
            return;
        if (job.Providers.Count != 1)
        {
            await MarkJobFailedSafelyAsync(jobId, "INVALID_PROVIDER_SNAPSHOT");
            return;
        }

        var now = UtcNow();
        if (!await _store.TryAcquireLeaseAsync(
                job.OrganizationId,
                job.Id,
                leaseOwner,
                now,
                now.Add(LeaseDuration),
                ct))
            return;

        job = await _store.GetJobForProcessingAsync(jobId, ct);
        if (job is null)
            return;

        try
        {
            if (job.Status == EdoImportJobStatus.CancelRequested)
            {
                await FinalizeCancellationAsync(job, ct);
                return;
            }

            if (job.Status != EdoImportJobStatus.Scanning)
                await TransitionJobAsync(job, EdoImportJobStatus.Scanning, ct);

            foreach (var checkpoint in job.Providers.OrderBy(item => item.ProviderCode))
            {
                ct.ThrowIfCancellationRequested();
                job = await _store.GetJobForProcessingAsync(jobId, ct) ?? job;
                if (job.Status == EdoImportJobStatus.CancelRequested)
                {
                    await FinalizeCancellationAsync(job, ct);
                    return;
                }

                var currentCheckpoint = job.Providers.Single(item => item.ProviderCode == checkpoint.ProviderCode);
                if (currentCheckpoint.Status is EdoImportProviderCheckpointStatus.Completed
                    or EdoImportProviderCheckpointStatus.Failed
                    or EdoImportProviderCheckpointStatus.Cancelled)
                    continue;
                if (currentCheckpoint.NextRetryAt.HasValue && currentCheckpoint.NextRetryAt > UtcNow())
                    continue;

                try
                {
                    await ProcessProviderAsync(job, currentCheckpoint, leaseOwner, ct);
                    job = await _store.GetJobForProcessingAsync(jobId, ct) ?? job;
                    if (job.Status == EdoImportJobStatus.Cancelled)
                        return;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        "EDO import provider preflight failed; JobId={JobId}; Provider={Provider}; ExceptionType={ExceptionType}",
                        jobId,
                        currentCheckpoint.ProviderCode,
                        exception.GetType().Name);
                    await UpdateProviderFailureAsync(
                        job,
                        currentCheckpoint,
                        EdoImportProviderCheckpointStatus.Failed,
                        "PROVIDER_PROCESSING_FAILURE",
                        retry: false,
                        ct);
                }
            }

            job = await _store.GetJobForProcessingAsync(jobId, ct) ?? job;
            await FinalizeJobAsync(job, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await ReleaseLeaseSafelyAsync(jobId);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "EDO import preflight failed; JobId={JobId}; ExceptionType={ExceptionType}",
                jobId,
                exception.GetType().Name);
            await MarkJobFailedSafelyAsync(jobId, "PREFLIGHT_UNHANDLED_FAILURE");
        }
    }

    private async Task ProcessProviderAsync(
        EdoImportJob job,
        EdoImportJobProvider checkpoint,
        string leaseOwner,
        CancellationToken ct)
    {
        if (!Enum.TryParse<EdoProviderCode>(checkpoint.ProviderCode, true, out var providerCode))
        {
            await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.Failed,
                "PROVIDER_NOT_REGISTERED", retry: false, ct);
            return;
        }

        var source = _providerRegistry.ResolveHistoricalSource(providerCode);
        var executionContext = new EdoHistoricalExecutionContextDto(job.OrganizationId, job.InitiatedByUserId);
        var previousPageIds = Array.Empty<string>();
        var previousProviderTotal = checkpoint.ProviderTotal;
        var overlapRescanInProgress = string.Equals(
            checkpoint.SafeErrorCode,
            "OVERLAP_RESCAN_IN_PROGRESS",
            StringComparison.Ordinal);
        string? lastDetailExclusionCode = null;
        checkpoint.Status = EdoImportProviderCheckpointStatus.Scanning;
        checkpoint.IsWaitingAuth = false;
        if (!overlapRescanInProgress)
            checkpoint.SafeErrorCode = null;
        checkpoint.UpdatedDate = UtcNow();
        await PersistCheckpointAsync(job, checkpoint, leaseOwner, ct);

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            if (await _store.IsCancellationRequestedAsync(job.Id, ct))
            {
                await ReloadAndFinalizeCancellationAsync(job.Id, ct);
                return;
            }
            var request = new EdoHistoricalPageRequestDto
            {
                Page = checkpoint.CurrentPage,
                PageSize = checkpoint.PageSize,
                PreviousProviderTotal = previousProviderTotal,
                PreviousPageProviderDocumentIds = previousPageIds
            };
            var page = await source.ReadInboxPageAsync(executionContext, request, ct);
            checkpoint.LastAttemptAt = UtcNow();

            if (page.State == EdoHistoricalReadState.WAITING_AUTH)
            {
                await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.WaitingAuth,
                    page.SafeFailureCode ?? "AUTHENTICATION_REQUIRED", retry: false, ct);
                return;
            }
            if (page.State == EdoHistoricalReadState.TRANSIENT_FAILURE)
            {
                await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.Partial,
                    page.SafeFailureCode ?? "TRANSIENT_PROVIDER_FAILURE", retry: true, ct);
                return;
            }
            if (page.State is EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE
                or EdoHistoricalReadState.VALIDATION_FAILURE)
            {
                var incomplete = checkpoint.ScannedCount > 0
                    && checkpoint.ProviderTotal.HasValue
                    && checkpoint.ScannedCount < checkpoint.ProviderTotal.Value;
                await UpdateProviderFailureAsync(
                    job,
                    checkpoint,
                    incomplete
                        ? EdoImportProviderCheckpointStatus.Partial
                        : EdoImportProviderCheckpointStatus.Failed,
                    BuildPageFailureCode(
                        page.SafeFailureCode ?? "TERMINAL_PROVIDER_FAILURE",
                        incomplete,
                        lastDetailExclusionCode),
                    retry: false,
                    ct);
                return;
            }
            if (page.IsRepeatedPage)
            {
                await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.Partial,
                    "REPEATED_PROVIDER_PAGE", retry: true, ct);
                return;
            }

            foreach (var item in page.Items)
            {
                ct.ThrowIfCancellationRequested();
                if (await _store.IsCancellationRequestedAsync(job.Id, ct))
                {
                    await ReloadAndFinalizeCancellationAsync(job.Id, ct);
                    return;
                }
                if (item.Status != EdoDocumentStatusCode.SIGNED || item.Direction != EdoDirection.INBOX)
                    continue;
                if (await _store.FindJobCandidateAsync(
                        job.OrganizationId,
                        job.Id,
                        checkpoint.ProviderCode,
                        item.ProviderDocumentId,
                        ct) is not null)
                    continue;

                var detail = await source.ReadDetailAsync(executionContext, new EdoHistoricalDetailRequestDto
                {
                    Item = item,
                    DateFrom = job.DateFrom,
                    DateTo = job.DateTo
                }, ct);
                if (detail.State == EdoHistoricalReadState.WAITING_AUTH)
                {
                    await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.WaitingAuth,
                        detail.SafeFailureCode ?? "AUTHENTICATION_REQUIRED", retry: false, ct);
                    return;
                }
                if (detail.State == EdoHistoricalReadState.TRANSIENT_FAILURE)
                {
                    await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.Partial,
                        detail.SafeFailureCode ?? "TRANSIENT_PROVIDER_FAILURE", retry: true, ct);
                    return;
                }
                if (detail.State == EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE)
                {
                    await UpdateProviderFailureAsync(job, checkpoint, EdoImportProviderCheckpointStatus.Failed,
                        detail.SafeFailureCode ?? "TERMINAL_PROVIDER_FAILURE", retry: false, ct);
                    return;
                }
                if (detail.State == EdoHistoricalReadState.VALIDATION_FAILURE)
                {
                    var detailCode = detail.SafeFailureCode ?? "DETAIL_VALIDATION_FAILURE";
                    if (IsExpectedEligibilityExclusion(detailCode))
                    {
                        lastDetailExclusionCode = detailCode;
                        job.SkippedCount++;
                        continue;
                    }

                    await UpdateProviderFailureAsync(
                        job,
                        checkpoint,
                        EdoImportProviderCheckpointStatus.Failed,
                        BuildDetailFailureCode(checkpoint.ProviderCode, detailCode),
                        retry: false,
                        ct);
                    return;
                }
                if (!detail.IsImportReady || detail.Document is null)
                    continue;

                await AnalyzeAndPersistCandidateAsync(job, checkpoint, detail.Document, ct);
            }

            previousPageIds = page.Items.Select(item => item.ProviderDocumentId).ToArray();
            previousProviderTotal = page.ProviderTotal;
            checkpoint.ProviderTotal = page.ProviderTotal;
            checkpoint.ScannedCount += page.Items.Count;
            checkpoint.LastSuccessfulPage = page.Page;
            checkpoint.AttemptCount = 0;
            checkpoint.NextRetryAt = null;
            checkpoint.SafeErrorCode = overlapRescanInProgress
                ? "OVERLAP_RESCAN_IN_PROGRESS"
                : null;

            if (page.HasNextPage == true && page.NextPage.HasValue)
            {
                checkpoint.CurrentPage = page.NextPage.Value;
                await PersistCheckpointAsync(job, checkpoint, leaseOwner, ct);
                continue;
            }

            if (page.ProviderTotal.HasValue
                && checkpoint.ScannedCount < page.ProviderTotal.Value)
            {
                checkpoint.Status = EdoImportProviderCheckpointStatus.Partial;
                checkpoint.SafeErrorCode = BuildPaginationIncompleteCode(
                    checkpoint.ProviderCode,
                    lastDetailExclusionCode);
                await PersistCheckpointAsync(job, checkpoint, leaseOwner, ct);
                return;
            }

            if (page.RequiresOverlapRescan && !overlapRescanInProgress)
            {
                overlapRescanInProgress = true;
                checkpoint.CurrentPage = Math.Max(1, page.Page - 1);
                checkpoint.SafeErrorCode = "OVERLAP_RESCAN_IN_PROGRESS";
                previousPageIds = [];
                previousProviderTotal = page.ProviderTotal;
                await PersistCheckpointAsync(job, checkpoint, leaseOwner, ct);
                continue;
            }

            checkpoint.ScanCompletedAt = UtcNow();
            checkpoint.Status = page.IsCompletenessConfirmed
                ? EdoImportProviderCheckpointStatus.Completed
                : EdoImportProviderCheckpointStatus.Partial;
            checkpoint.SafeErrorCode = page.RequiresOverlapRescan
                ? "OVERLAP_RESCAN_APPLIED"
                : job.DiscoveredCount == 0 && !string.IsNullOrWhiteSpace(lastDetailExclusionCode)
                    ? BuildNoEligibleCandidatesCode(lastDetailExclusionCode)
                    : null;
            await PersistCheckpointAsync(job, checkpoint, leaseOwner, ct);
            return;
        }
    }

    private async Task AnalyzeAndPersistCandidateAsync(
        EdoImportJob job,
        EdoImportJobProvider checkpoint,
        EdoHistoricalDocumentDetailDto document,
        CancellationToken ct)
    {
        var now = UtcNow();
        var candidate = new EdoImportCandidate(
            job.Id,
            job.OrganizationId,
            checkpoint.ProviderCode,
            document.ProviderDocumentId,
            now)
        {
            Direction = document.Direction.ToString(),
            NormalizedStatus = document.Status.ToString(),
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            DocumentDate = document.DocumentDate,
            SellerTin = document.Seller?.Tin,
            BuyerTin = document.Buyer?.Tin,
            SellerName = document.Seller?.Name,
            ProviderContractNumber = document.ContractNumber,
            ProviderContractDate = document.ContractDate,
            NetAmount = document.NetAmount,
            VatAmount = document.VatAmount,
            TotalAmount = document.TotalAmount,
            SharedDocumentIdentity = $"{checkpoint.ProviderCode}:{document.ProviderDocumentId}"
        };
        candidate.HeaderFingerprint = CreateHeaderFingerprint(document);
        candidate.ContentFingerprint = CreateContentFingerprint(document, candidate.HeaderFingerprint);

        var mapping = await _store.ResolveMappingAsync(
            job.OrganizationId,
            checkpoint.ProviderCode,
            document.Seller!.Tin,
            document.DocumentDate!.Value,
            document.Lines,
            ct);
        candidate.SelectedCounterpartyId = mapping.CounterpartyId;
        candidate.SelectedContractId = mapping.ContractId;
        candidate.SelectedCurrencyId = mapping.CurrencyId;
        candidate.SelectedWarehouseId = mapping.WarehouseId;

        var markingValid = true;
        var markingStructureValid = true;
        string? markingFailureCode = null;
        foreach (var line in document.Lines.OrderBy(item => item.Number))
        {
            var lineMapping = mapping.Lines.GetValueOrDefault(line.Number) ?? new EdoImportLineMappingResolutionDto();
            var providerMarkings = line.MarkingNumbers
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
            var isService = line.IsService || lineMapping.IsService;
            var normalizedMarkings = isService ? [] : providerMarkings;
            var structuralFailure = EdoImportMarkingPolicy.ValidateStructure(
                lineMapping.IsPieceTracked,
                isService,
                line.Quantity,
                normalizedMarkings);
            var hasUsedMarking = normalizedMarkings.Any(mapping.PreviouslyUsedMarkings.Contains);
            var lineFailureCode = structuralFailure
                ?? (lineMapping.IsPieceTracked && hasUsedMarking
                    ? EdoImportMarkingPolicy.AlreadyUsed
                    : null);
            var lineMarkingValid = lineFailureCode is null;
            markingStructureValid &= structuralFailure is null;
            markingValid &= lineMarkingValid;
            markingFailureCode ??= lineFailureCode;

            var candidateLine = new EdoImportCandidateLine
            {
                ProviderLineNumber = line.Number,
                CatalogCode = line.CatalogCode,
                ProviderProductName = line.CatalogName,
                PackageCode = line.PackageCode,
                PackageName = line.PackageName,
                IsService = line.IsService,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                NetAmount = line.NetAmount,
                VatRate = line.VatRate,
                VatAmount = line.VatAmount,
                TotalAmount = line.TotalAmount,
                SelectedProductId = lineMapping.ProductId,
                SelectedUnitId = lineMapping.UnitId,
                SelectedVatRateId = lineMapping.VatRateId,
                MappingStatus = lineMapping.ProductId.HasValue
                    && lineMapping.UnitId.HasValue
                    && lineMapping.VatRateId.HasValue
                    && lineMarkingValid
                        ? EdoImportMappingStatus.Resolved
                        : EdoImportMappingStatus.Unresolved,
                CreatedDate = now
            };
            foreach (var marking in normalizedMarkings.Distinct(StringComparer.Ordinal))
            {
                candidateLine.Markings.Add(new EdoImportCandidateMarking
                {
                    MarkingNumber = marking,
                    ProviderVerificationState = lineMarkingValid
                        ? EdoImportMarkingVerificationState.Verified
                        : EdoImportMarkingVerificationState.Mismatch,
                    CreatedDate = now
                });
            }
            candidate.Lines.Add(candidateLine);
        }

        var allMapped = mapping.CounterpartyId.HasValue
            && mapping.ContractId.HasValue
            && mapping.CurrencyId.HasValue
            && mapping.WarehouseId.HasValue
            && candidate.Lines.Count > 0
            && candidate.Lines.All(line => line.MappingStatus == EdoImportMappingStatus.Resolved)
            && markingValid;
        candidate.MappingStatus = allMapped ? EdoImportMappingStatus.Resolved : EdoImportMappingStatus.Partial;

        var localDocument = await _store.FindEdoDocumentAsync(
            job.OrganizationId,
            checkpoint.ProviderCode,
            document.ProviderDocumentId,
            ct);
        candidate.EdoDocumentId = localDocument?.Id;
        var linkedPurchaseId = await _store.FindLinkedPurchaseIdAsync(
            job.OrganizationId,
            checkpoint.ProviderCode,
            document.ProviderDocumentId,
            ct);
        var priorProviderCandidate = await _store.FindPriorProviderCandidateAsync(
            job.OrganizationId,
            job.Id,
            checkpoint.ProviderCode,
            document.ProviderDocumentId,
            ct);
        var existingPurchaseId = ResolveExistingPurchaseId(linkedPurchaseId, priorProviderCandidate)
            ?? (markingStructureValid ? mapping.ExistingPurchaseIdForAllMarkings : null);
        if (existingPurchaseId.HasValue)
        {
            candidate.DuplicateState = EdoImportDuplicateState.Confirmed;
            candidate.ExistingPurchaseId = existingPurchaseId;
            candidate.TransitionTo(EdoImportCandidateStatus.Duplicate, now);
        }
        else if (!allMapped)
        {
            candidate.SafeErrorCode = ResolveMappingFailureCode(candidate, markingFailureCode);
            candidate.TransitionTo(EdoImportCandidateStatus.MappingRequired, now);
        }
        else
        {
            candidate.TransitionTo(EdoImportCandidateStatus.Ready, now);
        }

        await ExecuteInTransactionAsync(async () =>
        {
            await _store.AddCandidateGraphAsync(candidate, ct);
            job.DiscoveredCount++;
            switch (candidate.Status)
            {
                case EdoImportCandidateStatus.Ready:
                    job.ReadyCount++;
                    break;
                case EdoImportCandidateStatus.MappingRequired:
                    job.MappingRequiredCount++;
                    break;
                case EdoImportCandidateStatus.Duplicate:
                case EdoImportCandidateStatus.PossibleDuplicate:
                    job.DuplicateCount++;
                    break;
            }
            job.Heartbeat(UtcNow().Add(LeaseDuration), UtcNow());
            await _store.SaveChangesAsync(ct);
            _auditLog.SetNewValues(new
            {
                candidate.Id,
                candidate.JobId,
                candidate.ProviderCode,
                candidate.Status,
                candidate.MappingStatus,
                candidate.DuplicateState
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportCandidate,
                candidate.Id.ToString(),
                AuditLogOperationTypeConst.Create,
                organizationId: job.OrganizationId);
        }, ct);
    }

    private async Task PersistCheckpointAsync(
        EdoImportJob job,
        EdoImportJobProvider checkpoint,
        string leaseOwner,
        CancellationToken ct) =>
        await ExecuteInTransactionAsync(async () =>
        {
            var now = UtcNow();
            checkpoint.UpdatedDate = now;
            if (string.Equals(job.LeaseOwner, leaseOwner, StringComparison.Ordinal))
                job.Heartbeat(now.Add(LeaseDuration), now);
            await _store.SaveChangesAsync(ct);
        }, ct);

    private async Task UpdateProviderFailureAsync(
        EdoImportJob job,
        EdoImportJobProvider checkpoint,
        string status,
        string safeErrorCode,
        bool retry,
        CancellationToken ct) =>
        await ExecuteInTransactionAsync(async () =>
        {
            var now = UtcNow();
            checkpoint.Status = status;
            checkpoint.IsWaitingAuth = status == EdoImportProviderCheckpointStatus.WaitingAuth;
            checkpoint.SafeErrorCode = safeErrorCode;
            checkpoint.AttemptCount++;
            checkpoint.NextRetryAt = retry && checkpoint.AttemptCount < MaxProviderAttempts
                ? now.Add(GetRetryDelay(checkpoint.AttemptCount))
                : null;
            if (retry && checkpoint.AttemptCount >= MaxProviderAttempts)
                checkpoint.Status = EdoImportProviderCheckpointStatus.Failed;
            checkpoint.UpdatedDate = now;
            job.Heartbeat(now.Add(LeaseDuration), now);
            await _store.SaveChangesAsync(ct);
        }, ct);

    private async Task FinalizeJobAsync(EdoImportJob job, CancellationToken ct)
    {
        var status = ResolveFinalJobStatus(job.Providers.Single());
        if (status == EdoImportJobStatus.Failed)
            await TransitionJobAsync(job, EdoImportJobStatus.Failed, ct);
        else
            await TransitionJobAndReleaseLeaseAsync(job, status, ct);
    }

    internal static string ResolveFinalJobStatus(EdoImportJobProvider provider) => provider.Status switch
    {
        EdoImportProviderCheckpointStatus.Completed => EdoImportJobStatus.PreflightReady,
        EdoImportProviderCheckpointStatus.Partial => EdoImportJobStatus.Partial,
        EdoImportProviderCheckpointStatus.WaitingAuth => EdoImportJobStatus.WaitingAuth,
        EdoImportProviderCheckpointStatus.Failed => EdoImportJobStatus.Failed,
        _ => EdoImportJobStatus.Partial
    };

    internal static string BuildPageFailureCode(
        string pageFailureCode,
        bool incomplete,
        string? lastDetailExclusionCode)
    {
        var code = incomplete
            ? $"{pageFailureCode}_INCOMPLETE"
            : pageFailureCode;
        if (!string.IsNullOrWhiteSpace(lastDetailExclusionCode))
            code = $"{code}_LAST_DETAIL_{lastDetailExclusionCode}";
        return code.Length <= 100 ? code : code[..100];
    }

    internal static string BuildDetailFailureCode(string providerCode, string detailFailureCode)
    {
        if (detailFailureCode.StartsWith($"{providerCode}_HISTORICAL_DETAIL_", StringComparison.Ordinal)
            || detailFailureCode.StartsWith($"{providerCode}_DETAIL_", StringComparison.Ordinal))
        {
            return detailFailureCode.Length <= 100 ? detailFailureCode : detailFailureCode[..100];
        }

        var code = $"{providerCode}_DETAIL_VALIDATION_{detailFailureCode}";
        return code.Length <= 100 ? code : code[..100];
    }

    internal static string BuildNoEligibleCandidatesCode(string lastDetailExclusionCode)
    {
        var code = $"NO_ELIGIBLE_CANDIDATES_LAST_DETAIL_{lastDetailExclusionCode}";
        return code.Length <= 100 ? code : code[..100];
    }

    internal static string BuildPaginationIncompleteCode(
        string providerCode,
        string? lastDetailExclusionCode)
    {
        var code = $"{providerCode}_PAGINATION_INCOMPLETE";
        if (!string.IsNullOrWhiteSpace(lastDetailExclusionCode))
            code = $"{code}_LAST_DETAIL_{lastDetailExclusionCode}";
        return code.Length <= 100 ? code : code[..100];
    }

    internal static bool IsExpectedEligibilityExclusion(string code) => code is
        "UNSUPPORTED_DOCUMENT_TYPE"
        or "DOCUMENT_DATE_REQUIRED"
        or "DOCUMENT_OUTSIDE_DATE_RANGE"
        or "BUYER_ORGANIZATION_MISMATCH"
        or "SELLER_TIN_REQUIRED"
        or "SELLER_BUYER_MUST_DIFFER"
        or "PROVIDER_LINE_NUMBER_INVALID"
        or "PROVIDER_LINE_NUMBER_DUPLICATE"
        or "DOCUMENT_LINES_REQUIRED"
        or "PROVIDER_PRODUCT_NAME_INVALID";

    private Task FinalizeCancellationAsync(EdoImportJob job, CancellationToken ct) =>
        ExecuteInTransactionAsync(async () =>
        {
            var now = UtcNow();
            foreach (var provider in job.Providers.Where(item =>
                         item.Status is not EdoImportProviderCheckpointStatus.Completed
                             and not EdoImportProviderCheckpointStatus.Failed))
            {
                provider.Status = EdoImportProviderCheckpointStatus.Cancelled;
                provider.IsWaitingAuth = false;
                provider.UpdatedDate = now;
            }
            job.TransitionTo(EdoImportJobStatus.Cancelled, now);
            await _store.SaveChangesAsync(ct);
        }, ct);

    private async Task ReloadAndFinalizeCancellationAsync(long jobId, CancellationToken ct)
    {
        var job = await _store.GetJobForProcessingAsync(jobId, ct);
        if (job is not null && job.Status == EdoImportJobStatus.CancelRequested)
            await FinalizeCancellationAsync(job, ct);
    }

    private Task TransitionJobAsync(EdoImportJob job, string status, CancellationToken ct) =>
        ExecuteInTransactionAsync(async () =>
        {
            job.TransitionTo(status, UtcNow());
            await _store.SaveChangesAsync(ct);
        }, ct);

    private Task TransitionJobAndReleaseLeaseAsync(EdoImportJob job, string status, CancellationToken ct) =>
        ExecuteInTransactionAsync(async () =>
        {
            var now = UtcNow();
            job.TransitionTo(status, now);
            job.ClearLease(now);
            await _store.SaveChangesAsync(ct);
        }, ct);

    private async Task MarkJobFailedSafelyAsync(long jobId, string code)
    {
        try
        {
            var job = await _store.GetJobForProcessingAsync(jobId);
            if (job is null || !EdoImportJobStatus.IsActive(job.Status))
                return;
            await ExecuteInTransactionAsync(async () =>
            {
                job.SafeErrorCode = code;
                job.TransitionTo(EdoImportJobStatus.Failed, UtcNow());
                await _store.SaveChangesAsync();
            }, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Unable to persist EDO preflight failure; JobId={JobId}; ExceptionType={ExceptionType}",
                jobId,
                exception.GetType().Name);
        }
    }

    private async Task ReleaseLeaseSafelyAsync(long jobId)
    {
        try
        {
            var job = await _store.GetJobForProcessingAsync(jobId);
            if (job is null || job.LeaseOwner is null)
                return;
            await ExecuteInTransactionAsync(async () =>
            {
                job.ClearLease(UtcNow());
                await _store.SaveChangesAsync();
            }, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Unable to release EDO preflight lease; JobId={JobId}; ExceptionType={ExceptionType}",
                jobId,
                exception.GetType().Name);
        }
    }

    private async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct)
    {
        await _unitOfWork.BeginAsync(ct);
        try
        {
            await action();
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    private static long? ResolveExistingPurchaseId(
        long? linkedPurchaseId,
        EdoImportCandidate? priorProviderCandidate)
    {
        return linkedPurchaseId
            ?? priorProviderCandidate?.ImportedPurchaseId
            ?? priorProviderCandidate?.ExistingPurchaseId;
    }

    private static string ResolveMappingFailureCode(
        EdoImportCandidate candidate,
        string? markingFailureCode)
    {
        if (markingFailureCode is not null)
            return markingFailureCode;
        if (!candidate.SelectedCounterpartyId.HasValue)
            return "COUNTERPARTY_MAPPING_REQUIRED";
        if (!candidate.SelectedContractId.HasValue)
            return "CONTRACT_MAPPING_REQUIRED";
        if (!candidate.SelectedCurrencyId.HasValue)
            return "CURRENCY_MAPPING_REQUIRED";
        if (!candidate.SelectedWarehouseId.HasValue)
            return "WAREHOUSE_MAPPING_REQUIRED";
        return "LINE_MAPPING_REQUIRED";
    }

    internal static string CreateHeaderFingerprint(EdoHistoricalDocumentDetailDto document) =>
        Hash(string.Join('|',
            Normalize(document.Buyer?.Tin),
            Normalize(document.Seller?.Tin),
            Normalize(document.DocumentNumber),
            document.DocumentDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Decimal(document.TotalAmount)));

    internal static string CreateContentFingerprint(
        EdoHistoricalDocumentDetailDto document,
        string headerFingerprint)
    {
        var lines = document.Lines.OrderBy(line => line.Number).Select(line => string.Join(':',
            Normalize(line.CatalogCode),
            Decimal(line.Quantity),
            Decimal(line.NetAmount),
            Decimal(line.VatAmount),
            Decimal(line.TotalAmount)));
        var markings = document.MarkingNumbers
            .Concat(document.Lines.SelectMany(line => line.MarkingNumbers))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Normalize)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);
        return Hash(string.Join('|', headerFingerprint, string.Join(';', lines), string.Join(';', markings)));
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;
    private static string Decimal(decimal? value) => value?.ToString("0.############################", CultureInfo.InvariantCulture) ?? string.Empty;
    private static TimeSpan GetRetryDelay(int attempt) => TimeSpan.FromMinutes(
        Math.Min(Math.Pow(2, Math.Max(0, attempt - 1)), MaxRetryDelay.TotalMinutes));
    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
