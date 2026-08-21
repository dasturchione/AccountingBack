using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features.SaleDocs.EdoSalePreflight;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Exceptions;

namespace Application.Features.SaleDocs;

public sealed class EdoSalePreflightService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoInboxService edoInboxService,
    IQueryBuilder queryBuilder,
    IQueryRepository<CounterpartyCard> counterpartyQuery,
    IQueryRepository<Contract> contractQuery,
    IQueryRepository<Product> productQuery,
    IQueryRepository<Warehouse> warehouseQuery,
    IQueryRepository<Currency> currencyQuery,
    IQueryRepository<VatRate> vatRateQuery,
    IWarehouseInventoryService warehouseInventoryService,
    IQueryRepository<EdoDocument> edoDocumentQuery,
    IQueryRepository<SaleDoc> saleDocQuery) : IEdoSalePreflightService
{
    private const int ProviderPageSize = 100;
    private const int MaxPlanPages = 100;
    private const string SourceLinkRequired = "SALE_SOURCE_LINK_REQUIRED";
    private const string SourceLinkInvalid = "SALE_SOURCE_LINK_INVALID";
    private const string SaleAlreadyLinked = "SALE_DOCUMENT_ALREADY_LINKED";

    public async Task<EdoSalePreflightPlanDto> GetPlanAsync(CancellationToken ct = default)
    {
        var organizationId = userContext.OrganizationId
            ?? throw new EdoOrganizationScopeRequiredException();

        var activeProvider = await activeProviderResolver.GetActiveProviderCodeAsync(ct);
        if (activeProvider != EdoProviderCode.EDOCS)
        {
            return BuildPlan(
                [],
                ["ACTIVE_PROVIDER_NOT_EDOCS"]);
        }

        var (listedDocuments, hasDuplicateProviderIds, hasIncompletePagination) =
            await LoadSignedOutboxDocumentsAsync(ct);
        var masterData = await LoadMasterDataAsync(organizationId, listedDocuments, ct);
        var candidates = new List<EdoSalePreflightCandidateDto>(listedDocuments.Count);

        foreach (var listedDocument in listedDocuments
                     .OrderBy(x => x.ProviderDocumentId ?? string.Empty, StringComparer.Ordinal))
        {
            candidates.Add(await BuildCandidateAsync(organizationId, listedDocument, masterData, ct));
        }

        return BuildPlan(
            candidates,
            new[]
            {
                hasDuplicateProviderIds ? "DUPLICATE_PROVIDER_DOCUMENT_ID" : null,
                hasIncompletePagination ? "PROVIDER_PAGINATION_INCOMPLETE" : null
            }
            .Where(x => x is not null)
            .Cast<string>()
            .ToArray());
    }

    private async Task<EdoSalePreflightCandidateDto> BuildCandidateAsync(
        int organizationId,
        EdoDocumentDto listedDocument,
        MasterData masterData,
        CancellationToken ct)
    {
        var codes = new HashSet<string>(StringComparer.Ordinal);
        EdoOutboxProviderDocumentDetailDto? detail = null;

        if (string.IsNullOrWhiteSpace(listedDocument.ProviderDocumentId))
        {
            codes.Add("PROVIDER_DOCUMENT_ID_REQUIRED");
        }
        else
        {
            try
            {
                detail = await edoInboxService.GetOutboxProviderDocumentDetailsAsync(
                    listedDocument.ProviderDocumentId,
                    ct);
            }
            catch (EdoOutboxProviderDocumentException exception)
            {
                codes.Add(exception.Code);
            }
        }

        if (detail is null)
        {
            var listedProviderDocumentId = listedDocument.ProviderDocumentId?.Trim() ?? string.Empty;
            if (masterData.ExistingSaleDocumentIds.TryGetValue(
                    listedProviderDocumentId,
                    out var existingSaleDocId))
            {
                codes.Add(SaleAlreadyLinked);
                return new EdoSalePreflightCandidateDto
                {
                    ProviderDocumentId = listedDocument.ProviderDocumentId,
                    DocumentType = listedDocument.DocumentType,
                    DocumentNumber = listedDocument.DocumentNumber,
                    DocumentDate = listedDocument.DocumentDate,
                    Status = "ALREADY_IMPORTED",
                    ExistingSaleDocId = existingSaleDocId,
                    TotalAmount = listedDocument.TotalAmount,
                    SafeErrorCodes = codes.OrderBy(x => x, StringComparer.Ordinal).ToArray()
                };
            }

            codes.Add(masterData.InvalidSaleLinkProviderIds.Contains(listedProviderDocumentId)
                ? SourceLinkInvalid
                : SourceLinkRequired);

            return new EdoSalePreflightCandidateDto
            {
                ProviderDocumentId = listedDocument.ProviderDocumentId,
                DocumentType = listedDocument.DocumentType,
                DocumentNumber = listedDocument.DocumentNumber,
                DocumentDate = listedDocument.DocumentDate,
                Status = "BLOCKED",
                TotalAmount = listedDocument.TotalAmount,
                SafeErrorCodes = codes.OrderBy(x => x, StringComparer.Ordinal).ToArray()
            };
        }

        var counterparty = ResolveCounterparty(detail, masterData.Counterparties, codes);
        var contract = ResolveContract(organizationId, detail, counterparty.SelectedId, masterData.Contracts, codes);
        var currency = ResolveCurrency(detail, masterData.Currencies, codes);
        var warehouse = ResolveWarehouse(masterData.Warehouses, codes);
        var lines = detail.Lines
            .OrderBy(x => x.Number)
            .Select(line => BuildLine(organizationId, line, detail.DocumentDate, masterData, codes))
            .ToArray();

        var marking = ResolveDocumentMarking(lines, codes);
        var providerDocumentId = detail.ProviderDocumentId;
        var hasExistingSaleLink = masterData.ExistingSaleDocumentIds.TryGetValue(
            providerDocumentId,
            out var linkedSaleDocId);
        codes.Add(hasExistingSaleLink
            ? SaleAlreadyLinked
            : masterData.InvalidSaleLinkProviderIds.Contains(providerDocumentId)
                ? SourceLinkInvalid
                : SourceLinkRequired);

        var status = hasExistingSaleLink
            ? "ALREADY_IMPORTED"
            : codes.Any(x => x.EndsWith("REQUIRED", StringComparison.Ordinal)
                                    || x.Contains("BLOCKED", StringComparison.Ordinal))
            ? "BLOCKED"
            : codes.Count > 0 ? "REQUIRES_SELECTION" : "READY";

        return new EdoSalePreflightCandidateDto
        {
            ProviderDocumentId = detail.ProviderDocumentId,
            DocumentType = detail.DocumentType,
            DocumentNumber = detail.DocumentNumber,
            DocumentDate = detail.DocumentDate,
            Status = status,
            ExistingSaleDocId = hasExistingSaleLink ? linkedSaleDocId : null,
            NetAmount = detail.NetAmount,
            VatAmount = detail.VatAmount,
            TotalAmount = detail.TotalAmount,
            CurrencyCode = detail.CurrencyCode,
            Counterparty = counterparty,
            Contract = contract,
            Warehouse = warehouse,
            Currency = currency,
            Marking = marking,
            Lines = lines,
            SafeErrorCodes = codes.OrderBy(x => x, StringComparer.Ordinal).ToArray()
        };
    }

    private static EdoSalePreflightLineDto BuildLine(
        int organizationId,
        EdoOutboxProviderDocumentLineDto line,
        DateOnly documentDate,
        MasterData masterData,
        ISet<string> documentCodes)
    {
        var lineCodes = new List<string>();
        var eligibility = EdoSaleProductEligibility.Resolve(
            organizationId,
            line.ProviderProductCode,
            line.Quantity,
            masterData.Products,
            masterData.AvailableQuantityByProductId,
            masterData.InventoryAvailable);
        var product = eligibility.Product;
        var productStatus = product is not null
            ? Mapping("READY", selectedId: product.Id)
            : Mapping(
                eligibility.Status,
                eligibility.SafeErrorCode,
                eligibility.SafeErrorCode == "SALE_PRODUCT_NOT_ENABLED"
                    ? "The matched product is not enabled for sale."
                    : eligibility.SafeErrorCode == "SALE_PRODUCT_STOCK_MAPPING_REQUIRED"
                        ? "Insufficient warehouse availability for the requested sale quantity."
                        : "No single eligible sold product matches the provider product code.");
        if (productStatus.SafeErrorCode is not null)
            lineCodes.Add(productStatus.SafeErrorCode);

        var unitStatus = product is null
            ? Mapping("BLOCKED", "UNIT_REQUIRES_PRODUCT_SELECTION", "Unit is resolved only from the selected product.")
            : Mapping("READY", selectedId: product.UnitId);
        if (unitStatus.SafeErrorCode is not null)
            lineCodes.Add(unitStatus.SafeErrorCode);

        var vatRates = line.VatRate.HasValue
            ? masterData.VatRates.Where(x => x.Rate == line.VatRate.Value
                && (x.EffectiveFrom is null || x.EffectiveFrom <= documentDate)
                && (x.EffectiveTo is null || x.EffectiveTo >= documentDate)).ToArray()
            : [];
        var vatStatus = !line.VatRate.HasValue
            ? Mapping("BLOCKED", "VAT_RATE_REQUIRED", "Provider VAT rate is missing.")
            : vatRates.Length == 1
                ? Mapping("READY", selectedId: vatRates[0].Id)
                : vatRates.Length == 0
                    ? Mapping("BLOCKED", "VAT_RATE_MAPPING_REQUIRED", "No active VAT rate matches the provider rate.")
                    : Mapping("REQUIRES_SELECTION", "VAT_RATE_MAPPING_AMBIGUOUS", "More than one active VAT rate matches the provider rate.");
        if (vatStatus.SafeErrorCode is not null)
            lineCodes.Add(vatStatus.SafeErrorCode);

        var marking = new EdoSaleMarkingStatusDto
        {
            HasMarkings = line.Marking.HasMarkings,
            MarkingCount = line.Marking.Count,
            MarkingRequired = line.Marking.HasMarkings,
            Status = line.Marking.HasMarkings
                ? product is null
                    ? "BLOCKED"
                    : "REQUIRES_SELECTION"
                : "NOT_REQUIRED",
            SafeErrorCode = line.Marking.HasMarkings
                ? product is null
                    ? "MARKING_PRODUCT_MAPPING_REQUIRED"
                    : "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN"
                : null
        };
        if (marking.SafeErrorCode is not null)
            lineCodes.Add(marking.SafeErrorCode);

        var costStatus = Mapping(
            "REQUIRES_SELECTION",
            "SALE_COST_PRICE_SOURCE_REQUIRED",
            "SaleDoc requires an explicit cost price source; the provider invoice does not supply one.");
        lineCodes.Add(costStatus.SafeErrorCode!);

        var productTableStatus = product is null
            ? Mapping("BLOCKED", "PRODUCT_TABLE_REQUIRES_PRODUCT_SELECTION", "Inventory selection requires a selected product.")
            : product.IsService || !product.IsPieceTracked
                ? Mapping("NOT_APPLICABLE", description: "ProductTable is required only for piece-tracked goods.")
                : Mapping("REQUIRES_SELECTION", "PRODUCT_TABLE_SELECTION_REQUIRED", "Sale inventory requires explicit ProductTable selection.");
        if (productTableStatus.SafeErrorCode is not null)
            lineCodes.Add(productTableStatus.SafeErrorCode);

        foreach (var code in lineCodes)
            documentCodes.Add(code);

        return new EdoSalePreflightLineDto
        {
            Number = line.Number,
            ProviderProductCode = line.ProviderProductCode,
            ProviderProductName = line.ProviderProductName,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            NetAmount = line.NetAmount,
            VatRate = line.VatRate,
            VatAmount = line.VatAmount,
            TotalWithVat = line.TotalWithVat,
            Product = productStatus,
            ProductTable = productTableStatus,
            Unit = unitStatus,
            Vat = vatStatus,
            CostPrice = costStatus,
            Marking = marking
        };
    }

    private static EdoSaleMappingStatusDto ResolveCounterparty(
        EdoOutboxProviderDocumentDetailDto detail,
        IReadOnlyCollection<CounterpartyCard> counterparties,
        ISet<string> codes)
    {
        var tin = detail.Buyer.TaxIdentifier?.Trim();
        if (string.IsNullOrWhiteSpace(tin))
        {
            codes.Add("COUNTERPARTY_BUYER_TIN_REQUIRED");
            return Mapping("BLOCKED", "COUNTERPARTY_BUYER_TIN_REQUIRED", "Buyer TIN is required for exact customer mapping.");
        }

        var matches = counterparties.Where(x => string.Equals(x.Inn?.Trim(), tin, StringComparison.Ordinal)).ToArray();
        return matches.Length switch
        {
            1 => Mapping("READY", selectedId: matches[0].Id),
            0 => AddCode(codes, Mapping("REQUIRES_SELECTION", "COUNTERPARTY_MAPPING_REQUIRED", "No active customer matches the buyer TIN.")),
            _ => AddCode(codes, Mapping("REQUIRES_SELECTION", "COUNTERPARTY_MAPPING_AMBIGUOUS", "More than one active customer matches the buyer TIN."))
        };
    }

    private static EdoSaleMappingStatusDto ResolveContract(
        int organizationId,
        EdoOutboxProviderDocumentDetailDto detail,
        long? counterpartyId,
        IReadOnlyCollection<Contract> contracts,
        ISet<string> codes)
        => EdoSaleContractResolver.Resolve(organizationId, detail, counterpartyId, contracts, codes);

    private static EdoSaleMappingStatusDto ResolveCurrency(
        EdoOutboxProviderDocumentDetailDto detail,
        IReadOnlyCollection<Currency> currencies,
        ISet<string> codes)
    {
        if (string.IsNullOrWhiteSpace(detail.CurrencyCode))
            return AddCode(codes, Mapping("BLOCKED", "CURRENCY_REQUIRED", "Provider currency is absent and cannot be guessed."));

        var matches = currencies.Where(x => string.Equals(x.Code, detail.CurrencyCode, StringComparison.OrdinalIgnoreCase)).ToArray();
        return matches.Length switch
        {
            1 => Mapping("READY", selectedId: matches[0].Id),
            0 => AddCode(codes, Mapping("BLOCKED", "CURRENCY_MAPPING_REQUIRED", "No active currency matches the provider currency.")),
            _ => AddCode(codes, Mapping("REQUIRES_SELECTION", "CURRENCY_MAPPING_AMBIGUOUS", "More than one active currency matches the provider currency."))
        };
    }

    private static EdoSaleMappingStatusDto ResolveWarehouse(
        IReadOnlyCollection<Warehouse> warehouses,
        ISet<string> codes)
    {
        var main = warehouses.Where(x => x.IsMain).ToArray();
        return main.Length switch
        {
            1 => Mapping("READY", selectedId: main[0].Id),
            0 => AddCode(codes, Mapping("BLOCKED", "WAREHOUSE_REQUIRED", "No single active main warehouse is configured.")),
            _ => AddCode(codes, Mapping("REQUIRES_SELECTION", "WAREHOUSE_SELECTION_REQUIRED", "More than one active main warehouse is configured."))
        };
    }

    private static EdoSaleMarkingStatusDto ResolveDocumentMarking(
        IReadOnlyCollection<EdoSalePreflightLineDto> lines,
        ISet<string> codes)
    {
        var marked = lines.Where(x => x.Marking.HasMarkings).ToArray();
        var result = new EdoSaleMarkingStatusDto
        {
            HasMarkings = marked.Length > 0,
            MarkingCount = marked.Sum(x => x.Marking.MarkingCount),
            MarkingRequired = marked.Length > 0,
            Status = marked.Length == 0 ? "NOT_REQUIRED" : "REQUIRES_SELECTION",
            SafeErrorCode = marked.Length == 0 ? null : "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN"
        };
        if (result.SafeErrorCode is not null)
            codes.Add(result.SafeErrorCode);
        return result;
    }

    private async Task<MasterData> LoadMasterDataAsync(
        int organizationId,
        IReadOnlyCollection<EdoDocumentDto> listedDocuments,
        CancellationToken ct)
    {
        var counterparties = await counterpartyQuery.GetAllAsync(
            queryBuilder.For<CounterpartyCard>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE && x.IsCustomer)
                .Build(), ct);
        var contracts = await contractQuery.GetAllAsync(
            queryBuilder.For<Contract>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        var products = await productQuery.GetAllAsync(
            queryBuilder.For<Product>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        var warehouses = await warehouseQuery.GetAllAsync(
            queryBuilder.For<Warehouse>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        var currencies = await currencyQuery.GetAllAsync(
            queryBuilder.For<Currency>()
                .Where(x => x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        var vatRates = await vatRateQuery.GetAllAsync(
            queryBuilder.For<VatRate>()
                .Where(x => x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);

        var availableQuantityByProductId = new Dictionary<int, decimal>();
        var inventoryAvailable = false;
        var mainWarehouses = warehouses.Where(x => x.IsMain).ToArray();
        if (mainWarehouses.Length == 1 && products.Count > 0)
        {
            var inventoryResult = await warehouseInventoryService.GetWarehouseProductsAsync(
                new WarehouseProductFilter
                {
                    WarehouseId = mainWarehouses[0].Id,
                    ProductIds = products.Select(x => x.Id).ToArray()
                },
                ct);
            if (inventoryResult.IsSuccess)
            {
                inventoryAvailable = true;
                availableQuantityByProductId = inventoryResult.Value
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(x => x.Key, x => x.First().AvailableQuantity);
            }
        }

        var providerDocumentIds = listedDocuments
            .Select(x => x.ProviderDocumentId?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var edoLinks = providerDocumentIds.Length == 0
            ? []
            : await edoDocumentQuery.GetAllAsync(
                queryBuilder.For<EdoDocument>()
                    .Where(x => x.OrganizationId == organizationId
                        && x.Provider == EdoProviderCode.EDOCS.ToString()
                        && x.ProviderDocumentId != null
                        && providerDocumentIds.Contains(x.ProviderDocumentId)
                        && (x.InternalDocumentType == "sale_doc"
                            || x.InternalDocumentType == "SALE_DOC"
                            || x.InternalDocumentType == "SALE"))
                    .Build(), ct);

        var saleIds = edoLinks
            .Where(x => x.InternalDocumentId > 0)
            .Select(x => x.InternalDocumentId)
            .Distinct()
            .ToArray();
        var sales = saleIds.Length == 0
            ? []
            : await saleDocQuery.GetAllAsync(
                queryBuilder.For<SaleDoc>()
                    .Where(x => x.OrganizationId == organizationId && saleIds.Contains(x.Id))
                    .Build(), ct);
        var existingSaleIds = sales.Select(x => x.Id).ToHashSet();
        var existingSaleDocumentIds = edoLinks
            .Where(x => x.ProviderDocumentId is not null
                && x.InternalDocumentId > 0
                && existingSaleIds.Contains(x.InternalDocumentId))
            .GroupBy(x => x.ProviderDocumentId!.Trim(), StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First().InternalDocumentId, StringComparer.Ordinal);
        var invalidSaleLinkProviderIds = edoLinks
            .Where(x => x.ProviderDocumentId is not null
                && (x.InternalDocumentId <= 0 || !existingSaleIds.Contains(x.InternalDocumentId)))
            .Select(x => x.ProviderDocumentId!.Trim())
            .ToHashSet(StringComparer.Ordinal);

        return new MasterData(
            counterparties,
            contracts,
            products,
            warehouses,
            currencies,
            vatRates,
            availableQuantityByProductId,
            inventoryAvailable,
            existingSaleDocumentIds,
            invalidSaleLinkProviderIds);
    }

    private async Task<(
        IReadOnlyCollection<EdoDocumentDto> Documents,
        bool HasDuplicateProviderIds,
        bool HasIncompletePagination)> LoadSignedOutboxDocumentsAsync(CancellationToken ct)
    {
        var result = new List<EdoDocumentDto>();
        var page = 1;
        var hasIncompletePagination = false;
        while (page <= MaxPlanPages)
        {
            var response = await edoInboxService.ListDocumentsAsync(new EdoDocumentQueryDto
            {
                Scope = EdoDocumentQueryScope.OUTBOX,
                Page = page,
                Limit = ProviderPageSize,
                Category = EdoDocumentCategory.OUTBOX,
                Status = EdoDocumentStatusCode.SIGNED
            }, ct);
            result.AddRange(response.Items.Where(x => x.ProviderCode == EdoProviderCode.EDOCS
                && x.Direction == EdoDirection.OUTBOX
                && string.Equals(x.DocumentType, "FACTURA", StringComparison.OrdinalIgnoreCase)
                && x.Status.Code == EdoDocumentStatusCode.SIGNED));

            var hasNextPage = response.TotalPages.HasValue
                ? page < response.TotalPages.Value
                : response.HasNextPage == true;
            if (!hasNextPage || response.Items.Count == 0)
                break;

            if (page == MaxPlanPages)
            {
                hasIncompletePagination = true;
                break;
            }

            page++;
        }

        var hasDuplicateProviderIds = result
            .Where(x => !string.IsNullOrWhiteSpace(x.ProviderDocumentId))
            .GroupBy(x => x.ProviderDocumentId!, StringComparer.Ordinal)
            .Any(x => x.Count() > 1);
        var documents = result
            .GroupBy(x => x.ProviderDocumentId ?? string.Empty, StringComparer.Ordinal)
            .Select(x => x.First())
            .ToArray();
        return (documents, hasDuplicateProviderIds, hasIncompletePagination);
    }

    private static EdoSalePreflightPlanDto BuildPlan(
        IReadOnlyCollection<EdoSalePreflightCandidateDto> candidates,
        IReadOnlyCollection<string> planCodes)
    {
        var ordered = candidates.OrderBy(x => x.ProviderDocumentId ?? string.Empty, StringComparer.Ordinal).ToArray();
        var allCodes = ordered.SelectMany(x => x.SafeErrorCodes).Concat(planCodes).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var hash = EdoSalePreflightPlanHash.Compute(ordered, allCodes);

        return new EdoSalePreflightPlanDto
        {
            PlanHash = hash,
            SourceLinkStatus = "BLOCKED",
            TotalCandidates = ordered.Length,
            ReadyCount = ordered.Count(x => x.Status == "READY"),
            RequiresSelectionCount = ordered.Count(x => x.Status == "REQUIRES_SELECTION"),
            BlockedCount = ordered.Count(x => x.Status == "BLOCKED"),
            DuplicateCount = ordered.Count(x => x.Status is "DUPLICATE" or "ALREADY_IMPORTED"),
            SafeErrorCodes = allCodes,
            Candidates = ordered
        };
    }

    private static EdoSaleMappingStatusDto Mapping(
        string status,
        string? code = null,
        string? description = null,
        long? selectedId = null) => new()
        {
            Status = status,
            SafeErrorCode = code,
            Description = description,
            SelectedId = selectedId
        };

    private static EdoSaleMappingStatusDto AddCode(
        ISet<string> codes,
        EdoSaleMappingStatusDto mapping)
    {
        if (mapping.SafeErrorCode is not null)
            codes.Add(mapping.SafeErrorCode);
        return mapping;
    }

    private sealed record MasterData(
        IReadOnlyCollection<CounterpartyCard> Counterparties,
        IReadOnlyCollection<Contract> Contracts,
        IReadOnlyCollection<Product> Products,
        IReadOnlyCollection<Warehouse> Warehouses,
        IReadOnlyCollection<Currency> Currencies,
        IReadOnlyCollection<VatRate> VatRates,
        IReadOnlyDictionary<int, decimal> AvailableQuantityByProductId,
        bool InventoryAvailable,
        IReadOnlyDictionary<string, long> ExistingSaleDocumentIds,
        IReadOnlySet<string> InvalidSaleLinkProviderIds);
}
