using Domain.Entities;

namespace Application.Features.PurchaseDocs;

public sealed class EdoImportPreflightRequestDto
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
}

public sealed class EdoImportCandidateListFilter
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class EdoImportCandidateMappingRequestDto
{
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public List<EdoImportCandidateLineMappingRequestDto> Lines { get; init; } = [];
}

public sealed class EdoImportCandidateLineMappingRequestDto
{
    public int LineNumber { get; init; }
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public int? DebitAccountId { get; init; }
    public int? VatAccountId { get; init; }
}

public sealed class EdoImportJobDto
{
    public long Id { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public string Status { get; init; } = string.Empty;
    public int DiscoveredCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int DuplicateCount { get; init; }
    public int SkippedCount { get; init; }
    public string? SafeErrorCode { get; init; }
    public IReadOnlyCollection<EdoImportProviderDto> Providers { get; init; } = [];
}

public sealed class EdoImportProviderDto
{
    public string ProviderCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int? ProviderTotal { get; init; }
    public int ScannedCount { get; init; }
    public bool IsWaitingAuth { get; init; }
    public string? SafeErrorCode { get; init; }
}

public sealed class EdoImportMappingSummaryDto
{
    public long JobId { get; init; }
    public int TotalCandidates { get; init; }
    public int ReadyCount { get; init; }
    public int DuplicateCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public IReadOnlyDictionary<string, int> SafeErrorCodeCounts { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyCollection<EdoImportMissingSellerSummaryDto> MissingSellers { get; init; } = [];
    public IReadOnlyCollection<EdoImportMissingContractSummaryDto> MissingContracts { get; init; } = [];
    public IReadOnlyCollection<EdoImportMissingProductSummaryDto> MissingProducts { get; init; } = [];
    public EdoImportMappingIssueCountsDto IssueCounts { get; init; } = new();
    public IReadOnlyCollection<EdoImportMissingMasterDataDto> MissingMasterData { get; init; } = [];
}

public sealed class EdoImportMissingSellerSummaryDto
{
    public string? SellerTin { get; init; }
    public string? SellerName { get; init; }
    public int CandidateCount { get; init; }
}

public sealed class EdoImportMissingContractSummaryDto
{
    public string? SellerTin { get; init; }
    public int CounterpartyId { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public int CandidateCount { get; init; }
}

public sealed class EdoImportMissingProductSummaryDto
{
    public string? CatalogCode { get; init; }
    public string? ProviderProductName { get; init; }
    public string ItemType { get; init; } = string.Empty;
    public bool? IsService { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public decimal? VatRate { get; init; }
    public int CandidateCount { get; init; }
}

public sealed class EdoImportMappingIssueCountsDto
{
    public int Counterparty { get; init; }
    public int Contract { get; init; }
    public int Product { get; init; }
    public int Currency { get; init; }
    public int Warehouse { get; init; }
    public int Unit { get; init; }
    public int VatRate { get; init; }
    public int Marking { get; init; }
}

public sealed class EdoImportMissingMasterDataDto
{
    public string Type { get; init; } = string.Empty;
    public int CandidateCount { get; init; }
}

public sealed class EdoImportMasterDataPlanDto
{
    public long JobId { get; init; }
    public string PlanHash { get; set; } = string.Empty;
    public IReadOnlyCollection<EdoImportCounterpartyPlanItemDto> Counterparties { get; init; } = [];
    public IReadOnlyCollection<EdoImportContractPlanItemDto> Contracts { get; init; } = [];
    public IReadOnlyCollection<EdoImportProductPlanItemDto> Products { get; init; } = [];
    public int MarkingRequiredCount { get; init; }
    public IReadOnlyCollection<string> BlockedReasonCodes { get; init; } = [];
    public int CreateCount { get; init; }
    public int UseExistingCount { get; init; }
    public int ConflictCount { get; init; }
    public int BlockedCount { get; init; }
}

public sealed class EdoImportMasterDataApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportCounterpartyApplyItemDto> Counterparties { get; init; } = [];
    public IReadOnlyCollection<EdoImportContractApplyItemDto> Contracts { get; init; } = [];
    public IReadOnlyCollection<EdoImportProductApplyItemDto> Products { get; init; } = [];
}

public sealed class EdoImportCounterpartyApplyItemDto
{
    public string SellerTin { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int? ExistingCounterpartyId { get; init; }
}

public sealed class EdoImportContractApplyItemDto
{
    public string SellerTin { get; init; } = string.Empty;
    public string ProviderContractNumber { get; init; } = string.Empty;
    public DateOnly ProviderContractDate { get; init; }
    public IReadOnlyCollection<long> CandidateIds { get; init; } = [];
    public string Action { get; init; } = string.Empty;
    public long? ExistingContractId { get; init; }
}

public sealed class EdoImportProductApplyItemDto
{
    public string CatalogCode { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int? ExistingProductId { get; init; }
    public bool? IsService { get; init; }
    public bool? IsPieceTracked { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
}

public sealed class EdoImportMasterDataApplyResponseDto
{
    public long JobId { get; init; }
    public int CreatedCounterpartyCount { get; init; }
    public int ReusedCounterpartyCount { get; init; }
    public int CreatedContractCount { get; init; }
    public int ReusedContractCount { get; init; }
    public int CreatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int ConflictCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
}

public sealed class EdoImportProductDefaultsApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportPackageUnitMappingDto> PackageUnitMappings { get; init; } = [];
    public string MarkingPolicy { get; init; } = string.Empty;
}

public sealed class EdoImportPackageUnitMappingDto
{
    public string PackageName { get; init; } = string.Empty;
    public short UnitId { get; init; }
}

public sealed class EdoImportProductConflictPlanDto
{
    public long JobId { get; init; }
    public string PlanHash { get; set; } = string.Empty;
    public IReadOnlyCollection<EdoImportProductConflictItemDto> Items { get; init; } = [];
}

public sealed class EdoImportProductConflictItemDto
{
    public string? IdentityKey { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string? CatalogCode { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public string? ProviderProductName { get; init; }
    public string ItemType { get; init; } = string.Empty;
    public bool? IsService { get; init; }
    public decimal? VatRate { get; init; }
    public short? ResolvedVatRateId { get; init; }
    public int CandidateCount { get; init; }
    public bool MarkingRequired { get; init; }
    public int MarkedCandidateCount { get; init; }
    public IReadOnlyCollection<string> BlockedReasonCodes { get; init; } = [];
    public IReadOnlyCollection<EdoImportCompatibleProductDto> CompatibleProducts { get; init; } = [];
}

public sealed class EdoImportCompatibleProductDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public bool IsPieceTracked { get; init; }
    public short UnitId { get; init; }
    public short? VatRateId { get; init; }
    public IReadOnlyCollection<string> CompatibilityCodes { get; init; } = [];
}

public sealed class EdoImportProductConflictApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportProductConflictApplyItemDto> Items { get; init; } = [];
}

public sealed class EdoImportProductConflictApplyItemDto
{
    public IReadOnlyCollection<string> IdentityKeys { get; init; } = [];
    public string Action { get; init; } = string.Empty;
    public int? ExistingProductId { get; init; }
    public bool? IsService { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public bool? IsPieceTracked { get; init; }
    public bool ConfirmItemTypeOverride { get; init; }
}

public sealed class EdoImportProductConflictApplyResponseDto
{
    public long JobId { get; init; }
    public int CreatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
    public int CreatedMappingCount { get; init; }
    public int ReusedMappingCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
}

public sealed class EdoImportMarkingConflictPlanDto
{
    public long JobId { get; init; }
    public string ConflictHash { get; set; } = string.Empty;
    public IReadOnlyCollection<EdoImportMarkingConflictItemDto> Items { get; init; } = [];
}

public sealed class EdoImportMarkingConflictItemDto
{
    public long CandidateId { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string SafeErrorCode { get; init; } = string.Empty;
    public decimal? ExpectedQuantity { get; init; }
    public int ActualMarkingCount { get; init; }
    public int ConflictCount { get; init; }
    public IReadOnlyCollection<long> ExistingPurchaseIds { get; init; } = [];
}

public sealed class EdoImportMarkingConflictApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedConflictHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportMarkingConflictApplyItemDto> Items { get; init; } = [];
}

public sealed class EdoImportMarkingConflictApplyItemDto
{
    public long CandidateId { get; init; }
    public string Action { get; init; } = string.Empty;
}

public sealed class EdoImportMarkingConflictApplyResponseDto
{
    public long JobId { get; init; }
    public int SkippedCandidateCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int DuplicateCount { get; init; }
    public int SkippedCount { get; init; }
}

public sealed class EdoImportDraftPlanDto
{
    public long JobId { get; init; }
    public string JobStatus { get; init; } = string.Empty;
    public string ImportPlanHash { get; set; } = string.Empty;
    public int TotalCandidates { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int DuplicateCount { get; init; }
    public int ImportedCount { get; init; }
    public int FailedCount { get; init; }
    public int SkippedCount { get; init; }
    public decimal ReadyNetAmount { get; init; }
    public decimal ReadyVatAmount { get; init; }
    public decimal ReadyTotalAmount { get; init; }
    public int MarkedCandidateCount { get; init; }
    public DateOnly? EarliestDocumentDate { get; init; }
    public DateOnly? LatestDocumentDate { get; init; }
    public IReadOnlyCollection<EdoImportDraftProviderSummaryDto> Providers { get; init; } = [];
}

public sealed class EdoImportDraftProviderSummaryDto
{
    public string ProviderCode { get; init; } = string.Empty;
    public int ReadyCount { get; init; }
}

public sealed class EdoImportDraftBatchRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedImportPlanHash { get; init; } = string.Empty;
    public int BatchSize { get; init; }
}

public sealed class EdoImportDraftBatchResponseDto
{
    public long JobId { get; init; }
    public string JobStatus { get; init; } = string.Empty;
    public int ProcessedCandidateCount { get; init; }
    public int CreatedDraftCount { get; init; }
    public int ReusedDraftCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RemainingReadyCount { get; init; }
    public int ImportedCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyCollection<EdoImportDraftFailureSummaryDto> Failures { get; init; } = [];
}

public sealed class EdoImportBulkDraftStartRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedImportPlanHash { get; init; } = string.Empty;
    public int BatchSize { get; init; }
    public string LineValuesInvalidPolicy { get; init; } = string.Empty;
    public string MarkingAlreadyUsedPolicy { get; init; } = string.Empty;
}

public sealed class EdoImportBulkDraftStatusDto
{
    public long JobId { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ProcessedCount { get; init; }
    public int CreatedDraftCount { get; init; }
    public int ReusedDraftCount { get; init; }
    public int DuplicateCount { get; init; }
    public int SkippedCount { get; init; }
    public int FailedCount { get; init; }
    public int RemainingReadyCount { get; init; }
    public string? LastSafeErrorCode { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class EdoImportDraftRequeueRequestDto
{
    public bool Confirm { get; init; }
}

public sealed class EdoImportDraftRequeueResponseDto
{
    public long JobId { get; init; }
    public long CandidateId { get; init; }
    public string CandidateStatus { get; init; } = string.Empty;
    public bool Requeued { get; init; }
    public int ReadyCount { get; init; }
    public int FailedCount { get; init; }
}

public sealed class EdoImportDraftFailureListDto
{
    public long JobId { get; init; }
    public string FailureHash { get; set; } = string.Empty;
    public IReadOnlyCollection<EdoImportDraftFailureItemDto> Items { get; init; } = [];
}

public sealed class EdoImportDraftFailureItemDto
{
    public long CandidateId { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string SafeErrorCode { get; init; } = string.Empty;
    public int TotalMarkingCount { get; init; }
    public int UsedMarkingCount { get; init; }
    public IReadOnlyCollection<long> ExistingPurchaseIds { get; init; } = [];
}

public sealed class EdoImportDraftFailureApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedFailureHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportDraftFailureApplyItemDto> Items { get; init; } = [];
}

public sealed class EdoImportDraftFailureApplyItemDto
{
    public long CandidateId { get; init; }
    public string Action { get; init; } = string.Empty;
}

public sealed class EdoImportDraftFailureApplyResponseDto
{
    public long JobId { get; init; }
    public int SkippedCandidateCount { get; init; }
    public int MarkedDuplicateCandidateCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int DuplicateCount { get; init; }
    public int ImportedCount { get; init; }
    public int FailedCount { get; init; }
    public int SkippedCount { get; init; }
}

public sealed class EdoImportPieceTrackingPlanDto
{
    public long JobId { get; init; }
    public string PlanHash { get; set; } = string.Empty;
    public IReadOnlyCollection<EdoImportPieceTrackingPlanItemDto> Products { get; init; } = [];
}

public sealed class EdoImportPieceTrackingPlanItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string CatalogCode { get; init; } = string.Empty;
    public int AffectedCandidateCount { get; init; }
    public int MarkingCount { get; init; }
    public bool IsService { get; init; }
    public bool IsPieceTracked { get; init; }
    public string SafeAction { get; init; } = string.Empty;
}

public sealed class EdoImportPieceTrackingApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<int> ProductIds { get; init; } = [];
}

public sealed class EdoImportPieceTrackingApplyResponseDto
{
    public long JobId { get; init; }
    public int UpdatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
    public int ReadyCount { get; init; }
    public int MappingRequiredCount { get; init; }
    public int FailedCount { get; init; }
}

public sealed class EdoImportDraftFailureSummaryDto
{
    public string SafeErrorCode { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class EdoImportCounterpartyPlanItemDto
{
    public string? SellerTin { get; init; }
    public string? CanonicalSellerName { get; init; }
    public IReadOnlyCollection<string> NameAliases { get; init; } = [];
    public int CandidateCount { get; init; }
    public int? ExistingCounterpartyId { get; init; }
    public string Action { get; init; } = string.Empty;
}

public sealed class EdoImportContractPlanItemDto
{
    public string ProviderCode { get; init; } = string.Empty;
    public string? SellerTin { get; init; }
    public int? CounterpartyId { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public int CandidateCount { get; init; }
    public IReadOnlyCollection<long> CandidateIds { get; init; } = [];
    public long? ExistingContractId { get; init; }
    public IReadOnlyCollection<long> ReconciliationContractIds { get; init; } = [];
    public string Action { get; init; } = string.Empty;
}

public sealed class EdoImportProductPlanItemDto
{
    public string? CatalogCode { get; init; }
    public string? ProviderProductName { get; init; }
    public bool? IsService { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public short? ResolvedUnitId { get; init; }
    public short? ResolvedVatRateId { get; init; }
    public bool MarkingRequired { get; init; }
    public int CandidateCount { get; init; }
    public int? ExistingProductId { get; init; }
    public string Action { get; init; } = string.Empty;
    public IReadOnlyCollection<string> BlockedReasonCodes { get; init; } = [];
}

public sealed class EdoImportCandidateListDto
{
    public long Id { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string? SellerTin { get; init; }
    public string? SellerName { get; init; }
    public decimal? TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string MappingStatus { get; init; } = string.Empty;
    public string DuplicateState { get; init; } = string.Empty;
    public long? ExistingPurchaseId { get; init; }
    public string? SafeErrorCode { get; init; }
}

public sealed class EdoImportCandidateDetailDto
{
    public long Id { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string? SellerTin { get; init; }
    public string? BuyerTin { get; init; }
    public string? SellerName { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string MappingStatus { get; init; } = string.Empty;
    public string DuplicateState { get; init; } = string.Empty;
    public long? ExistingPurchaseId { get; init; }
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public string? SafeErrorCode { get; init; }
    public IReadOnlyCollection<EdoImportCandidateLineDto> Lines { get; init; } = [];
}

public sealed class EdoImportCandidateLineDto
{
    public int Number { get; init; }
    public string? CatalogCode { get; init; }
    public string? ProviderProductName { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public bool? IsService { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalAmount { get; init; }
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public int? DebitAccountId { get; init; }
    public int? VatAccountId { get; init; }
    public string MappingStatus { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoImportCandidateMarkingDto> Markings { get; init; } = [];
}

public sealed class EdoImportCandidateMarkingDto
{
    public string MarkingNumber { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string VerificationState { get; init; } = string.Empty;
}

internal static class EdoImportPreflightMappings
{
    public static EdoImportJobDto ToDto(this EdoImportJob job) => new()
    {
        Id = job.Id,
        DateFrom = job.DateFrom,
        DateTo = job.DateTo,
        Status = job.Status,
        DiscoveredCount = job.DiscoveredCount,
        ReadyCount = job.ReadyCount,
        MappingRequiredCount = job.MappingRequiredCount,
        DuplicateCount = job.DuplicateCount,
        SkippedCount = job.SkippedCount,
        SafeErrorCode = job.SafeErrorCode,
        Providers = job.Providers.OrderBy(x => x.ProviderCode).Select(x => new EdoImportProviderDto
        {
            ProviderCode = x.ProviderCode,
            Status = x.Status,
            CurrentPage = x.CurrentPage,
            PageSize = x.PageSize,
            ProviderTotal = x.ProviderTotal,
            ScannedCount = x.ScannedCount,
            IsWaitingAuth = x.IsWaitingAuth,
            SafeErrorCode = x.SafeErrorCode
        }).ToArray()
    };

    public static EdoImportCandidateListDto ToListDto(this EdoImportCandidate candidate) => new()
    {
        Id = candidate.Id,
        ProviderCode = candidate.ProviderCode,
        ProviderDocumentId = candidate.ProviderDocumentId,
        DocumentNumber = candidate.DocumentNumber,
        DocumentDate = candidate.DocumentDate,
        SellerTin = candidate.SellerTin,
        SellerName = candidate.SellerName,
        TotalAmount = candidate.TotalAmount,
        Status = candidate.Status,
        MappingStatus = candidate.MappingStatus,
        DuplicateState = candidate.DuplicateState,
        ExistingPurchaseId = candidate.ExistingPurchaseId,
        SafeErrorCode = candidate.SafeErrorCode
    };

    public static EdoImportCandidateDetailDto ToDetailDto(this EdoImportCandidate candidate) => new()
    {
        Id = candidate.Id,
        ProviderCode = candidate.ProviderCode,
        ProviderDocumentId = candidate.ProviderDocumentId,
        DocumentNumber = candidate.DocumentNumber,
        DocumentDate = candidate.DocumentDate,
        SellerTin = candidate.SellerTin,
        BuyerTin = candidate.BuyerTin,
        SellerName = candidate.SellerName,
        ProviderContractNumber = candidate.ProviderContractNumber,
        ProviderContractDate = candidate.ProviderContractDate,
        NetAmount = candidate.NetAmount,
        VatAmount = candidate.VatAmount,
        TotalAmount = candidate.TotalAmount,
        Status = candidate.Status,
        MappingStatus = candidate.MappingStatus,
        DuplicateState = candidate.DuplicateState,
        ExistingPurchaseId = candidate.ExistingPurchaseId,
        CounterpartyId = candidate.SelectedCounterpartyId,
        ContractId = candidate.SelectedContractId,
        CurrencyId = candidate.SelectedCurrencyId,
        WarehouseId = candidate.SelectedWarehouseId,
        SafeErrorCode = candidate.SafeErrorCode,
        Lines = candidate.Lines.OrderBy(x => x.ProviderLineNumber).Select(x => new EdoImportCandidateLineDto
        {
            Number = x.ProviderLineNumber,
            CatalogCode = x.CatalogCode,
            ProviderProductName = x.ProviderProductName,
            PackageCode = x.PackageCode,
            PackageName = x.PackageName,
            IsService = x.IsService,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            NetAmount = x.NetAmount,
            VatRate = x.VatRate,
            VatAmount = x.VatAmount,
            TotalAmount = x.TotalAmount,
            ProductId = x.SelectedProductId,
            UnitId = x.SelectedUnitId,
            VatRateId = x.SelectedVatRateId,
            DebitAccountId = x.SelectedDebitAccountId,
            VatAccountId = x.SelectedVatAccountId,
            MappingStatus = x.MappingStatus,
            Markings = x.Markings.Select(marking => new EdoImportCandidateMarkingDto
            {
                MarkingNumber = marking.MarkingNumber,
                SerialNumber = marking.SerialNumber,
                VerificationState = marking.ProviderVerificationState
            }).ToArray()
        }).ToArray()
    };
}
