using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features.PurchaseDocs;
using Application.Features.SaleDocs;
using Application.Features.SaleDocs.EdoSalePreflight;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Integration.Edo.UnifiedImport;

public sealed class EdoUnifiedImportService(
    IUserContext userContext,
    IActiveEdoProviderResolver providerResolver,
    IEdoInboxService inboxService,
    IEdoDocumentStore documentStore,
    IEdoUnifiedImportStore batchStore,
    IUnitOfWork unitOfWork,
    IEdoHistoricalPurchaseDraftFactory purchaseFactory,
    IEdoSalePreflightService salePreflight,
    IEdoSaleDraftApplyService saleApply,
    IQueryBuilder queryBuilder,
    IQueryRepository<ProductTable> productTableQuery,
    IWarehouseInventoryService warehouseInventoryService,
    ILogger<EdoUnifiedImportService> logger) : IEdoUnifiedImportService
{
    private const string Edocs = "EDOCS";
    private const string Factura = EdoUnifiedImportPlanRules.Factura;
    private const string Signed = "SIGNED";
    private const string Sent = "SENT";

    public async Task<Result<EdoUnifiedImportPlanDto>> GetPlanAsync(
        IReadOnlyCollection<string>? providerDocumentIds = null,
        bool allowSentDocuments = false,
        CancellationToken ct = default,
        bool allowUnmatchedMarkings = false)
    {
        var organizationId = CurrentOrganization();
        var provider = await providerResolver.GetActiveProviderAsync(ct);
        if (!string.Equals(provider.Code.ToString(), Edocs, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<EdoUnifiedImportPlanDto>(EdoUnifiedImportErrors.ActiveProviderRequired(userContext.LanguageId));

        var selectedIds = EdoUnifiedImportPlanRules.NormalizeSelection(providerDocumentIds);
        var documents = await ReadAllDocumentsAsync(ct);
        var salePreflightCandidates = await LoadSalePreflightCandidatesAsync(
            documents,
            allowSentDocuments,
            ct);
        var items = new List<EdoUnifiedImportPlanItemDto>(documents.Count);
        foreach (var document in documents
                     .Where(x => string.Equals(x.ProviderCode.ToString(), Edocs, StringComparison.OrdinalIgnoreCase))
                     .Where(x => x.Direction == EdoDirection.OUTBOX
                         ? EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType(x.DocumentType)
                         : EdoUnifiedImportPlanRules.IsFactura(x.DocumentType))
                     .Where(x => selectedIds is null || selectedIds.Contains(x.ProviderDocumentId ?? string.Empty))
                     .OrderBy(x => x.ProviderDocumentId, StringComparer.Ordinal))
        {
            salePreflightCandidates.TryGetValue(document.ProviderDocumentId ?? string.Empty, out var saleCandidate);
            items.Add(await BuildPlanItemAsync(
                document,
                provider,
                organizationId,
                allowSentDocuments,
                allowUnmatchedMarkings,
                saleCandidate,
                ct));
        }

        var alreadyImportedCount = items.Count(x => x.Status == EdoImportBatchDocumentStatus.AlreadyImported);
        var candidates = items
            .Where(x => x.Status != EdoImportBatchDocumentStatus.AlreadyImported)
            .ToList();

        var plan = new EdoUnifiedImportPlanDto
        {
            AllowUnmatchedMarkings = allowUnmatchedMarkings,
            TotalCandidates = candidates.Count,
            SignedCount = candidates.Count(x => string.Equals(x.ProviderStatus, Signed, StringComparison.OrdinalIgnoreCase)),
            WaitingForSignatureCount = candidates.Count(x => x.Status == EdoImportBatchDocumentStatus.WaitingForSignature),
            BlockedCount = candidates.Count(x => x.Status == EdoImportBatchDocumentStatus.Blocked),
            AlreadyImportedCount = alreadyImportedCount,
            WaybillLocalCount = candidates.Count(x => x.DocumentType == EdoUnifiedImportPlanRules.WaybillLocal),
            Items = candidates,
            AllItems = items
        };

        return Result.Success(new EdoUnifiedImportPlanDto
        {
            ProviderCode = plan.ProviderCode,
            AllowUnmatchedMarkings = plan.AllowUnmatchedMarkings,
            TotalCandidates = plan.TotalCandidates,
            SignedCount = plan.SignedCount,
            WaitingForSignatureCount = plan.WaitingForSignatureCount,
            BlockedCount = plan.BlockedCount,
            AlreadyImportedCount = plan.AlreadyImportedCount,
            WaybillLocalCount = plan.WaybillLocalCount,
            Items = plan.Items,
            AllItems = plan.AllItems,
            PlanHash = ComputePlanHash(plan)
        });
    }

    public async Task<Result<EdoUnifiedImportApplyResponseDto>> ApplyBatchAsync(
        EdoUnifiedImportApplyRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = CurrentOrganization();
        if (!request.Confirm)
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.ConfirmationRequired(userContext.LanguageId));
        if (!IsSha256(request.ExpectedPlanHash))
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.InvalidPlanHash(userContext.LanguageId));
        if (request.Items.Count == 0 || request.Items.Count > 100)
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.InvalidItems(userContext.LanguageId));

        var currentPlanResult = await GetPlanAsync(
            request.Items.Select(x => x.ProviderDocumentId).ToArray(),
            request.AllowSentDocuments,
            ct,
            request.AllowUnmatchedMarkings);
        if (!currentPlanResult.IsSuccess)
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(currentPlanResult.Error);
        if (!string.Equals(request.ExpectedPlanHash, currentPlanResult.Value.PlanHash, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.StalePlan(userContext.LanguageId));

        var idempotencyKey = EdoUnifiedImportIdempotency.Compute(organizationId, request);
        var existingBatch = await batchStore.FindByIdempotencyKeyAsync(organizationId, idempotencyKey, ct);
        if (existingBatch is not null)
            return Result.Success(ToApplyResponse(existingBatch, await batchStore.GetBatchDocumentsAsync(organizationId, existingBatch.Id, ct)));

        var now = DateTime.UtcNow;
        var batch = new EdoImportBatch(organizationId, currentPlanResult.Value.PlanHash, idempotencyKey, now);
        var planById = currentPlanResult.Value.AllItems
            .ToDictionary(x => x.ProviderDocumentId, StringComparer.Ordinal);
        foreach (var item in request.Items)
        {
            if (!planById.TryGetValue(item.ProviderDocumentId, out var planItem) ||
                !string.Equals(item.Direction, planItem.Direction, StringComparison.OrdinalIgnoreCase)
                || EdoUnifiedImportPlanRules.NormalizeDocumentType(item.DocumentType) != planItem.DocumentType)
                return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.DocumentNotInPlan(userContext.LanguageId));

        }

        await unitOfWork.BeginAsync(ct);
        try
        {
            await batchStore.AcquireOrganizationLockAsync(organizationId, ct);
            await batchStore.AddBatchAsync(batch, ct);
            await unitOfWork.SaveChangesAsync(ct);
            var batchDocuments = new List<EdoImportBatchDocument>(request.Items.Count);
            foreach (var item in request.Items)
            {
                var planItem = planById[item.ProviderDocumentId];
                var status = planItem.Status == EdoImportBatchDocumentStatus.AlreadyImported
                    ? EdoImportBatchDocumentStatus.AlreadyImported
                    : planItem.Status == EdoImportBatchDocumentStatus.WaitingForSignature
                        ? EdoImportBatchDocumentStatus.WaitingForSignature
                        : planItem.Status == EdoImportBatchDocumentStatus.Blocked
                          && EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType(planItem.DocumentType)
                          && (planItem.ProviderStatus.Equals(Signed, StringComparison.OrdinalIgnoreCase)
                              || planItem.SentOverrideApplied)
                            ? EdoImportBatchDocumentStatus.Signed
                            : EdoImportBatchDocumentStatus.Blocked;
                var document = new EdoImportBatchDocument(
                    batch.Id,
                    organizationId,
                    item.ProviderDocumentId,
                    item.Direction.ToUpperInvariant(),
                    status,
                    now,
                    planItem.DocumentType,
                    planItem.SentOverrideApplied);
                document.SetSnapshot(
                    planItem.EdoDocumentId,
                    planItem.DocumentNumber,
                    planItem.DocumentDate,
                    planItem.HasMarking,
                    planItem.MarkingCount,
                    planItem.MarkingVerificationState,
                    planItem.MarkingSourceType,
                    planItem.SentOverrideApplied,
                    now);
                if (planItem.Status == EdoImportBatchDocumentStatus.AlreadyImported)
                    document.SetResult(
                        EdoImportBatchDocumentStatus.AlreadyImported,
                        planItem.ExistingPurchaseId,
                        planItem.ExistingSaleId,
                        null,
                        now);
                batchDocuments.Add(document);
            }
            await batchStore.AddBatchDocumentsAsync(batchDocuments, ct);
            batch.SetStatus(EdoImportBatchStatus.Applying, now);
            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            var racedBatch = await batchStore.FindByIdempotencyKeyAsync(organizationId, idempotencyKey, ct);
            if (racedBatch is not null)
                return Result.Success(ToApplyResponse(
                    racedBatch,
                    await batchStore.GetBatchDocumentsAsync(organizationId, racedBatch.Id, ct)));
            logger.LogWarning("Unified EDO batch initialization failed with a controlled error.");
            return Result.Failure<EdoUnifiedImportApplyResponseDto>(EdoUnifiedImportErrors.BatchInitializationFailed(userContext.LanguageId));
        }

        foreach (var requestItem in request.Items.OrderBy(x => x.ProviderDocumentId, StringComparer.Ordinal))
            await ProcessDocumentAsync(
                organizationId,
                batch.Id,
                requestItem,
                currentPlanResult.Value,
                request.AllowUnmatchedMarkings,
                ct);

        await ReconcileBatchStatusAsync(organizationId, batch.Id, ct);
        var resultBatch = await batchStore.GetBatchAsync(organizationId, batch.Id, ct);
        var resultDocuments = await batchStore.GetBatchDocumentsAsync(organizationId, batch.Id, ct);
        return Result.Success(ToApplyResponse(resultBatch!, resultDocuments));
    }

    public async Task<Result<EdoUnifiedImportBatchDto>> GetBatchAsync(long batchId, CancellationToken ct = default)
    {
        var organizationId = CurrentOrganization();
        var batch = await batchStore.GetBatchAsync(organizationId, batchId, ct);
        return batch is null
            ? Result.Failure<EdoUnifiedImportBatchDto>(EdoUnifiedImportErrors.BatchNotFound(languageId: userContext.LanguageId))
            : Result.Success(ToBatchDto(batch, batch.Documents));
    }

    public async Task<Result<EdoUnifiedImportBatchDto>> RefreshStatusAsync(CancellationToken ct = default)
    {
        var organizationId = CurrentOrganization();
        var provider = await providerResolver.GetActiveProviderAsync(ct);
        if (provider.Code.ToString() != Edocs)
            return Result.Failure<EdoUnifiedImportBatchDto>(EdoUnifiedImportErrors.ActiveProviderRequired(userContext.LanguageId));

        var batch = await batchStore.GetLatestBatchAsync(organizationId, ct);
        if (batch is null)
            return Result.Failure<EdoUnifiedImportBatchDto>(EdoUnifiedImportErrors.BatchNotFound(latest: true, languageId: userContext.LanguageId));

        await unitOfWork.BeginAsync(ct);
        try
        {
            await batchStore.AcquireOrganizationLockAsync(organizationId, ct);
            foreach (var document in batch.Documents.Where(x => x.Status == EdoImportBatchDocumentStatus.WaitingForSignature))
            {
                var status = document.Direction == "INBOX"
                    ? await provider.GetInboxStatusAsync(document.DocumentType, document.ProviderDocumentId, ct)
                    : await provider.GetOutboxStatusAsync(document.DocumentType, document.ProviderDocumentId, ct);
                if (status.Code == EdoDocumentStatusCode.SIGNED)
                    document.SetResult(EdoImportBatchDocumentStatus.Signed, null, null, null, DateTime.UtcNow);
            }
            await ReconcileLoadedBatchStatusAsync(batch, DateTime.UtcNow, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            return Result.Failure<EdoUnifiedImportBatchDto>(EdoUnifiedImportErrors.StatusRefreshFailed(userContext.LanguageId));
        }

        return Result.Success(ToBatchDto(batch, batch.Documents));
    }

    private async Task ProcessDocumentAsync(
        int organizationId,
        long batchId,
        EdoUnifiedImportApplyItemDto request,
        EdoUnifiedImportPlanDto plan,
        bool allowUnmatchedMarkings,
        CancellationToken ct)
    {
        var document = await batchStore.FindBatchDocumentAsync(organizationId, batchId, request.ProviderDocumentId, ct);
        if (document is null || document.Status is EdoImportBatchDocumentStatus.AlreadyImported or EdoImportBatchDocumentStatus.WaitingForSignature)
            return;

        try
        {
            var planItem = plan.Items.FirstOrDefault(x => x.ProviderDocumentId == request.ProviderDocumentId);
            if (planItem is null ||
                !(planItem.ProviderStatus.Equals(Signed, StringComparison.OrdinalIgnoreCase)
                    || planItem.SentOverrideApplied)
                || !EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType(planItem.DocumentType))
            {
                await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Blocked, null, null, "EDO_UNIFIED_DOCUMENT_NOT_ELIGIBLE", ct);
                return;
            }

            var provider = await providerResolver.GetActiveProviderAsync(ct);
            var snapshot = await provider.GetDocumentDetailsAsync(
                request.Direction.Equals("INBOX", StringComparison.OrdinalIgnoreCase) ? EdoDirection.INBOX : EdoDirection.OUTBOX,
                planItem.DocumentType,
                request.ProviderDocumentId,
                ct);
            if (request.Direction.Equals("INBOX", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Lines.Any(x => x.ProductTableIds.Count > 0))
                {
                    await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Failed, null, null,
                        "EDO_UNIFIED_PURCHASE_PRODUCT_TABLE_MAPPING_UNSUPPORTED", ct);
                    return;
                }
                if (!HasMatchingProviderValues(request, snapshot))
                {
                    await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Failed, null, null,
                        "EDO_UNIFIED_PROVIDER_LINE_VALUES_INVALID", ct);
                    return;
                }
                var purchaseResult = await purchaseFactory.CreateFromHistoricalSnapshotAsync(
                    snapshot,
                    BuildPurchaseRequest(request, snapshot),
                    ct);
                if (purchaseResult.IsSuccess)
                    await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Imported, purchaseResult.Value.Id, null, null, ct);
                else
                    await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Failed, null, null, purchaseResult.Error.Code, ct);
            }
            else
            {
                var salePlan = await salePreflight.GetPlanAsync(
                    ct,
                    allowSentDocuments: planItem.SentOverrideApplied,
                    documentType: planItem.DocumentType);
                var saleResult = await saleApply.ApplyAsync(new EdoSaleDraftApplyRequestDto
                {
                    Confirm = true,
                    ExpectedPlanHash = salePlan.PlanHash,
                        Items = [BuildSaleRequest(request, planItem, allowUnmatchedMarkings)]
                }, ct);
                if (saleResult.IsSuccess)
                {
                    var result = saleResult.Value.Items.FirstOrDefault(x => x.ProviderDocumentId == request.ProviderDocumentId);
                    await UpdateDocumentAsync(document,
                        result?.Status == "ALREADY_IMPORTED" ? EdoImportBatchDocumentStatus.AlreadyImported : EdoImportBatchDocumentStatus.Imported,
                        null,
                        result?.SaleDocId,
                        result?.SafeErrorCodes.FirstOrDefault(),
                        ct);
                }
                else
                    await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Failed, null, null, saleResult.Error.Code, ct);
            }
        }
        catch
        {
            logger.LogWarning("Unified EDO document processing failed with a controlled error.");
            await UpdateDocumentAsync(document, EdoImportBatchDocumentStatus.Failed, null, null, "EDO_UNIFIED_DOCUMENT_FAILED", ct);
        }
    }

    private async Task UpdateDocumentAsync(
        EdoImportBatchDocument document,
        string status,
        long? purchaseId,
        long? saleId,
        string? safeErrorCode,
        CancellationToken ct)
    {
        await unitOfWork.BeginAsync(ct);
        try
        {
            document.SetResult(status, purchaseId, saleId, safeErrorCode, DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
        }
    }

    private async Task ReconcileBatchStatusAsync(int organizationId, long batchId, CancellationToken ct)
    {
        var batch = await batchStore.GetBatchAsync(organizationId, batchId, ct);
        if (batch is null) return;
        await unitOfWork.BeginAsync(ct);
        try
        {
            await batchStore.AcquireOrganizationLockAsync(organizationId, ct);
            await ReconcileLoadedBatchStatusAsync(batch, DateTime.UtcNow, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
        }
    }

    private static Task ReconcileLoadedBatchStatusAsync(EdoImportBatch batch, DateTime now, CancellationToken ct)
    {
        if (batch.Documents.Any(x => x.Status == EdoImportBatchDocumentStatus.Failed))
            batch.SetStatus(EdoImportBatchStatus.Failed, now);
        else if (batch.Documents.Any(x => x.Status == EdoImportBatchDocumentStatus.WaitingForSignature))
            batch.SetStatus(EdoImportBatchStatus.Partial, now);
        else if (batch.Documents.Any(x => x.Status is EdoImportBatchDocumentStatus.Blocked or EdoImportBatchDocumentStatus.Signed))
            batch.SetStatus(EdoImportBatchStatus.Partial, now);
        else
            batch.SetStatus(EdoImportBatchStatus.Completed, now);
        return Task.CompletedTask;
    }

    private async Task<IReadOnlyCollection<EdoDocumentDto>> ReadAllDocumentsAsync(CancellationToken ct)
    {
        var result = new List<EdoDocumentDto>();
        var page = 1;
        while (true)
        {
            var response = await inboxService.ListAllDocumentsAsync(new EdoAllDocumentsQueryDto
            {
                Page = page,
                PageSize = 100,
                Category = EdoDocumentCategory.ALL
            }, ct);
            result.AddRange(response.Items);
            if (response.HasNextPage != true || page >= response.TotalPages) break;
            page++;
            if (page > 10000) throw new InvalidOperationException("EDO pagination limit exceeded.");
        }
        return result;
    }

    private async Task<EdoUnifiedImportPlanItemDto> BuildPlanItemAsync(
        EdoDocumentDto document,
        IEdoProvider provider,
        int organizationId,
        bool allowSentDocuments,
        bool allowUnmatchedMarkings,
        EdoSalePreflightCandidateDto? saleCandidate,
        CancellationToken ct)
    {
        var providerStatus = document.Status.Code.ToString().ToUpperInvariant();
        var source = string.IsNullOrWhiteSpace(document.ProviderDocumentId)
            ? null
            : await documentStore.FindByProviderDocumentIdAsync(organizationId, EdoProviderCode.EDOCS, document.ProviderDocumentId, ct);
        long? existingPurchaseId = source is not null &&
            string.Equals(source.InternalDocumentType, "PURCHASE", StringComparison.OrdinalIgnoreCase)
            ? source.InternalDocumentId
            : null;
        long? existingSaleId = source is not null &&
            string.Equals(source.InternalDocumentType, "SALE", StringComparison.OrdinalIgnoreCase)
            ? source.InternalDocumentId
            : null;
        var normalizedType = EdoUnifiedImportPlanRules.NormalizeDocumentType(document.DocumentType);
        var isSupported = document.Direction == EdoDirection.OUTBOX
            ? EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType(normalizedType)
            : EdoUnifiedImportPlanRules.IsFactura(normalizedType);
        var sentOverrideApplied = providerStatus == Sent && allowSentDocuments && isSupported;
        var status = source is not null && (existingPurchaseId.HasValue || existingSaleId.HasValue)
            ? EdoImportBatchDocumentStatus.AlreadyImported
            : providerStatus == Sent && !sentOverrideApplied
                ? EdoImportBatchDocumentStatus.WaitingForSignature
                : providerStatus == Signed && isSupported || sentOverrideApplied
                    ? EdoImportBatchDocumentStatus.Blocked
                    : EdoImportBatchDocumentStatus.Blocked;
        IReadOnlyCollection<string> errors = source is not null && (existingPurchaseId.HasValue || existingSaleId.HasValue)
            ? Array.Empty<string>()
            : providerStatus == Sent && !sentOverrideApplied
                ? Array.Empty<string>()
                : providerStatus == Signed && isSupported || sentOverrideApplied
                    ? new[] { "EDO_UNIFIED_EXPLICIT_MAPPING_REQUIRED" }
                    : new[] { "EDO_UNIFIED_DOCUMENT_NOT_ELIGIBLE" };
        var mappingStatus = source is not null && (existingPurchaseId.HasValue || existingSaleId.HasValue)
            ? "ALREADY_IMPORTED"
            : providerStatus == Sent && !sentOverrideApplied
                ? "WAITING_FOR_SIGNATURE"
                : providerStatus == Signed && isSupported || sentOverrideApplied
                    ? "REQUIRES_SELECTION"
                    : "BLOCKED";
        var hasMarking = EdoUnifiedImportPlanRules.HasMarking(document);
        var markingCount = EdoUnifiedImportPlanRules.GetMarkingCount(document);
        var detailError = string.Empty;
        if (source is null && isSupported && !string.IsNullOrWhiteSpace(document.ProviderDocumentId))
        {
            try
            {
                var detail = await provider.GetDocumentDetailsAsync(
                    document.Direction,
                    normalizedType,
                    document.ProviderDocumentId,
                    ct);
                markingCount = EdoUnifiedImportPlanRules.GetMarkingCount(detail);
                hasMarking = EdoUnifiedImportPlanRules.HasMarking(detail);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                detailError = "EDO_UNIFIED_DETAIL_UNAVAILABLE";
            }
        }
        var saleMapping = document.Direction == EdoDirection.OUTBOX && saleCandidate is not null
            ? EdoUnifiedImportPlanMapping.FromSaleCandidate(saleCandidate)
            : null;
        if (saleMapping is not null && source is null)
        {
            saleMapping = await EnrichSaleMappingAsync(
                organizationId,
                document,
                saleCandidate!,
                saleMapping,
                allowSentDocuments,
                allowUnmatchedMarkings,
                ct);
        }
        if (saleMapping is not null && source is null)
        {
            var currentMappingErrors = EdoUnifiedImportPlanMapping
                .BuildCurrentSafeErrorCodes(saleCandidate!, saleMapping)
                .Concat(saleMapping.SafeErrorCodes)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            var mappingReady = currentMappingErrors.Length == 0
                && EdoUnifiedImportPlanMapping.IsReady(saleMapping);
            errors = currentMappingErrors;
            status = mappingReady
                ? EdoImportBatchDocumentStatus.Signed
                : providerStatus == Sent && !sentOverrideApplied
                    ? EdoImportBatchDocumentStatus.WaitingForSignature
                    : EdoImportBatchDocumentStatus.Blocked;
            mappingStatus = mappingReady
                ? "READY"
                : currentMappingErrors.Length == 0
                    ? "REQUIRES_SELECTION"
                    : "BLOCKED";
        }

        var markingStatus = !hasMarking
            ? "NOT_REQUIRED"
            : mappingStatus == "REQUIRES_SELECTION"
                ? "REQUIRES_SELECTION"
                : mappingStatus;

        return new EdoUnifiedImportPlanItemDto
        {
            ProviderDocumentId = document.ProviderDocumentId ?? string.Empty,
            Direction = document.Direction.ToString(),
            DocumentType = normalizedType,
            DocumentNumber = document.DocumentNumber,
            DocumentDate = document.DocumentDate,
            ProviderStatus = providerStatus,
            SentOverrideApplied = sentOverrideApplied,
            Status = status,
            EdoDocumentId = document.Id,
            ExistingPurchaseId = existingPurchaseId,
            ExistingSaleId = existingSaleId,
            HasMarking = hasMarking,
            MarkingCount = markingCount,
            MarkingVerificationState = document.VerificationState,
            MarkingSourceType = document.SourceType,
            CounterpartyMappingStatus = saleMapping?.CounterpartyMappingStatus ?? mappingStatus,
            ContractMappingStatus = saleMapping?.ContractMappingStatus ?? mappingStatus,
            CurrencyMappingStatus = saleMapping?.CurrencyMappingStatus ?? mappingStatus,
            ProductMappingStatus = saleMapping?.ProductMappingStatus ?? mappingStatus,
            WarehouseMappingStatus = saleMapping?.WarehouseMappingStatus ?? mappingStatus,
            VatMappingStatus = saleMapping?.VatMappingStatus ?? mappingStatus,
            CostPriceStatus = saleMapping?.CostPriceStatus ?? mappingStatus,
            ProductTableMappingStatus = saleMapping?.ProductTableMappingStatus ?? mappingStatus,
            MarkingMappingStatus = saleMapping?.MarkingMappingStatus ?? markingStatus,
            CounterpartyId = saleMapping?.CounterpartyId,
            ContractId = saleMapping?.ContractId,
            ContractCandidateIds = saleMapping?.ContractCandidateIds ?? [],
            CurrencyId = saleMapping?.CurrencyId,
            WarehouseId = saleMapping?.WarehouseId,
            Lines = saleMapping?.Lines ?? [],
            SafeErrorCodes = detailError.Length == 0
                ? errors
                : errors.Concat([detailError]).Distinct(StringComparer.Ordinal).ToArray()
        };
    }

    private async Task<IReadOnlyDictionary<string, EdoSalePreflightCandidateDto>>
        LoadSalePreflightCandidatesAsync(
            IReadOnlyCollection<EdoDocumentDto> documents,
            bool allowSentDocuments,
            CancellationToken ct)
    {
        var types = documents
            .Where(x => x.Direction == EdoDirection.OUTBOX)
            .Select(x => EdoUnifiedImportPlanRules.NormalizeDocumentType(x.DocumentType))
            .Where(EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var candidates = new Dictionary<string, EdoSalePreflightCandidateDto>(StringComparer.Ordinal);

        foreach (var type in types)
        {
            var plan = await salePreflight.GetPlanAsync(ct, allowSentDocuments, type);
            foreach (var candidate in plan.Candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate.ProviderDocumentId))
                    candidates.TryAdd(candidate.ProviderDocumentId.Trim(), candidate);
            }
        }

        return candidates;
    }

    private async Task<EdoUnifiedImportPlanMappingSnapshot> EnrichSaleMappingAsync(
        int organizationId,
        EdoDocumentDto document,
        EdoSalePreflightCandidateDto candidate,
        EdoUnifiedImportPlanMappingSnapshot mapping,
        bool allowSentDocuments,
        bool allowUnmatchedMarkings,
        CancellationToken ct)
    {
        if (!mapping.WarehouseId.HasValue || mapping.Lines.Count == 0)
            return mapping;

        var productIds = mapping.Lines
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToArray();
        if (productIds.Length == 0)
            return mapping;

        var inventoryResult = await warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter
            {
                WarehouseId = mapping.WarehouseId.Value,
                ProductIds = productIds
            },
            ct);
        if (!inventoryResult.IsSuccess)
            return AddMappingErrors(mapping, "SALE_PRODUCT_STOCK_MAPPING_REQUIRED");

        var inventoryByProductId = inventoryResult.Value
            .Where(x => productIds.Contains(x.ProductId))
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.First());
        var availableTables = await productTableQuery.GetAllAsync(
            queryBuilder.For<ProductTable>()
                .Where(x => productIds.Contains(x.ProductId)
                    && x.Product.OrganizationId == organizationId
                    && x.Product.StateId == StateIdConst.ACTIVE
                    && x.WarehouseProductTable != null
                    && x.WarehouseProductTable.WarehouseId == mapping.WarehouseId.Value
                    && x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK)
                .Build(),
            ct);
        var tablesByProductId = availableTables
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.OrderBy(table => table.Id).ToArray());

        var normalizedDocumentType = EdoUnifiedImportPlanRules.NormalizeDocumentType(document.DocumentType);
        var allowFacturaUnmatchedMarkings = EdoPartialMarkingRules
            .IsFacturaUnmatchedMarkingPolicyEnabled(normalizedDocumentType, allowUnmatchedMarkings);
        EdoOutboxProviderDocumentMappingSourceDto? markingSource = null;
        var markingSourceRequired = mapping.Lines.Any(x => x.MarkingRequired)
            || allowFacturaUnmatchedMarkings;
        if (markingSourceRequired)
        {
            try
            {
                markingSource = await inboxService.GetOutboxProviderDocumentMappingSourceAsync(
                    document.ProviderDocumentId ?? string.Empty,
                    ct,
                    EdoUnifiedImportPlanRules.NormalizeDocumentType(document.DocumentType),
                    allowSentDocuments);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return AddMappingErrors(mapping, "MARKING_SOURCE_REQUIRED");
            }
        }

        var errors = new HashSet<string>(StringComparer.Ordinal);
        var lines = mapping.Lines.Select(line =>
        {
            if (!line.ProductId.HasValue
                || !inventoryByProductId.TryGetValue(line.ProductId.Value, out var inventory))
            {
                errors.Add("SALE_PRODUCT_STOCK_MAPPING_REQUIRED");
                return line with
                {
                    ProductTableMappingStatus = "BLOCKED",
                    CostPriceStatus = "BLOCKED"
                };
            }

            if (!inventory.IsService
                && (!line.Quantity.HasValue || inventory.AvailableQuantity < line.Quantity.Value))
            {
                errors.Add("SALE_PRODUCT_STOCK_MAPPING_REQUIRED");
                return line with
                {
                    ProductTableMappingStatus = "BLOCKED",
                    CostPriceStatus = "BLOCKED"
                };
            }

            var updated = line;
            if (!inventory.IsService && inventory.IsPieceTracked)
            {
                if (!line.Quantity.HasValue
                    || line.Quantity <= 0m
                    || decimal.Truncate(line.Quantity.Value) != line.Quantity.Value)
                {
                    errors.Add("PRODUCT_TABLE_COUNT_INVALID");
                    updated = updated with { ProductTableMappingStatus = "BLOCKED" };
                }
                else
                {
                    var requiredCount = decimal.ToInt32(line.Quantity.Value);
                    var tables = tablesByProductId.GetValueOrDefault(line.ProductId.Value) ?? [];
                    var selectedTables = allowFacturaUnmatchedMarkings
                        ? ResolveFacturaTablesAllowingUnmatchedMarkings(
                            line.LineNumber,
                            requiredCount,
                            tables,
                            markingSource,
                            errors)
                        : line.MarkingRequired
                            ? ResolveMarkedTables(
                                line.LineNumber,
                                requiredCount,
                                tables,
                                markingSource,
                                errors)
                            : tables.Take(requiredCount).ToArray();

                    if (selectedTables.Length == requiredCount)
                    {
                        updated = updated with
                        {
                            ProductTableIds = selectedTables.Select(x => x.Id).ToArray(),
                            ProductTableMappingStatus = "READY"
                        };
                    }
                    else
                    {
                        errors.Add("PRODUCT_TABLE_SELECTION_REQUIRED");
                        updated = updated with { ProductTableMappingStatus = "BLOCKED" };
                    }
                }
            }
            else
            {
                updated = updated with
                {
                    ProductTableIds = [],
                    ProductTableMappingStatus = "NOT_APPLICABLE"
                };
            }

            var batch = inventory.Batches
                .Where(x => x.AvailableQuantity > 0m)
                .OrderBy(x => x.ReceivedDate)
                .ThenBy(x => x.BatchId)
                .FirstOrDefault(x => line.Quantity.HasValue && x.AvailableQuantity >= line.Quantity.Value);
            if (batch is not null)
            {
                updated = updated with
                {
                    CostPrice = batch.UnitCost,
                    CostPriceSource = "WAREHOUSE_STOCK_BATCH",
                    CostPriceStatus = "READY"
                };
            }
            else
            {
                errors.Add("SALE_COST_PRICE_SOURCE_REQUIRED");
                updated = updated with { CostPriceStatus = "REQUIRES_SELECTION" };
            }

            return updated;
        }).ToArray();

        var markingMappingStatus = !mapping.Lines.Any(line => line.MarkingRequired)
            ? "NOT_REQUIRED"
            : errors.Any(code => code.StartsWith("MARKING_", StringComparison.Ordinal))
                || lines.Any(line => line.MarkingRequired && line.ProductTableMappingStatus != "READY")
                ? "BLOCKED"
                : "READY";

        return new EdoUnifiedImportPlanMappingSnapshot
        {
            CounterpartyId = mapping.CounterpartyId,
            ContractId = mapping.ContractId,
            ContractCandidateIds = mapping.ContractCandidateIds,
            CurrencyId = mapping.CurrencyId,
            WarehouseId = mapping.WarehouseId,
            CounterpartyMappingStatus = mapping.CounterpartyMappingStatus,
            ContractMappingStatus = mapping.ContractMappingStatus,
            CurrencyMappingStatus = mapping.CurrencyMappingStatus,
            WarehouseMappingStatus = mapping.WarehouseMappingStatus,
            ProductMappingStatus = AggregateStatus(lines.Select(x => x.ProductMappingStatus)),
            VatMappingStatus = AggregateStatus(lines.Select(x => x.VatMappingStatus)),
            CostPriceStatus = AggregateStatus(lines.Select(x => x.CostPriceStatus)),
            ProductTableMappingStatus = AggregateStatus(lines.Select(x => x.ProductTableMappingStatus)),
            MarkingMappingStatus = markingMappingStatus,
            SafeErrorCodes = errors.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            Lines = lines
        };
    }

    private static ProductTable[] ResolveMarkedTables(
        int lineNumber,
        int requiredCount,
        IReadOnlyCollection<ProductTable> tables,
        EdoOutboxProviderDocumentMappingSourceDto? markingSource,
        ISet<string> errors)
    {
        if (markingSource is null)
        {
            errors.Add("MARKING_SOURCE_REQUIRED");
            return [];
        }

        var providerMarkings = markingSource.MarkingCodesByLine.GetValueOrDefault(lineNumber) ?? [];
        if (!EdoPartialMarkingRules.IsProviderMarkingSetValid(requiredCount, providerMarkings)
            || providerMarkings.Count == 0)
        {
            errors.Add("MARKING_COUNT_MISMATCH");
            return [];
        }

        if (!EdoPartialMarkingRules.TrySelectTables(
                requiredCount,
                tables,
                providerMarkings,
                out var selectedTables))
        {
            errors.Add("MARKING_MAPPING_REQUIRED");
            return [];
        }

        return selectedTables.ToArray();
    }

    private static ProductTable[] ResolveFacturaTablesAllowingUnmatchedMarkings(
        int lineNumber,
        int requiredCount,
        IReadOnlyCollection<ProductTable> tables,
        EdoOutboxProviderDocumentMappingSourceDto? markingSource,
        ISet<string> errors)
    {
        if (markingSource is null)
        {
            errors.Add("MARKING_SOURCE_REQUIRED");
            return [];
        }

        var providerMarkings = markingSource.MarkingCodesByLine.GetValueOrDefault(lineNumber) ?? [];
        if (!EdoPartialMarkingRules.IsProviderMarkingSetValid(requiredCount, providerMarkings))
        {
            errors.Add("MARKING_COUNT_MISMATCH");
            return [];
        }

        if (!EdoPartialMarkingRules.TrySelectFacturaTablesAllowingUnmatchedMarkings(
                requiredCount,
                tables,
                providerMarkings,
                out var selectedTables))
        {
            errors.Add("PRODUCT_TABLE_SELECTION_REQUIRED");
            return [];
        }

        return selectedTables.ToArray();
    }

    private static EdoUnifiedImportPlanMappingSnapshot AddMappingErrors(
        EdoUnifiedImportPlanMappingSnapshot mapping,
        params string[] errors) => new()
    {
        CounterpartyId = mapping.CounterpartyId,
        ContractId = mapping.ContractId,
        ContractCandidateIds = mapping.ContractCandidateIds,
        CurrencyId = mapping.CurrencyId,
        WarehouseId = mapping.WarehouseId,
        CounterpartyMappingStatus = mapping.CounterpartyMappingStatus,
        ContractMappingStatus = mapping.ContractMappingStatus,
        CurrencyMappingStatus = mapping.CurrencyMappingStatus,
        WarehouseMappingStatus = mapping.WarehouseMappingStatus,
        ProductMappingStatus = mapping.ProductMappingStatus,
        VatMappingStatus = mapping.VatMappingStatus,
        CostPriceStatus = mapping.CostPriceStatus,
        ProductTableMappingStatus = mapping.ProductTableMappingStatus,
        MarkingMappingStatus = errors.Any(code => code.StartsWith("MARKING_", StringComparison.Ordinal))
            ? "BLOCKED"
            : mapping.MarkingMappingStatus,
        SafeErrorCodes = errors.Distinct(StringComparer.Ordinal).ToArray(),
        Lines = mapping.Lines
    };

    private static string AggregateStatus(IEnumerable<string> statuses)
    {
        var values = statuses.ToArray();
        if (values.Length > 0 && values.All(x => x == "READY" || x == "NOT_APPLICABLE"))
            return "READY";
        if (values.Any(x => x == "BLOCKED"))
            return "BLOCKED";
        return "REQUIRES_SELECTION";
    }

    private static PurchaseDocFromEdoRequestDto BuildPurchaseRequest(EdoUnifiedImportApplyItemDto request, EdoDocumentDto snapshot) => new()
    {
        DocumentIdentity = request.ProviderDocumentId,
        CounterpartyId = request.CounterpartyId,
        ContractId = request.ContractId,
        WarehouseId = request.WarehouseId,
        CurrencyId = request.CurrencyId,
        Comment = request.Comment,
        Lines = request.Lines.Select(line => new PurchaseDocFromEdoLineDto
        {
            LineNumber = line.LineNumber,
            ProductId = line.ProductId,
            UnitId = line.UnitId,
            VatRateId = line.VatRateId,
            Items = snapshot.PreviewLines.FirstOrDefault(x => x.Number == line.LineNumber)?.MarkingCodes
                .Select(marking => new PurchaseDocLineItemDto { MarkingNumber = marking })
                .ToList() ?? []
        }).ToList()
    };

    private static bool HasMatchingProviderValues(EdoUnifiedImportApplyItemDto request, EdoDocumentDto snapshot)
    {
        foreach (var line in request.Lines)
        {
            if (line.Quantity is null || line.UnitPrice is null)
                return false;
            var providerLine = snapshot.PreviewLines.FirstOrDefault(x => x.Number == line.LineNumber);
            if (providerLine is null ||
                providerLine.Quantity is null || providerLine.UnitPrice is null ||
                Math.Abs(providerLine.Quantity.Value - line.Quantity.Value) > 0.000001m ||
                Math.Abs(providerLine.UnitPrice.Value - line.UnitPrice.Value) > 0.000001m)
                return false;
        }
        return request.Lines.Count > 0;
    }

    private static EdoSaleDraftApplyItemDto BuildSaleRequest(
        EdoUnifiedImportApplyItemDto request,
        EdoUnifiedImportPlanItemDto planItem,
        bool allowUnmatchedMarkings) => new()
    {
        ProviderDocumentId = request.ProviderDocumentId,
        DocumentType = planItem.DocumentType,
        AllowSentDocuments = planItem.SentOverrideApplied,
        AllowUnmatchedMarkings = EdoPartialMarkingRules
            .IsFacturaUnmatchedMarkingPolicyEnabled(planItem.DocumentType, allowUnmatchedMarkings),
        CounterpartyId = request.CounterpartyId,
        ContractId = request.ContractId ?? 0,
        CurrencyId = request.CurrencyId,
        WarehouseId = request.WarehouseId,
        ExchangeRate = request.ExchangeRate,
        Comment = request.Comment,
        Lines = request.Lines.Select(line => new EdoSaleDraftApplyLineDto
        {
            Number = line.LineNumber,
            ProductId = line.ProductId,
            Quantity = line.Quantity ?? 0m,
            UnitPrice = line.UnitPrice ?? 0m,
            UnitId = line.UnitId,
            VatRateId = line.VatRateId,
            CostPrice = line.CostPrice ?? 0m,
            CostPriceSource = line.CostPriceSource ?? string.Empty,
            MarkingSource = line.MarkingSource,
            Items = line.ProductTableIds.Select(id => new SaleDocCreateProductTableDto { ProductTableId = id }).ToArray()
        }).ToArray()
    };

    private static EdoUnifiedImportBatchDto ToBatchDto(EdoImportBatch batch, IEnumerable<EdoImportBatchDocument> documents)
    {
        var dtoDocuments = documents.Select(ToDocumentDto).ToArray();
        return new EdoUnifiedImportBatchDto
        {
            BatchId = batch.Id,
            ProviderCode = batch.ProviderCode,
            PlanHash = batch.PlanHash,
            Status = batch.Status,
            ImportedCount = dtoDocuments.Count(x => x.Status == EdoImportBatchDocumentStatus.Imported),
            WaitingForSignatureCount = dtoDocuments.Count(x => x.Status == EdoImportBatchDocumentStatus.WaitingForSignature),
            BlockedCount = dtoDocuments.Count(x => x.Status == EdoImportBatchDocumentStatus.Blocked),
            FailedCount = dtoDocuments.Count(x => x.Status == EdoImportBatchDocumentStatus.Failed),
            AlreadyImportedCount = dtoDocuments.Count(x => x.Status == EdoImportBatchDocumentStatus.AlreadyImported),
            Documents = dtoDocuments
        };
    }

    private static EdoUnifiedImportBatchDocumentDto ToDocumentDto(EdoImportBatchDocument x) => new()
    {
            BatchDocumentId = x.Id,
            ProviderDocumentId = x.ProviderDocumentId,
            Direction = x.Direction,
            DocumentType = x.DocumentType,
            Status = x.Status,
            ProviderStatus = x.SentOverrideApplied || x.Status == EdoImportBatchDocumentStatus.WaitingForSignature
                ? Sent
                : x.Status == EdoImportBatchDocumentStatus.Signed ? Signed : string.Empty,
        DocumentNumber = x.DocumentNumber,
        DocumentDate = x.DocumentDate,
        PurchaseDocumentId = x.PurchaseDocumentId,
        SaleDocumentId = x.SaleDocumentId,
        SafeErrorCode = x.SafeErrorCode,
        HasMarking = x.HasMarking,
        MarkingCount = x.MarkingCount,
        MarkingVerificationState = x.MarkingVerificationState,
            MarkingSourceType = x.MarkingSourceType,
            SentOverrideApplied = x.SentOverrideApplied
        };

    private static EdoUnifiedImportApplyResponseDto ToApplyResponse(EdoImportBatch batch, IEnumerable<EdoImportBatchDocument> documents)
    {
        var dto = ToBatchDto(batch, documents);
        return new EdoUnifiedImportApplyResponseDto
        {
            BatchId = dto.BatchId,
            PlanHash = dto.PlanHash,
            ImportedCount = dto.ImportedCount,
            WaitingForSignatureCount = dto.WaitingForSignatureCount,
            BlockedCount = dto.BlockedCount,
            FailedCount = dto.FailedCount,
            AlreadyImportedCount = dto.AlreadyImportedCount,
            Documents = dto.Documents
        };
    }

    private static string ComputePlanHash(EdoUnifiedImportPlanDto plan)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            plan.AllowUnmatchedMarkings,
            Items = plan.Items.OrderBy(x => x.ProviderDocumentId, StringComparer.Ordinal)
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private int CurrentOrganization() => userContext.OrganizationId is { } id && id > 0
        ? id
        : throw new InvalidOperationException("The current user has no organization scope.");

    private static bool IsSha256(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);
}
