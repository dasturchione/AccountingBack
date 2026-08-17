using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public sealed class EdoImportPreflightService : BaseService, IEdoImportPreflightService, IEdoBulkDraftImportProcessor
{
    private const string ServiceItemsNotAllowedDraftFailure =
        "DRAFT_IMPORT_PURCHASEDOC_SERVICEITEMSNOTALLOWED";
    private const string ProductPieceTrackingDraftFailure =
        "DRAFT_IMPORT_PRODUCT_PIECE_TRACKING_REQUIRED";
    private const string EarlierDocumentDateDraftFailure =
        "DRAFT_IMPORT_DOCUMENTNUMBER_EARLIERDOCUMENTDATE";
    private const string GenericValidationDraftFailure =
        "DRAFT_IMPORT_PURCHASEFROMEDO_VALIDATIONFAILED";
    private const string BackgroundOrganizationRequiredDraftFailure =
        "DRAFT_IMPORT_COMMON_USERHASNOORGANIZATION";
    private const string DocumentNumberInvalidOrganizationDraftFailure =
        "DRAFT_IMPORT_DOCUMENTNUMBER_INVALIDORGANIZATION";
    private const string BackgroundOrganizationRecoveryExhaustedFailure =
        "DRAFT_IMPORT_BACKGROUND_ORGANIZATION_RECOVERY_EXHAUSTED";

    private readonly ILogger<EdoImportPreflightService> _logger;
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;
    private readonly IActiveEdoProviderResolver _activeProviderResolver;
    private readonly IEdoImportStore _store;
    private readonly IEdoImportPreflightScheduler _scheduler;
    private readonly IAuditLogService _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IEdoHistoricalPurchaseDraftFactory? _draftFactory;
    private readonly IBackgroundOrganizationScope? _backgroundOrganizationScope;
    private readonly IQueryBuilder _queryBuilder;

    public EdoImportPreflightService(
        ILogger<EdoImportPreflightService> logger,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IQueryRepository<OrganizationConfig> organizationConfigQuery,
        IActiveEdoProviderResolver activeProviderResolver,
        IEdoImportStore store,
        IEdoImportPreflightScheduler scheduler,
        IAuditLogService auditLog,
        TimeProvider timeProvider,
        IQueryBuilder queryBuilder) : this(
            logger, unitOfWork, userContext, organizationConfigQuery,
            activeProviderResolver, store, scheduler, auditLog, timeProvider, queryBuilder, null, null)
    {
    }

    public EdoImportPreflightService(
        ILogger<EdoImportPreflightService> logger,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IQueryRepository<OrganizationConfig> organizationConfigQuery,
        IActiveEdoProviderResolver activeProviderResolver,
        IEdoImportStore store,
        IEdoImportPreflightScheduler scheduler,
        IAuditLogService auditLog,
        TimeProvider timeProvider,
        IQueryBuilder queryBuilder,
        IEdoHistoricalPurchaseDraftFactory? draftFactory,
        IBackgroundOrganizationScope? backgroundOrganizationScope = null) : base(logger, unitOfWork)
    {
        _logger = logger;
        _userContext = userContext;
        _organizationConfigQuery = organizationConfigQuery;
        _activeProviderResolver = activeProviderResolver;
        _store = store;
        _scheduler = scheduler;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _draftFactory = draftFactory;
        _backgroundOrganizationScope = backgroundOrganizationScope;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<EdoImportJobDto>> StartAsync(
        EdoImportPreflightRequestDto request,
        CancellationToken ct = default)
    {
        var result = await ExecuteInTransactionAsync(nameof(StartAsync), async () =>
        {
            if (!_userContext.OrganizationId.HasValue || !_userContext.Id.HasValue)
                return Result.Failure<EdoImportJobDto>(Error.Forbidden(
                    "EdoImport.OrganizationContextRequired",
                    "An authenticated organization and user context are required."));

            var organizationId = _userContext.OrganizationId.Value;
            var dateFrom = request.DateFrom;
            if (!dateFrom.HasValue)
            {
                var config = await _organizationConfigQuery.GetAsync(_queryBuilder.For<OrganizationConfig>()
                    .Where(item => item.OrganizationId == organizationId)
                    .Build(), ct);
                dateFrom = config?.AccountingStartDate;
            }

            if (!dateFrom.HasValue)
                return Result.Failure<EdoImportJobDto>(Error.Business(
                    "EdoImport.AccountingStartDateRequired",
                    "OrganizationConfig.AccountingStartDate must be configured when dateFrom is omitted."));
            if (dateFrom.Value > request.DateTo)
                return Result.Failure<EdoImportJobDto>(Error.Business(
                    "EdoImport.InvalidDateRange",
                    "DateFrom must not be later than DateTo."));
            if (await _store.FindActiveJobAsync(organizationId, ct) is not null)
                return Result.Failure<EdoImportJobDto>(Error.Conflict(
                    "EdoImport.ActiveJobExists",
                    "An active EDO import job already exists for this organization."));

            var activeProvider = await _activeProviderResolver.GetActiveProviderCodeAsync(ct);
            if (activeProvider is not EdoProviderCode.EDOCS and not EdoProviderCode.DIDOX)
                return Result.Failure<EdoImportJobDto>(Error.Business(
                    "EdoImport.ActiveProviderUnsupported",
                    "Historical Purchase preflight is not supported for the active EDO provider."));

            var now = UtcNow();
            var job = new EdoImportJob(
                organizationId,
                _userContext.Id.Value,
                dateFrom.Value,
                request.DateTo,
                now);
            await _store.AddJobAsync(job, ct);
            var providerCode = activeProvider.ToString();
            var provider = new EdoImportJobProvider
            {
                JobId = job.Id,
                ProviderCode = providerCode,
                Status = EdoImportProviderCheckpointStatus.Queued,
                CurrentPage = 1,
                PageSize = 100,
                CreatedDate = now
            };
            await _store.AddProviderAsync(organizationId, provider, ct);

            _auditLog.SetNewValues(new
            {
                job.Id,
                job.DateFrom,
                job.DateTo,
                Provider = providerCode
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Create,
                organizationId: organizationId);

            return Result.Success(job.ToDto());
        }, ct);

        if (result.IsSuccess)
            await _scheduler.ScheduleAsync(result.Value.Id, ct);

        return result;
    }

    public async Task<Result<EdoImportJobDto>> GetJobAsync(long jobId, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportJobDto>(OrganizationRequired());

        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        return job is null
            ? Result.Failure<EdoImportJobDto>(NotFound(jobId))
            : Result.Success(job.ToDto());
    }

    public async Task<Result<PagedResponse<EdoImportCandidateListDto>>> GetCandidatesAsync(
        long jobId,
        EdoImportCandidateListFilter filter,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<PagedResponse<EdoImportCandidateListDto>>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<PagedResponse<EdoImportCandidateListDto>>(NotFound(jobId));

        var page = await _store.GetCandidatesAsync(
            organizationId.Value,
            jobId,
            filter.Page,
            filter.PageSize,
            ct);
        var totalPages = page.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(page.TotalCount / (double)filter.PageSize);
        return Result.Success(new PagedResponse<EdoImportCandidateListDto>
        {
            Items = page.Items.Select(item => item.ToListDto()).ToArray(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = totalPages,
            HasPreviousPage = filter.Page > 1,
            HasNextPage = filter.Page < totalPages
        });
    }

    public async Task<Result<EdoImportCandidateDetailDto>> GetCandidateAsync(
        long jobId,
        long candidateId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportCandidateDetailDto>(OrganizationRequired());

        var candidate = await _store.GetCandidateAsync(organizationId.Value, candidateId, ct);
        return candidate is null || candidate.JobId != jobId
            ? Result.Failure<EdoImportCandidateDetailDto>(Error.NotFound(
                "EdoImport.CandidateNotFound",
                "The EDO import candidate was not found in this organization job."))
            : Result.Success(candidate.ToDetailDto());
    }

    public async Task<Result<EdoImportMappingSummaryDto>> GetMappingSummaryAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportMappingSummaryDto>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<EdoImportMappingSummaryDto>(NotFound(jobId));

        var candidates = await _store.GetMappingSummaryCandidatesAsync(
            organizationId.Value,
            jobId,
            ct);
        return Result.Success(BuildMappingSummary(jobId, candidates));
    }

    public async Task<Result<EdoImportMasterDataPlanDto>> GetMasterDataPlanAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportMasterDataPlanDto>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<EdoImportMasterDataPlanDto>(NotFound(jobId));

        var source = await _store.GetMasterDataPlanSourceAsync(
            organizationId.Value,
            jobId,
            ct);
        return Result.Success(BuildMasterDataPlanWithHash(jobId, source));
    }

    public async Task<Result<EdoImportProductConflictPlanDto>> GetProductConflictsAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportProductConflictPlanDto>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<EdoImportProductConflictPlanDto>(NotFound(jobId));
        var source = await _store.GetProductConflictSourceAsync(organizationId.Value, jobId, ct);
        return Result.Success(BuildProductConflictPlan(jobId, source));
    }

    public async Task<Result<EdoImportMarkingConflictPlanDto>> GetMarkingConflictsAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportMarkingConflictPlanDto>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<EdoImportMarkingConflictPlanDto>(NotFound(jobId));

        var source = await _store.GetMarkingConflictSourceAsync(organizationId.Value, jobId, ct);
        return Result.Success(BuildMarkingConflictPlan(jobId, source));
    }

    public async Task<Result<EdoImportDraftPlanDto>> GetImportPlanAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportDraftPlanDto>(OrganizationRequired());
        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        if (job is null)
            return Result.Failure<EdoImportDraftPlanDto>(NotFound(jobId));
        var candidates = await _store.GetReadyImportCandidatesAsync(
            organizationId.Value, jobId, ct);
        return Result.Success(BuildImportPlan(job, candidates));
    }

    public async Task<Result<EdoImportDraftBatchResponseDto>> ImportDraftsAsync(
        long jobId,
        EdoImportDraftBatchRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportDraftBatchResponseDto>(OrganizationRequired());
        if (!request.Confirm)
            return Result.Failure<EdoImportDraftBatchResponseDto>(Error.Business(
                "DRAFT_IMPORT_CONFIRMATION_REQUIRED",
                "Explicit Draft Purchase import confirmation is required."));
        if (request.BatchSize is < 1 or > 50 || request.ExpectedImportPlanHash.Length != 64)
            return Result.Failure<EdoImportDraftBatchResponseDto>(Error.Business(
                "DRAFT_IMPORT_REQUEST_INVALID",
                "Batch size must be between 1 and 50 and the import plan hash is required."));
        if (_draftFactory is null)
            return Result.Failure<EdoImportDraftBatchResponseDto>(Error.Conflict(
                "DRAFT_IMPORT_FACTORY_UNAVAILABLE",
                "The historical Draft Purchase factory is unavailable."));

        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        if (job is null)
            return Result.Failure<EdoImportDraftBatchResponseDto>(NotFound(jobId));
        if (!IsDraftImportAllowed(job.Status))
            return Result.Failure<EdoImportDraftBatchResponseDto>(Error.Conflict(
                "EdoImport.JobNotImportable",
                "Draft import is allowed only for PREFLIGHT_READY or PARTIAL jobs."));
        var readyCandidates = await _store.GetReadyImportCandidatesAsync(
            organizationId.Value, jobId, ct);
        var plan = BuildImportPlan(job, readyCandidates);
        if (!string.Equals(
            request.ExpectedImportPlanHash, plan.ImportPlanHash, StringComparison.Ordinal))
            return Result.Failure<EdoImportDraftBatchResponseDto>(StaleImportPlan());

        var selectedIds = readyCandidates.OrderBy(candidate => candidate.Id)
            .Take(request.BatchSize).Select(candidate => candidate.Id).ToArray();
        var createdCount = 0;
        var reusedCount = 0;
        var failureCodes = new List<string>();
        foreach (var candidateId in selectedIds)
        {
            try
            {
                var imported = await ImportDraftCandidateAsync(
                    organizationId.Value, jobId, candidateId, ct);
                if (imported.IsSuccess)
                {
                    if (imported.Value.Created)
                        createdCount++;
                    else
                        reusedCount++;
                }
                else
                {
                    var safeCode = BuildDraftImportFailureCode(imported.Error.Code);
                    _store.ClearTracking();
                    var recovered = await TryRecoverDraftCandidateAsync(
                        organizationId.Value, jobId, candidateId, ct);
                    if (recovered.IsSuccess && recovered.Value)
                    {
                        reusedCount++;
                        continue;
                    }
                    _store.ClearTracking();
                    await MarkDraftCandidateFailedAsync(
                        organizationId.Value, jobId, candidateId, safeCode, ct);
                    failureCodes.Add(safeCode);
                }
            }
            catch
            {
                const string safeCode = "DRAFT_IMPORT_PROCESSING_FAILURE";
                _store.ClearTracking();
                try
                {
                    var recovered = await TryRecoverDraftCandidateAsync(
                        organizationId.Value, jobId, candidateId, ct);
                    if (recovered.IsSuccess && recovered.Value)
                    {
                        reusedCount++;
                        continue;
                    }
                    _store.ClearTracking();
                    await MarkDraftCandidateFailedAsync(
                        organizationId.Value, jobId, candidateId, safeCode, ct);
                }
                catch
                {
                    _store.ClearTracking();
                }
                failureCodes.Add(safeCode);
            }
            finally
            {
                _store.ClearTracking();
            }
        }

        var finalJob = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        if (finalJob is null)
            return Result.Failure<EdoImportDraftBatchResponseDto>(NotFound(jobId));
        return Result.Success(new EdoImportDraftBatchResponseDto
        {
            JobId = finalJob.Id,
            JobStatus = finalJob.Status,
            ProcessedCandidateCount = createdCount + reusedCount + failureCodes.Count,
            CreatedDraftCount = createdCount,
            ReusedDraftCount = reusedCount,
            FailedCandidateCount = failureCodes.Count,
            RemainingReadyCount = finalJob.ReadyCount,
            ImportedCount = finalJob.ImportedCount,
            FailedCount = finalJob.FailedCount,
            Failures = failureCodes.GroupBy(code => code, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new EdoImportDraftFailureSummaryDto
                {
                    SafeErrorCode = group.Key,
                    Count = group.Count()
                }).ToArray()
        });
    }

    public async Task<Result<EdoImportBulkDraftStatusDto>> StartBulkImportAsync(
        long jobId,
        EdoImportBulkDraftStartRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportBulkDraftStatusDto>(OrganizationRequired());
        if (!request.Confirm || request.BatchSize != 50
            || request.ExpectedImportPlanHash.Length != 64
            || request.LineValuesInvalidPolicy != "SKIP"
            || request.MarkingAlreadyUsedPolicy
                != "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP")
            return Result.Failure<EdoImportBulkDraftStatusDto>(Error.Business(
                "BULK_DRAFT_IMPORT_REQUEST_INVALID",
                "Bulk Draft import requires explicit confirmation, current plan hash and supported policies."));
        if (_draftFactory is null)
            return Result.Failure<EdoImportBulkDraftStatusDto>(Error.Conflict(
                "DRAFT_IMPORT_FACTORY_UNAVAILABLE",
                "The historical Draft Purchase factory is unavailable."));

        var result = await ExecuteInTransactionAsync(nameof(StartBulkImportAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportBulkDraftStatusDto>(NotFound(jobId));
            if (!IsDraftImportAllowed(job.Status))
                return Result.Failure<EdoImportBulkDraftStatusDto>(Error.Conflict(
                    "EdoImport.JobNotImportable",
                    "Bulk Draft import is allowed only for PREFLIGHT_READY or PARTIAL jobs."));
            var plan = BuildImportPlan(job, await _store.GetReadyImportCandidatesAsync(
                organizationId.Value, jobId, ct));
            if (!string.Equals(request.ExpectedImportPlanHash, plan.ImportPlanHash, StringComparison.Ordinal))
                return Result.Failure<EdoImportBulkDraftStatusDto>(StaleImportPlan());
            if (EdoImportBulkImportStatus.IsActive(job.BulkImportStatus)
                && job.BulkImportStatus != EdoImportBulkImportStatus.Paused)
                return Result.Failure<EdoImportBulkDraftStatusDto>(Error.Conflict(
                    "BULK_DRAFT_IMPORT_ACTIVE",
                    "A bulk Draft import is already active for this organization."));

            var wasPaused = job.BulkImportStatus == EdoImportBulkImportStatus.Paused;
            var now = UtcNow();
            job.BulkImportStatus = EdoImportBulkImportStatus.Queued;
            job.BulkBatchSize = 50;
            job.BulkLineValuesInvalidPolicy = request.LineValuesInvalidPolicy;
            job.BulkMarkingAlreadyUsedPolicy = request.MarkingAlreadyUsedPolicy;
            job.BulkStartedAt ??= now;
            job.BulkCompletedAt = null;
            job.BulkCancelRequestedAt = null;
            if (!wasPaused)
                job.BulkLastSafeErrorCode = null;
            ReconcileBulkCounters(job);
            await _store.SaveChangesAsync(ct);
            return Result.Success(ToBulkImportStatus(job));
        }, ct);
        if (result.IsSuccess)
            await _scheduler.ScheduleBulkImportAsync(jobId, ct);
        return result;
    }

    public async Task<Result<EdoImportBulkDraftStatusDto>> GetBulkImportStatusAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportBulkDraftStatusDto>(OrganizationRequired());
        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        return job is null
            ? Result.Failure<EdoImportBulkDraftStatusDto>(NotFound(jobId))
            : Result.Success(ToBulkImportStatus(job));
    }

    public Task<Result<EdoImportBulkDraftStatusDto>> CancelBulkImportAsync(
        long jobId,
        CancellationToken ct = default) => ExecuteInTransactionAsync(nameof(CancelBulkImportAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportBulkDraftStatusDto>(OrganizationRequired());
            var job = await _store.GetJobForImportAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportBulkDraftStatusDto>(NotFound(jobId));
            if (!EdoImportBulkImportStatus.IsActive(job.BulkImportStatus)
                && job.BulkImportStatus != EdoImportBulkImportStatus.Cancelled)
                return Result.Failure<EdoImportBulkDraftStatusDto>(Error.Conflict(
                    "BULK_DRAFT_IMPORT_NOT_ACTIVE", "No active bulk Draft import exists."));
            if (job.BulkImportStatus != EdoImportBulkImportStatus.Cancelled)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Cancelled;
                job.BulkCancelRequestedAt = UtcNow();
                job.BulkCompletedAt = job.BulkCancelRequestedAt;
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success(ToBulkImportStatus(job));
        }, ct);

    public Task<IReadOnlyCollection<long>> GetRunnableBulkImportJobIdsAsync(
        CancellationToken ct = default) => _store.GetRunnableBulkImportJobIdsAsync(ct);

    public async Task ProcessBulkImportAsync(
        long jobId,
        string workerId,
        CancellationToken ct = default)
    {
        var initial = await _store.GetJobForProcessingAsync(jobId, ct);
        if (initial is null || !EdoImportBulkImportStatus.IsRunnable(initial.BulkImportStatus)
            || _draftFactory is null)
            return;
        var organizationId = initial.OrganizationId;
        _ = workerId;

        using var organizationScope = _backgroundOrganizationScope?.Enter(
            organizationId, "EDO_BULK_DRAFT_IMPORT");
        try
        {
            await SetBulkRunningAsync(organizationId, jobId, ct);

            while (!ct.IsCancellationRequested)
            {
                var state = await _store.GetJobAsync(organizationId, jobId, ct);
                if (state is null || state.BulkImportStatus == EdoImportBulkImportStatus.CancelRequested)
                {
                    await SetBulkCancelledAsync(organizationId, jobId, ct);
                    return;
                }
                if (state.BulkImportStatus != EdoImportBulkImportStatus.Running)
                    return;

                var failures = await _store.GetDraftImportFailuresAsync(organizationId, jobId, ct);
                var backgroundScopeFailure = failures.OrderBy(item => item.CandidateId).FirstOrDefault(item =>
                    IsBackgroundOrganizationRecoveryFailure(item.SafeErrorCode));
                if (backgroundScopeFailure is not null)
                {
                    var recovery = await RequeueBackgroundOrganizationFailureAsync(
                        organizationId, jobId, backgroundScopeFailure.CandidateId, ct);
                    if (recovery != BackgroundOrganizationRecoveryOutcome.Requeued)
                    {
                        await PauseBulkImportAsync(
                            organizationId,
                            jobId,
                            recovery == BackgroundOrganizationRecoveryOutcome.Exhausted
                                ? BackgroundOrganizationRecoveryExhaustedFailure
                                : backgroundScopeFailure.SafeErrorCode!,
                            ct,
                            countFailure: false);
                        return;
                    }
                    continue;
                }
                var unresolvedFailure = failures.OrderBy(item => item.CandidateId).FirstOrDefault(item =>
                    item.SafeErrorCode != EdoImportDraftFailurePolicy.LineValuesInvalid
                    && !EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(item.SafeErrorCode));
            if (unresolvedFailure is not null)
            {
                await PauseBulkImportAsync(
                    organizationId, jobId, unresolvedFailure.SafeErrorCode, ct, countFailure: false);
                    return;
                }
                foreach (var failure in failures.OrderBy(item => item.CandidateId))
                {
                    var outcome = await ResolveBulkFailureAsync(organizationId, jobId, failure.CandidateId, ct);
                    if (outcome == BulkFailureOutcome.Paused)
                        return;
                }

                var ready = await _store.GetReadyImportCandidatesAsync(organizationId, jobId, ct);
                var ids = ready.OrderBy(candidate => candidate.Id).Take(50).Select(candidate => candidate.Id).ToArray();
                if (ids.Length == 0)
                {
                    await CompleteBulkImportAsync(organizationId, jobId, ct);
                    return;
                }

                foreach (var candidateId in ids)
                {
                    var latest = await _store.GetJobAsync(organizationId, jobId, ct);
                    if (latest?.BulkImportStatus == EdoImportBulkImportStatus.CancelRequested)
                    {
                        await SetBulkCancelledAsync(organizationId, jobId, ct);
                        return;
                    }
                    Result<DraftImportOutcome> import;
                    try
                    {
                        import = await ImportDraftCandidateAsync(organizationId, jobId, candidateId, ct);
                    }
                    catch
                    {
                        const string processingFailureCode = "DRAFT_IMPORT_PROCESSING_FAILURE";
                        _store.ClearTracking();
                        try
                        {
                            await MarkDraftCandidateFailedAsync(
                                organizationId, jobId, candidateId, processingFailureCode, ct);
                        }
                        catch
                        {
                            _store.ClearTracking();
                        }
                        await PauseBulkImportAsync(organizationId, jobId, processingFailureCode, ct);
                        return;
                    }
                    if (import.IsSuccess)
                    {
                        await AddBulkProgressAsync(organizationId, jobId,
                            import.Value.Created ? 1 : 0, import.Value.Created ? 0 : 1,
                            0, 0, 0, null, ct);
                        continue;
                    }

                    var safeCode = BuildDraftImportFailureCode(import.Error.Code);
                    _store.ClearTracking();
                    var recovered = await TryRecoverDraftCandidateAsync(organizationId, jobId, candidateId, ct);
                    if (recovered.IsSuccess && recovered.Value)
                    {
                        await AddBulkProgressAsync(organizationId, jobId, 0, 1, 0, 0, 0, null, ct);
                        continue;
                    }
                    _store.ClearTracking();
                    await MarkDraftCandidateFailedAsync(organizationId, jobId, candidateId, safeCode, ct);
                    _store.ClearTracking();
                    if (safeCode == EdoImportDraftFailurePolicy.LineValuesInvalid
                        || EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(safeCode))
                    {
                        if (await ResolveBulkFailureAsync(organizationId, jobId, candidateId, ct)
                            != BulkFailureOutcome.Paused)
                            continue;
                        return;
                    }
                    await PauseBulkImportAsync(organizationId, jobId, safeCode, ct);
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            _store.ClearTracking();
            await PauseBulkImportAsync(organizationId, jobId, "DRAFT_IMPORT_PROCESSING_FAILURE", ct);
        }
    }

    public async Task<Result<EdoImportDraftFailureListDto>> GetDraftImportFailuresAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportDraftFailureListDto>(OrganizationRequired());
        if (await _store.GetJobAsync(organizationId.Value, jobId, ct) is null)
            return Result.Failure<EdoImportDraftFailureListDto>(NotFound(jobId));

        var failures = await _store.GetDraftImportFailuresAsync(
            organizationId.Value, jobId, ct);
        return Result.Success(BuildDraftFailurePlan(jobId, failures));
    }

    public Task<Result<EdoImportDraftFailureApplyResponseDto>> ApplyDraftImportFailuresAsync(
        long jobId,
        EdoImportDraftFailureApplyRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ApplyDraftImportFailuresAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(OrganizationRequired());
            if (!request.Confirm)
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(Error.Business(
                    "DRAFT_IMPORT_FAILURE_CONFIRMATION_REQUIRED",
                    "Explicit Draft import failure confirmation is required."));
            if (request.ExpectedFailureHash.Length != 64
                || request.Items.Count == 0
                || request.Items.Any(item => item.CandidateId <= 0
                    || item.Action is not ("SKIP" or "MARK_DUPLICATE"))
                || request.Items.Select(item => item.CandidateId).Distinct().Count()
                    != request.Items.Count)
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                    InvalidDraftFailureSelection());

            var job = await _store.GetJobForImportAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(NotFound(jobId));
            if (!IsDraftImportAllowed(job.Status))
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(Error.Conflict(
                    "EdoImport.JobNotImportable",
                    "Draft failure resolution is allowed only for PREFLIGHT_READY or PARTIAL jobs."));

            var source = await _store.GetDraftImportFailuresAsync(
                organizationId.Value, jobId, ct);
            var plan = BuildDraftFailurePlan(jobId, source);
            var requestedIds = request.Items.Select(item => item.CandidateId).Order().ToArray();
            if (!string.Equals(
                    request.ExpectedFailureHash, plan.FailureHash, StringComparison.Ordinal))
            {
                foreach (var item in request.Items)
                {
                    var replay = await _store.GetCandidateForImportAsync(
                        organizationId.Value, jobId, item.CandidateId, ct);
                    if (replay is null
                        || replay.JobId != jobId
                        || (item.Action == "SKIP"
                            && (replay.Status != EdoImportCandidateStatus.Skipped
                                || !EdoImportDraftFailurePolicy.IsSkipped(replay.SafeErrorCode)))
                        || (item.Action == "MARK_DUPLICATE"
                            && (replay.Status != EdoImportCandidateStatus.Duplicate
                                || replay.DuplicateState != EdoImportDuplicateState.Confirmed
                                || !replay.ExistingPurchaseId.HasValue)))
                        return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                            StaleDraftFailurePlan());
                }

                return Result.Success(ToDraftFailureApplyResponse(job, 0, 0));
            }

            var planById = plan.Items.ToDictionary(item => item.CandidateId);
            if (request.Items.Any(requestItem =>
                    !planById.TryGetValue(requestItem.CandidateId, out var planItem)
                    || !IsDraftFailureActionAllowedByCode(
                        planItem.SafeErrorCode, requestItem.Action)))
                return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                    InvalidDraftFailureSelection());

            var decisions = new List<DraftFailureDecision>(requestedIds.Length);
            foreach (var item in request.Items.OrderBy(item => item.CandidateId))
            {
                var candidate = await _store.GetCandidateForImportAsync(
                    organizationId.Value, jobId, item.CandidateId, ct);
                if (candidate is null
                    || candidate.JobId != jobId
                    || candidate.Status != EdoImportCandidateStatus.Failed
                    || !IsDraftFailureActionAllowedByCode(candidate.SafeErrorCode, item.Action)
                    || candidate.ImportedPurchaseId.HasValue
                    || candidate.ExistingPurchaseId.HasValue)
                    return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                        InvalidDraftFailureSelection());

                var markings = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                    .SelectMany(line => line.Markings.OrderBy(marking => marking.Id))
                    .Select(marking => marking.MarkingNumber).ToArray();
                await _store.AcquireDraftImportLocksAsync(
                    organizationId.Value,
                    candidate.ProviderCode,
                    candidate.ProviderDocumentId,
                    markings,
                    ct);
                if (await _store.FindExistingPurchaseForProviderDocumentAsync(
                        organizationId.Value,
                        candidate.ProviderCode,
                        candidate.ProviderDocumentId,
                        ct) is not null)
                    return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                        InvalidDraftFailureSelection());

                long? duplicatePurchaseId = null;
                if (EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(candidate.SafeErrorCode))
                {
                    var usage = await _store.GetDraftMarkingUsageAsync(
                        organizationId.Value, markings, ct);
                    var expectedUsage = planById[candidate.Id];
                    if (usage.TotalMarkingCount != expectedUsage.TotalMarkingCount
                        || usage.UsedMarkingCount != expectedUsage.UsedMarkingCount
                        || !usage.ExistingPurchaseIds.SequenceEqual(
                            expectedUsage.ExistingPurchaseIds))
                        return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                            StaleDraftFailurePlan());
                    if (item.Action == "MARK_DUPLICATE")
                    {
                        duplicatePurchaseId = usage.ExistingPurchaseIdForAllMarkings;
                        if (!duplicatePurchaseId.HasValue)
                            return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                                InvalidDraftFailureSelection());
                    }
                    else if (!usage.HasPartialOrMultiplePurchaseConflict)
                    {
                        return Result.Failure<EdoImportDraftFailureApplyResponseDto>(
                            InvalidDraftFailureSelection());
                    }
                }

                decisions.Add(new DraftFailureDecision(
                    candidate, item.Action, duplicatePurchaseId));
            }

            var now = UtcNow();
            foreach (var decision in decisions)
            {
                if (decision.Action == "MARK_DUPLICATE")
                {
                    decision.Candidate.DuplicateState = EdoImportDuplicateState.Confirmed;
                    decision.Candidate.ExistingPurchaseId = decision.DuplicatePurchaseId;
                    decision.Candidate.SafeErrorCode = null;
                    decision.Candidate.TransitionTo(EdoImportCandidateStatus.Duplicate, now);
                }
                else
                {
                    decision.Candidate.SafeErrorCode = EdoImportDraftFailurePolicy.ToSkipped(
                        decision.Candidate.SafeErrorCode!);
                    decision.Candidate.TransitionTo(EdoImportCandidateStatus.Skipped, now);
                }
            }

            var skippedCount = decisions.Count(decision => decision.Action == "SKIP");
            var markedDuplicateCount = decisions.Count - skippedCount;
            job.FailedCount = Math.Max(0, job.FailedCount - decisions.Count);
            job.SkippedCount += skippedCount;
            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);
            _auditLog.SetNewValues(new
            {
                job.Id,
                SkippedCandidateCount = skippedCount,
                MarkedDuplicateCandidateCount = markedDuplicateCount,
                job.FailedCount,
                job.SkippedCount,
                job.DuplicateCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);
            return Result.Success(ToDraftFailureApplyResponse(
                job, skippedCount, markedDuplicateCount));
        }, ct);

    public Task<Result<EdoImportDraftRequeueResponseDto>> RequeueDraftCandidateAsync(
        long jobId,
        long candidateId,
        EdoImportDraftRequeueRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RequeueDraftCandidateAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(OrganizationRequired());
            if (!request.Confirm)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(Error.Business(
                    "DRAFT_IMPORT_REQUEUE_CONFIRMATION_REQUIRED",
                    "Explicit Draft import requeue confirmation is required."));

            var job = await _store.GetJobForImportAsync(organizationId.Value, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId.Value, jobId, candidateId, ct);
            if (job is null || candidate is null)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(Error.NotFound(
                    "DRAFT_IMPORT_CANDIDATE_NOT_FOUND",
                    "The Draft import candidate was not found in this organization job."));
            if (!IsDraftImportAllowed(job.Status))
                return Result.Failure<EdoImportDraftRequeueResponseDto>(Error.Conflict(
                    "EdoImport.JobNotImportable",
                    "Draft import requeue is allowed only for PREFLIGHT_READY or PARTIAL jobs."));
            if (candidate.Status == EdoImportCandidateStatus.Imported
                || candidate.ImportedPurchaseId.HasValue
                || candidate.ExistingPurchaseId.HasValue)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(DraftRequeuePurchaseLinked());

            var markings = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                .SelectMany(line => line.Markings.OrderBy(marking => marking.Id))
                .Select(marking => marking.MarkingNumber).ToArray();
            await _store.AcquireDraftImportLocksAsync(
                organizationId.Value, candidate.ProviderCode,
                candidate.ProviderDocumentId, markings, ct);
            if (await _store.FindExistingPurchaseForProviderDocumentAsync(
                    organizationId.Value, candidate.ProviderCode,
                    candidate.ProviderDocumentId, ct) is not null)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(DraftRequeuePurchaseLinked());

            if (candidate.Status == EdoImportCandidateStatus.Ready)
                return Result.Success(new EdoImportDraftRequeueResponseDto
                {
                    JobId = job.Id,
                    CandidateId = candidate.Id,
                    CandidateStatus = candidate.Status,
                    Requeued = false,
                    ReadyCount = job.ReadyCount,
                    FailedCount = job.FailedCount
                });
            if (candidate.Status != EdoImportCandidateStatus.Failed
                || !CanRequeueDraftFailure(candidate.SafeErrorCode))
                return Result.Failure<EdoImportDraftRequeueResponseDto>(Error.Conflict(
                    "DRAFT_IMPORT_REQUEUE_NOT_ALLOWED",
                    "Only an unlinked candidate failed by a corrected Draft validation can be requeued."));
            if (string.IsNullOrWhiteSpace(candidate.SellerTin)
                || !candidate.DocumentDate.HasValue)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(DraftImportSourceInvalid());

            var now = UtcNow();
            var mapping = await _store.ResolveSelectedMappingAsync(
                organizationId.Value,
                candidate.SellerTin,
                candidate.DocumentDate.Value,
                ToHistoricalLines(candidate),
                ToPersistedMappingSelection(candidate),
                ct);
            ApplyMapping(candidate, mapping, preserveAccounts: true, now);
            if (candidate.Status != EdoImportCandidateStatus.Ready)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(Error.Conflict(
                    "DRAFT_IMPORT_REQUEUE_MAPPING_INVALID",
                    "The candidate mappings or marking state are no longer valid for Draft import."));
            var source = BuildHistoricalDraftSource(candidate);
            if (!source.IsSuccess)
                return Result.Failure<EdoImportDraftRequeueResponseDto>(source.Error);

            job.FailedCount = Math.Max(0, job.FailedCount - 1);
            job.ReadyCount++;
            await _store.SaveChangesAsync(ct);
            _auditLog.SetNewValues(new
            {
                candidate.Id,
                candidate.Status
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportCandidate,
                candidate.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);
            return Result.Success(new EdoImportDraftRequeueResponseDto
            {
                JobId = job.Id,
                CandidateId = candidate.Id,
                CandidateStatus = candidate.Status,
                Requeued = true,
                ReadyCount = job.ReadyCount,
                FailedCount = job.FailedCount
            });
        }, ct);

    public async Task<Result<EdoImportPieceTrackingPlanDto>> GetPieceTrackingPlanAsync(
        long jobId,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportPieceTrackingPlanDto>(OrganizationRequired());
        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        if (job is null)
            return Result.Failure<EdoImportPieceTrackingPlanDto>(NotFound(jobId));
        var source = await _store.GetMasterDataPlanSourceAsync(organizationId.Value, jobId, ct);
        return Result.Success(BuildPieceTrackingPlan(jobId, source));
    }

    public Task<Result<EdoImportPieceTrackingApplyResponseDto>> ApplyPieceTrackingAsync(
        long jobId,
        EdoImportPieceTrackingApplyRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ApplyPieceTrackingAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(OrganizationRequired());
            if (!request.Confirm)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(Error.Business(
                    "PIECE_TRACKING_CONFIRMATION_REQUIRED",
                    "Explicit piece-tracking confirmation is required."));
            if (request.ProductIds.Count == 0
                || request.ProductIds.Any(id => id <= 0)
                || request.ProductIds.Distinct().Count() != request.ProductIds.Count)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(Error.Business(
                    "PIECE_TRACKING_SELECTION_INVALID",
                    "Product IDs must be positive and unique."));

            var job = await _store.GetJobForImportAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(JobNotMappable());
            await _store.AcquireMasterDataApplyLockAsync(organizationId.Value, ct);
            var source = await _store.GetMasterDataPlanSourceAsync(organizationId.Value, jobId, ct);
            var plan = BuildPieceTrackingPlan(jobId, source);
            var selectedIds = request.ProductIds.Order().ToArray();
            var currentSelectionsValid = string.Equals(
                    request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal)
                && selectedIds.All(id => plan.Products.Any(item =>
                    item.ProductId == id
                    && item.SafeAction == "ENABLE_PIECE_TRACKING"));
            var replayPlan = BuildPieceTrackingPlan(
                jobId, source, selectedIds.ToHashSet());
            var idempotentReplay = string.Equals(
                    request.ExpectedPlanHash, replayPlan.PlanHash, StringComparison.Ordinal)
                && selectedIds.All(id => source.Products.Any(product =>
                    product.Id == id
                    && !product.IsService
                    && product.IsPieceTracked));
            if (!currentSelectionsValid && !idempotentReplay)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(Error.Conflict(
                    string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal)
                        ? "PIECE_TRACKING_SELECTION_INVALID"
                        : "STALE_PIECE_TRACKING_PLAN",
                    string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal)
                        ? "Only products explicitly offered for piece-tracking can be selected."
                        : "The piece-tracking plan has changed; refresh it before applying changes."));

            var applied = idempotentReplay
                ? new EdoImportPieceTrackingApplyStoreResultDto
                {
                    ReusedProductCount = selectedIds.Length
                }
                : await _store.ApplyPieceTrackingAsync(
                    organizationId.Value, selectedIds, ct);
            if (applied.SafeErrorCode is not null)
                return Result.Failure<EdoImportPieceTrackingApplyResponseDto>(
                    Error.Conflict(applied.SafeErrorCode,
                        "The selected product cannot be safely enabled for piece tracking."));

            var candidates = await _store.GetPieceTrackingCandidatesAsync(
                organizationId.Value, jobId, selectedIds, ct);
            var now = UtcNow();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.SellerTin)
                    || !candidate.DocumentDate.HasValue)
                    continue;
                var mapping = await _store.ResolveSelectedMappingAsync(
                    organizationId.Value,
                    candidate.SellerTin,
                    candidate.DocumentDate.Value,
                    ToHistoricalLines(candidate),
                    ToPersistedMappingSelection(candidate),
                    ct);
                ApplyMapping(candidate, mapping, preserveAccounts: true, now);
            }

            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);
            _auditLog.SetNewValues(new
            {
                job.Id,
                ProductIds = selectedIds,
                applied.UpdatedProductCount,
                applied.ReusedProductCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);
            return Result.Success(new EdoImportPieceTrackingApplyResponseDto
            {
                JobId = job.Id,
                UpdatedProductCount = applied.UpdatedProductCount,
                ReusedProductCount = applied.ReusedProductCount,
                ReadyCount = job.ReadyCount,
                MappingRequiredCount = job.MappingRequiredCount,
                FailedCount = job.FailedCount
            });
        }, ct);

    public Task<Result<EdoImportMasterDataApplyResponseDto>> ApplyMasterDataAsync(
        long jobId,
        EdoImportMasterDataApplyRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ApplyMasterDataAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(OrganizationRequired());
            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(JobNotMappable());
            if (!request.Confirm)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(Error.Business(
                    "MASTER_DATA_CONFIRMATION_REQUIRED",
                    "Explicit master-data apply confirmation is required."));

            await _store.AcquireMasterDataApplyLockAsync(organizationId.Value, ct);
            var source = await _store.GetMasterDataPlanSourceAsync(organizationId.Value, jobId, ct);
            var plan = BuildMasterDataPlanWithHash(jobId, source);
            var stalePlan = !string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal);
            var commandResult = stalePlan
                ? BuildIdempotentReplayCommand(request, source)
                : BuildApplyCommand(plan, request);
            if (!commandResult.IsSuccess)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(stalePlan
                    ? StaleMasterDataPlan()
                    : commandResult.Error);
            var applied = await _store.ApplyMasterDataAsync(
                organizationId.Value, commandResult.Value, UtcNow(), ct);
            if (applied.SafeErrorCode is not null)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(
                    stalePlan ? StaleMasterDataPlan() : Error.Conflict(
                        applied.SafeErrorCode,
                        "The selected master-data item cannot be applied safely."));

            var candidates = await _store.GetMappingRequiredCandidatesAsync(
                organizationId.Value, jobId, ct);
            var now = UtcNow();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.SellerTin) || !candidate.DocumentDate.HasValue)
                    continue;
                var normalizedCandidateTin = NormalizeSummaryText(candidate.SellerTin)!;
                var candidateContractKey = candidate.ProviderContractNumber is not null
                    && candidate.ProviderContractDate.HasValue
                        ? ContractKey(
                            candidate.ProviderCode,
                            normalizedCandidateTin,
                            NormalizeSummaryText(candidate.ProviderContractNumber)!,
                            candidate.ProviderContractDate.Value)
                        : null;
                var affected = applied.CounterpartyIdsBySellerTin.ContainsKey(normalizedCandidateTin)
                    || candidateContractKey is not null
                        && applied.ContractIdsByKey.ContainsKey(candidateContractKey)
                    || applied.ContractIdsByCandidateId.ContainsKey(candidate.Id)
                    || candidate.Lines.Any(line => NormalizeSummaryText(line.CatalogCode) is { } code
                        && applied.ProductIdsByCatalogCode.ContainsKey(code));
                if (!affected)
                    continue;
                var lines = ToHistoricalLines(candidate);
                var automatic = await _store.ResolveMappingAsync(
                    organizationId.Value, candidate.ProviderCode, candidate.SellerTin,
                    candidate.DocumentDate.Value, lines, ct);
                var normalizedSellerTin = NormalizeSummaryText(candidate.SellerTin)!;
                var counterpartyId = applied.CounterpartyIdsBySellerTin.TryGetValue(
                    normalizedSellerTin, out var selectedCounterpartyId)
                        ? selectedCounterpartyId
                        : automatic.CounterpartyId ?? candidate.SelectedCounterpartyId;
                var normalizedContractNumber = NormalizeSummaryText(candidate.ProviderContractNumber);
                var contractKey = normalizedContractNumber is not null
                    && candidate.ProviderContractDate.HasValue
                        ? ContractKey(candidate.ProviderCode, normalizedSellerTin, normalizedContractNumber,
                            candidate.ProviderContractDate.Value)
                        : null;
                var contractId = applied.ContractIdsByCandidateId.TryGetValue(
                        candidate.Id, out var explicitlySelectedContractId)
                    ? explicitlySelectedContractId
                    : contractKey is not null
                    && applied.ContractIdsByKey.TryGetValue(contractKey, out var selectedContractId)
                        ? selectedContractId
                        : automatic.ContractId ?? candidate.SelectedContractId;
                var lineSelections = candidate.Lines.ToDictionary(line => line.ProviderLineNumber, line =>
                {
                    var resolved = automatic.Lines.GetValueOrDefault(line.ProviderLineNumber)
                        ?? new EdoImportLineMappingResolutionDto();
                    var catalogCode = NormalizeSummaryText(line.CatalogCode);
                    var productId = catalogCode is not null
                        && applied.ProductIdsByCatalogCode.TryGetValue(catalogCode, out var selectedProductId)
                            ? selectedProductId
                            : resolved.ProductId;
                    return new EdoImportLineMappingSelectionDto
                    {
                        ProductId = productId,
                        UnitId = resolved.UnitId,
                        VatRateId = resolved.VatRateId,
                        DebitAccountId = line.SelectedDebitAccountId,
                        VatAccountId = line.SelectedVatAccountId
                    };
                });
                var resolvedMapping = await _store.ResolveSelectedMappingAsync(
                    organizationId.Value,
                    candidate.SellerTin,
                    candidate.DocumentDate.Value,
                    lines,
                    new EdoImportMappingSelectionDto
                    {
                        ProviderCode = candidate.ProviderCode,
                        ProviderContractNumber = candidate.ProviderContractNumber,
                        ProviderContractDate = candidate.ProviderContractDate,
                        CounterpartyId = counterpartyId,
                        ContractId = contractId,
                        CurrencyId = automatic.CurrencyId,
                        WarehouseId = automatic.WarehouseId,
                        Lines = lineSelections
                    },
                    ct);
                var appliedContractIsValid = contractId.HasValue
                    && counterpartyId.HasValue
                    && await _store.IsContractApplicableAsync(
                        organizationId.Value,
                        counterpartyId.Value,
                        contractId.Value,
                        candidate.DocumentDate.Value,
                        ct);
                var persistedContractId = resolvedMapping.ContractId
                    ?? (appliedContractIsValid ? contractId : null);
                ApplyMapping(candidate, new EdoImportMappingResolutionDto
                {
                    CounterpartyId = resolvedMapping.CounterpartyId,
                    ContractId = persistedContractId,
                    CurrencyId = resolvedMapping.CurrencyId,
                    WarehouseId = resolvedMapping.WarehouseId,
                    Lines = resolvedMapping.Lines,
                    PreviouslyUsedMarkings = resolvedMapping.PreviouslyUsedMarkings
                }, preserveAccounts: true, now);
            }

            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);
            var finalSource = await _store.GetMasterDataPlanSourceAsync(organizationId.Value, jobId, ct);
            var finalPlan = BuildMasterDataPlanWithHash(jobId, finalSource);
            var safeErrorCodes = finalSource.Candidates
                .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired
                    && !string.IsNullOrWhiteSpace(candidate.SafeErrorCode))
                .Select(candidate => candidate.SafeErrorCode!)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            _auditLog.SetNewValues(new
            {
                job.Id,
                applied.CreatedCounterpartyCount,
                applied.ReusedCounterpartyCount,
                applied.CreatedContractCount,
                applied.ReusedContractCount,
                applied.CreatedProductCount,
                applied.ReusedProductCount,
                job.ReadyCount,
                job.MappingRequiredCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);

            return Result.Success(new EdoImportMasterDataApplyResponseDto
            {
                JobId = job.Id,
                CreatedCounterpartyCount = applied.CreatedCounterpartyCount,
                ReusedCounterpartyCount = applied.ReusedCounterpartyCount,
                CreatedContractCount = applied.CreatedContractCount,
                ReusedContractCount = applied.ReusedContractCount,
                CreatedProductCount = applied.CreatedProductCount,
                ReusedProductCount = applied.ReusedProductCount,
                ReadyCount = job.ReadyCount,
                MappingRequiredCount = job.MappingRequiredCount,
                ConflictCount = finalPlan.ConflictCount,
                BlockedCount = finalPlan.BlockedCount,
                SafeErrorCodes = safeErrorCodes
            });
        }, ct);

    public async Task<Result<EdoImportMasterDataApplyResponseDto>> ApplyProductDefaultsAsync(
        long jobId,
        EdoImportProductDefaultsApplyRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        if (!organizationId.HasValue)
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(OrganizationRequired());
        var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
        if (job is null)
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(NotFound(jobId));
        if (!IsMappingAllowed(job.Status))
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(JobNotMappable());
        if (!request.Confirm)
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(Error.Business(
                "MASTER_DATA_CONFIRMATION_REQUIRED",
                "Explicit product defaults confirmation is required."));
        if (!string.Equals(request.MarkingPolicy, "PIECE_TRACKED_WHEN_REQUIRED", StringComparison.Ordinal))
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(Error.Business(
                "MARKING_POLICY_INVALID",
                "The marking policy must explicitly preserve piece tracking when provider markings are required."));
        if (request.PackageUnitMappings.Count == 0
            || request.PackageUnitMappings.GroupBy(item => item.PackageName, StringComparer.Ordinal)
                .Any(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 1))
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(Error.Business(
                "PACKAGE_UNIT_MAPPING_INVALID",
                "Package to unit mappings must be non-empty, exact and unique."));

        var packageUnits = request.PackageUnitMappings.ToDictionary(
            item => item.PackageName, item => item.UnitId, StringComparer.Ordinal);
        if (packageUnits.Values.Any(unitId => unitId <= 0))
            return Result.Failure<EdoImportMasterDataApplyResponseDto>(Error.Business(
                "PACKAGE_UNIT_MAPPING_INVALID",
                "Every package mapping must reference a positive unit ID."));

        var source = await _store.GetMasterDataPlanSourceAsync(organizationId.Value, jobId, ct);
        var plan = BuildMasterDataPlanWithHash(jobId, source);
        IReadOnlyCollection<EdoImportProductPlanItemDto> selectedProducts;
        if (string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal))
        {
            selectedProducts = plan.Products.Where(item =>
                    item.Action == "BLOCKED"
                    && item.BlockedReasonCodes.Count == 1
                    && item.BlockedReasonCodes.Contains("PRODUCT_UNIT_REQUIRED", StringComparer.Ordinal)
                    && item.ProviderProductName is { Length: > 0 }
                    && item.CatalogCode is { Length: > 0 }
                    && item.IsService.HasValue
                    && item.ResolvedVatRateId.HasValue
                    && item.PackageName is not null
                    && packageUnits.ContainsKey(item.PackageName))
                .ToArray();
        }
        else
        {
            var stillPending = plan.Products.Any(item =>
                item.Action == "BLOCKED"
                && item.BlockedReasonCodes.Count == 1
                && item.BlockedReasonCodes.Contains("PRODUCT_UNIT_REQUIRED", StringComparer.Ordinal)
                && item.PackageName is not null
                && packageUnits.ContainsKey(item.PackageName));
            if (stillPending)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(StaleMasterDataPlan());

            selectedProducts = BuildProductReplayPlanItems(source)
                .Where(item => item.Action == "USE_EXISTING"
                    && HasPersistedProductSelection(source, item)
                    && item.ProviderProductName is { Length: > 0 }
                    && item.CatalogCode is { Length: > 0 }
                    && item.IsService.HasValue
                    && item.ResolvedVatRateId.HasValue
                    && item.ResolvedUnitId.HasValue
                    && item.PackageName is not null
                    && packageUnits.TryGetValue(item.PackageName, out var unitId)
                    && unitId == item.ResolvedUnitId.Value)
                .ToArray();
            if (selectedProducts.Count == 0)
                return Result.Failure<EdoImportMasterDataApplyResponseDto>(StaleMasterDataPlan());
        }

        var applyRequest = new EdoImportMasterDataApplyRequestDto
        {
            Confirm = true,
            ExpectedPlanHash = request.ExpectedPlanHash,
            Products = selectedProducts.Select(item => new EdoImportProductApplyItemDto
            {
                CatalogCode = item.CatalogCode!,
                Action = "CREATE",
                IsService = item.IsService,
                IsPieceTracked = item.MarkingRequired,
                UnitId = packageUnits[item.PackageName!],
                VatRateId = item.ResolvedVatRateId
            }).ToArray()
        };
        return await ApplyMasterDataAsync(jobId, applyRequest, ct);
    }

    public Task<Result<EdoImportProductConflictApplyResponseDto>> ApplyProductConflictsAsync(
        long jobId,
        EdoImportProductConflictApplyRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ApplyProductConflictsAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(OrganizationRequired());
            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(JobNotMappable());
            if (!request.Confirm)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(Error.Business(
                    "PRODUCT_CONFLICT_CONFIRMATION_REQUIRED",
                    "Explicit provider product mapping confirmation is required."));
            var requestedIdentityKeys = request.Items.SelectMany(item => item.IdentityKeys).ToArray();
            if (request.Items.Count == 0 || request.Items.Any(item => item.IdentityKeys.Count == 0)
                || requestedIdentityKeys.Any(key => key.Length != 64)
                || requestedIdentityKeys.Distinct(StringComparer.Ordinal).Count() != requestedIdentityKeys.Length)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(Error.Business(
                    "PRODUCT_CONFLICT_SELECTION_INVALID",
                    "Provider product conflict selections must be non-empty and unique."));

            await _store.AcquireMasterDataApplyLockAsync(organizationId.Value, ct);
            var source = await _store.GetProductConflictSourceAsync(organizationId.Value, jobId, ct);
            var plan = BuildProductConflictPlan(jobId, source);
            var stale = !string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal);
            var commandResult = stale
                ? BuildProductConflictReplayCommand(request, source)
                : BuildProductConflictApplyCommand(plan, request, reuseOnly: false);
            if (!commandResult.IsSuccess)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(stale
                    ? StaleProductConflictPlan()
                    : commandResult.Error);
            var applied = await _store.ApplyProductConflictMappingsAsync(
                organizationId.Value, commandResult.Value, UtcNow(), ct);
            if (applied.SafeErrorCode is not null)
                return Result.Failure<EdoImportProductConflictApplyResponseDto>(
                    applied.SafeErrorCode == "STALE_PRODUCT_CONFLICT_PLAN"
                        ? StaleProductConflictPlan()
                        : Error.Conflict(applied.SafeErrorCode,
                            "The provider product mapping cannot be applied safely."));

            var candidates = await _store.GetMappingRequiredCandidatesAsync(
                organizationId.Value, jobId, ct);
            var now = UtcNow();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.SellerTin) || !candidate.DocumentDate.HasValue)
                    continue;
                var mapping = await _store.ResolveSelectedMappingAsync(
                    organizationId.Value,
                    candidate.SellerTin,
                    candidate.DocumentDate.Value,
                    ToHistoricalLines(candidate),
                    ToPersistedMappingSelection(candidate),
                    ct);
                ApplyMapping(candidate, mapping, preserveAccounts: false, now);
            }
            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);
            _auditLog.SetNewValues(new
            {
                job.Id,
                applied.CreatedProductCount,
                applied.ReusedProductCount,
                applied.CreatedMappingCount,
                applied.ReusedMappingCount,
                job.ReadyCount,
                job.MappingRequiredCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);
            return Result.Success(new EdoImportProductConflictApplyResponseDto
            {
                JobId = job.Id,
                CreatedProductCount = applied.CreatedProductCount,
                ReusedProductCount = applied.ReusedProductCount,
                CreatedMappingCount = applied.CreatedMappingCount,
                ReusedMappingCount = applied.ReusedMappingCount,
                ReadyCount = job.ReadyCount,
                MappingRequiredCount = job.MappingRequiredCount
            });
        }, ct);

    public Task<Result<EdoImportMarkingConflictApplyResponseDto>> ApplyMarkingConflictsAsync(
        long jobId,
        EdoImportMarkingConflictApplyRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ApplyMarkingConflictsAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(OrganizationRequired());
            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(JobNotMappable());
            if (!request.Confirm)
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(Error.Business(
                    "MARKING_CONFLICT_CONFIRMATION_REQUIRED",
                    "Explicit marking conflict confirmation is required."));
            if (request.ExpectedConflictHash.Length != 64
                || request.Items.Count == 0
                || request.Items.Any(item => item.CandidateId <= 0 || item.Action != "SKIP")
                || request.Items.Select(item => item.CandidateId).Distinct().Count() != request.Items.Count)
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(Error.Business(
                    "MARKING_CONFLICT_SELECTION_INVALID",
                    "Marking conflict selections must be non-empty, unique and use the SKIP action."));

            await _store.AcquireMasterDataApplyLockAsync(organizationId.Value, ct);
            var source = await _store.GetMarkingConflictSourceAsync(organizationId.Value, jobId, ct);
            var plan = BuildMarkingConflictPlan(jobId, source);
            var requestedIds = request.Items.Select(item => item.CandidateId).ToHashSet();
            var stale = !string.Equals(
                request.ExpectedConflictHash, plan.ConflictHash, StringComparison.Ordinal);
            if (stale)
            {
                var idempotentReplay = requestedIds.All(candidateId => source.Any(candidate =>
                    candidate.CandidateId == candidateId
                    && candidate.Status == EdoImportCandidateStatus.Skipped
                    && EdoImportMarkingPolicy.IsSkippedConflict(candidate.SafeErrorCode)));
                if (!idempotentReplay)
                    return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(
                        StaleMarkingConflictPlan());

                return Result.Success(ToMarkingConflictApplyResponse(job, 0));
            }

            if (requestedIds.Any(candidateId => plan.Items.All(item => item.CandidateId != candidateId)))
                return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(Error.Conflict(
                    "MARKING_CONFLICT_SELECTION_INVALID",
                    "Only current supported marking conflict candidates can be skipped."));

            var now = UtcNow();
            var skippedCandidateCount = 0;
            foreach (var candidateId in requestedIds.Order())
            {
                var candidate = await _store.GetCandidateAsync(organizationId.Value, candidateId, ct);
                if (candidate is null
                    || candidate.JobId != jobId
                    || candidate.Status != EdoImportCandidateStatus.MappingRequired
                    || !EdoImportMarkingPolicy.CanSkipConflict(candidate.SafeErrorCode))
                    return Result.Failure<EdoImportMarkingConflictApplyResponseDto>(
                        StaleMarkingConflictPlan());

                candidate.SafeErrorCode = EdoImportMarkingPolicy.ToSkippedConflict(
                    candidate.SafeErrorCode!);
                candidate.TransitionTo(EdoImportCandidateStatus.Skipped, now);
                skippedCandidateCount++;
            }

            job.SkippedCount += skippedCandidateCount;
            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);

            _auditLog.SetNewValues(new
            {
                job.Id,
                SkippedCandidateCount = skippedCandidateCount,
                job.ReadyCount,
                job.MappingRequiredCount,
                job.DuplicateCount,
                job.SkippedCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);

            return Result.Success(ToMarkingConflictApplyResponse(job, skippedCandidateCount));
        }, ct);

    private static bool HasPersistedProductSelection(
        EdoImportMasterDataPlanSourceDto source,
        EdoImportProductPlanItemDto item) =>
        item.ExistingProductId.HasValue
        && source.Candidates.SelectMany(candidate => candidate.Lines).Any(line =>
            line.ProductId == item.ExistingProductId.Value
            && string.Equals(
                NormalizeSummaryText(line.CatalogCode),
                item.CatalogCode,
                StringComparison.Ordinal));

    public Task<Result<EdoImportCandidateDetailDto>> UpdateCandidateMappingAsync(
        long jobId,
        long candidateId,
        EdoImportCandidateMappingRequestDto request,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateCandidateMappingAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportCandidateDetailDto>(OrganizationRequired());

            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportCandidateDetailDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportCandidateDetailDto>(JobNotMappable());

            var candidate = await _store.GetCandidateAsync(organizationId.Value, candidateId, ct);
            if (candidate is null || candidate.JobId != jobId)
                return Result.Failure<EdoImportCandidateDetailDto>(CandidateNotFound());
            if (!IsCandidateMappable(candidate.Status))
                return Result.Failure<EdoImportCandidateDetailDto>(CandidateNotMappable());
            if (!HasExactLineCoverage(candidate, request.Lines))
                return Result.Failure<EdoImportCandidateDetailDto>(InvalidLineCoverage());
            if (string.IsNullOrWhiteSpace(candidate.SellerTin) || !candidate.DocumentDate.HasValue)
                return Result.Failure<EdoImportCandidateDetailDto>(CandidateSourceInvalid());

            var selection = new EdoImportMappingSelectionDto
            {
                ProviderCode = candidate.ProviderCode,
                ProviderContractNumber = candidate.ProviderContractNumber,
                ProviderContractDate = candidate.ProviderContractDate,
                CounterpartyId = request.CounterpartyId,
                ContractId = request.ContractId,
                CurrencyId = request.CurrencyId,
                WarehouseId = request.WarehouseId,
                Lines = request.Lines.ToDictionary(line => line.LineNumber, line =>
                    new EdoImportLineMappingSelectionDto
                    {
                        ProductId = line.ProductId,
                        UnitId = line.UnitId,
                        VatRateId = line.VatRateId,
                        DebitAccountId = line.DebitAccountId,
                        VatAccountId = line.VatAccountId
                    })
            };
            var mapping = await _store.ResolveSelectedMappingAsync(
                organizationId.Value,
                candidate.SellerTin,
                candidate.DocumentDate.Value,
                ToHistoricalLines(candidate),
                selection,
                ct);
            ApplyMapping(candidate, mapping, preserveAccounts: false, UtcNow());
            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);

            _auditLog.SetNewValues(new
            {
                candidate.Id,
                candidate.JobId,
                candidate.Status,
                candidate.MappingStatus,
                candidate.SafeErrorCode
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportCandidate,
                candidate.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);

            return Result.Success(candidate.ToDetailDto());
        }, ct);

    public Task<Result<EdoImportJobDto>> ResolveMappingsAsync(
        long jobId,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ResolveMappingsAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportJobDto>(OrganizationRequired());

            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportJobDto>(NotFound(jobId));
            if (!IsMappingAllowed(job.Status))
                return Result.Failure<EdoImportJobDto>(JobNotMappable());

            var candidates = await _store.GetMappingRequiredCandidatesAsync(
                organizationId.Value,
                jobId,
                ct);
            var now = UtcNow();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.SellerTin) || !candidate.DocumentDate.HasValue)
                {
                    candidate.SafeErrorCode = "CANDIDATE_SOURCE_MAPPING_REQUIRED";
                    continue;
                }

                var mapping = await _store.ResolveSelectedMappingAsync(
                    organizationId.Value,
                    candidate.SellerTin,
                    candidate.DocumentDate.Value,
                    ToHistoricalLines(candidate),
                    ToPersistedMappingSelection(candidate),
                    ct);
                ApplyMapping(candidate, mapping, preserveAccounts: false, now);
            }

            await _store.SaveChangesAsync(ct);
            await RefreshJobMappingCountsAsync(job, ct);

            _auditLog.SetNewValues(new
            {
                job.Id,
                ResolvedCandidateCount = candidates.Count,
                job.ReadyCount,
                job.MappingRequiredCount,
                job.DuplicateCount
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);

            return Result.Success(job.ToDto());
        }, ct);

    public async Task<Result<EdoImportJobDto>> CancelAsync(long jobId, CancellationToken ct = default)
    {
        var result = await ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var organizationId = RequireOrganization();
            if (!organizationId.HasValue)
                return Result.Failure<EdoImportJobDto>(OrganizationRequired());

            var job = await _store.GetJobAsync(organizationId.Value, jobId, ct);
            if (job is null)
                return Result.Failure<EdoImportJobDto>(NotFound(jobId));
            if (!EdoImportJobStatus.IsActive(job.Status))
                return Result.Failure<EdoImportJobDto>(Error.Conflict(
                    "EdoImport.JobNotActive",
                    "Only an active EDO import job can be cancelled."));
            var previousStatus = job.Status;
            if (job.Status != EdoImportJobStatus.CancelRequested)
                job.TransitionTo(EdoImportJobStatus.CancelRequested, UtcNow());
            await _store.SaveChangesAsync(ct);

            _auditLog.SetOldValues(new { Status = previousStatus });
            _auditLog.SetNewValues(new { job.Status });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportJob,
                job.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId.Value);
            return Result.Success(job.ToDto());
        }, ct);

        if (result.IsSuccess)
            await _scheduler.ScheduleAsync(result.Value.Id, ct);

        return result;
    }

    private int? RequireOrganization() => _userContext.OrganizationId;
    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private static EdoImportMappingSummaryDto BuildMappingSummary(
        long jobId,
        IReadOnlyCollection<EdoImportMappingSummaryCandidateSourceDto> candidates)
    {
        var mappingRequired = candidates
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .ToArray();
        var mappingLines = mappingRequired
            .SelectMany(candidate => candidate.Lines.Select(line => new
            {
                Candidate = candidate,
                Line = line
            }))
            .ToArray();

        var issueCounts = new EdoImportMappingIssueCountsDto
        {
            Counterparty = mappingRequired.Count(candidate => !candidate.CounterpartyId.HasValue),
            Contract = mappingRequired.Count(candidate => candidate.CounterpartyId.HasValue
                && !candidate.ContractId.HasValue),
            Product = mappingRequired.Count(candidate => candidate.Lines.Any(line => !line.ProductId.HasValue)),
            Currency = mappingRequired.Count(candidate => !candidate.CurrencyId.HasValue),
            Warehouse = mappingRequired.Count(candidate => !candidate.WarehouseId.HasValue),
            Unit = mappingRequired.Count(candidate => candidate.Lines.Any(line =>
                line.ProductId.HasValue && !line.UnitId.HasValue)),
            VatRate = mappingRequired.Count(candidate => candidate.Lines.Any(line =>
                line.ProductId.HasValue && !line.VatRateId.HasValue)),
            Marking = mappingRequired.Count(candidate => IsMarkingIssue(candidate))
        };

        return new EdoImportMappingSummaryDto
        {
            JobId = jobId,
            TotalCandidates = candidates.Count,
            ReadyCount = candidates.Count(candidate => candidate.Status == EdoImportCandidateStatus.Ready),
            DuplicateCount = candidates.Count(candidate => candidate.Status == EdoImportCandidateStatus.Duplicate),
            MappingRequiredCount = mappingRequired.Length,
            SafeErrorCodeCounts = mappingRequired
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate.SafeErrorCode))
                .GroupBy(candidate => candidate.SafeErrorCode!, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
            MissingSellers = mappingRequired
                .Where(candidate => !candidate.CounterpartyId.HasValue)
                .GroupBy(candidate => new
                {
                    SellerTin = NormalizeSummaryText(candidate.SellerTin),
                    SellerName = NormalizeSummaryText(candidate.SellerName)
                })
                .Select(group => new EdoImportMissingSellerSummaryDto
                {
                    SellerTin = group.Key.SellerTin,
                    SellerName = group.Key.SellerName,
                    CandidateCount = group.Count()
                })
                .OrderByDescending(item => item.CandidateCount)
                .ThenBy(item => item.SellerTin, StringComparer.Ordinal)
                .ToArray(),
            MissingContracts = mappingRequired
                .Where(candidate => candidate.CounterpartyId.HasValue
                    && !candidate.ContractId.HasValue)
                .GroupBy(candidate => new
                {
                    SellerTin = NormalizeSummaryText(candidate.SellerTin),
                    CounterpartyId = candidate.CounterpartyId!.Value,
                    ContractNumber = NormalizeSummaryText(candidate.ProviderContractNumber),
                    candidate.ProviderContractDate
                })
                .Select(group => new EdoImportMissingContractSummaryDto
                {
                    SellerTin = group.Key.SellerTin,
                    CounterpartyId = group.Key.CounterpartyId,
                    ProviderContractNumber = group.Key.ContractNumber,
                    ProviderContractDate = group.Key.ProviderContractDate,
                    CandidateCount = group.Count()
                })
                .OrderByDescending(item => item.CandidateCount)
                .ThenBy(item => item.SellerTin, StringComparer.Ordinal)
                .ThenBy(item => item.ProviderContractNumber, StringComparer.Ordinal)
                .ToArray(),
            MissingProducts = mappingLines
                .Where(item => !item.Line.ProductId.HasValue)
                .GroupBy(item => new
                {
                    CatalogCode = NormalizeSummaryText(item.Line.CatalogCode),
                    ProductName = NormalizeSummaryText(item.Line.ProviderProductName),
                    item.Line.IsService,
                    PackageCode = NormalizeSummaryText(item.Line.PackageCode),
                    PackageName = NormalizeSummaryText(item.Line.PackageName),
                    item.Line.VatRate
                })
                .Select(group => new EdoImportMissingProductSummaryDto
                {
                    CatalogCode = group.Key.CatalogCode,
                    ProviderProductName = group.Key.ProductName,
                    ItemType = group.Key.IsService switch
                    {
                        true => "SERVICE",
                        false => "GOODS",
                        _ => "UNKNOWN"
                    },
                    IsService = group.Key.IsService,
                    PackageCode = group.Key.PackageCode,
                    PackageName = group.Key.PackageName,
                    VatRate = group.Key.VatRate,
                    CandidateCount = group.Select(item => item.Candidate.CandidateId).Distinct().Count()
                })
                .OrderByDescending(item => item.CandidateCount)
                .ThenBy(item => item.CatalogCode, StringComparer.Ordinal)
                .ToArray(),
            IssueCounts = issueCounts,
            MissingMasterData = BuildMissingMasterData(issueCounts)
        };
    }

    private static IReadOnlyCollection<EdoImportMissingMasterDataDto> BuildMissingMasterData(
        EdoImportMappingIssueCountsDto counts)
    {
        var values = new (string Type, int Count)[]
        {
            ("COUNTERPARTY", counts.Counterparty),
            ("CONTRACT", counts.Contract),
            ("PRODUCT", counts.Product),
            ("CURRENCY", counts.Currency),
            ("WAREHOUSE", counts.Warehouse),
            ("UNIT", counts.Unit),
            ("VAT_RATE", counts.VatRate),
            ("MARKING_VALIDATION", counts.Marking)
        };
        return values.Where(value => value.Count > 0)
            .Select(value => new EdoImportMissingMasterDataDto
            {
                Type = value.Type,
                CandidateCount = value.Count
            })
            .ToArray();
    }

    private static EdoImportMasterDataPlanDto BuildMasterDataPlan(
        long jobId,
        EdoImportMasterDataPlanSourceDto source)
    {
        var candidates = source.Candidates
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .ToArray();
        var counterpartiesByTin = source.Counterparties
            .GroupBy(counterparty => counterparty.Tin, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var counterpartyItems = candidates
            .GroupBy(candidate => NormalizeSummaryText(candidate.SellerTin), StringComparer.Ordinal)
            .Select(group =>
            {
                var names = SelectCanonicalAndAliases(group.Select(candidate => candidate.SellerName));
                var existing = group.Key is not null
                    && counterpartiesByTin.TryGetValue(group.Key, out var matches)
                        ? matches
                        : [];
                var canCreate = IsDigitsOnly(group.Key)
                    && names.Canonical is { Length: <= 250 };
                return new EdoImportCounterpartyPlanItemDto
                {
                    SellerTin = group.Key,
                    CanonicalSellerName = names.Canonical,
                    NameAliases = names.Aliases,
                    CandidateCount = group.Select(candidate => candidate.CandidateId).Distinct().Count(),
                    ExistingCounterpartyId = existing.Length == 1 ? existing[0].Id : null,
                    Action = existing.Length switch
                    {
                        1 => "USE_EXISTING",
                        0 when canCreate => "CREATE",
                        _ => "CONFLICT"
                    }
                };
            })
            .OrderByDescending(item => item.CandidateCount)
            .ThenBy(item => item.SellerTin, StringComparer.Ordinal)
            .ToArray();

        var contractItems = candidates
            .GroupBy(candidate => new
            {
                candidate.ProviderCode,
                SellerTin = NormalizeSummaryText(candidate.SellerTin),
                ContractNumber = NormalizeSummaryText(candidate.ProviderContractNumber),
                candidate.ProviderContractDate
            })
            .Select(group =>
            {
                var candidateIds = group.Select(candidate => candidate.CandidateId)
                    .Distinct()
                    .Order()
                    .ToArray();
                var candidateDocumentDates = group.Where(candidate => candidate.DocumentDate.HasValue)
                    .Select(candidate => candidate.DocumentDate!.Value)
                    .Distinct()
                    .ToArray();
                var hasProviderIdentity = group.Key.ContractNumber is not null
                    && group.Key.ProviderContractDate.HasValue;
                var selectedCounterpartyIds = group
                    .Where(candidate => candidate.CounterpartyId.HasValue)
                    .Select(candidate => candidate.CounterpartyId!.Value)
                    .Distinct()
                    .ToArray();
                var selectedContractIds = group
                    .Where(candidate => candidate.ContractId.HasValue)
                    .Select(candidate => candidate.ContractId!.Value)
                    .Distinct()
                    .ToArray();
                var localCounterparties = group.Key.SellerTin is not null
                    && counterpartiesByTin.TryGetValue(group.Key.SellerTin, out var matches)
                        ? matches
                        : [];
                var counterpartyId = selectedCounterpartyIds.Length == 1
                    ? selectedCounterpartyIds[0]
                    : localCounterparties.Length == 1
                        ? localCounterparties[0].Id
                        : (int?)null;
                var contracts = counterpartyId.HasValue && hasProviderIdentity
                    ? source.Contracts.Where(contract =>
                            contract.CounterpartyId == counterpartyId.Value
                            && string.Equals(contract.ProviderCode, group.Key.ProviderCode, StringComparison.Ordinal)
                            && string.Equals(contract.ProviderContractNumber, group.Key.ContractNumber, StringComparison.Ordinal)
                            && contract.ProviderContractDate == group.Key.ProviderContractDate.GetValueOrDefault())
                        .ToArray()
                    : [];
                var selectedContract = selectedContractIds.Length == 1
                    ? source.Contracts.SingleOrDefault(contract =>
                        contract.Id == selectedContractIds[0]
                        && (!counterpartyId.HasValue || contract.CounterpartyId == counterpartyId.Value)
                        && (!hasProviderIdentity
                            || contract.ProviderCode is null
                            || string.Equals(contract.ProviderCode, group.Key.ProviderCode, StringComparison.Ordinal)
                                && string.Equals(contract.ProviderContractNumber, group.Key.ContractNumber, StringComparison.Ordinal)
                                && contract.ProviderContractDate == group.Key.ProviderContractDate)
                        && IsApplicableContractForDates(contract, candidateDocumentDates))
                    : null;
                var exactContract = selectedContract ?? (contracts.Length == 1 ? contracts[0] : null);
                var reconciliationContracts = counterpartyId.HasValue
                    ? source.Contracts.Where(contract =>
                            contract.CounterpartyId == counterpartyId.Value
                            && IsApplicableContractForDates(contract, candidateDocumentDates)
                            && (!hasProviderIdentity
                                || contract.Date == group.Key.ProviderContractDate.GetValueOrDefault()
                                    && contract.ProviderCode is null
                                    && contract.ProviderContractNumber is null
                                    && !contract.ProviderContractDate.HasValue))
                        .Select(contract => contract.Id)
                        .Distinct()
                        .Order()
                        .ToArray()
                    : [];
                var canCreate = counterpartyId.HasValue && hasProviderIdentity
                    && group.Key.ContractNumber is { Length: <= 100 };
                return new EdoImportContractPlanItemDto
                {
                    ProviderCode = group.Key.ProviderCode,
                    SellerTin = group.Key.SellerTin,
                    CounterpartyId = counterpartyId,
                    ProviderContractNumber = group.Key.ContractNumber,
                    ProviderContractDate = group.Key.ProviderContractDate,
                    CandidateCount = candidateIds.Length,
                    CandidateIds = candidateIds,
                    ExistingContractId = exactContract?.Id,
                    ReconciliationContractIds = exactContract is null ? reconciliationContracts : [],
                    Action = exactContract is not null
                        ? "USE_EXISTING"
                        : !hasProviderIdentity || contracts.Length > 1 || reconciliationContracts.Length > 0
                            ? "REQUIRES_SELECTION"
                        : canCreate
                            ? "CREATE"
                            : "REQUIRES_SELECTION"
                };
            })
            .OrderByDescending(item => item.CandidateCount)
            .ThenBy(item => item.SellerTin, StringComparer.Ordinal)
            .ThenBy(item => item.ProviderContractNumber, StringComparer.Ordinal)
            .ToArray();

        var productItems = candidates
            .SelectMany(candidate => candidate.Lines.Select(line => new ProductPlanLine(
                candidate.CandidateId,
                line.CatalogCode,
                line.ProviderProductName,
                line.IsService,
                line.PackageCode,
                line.PackageName,
                line.UnitId,
                line.VatRateId,
                IsMarkingIssue(candidate) || line.HasProviderMarkings)))
            .GroupBy(item => NormalizeSummaryText(item.CatalogCode), StringComparer.Ordinal)
            .Select(group => BuildProductPlanItem(group.Key, group.ToArray(), source.Products))
            .OrderByDescending(item => item.CandidateCount)
            .ThenBy(item => item.CatalogCode, StringComparer.Ordinal)
            .ToArray();

        var actions = counterpartyItems.Select(item => item.Action)
            .Concat(contractItems.Select(item => item.Action))
            .Concat(productItems.Select(item => item.Action))
            .ToArray();
        var markingRequiredCount = candidates.Count(candidate => IsMarkingIssue(candidate));
        var blockedReasons = productItems.SelectMany(item => item.BlockedReasonCodes)
            .Concat(contractItems.Where(item => item.Action == "REQUIRES_SELECTION")
                .Select(_ => "CONTRACT_REQUIRES_SELECTION"))
            .Concat(counterpartyItems.Where(item => item.Action == "CONFLICT")
                .Select(_ => "COUNTERPARTY_CONFLICT"))
            .Concat(markingRequiredCount > 0 ? ["MARKING_VALIDATION_REQUIRED"] : [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new EdoImportMasterDataPlanDto
        {
            JobId = jobId,
            Counterparties = counterpartyItems,
            Contracts = contractItems,
            Products = productItems,
            MarkingRequiredCount = markingRequiredCount,
            BlockedReasonCodes = blockedReasons,
            CreateCount = actions.Count(action => action == "CREATE"),
            UseExistingCount = actions.Count(action => action == "USE_EXISTING"),
            ConflictCount = actions.Count(action => action == "CONFLICT"),
            BlockedCount = actions.Count(action => action is "BLOCKED" or "REQUIRES_SELECTION")
        };
    }

    private static EdoImportProductPlanItemDto BuildProductPlanItem(
        string? catalogCode,
        IReadOnlyCollection<ProductPlanLine> items,
        IReadOnlyCollection<EdoImportExistingProductSourceDto> existingProducts)
    {
        var names = items.Select(item => NormalizeSummaryText(item.ProviderProductName))
            .Where(name => name is not null).Select(name => name!)
            .Distinct(StringComparer.Ordinal).ToArray();
        var serviceValues = items.Where(item => item.IsService.HasValue)
            .Select(item => item.IsService!.Value).Distinct().ToArray();
        var unitIds = items.Where(item => item.UnitId.HasValue)
            .Select(item => item.UnitId!.Value).Distinct().ToArray();
        var vatRateIds = items.Where(item => item.VatRateId.HasValue)
            .Select(item => item.VatRateId!.Value).Distinct().ToArray();
        var localProducts = catalogCode is null
            ? []
            : existingProducts.Where(product =>
                string.Equals(product.CatalogCode, catalogCode, StringComparison.Ordinal)).ToArray();
        var localProduct = localProducts.Length == 1 ? localProducts[0] : null;
        var resolvedService = serviceValues.Length == 1
            ? serviceValues[0]
            : localProduct?.IsService;
        var resolvedUnitId = unitIds.Length == 1 ? unitIds[0] : localProduct?.UnitId;
        var resolvedVatRateId = vatRateIds.Length == 1 ? vatRateIds[0] : localProduct?.VatRateId;
        var packageCodes = SelectCanonicalAndAliases(items.Select(item => item.PackageCode));
        var packageNames = SelectCanonicalAndAliases(items.Select(item => item.PackageName));
        var reasons = new List<string>();

        var hasConflict = false;
        AddConflict(names.Length > 1, "PRODUCT_NAME_CONFLICT");
        AddConflict(serviceValues.Length > 1, "PRODUCT_ITEM_TYPE_CONFLICT");
        AddConflict(unitIds.Length > 1, "PRODUCT_UNIT_CONFLICT");
        AddConflict(vatRateIds.Length > 1, "PRODUCT_VAT_RATE_CONFLICT");
        AddConflict(localProducts.Length > 1, "LOCAL_PRODUCT_CONFLICT");
        if (localProduct is not null)
        {
            AddConflict(serviceValues.Length == 1 && serviceValues[0] != localProduct.IsService,
                "LOCAL_PRODUCT_ITEM_TYPE_CONFLICT");
            AddConflict(unitIds.Length == 1 && unitIds[0] != localProduct.UnitId,
                "LOCAL_PRODUCT_UNIT_CONFLICT");
            AddConflict(vatRateIds.Length == 1 && localProduct.VatRateId.HasValue
                && vatRateIds[0] != localProduct.VatRateId.Value,
                "LOCAL_PRODUCT_VAT_RATE_CONFLICT");
        }

        if (localProduct is null && !hasConflict)
        {
            AddBlocked(!IsValidCatalogCode(catalogCode), "CATALOG_CODE_INVALID");
            AddBlocked(names.Length == 0, "PRODUCT_NAME_REQUIRED");
            AddBlocked(names.Length == 1 && names[0].Length > 250, "PRODUCT_NAME_TOO_LONG_FOR_MASTER_DATA");
            AddBlocked(!resolvedService.HasValue, "PRODUCT_ITEM_TYPE_REQUIRED");
            AddBlocked(!resolvedUnitId.HasValue, "PRODUCT_UNIT_REQUIRED");
            AddBlocked(!resolvedVatRateId.HasValue, "PRODUCT_VAT_RATE_REQUIRED");
        }

        var action = hasConflict
            ? "CONFLICT"
            : reasons.Count > 0
                ? "BLOCKED"
                : localProduct is not null
                    ? "USE_EXISTING"
                    : "CREATE";
        return new EdoImportProductPlanItemDto
        {
            CatalogCode = catalogCode,
            ProviderProductName = names.Length == 1 ? names[0] : null,
            IsService = resolvedService,
            PackageCode = packageCodes.Canonical,
            PackageName = packageNames.Canonical,
            ResolvedUnitId = resolvedUnitId,
            ResolvedVatRateId = resolvedVatRateId,
            MarkingRequired = items.Any(item => item.MarkingRequired),
            CandidateCount = items.Select(item => item.CandidateId).Distinct().Count(),
            ExistingProductId = localProduct?.Id,
            Action = action,
            BlockedReasonCodes = reasons.Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal).ToArray()
        };

        void AddConflict(bool condition, string code)
        {
            if (!condition)
                return;
            hasConflict = true;
            reasons.Add(code);
        }

        void AddBlocked(bool condition, string code)
        {
            if (condition)
                reasons.Add(code);
        }
    }

    private static IReadOnlyCollection<EdoImportProductPlanItemDto> BuildProductReplayPlanItems(
        EdoImportMasterDataPlanSourceDto source) =>
        source.Candidates
            .SelectMany(candidate => candidate.Lines.Select(line => new ProductPlanLine(
                candidate.CandidateId,
                line.CatalogCode,
                line.ProviderProductName,
                line.IsService,
                line.PackageCode,
                line.PackageName,
                line.UnitId,
                line.VatRateId,
                line.HasProviderMarkings)))
            .GroupBy(item => NormalizeSummaryText(item.CatalogCode), StringComparer.Ordinal)
            .Select(group => BuildProductPlanItem(group.Key, group.ToArray(), source.Products))
            .ToArray();

    private static (string? Canonical, IReadOnlyCollection<string> Aliases) SelectCanonicalAndAliases(
        IEnumerable<string?> values)
    {
        var ranked = values.Select(NormalizeSummaryText)
            .Where(value => value is not null)
            .Select(value => value!)
            .GroupBy(value => value, StringComparer.Ordinal)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Value, StringComparer.Ordinal)
            .ToArray();
        return ranked.Length == 0
            ? (null, [])
            : (ranked[0].Value, ranked.Skip(1).Select(item => item.Value).ToArray());
    }

    private static EdoImportMasterDataPlanDto BuildMasterDataPlanWithHash(
        long jobId,
        EdoImportMasterDataPlanSourceDto source)
    {
        var plan = BuildMasterDataPlan(jobId, source);
        plan.PlanHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(plan))).ToLowerInvariant();
        return plan;
    }

    private static EdoImportProductConflictPlanDto BuildProductConflictPlan(
        long jobId,
        EdoImportProductConflictSourceDto source)
    {
        var lines = source.Candidates
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .SelectMany(candidate => candidate.Lines
                .Where(line => !line.ProductId.HasValue)
                .Select(line => new ProductConflictLine(
                    candidate.CandidateId,
                    candidate.ProviderCode,
                    candidate.DocumentDate,
                    line)))
            .ToArray();
        var items = lines.GroupBy(item => new ProductConflictIdentity(
                EdoProviderProductIdentity.Normalize(item.ProviderCode) ?? string.Empty,
                EdoProviderProductIdentity.Normalize(item.Line.CatalogCode),
                EdoProviderProductIdentity.Normalize(item.Line.PackageCode) ?? string.Empty,
                EdoProviderProductIdentity.Normalize(item.Line.ProviderProductName),
                item.Line.IsService))
            .Select(group =>
            {
                var key = group.Key;
                var identityHash = EdoProviderProductIdentity.Create(
                    key.ProviderCode, key.CatalogCode, key.PackageCode,
                    key.ProviderProductName, key.IsService);
                var vatValues = group.Where(item => item.Line.VatRate.HasValue)
                    .Select(item => item.Line.VatRate!.Value).Distinct().ToArray();
                var dates = group.Where(item => item.DocumentDate.HasValue)
                    .Select(item => item.DocumentDate!.Value).Distinct().ToArray();
                var matchingVatRates = vatValues.Length == 1
                    ? source.VatRates.Where(vat => vat.Rate == vatValues[0]
                        && dates.All(date => (!vat.EffectiveFrom.HasValue || vat.EffectiveFrom <= date)
                            && (!vat.EffectiveTo.HasValue || vat.EffectiveTo >= date))).ToArray()
                    : [];
                var resolvedVatRateId = matchingVatRates.Length == 1
                    ? matchingVatRates[0].Id
                    : (short?)null;
                var markingRequired = group.Any(item => item.Line.HasProviderMarkings);
                var markedCandidateCount = group.Where(item => item.Line.HasProviderMarkings)
                    .Select(item => item.CandidateId).Distinct().Count();
                var reasons = new List<string>();
                if (identityHash is null)
                    reasons.Add("PROVIDER_PRODUCT_IDENTITY_INCOMPLETE");
                if (!IsValidCatalogCode(key.CatalogCode))
                    reasons.Add("CATALOG_CODE_INVALID");
                if (key.ProviderProductName is not { Length: > 0 and <= 500 })
                    reasons.Add("PROVIDER_PRODUCT_NAME_REQUIRED");
                if (!key.IsService.HasValue)
                    reasons.Add("PRODUCT_ITEM_TYPE_REQUIRED");
                if (!resolvedVatRateId.HasValue)
                    reasons.Add(vatValues.Length > 1
                        ? "PROVIDER_VAT_RATE_CONFLICT"
                        : "PROVIDER_VAT_RATE_REQUIRED");
                var compatibleProducts = identityHash is null || !key.IsService.HasValue
                    ? []
                    : source.Products.Where(product =>
                            string.Equals(product.CatalogCode, key.CatalogCode, StringComparison.Ordinal))
                        .OrderBy(product => product.Id)
                        .Select(product => new EdoImportCompatibleProductDto
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            IsService = product.IsService,
                            IsPieceTracked = product.IsPieceTracked,
                            UnitId = product.UnitId,
                            VatRateId = product.VatRateId,
                            CompatibilityCodes = ProductCompatibilityCodes(
                                product, key.IsService.Value, resolvedVatRateId)
                        }).ToArray();
                return new EdoImportProductConflictItemDto
                {
                    IdentityKey = identityHash,
                    ProviderCode = key.ProviderCode,
                    CatalogCode = key.CatalogCode,
                    PackageCode = key.PackageCode,
                    PackageName = SelectCanonicalAndAliases(group.Select(item => item.Line.PackageName)).Canonical,
                    ProviderProductName = key.ProviderProductName,
                    ItemType = key.IsService == true ? "SERVICE" : key.IsService == false ? "GOODS" : "UNKNOWN",
                    IsService = key.IsService,
                    VatRate = vatValues.Length == 1 ? vatValues[0] : null,
                    ResolvedVatRateId = resolvedVatRateId,
                    CandidateCount = group.Select(item => item.CandidateId).Distinct().Count(),
                    MarkingRequired = markingRequired,
                    MarkedCandidateCount = markedCandidateCount,
                    BlockedReasonCodes = reasons.Order(StringComparer.Ordinal).ToArray(),
                    CompatibleProducts = compatibleProducts
                };
            })
            .OrderByDescending(item => item.CandidateCount)
            .ThenBy(item => item.CatalogCode, StringComparer.Ordinal)
            .ThenBy(item => item.PackageCode, StringComparer.Ordinal)
            .ThenBy(item => item.ProviderProductName, StringComparer.Ordinal)
            .ToArray();
        var plan = new EdoImportProductConflictPlanDto { JobId = jobId, Items = items };
        plan.PlanHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(plan))).ToLowerInvariant();
        return plan;
    }

    private static EdoImportMarkingConflictPlanDto BuildMarkingConflictPlan(
        long jobId,
        IReadOnlyCollection<EdoImportMarkingConflictSourceDto> source)
    {
        var items = source
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired
                && EdoImportMarkingPolicy.CanSkipConflict(candidate.SafeErrorCode))
            .OrderBy(candidate => candidate.CandidateId)
            .Select(candidate => new EdoImportMarkingConflictItemDto
            {
                CandidateId = candidate.CandidateId,
                DocumentNumber = NormalizeSummaryText(candidate.DocumentNumber),
                DocumentDate = candidate.DocumentDate,
                SafeErrorCode = candidate.SafeErrorCode!,
                ExpectedQuantity = candidate.ExpectedQuantity,
                ActualMarkingCount = candidate.ActualMarkingCount,
                ConflictCount = candidate.ConflictCount,
                ExistingPurchaseIds = candidate.ExistingPurchaseIds.Distinct().Order().ToArray()
            })
            .ToArray();
        var plan = new EdoImportMarkingConflictPlanDto { JobId = jobId, Items = items };
        plan.ConflictHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(plan))).ToLowerInvariant();
        return plan;
    }

    private async Task<BackgroundOrganizationRecoveryOutcome> RequeueBackgroundOrganizationFailureAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct)
    {
        var result = await ExecuteInTransactionAsync(nameof(RequeueBackgroundOrganizationFailureAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId, jobId, candidateId, ct);
            if (job is null || candidate is null)
                return Result.Success(BackgroundOrganizationRecoveryOutcome.NotRecoverable);
            if (candidate.Status == EdoImportCandidateStatus.Ready)
                return Result.Success(BackgroundOrganizationRecoveryOutcome.Requeued);
            var oldSafeErrorCode = candidate.SafeErrorCode;
            var attemptCount = candidate.AttemptCount;
            var policyOutcome = GetBackgroundOrganizationRecoveryOutcome(
                oldSafeErrorCode, attemptCount);
            if (policyOutcome == BackgroundOrganizationRecoveryOutcome.Exhausted)
            {
                await AuditBackgroundOrganizationRecoveryDecisionAsync(
                    organizationId, candidate.Id, oldSafeErrorCode!, attemptCount, "EXHAUSTED", ct);
                return Result.Success(policyOutcome);
            }
            if (candidate.Status != EdoImportCandidateStatus.Failed
                || policyOutcome != BackgroundOrganizationRecoveryOutcome.Requeued
                || candidate.ImportedPurchaseId.HasValue
                || candidate.ExistingPurchaseId.HasValue
                || string.IsNullOrWhiteSpace(candidate.SellerTin)
                || !candidate.DocumentDate.HasValue
                || !IsDraftImportAllowed(job.Status))
            {
                await AuditBackgroundOrganizationRecoveryDecisionAsync(
                    organizationId, candidate.Id, oldSafeErrorCode ?? string.Empty,
                    attemptCount, "NOT_RECOVERABLE", ct);
                return Result.Success(BackgroundOrganizationRecoveryOutcome.NotRecoverable);
            }

            var markings = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                .SelectMany(line => line.Markings.OrderBy(marking => marking.Id))
                .Select(marking => marking.MarkingNumber).ToArray();
            await _store.AcquireDraftImportLocksAsync(
                organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, markings, ct);
            if (await _store.FindExistingPurchaseForProviderDocumentAsync(
                    organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, ct) is not null)
            {
                await AuditBackgroundOrganizationRecoveryDecisionAsync(
                    organizationId, candidate.Id, oldSafeErrorCode!, attemptCount, "NOT_RECOVERABLE", ct);
                return Result.Success(BackgroundOrganizationRecoveryOutcome.NotRecoverable);
            }

            var now = UtcNow();
            var mapping = await _store.ResolveSelectedMappingAsync(
                organizationId,
                candidate.SellerTin,
                candidate.DocumentDate.Value,
                ToHistoricalLines(candidate),
                ToPersistedMappingSelection(candidate),
                ct);
            ApplyMapping(candidate, mapping, preserveAccounts: true, now);
            var source = BuildHistoricalDraftSource(candidate);
            if (candidate.Status != EdoImportCandidateStatus.Ready || !source.IsSuccess)
                return Result.Failure<BackgroundOrganizationRecoveryOutcome>(Error.Business(
                    "BACKGROUND_EDO_SCOPE_RECOVERY_MAPPING_INVALID",
                    "The candidate is not valid for background Draft import recovery."));

            job.FailedCount = Math.Max(0, job.FailedCount - 1);
            job.ReadyCount++;
            job.BulkFailedCount = Math.Max(0, job.BulkFailedCount - 1);
            ReconcileBulkCounters(job);
            await _store.SaveChangesAsync(ct);
            await AuditBackgroundOrganizationRecoveryDecisionAsync(
                organizationId, candidate.Id, oldSafeErrorCode!, attemptCount, "REQUEUED", ct);
            return Result.Success(BackgroundOrganizationRecoveryOutcome.Requeued);
        }, ct);
        return result.IsSuccess
            ? result.Value
            : BackgroundOrganizationRecoveryOutcome.NotRecoverable;
    }

    private async Task AuditBackgroundOrganizationRecoveryDecisionAsync(
        int organizationId,
        long candidateId,
        string oldSafeErrorCode,
        int attemptCount,
        string decision,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "EDO background organization recovery decision; CandidateId={CandidateId}; SafeErrorCode={SafeErrorCode}; AttemptCount={AttemptCount}; Decision={Decision}",
            candidateId, oldSafeErrorCode, attemptCount, decision);
        _auditLog.SetNewValues(new
        {
            CandidateId = candidateId,
            OldSafeErrorCode = oldSafeErrorCode,
            AttemptCount = attemptCount,
            RecoveryDecision = decision
        });
        await _auditLog.CreateAsync(
            AuditLogTableConst.EdoImportCandidate,
            candidateId.ToString(),
            AuditLogOperationTypeConst.Update,
            $"BACKGROUND_EDO_SCOPE_RECOVERY_{decision}",
            organizationId);
    }

    private async Task<BulkFailureOutcome> ResolveBulkFailureAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct)
    {
        var result = await ExecuteInTransactionAsync(nameof(ResolveBulkFailureAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId, jobId, candidateId, ct);
            if (job is null || candidate is null || candidate.Status != EdoImportCandidateStatus.Failed)
                return Result.Success(BulkFailureOutcome.None);
            var action = candidate.SafeErrorCode == EdoImportDraftFailurePolicy.LineValuesInvalid
                && job.BulkLineValuesInvalidPolicy == "SKIP"
                    ? "SKIP"
                    : EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(candidate.SafeErrorCode)
                        && job.BulkMarkingAlreadyUsedPolicy
                            == "MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP"
                        ? "MARKING"
                        : null;
            if (action is null || candidate.ImportedPurchaseId.HasValue || candidate.ExistingPurchaseId.HasValue)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
                job.BulkLastSafeErrorCode = candidate.SafeErrorCode;
                await _store.SaveChangesAsync(ct);
                return Result.Success(BulkFailureOutcome.Paused);
            }
            var markings = candidate.Lines.SelectMany(line => line.Markings)
                .Select(marking => marking.MarkingNumber).ToArray();
            await _store.AcquireDraftImportLocksAsync(
                organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, markings, ct);
            if (await _store.FindExistingPurchaseForProviderDocumentAsync(
                    organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, ct) is not null)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
                job.BulkLastSafeErrorCode = candidate.SafeErrorCode;
                await _store.SaveChangesAsync(ct);
                return Result.Success(BulkFailureOutcome.Paused);
            }

            var now = UtcNow();
            if (action == "MARKING")
            {
                var usage = await _store.GetDraftMarkingUsageAsync(organizationId, markings, ct);
                if (usage.ExistingPurchaseIdForAllMarkings.HasValue)
                {
                    candidate.DuplicateState = EdoImportDuplicateState.Confirmed;
                    candidate.ExistingPurchaseId = usage.ExistingPurchaseIdForAllMarkings;
                    candidate.SafeErrorCode = null;
                    candidate.TransitionTo(EdoImportCandidateStatus.Duplicate, now);
                    job.FailedCount = Math.Max(0, job.FailedCount - 1);
                    job.DuplicateCount++;
                    job.BulkDuplicateCount++;
                    ReconcileBulkCounters(job);
                    await _store.SaveChangesAsync(ct);
                    return Result.Success(BulkFailureOutcome.Resolved);
                }
                if (!usage.HasPartialOrMultiplePurchaseConflict)
                {
                    job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
                    job.BulkLastSafeErrorCode = EdoImportDraftFailurePolicy.MarkingAlreadyUsed;
                    await _store.SaveChangesAsync(ct);
                    return Result.Success(BulkFailureOutcome.Paused);
                }
            }

            candidate.SafeErrorCode = EdoImportDraftFailurePolicy.ToSkipped(candidate.SafeErrorCode!);
            candidate.TransitionTo(EdoImportCandidateStatus.Skipped, now);
            job.FailedCount = Math.Max(0, job.FailedCount - 1);
            job.SkippedCount++;
            job.BulkSkippedCount++;
            ReconcileBulkCounters(job);
            await _store.SaveChangesAsync(ct);
            return Result.Success(BulkFailureOutcome.Resolved);
        }, ct);
        return result.IsSuccess ? result.Value : BulkFailureOutcome.Paused;
    }

    private Task SetBulkRunningAsync(int organizationId, long jobId, CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(SetBulkRunningAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            if (job is not null && job.BulkImportStatus == EdoImportBulkImportStatus.Queued)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Running;
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success();
        }, ct);

    private Task PauseBulkImportAsync(
        int organizationId,
        long jobId,
        string safeCode,
        CancellationToken ct,
        bool countFailure = true) =>
        ExecuteInTransactionAsync(nameof(PauseBulkImportAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            if (job is not null)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Paused;
                job.BulkLastSafeErrorCode = safeCode;
                if (countFailure)
                    job.BulkFailedCount++;
                ReconcileBulkCounters(job);
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success();
        }, ct);

    private Task SetBulkCancelledAsync(int organizationId, long jobId, CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(SetBulkCancelledAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            if (job is not null && job.BulkImportStatus == EdoImportBulkImportStatus.CancelRequested)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Cancelled;
                job.BulkCompletedAt = UtcNow();
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success();
        }, ct);

    private Task CompleteBulkImportAsync(int organizationId, long jobId, CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(CompleteBulkImportAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            if (job is not null && job.BulkImportStatus == EdoImportBulkImportStatus.Running)
            {
                job.BulkImportStatus = EdoImportBulkImportStatus.Completed;
                job.BulkCompletedAt = UtcNow();
                ReconcileBulkCounters(job);
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success();
        }, ct);

    private Task AddBulkProgressAsync(int organizationId, long jobId, int created, int reused,
        int duplicates, int skipped, int failed, string? lastSafeCode, CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(AddBulkProgressAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            if (job is not null)
            {
                job.BulkProcessedCount += created + reused + duplicates + skipped + failed;
                job.BulkCreatedDraftCount += created;
                job.BulkReusedDraftCount += reused;
                job.BulkDuplicateCount += duplicates;
                job.BulkSkippedCount += skipped;
                job.BulkFailedCount += failed;
                job.BulkLastSafeErrorCode = lastSafeCode;
                ReconcileBulkCounters(job);
                await _store.SaveChangesAsync(ct);
            }
            return Result.Success();
        }, ct);

    private static EdoImportBulkDraftStatusDto ToBulkImportStatus(EdoImportJob job) => new()
    {
        JobId = job.Id,
        Status = job.BulkImportStatus ?? "NOT_STARTED",
        ProcessedCount = job.BulkProcessedCount,
        CreatedDraftCount = job.BulkCreatedDraftCount,
        ReusedDraftCount = job.BulkReusedDraftCount,
        DuplicateCount = job.BulkDuplicateCount,
        SkippedCount = job.BulkSkippedCount,
        FailedCount = job.BulkFailedCount,
        RemainingReadyCount = job.ReadyCount,
        LastSafeErrorCode = job.BulkLastSafeErrorCode,
        StartedAt = job.BulkStartedAt,
        CompletedAt = job.BulkCompletedAt
    };

    private enum BulkFailureOutcome { None, Resolved, Paused }

    private enum BackgroundOrganizationRecoveryOutcome { Requeued, Exhausted, NotRecoverable }

    private static bool IsBackgroundOrganizationRecoveryFailure(string? safeErrorCode) => safeErrorCode is
        BackgroundOrganizationRequiredDraftFailure
        or DocumentNumberInvalidOrganizationDraftFailure;

    private static BackgroundOrganizationRecoveryOutcome GetBackgroundOrganizationRecoveryOutcome(
        string? safeErrorCode,
        int attemptCount)
    {
        if (safeErrorCode == BackgroundOrganizationRequiredDraftFailure && attemptCount == 1)
            return BackgroundOrganizationRecoveryOutcome.Requeued;
        if (safeErrorCode == DocumentNumberInvalidOrganizationDraftFailure && attemptCount <= 2)
            return BackgroundOrganizationRecoveryOutcome.Requeued;
        return IsBackgroundOrganizationRecoveryFailure(safeErrorCode)
            ? BackgroundOrganizationRecoveryOutcome.Exhausted
            : BackgroundOrganizationRecoveryOutcome.NotRecoverable;
    }

    private static void ReconcileBulkCounters(EdoImportJob job) =>
        job.BulkProcessedCount = job.BulkCreatedDraftCount
            + job.BulkReusedDraftCount
            + job.BulkDuplicateCount
            + job.BulkSkippedCount
            + job.BulkFailedCount;

    private static EdoImportDraftFailureListDto BuildDraftFailurePlan(
        long jobId,
        IReadOnlyCollection<EdoImportDraftFailureSourceDto> source)
    {
        var plan = new EdoImportDraftFailureListDto
        {
            JobId = jobId,
            Items = source.OrderBy(candidate => candidate.CandidateId)
                .Select(candidate => new EdoImportDraftFailureItemDto
                {
                    CandidateId = candidate.CandidateId,
                    DocumentNumber = NormalizeSummaryText(candidate.DocumentNumber),
                    DocumentDate = candidate.DocumentDate,
                    SafeErrorCode = candidate.SafeErrorCode,
                    TotalMarkingCount = candidate.TotalMarkingCount,
                    UsedMarkingCount = candidate.UsedMarkingCount,
                    ExistingPurchaseIds = candidate.ExistingPurchaseIds.Distinct().Order().ToArray()
                }).ToArray()
        };
        plan.FailureHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(plan))).ToLowerInvariant();
        return plan;
    }

    private static EdoImportDraftFailureApplyResponseDto ToDraftFailureApplyResponse(
        EdoImportJob job,
        int skippedCandidateCount,
        int markedDuplicateCandidateCount) => new()
        {
            JobId = job.Id,
            SkippedCandidateCount = skippedCandidateCount,
            MarkedDuplicateCandidateCount = markedDuplicateCandidateCount,
            ReadyCount = job.ReadyCount,
            MappingRequiredCount = job.MappingRequiredCount,
            DuplicateCount = job.DuplicateCount,
            ImportedCount = job.ImportedCount,
            FailedCount = job.FailedCount,
            SkippedCount = job.SkippedCount
        };

    private static bool IsDraftFailureActionAllowedByCode(
        string? safeErrorCode,
        string action) =>
        action == "SKIP" && (EdoImportDraftFailurePolicy.CanSkip(safeErrorCode)
            || EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(safeErrorCode))
        || action == "MARK_DUPLICATE"
            && EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(safeErrorCode);

    private static EdoImportMarkingConflictApplyResponseDto ToMarkingConflictApplyResponse(
        EdoImportJob job,
        int skippedCandidateCount) => new()
        {
            JobId = job.Id,
            SkippedCandidateCount = skippedCandidateCount,
            ReadyCount = job.ReadyCount,
            MappingRequiredCount = job.MappingRequiredCount,
            DuplicateCount = job.DuplicateCount,
            SkippedCount = job.SkippedCount
        };

    private static EdoImportPieceTrackingPlanDto BuildPieceTrackingPlan(
        long jobId,
        EdoImportMasterDataPlanSourceDto source,
        IReadOnlySet<int>? replayAsNotPieceTracked = null)
    {
        var eligibleCandidates = source.Candidates.Where(candidate => candidate.Status is
            EdoImportCandidateStatus.Ready
            or EdoImportCandidateStatus.MappingRequired
            or EdoImportCandidateStatus.Failed).ToArray();
        var productsById = source.Products.ToDictionary(product => product.Id);
        var items = eligibleCandidates
            .SelectMany(candidate => candidate.Lines
                .Where(line => line.HasProviderMarkings
                    && line.IsService != true
                    && line.ProductId.HasValue)
                .Select(line => new { Candidate = candidate, Line = line }))
            .Where(item => productsById.ContainsKey(item.Line.ProductId!.Value))
            .GroupBy(item => item.Line.ProductId!.Value)
            .Select(group =>
            {
                var product = productsById[group.Key];
                var isPieceTracked = product.IsPieceTracked
                    && replayAsNotPieceTracked?.Contains(product.Id) != true;
                return new EdoImportPieceTrackingPlanItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CatalogCode = product.CatalogCode,
                    AffectedCandidateCount = group.Select(item => item.Candidate.CandidateId)
                        .Distinct().Count(),
                    MarkingCount = group.Sum(item => item.Line.ProviderMarkingCount),
                    IsService = product.IsService,
                    IsPieceTracked = isPieceTracked,
                    SafeAction = product.IsService
                        ? "BLOCKED_SERVICE"
                        : isPieceTracked
                            ? "NO_CHANGE"
                            : "ENABLE_PIECE_TRACKING"
                };
            })
            .OrderBy(item => item.ProductId)
            .ToArray();
        var plan = new EdoImportPieceTrackingPlanDto
        {
            JobId = jobId,
            Products = items
        };
        plan.PlanHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(new
            {
                jobId,
                Products = items.Select(item => new
                {
                    item.ProductId,
                    item.ProductName,
                    item.CatalogCode,
                    item.AffectedCandidateCount,
                    item.MarkingCount,
                    item.IsService,
                    item.IsPieceTracked,
                    item.SafeAction
                })
            }))).ToLowerInvariant();
        return plan;
    }

    private static EdoImportDraftPlanDto BuildImportPlan(
        EdoImportJob job,
        IReadOnlyCollection<EdoImportCandidate> candidates)
    {
        var ordered = candidates.OrderBy(candidate => candidate.Id).ToArray();
        var dates = ordered.Where(candidate => candidate.DocumentDate.HasValue)
            .Select(candidate => candidate.DocumentDate!.Value).ToArray();
        var plan = new EdoImportDraftPlanDto
        {
            JobId = job.Id,
            JobStatus = job.Status,
            TotalCandidates = job.DiscoveredCount,
            ReadyCount = ordered.Length,
            MappingRequiredCount = job.MappingRequiredCount,
            DuplicateCount = job.DuplicateCount,
            ImportedCount = job.ImportedCount,
            FailedCount = job.FailedCount,
            SkippedCount = job.SkippedCount,
            ReadyNetAmount = ordered.Sum(candidate => candidate.NetAmount ?? 0m),
            ReadyVatAmount = ordered.Sum(candidate => candidate.VatAmount ?? 0m),
            ReadyTotalAmount = ordered.Sum(candidate => candidate.TotalAmount ?? 0m),
            MarkedCandidateCount = ordered.Count(candidate =>
                candidate.Lines.Any(line => line.Markings.Count > 0)),
            EarliestDocumentDate = dates.Length == 0 ? null : dates.Min(),
            LatestDocumentDate = dates.Length == 0 ? null : dates.Max(),
            Providers = ordered.GroupBy(candidate => candidate.ProviderCode, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new EdoImportDraftProviderSummaryDto
                {
                    ProviderCode = group.Key,
                    ReadyCount = group.Count()
                }).ToArray()
        };
        var hashSource = ordered.Select(candidate => new
        {
            candidate.Id,
            candidate.ProviderCode,
            candidate.ProviderDocumentId,
            candidate.Direction,
            candidate.NormalizedStatus,
            candidate.DocumentType,
            candidate.DocumentNumber,
            candidate.DocumentDate,
            candidate.DocumentDateTime,
            candidate.SellerTin,
            candidate.ProviderContractNumber,
            candidate.ProviderContractDate,
            candidate.NetAmount,
            candidate.VatAmount,
            candidate.TotalAmount,
            candidate.SelectedCounterpartyId,
            candidate.SelectedContractId,
            candidate.SelectedCurrencyId,
            candidate.SelectedWarehouseId,
            Lines = candidate.Lines.OrderBy(line => line.ProviderLineNumber).Select(line => new
            {
                line.ProviderLineNumber,
                line.CatalogCode,
                line.IsService,
                line.Quantity,
                line.UnitPrice,
                line.NetAmount,
                line.VatRate,
                line.VatAmount,
                line.TotalAmount,
                line.SelectedProductId,
                line.SelectedUnitId,
                line.SelectedVatRateId,
                line.SelectedDebitAccountId,
                line.SelectedVatAccountId,
                Markings = line.Markings.OrderBy(marking => marking.Id)
                    .Select(marking => marking.MarkingNumber).ToArray()
            }).ToArray()
        }).ToArray();
        plan.ImportPlanHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(new { job.Id, Candidates = hashSource })))
            .ToLowerInvariant();
        return plan;
    }

    private Task<Result<DraftImportOutcome>> ImportDraftCandidateAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(ImportDraftCandidateAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId, jobId, candidateId, ct);
            if (job is null || candidate is null)
                return Result.Failure<DraftImportOutcome>(Error.NotFound(
                    "DRAFT_IMPORT_CANDIDATE_NOT_FOUND",
                    "The Draft import candidate was not found in this organization job."));
            if (candidate.Status == EdoImportCandidateStatus.Imported
                && (candidate.ImportedPurchaseId ?? candidate.ExistingPurchaseId) is { } importedId)
                return Result.Success(new DraftImportOutcome(importedId, Created: false));
            if (candidate.Status != EdoImportCandidateStatus.Ready
                || !IsDraftImportAllowed(job.Status))
                return Result.Failure<DraftImportOutcome>(StaleImportPlan());

            var markings = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                .SelectMany(line => line.Markings.OrderBy(marking => marking.Id))
                .Select(marking => marking.MarkingNumber).ToArray();
            await _store.AcquireDraftImportLocksAsync(
                organizationId, candidate.ProviderCode,
                candidate.ProviderDocumentId, markings, ct);
            var existingPurchaseId = await _store.FindExistingPurchaseForProviderDocumentAsync(
                organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, ct);
            var now = UtcNow();
            if (existingPurchaseId.HasValue)
            {
                CompleteDraftCandidateImport(job, candidate, existingPurchaseId.Value, now);
                await _store.SaveChangesAsync(ct);
                return Result.Success(new DraftImportOutcome(existingPurchaseId.Value, Created: false));
            }
            if (await _store.AnyUsedMarkingsAsync(organizationId, markings, ct))
                return Result.Failure<DraftImportOutcome>(Error.Conflict(
                    "DRAFT_IMPORT_MARKING_ALREADY_USED",
                    "A provider marking is already linked to an existing organization inventory item."));
            if (string.IsNullOrWhiteSpace(candidate.SellerTin)
                || !candidate.DocumentDate.HasValue)
                return Result.Failure<DraftImportOutcome>(DraftImportSourceInvalid());

            var verifiedMapping = await _store.ResolveSelectedMappingAsync(
                organizationId,
                candidate.SellerTin,
                candidate.DocumentDate.Value,
                ToHistoricalLines(candidate),
                ToPersistedMappingSelection(candidate),
                ct);
            var markedNonPieceGoods = candidate.Lines.Any(line =>
                line.Markings.Count > 0
                && verifiedMapping.Lines.TryGetValue(line.ProviderLineNumber, out var resolved)
                && !resolved.IsService
                && !resolved.IsPieceTracked);
            if (markedNonPieceGoods)
                return Result.Failure<DraftImportOutcome>(Error.Business(
                    ProductPieceTrackingDraftFailure,
                    "Marked goods require an active piece-tracked local product before Draft import."));

            var source = BuildHistoricalDraftSource(candidate);
            if (!source.IsSuccess)
                return Result.Failure<DraftImportOutcome>(source.Error);
            candidate.AttemptCount++;
            candidate.TransitionTo(EdoImportCandidateStatus.Importing, now);
            var created = await _draftFactory!.CreateFromHistoricalSnapshotAsync(
                source.Value.Document, source.Value.Request, ct);
            if (!created.IsSuccess)
                return Result.Failure<DraftImportOutcome>(created.Error);

            CompleteDraftCandidateImport(job, candidate, created.Value.Id, now);
            await _store.SaveChangesAsync(ct);
            _auditLog.SetNewValues(new
            {
                candidate.Id,
                PurchaseId = created.Value.Id,
                candidate.Status
            });
            await _auditLog.CreateAsync(
                AuditLogTableConst.EdoImportCandidate,
                candidate.Id.ToString(),
                AuditLogOperationTypeConst.Update,
                organizationId: organizationId);
            return Result.Success(new DraftImportOutcome(created.Value.Id, Created: true));
        }, ct);

    private Task<Result> MarkDraftCandidateFailedAsync(
        int organizationId,
        long jobId,
        long candidateId,
        string safeErrorCode,
        CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(MarkDraftCandidateFailedAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId, jobId, candidateId, ct);
            if (job is null || candidate is null)
                return Result.Failure(Error.NotFound(
                    "DRAFT_IMPORT_CANDIDATE_NOT_FOUND",
                    "The Draft import candidate was not found in this organization job."));
            if (candidate.Status is EdoImportCandidateStatus.Imported or EdoImportCandidateStatus.Failed)
                return Result.Success();
            if (candidate.Status != EdoImportCandidateStatus.Ready)
                return Result.Failure(StaleImportPlan());

            var now = UtcNow();
            candidate.AttemptCount++;
            candidate.SafeErrorCode = safeErrorCode;
            candidate.TransitionTo(EdoImportCandidateStatus.Failed, now);
            job.ReadyCount = Math.Max(0, job.ReadyCount - 1);
            job.FailedCount++;
            UpdateJobAfterDraftImport(job, now);
            await _store.SaveChangesAsync(ct);
            return Result.Success();
        }, ct);

    private Task<Result<bool>> TryRecoverDraftCandidateAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct) =>
        ExecuteInTransactionAsync(nameof(TryRecoverDraftCandidateAsync), async () =>
        {
            var job = await _store.GetJobForImportAsync(organizationId, jobId, ct);
            var candidate = await _store.GetCandidateForImportAsync(
                organizationId, jobId, candidateId, ct);
            if (job is null || candidate is null)
                return Result.Success(false);
            if (candidate.Status == EdoImportCandidateStatus.Imported
                && (candidate.ImportedPurchaseId ?? candidate.ExistingPurchaseId).HasValue)
                return Result.Success(true);
            if (candidate.Status != EdoImportCandidateStatus.Ready
                || !IsDraftImportAllowed(job.Status))
                return Result.Success(false);

            var markings = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                .SelectMany(line => line.Markings.OrderBy(marking => marking.Id))
                .Select(marking => marking.MarkingNumber).ToArray();
            await _store.AcquireDraftImportLocksAsync(
                organizationId, candidate.ProviderCode,
                candidate.ProviderDocumentId, markings, ct);
            var purchaseId = await _store.FindExistingPurchaseForProviderDocumentAsync(
                organizationId, candidate.ProviderCode, candidate.ProviderDocumentId, ct);
            if (!purchaseId.HasValue)
                return Result.Success(false);

            CompleteDraftCandidateImport(job, candidate, purchaseId.Value, UtcNow());
            await _store.SaveChangesAsync(ct);
            return Result.Success(true);
        }, ct);

    private static void CompleteDraftCandidateImport(
        EdoImportJob job,
        EdoImportCandidate candidate,
        long purchaseId,
        DateTime now)
    {
        candidate.ImportedPurchaseId = purchaseId;
        candidate.ExistingPurchaseId = purchaseId;
        candidate.SafeErrorCode = null;
        if (candidate.Status == EdoImportCandidateStatus.Ready)
            candidate.TransitionTo(EdoImportCandidateStatus.Importing, now);
        candidate.TransitionTo(EdoImportCandidateStatus.Imported, now);
        job.ReadyCount = Math.Max(0, job.ReadyCount - 1);
        job.ImportedCount++;
        UpdateJobAfterDraftImport(job, now);
    }

    private static void UpdateJobAfterDraftImport(EdoImportJob job, DateTime now)
    {
        if (job.Status != EdoImportJobStatus.Importing)
            job.TransitionTo(EdoImportJobStatus.Importing, now);
        var completed = job.ReadyCount == 0
            && job.MappingRequiredCount == 0
            && job.FailedCount == 0;
        job.TransitionTo(
            completed ? EdoImportJobStatus.Completed : EdoImportJobStatus.Partial,
            now);
    }

    private static Result<HistoricalDraftSource> BuildHistoricalDraftSource(
        EdoImportCandidate candidate)
    {
        if (!Enum.TryParse<EdoProviderCode>(candidate.ProviderCode, true, out var providerCode)
            || providerCode is not EdoProviderCode.EDOCS and not EdoProviderCode.DIDOX
            || candidate.Direction != EdoDirection.INBOX.ToString()
            || candidate.NormalizedStatus != EdoDocumentStatusCode.SIGNED.ToString()
            || !string.Equals(candidate.DocumentType, "FACTURA", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(candidate.ProviderDocumentId)
            || string.IsNullOrWhiteSpace(candidate.SellerTin)
            || !candidate.DocumentDate.HasValue
            || !candidate.SelectedCounterpartyId.HasValue
            || !candidate.SelectedContractId.HasValue
            || !candidate.SelectedCurrencyId.HasValue
            || !candidate.SelectedWarehouseId.HasValue
            || candidate.Lines.Count == 0
            || candidate.Lines.Select(line => line.ProviderLineNumber).Distinct().Count()
                != candidate.Lines.Count)
            return Result.Failure<HistoricalDraftSource>(DraftImportSourceInvalid());

        var orderedLines = candidate.Lines.OrderBy(line => line.ProviderLineNumber).ToArray();
        if (orderedLines.Any(line => line.ProviderLineNumber <= 0
            || !line.SelectedProductId.HasValue
            || !line.SelectedUnitId.HasValue
            || !line.SelectedVatRateId.HasValue
            || !line.Quantity.HasValue
            || !line.UnitPrice.HasValue
            || !line.TotalAmount.HasValue
            || line.Markings.Any(marking => string.IsNullOrWhiteSpace(marking.MarkingNumber)
                || marking.MarkingNumber.Trim().Length > 250)))
            return Result.Failure<HistoricalDraftSource>(DraftImportSourceInvalid());
        var allMarkings = orderedLines.SelectMany(line => line.Markings)
            .Select(marking => marking.MarkingNumber.Trim()).ToArray();
        if (allMarkings.Distinct(StringComparer.OrdinalIgnoreCase).Count() != allMarkings.Length)
            return Result.Failure<HistoricalDraftSource>(DraftImportSourceInvalid());

        var document = new EdoDocumentDto
        {
            ProviderCode = providerCode,
            ProviderDocumentId = candidate.ProviderDocumentId,
            Direction = EdoDirection.INBOX,
            Category = EdoDocumentCategory.INBOX,
            DocumentType = "FACTURA",
            DocumentNumber = candidate.DocumentNumber,
            DocumentDate = candidate.DocumentDate,
            DocumentDateTime = candidate.DocumentDateTime,
            Status = new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.SIGNED,
                IsTerminal = true,
                IsSuccessful = true
            },
            Seller = new EdoPartyDto
            {
                Name = candidate.SellerName ?? string.Empty,
                TaxIdentifier = candidate.SellerTin
            },
            PreviewSellerTin = candidate.SellerTin,
            PreviewContractNumber = candidate.ProviderContractNumber,
            PreviewContractDate = candidate.ProviderContractDate,
            TotalAmount = candidate.TotalAmount,
            PreviewLines = orderedLines.Select(line => new EdoDocumentPreviewLineDto
            {
                Number = line.ProviderLineNumber,
                CatalogCode = line.CatalogCode,
                CatalogName = line.ProviderProductName,
                PackageCode = line.PackageCode,
                PackageName = line.PackageName,
                IsService = line.IsService == true,
                NetAmount = line.NetAmount,
                VatAmount = line.VatAmount,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                VatRate = line.VatRate,
                TotalWithVat = line.TotalAmount,
                MarkingCodes = line.Markings.OrderBy(marking => marking.Id)
                    .Select(marking => marking.MarkingNumber).ToArray()
            }).ToArray()
        };
        var request = new PurchaseDocFromEdoRequestDto
        {
            DocumentIdentity = candidate.ProviderDocumentId,
            CounterpartyId = candidate.SelectedCounterpartyId.Value,
            ContractId = candidate.SelectedContractId.Value,
            CurrencyId = candidate.SelectedCurrencyId.Value,
            WarehouseId = candidate.SelectedWarehouseId.Value,
            Lines = orderedLines.Select(line => new PurchaseDocFromEdoLineDto
            {
                LineNumber = line.ProviderLineNumber,
                ProductId = line.SelectedProductId!.Value,
                UnitId = line.SelectedUnitId!.Value,
                VatRateId = line.SelectedVatRateId,
                DebitAccountId = line.SelectedDebitAccountId,
                VatAccountId = line.SelectedVatAccountId,
                Items = line.Markings.OrderBy(marking => marking.Id)
                    .Select(marking => new PurchaseDocLineItemDto
                    {
                        MarkingNumber = marking.MarkingNumber
                    }).ToList()
            }).ToList()
        };
        return Result.Success(new HistoricalDraftSource(document, request));
    }

    private static string BuildDraftImportFailureCode(string errorCode)
    {
        if (string.Equals(
                errorCode, "PurchaseFromEdo.ValidationFailed", StringComparison.Ordinal))
            return GenericValidationDraftFailure;

        var normalized = new string(errorCode.ToUpperInvariant()
            .Select(character => char.IsAsciiLetterOrDigit(character) ? character : '_')
            .ToArray()).Trim('_');
        if (normalized.Length > 70)
            normalized = normalized[..70];
        return normalized.Length == 0
            ? "DRAFT_IMPORT_VALIDATION_FAILURE"
            : normalized.StartsWith("DRAFT_IMPORT_", StringComparison.Ordinal)
                ? normalized
                : $"DRAFT_IMPORT_{normalized}";
    }

    private static bool CanRequeueDraftFailure(string? safeErrorCode) => safeErrorCode is
        ServiceItemsNotAllowedDraftFailure
        or ProductPieceTrackingDraftFailure
        or EarlierDocumentDateDraftFailure
        or GenericValidationDraftFailure;

    private sealed record HistoricalDraftSource(
        EdoDocumentDto Document,
        PurchaseDocFromEdoRequestDto Request);

    private sealed record DraftFailureDecision(
        EdoImportCandidate Candidate,
        string Action,
        long? DuplicatePurchaseId);

    private sealed record DraftImportOutcome(long PurchaseId, bool Created);

    private static Result<EdoImportProductConflictApplyCommandDto> BuildProductConflictApplyCommand(
        EdoImportProductConflictPlanDto plan,
        EdoImportProductConflictApplyRequestDto request,
        bool reuseOnly)
    {
        var commands = new List<EdoImportProductConflictApplyCommandItemDto>();
        foreach (var requested in request.Items)
        {
            var planned = requested.IdentityKeys.Select(key => plan.Items.SingleOrDefault(item =>
                string.Equals(item.IdentityKey, key, StringComparison.Ordinal))).ToArray();
            if (planned.Length == 0 || planned.Any(item => item is null))
                return InvalidProductConflictSelection();
            var selected = planned.Select(item => item!).ToArray();
            var catalogCode = selected[0].CatalogCode;
            if (selected.Any(item => item.BlockedReasonCodes.Count > 0
                    || item.IdentityKey is null || item.CatalogCode is null
                    || item.ProviderProductName is null || !item.IsService.HasValue
                    || !string.Equals(item.CatalogCode, catalogCode, StringComparison.Ordinal))
                || requested.Action is not ("CREATE" or "USE_EXISTING")
                || !requested.IsService.HasValue || !requested.UnitId.HasValue
                || !requested.VatRateId.HasValue || !requested.IsPieceTracked.HasValue
                || selected.Any(item => item.ResolvedVatRateId != requested.VatRateId)
                || selected.Any(item => item.IsService != requested.IsService)
                    && !requested.ConfirmItemTypeOverride)
                return InvalidProductConflictSelection();
            var markingRequired = selected.Any(item => item.MarkingRequired);
            if (requested.IsService == true
                    && (requested.IsPieceTracked != false || markingRequired)
                || requested.IsService == false
                    && markingRequired && requested.IsPieceTracked != true)
                return InvalidProductMarkingSelection();

            EdoImportCompatibleProductDto? existingProduct = null;
            if (requested.Action == "USE_EXISTING")
            {
                existingProduct = selected.SelectMany(item => item.CompatibleProducts)
                    .FirstOrDefault(product => product.ProductId == requested.ExistingProductId);
                if (existingProduct is null
                    || existingProduct.IsService != requested.IsService
                    || existingProduct.UnitId != requested.UnitId
                    || existingProduct.VatRateId != requested.VatRateId
                    || existingProduct.IsPieceTracked != requested.IsPieceTracked)
                    return InvalidProductConflictSelection();
            }
            else if (selected[0].ProviderProductName!.Length > 250)
            {
                return InvalidProductConflictSelection();
            }

            commands.Add(new EdoImportProductConflictApplyCommandItemDto
            {
                Identities = selected.Select(ToIdentityCommand).ToArray(),
                Action = requested.Action,
                ProductId = requested.ExistingProductId,
                ProductName = selected[0].ProviderProductName!,
                IsService = requested.IsService.Value,
                UnitId = requested.UnitId,
                VatRateId = requested.VatRateId,
                IsPieceTracked = requested.IsPieceTracked
            });
        }
        return Result.Success(new EdoImportProductConflictApplyCommandDto
        {
            ReuseOnly = reuseOnly,
            Items = commands
        });
    }

    private static Result<EdoImportProductConflictApplyCommandDto> InvalidProductConflictSelection() =>
        Result.Failure<EdoImportProductConflictApplyCommandDto>(Error.Business(
            "PRODUCT_CONFLICT_SELECTION_INVALID",
            "Every selection must match one current unresolved provider product identity exactly."));

    private static Result<EdoImportProductConflictApplyCommandDto> InvalidProductMarkingSelection() =>
        Result.Failure<EdoImportProductConflictApplyCommandDto>(Error.Conflict(
            "PRODUCT_MARKING_SELECTION_INVALID",
            "Piece tracking must preserve provider marking requirements and cannot be enabled for services."));

    private static Result<EdoImportProductConflictApplyCommandDto> BuildProductConflictReplayCommand(
        EdoImportProductConflictApplyRequestDto request,
        EdoImportProductConflictSourceDto source)
    {
        var commands = new List<EdoImportProductConflictApplyCommandItemDto>();
        foreach (var requested in request.Items)
        {
            var mappings = requested.IdentityKeys.Select(key => source.Mappings.SingleOrDefault(item =>
                string.Equals(item.IdentityHash, key, StringComparison.Ordinal))).ToArray();
            if (mappings.Length == 0 || mappings.Any(item => item is null))
                return InvalidProductConflictSelection();
            var selectedMappings = mappings.Select(item => item!).ToArray();
            var productIds = selectedMappings.Select(item => item.ProductId).Distinct().ToArray();
            var product = productIds.Length == 1
                ? source.Products.SingleOrDefault(item => item.Id == productIds[0])
                : null;
            if (product is null || requested.Action is not ("CREATE" or "USE_EXISTING")
                || requested.Action == "USE_EXISTING" && requested.ExistingProductId != product.Id
                || !requested.IsService.HasValue || requested.IsService != product.IsService
                || requested.UnitId != product.UnitId || requested.VatRateId != product.VatRateId
                || requested.IsPieceTracked != product.IsPieceTracked
                || selectedMappings.Any(item => item.IsService != requested.IsService)
                    && !requested.ConfirmItemTypeOverride)
                return InvalidProductConflictSelection();
            var markingRequired = selectedMappings.Any(mapping =>
                ProviderIdentityHasMarkings(source, mapping.IdentityHash));
            if (product.IsService && (product.IsPieceTracked || markingRequired)
                || !product.IsService && markingRequired && !product.IsPieceTracked)
                return InvalidProductMarkingSelection();
            commands.Add(new EdoImportProductConflictApplyCommandItemDto
            {
                Identities = selectedMappings.Select(ToIdentityCommand).ToArray(),
                Action = requested.Action,
                ProductId = product.Id,
                ProductName = product.Name,
                IsService = product.IsService,
                UnitId = product.UnitId,
                VatRateId = product.VatRateId,
                IsPieceTracked = product.IsPieceTracked
            });
        }
        return Result.Success(new EdoImportProductConflictApplyCommandDto
        {
            ReuseOnly = true,
            Items = commands
        });
    }

    private static EdoImportProviderProductIdentityCommandDto ToIdentityCommand(
        EdoImportProductConflictItemDto item) => new()
    {
        IdentityHash = item.IdentityKey!,
        ProviderCode = item.ProviderCode,
        CatalogCode = item.CatalogCode!,
        PackageCode = item.PackageCode ?? string.Empty,
        ProviderProductName = item.ProviderProductName!,
        ProviderProductNameHash = EdoProviderProductIdentity.HashName(item.ProviderProductName!),
        IsService = item.IsService!.Value
    };

    private static EdoImportProviderProductIdentityCommandDto ToIdentityCommand(
        EdoImportProviderProductMappingSourceDto item) => new()
    {
        IdentityHash = item.IdentityHash,
        ProviderCode = item.ProviderCode,
        CatalogCode = item.CatalogCode,
        PackageCode = item.PackageCode,
        ProviderProductName = item.ProviderProductName,
        ProviderProductNameHash = item.ProviderProductNameHash,
        IsService = item.IsService
    };

    private static IReadOnlyCollection<string> ProductCompatibilityCodes(
        EdoImportExistingProductSourceDto product,
        bool providerIsService,
        short? resolvedVatRateId)
    {
        var codes = new List<string>();
        if (product.IsService != providerIsService)
            codes.Add("ITEM_TYPE_MISMATCH");
        if (product.VatRateId != resolvedVatRateId)
            codes.Add("VAT_RATE_MISMATCH");
        return codes.Count == 0 ? ["STRICT_COMPATIBLE"] : codes;
    }

    private static bool ProviderIdentityHasMarkings(
        EdoImportProductConflictSourceDto source,
        string identityHash) => source.Candidates.Any(candidate => candidate.Lines.Any(line =>
        line.HasProviderMarkings
        && string.Equals(EdoProviderProductIdentity.Create(
                candidate.ProviderCode,
                line.CatalogCode,
                line.PackageCode,
                line.ProviderProductName,
                line.IsService),
            identityHash,
            StringComparison.Ordinal)));

    private static Result<EdoImportMasterDataApplyCommandDto> BuildApplyCommand(
        EdoImportMasterDataPlanDto plan,
        EdoImportMasterDataApplyRequestDto request)
    {
        if (request.Counterparties.GroupBy(item => NormalizeSummaryText(item.SellerTin), StringComparer.Ordinal)
            .Any(group => group.Key is null || group.Count() != 1)
            || request.Contracts.GroupBy(item => ContractKey(string.Empty,
                    NormalizeSummaryText(item.SellerTin) ?? string.Empty,
                    NormalizeSummaryText(item.ProviderContractNumber) ?? string.Empty,
                    item.ProviderContractDate), StringComparer.Ordinal)
                .Any(group => group.Count() != 1)
            || request.Contracts.SelectMany(item => item.CandidateIds)
                .GroupBy(candidateId => candidateId)
                .Any(group => group.Key <= 0 || group.Count() != 1)
            || request.Products.GroupBy(item => NormalizeSummaryText(item.CatalogCode), StringComparer.Ordinal)
                .Any(group => group.Key is null || group.Count() != 1))
        {
            return InvalidApplySelection();
        }

        var counterparties = new List<EdoImportCounterpartyApplyCommandItemDto>();
        foreach (var requested in request.Counterparties)
        {
            var sellerTin = NormalizeSummaryText(requested.SellerTin)!;
            var planned = plan.Counterparties.SingleOrDefault(item =>
                string.Equals(item.SellerTin, sellerTin, StringComparison.Ordinal));
            if (planned is null || !IsDigitsOnly(sellerTin)
                || requested.Action == "CREATE" && (planned.Action != "CREATE"
                    || planned.CanonicalSellerName is not { Length: > 0 and <= 250 })
                || requested.Action == "USE_EXISTING" && (planned.Action != "USE_EXISTING"
                    || !requested.ExistingCounterpartyId.HasValue
                    || requested.ExistingCounterpartyId != planned.ExistingCounterpartyId)
                || requested.Action is not ("CREATE" or "USE_EXISTING"))
            {
                return InvalidApplySelection();
            }
            counterparties.Add(new EdoImportCounterpartyApplyCommandItemDto
            {
                SellerTin = sellerTin,
                Name = planned.CanonicalSellerName!,
                Action = requested.Action,
                ExistingId = requested.ExistingCounterpartyId
            });
        }

        var contracts = new List<EdoImportContractApplyCommandItemDto>();
        foreach (var requested in request.Contracts)
        {
            var sellerTin = NormalizeSummaryText(requested.SellerTin)!;
            var number = NormalizeSummaryText(requested.ProviderContractNumber);
            var hasProviderIdentity = number is { Length: > 0 }
                && requested.ProviderContractDate != default;
            var requestedCandidateIds = requested.CandidateIds.Distinct().Order().ToArray();
            var planned = hasProviderIdentity
                ? plan.Contracts.SingleOrDefault(item =>
                    string.Equals(item.SellerTin, sellerTin, StringComparison.Ordinal)
                    && string.Equals(item.ProviderContractNumber, number, StringComparison.Ordinal)
                    && item.ProviderContractDate == requested.ProviderContractDate)
                : plan.Contracts.SingleOrDefault(item =>
                    string.Equals(item.SellerTin, sellerTin, StringComparison.Ordinal)
                    && item.ProviderContractNumber is null
                    && !item.ProviderContractDate.HasValue
                    && item.CandidateIds.Order().SequenceEqual(requestedCandidateIds));
            var validReuse = requested.ExistingContractId.HasValue && planned is not null
                && (hasProviderIdentity
                    ? planned.Action == "USE_EXISTING"
                        && requested.ExistingContractId == planned.ExistingContractId
                        || planned.Action == "REQUIRES_SELECTION"
                        && planned.ReconciliationContractIds.Contains(requested.ExistingContractId.Value)
                    : planned.Action == "REQUIRES_SELECTION"
                        && planned.ReconciliationContractIds.Contains(requested.ExistingContractId.Value));
            if (planned is null
                || !IsDigitsOnly(sellerTin)
                || !hasProviderIdentity && (number is not null || requested.ProviderContractDate != default
                    || requestedCandidateIds.Length == 0
                    || requested.CandidateIds.Count != requestedCandidateIds.Length)
                || requested.Action == "CREATE" && (!hasProviderIdentity || planned.Action != "CREATE")
                || requested.Action == "USE_EXISTING" && !validReuse
                || requested.Action is not ("CREATE" or "USE_EXISTING"))
            {
                return InvalidApplySelection();
            }
            contracts.Add(new EdoImportContractApplyCommandItemDto
            {
                ProviderCode = planned.ProviderCode,
                SellerTin = sellerTin,
                Number = number ?? string.Empty,
                Date = requested.ProviderContractDate,
                HasProviderIdentity = hasProviderIdentity,
                CandidateIds = hasProviderIdentity ? [] : requestedCandidateIds,
                Action = requested.Action,
                ExistingId = requested.ExistingContractId
            });
        }

        var products = new List<EdoImportProductApplyCommandItemDto>();
        foreach (var requested in request.Products)
        {
            var catalogCode = NormalizeSummaryText(requested.CatalogCode)!;
            var planned = plan.Products.SingleOrDefault(item =>
                string.Equals(item.CatalogCode, catalogCode, StringComparison.Ordinal));
            if (planned is null || requested.Action is not ("CREATE" or "USE_EXISTING"))
                return InvalidApplySelection();
            if (requested.Action == "USE_EXISTING")
            {
                if (planned.Action != "USE_EXISTING"
                    || !requested.ExistingProductId.HasValue
                    || requested.ExistingProductId != planned.ExistingProductId
                    || !planned.IsService.HasValue
                    || !planned.ResolvedUnitId.HasValue
                    || !planned.ResolvedVatRateId.HasValue)
                    return InvalidApplySelection();
                products.Add(new EdoImportProductApplyCommandItemDto
                {
                    CatalogCode = catalogCode,
                    Name = planned.ProviderProductName ?? string.Empty,
                    IsService = planned.IsService.Value,
                    IsPieceTracked = requested.IsPieceTracked ?? false,
                    UnitId = planned.ResolvedUnitId.Value,
                    VatRateId = planned.ResolvedVatRateId.Value,
                    Action = requested.Action,
                    ExistingId = requested.ExistingProductId
                });
                continue;
            }

            var onlyExplicitUnitBlocker = planned.Action == "BLOCKED"
                && planned.BlockedReasonCodes.All(code => code == "PRODUCT_UNIT_REQUIRED");
            if (planned.Action != "CREATE" && !onlyExplicitUnitBlocker
                || planned.ProviderProductName is not { Length: > 0 and <= 250 }
                || !planned.IsService.HasValue
                || !planned.ResolvedVatRateId.HasValue
                || !requested.IsService.HasValue
                || requested.IsService != planned.IsService
                || !requested.IsPieceTracked.HasValue
                || !requested.UnitId.HasValue
                || requested.UnitId <= 0
                || !requested.VatRateId.HasValue
                || requested.VatRateId != planned.ResolvedVatRateId
                || planned.ResolvedUnitId.HasValue && requested.UnitId != planned.ResolvedUnitId
                || planned.MarkingRequired && requested.IsPieceTracked != true)
            {
                return Result.Failure<EdoImportMasterDataApplyCommandDto>(Error.Business(
                    "PRODUCT_MASTER_DATA_RESOLUTION_REQUIRED",
                    "Product creation requires an explicit non-conflicting item type, unit, VAT and marking choice."));
            }
            products.Add(new EdoImportProductApplyCommandItemDto
            {
                CatalogCode = catalogCode,
                Name = planned.ProviderProductName,
                IsService = requested.IsService.Value,
                IsPieceTracked = requested.IsPieceTracked.Value,
                UnitId = requested.UnitId.Value,
                VatRateId = requested.VatRateId.Value,
                Action = requested.Action
            });
        }

        var command = new EdoImportMasterDataApplyCommandDto
        {
            Counterparties = counterparties,
            Contracts = contracts,
            Products = products
        };
        return Result.Success(command);
    }

    private static Result<EdoImportMasterDataApplyCommandDto> InvalidApplySelection() =>
        Result.Failure<EdoImportMasterDataApplyCommandDto>(Error.Business(
            "MASTER_DATA_SELECTION_INVALID",
            "Every selected item must match one current master-data plan item exactly."));

    private static Result<EdoImportMasterDataApplyCommandDto> BuildIdempotentReplayCommand(
        EdoImportMasterDataApplyRequestDto request,
        EdoImportMasterDataPlanSourceDto source)
    {
        if (request.Counterparties.Count + request.Contracts.Count + request.Products.Count == 0
            || request.Counterparties.GroupBy(item => NormalizeSummaryText(item.SellerTin), StringComparer.Ordinal)
                .Any(group => group.Key is null || group.Count() != 1)
            || request.Contracts.GroupBy(item => ContractKey(string.Empty,
                    NormalizeSummaryText(item.SellerTin) ?? string.Empty,
                    NormalizeSummaryText(item.ProviderContractNumber) ?? string.Empty,
                    item.ProviderContractDate), StringComparer.Ordinal)
                .Any(group => group.Count() != 1)
            || request.Products.GroupBy(item => NormalizeSummaryText(item.CatalogCode), StringComparer.Ordinal)
                .Any(group => group.Key is null || group.Count() != 1)
            || request.Counterparties.Any(item =>
                !IsDigitsOnly(NormalizeSummaryText(item.SellerTin))
                || item.Action is not ("CREATE" or "USE_EXISTING"))
            || request.Contracts.Any(item =>
                !IsDigitsOnly(NormalizeSummaryText(item.SellerTin))
                || item.Action is not ("CREATE" or "USE_EXISTING")
                || HasProviderContractIdentity(item)
                    && (NormalizeSummaryText(item.ProviderContractNumber) is not { Length: > 0 and <= 100 }
                        || item.ProviderContractDate == default)
                || !HasProviderContractIdentity(item)
                    && (NormalizeSummaryText(item.ProviderContractNumber) is not null
                        || item.ProviderContractDate != default
                        || item.Action != "USE_EXISTING"
                        || !item.ExistingContractId.HasValue
                        || item.CandidateIds.Count == 0
                        || item.CandidateIds.Distinct().Count() != item.CandidateIds.Count))
                || request.Contracts.Any(item => !HasProviderContractIdentity(item)
                    && source.Candidates.Count(candidate =>
                        item.CandidateIds.Contains(candidate.CandidateId)
                        && string.Equals(NormalizeSummaryText(candidate.SellerTin),
                            NormalizeSummaryText(item.SellerTin), StringComparison.Ordinal)
                        && candidate.ProviderContractNumber is null
                        && !candidate.ProviderContractDate.HasValue
                        && candidate.ContractId == item.ExistingContractId) != item.CandidateIds.Count)
            || request.Products.Any(item =>
                !IsValidCatalogCode(NormalizeSummaryText(item.CatalogCode))
                || item.Action is not ("CREATE" or "USE_EXISTING")
                || item.Action == "USE_EXISTING" && !item.ExistingProductId.HasValue
                || item.Action == "CREATE" && (!item.IsService.HasValue
                    || !item.IsPieceTracked.HasValue
                    || !item.UnitId.HasValue
                    || !item.VatRateId.HasValue)))
            return InvalidApplySelection();

        var command = new EdoImportMasterDataApplyCommandDto
        {
            ReuseOnly = true,
            Counterparties = request.Counterparties.Select(item =>
                new EdoImportCounterpartyApplyCommandItemDto
                {
                    SellerTin = NormalizeSummaryText(item.SellerTin)!,
                    Action = item.Action,
                    ExistingId = item.ExistingCounterpartyId
                }).ToArray(),
            Contracts = request.Contracts.Select(item =>
            {
                var sellerTin = NormalizeSummaryText(item.SellerTin)!;
                var hasProviderIdentity = HasProviderContractIdentity(item);
                var number = NormalizeSummaryText(item.ProviderContractNumber) ?? string.Empty;
                var requestedCandidateIds = item.CandidateIds.Distinct().Order().ToArray();
                var matchingCandidates = source.Candidates.Where(candidate =>
                        string.Equals(NormalizeSummaryText(candidate.SellerTin), sellerTin, StringComparison.Ordinal)
                        && (hasProviderIdentity
                            ? string.Equals(NormalizeSummaryText(candidate.ProviderContractNumber), number,
                                StringComparison.Ordinal)
                                && candidate.ProviderContractDate == item.ProviderContractDate
                            : requestedCandidateIds.Contains(candidate.CandidateId)
                                && candidate.ProviderContractNumber is null
                                && !candidate.ProviderContractDate.HasValue
                                && candidate.ContractId == item.ExistingContractId))
                    .ToArray();
                var providerCodes = matchingCandidates
                    .Select(candidate => candidate.ProviderCode)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                return new EdoImportContractApplyCommandItemDto
                {
                    ProviderCode = providerCodes.Length == 1 ? providerCodes[0] : string.Empty,
                    SellerTin = sellerTin,
                    Number = number,
                    Date = item.ProviderContractDate,
                    HasProviderIdentity = hasProviderIdentity,
                    CandidateIds = hasProviderIdentity ? [] : requestedCandidateIds,
                    Action = item.Action,
                    ExistingId = item.ExistingContractId
                };
            }).ToArray(),
            Products = request.Products.Select(item =>
                new EdoImportProductApplyCommandItemDto
                {
                    CatalogCode = NormalizeSummaryText(item.CatalogCode)!,
                    IsService = item.IsService ?? false,
                    IsPieceTracked = item.IsPieceTracked ?? false,
                    UnitId = item.UnitId ?? 0,
                    VatRateId = item.VatRateId ?? 0,
                    Action = item.Action,
                    ExistingId = item.ExistingProductId
                }).ToArray()
        };
        return command.Contracts.All(item => item.ProviderCode.Length > 0)
            ? Result.Success(command)
            : InvalidApplySelection();
    }

    private static Error StaleMasterDataPlan() => Error.Conflict(
        "STALE_MASTER_DATA_PLAN",
        "The master-data plan changed and must be reviewed again.");

    private static Error StaleProductConflictPlan() => Error.Conflict(
        "STALE_PRODUCT_CONFLICT_PLAN",
        "The provider product conflict plan changed and must be reviewed again.");

    private static Error StaleMarkingConflictPlan() => Error.Conflict(
        "STALE_MARKING_CONFLICT_PLAN",
        "The marking conflict plan changed and must be reviewed again.");

    private static Error StaleImportPlan() => Error.Conflict(
        "STALE_IMPORT_PLAN",
        "The Draft Purchase import plan changed and must be reviewed again.");

    private static Error StaleDraftFailurePlan() => Error.Conflict(
        "STALE_DRAFT_IMPORT_FAILURE_PLAN",
        "The Draft import failure plan changed and must be reviewed again.");

    private static Error InvalidDraftFailureSelection() => Error.Conflict(
        "DRAFT_IMPORT_FAILURE_SELECTION_INVALID",
        "Only current unlinked non-retryable historical validation failures can be skipped.");

    private static Error DraftImportSourceInvalid() => Error.Business(
        "DRAFT_IMPORT_SOURCE_INVALID",
        "The historical candidate source or persisted mappings are incomplete.");

    private static string ContractKey(
        string providerCode,
        string sellerTin,
        string number,
        DateOnly date) =>
        $"{providerCode}\u001f{sellerTin}\u001f{number}\u001f{date:yyyy-MM-dd}";

    private static bool HasProviderContractIdentity(EdoImportContractApplyItemDto item) =>
        NormalizeSummaryText(item.ProviderContractNumber) is { Length: > 0 }
        && item.ProviderContractDate != default;

    private static bool IsApplicableContractForDates(
        EdoImportExistingContractSourceDto contract,
        IReadOnlyCollection<DateOnly> documentDates) =>
        documentDates.Count == 0
        || documentDates.All(date => (!contract.StartDate.HasValue || contract.StartDate.Value <= date)
            && (!contract.EndDate.HasValue || contract.EndDate.Value >= date));

    private static bool IsDigitsOnly(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.All(char.IsAsciiDigit);

    private static bool IsValidCatalogCode(string? value) =>
        value is { Length: 17 } && value.All(char.IsAsciiDigit);

    private sealed record ProductPlanLine(
        long CandidateId,
        string? CatalogCode,
        string? ProviderProductName,
        bool? IsService,
        string? PackageCode,
        string? PackageName,
        short? UnitId,
        short? VatRateId,
        bool MarkingRequired);

    private sealed record ProductConflictLine(
        long CandidateId,
        string ProviderCode,
        DateOnly? DocumentDate,
        EdoImportMappingSummaryLineSourceDto Line);

    private sealed record ProductConflictIdentity(
        string ProviderCode,
        string? CatalogCode,
        string PackageCode,
        string? ProviderProductName,
        bool? IsService);

    private static bool IsMarkingIssue(EdoImportMappingSummaryCandidateSourceDto candidate) =>
        string.Equals(candidate.SafeErrorCode, "MARKING_MAPPING_REQUIRED", StringComparison.Ordinal)
        || candidate.Lines.Any(line => line.ProductId.HasValue
            && line.UnitId.HasValue
            && line.VatRateId.HasValue
            && line.MappingStatus != EdoImportMappingStatus.Resolved);

    private static string? NormalizeSummaryText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task RefreshJobMappingCountsAsync(EdoImportJob job, CancellationToken ct)
    {
        var counts = await _store.GetJobMappingCountsAsync(job.OrganizationId, job.Id, ct);
        job.ReadyCount = counts.ReadyCount;
        job.MappingRequiredCount = counts.MappingRequiredCount;
        job.DuplicateCount = counts.DuplicateCount;
        await _store.SaveChangesAsync(ct);
    }

    private static IReadOnlyCollection<EdoHistoricalDocumentLineDto> ToHistoricalLines(
        EdoImportCandidate candidate) =>
        candidate.Lines.Select(line => new EdoHistoricalDocumentLineDto
        {
            Number = line.ProviderLineNumber,
            CatalogCode = line.CatalogCode,
            CatalogName = line.ProviderProductName,
            PackageCode = line.PackageCode,
            PackageName = line.PackageName,
            IsService = line.IsService == true,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            NetAmount = line.NetAmount,
            VatRate = line.VatRate,
            VatAmount = line.VatAmount,
            TotalAmount = line.TotalAmount,
            MarkingNumbers = line.Markings.Select(marking => marking.MarkingNumber).ToArray()
        }).ToArray();

    private static EdoImportMappingSelectionDto ToPersistedMappingSelection(
        EdoImportCandidate candidate) => new()
    {
        UseAutomaticFallbackForMissingSelections = true,
        ProviderCode = candidate.ProviderCode,
        ProviderContractNumber = candidate.ProviderContractNumber,
        ProviderContractDate = candidate.ProviderContractDate,
        CounterpartyId = candidate.SelectedCounterpartyId,
        ContractId = candidate.SelectedContractId,
        CurrencyId = candidate.SelectedCurrencyId,
        WarehouseId = candidate.SelectedWarehouseId,
        Lines = candidate.Lines.ToDictionary(line => line.ProviderLineNumber, line =>
            new EdoImportLineMappingSelectionDto
            {
                ProductId = line.SelectedProductId,
                UnitId = line.SelectedUnitId,
                VatRateId = line.SelectedVatRateId,
                DebitAccountId = line.SelectedDebitAccountId,
                VatAccountId = line.SelectedVatAccountId
            })
    };

    private static void ApplyMapping(
        EdoImportCandidate candidate,
        EdoImportMappingResolutionDto mapping,
        bool preserveAccounts,
        DateTime now)
    {
        candidate.SelectedCounterpartyId = mapping.CounterpartyId;
        candidate.SelectedContractId = mapping.ContractId;
        candidate.SelectedCurrencyId = mapping.CurrencyId;
        candidate.SelectedWarehouseId = mapping.WarehouseId;

        var allLinesMapped = candidate.Lines.Count > 0;
        string? persistedStructuralFailure = EdoImportMarkingPolicy.IsStructuralFailure(candidate.SafeErrorCode)
            ? candidate.SafeErrorCode
            : null;
        var markingStructureValid = persistedStructuralFailure is null;
        string? markingFailureCode = persistedStructuralFailure;
        foreach (var line in candidate.Lines)
        {
            var resolved = mapping.Lines.GetValueOrDefault(line.ProviderLineNumber)
                ?? new EdoImportLineMappingResolutionDto();
            line.SelectedProductId = resolved.ProductId;
            line.SelectedUnitId = resolved.UnitId;
            line.SelectedVatRateId = resolved.VatRateId;
            if (!preserveAccounts)
            {
                line.SelectedDebitAccountId = resolved.DebitAccountId;
                line.SelectedVatAccountId = resolved.VatAccountId;
            }

            var normalizedMarkings = line.Markings
                .Select(marking => marking.MarkingNumber.Trim())
                .ToArray();
            var isService = line.IsService == true || resolved.IsService;
            var structuralFailure = persistedStructuralFailure
                ?? EdoImportMarkingPolicy.ValidateStructure(
                    resolved.IsPieceTracked,
                    isService,
                    line.Quantity,
                    isService ? [] : normalizedMarkings);
            var hasUsedMarking = !isService
                && normalizedMarkings.Any(mapping.PreviouslyUsedMarkings.Contains);
            var lineFailureCode = structuralFailure
                ?? (resolved.IsPieceTracked && hasUsedMarking
                    ? EdoImportMarkingPolicy.AlreadyUsed
                    : null);
            var markingValid = lineFailureCode is null;
            markingStructureValid &= structuralFailure is null;
            markingFailureCode ??= lineFailureCode;
            foreach (var marking in line.Markings)
            {
                marking.ProviderVerificationState = markingValid
                    ? EdoImportMarkingVerificationState.Verified
                    : EdoImportMarkingVerificationState.Mismatch;
                marking.UpdatedDate = now;
            }

            var lineMapped = resolved.ProductId.HasValue
                && resolved.UnitId.HasValue
                && resolved.VatRateId.HasValue
                && markingValid;
            line.MappingStatus = lineMapped
                ? EdoImportMappingStatus.Resolved
                : EdoImportMappingStatus.Unresolved;
            line.UpdatedDate = now;
            allLinesMapped &= lineMapped;
        }

        var allMapped = mapping.CounterpartyId.HasValue
            && mapping.ContractId.HasValue
            && mapping.CurrencyId.HasValue
            && mapping.WarehouseId.HasValue
            && allLinesMapped;
        candidate.MappingStatus = allMapped
            ? EdoImportMappingStatus.Resolved
            : EdoImportMappingStatus.Partial;
        if (markingStructureValid && mapping.ExistingPurchaseIdForAllMarkings.HasValue)
        {
            candidate.DuplicateState = EdoImportDuplicateState.Confirmed;
            candidate.ExistingPurchaseId = mapping.ExistingPurchaseIdForAllMarkings;
            candidate.SafeErrorCode = null;
            if (!string.Equals(candidate.Status, EdoImportCandidateStatus.Duplicate, StringComparison.Ordinal))
                candidate.TransitionTo(EdoImportCandidateStatus.Duplicate, now);
            return;
        }

        candidate.SafeErrorCode = allMapped
            ? null
            : ResolveMappingFailureCode(candidate, mapping, markingFailureCode);

        var targetStatus = allMapped
            ? EdoImportCandidateStatus.Ready
            : EdoImportCandidateStatus.MappingRequired;
        if (!string.Equals(candidate.Status, targetStatus, StringComparison.Ordinal))
            candidate.TransitionTo(targetStatus, now);
    }

    private static string ResolveMappingFailureCode(
        EdoImportCandidate candidate,
        EdoImportMappingResolutionDto mapping,
        string? markingFailureCode)
    {
        if (markingFailureCode is not null)
            return markingFailureCode;
        if (!mapping.CounterpartyId.HasValue)
            return "COUNTERPARTY_MAPPING_REQUIRED";
        if (!mapping.ContractId.HasValue)
            return "CONTRACT_MAPPING_REQUIRED";
        if (!mapping.CurrencyId.HasValue)
            return "CURRENCY_MAPPING_REQUIRED";
        if (!mapping.WarehouseId.HasValue)
            return "WAREHOUSE_MAPPING_REQUIRED";
        if (candidate.Lines.Any(line => !line.SelectedProductId.HasValue))
            return "PRODUCT_MAPPING_REQUIRED";
        if (candidate.Lines.Any(line => !line.SelectedUnitId.HasValue))
            return "UNIT_MAPPING_REQUIRED";
        if (candidate.Lines.Any(line => !line.SelectedVatRateId.HasValue))
            return "VAT_MAPPING_REQUIRED";
        if (candidate.Lines.Any(line =>
                line.SelectedProductId.HasValue
                && line.SelectedUnitId.HasValue
                && line.SelectedVatRateId.HasValue
                && line.MappingStatus != EdoImportMappingStatus.Resolved))
            return "MARKING_MAPPING_REQUIRED";
        return "LINE_MAPPING_REQUIRED";
    }

    private static bool HasExactLineCoverage(
        EdoImportCandidate candidate,
        IReadOnlyCollection<EdoImportCandidateLineMappingRequestDto>? requestedLines)
    {
        if (requestedLines is null || requestedLines.Count != candidate.Lines.Count)
            return false;
        var requestedNumbers = requestedLines.Select(line => line.LineNumber).ToArray();
        return requestedNumbers.Distinct().Count() == requestedNumbers.Length
            && requestedNumbers.ToHashSet().SetEquals(
                candidate.Lines.Select(line => line.ProviderLineNumber));
    }

    private static bool IsMappingAllowed(string status) => status is
        EdoImportJobStatus.PreflightReady or EdoImportJobStatus.Partial;

    private static bool IsDraftImportAllowed(string status) => status is
        EdoImportJobStatus.PreflightReady or EdoImportJobStatus.Partial;

    private static bool IsCandidateMappable(string status) => status is
        EdoImportCandidateStatus.MappingRequired or EdoImportCandidateStatus.Ready;

    private static Error OrganizationRequired() => Error.Forbidden(
        "EdoImport.OrganizationContextRequired",
        "An authenticated organization context is required.");
    private static Error NotFound(long jobId) => Error.NotFound(
        "EdoImport.JobNotFound",
        $"EDO import job {jobId} was not found in this organization.");
    private static Error CandidateNotFound() => Error.NotFound(
        "EdoImport.CandidateNotFound",
        "The EDO import candidate was not found in this organization job.");
    private static Error JobNotMappable() => Error.Conflict(
        "EdoImport.JobNotMappable",
        "Mappings can be changed only for PREFLIGHT_READY or PARTIAL jobs.");
    private static Error CandidateNotMappable() => Error.Conflict(
        "EdoImport.CandidateNotMappable",
        "Only MAPPING_REQUIRED or READY candidates can be mapped.");
    private static Error InvalidLineCoverage() => Error.Business(
        "EdoImport.MappingLineCoverageInvalid",
        "The mapping request must contain every candidate line exactly once.");
    private static Error CandidateSourceInvalid() => Error.Business(
        "EdoImport.CandidateSourceInvalid",
        "The candidate does not contain the source values required for mapping.");
    private static Error DraftRequeuePurchaseLinked() => Error.Conflict(
        "DRAFT_IMPORT_REQUEUE_PURCHASE_LINKED",
        "A candidate linked to a Purchase cannot be requeued.");
}
