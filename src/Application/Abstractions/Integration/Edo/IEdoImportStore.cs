using Domain.Entities;
using SharedKernel.QueryResults;

namespace Application.Abstractions.Integration.Edo;

public interface IEdoImportStore
{
    Task<EdoImportJob?> GetJobAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportJob?> GetJobForProcessingAsync(
        long jobId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<long>> GetRunnableBulkImportJobIdsAsync(
        CancellationToken ct = default);

    Task<EdoImportJob?> FindActiveJobAsync(
        int organizationId,
        CancellationToken ct = default);

    Task<EdoImportJobProvider?> GetProviderAsync(
        int organizationId,
        long jobId,
        string providerCode,
        CancellationToken ct = default);

    Task<EdoImportCandidate?> GetCandidateAsync(
        int organizationId,
        long candidateId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportCandidate>> GetMappingRequiredCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<PagedList<EdoImportCandidate>> GetCandidatesAsync(
        int organizationId,
        long jobId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportMappingSummaryCandidateSourceDto>> GetMappingSummaryCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportMasterDataPlanSourceDto> GetMasterDataPlanSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportProductConflictSourceDto> GetProductConflictSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportMarkingConflictSourceDto>> GetMarkingConflictSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportPieceTrackingApplyStoreResultDto> ApplyPieceTrackingAsync(
        int organizationId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportCandidate>> GetPieceTrackingCandidatesAsync(
        int organizationId,
        long jobId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportCandidate>> GetReadyImportCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoImportDraftFailureSourceDto>> GetDraftImportFailuresAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportJob?> GetJobForImportAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task<EdoImportCandidate?> GetCandidateForImportAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct = default);

    Task AcquireDraftImportLocksAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default);

    Task<long?> FindExistingPurchaseForProviderDocumentAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<bool> AnyUsedMarkingsAsync(
        int organizationId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default);

    Task<EdoImportDraftMarkingUsageDto> GetDraftMarkingUsageAsync(
        int organizationId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default);

    void ClearTracking();

    Task AcquireMasterDataApplyLockAsync(int organizationId, CancellationToken ct = default);

    Task<EdoImportMasterDataApplyStoreResultDto> ApplyMasterDataAsync(
        int organizationId,
        EdoImportMasterDataApplyCommandDto command,
        DateTime now,
        CancellationToken ct = default);

    Task<EdoImportProductConflictApplyStoreResultDto> ApplyProductConflictMappingsAsync(
        int organizationId,
        EdoImportProductConflictApplyCommandDto command,
        DateTime now,
        CancellationToken ct = default);

    Task<bool> IsContractApplicableAsync(
        int organizationId,
        int counterpartyId,
        long contractId,
        DateOnly documentDate,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<long>> GetRunnableJobIdsAsync(
        DateTime now,
        CancellationToken ct = default);

    Task<bool> IsCancellationRequestedAsync(
        long jobId,
        CancellationToken ct = default);

    Task<bool> TryAcquireLeaseAsync(
        int organizationId,
        long jobId,
        string leaseOwner,
        DateTime now,
        DateTime leaseExpiresAt,
        CancellationToken ct = default);

    Task<EdoImportCandidate?> FindPriorProviderCandidateAsync(
        int organizationId,
        long currentJobId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoImportCandidate?> FindJobCandidateAsync(
        int organizationId,
        long jobId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoImportCandidate?> FindCrossProviderCandidateAsync(
        int organizationId,
        string providerCode,
        string fingerprint,
        bool contentFingerprint,
        CancellationToken ct = default);

    Task<EdoDocument?> FindEdoDocumentAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<long?> FindLinkedPurchaseIdAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoImportMappingResolutionDto> ResolveMappingAsync(
        int organizationId,
        string providerCode,
        string sellerTin,
        DateOnly documentDate,
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines,
        CancellationToken ct = default);

    Task<EdoImportMappingResolutionDto> ResolveSelectedMappingAsync(
        int organizationId,
        string sellerTin,
        DateOnly documentDate,
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines,
        EdoImportMappingSelectionDto selection,
        CancellationToken ct = default);

    Task<EdoImportJobMappingCountsDto> GetJobMappingCountsAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default);

    Task AddCandidateGraphAsync(
        EdoImportCandidate candidate,
        CancellationToken ct = default);

    Task AddJobAsync(EdoImportJob job, CancellationToken ct = default);

    Task AddProviderAsync(
        int organizationId,
        EdoImportJobProvider provider,
        CancellationToken ct = default);

    Task AddCandidateAsync(EdoImportCandidate candidate, CancellationToken ct = default);

    Task AddLineAsync(
        int organizationId,
        EdoImportCandidateLine line,
        CancellationToken ct = default);

    Task AddMarkingAsync(
        int organizationId,
        EdoImportCandidateMarking marking,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

public sealed class EdoImportMappingResolutionDto
{
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public IReadOnlyDictionary<int, EdoImportLineMappingResolutionDto> Lines { get; init; } =
        new Dictionary<int, EdoImportLineMappingResolutionDto>();
    public IReadOnlySet<string> PreviouslyUsedMarkings { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    public long? ExistingPurchaseIdForAllMarkings { get; init; }
    public bool HasConflictingMarkingUsage { get; init; }
}

public sealed class EdoImportLineMappingResolutionDto
{
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public int? DebitAccountId { get; init; }
    public int? VatAccountId { get; init; }
    public bool IsPieceTracked { get; init; }
    public bool IsService { get; init; }
}

public sealed class EdoImportMappingSelectionDto
{
    public bool UseAutomaticFallbackForMissingSelections { get; init; }
    public string? ProviderCode { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public IReadOnlyDictionary<int, EdoImportLineMappingSelectionDto> Lines { get; init; } =
        new Dictionary<int, EdoImportLineMappingSelectionDto>();
}

public sealed class EdoImportLineMappingSelectionDto
{
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public int? DebitAccountId { get; init; }
    public int? VatAccountId { get; init; }
}

public sealed record EdoImportJobMappingCountsDto(
    int ReadyCount,
    int MappingRequiredCount,
    int DuplicateCount);

public sealed class EdoImportDraftFailureSourceDto
{
    public long CandidateId { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string SafeErrorCode { get; init; } = string.Empty;
    public int TotalMarkingCount { get; init; }
    public int UsedMarkingCount { get; init; }
    public IReadOnlyCollection<long> ExistingPurchaseIds { get; init; } = [];
}

public sealed class EdoImportDraftMarkingUsageDto
{
    public int TotalMarkingCount { get; init; }
    public int UsedMarkingCount { get; init; }
    public IReadOnlyCollection<long> ExistingPurchaseIds { get; init; } = [];
    public long? ExistingPurchaseIdForAllMarkings { get; init; }
    public bool HasPartialOrMultiplePurchaseConflict =>
        UsedMarkingCount > 0 && !ExistingPurchaseIdForAllMarkings.HasValue;
}

public sealed class EdoImportMarkingConflictSourceDto
{
    public long CandidateId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? SafeErrorCode { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public decimal? ExpectedQuantity { get; init; }
    public int ActualMarkingCount { get; init; }
    public int ConflictCount { get; init; }
    public IReadOnlyCollection<long> ExistingPurchaseIds { get; init; } = [];
}

public sealed class EdoImportMappingSummaryCandidateSourceDto
{
    public long CandidateId { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? SafeErrorCode { get; init; }
    public string? SellerTin { get; init; }
    public string? SellerName { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public IReadOnlyCollection<EdoImportMappingSummaryLineSourceDto> Lines { get; init; } = [];
}

public sealed class EdoImportMappingSummaryLineSourceDto
{
    public string? CatalogCode { get; init; }
    public string? ProviderProductName { get; init; }
    public bool? IsService { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public decimal? VatRate { get; init; }
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public string MappingStatus { get; init; } = string.Empty;
    public bool HasProviderMarkings { get; init; }
    public int ProviderMarkingCount { get; init; }
}

public sealed class EdoImportPieceTrackingApplyStoreResultDto
{
    public string? SafeErrorCode { get; init; }
    public int UpdatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
}

public sealed class EdoImportMasterDataPlanSourceDto
{
    public IReadOnlyCollection<EdoImportMappingSummaryCandidateSourceDto> Candidates { get; init; } = [];
    public IReadOnlyCollection<EdoImportExistingCounterpartySourceDto> Counterparties { get; init; } = [];
    public IReadOnlyCollection<EdoImportExistingContractSourceDto> Contracts { get; init; } = [];
    public IReadOnlyCollection<EdoImportExistingProductSourceDto> Products { get; init; } = [];
}

public sealed class EdoImportExistingCounterpartySourceDto
{
    public int Id { get; init; }
    public string Tin { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public sealed class EdoImportExistingContractSourceDto
{
    public long Id { get; init; }
    public int CounterpartyId { get; init; }
    public string Number { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public string? ProviderCode { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}

public sealed class EdoImportExistingProductSourceDto
{
    public int Id { get; init; }
    public string CatalogCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public bool IsPieceTracked { get; init; }
    public short UnitId { get; init; }
    public short? VatRateId { get; init; }
}

public sealed class EdoImportProductConflictSourceDto
{
    public IReadOnlyCollection<EdoImportMappingSummaryCandidateSourceDto> Candidates { get; init; } = [];
    public IReadOnlyCollection<EdoImportExistingProductSourceDto> Products { get; init; } = [];
    public IReadOnlyCollection<EdoImportVatRateSourceDto> VatRates { get; init; } = [];
    public IReadOnlyCollection<EdoImportProviderProductMappingSourceDto> Mappings { get; init; } = [];
}

public sealed class EdoImportVatRateSourceDto
{
    public short Id { get; init; }
    public decimal Rate { get; init; }
    public DateOnly? EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
}

public sealed class EdoImportProviderProductMappingSourceDto
{
    public string IdentityHash { get; init; } = string.Empty;
    public string ProviderCode { get; init; } = string.Empty;
    public string CatalogCode { get; init; } = string.Empty;
    public string PackageCode { get; init; } = string.Empty;
    public string ProviderProductName { get; init; } = string.Empty;
    public string ProviderProductNameHash { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public int ProductId { get; init; }
}

public sealed class EdoImportProductConflictApplyCommandDto
{
    public bool ReuseOnly { get; init; }
    public IReadOnlyCollection<EdoImportProductConflictApplyCommandItemDto> Items { get; init; } = [];
}

public sealed class EdoImportProductConflictApplyCommandItemDto
{
    public IReadOnlyCollection<EdoImportProviderProductIdentityCommandDto> Identities { get; init; } = [];
    public string Action { get; init; } = string.Empty;
    public int? ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public bool? IsPieceTracked { get; init; }
}

public sealed class EdoImportProviderProductIdentityCommandDto
{
    public string IdentityHash { get; init; } = string.Empty;
    public string ProviderCode { get; init; } = string.Empty;
    public string CatalogCode { get; init; } = string.Empty;
    public string PackageCode { get; init; } = string.Empty;
    public string ProviderProductName { get; init; } = string.Empty;
    public string ProviderProductNameHash { get; init; } = string.Empty;
    public bool IsService { get; init; }
}

public sealed class EdoImportProductConflictApplyStoreResultDto
{
    public string? SafeErrorCode { get; init; }
    public int CreatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
    public int CreatedMappingCount { get; init; }
    public int ReusedMappingCount { get; init; }
}

public sealed class EdoImportMasterDataApplyCommandDto
{
    public bool ReuseOnly { get; init; }
    public IReadOnlyCollection<EdoImportCounterpartyApplyCommandItemDto> Counterparties { get; init; } = [];
    public IReadOnlyCollection<EdoImportContractApplyCommandItemDto> Contracts { get; init; } = [];
    public IReadOnlyCollection<EdoImportProductApplyCommandItemDto> Products { get; init; } = [];
}

public sealed class EdoImportCounterpartyApplyCommandItemDto
{
    public string SellerTin { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public int? ExistingId { get; init; }
}

public sealed class EdoImportContractApplyCommandItemDto
{
    public string ProviderCode { get; init; } = string.Empty;
    public string SellerTin { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public bool HasProviderIdentity { get; init; }
    public IReadOnlyCollection<long> CandidateIds { get; init; } = [];
    public string Action { get; init; } = string.Empty;
    public long? ExistingId { get; init; }
}

public sealed class EdoImportProductApplyCommandItemDto
{
    public string CatalogCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsService { get; init; }
    public bool IsPieceTracked { get; init; }
    public short UnitId { get; init; }
    public short VatRateId { get; init; }
    public string Action { get; init; } = string.Empty;
    public int? ExistingId { get; init; }
}

public sealed class EdoImportMasterDataApplyStoreResultDto
{
    public string? SafeErrorCode { get; init; }
    public IReadOnlyDictionary<string, int> CounterpartyIdsBySellerTin { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, long> ContractIdsByKey { get; init; } =
        new Dictionary<string, long>(StringComparer.Ordinal);
    public IReadOnlyDictionary<long, long> ContractIdsByCandidateId { get; init; } =
        new Dictionary<long, long>();
    public IReadOnlyDictionary<string, int> ProductIdsByCatalogCode { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public int CreatedCounterpartyCount { get; init; }
    public int ReusedCounterpartyCount { get; init; }
    public int CreatedContractCount { get; init; }
    public int ReusedContractCount { get; init; }
    public int CreatedProductCount { get; init; }
    public int ReusedProductCount { get; init; }
}
