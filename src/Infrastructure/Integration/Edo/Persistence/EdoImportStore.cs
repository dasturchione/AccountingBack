using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Integration.Edo.Historical;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel.Exceptions;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Integration.Edo.Persistence;

public sealed class EdoImportStore(AppDbContext context) : IEdoImportStore
{
    public Task<EdoImportJob?> GetJobAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default) =>
        context.EdoImportJobs.IgnoreQueryFilters()
            .Include(job => job.Providers)
            .SingleOrDefaultAsync(job => job.Id == jobId && job.OrganizationId == organizationId, ct);

    public Task<EdoImportJob?> GetJobForProcessingAsync(
        long jobId,
        CancellationToken ct = default) =>
        context.EdoImportJobs.IgnoreQueryFilters()
            .Include(job => job.Providers)
            .SingleOrDefaultAsync(job => job.Id == jobId, ct);

    public async Task<IReadOnlyCollection<long>> GetRunnableBulkImportJobIdsAsync(
        CancellationToken ct = default) =>
        await context.EdoImportJobs.IgnoreQueryFilters().AsNoTracking()
            .Where(job => job.BulkImportStatus == EdoImportBulkImportStatus.Queued
                || job.BulkImportStatus == EdoImportBulkImportStatus.Running)
            .OrderBy(job => job.Id)
            .Select(job => job.Id)
            .ToListAsync(ct);

    public Task<EdoImportJob?> FindActiveJobAsync(
        int organizationId,
        CancellationToken ct = default) =>
        context.EdoImportJobs.IgnoreQueryFilters().SingleOrDefaultAsync(job =>
            job.OrganizationId == organizationId
            && EdoImportJobStatus.ActiveValues.Contains(job.Status), ct);

    public Task<EdoImportJobProvider?> GetProviderAsync(
        int organizationId,
        long jobId,
        string providerCode,
        CancellationToken ct = default) =>
        context.EdoImportJobProviders.IgnoreQueryFilters().SingleOrDefaultAsync(provider =>
            provider.JobId == jobId
            && provider.Job.OrganizationId == organizationId
            && provider.ProviderCode == providerCode, ct);

    public Task<EdoImportCandidate?> GetCandidateAsync(
        int organizationId,
        long candidateId,
        CancellationToken ct = default) =>
        context.EdoImportCandidates.IgnoreQueryFilters()
            .Include(candidate => candidate.Lines)
            .ThenInclude(line => line.Markings)
            .SingleOrDefaultAsync(candidate => candidate.Id == candidateId
                && candidate.OrganizationId == organizationId, ct);

    public async Task<IReadOnlyCollection<EdoImportCandidate>> GetMappingRequiredCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default) =>
        await context.EdoImportCandidates.IgnoreQueryFilters()
            .Include(candidate => candidate.Lines)
            .ThenInclude(line => line.Markings)
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .OrderBy(candidate => candidate.Id)
            .ToListAsync(ct);

    public async Task<SharedKernel.QueryResults.PagedList<EdoImportCandidate>> GetCandidatesAsync(
        int organizationId,
        long jobId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId && candidate.JobId == jobId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(candidate => candidate.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new SharedKernel.QueryResults.PagedList<EdoImportCandidate>(items, total);
    }

    public async Task<IReadOnlyCollection<EdoImportMappingSummaryCandidateSourceDto>> GetMappingSummaryCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default) =>
        await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId)
            .OrderBy(candidate => candidate.Id)
            .Select(candidate => new EdoImportMappingSummaryCandidateSourceDto
            {
                CandidateId = candidate.Id,
                ProviderCode = candidate.ProviderCode,
                Status = candidate.Status,
                SafeErrorCode = candidate.SafeErrorCode,
                SellerTin = candidate.SellerTin,
                SellerName = candidate.SellerName,
                ProviderContractNumber = candidate.ProviderContractNumber,
                ProviderContractDate = candidate.ProviderContractDate,
                DocumentDate = candidate.DocumentDate,
                CounterpartyId = candidate.SelectedCounterpartyId,
                ContractId = candidate.SelectedContractId,
                CurrencyId = candidate.SelectedCurrencyId,
                WarehouseId = candidate.SelectedWarehouseId,
                Lines = candidate.Lines.OrderBy(line => line.ProviderLineNumber)
                    .Select(line => new EdoImportMappingSummaryLineSourceDto
                    {
                        CatalogCode = line.CatalogCode,
                        ProviderProductName = line.ProviderProductName,
                        IsService = line.IsService,
                        PackageCode = line.PackageCode,
                        PackageName = line.PackageName,
                        VatRate = line.VatRate,
                        ProductId = line.SelectedProductId,
                        UnitId = line.SelectedUnitId,
                        VatRateId = line.SelectedVatRateId,
                        MappingStatus = line.MappingStatus,
                        HasProviderMarkings = line.Markings.Any(),
                        ProviderMarkingCount = line.Markings.Count
                    })
                    .ToArray()
            })
            .ToListAsync(ct);

    public async Task<EdoImportPieceTrackingApplyStoreResultDto> ApplyPieceTrackingAsync(
        int organizationId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct = default)
    {
        var requestedIds = productIds.Distinct().Order().ToArray();
        var products = await context.Products.IgnoreQueryFilters()
            .Where(product => product.OrganizationId == organizationId
                && requestedIds.Contains(product.Id))
            .ToListAsync(ct);
        if (requestedIds.Length == 0 || products.Count != requestedIds.Length
            || products.Any(product => product.StateId != SharedKernel.Constants.StateIdConst.ACTIVE
                || product.IsService || !product.IsPurchased))
            return new EdoImportPieceTrackingApplyStoreResultDto
            {
                SafeErrorCode = "PIECE_TRACKING_PRODUCT_INVALID"
            };

        var updated = 0;
        var reused = 0;
        foreach (var product in products)
        {
            if (product.IsPieceTracked)
            {
                reused++;
                continue;
            }

            product.IsPieceTracked = true;
            updated++;
        }

        await context.SaveChangesAsync(ct);
        return new EdoImportPieceTrackingApplyStoreResultDto
        {
            UpdatedProductCount = updated,
            ReusedProductCount = reused
        };
    }

    public async Task<IReadOnlyCollection<EdoImportCandidate>> GetPieceTrackingCandidatesAsync(
        int organizationId,
        long jobId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToArray();
        return await context.EdoImportCandidates.IgnoreQueryFilters()
            .Include(candidate => candidate.Lines)
            .ThenInclude(line => line.Markings)
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && (candidate.Status == EdoImportCandidateStatus.MappingRequired
                    || candidate.Status == EdoImportCandidateStatus.Ready)
                && candidate.Lines.Any(line => line.SelectedProductId.HasValue
                    && ids.Contains(line.SelectedProductId.Value)
                    && line.Markings.Any()))
            .OrderBy(candidate => candidate.Id)
            .ToListAsync(ct);
    }

    public async Task<EdoImportMasterDataPlanSourceDto> GetMasterDataPlanSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var candidates = await GetMappingSummaryCandidatesAsync(organizationId, jobId, ct);
        var mappingCandidates = candidates
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .ToArray();
        var sellerTins = mappingCandidates
            .Select(candidate => candidate.SellerTin?.Trim())
            .Where(tin => !string.IsNullOrWhiteSpace(tin))
            .Select(tin => tin!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var catalogCodes = candidates
            .SelectMany(candidate => candidate.Lines)
            .Select(line => line.CatalogCode?.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var counterparties = await context.CounterpartyCards.IgnoreQueryFilters().AsNoTracking()
            .Where(counterparty => counterparty.OrganizationId == organizationId
                && counterparty.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && counterparty.Inn != null
                && sellerTins.Contains(counterparty.Inn))
            .Select(counterparty => new EdoImportExistingCounterpartySourceDto
            {
                Id = counterparty.Id,
                Tin = counterparty.Inn!,
                Name = counterparty.ShortName
            })
            .ToListAsync(ct);
        var counterpartyIds = counterparties.Select(counterparty => counterparty.Id).ToArray();
        var contractRows = await context.Contracts.IgnoreQueryFilters().AsNoTracking()
            .Where(contract => contract.OrganizationId == organizationId
                && contract.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && counterpartyIds.Contains(contract.CounterpartyId))
            .Select(contract => new
            {
                contract.Id,
                contract.CounterpartyId,
                contract.ContractNumber,
                contract.ContractDate,
                contract.StartDate,
                contract.EndDate,
                contract.ProviderCode,
                contract.ProviderContractNumber,
                contract.ProviderContractDate
            })
            .ToListAsync(ct);
        var contracts = contractRows.Select(contract => new EdoImportExistingContractSourceDto
        {
            Id = contract.Id,
            CounterpartyId = contract.CounterpartyId,
            Number = contract.ContractNumber,
            Date = DateOnly.FromDateTime(contract.ContractDate),
            StartDate = contract.StartDate.HasValue
                ? DateOnly.FromDateTime(contract.StartDate.Value)
                : null,
            EndDate = contract.EndDate.HasValue
                ? DateOnly.FromDateTime(contract.EndDate.Value)
                : null,
            ProviderCode = contract.ProviderCode,
            ProviderContractNumber = contract.ProviderContractNumber,
            ProviderContractDate = contract.ProviderContractDate
        }).ToArray();
        var products = await context.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.OrganizationId == organizationId
                && product.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && product.IsPurchased
                && product.Mxik != null
                && catalogCodes.Contains(product.Mxik))
            .Select(product => new EdoImportExistingProductSourceDto
            {
                Id = product.Id,
                CatalogCode = product.Mxik!,
                Name = product.Name,
                IsService = product.IsService,
                IsPieceTracked = product.IsPieceTracked,
                UnitId = product.UnitId,
                VatRateId = product.DefaultVatRateId
            })
            .ToListAsync(ct);

        return new EdoImportMasterDataPlanSourceDto
        {
            Candidates = candidates,
            Counterparties = counterparties,
            Contracts = contracts,
            Products = products
        };
    }

    public async Task<EdoImportProductConflictSourceDto> GetProductConflictSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var candidates = await GetMappingSummaryCandidatesAsync(organizationId, jobId, ct);
        var catalogCodes = candidates
            .Where(candidate => candidate.Status == EdoImportCandidateStatus.MappingRequired)
            .SelectMany(candidate => candidate.Lines)
            .Where(line => !line.ProductId.HasValue)
            .Select(line => EdoProviderProductIdentity.Normalize(line.CatalogCode))
            .Where(code => code is not null)
            .Select(code => code!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var mappings = await context.EdoProviderProductMappings.IgnoreQueryFilters().AsNoTracking()
            .Where(mapping => mapping.OrganizationId == organizationId)
            .Select(mapping => new EdoImportProviderProductMappingSourceDto
            {
                IdentityHash = mapping.IdentityHash,
                ProviderCode = mapping.ProviderCode,
                CatalogCode = mapping.CatalogCode,
                PackageCode = mapping.PackageCode,
                ProviderProductName = mapping.ProviderProductName,
                ProviderProductNameHash = mapping.ProviderProductNameHash,
                IsService = mapping.IsService,
                ProductId = mapping.ProductId
            })
            .ToListAsync(ct);
        var mappedProductIds = mappings.Select(mapping => mapping.ProductId).Distinct().ToArray();
        var products = await context.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.OrganizationId == organizationId
                && product.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && product.IsPurchased
                && product.Mxik != null
                && (catalogCodes.Contains(product.Mxik) || mappedProductIds.Contains(product.Id)))
            .Select(product => new EdoImportExistingProductSourceDto
            {
                Id = product.Id,
                CatalogCode = product.Mxik!,
                Name = product.Name,
                IsService = product.IsService,
                IsPieceTracked = product.IsPieceTracked,
                UnitId = product.UnitId,
                VatRateId = product.DefaultVatRateId
            })
            .ToListAsync(ct);
        var vatRates = await context.VatRates.IgnoreQueryFilters().AsNoTracking()
            .Where(vat => vat.StateId == SharedKernel.Constants.StateIdConst.ACTIVE)
            .Select(vat => new EdoImportVatRateSourceDto
            {
                Id = vat.Id,
                Rate = vat.Rate,
                EffectiveFrom = vat.EffectiveFrom,
                EffectiveTo = vat.EffectiveTo
            })
            .ToListAsync(ct);
        return new EdoImportProductConflictSourceDto
        {
            Candidates = candidates,
            Products = products,
            VatRates = vatRates,
            Mappings = mappings
        };
    }

    public async Task<IReadOnlyCollection<EdoImportMarkingConflictSourceDto>> GetMarkingConflictSourceAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var activeCodes = new[]
        {
            EdoImportMarkingPolicy.AlreadyUsed,
            EdoImportMarkingPolicy.CountMismatch,
            EdoImportMarkingPolicy.ProviderDataRequired,
            EdoImportMarkingPolicy.Duplicate,
            EdoImportMarkingPolicy.QuantityInvalid
        };
        var skippedCodes = activeCodes.Select(EdoImportMarkingPolicy.ToSkippedConflict).ToArray();
        var candidates = await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && (candidate.Status == EdoImportCandidateStatus.MappingRequired
                        && activeCodes.Contains(candidate.SafeErrorCode!)
                    || candidate.Status == EdoImportCandidateStatus.Skipped
                        && skippedCodes.Contains(candidate.SafeErrorCode!)))
            .OrderBy(candidate => candidate.Id)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Status,
                candidate.SafeErrorCode,
                candidate.DocumentNumber,
                candidate.DocumentDate,
                Lines = candidate.Lines.Select(line => new
                {
                    line.Quantity,
                    line.IsService,
                    Markings = line.Markings.Select(marking => marking.MarkingNumber).ToArray()
                }).ToArray(),
                Markings = candidate.Lines.SelectMany(line => line.Markings)
                    .Select(marking => marking.MarkingNumber)
                    .Distinct()
                    .ToArray()
            })
            .ToListAsync(ct);
        var result = new List<EdoImportMarkingConflictSourceDto>(candidates.Count);
        foreach (var candidate in candidates)
        {
            var originalCode = EdoImportMarkingPolicy.GetOriginalConflict(candidate.SafeErrorCode)!;
            var goodsLines = candidate.Lines.Where(line => line.IsService != true).ToArray();
            var relevantLines = originalCode switch
            {
                EdoImportMarkingPolicy.ProviderDataRequired => goodsLines
                    .Where(line => line.Markings.Length == 0).ToArray(),
                EdoImportMarkingPolicy.CountMismatch => goodsLines
                    .Where(line => IsPositiveWholeQuantity(line.Quantity)
                        && line.Markings.Length > decimal.ToInt32(line.Quantity!.Value)).ToArray(),
                EdoImportMarkingPolicy.Duplicate => goodsLines
                    .Where(line => line.Markings.Distinct(StringComparer.Ordinal).Count()
                        != line.Markings.Length).ToArray(),
                EdoImportMarkingPolicy.QuantityInvalid => goodsLines
                    .Where(line => !IsPositiveWholeQuantity(line.Quantity)).ToArray(),
                _ => goodsLines.Where(line => line.Markings.Length > 0).ToArray()
            };
            if (relevantLines.Length == 0)
                relevantLines = goodsLines;

            var expectedQuantity = relevantLines.Length == 0
                || relevantLines.Any(line => !line.Quantity.HasValue)
                    ? (decimal?)null
                    : relevantLines.Sum(line => line.Quantity!.Value);
            var actualMarkingCount = relevantLines.Sum(line => line.Markings.Length);
            var owners = originalCode != EdoImportMarkingPolicy.AlreadyUsed
                || candidate.Markings.Length == 0
                ? []
                : await (
                    from purchaseTable in context.PurchaseDocTables.IgnoreQueryFilters().AsNoTracking()
                    join purchaseLine in context.PurchaseDocProducts.IgnoreQueryFilters().AsNoTracking()
                        on purchaseTable.OwnerId equals purchaseLine.Id
                    join purchase in context.PurchaseDocs.IgnoreQueryFilters().AsNoTracking()
                        on purchaseLine.OwnerId equals purchase.Id
                    join productTable in context.ProductTables.IgnoreQueryFilters().AsNoTracking()
                        on purchaseTable.ProductTableId equals productTable.Id
                    join product in context.Products.IgnoreQueryFilters().AsNoTracking()
                        on productTable.ProductId equals product.Id
                    where purchase.OrganizationId == organizationId
                        && product.OrganizationId == organizationId
                        && productTable.MarkingNumber != null
                        && candidate.Markings.Contains(productTable.MarkingNumber)
                    select new { productTable.MarkingNumber, PurchaseId = purchase.Id })
                .Distinct()
                .ToListAsync(ct);
            result.Add(new EdoImportMarkingConflictSourceDto
            {
                CandidateId = candidate.Id,
                Status = candidate.Status,
                SafeErrorCode = candidate.SafeErrorCode,
                DocumentNumber = candidate.DocumentNumber,
                DocumentDate = candidate.DocumentDate,
                ExpectedQuantity = expectedQuantity,
                ActualMarkingCount = actualMarkingCount,
                ConflictCount = owners.Select(owner => owner.MarkingNumber)
                    .Distinct(StringComparer.Ordinal).Count(),
                ExistingPurchaseIds = owners.Select(owner => owner.PurchaseId)
                    .Distinct().Order().ToArray()
            });
        }

        return result;
    }

    private static bool IsPositiveWholeQuantity(decimal? quantity) =>
        quantity.HasValue
        && quantity.Value > 0
        && quantity.Value <= int.MaxValue
        && decimal.Truncate(quantity.Value) == quantity.Value;

    public async Task<IReadOnlyCollection<EdoImportCandidate>> GetReadyImportCandidatesAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default) =>
        await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Include(candidate => candidate.Lines)
            .ThenInclude(line => line.Markings)
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && candidate.Status == EdoImportCandidateStatus.Ready)
            .OrderBy(candidate => candidate.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<EdoImportDraftFailureSourceDto>> GetDraftImportFailuresAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var candidates = await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && candidate.Status == EdoImportCandidateStatus.Failed
                && candidate.SafeErrorCode != null
                && candidate.SafeErrorCode.StartsWith("DRAFT_IMPORT_"))
            .OrderBy(candidate => candidate.Id)
            .Select(candidate => new EdoImportDraftFailureSourceDto
            {
                CandidateId = candidate.Id,
                DocumentNumber = candidate.DocumentNumber,
                DocumentDate = candidate.DocumentDate,
                SafeErrorCode = candidate.SafeErrorCode!,
                TotalMarkingCount = candidate.Lines.SelectMany(line => line.Markings).Count()
            })
            .ToListAsync(ct);

        var markingFailures = candidates
            .Where(candidate => EdoImportDraftFailurePolicy.IsMarkingAlreadyUsed(
                candidate.SafeErrorCode))
            .ToDictionary(candidate => candidate.CandidateId);
        if (markingFailures.Count == 0)
            return candidates;

        var markingFailureIds = markingFailures.Keys.ToArray();
        var candidateMarkings = await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId
                && markingFailureIds.Contains(candidate.Id))
            .Select(candidate => new
            {
                candidate.Id,
                Markings = candidate.Lines.SelectMany(line => line.Markings)
                    .Select(marking => marking.MarkingNumber).ToArray()
            })
            .ToListAsync(ct);
        var result = new List<EdoImportDraftFailureSourceDto>(candidates.Count);
        foreach (var candidate in candidates)
        {
            if (!markingFailures.ContainsKey(candidate.CandidateId))
            {
                result.Add(candidate);
                continue;
            }

            var markings = candidateMarkings.Single(item => item.Id == candidate.CandidateId).Markings;
            var usage = await GetDraftMarkingUsageAsync(organizationId, markings, ct);
            result.Add(new EdoImportDraftFailureSourceDto
            {
                CandidateId = candidate.CandidateId,
                DocumentNumber = candidate.DocumentNumber,
                DocumentDate = candidate.DocumentDate,
                SafeErrorCode = candidate.SafeErrorCode,
                TotalMarkingCount = usage.TotalMarkingCount,
                UsedMarkingCount = usage.UsedMarkingCount,
                ExistingPurchaseIds = usage.ExistingPurchaseIds
            });
        }

        return result;
    }

    public Task<EdoImportJob?> GetJobForImportAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var query = context.Database.ProviderName?.Contains("Npgsql", StringComparison.Ordinal) == true
            ? context.EdoImportJobs.FromSqlInterpolated(
                $"select * from edo_import_job where id = {jobId} and organization_id = {organizationId} for update")
            : context.EdoImportJobs.Where(job => job.Id == jobId && job.OrganizationId == organizationId);
        return query.IgnoreQueryFilters().SingleOrDefaultAsync(ct);
    }

    public Task<EdoImportCandidate?> GetCandidateForImportAsync(
        int organizationId,
        long jobId,
        long candidateId,
        CancellationToken ct = default)
    {
        var query = context.Database.ProviderName?.Contains("Npgsql", StringComparison.Ordinal) == true
            ? context.EdoImportCandidates.FromSqlInterpolated(
                $"select * from edo_import_candidate where id = {candidateId} and job_id = {jobId} and organization_id = {organizationId} for update")
            : context.EdoImportCandidates.Where(candidate => candidate.Id == candidateId
                && candidate.JobId == jobId
                && candidate.OrganizationId == organizationId);
        return query.IgnoreQueryFilters()
            .Include(candidate => candidate.Lines)
            .ThenInclude(line => line.Markings)
            .SingleOrDefaultAsync(ct);
    }

    public async Task AcquireDraftImportLocksAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default)
    {
        if (context.Database.ProviderName?.Contains("Npgsql", StringComparison.Ordinal) != true)
            return;

        var documentLock = ImportLockKey(
            "DOCUMENT", organizationId, providerCode, providerDocumentId);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"select pg_advisory_xact_lock({documentLock})", ct);
        foreach (var markingLock in markings
            .Where(marking => !string.IsNullOrWhiteSpace(marking))
            .Select(marking => ImportLockKey("MARKING", organizationId, string.Empty, marking.Trim()))
            .Distinct()
            .Order())
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"select pg_advisory_xact_lock({markingLock})", ct);
        }
    }

    public async Task<long?> FindExistingPurchaseForProviderDocumentAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default)
    {
        var linked = await FindLinkedPurchaseIdAsync(
            organizationId, providerCode, providerDocumentId, ct);
        if (linked.HasValue)
            return linked;

        return await (
            from candidate in context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            join purchase in context.PurchaseDocs.IgnoreQueryFilters().AsNoTracking()
                on (candidate.ImportedPurchaseId ?? candidate.ExistingPurchaseId) equals purchase.Id
            where candidate.OrganizationId == organizationId
                && candidate.ProviderCode == providerCode
                && candidate.ProviderDocumentId == providerDocumentId
                && purchase.OrganizationId == organizationId
            orderby candidate.Id descending
            select (long?)purchase.Id)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> AnyUsedMarkingsAsync(
        int organizationId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default)
    {
        var normalized = markings.Where(marking => !string.IsNullOrWhiteSpace(marking))
            .Select(marking => marking.Trim()).Distinct(StringComparer.Ordinal).ToArray();
        return normalized.Length == 0
            ? Task.FromResult(false)
            : (from table in context.ProductTables.IgnoreQueryFilters().AsNoTracking()
               join product in context.Products.IgnoreQueryFilters().AsNoTracking()
                   on table.ProductId equals product.Id
               where product.OrganizationId == organizationId
                   && table.MarkingNumber != null
                   && normalized.Contains(table.MarkingNumber)
               select table.Id).AnyAsync(ct);
    }

    public async Task<EdoImportDraftMarkingUsageDto> GetDraftMarkingUsageAsync(
        int organizationId,
        IReadOnlyCollection<string> markings,
        CancellationToken ct = default)
    {
        var normalized = markings.Where(marking => !string.IsNullOrWhiteSpace(marking))
            .Select(marking => marking.Trim()).ToArray();
        var distinctMarkings = normalized.Distinct(StringComparer.Ordinal).ToArray();
        if (distinctMarkings.Length == 0)
            return new EdoImportDraftMarkingUsageDto();

        var owners = await (
                from purchaseTable in context.PurchaseDocTables.IgnoreQueryFilters().AsNoTracking()
                join purchaseLine in context.PurchaseDocProducts.IgnoreQueryFilters().AsNoTracking()
                    on purchaseTable.OwnerId equals purchaseLine.Id
                join purchase in context.PurchaseDocs.IgnoreQueryFilters().AsNoTracking()
                    on purchaseLine.OwnerId equals purchase.Id
                join productTable in context.ProductTables.IgnoreQueryFilters().AsNoTracking()
                    on purchaseTable.ProductTableId equals productTable.Id
                join product in context.Products.IgnoreQueryFilters().AsNoTracking()
                    on productTable.ProductId equals product.Id
                where purchase.OrganizationId == organizationId
                    && product.OrganizationId == organizationId
                    && productTable.MarkingNumber != null
                    && distinctMarkings.Contains(productTable.MarkingNumber)
                select new { Marking = productTable.MarkingNumber!, PurchaseId = purchase.Id })
            .Distinct()
            .ToListAsync(ct);
        var ownersByMarking = owners.GroupBy(owner => owner.Marking, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(owner => owner.PurchaseId).Distinct().ToArray(),
                StringComparer.Ordinal);
        var usedMarkingCount = normalized.Count(ownersByMarking.ContainsKey);
        var ownerIds = owners.Select(owner => owner.PurchaseId).Distinct().Order().ToArray();
        var exactOwnerIds = normalized.Length == distinctMarkings.Length
            && normalized.All(marking => ownersByMarking.TryGetValue(marking, out var markingOwners)
                && markingOwners.Length == 1)
                ? normalized.Select(marking => ownersByMarking[marking][0]).Distinct().ToArray()
                : [];

        return new EdoImportDraftMarkingUsageDto
        {
            TotalMarkingCount = normalized.Length,
            UsedMarkingCount = usedMarkingCount,
            ExistingPurchaseIds = ownerIds,
            ExistingPurchaseIdForAllMarkings = exactOwnerIds.Length == 1
                ? exactOwnerIds[0]
                : null
        };
    }

    public void ClearTracking() => context.ChangeTracker.Clear();

    public async Task AcquireMasterDataApplyLockAsync(
        int organizationId,
        CancellationToken ct = default)
    {
        if (context.Database.ProviderName?.Contains("Npgsql", StringComparison.Ordinal) == true)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"select pg_advisory_xact_lock(1162104655, {organizationId})", ct);
            await context.Database.ExecuteSqlRawAsync(
                "lock table counterparty_card, cmn_contract, inv_product, edo_provider_product_mapping in share row exclusive mode", ct);
        }
    }

    private static long ImportLockKey(
        string kind,
        int organizationId,
        string providerCode,
        string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{kind}\u001f{organizationId}\u001f{providerCode}\u001f{value}"));
        return BinaryPrimitives.ReadInt64BigEndian(bytes);
    }

    public async Task<EdoImportMasterDataApplyStoreResultDto> ApplyMasterDataAsync(
        int organizationId,
        EdoImportMasterDataApplyCommandDto command,
        DateTime now,
        CancellationToken ct = default)
    {
        var counterpartyIds = new Dictionary<string, int>(StringComparer.Ordinal);
        var contractIds = new Dictionary<string, long>(StringComparer.Ordinal);
        var contractIdsByCandidateId = new Dictionary<long, long>();
        var productIds = new Dictionary<string, int>(StringComparer.Ordinal);
        var createdCounterparties = 0;
        var reusedCounterparties = 0;
        var createdContracts = 0;
        var reusedContracts = 0;
        var createdProducts = 0;
        var reusedProducts = 0;

        foreach (var item in command.Counterparties)
        {
            var matches = await context.CounterpartyCards.IgnoreQueryFilters()
                .Where(entity => entity.OrganizationId == organizationId && entity.Inn == item.SellerTin)
                .ToListAsync(ct);
            var reusable = matches.Count == 1
                && matches[0].StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                    ? matches[0]
                    : null;
            if (matches.Count > 1 || matches.Count == 1 && reusable is null)
                return Failure("COUNTERPARTY_CONFLICT");
            if (reusable is not null)
            {
                if (item.Action == "USE_EXISTING" && item.ExistingId != reusable.Id)
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                counterpartyIds[item.SellerTin] = reusable.Id;
                reusedCounterparties++;
                continue;
            }
            if (item.Action != "CREATE")
                return Failure("MASTER_DATA_SELECTION_INVALID");
            if (command.ReuseOnly)
                return Failure("STALE_MASTER_DATA_PLAN");

            var entity = new CounterpartyCard
            {
                OrganizationId = organizationId,
                ShortName = item.Name,
                FullName = item.Name,
                Inn = item.SellerTin,
                IsVatPayer = false,
                StateId = SharedKernel.Constants.StateIdConst.ACTIVE,
                CreatedDate = now
            };
            context.CounterpartyCards.Add(entity);
            await context.SaveChangesAsync(ct);
            counterpartyIds[item.SellerTin] = entity.Id;
            createdCounterparties++;
        }

        foreach (var item in command.Contracts)
        {
            if (!counterpartyIds.TryGetValue(item.SellerTin, out var counterpartyId))
            {
                var matches = await context.CounterpartyCards.IgnoreQueryFilters().AsNoTracking()
                    .Where(entity => entity.OrganizationId == organizationId
                        && entity.Inn == item.SellerTin
                        && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE)
                    .Select(entity => entity.Id)
                    .Take(2)
                    .ToListAsync(ct);
                if (matches.Count != 1)
                    return Failure("COUNTERPARTY_MAPPING_REQUIRED");
                counterpartyId = matches[0];
            }

            if (!item.HasProviderIdentity)
            {
                if (item.Action != "USE_EXISTING"
                    || !item.ExistingId.HasValue
                    || item.CandidateIds.Count == 0)
                {
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                }

                var candidateIds = item.CandidateIds.Distinct().ToArray();
                if (candidateIds.Length != item.CandidateIds.Count)
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                var candidates = await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
                    .Where(candidate => candidate.OrganizationId == organizationId
                        && candidateIds.Contains(candidate.Id))
                    .Select(candidate => new
                    {
                        candidate.Id,
                        candidate.SellerTin,
                        candidate.ProviderContractNumber,
                        candidate.ProviderContractDate,
                        candidate.DocumentDate,
                        candidate.ExistingPurchaseId,
                        candidate.ImportedPurchaseId
                    })
                    .ToListAsync(ct);
                if (candidates.Count != candidateIds.Length
                    || candidates.Any(candidate => !string.Equals(candidate.SellerTin, item.SellerTin,
                        StringComparison.Ordinal)
                        || candidate.ProviderContractNumber is not null
                        || candidate.ProviderContractDate.HasValue
                        || !candidate.DocumentDate.HasValue
                        || candidate.ExistingPurchaseId.HasValue
                        || candidate.ImportedPurchaseId.HasValue))
                {
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                }

                var selected = await context.Contracts.IgnoreQueryFilters().SingleOrDefaultAsync(contract =>
                    contract.Id == item.ExistingId.Value
                    && contract.OrganizationId == organizationId
                    && contract.CounterpartyId == counterpartyId
                    && contract.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                    && contract.StartDate.HasValue, ct);
                if (selected is null
                    || candidates.Any(candidate =>
                        selected.StartDate!.Value > candidate.DocumentDate!.Value.ToDateTime(TimeOnly.MinValue)
                        || selected.EndDate.HasValue
                            && selected.EndDate.Value < candidate.DocumentDate!.Value.ToDateTime(TimeOnly.MinValue)))
                {
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                }

                foreach (var candidateId in candidateIds)
                    contractIdsByCandidateId[candidateId] = selected.Id;
                reusedContracts++;
                continue;
            }

            var exactProviderIdentity = await context.Contracts.IgnoreQueryFilters()
                .Where(entity => entity.OrganizationId == organizationId
                    && entity.CounterpartyId == counterpartyId
                    && entity.ProviderCode == item.ProviderCode
                    && entity.ProviderContractNumber == item.Number
                    && entity.ProviderContractDate == item.Date)
                .ToListAsync(ct);
            if (exactProviderIdentity.Count > 1)
                return Failure("CONTRACT_CONFLICT");
            var reusable = exactProviderIdentity.SingleOrDefault();
            if (reusable is not null && reusable.StateId != SharedKernel.Constants.StateIdConst.ACTIVE)
                return Failure("CONTRACT_CONFLICT");

            if (item.Action == "USE_EXISTING")
            {
                if (!item.ExistingId.HasValue)
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                var explicitlySelected = await context.Contracts.IgnoreQueryFilters().SingleOrDefaultAsync(entity =>
                    entity.Id == item.ExistingId.Value
                    && entity.OrganizationId == organizationId
                    && entity.CounterpartyId == counterpartyId
                    && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct);
                if (explicitlySelected is null
                    || DateOnly.FromDateTime(explicitlySelected.ContractDate) != item.Date)
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                var hasNoProviderIdentity = explicitlySelected.ProviderCode is null
                    && explicitlySelected.ProviderContractNumber is null
                    && !explicitlySelected.ProviderContractDate.HasValue;
                var hasExactProviderIdentity = string.Equals(
                        explicitlySelected.ProviderCode, item.ProviderCode, StringComparison.Ordinal)
                    && string.Equals(
                        explicitlySelected.ProviderContractNumber, item.Number, StringComparison.Ordinal)
                    && explicitlySelected.ProviderContractDate == item.Date;
                if (!hasNoProviderIdentity && !hasExactProviderIdentity)
                    return Failure("CONTRACT_CONFLICT");
                if (reusable is not null && reusable.Id != explicitlySelected.Id)
                    return Failure("CONTRACT_CONFLICT");
                if (hasNoProviderIdentity)
                {
                    explicitlySelected.ProviderCode = item.ProviderCode;
                    explicitlySelected.ProviderContractNumber = item.Number;
                    explicitlySelected.ProviderContractDate = item.Date;
                    await context.SaveChangesAsync(ct);
                }
                reusable = explicitlySelected;
            }
            if (reusable is not null)
            {
                contractIds[ContractKey(item.ProviderCode, item.SellerTin, item.Number, item.Date)] = reusable.Id;
                reusedContracts++;
                continue;
            }
            if (item.Action != "CREATE")
                return Failure("MASTER_DATA_SELECTION_INVALID");
            if (command.ReuseOnly)
                return Failure("STALE_MASTER_DATA_PLAN");

            var date = item.Date.ToDateTime(TimeOnly.MinValue);
            var entity = new Contract
            {
                OrganizationId = organizationId,
                CounterpartyId = counterpartyId,
                ContractTypeId = 1,
                ContractNumber = string.Empty,
                ProviderCode = item.ProviderCode,
                ProviderContractNumber = item.Number,
                ProviderContractDate = item.Date,
                ContractDate = date,
                StartDate = date,
                EndDate = null,
                StateId = SharedKernel.Constants.StateIdConst.ACTIVE,
                CreatedDate = now
            };
            context.Contracts.Add(entity);
            await context.SaveChangesAsync(ct);
            contractIds[ContractKey(item.ProviderCode, item.SellerTin, item.Number, item.Date)] = entity.Id;
            createdContracts++;
        }

        foreach (var item in command.Products)
        {
            var matches = await context.Products.IgnoreQueryFilters()
                .Where(entity => entity.OrganizationId == organizationId && entity.Mxik == item.CatalogCode)
                .ToListAsync(ct);
            var reusable = matches.Count == 1
                && matches[0].StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && matches[0].IsPurchased
                && (item.Action == "USE_EXISTING"
                    ? matches[0].Id == item.ExistingId
                    : matches[0].IsService == item.IsService
                        && matches[0].UnitId == item.UnitId
                        && matches[0].DefaultVatRateId == item.VatRateId)
                    ? matches[0]
                    : null;
            if (matches.Count > 1 || matches.Count == 1 && reusable is null)
                return Failure("PRODUCT_CONFLICT");
            if (reusable is not null)
            {
                if (item.Action == "USE_EXISTING" && item.ExistingId != reusable.Id)
                    return Failure("MASTER_DATA_SELECTION_INVALID");
                productIds[item.CatalogCode] = reusable.Id;
                reusedProducts++;
                continue;
            }
            if (item.Action != "CREATE")
                return Failure("MASTER_DATA_SELECTION_INVALID");
            if (command.ReuseOnly)
                return Failure("STALE_MASTER_DATA_PLAN");

            var validUnit = await context.Units.IgnoreQueryFilters().AsNoTracking().AnyAsync(entity =>
                entity.Id == item.UnitId && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct);
            var validVat = await context.VatRates.IgnoreQueryFilters().AsNoTracking().AnyAsync(entity =>
                entity.Id == item.VatRateId && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct);
            if (!validUnit || !validVat)
                return Failure("MASTER_DATA_SELECTION_INVALID");

            var entity = new Product
            {
                OrganizationId = organizationId,
                UnitId = item.UnitId,
                Name = item.Name,
                IsService = item.IsService,
                IsPieceTracked = item.IsPieceTracked,
                IsSold = false,
                IsPurchased = true,
                Mxik = item.CatalogCode,
                DefaultVatRateId = item.VatRateId,
                StateId = SharedKernel.Constants.StateIdConst.ACTIVE,
                CreatedDate = now
            };
            context.Products.Add(entity);
            await context.SaveChangesAsync(ct);
            productIds[item.CatalogCode] = entity.Id;
            createdProducts++;
        }

        return new EdoImportMasterDataApplyStoreResultDto
        {
            CounterpartyIdsBySellerTin = counterpartyIds,
            ContractIdsByKey = contractIds,
            ContractIdsByCandidateId = contractIdsByCandidateId,
            ProductIdsByCatalogCode = productIds,
            CreatedCounterpartyCount = createdCounterparties,
            ReusedCounterpartyCount = reusedCounterparties,
            CreatedContractCount = createdContracts,
            ReusedContractCount = reusedContracts,
            CreatedProductCount = createdProducts,
            ReusedProductCount = reusedProducts
        };

        EdoImportMasterDataApplyStoreResultDto Failure(string code) => new() { SafeErrorCode = code };
    }

    public async Task<EdoImportProductConflictApplyStoreResultDto> ApplyProductConflictMappingsAsync(
        int organizationId,
        EdoImportProductConflictApplyCommandDto command,
        DateTime now,
        CancellationToken ct = default)
    {
        var createdProducts = 0;
        var reusedProducts = 0;
        var createdMappings = 0;
        var reusedMappings = 0;
        foreach (var item in command.Items)
        {
            if (item.Identities.Count == 0
                || item.Identities.Select(identity => identity.IdentityHash)
                    .Distinct(StringComparer.Ordinal).Count() != item.Identities.Count
                || item.Identities.Select(identity => identity.CatalogCode)
                    .Distinct(StringComparer.Ordinal).Count() != 1)
                return Failure("PRODUCT_SELECTION_INVALID");
            var identityHashes = item.Identities.Select(identity => identity.IdentityHash).ToArray();
            var mappings = await context.EdoProviderProductMappings.IgnoreQueryFilters()
                .Where(entity => entity.OrganizationId == organizationId
                    && identityHashes.Contains(entity.IdentityHash))
                .ToListAsync(ct);
            var mappedProductIds = mappings.Select(mapping => mapping.ProductId).Distinct().ToArray();
            if (mappedProductIds.Length > 1)
                return Failure("PROVIDER_PRODUCT_MAPPING_CONFLICT");

            var catalogCode = item.Identities.First().CatalogCode;
            Product? product = null;
            if (mappedProductIds.Length == 1)
            {
                product = await context.Products.IgnoreQueryFilters().SingleOrDefaultAsync(entity =>
                    entity.Id == mappedProductIds[0]
                    && entity.OrganizationId == organizationId
                    && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                    && entity.IsPurchased
                    && entity.Mxik == catalogCode, ct);
                if (product is null
                    || item.Action == "USE_EXISTING" && product.Id != item.ProductId
                    || product.IsService != item.IsService
                    || product.UnitId != item.UnitId
                    || product.DefaultVatRateId != item.VatRateId
                    || product.IsPieceTracked != item.IsPieceTracked)
                    return Failure("PROVIDER_PRODUCT_MAPPING_CONFLICT");
                reusedProducts++;
            }
            else
            {
                if (command.ReuseOnly)
                    return Failure("STALE_PRODUCT_CONFLICT_PLAN");
                if (item.Action == "USE_EXISTING")
                {
                    if (!item.ProductId.HasValue)
                        return Failure("PRODUCT_SELECTION_INVALID");
                    product = await context.Products.IgnoreQueryFilters().SingleOrDefaultAsync(entity =>
                        entity.Id == item.ProductId.Value
                        && entity.OrganizationId == organizationId
                        && entity.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                        && entity.IsPurchased
                        && entity.Mxik == catalogCode
                        && entity.IsService == item.IsService
                        && entity.UnitId == item.UnitId
                        && entity.DefaultVatRateId == item.VatRateId
                        && entity.IsPieceTracked == item.IsPieceTracked, ct);
                    if (product is null)
                        return Failure("PRODUCT_SELECTION_INVALID");
                    reusedProducts++;
                }
                else if (item.Action == "CREATE")
                {
                    if (!item.UnitId.HasValue || !item.VatRateId.HasValue || !item.IsPieceTracked.HasValue)
                        return Failure("PRODUCT_SELECTION_INVALID");
                    var validUnit = await context.Units.IgnoreQueryFilters().AsNoTracking().AnyAsync(unit =>
                        unit.Id == item.UnitId.Value
                        && unit.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct);
                    var validVat = await context.VatRates.IgnoreQueryFilters().AsNoTracking().AnyAsync(vat =>
                        vat.Id == item.VatRateId.Value
                        && vat.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct);
                    if (!validUnit || !validVat || item.ProductName.Length is 0 or > 250)
                        return Failure("PRODUCT_SELECTION_INVALID");
                    product = new Product
                    {
                        OrganizationId = organizationId,
                        UnitId = item.UnitId.Value,
                        Name = item.ProductName,
                        IsService = item.IsService,
                        IsPieceTracked = item.IsPieceTracked.Value,
                        IsSold = false,
                        IsPurchased = true,
                        Mxik = catalogCode,
                        DefaultVatRateId = item.VatRateId,
                        StateId = SharedKernel.Constants.StateIdConst.ACTIVE,
                        CreatedDate = now
                    };
                    context.Products.Add(product);
                    await context.SaveChangesAsync(ct);
                    createdProducts++;
                }
                else
                {
                    return Failure("PRODUCT_SELECTION_INVALID");
                }
            }

            var mappedIdentityHashes = mappings.Select(mapping => mapping.IdentityHash)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var identity in item.Identities.Where(identity =>
                         !mappedIdentityHashes.Contains(identity.IdentityHash)))
            {
                context.EdoProviderProductMappings.Add(new EdoProviderProductMapping
                {
                    OrganizationId = organizationId,
                    ProviderCode = identity.ProviderCode,
                    CatalogCode = identity.CatalogCode,
                    PackageCode = identity.PackageCode,
                    ProviderProductName = identity.ProviderProductName,
                    ProviderProductNameHash = identity.ProviderProductNameHash,
                    IdentityHash = identity.IdentityHash,
                    IsService = identity.IsService,
                    ProductId = product.Id,
                    CreatedDate = now
                });
                createdMappings++;
            }
            reusedMappings += mappings.Count;
            await context.SaveChangesAsync(ct);
        }

        return new EdoImportProductConflictApplyStoreResultDto
        {
            CreatedProductCount = createdProducts,
            ReusedProductCount = reusedProducts,
            CreatedMappingCount = createdMappings,
            ReusedMappingCount = reusedMappings
        };

        EdoImportProductConflictApplyStoreResultDto Failure(string code) => new() { SafeErrorCode = code };
    }

    public Task<bool> IsContractApplicableAsync(
        int organizationId,
        int counterpartyId,
        long contractId,
        DateOnly documentDate,
        CancellationToken ct = default)
    {
        var date = documentDate.ToDateTime(TimeOnly.MinValue);
        return context.Contracts.IgnoreQueryFilters().AsNoTracking().AnyAsync(contract =>
            contract.Id == contractId
            && contract.OrganizationId == organizationId
            && contract.CounterpartyId == counterpartyId
            && contract.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
            && contract.StartDate.HasValue
            && contract.StartDate.Value <= date
            && (!contract.EndDate.HasValue || contract.EndDate.Value >= date), ct);
    }

    private static string ContractKey(string providerCode, string sellerTin, string number, DateOnly date) =>
        $"{providerCode}\u001f{sellerTin}\u001f{number}\u001f{date:yyyy-MM-dd}";

    public async Task<IReadOnlyCollection<long>> GetRunnableJobIdsAsync(
        DateTime now,
        CancellationToken ct = default) =>
        await context.EdoImportJobs.IgnoreQueryFilters().AsNoTracking()
            .Where(job => EdoImportJobStatus.ActiveValues.Contains(job.Status)
                && job.Status != EdoImportJobStatus.PreflightReady
                && job.Status != EdoImportJobStatus.Importing
                && (job.Status != EdoImportJobStatus.Partial
                    || job.Providers.Any(provider =>
                        provider.Status == EdoImportProviderCheckpointStatus.Partial
                        && provider.NextRetryAt.HasValue
                        && provider.NextRetryAt <= now))
                && (job.LeaseExpiresAt == null || job.LeaseExpiresAt <= now))
            .OrderBy(job => job.Id)
            .Select(job => job.Id)
            .ToListAsync(ct);

    public Task<bool> IsCancellationRequestedAsync(
        long jobId,
        CancellationToken ct = default) =>
        context.EdoImportJobs.IgnoreQueryFilters().AsNoTracking()
            .Where(job => job.Id == jobId)
            .Select(job => job.Status == EdoImportJobStatus.CancelRequested)
            .SingleOrDefaultAsync(ct);

    public async Task<bool> TryAcquireLeaseAsync(
        int organizationId,
        long jobId,
        string leaseOwner,
        DateTime now,
        DateTime leaseExpiresAt,
        CancellationToken ct = default)
    {
        var runnableStatuses = new[]
        {
            EdoImportJobStatus.Queued,
            EdoImportJobStatus.Scanning,
            EdoImportJobStatus.WaitingAuth,
            EdoImportJobStatus.Partial,
            EdoImportJobStatus.CancelRequested
        };
        var query = context.EdoImportJobs.IgnoreQueryFilters().Where(job =>
            job.Id == jobId
            && job.OrganizationId == organizationId
            && runnableStatuses.Contains(job.Status)
            && (job.LeaseExpiresAt == null
                || job.LeaseExpiresAt <= now
                || job.LeaseOwner == leaseOwner));

        if (context.Database.IsRelational())
        {
            var affected = await query.ExecuteUpdateAsync(update => update
                .SetProperty(job => job.LeaseOwner, leaseOwner)
                .SetProperty(job => job.LeaseExpiresAt, leaseExpiresAt)
                .SetProperty(job => job.HeartbeatAt, now)
                .SetProperty(job => job.UpdatedDate, now), ct);
            context.ChangeTracker.Clear();
            return affected == 1;
        }

        var inMemoryJob = await query.SingleOrDefaultAsync(ct);
        if (inMemoryJob is null)
            return false;
        inMemoryJob.SetLease(leaseOwner, leaseExpiresAt, now);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public Task<EdoImportCandidate?> FindPriorProviderCandidateAsync(
        int organizationId,
        long currentJobId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default) =>
        context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId != currentJobId
                && candidate.ProviderCode == providerCode
                && candidate.ProviderDocumentId == providerDocumentId)
            .OrderByDescending(candidate => candidate.Id)
            .FirstOrDefaultAsync(ct);

    public Task<EdoImportCandidate?> FindJobCandidateAsync(
        int organizationId,
        long jobId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default) =>
        context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(candidate =>
            candidate.OrganizationId == organizationId
            && candidate.JobId == jobId
            && candidate.ProviderCode == providerCode
            && candidate.ProviderDocumentId == providerDocumentId, ct);

    public Task<EdoImportCandidate?> FindCrossProviderCandidateAsync(
        int organizationId,
        string providerCode,
        string fingerprint,
        bool contentFingerprint,
        CancellationToken ct = default) =>
        context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.ProviderCode != providerCode
                && (contentFingerprint
                    ? candidate.ContentFingerprint == fingerprint
                    : candidate.HeaderFingerprint == fingerprint))
            .OrderByDescending(candidate => candidate.Id)
            .FirstOrDefaultAsync(ct);

    public Task<EdoDocument?> FindEdoDocumentAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default) =>
        context.EdoDocuments.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(document =>
            document.OrganizationId == organizationId
            && document.Provider == providerCode
            && document.ProviderDocumentId == providerDocumentId, ct);

    public Task<long?> FindLinkedPurchaseIdAsync(
        int organizationId,
        string providerCode,
        string providerDocumentId,
        CancellationToken ct = default) =>
        (from document in context.EdoDocuments.IgnoreQueryFilters().AsNoTracking()
         join purchase in context.PurchaseDocs.IgnoreQueryFilters().AsNoTracking()
             on document.InternalDocumentId equals purchase.Id
         where document.OrganizationId == organizationId
             && document.Provider == providerCode
             && document.ProviderDocumentId == providerDocumentId
             && document.InternalDocumentType.ToUpper() == "PURCHASE"
             && purchase.OrganizationId == organizationId
         select (long?)purchase.Id).SingleOrDefaultAsync(ct);

    public Task<EdoImportMappingResolutionDto> ResolveMappingAsync(
        int organizationId,
        string providerCode,
        string sellerTin,
        DateOnly documentDate,
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines,
        CancellationToken ct = default) =>
        ResolveMappingCoreAsync(
            organizationId,
            providerCode,
            sellerTin,
            documentDate,
            lines,
            selection: null,
            ct);

    public Task<EdoImportMappingResolutionDto> ResolveSelectedMappingAsync(
        int organizationId,
        string sellerTin,
        DateOnly documentDate,
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines,
        EdoImportMappingSelectionDto selection,
        CancellationToken ct = default) =>
        ResolveMappingCoreAsync(
            organizationId,
            selection.ProviderCode,
            sellerTin,
            documentDate,
            lines,
            selection,
            ct);

    private async Task<EdoImportMappingResolutionDto> ResolveMappingCoreAsync(
        int organizationId,
        string? providerCode,
        string sellerTin,
        DateOnly documentDate,
        IReadOnlyCollection<EdoHistoricalDocumentLineDto> lines,
        EdoImportMappingSelectionDto? selection,
        CancellationToken ct)
    {
        var validatedLines = lines.ToArray();
        var lineIntegrityFailure = EdoHistoricalSourceSupport.ValidateLineIntegrity(validatedLines);
        if (lineIntegrityFailure is not null)
            throw new EdoHistoricalMappingException(lineIntegrityFailure);

        var allowAutomaticFallback = selection is null
            || selection.UseAutomaticFallbackForMissingSelections;
        var selectedCounterpartyId = selection?.CounterpartyId;
        var counterparties = await context.CounterpartyCards.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.OrganizationId == organizationId
                && item.Inn == sellerTin
                && item.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && (selectedCounterpartyId.HasValue
                    ? item.Id == selectedCounterpartyId.Value
                    : allowAutomaticFallback))
            .Select(item => item.Id)
            .Take(2)
            .ToListAsync(ct);
        var counterpartyId = counterparties.Count == 1 ? counterparties[0] : (int?)null;

        long? contractId = null;
        if (counterpartyId.HasValue)
        {
            var date = documentDate.ToDateTime(TimeOnly.MinValue);
            var applicableContracts = context.Contracts.IgnoreQueryFilters().AsNoTracking()
                .Where(item => item.OrganizationId == organizationId
                    && item.CounterpartyId == counterpartyId.Value
                    && item.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                    && item.StartDate.HasValue
                    && item.StartDate.Value <= date
                    && (!item.EndDate.HasValue || item.EndDate.Value >= date));
            if (selection?.ContractId.HasValue == true)
            {
                contractId = await applicableContracts
                    .Where(item => item.Id == selection.ContractId.Value)
                    .Select(item => (long?)item.Id)
                    .SingleOrDefaultAsync(ct);
            }
            else if (allowAutomaticFallback)
            {
                var hasProviderIdentity = !string.IsNullOrWhiteSpace(selection?.ProviderCode)
                    && !string.IsNullOrWhiteSpace(selection.ProviderContractNumber)
                    && selection.ProviderContractDate.HasValue;
                var exactContracts = hasProviderIdentity
                    ? await applicableContracts.Where(item =>
                            item.ProviderCode == selection!.ProviderCode
                            && item.ProviderContractNumber == selection.ProviderContractNumber
                            && item.ProviderContractDate == selection.ProviderContractDate)
                        .Select(item => item.Id)
                        .Take(2)
                        .ToListAsync(ct)
                    : [];
                if (exactContracts.Count == 1)
                {
                    contractId = exactContracts[0];
                }
                else if (exactContracts.Count == 0)
                {
                    var contracts = await applicableContracts.Select(item => item.Id)
                        .Take(2)
                        .ToListAsync(ct);
                    contractId = contracts.Count == 1 ? contracts[0] : null;
                }
            }
        }

        var catalogCodes = validatedLines.Select(line => line.CatalogCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var lineIdentityHashes = validatedLines
            .Select(line => EdoProviderProductIdentity.Create(
                providerCode,
                line.CatalogCode,
                line.PackageCode,
                line.CatalogName,
                line.IsService))
            .Where(hash => hash is not null)
            .Select(hash => hash!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var durableMappings = await context.EdoProviderProductMappings.IgnoreQueryFilters().AsNoTracking()
            .Where(mapping => mapping.OrganizationId == organizationId
                && lineIdentityHashes.Contains(mapping.IdentityHash))
            .Select(mapping => new { mapping.IdentityHash, mapping.ProductId })
            .ToDictionaryAsync(mapping => mapping.IdentityHash, mapping => mapping.ProductId, ct);
        var products = await context.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.OrganizationId == organizationId
                && product.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && product.IsPurchased
                && product.Mxik != null
                && catalogCodes.Contains(product.Mxik))
            .Select(product => new
            {
                product.Id,
                product.Mxik,
                product.UnitId,
                product.IsService,
                product.DefaultVatRateId,
                product.IsPieceTracked
            })
            .ToListAsync(ct);
        var productUnitIds = products.Select(product => product.UnitId).Distinct().ToArray();
        var activeUnitIds = (await context.Units.IgnoreQueryFilters().AsNoTracking()
                .Where(unit => productUnitIds.Contains(unit.Id)
                    && unit.StateId == SharedKernel.Constants.StateIdConst.ACTIVE)
                .Select(unit => unit.Id)
                .ToListAsync(ct))
            .ToHashSet();
        var vatRates = await context.VatRates.IgnoreQueryFilters().AsNoTracking()
            .Where(vat => vat.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                && (!vat.EffectiveFrom.HasValue || vat.EffectiveFrom.Value <= documentDate)
                && (!vat.EffectiveTo.HasValue || vat.EffectiveTo.Value >= documentDate))
            .Select(vat => new { vat.Id, vat.Rate })
            .ToListAsync(ct);
        var requestedAccountIds = selection?.Lines.Values
            .SelectMany(line => new[] { line.DebitAccountId, line.VatAccountId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray() ?? [];
        var validAccountIds = requestedAccountIds.Length == 0
            ? new HashSet<int>()
            : (await context.ChartAccounts.IgnoreQueryFilters().AsNoTracking()
                .Where(account => account.OrganizationId == organizationId
                    && account.StateId == SharedKernel.Constants.StateIdConst.ACTIVE
                    && requestedAccountIds.Contains(account.Id))
                .Select(account => account.Id)
                .ToListAsync(ct))
                .ToHashSet();
        var lineMappings = validatedLines.ToDictionary(line => line.Number, line =>
        {
            var matchedProducts = products.Where(product =>
                string.Equals(product.Mxik, line.CatalogCode, StringComparison.Ordinal)).ToArray();
            EdoImportLineMappingSelectionDto? requestedLine = null;
            if (selection is not null)
                selection.Lines.TryGetValue(line.Number, out requestedLine);
            var matchedVatRates = line.VatRate.HasValue
                ? vatRates.Where(vat => vat.Rate == line.VatRate.Value).ToArray()
                : [];
            var identityHash = EdoProviderProductIdentity.Create(
                providerCode,
                line.CatalogCode,
                line.PackageCode,
                line.CatalogName,
                line.IsService);
            var durableProductId = identityHash is not null
                && durableMappings.TryGetValue(identityHash, out var mappedProductId)
                    ? mappedProductId
                    : (int?)null;
            var hasDurableMapping = durableProductId.HasValue;
            var product = requestedLine?.ProductId.HasValue == true
                ? matchedProducts.SingleOrDefault(item =>
                    item.Id == requestedLine.ProductId.Value
                    && (item.IsService == line.IsService
                        || hasDurableMapping && item.Id == durableProductId))
                : hasDurableMapping
                    ? matchedProducts.SingleOrDefault(item =>
                        item.Id == durableProductId.GetValueOrDefault())
                : allowAutomaticFallback
                    && !EdoProviderProductIdentity.RequiresExplicitMapping(line.CatalogCode)
                    && matchedProducts.Length == 1
                    && matchedProducts[0].IsService == line.IsService
                    && (!matchedProducts[0].DefaultVatRateId.HasValue
                        || matchedVatRates.Length == 1
                            && matchedProducts[0].DefaultVatRateId == matchedVatRates[0].Id)
                        ? matchedProducts[0]
                        : null;
            var vatRateId = requestedLine?.VatRateId.HasValue == true
                ? matchedVatRates.SingleOrDefault(vat => vat.Id == requestedLine.VatRateId.Value)?.Id
                : allowAutomaticFallback && matchedVatRates.Length == 1
                    ? matchedVatRates[0].Id
                    : (short?)null;
            var unitId = product is not null
                && activeUnitIds.Contains(product.UnitId)
                && (requestedLine?.UnitId == product.UnitId
                    || allowAutomaticFallback && requestedLine?.UnitId is null)
                    ? (short?)product.UnitId
                    : (short?)null;
            return new EdoImportLineMappingResolutionDto
            {
                ProductId = product?.Id,
                UnitId = unitId,
                VatRateId = vatRateId,
                DebitAccountId = requestedLine?.DebitAccountId is { } debitAccountId
                    && validAccountIds.Contains(debitAccountId)
                        ? debitAccountId
                        : null,
                VatAccountId = requestedLine?.VatAccountId is { } vatAccountId
                    && validAccountIds.Contains(vatAccountId)
                        ? vatAccountId
                        : null,
                IsPieceTracked = product?.IsPieceTracked == true,
                IsService = product?.IsService == true
            };
        });

        var defaults = await context.OrganizationDefaults.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(item => item.OrganizationId == organizationId, ct);
        var requestedWarehouseId = selection?.WarehouseId
            ?? (allowAutomaticFallback ? defaults?.WarehouseId : null);
        var warehouseId = defaults?.WarehouseId.HasValue == true
            && requestedWarehouseId == defaults.WarehouseId
            && await context.Warehouses.IgnoreQueryFilters().AnyAsync(item =>
                item.Id == requestedWarehouseId!.Value
                && item.OrganizationId == organizationId
                && item.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct)
                ? requestedWarehouseId
                : null;
        var config = await context.OrganizationConfigs.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(item => item.OrganizationId == organizationId, ct);
        var requestedCurrencyId = selection?.CurrencyId
            ?? (allowAutomaticFallback ? config?.BaseCurrencyId : null);
        var currencyId = config?.BaseCurrencyId.HasValue == true
            && requestedCurrencyId == config.BaseCurrencyId
            && await context.Currencies.IgnoreQueryFilters().AnyAsync(item =>
                item.Id == requestedCurrencyId!.Value
                && item.StateId == SharedKernel.Constants.StateIdConst.ACTIVE, ct)
                ? requestedCurrencyId
                : null;

        var markings = validatedLines
            .Where(line => lineMappings.GetValueOrDefault(line.Number)?.IsPieceTracked == true)
            .SelectMany(line => line.MarkingNumbers)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var usedMarkings = await context.ProductTables.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.Product.OrganizationId == organizationId
                && item.MarkingNumber != null
                && markings.Contains(item.MarkingNumber))
            .Select(item => item.MarkingNumber!)
            .ToListAsync(ct);
        var usedMarkingSet = usedMarkings.ToHashSet(StringComparer.Ordinal);
        var markingOwners = await (
                from purchaseTable in context.PurchaseDocTables.IgnoreQueryFilters().AsNoTracking()
                join purchaseLine in context.PurchaseDocProducts.IgnoreQueryFilters().AsNoTracking()
                    on purchaseTable.OwnerId equals purchaseLine.Id
                join purchase in context.PurchaseDocs.IgnoreQueryFilters().AsNoTracking()
                    on purchaseLine.OwnerId equals purchase.Id
                join productTable in context.ProductTables.IgnoreQueryFilters().AsNoTracking()
                    on purchaseTable.ProductTableId equals productTable.Id
                join product in context.Products.IgnoreQueryFilters().AsNoTracking()
                    on productTable.ProductId equals product.Id
                where purchase.OrganizationId == organizationId
                    && product.OrganizationId == organizationId
                    && productTable.MarkingNumber != null
                    && markings.Contains(productTable.MarkingNumber)
                select new { Marking = productTable.MarkingNumber!, PurchaseId = purchase.Id })
            .ToListAsync(ct);
        var ownersByMarking = markingOwners.GroupBy(item => item.Marking, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.PurchaseId).Distinct().ToArray(),
                StringComparer.Ordinal);
        var singleOwnerIds = markings.Length > 0
            && markings.All(marking => usedMarkingSet.Contains(marking)
                && ownersByMarking.TryGetValue(marking, out var owners)
                && owners.Length == 1)
                ? markings.Select(marking => ownersByMarking[marking][0]).Distinct().ToArray()
                : [];
        var existingPurchaseIdForAllMarkings = singleOwnerIds.Length == 1
            ? singleOwnerIds[0]
            : (long?)null;
        var hasConflictingMarkingUsage = usedMarkingSet.Count > 0
            && !existingPurchaseIdForAllMarkings.HasValue;

        return new EdoImportMappingResolutionDto
        {
            CounterpartyId = counterpartyId,
            ContractId = contractId,
            CurrencyId = currencyId,
            WarehouseId = warehouseId,
            Lines = lineMappings,
            PreviouslyUsedMarkings = usedMarkingSet,
            ExistingPurchaseIdForAllMarkings = existingPurchaseIdForAllMarkings,
            HasConflictingMarkingUsage = hasConflictingMarkingUsage
        };
    }

    public async Task<EdoImportJobMappingCountsDto> GetJobMappingCountsAsync(
        int organizationId,
        long jobId,
        CancellationToken ct = default)
    {
        var statuses = await context.EdoImportCandidates.IgnoreQueryFilters().AsNoTracking()
            .Where(candidate => candidate.OrganizationId == organizationId
                && candidate.JobId == jobId)
            .GroupBy(candidate => candidate.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(ct);
        var counts = statuses.ToDictionary(item => item.Status, item => item.Count, StringComparer.Ordinal);
        return new EdoImportJobMappingCountsDto(
            counts.GetValueOrDefault(EdoImportCandidateStatus.Ready),
            counts.GetValueOrDefault(EdoImportCandidateStatus.MappingRequired),
            counts.GetValueOrDefault(EdoImportCandidateStatus.Duplicate)
                + counts.GetValueOrDefault(EdoImportCandidateStatus.PossibleDuplicate));
    }

    public async Task AddCandidateGraphAsync(
        EdoImportCandidate candidate,
        CancellationToken ct = default)
    {
        var scopedCheckpointExists = await context.EdoImportJobProviders.IgnoreQueryFilters().AnyAsync(provider =>
            provider.JobId == candidate.JobId
            && provider.ProviderCode == candidate.ProviderCode
            && provider.Job.OrganizationId == candidate.OrganizationId, ct);
        if (!scopedCheckpointExists)
            throw new InvalidOperationException("The EDO import candidate is outside the job organization/provider scope.");

        context.EdoImportCandidates.Add(candidate);
        await SaveUniqueAsync(ct);
    }

    public async Task AddJobAsync(EdoImportJob job, CancellationToken ct = default)
    {
        if (EdoImportJobStatus.IsActive(job.Status)
            && await context.EdoImportJobs.IgnoreQueryFilters().AnyAsync(existing =>
                existing.OrganizationId == job.OrganizationId
                && EdoImportJobStatus.ActiveValues.Contains(existing.Status), ct))
        {
            throw Duplicate("An active EDO historical import job already exists for this organization.");
        }

        context.EdoImportJobs.Add(job);
        await SaveUniqueAsync(ct);
    }

    public async Task AddProviderAsync(
        int organizationId,
        EdoImportJobProvider provider,
        CancellationToken ct = default)
    {
        var scopedJobExists = await context.EdoImportJobs.IgnoreQueryFilters().AnyAsync(job =>
            job.Id == provider.JobId && job.OrganizationId == organizationId, ct);

        if (!scopedJobExists)
            throw new InvalidOperationException("The EDO import provider checkpoint is outside the job organization scope.");

        if (await context.EdoImportJobProviders.IgnoreQueryFilters().AnyAsync(existing =>
            existing.JobId == provider.JobId
            && existing.ProviderCode == provider.ProviderCode, ct))
        {
            throw Duplicate("An EDO import provider checkpoint already exists for this job and provider.");
        }

        context.EdoImportJobProviders.Add(provider);
        await SaveUniqueAsync(ct);
    }

    public async Task AddCandidateAsync(EdoImportCandidate candidate, CancellationToken ct = default)
    {
        var scopedCheckpointExists = await context.EdoImportJobProviders.IgnoreQueryFilters().AnyAsync(provider =>
            provider.JobId == candidate.JobId
            && provider.ProviderCode == candidate.ProviderCode
            && provider.Job.OrganizationId == candidate.OrganizationId, ct);

        if (!scopedCheckpointExists)
            throw new InvalidOperationException("The EDO import candidate is outside the job organization/provider scope.");

        if (await context.EdoImportCandidates.IgnoreQueryFilters().AnyAsync(existing =>
            existing.JobId == candidate.JobId
            && existing.ProviderCode == candidate.ProviderCode
            && existing.ProviderDocumentId == candidate.ProviderDocumentId, ct))
        {
            throw Duplicate("An EDO import candidate already exists for this job, provider, and provider document ID.");
        }

        context.EdoImportCandidates.Add(candidate);
        await SaveUniqueAsync(ct);
    }

    public async Task AddLineAsync(
        int organizationId,
        EdoImportCandidateLine line,
        CancellationToken ct = default)
    {
        var scopedCandidateExists = await context.EdoImportCandidates.IgnoreQueryFilters().AnyAsync(candidate =>
            candidate.Id == line.CandidateId && candidate.OrganizationId == organizationId, ct);

        if (!scopedCandidateExists)
            throw new InvalidOperationException("The EDO import candidate line is outside the organization scope.");

        if (await context.EdoImportCandidateLines.IgnoreQueryFilters().AnyAsync(existing =>
            existing.CandidateId == line.CandidateId
            && existing.ProviderLineNumber == line.ProviderLineNumber, ct))
        {
            throw Duplicate("An EDO import candidate line already exists for this provider line number.");
        }

        context.EdoImportCandidateLines.Add(line);
        await SaveUniqueAsync(ct);
    }

    public async Task AddMarkingAsync(
        int organizationId,
        EdoImportCandidateMarking marking,
        CancellationToken ct = default)
    {
        var scopedLineExists = await context.EdoImportCandidateLines.IgnoreQueryFilters().AnyAsync(line =>
            line.Id == marking.CandidateLineId
            && line.Candidate.OrganizationId == organizationId, ct);

        if (!scopedLineExists)
            throw new InvalidOperationException("The EDO import candidate marking is outside the organization scope.");

        if (await context.EdoImportCandidateMarkings.IgnoreQueryFilters().AnyAsync(existing =>
            existing.CandidateLineId == marking.CandidateLineId
            && existing.MarkingNumber == marking.MarkingNumber, ct))
        {
            throw Duplicate("An EDO import candidate marking already exists for this line.");
        }

        context.EdoImportCandidateMarkings.Add(marking);
        await SaveUniqueAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    private async Task SaveUniqueAsync(CancellationToken ct)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("The EDO historical import record conflicts with existing data.", exception);
        }
    }

    private static UniqueConstraintViolationException Duplicate(
        string message,
        Exception? innerException = null) => new(message, innerException);
}
