using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features.DocumentNumbers;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public sealed class EdoSaleDraftApplyService(
    IUserContext userContext,
    IEdoSalePreflightService planService,
    IEdoInboxService edoInboxService,
    IEdoDocumentStore edoDocumentStore,
    IQueryBuilder queryBuilder,
    IQueryRepository<CounterpartyCard> counterpartyQuery,
    IQueryRepository<Contract> contractQuery,
    IQueryRepository<Product> productQuery,
    IQueryRepository<ProductTable> productTableQuery,
    IQueryRepository<Warehouse> warehouseQuery,
    IQueryRepository<Currency> currencyQuery,
    IQueryRepository<VatRate> vatRateQuery,
    IQueryRepository<SaleDoc> saleDocQuery,
    IWarehouseInventoryService warehouseInventoryService,
    ICommandRepository<SaleDoc> saleDocCommand,
    ICommandRepository<SaleDocProduct> saleDocProductCommand,
    ICommandRepository<SaleDocTable> saleDocTableCommand,
    IDocumentNumberService documentNumberService,
    ILogger<EdoSaleDraftApplyService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoSaleDraftApplyService
{
    private const string ProviderCode = "EDOCS";
    private const string InternalDocumentType = "sale_doc";
    private const string OperationType = "EDO_SALE_DRAFT_IMPORT";
    private const string ExplicitCostPriceSource = "EXPLICIT_USER";
    private const string WarehouseStockBatchCostPriceSource = "WAREHOUSE_STOCK_BATCH";
    private const string ProviderMarkingSource = "PROVIDER_SNAPSHOT";
    private const string NoMarkingSource = "NONE";

    public async Task<Result<EdoSaleDraftApplyResponseDto>> ApplyAsync(
        EdoSaleDraftApplyRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = userContext.OrganizationId;
        if (!organizationId.HasValue)
            return Failure("EDO_SALE_ORGANIZATION_REQUIRED");

        if (!request.Confirm)
            return Failure("EDO_SALE_CONFIRM_REQUIRED");

        if (request.ExpectedPlanHash.Length != 64
            || request.ExpectedPlanHash.Any(char.IsWhiteSpace))
            return Failure("EDO_SALE_PLAN_HASH_REQUIRED");

        if (request.Items.Count == 0)
            return Failure("EDO_SALE_SELECTIONS_REQUIRED");

        var requestedDocumentTypes = request.Items
            .Select(x => NormalizeDocumentType(x.DocumentType))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (requestedDocumentTypes.Length != 1)
            return Failure("EDO_SALE_DOCUMENT_TYPE_INVALID");

        var plan = await planService.GetPlanAsync(
            ct,
            request.Items.Any(x => x.AllowSentDocuments),
            requestedDocumentTypes[0]);
        if (!string.Equals(request.ExpectedPlanHash, plan.PlanHash, StringComparison.Ordinal))
        {
            var replay = await TryBuildIdempotentReplayAsync(organizationId.Value, request, plan, ct);
            if (replay is not null)
                return Result.Success(replay);

            return Failure("STALE_SALE_PREFLIGHT_PLAN", conflict: true);
        }

        return await ExecuteInTransactionAsync(
            nameof(ApplyAsync),
            () => ApplyCoreAsync(organizationId.Value, request, plan, ct),
            ct);
    }

    private async Task<EdoSaleDraftApplyResponseDto?> TryBuildIdempotentReplayAsync(
        int organizationId,
        EdoSaleDraftApplyRequestDto request,
        EdoSalePreflightPlanDto plan,
        CancellationToken ct)
    {
        var planIds = plan.Candidates
            .Where(x => !string.IsNullOrWhiteSpace(x.ProviderDocumentId))
            .Select(x => x.ProviderDocumentId!)
            .ToHashSet(StringComparer.Ordinal);
        var items = new List<EdoSaleDraftApplyResultItemDto>(request.Items.Count);
        foreach (var requestItem in request.Items)
        {
            var providerDocumentId = requestItem.ProviderDocumentId?.Trim() ?? string.Empty;
            if (!planIds.Contains(providerDocumentId))
                return null;

            var existing = await edoDocumentStore.FindByProviderDocumentIdAsync(
                organizationId,
                EdoProviderCode.EDOCS,
                providerDocumentId,
                ct);
            if (existing is null || !IsSaleLink(existing) || existing.InternalDocumentId <= 0)
                return null;

            var sale = await saleDocQuery.GetAsync(
                queryBuilder.For<SaleDoc>()
                    .Where(x => x.Id == existing.InternalDocumentId && x.OrganizationId == organizationId)
                    .Build(), ct);
            if (sale is null)
                return null;

            items.Add(new EdoSaleDraftApplyResultItemDto
            {
                ProviderDocumentId = providerDocumentId,
                Status = "ALREADY_IMPORTED",
                SaleDocId = sale.Id,
                SafeErrorCodes = ["SALE_DOCUMENT_ALREADY_LINKED"]
            });
        }

        return new EdoSaleDraftApplyResponseDto
        {
            PlanHash = plan.PlanHash,
            AlreadyImportedCount = items.Count,
            Items = items.OrderBy(x => x.ProviderDocumentId, StringComparer.Ordinal).ToArray()
        };
    }

    private async Task<Result<EdoSaleDraftApplyResponseDto>> ApplyCoreAsync(
        int organizationId,
        EdoSaleDraftApplyRequestDto request,
        EdoSalePreflightPlanDto plan,
        CancellationToken ct)
    {
        var planByProviderId = plan.Candidates
            .Where(x => !string.IsNullOrWhiteSpace(x.ProviderDocumentId))
            .ToDictionary(x => x.ProviderDocumentId!, StringComparer.Ordinal);
        var requestedIds = request.Items
            .Select(x => x.ProviderDocumentId?.Trim() ?? string.Empty)
            .ToArray();

        if (requestedIds.Any(string.IsNullOrWhiteSpace)
            || requestedIds.Distinct(StringComparer.Ordinal).Count() != requestedIds.Length)
            return Failure("EDO_SALE_PROVIDER_DOCUMENT_SELECTION_INVALID");

        if (requestedIds.Any(x => !planByProviderId.ContainsKey(x)))
            return Failure("EDO_SALE_PROVIDER_DOCUMENT_NOT_IN_PLAN");

        var prepared = new List<PreparedDraft>(request.Items.Count);
        var results = new List<EdoSaleDraftApplyResultItemDto>(request.Items.Count);

        foreach (var item in request.Items)
        {
            var providerDocumentId = item.ProviderDocumentId.Trim();
            await edoDocumentStore.AcquireProviderDocumentLockAsync(
                organizationId,
                EdoProviderCode.EDOCS,
                providerDocumentId,
                ct);

            var existing = await edoDocumentStore.FindByProviderDocumentIdAsync(
                organizationId,
                EdoProviderCode.EDOCS,
                providerDocumentId,
                ct);
            var existingResult = await ResolveExistingLinkAsync(organizationId, providerDocumentId, existing, ct);
            if (existingResult is not null)
            {
                if (existingResult.Value.IsConflict)
                    return Failure(existingResult.Value.ErrorCode!);

                results.Add(new EdoSaleDraftApplyResultItemDto
                {
                    ProviderDocumentId = providerDocumentId,
                    Status = "ALREADY_IMPORTED",
                    SaleDocId = existingResult.Value.SaleDocId,
                    SafeErrorCodes = ["SALE_DOCUMENT_ALREADY_LINKED"]
                });
                continue;
            }

            var source = await edoInboxService.GetOutboxProviderDocumentMappingSourceAsync(
                providerDocumentId,
                ct,
                providerDocumentType: item.DocumentType,
                allowSentDocuments: item.AllowSentDocuments);
            var sourceValidation = ValidateSourceAgainstPlan(
                planByProviderId[providerDocumentId],
                source.Document);
            if (sourceValidation is not null)
                return Failure(
                    sourceValidation,
                    string.Equals(sourceValidation, "STALE_SALE_PREFLIGHT_PLAN", StringComparison.Ordinal));

            var validation = await ValidateSelectionAsync(
                organizationId,
                item,
                source,
                ct);
            if (validation.ErrorCode is not null)
                return Failure(validation.ErrorCode);

            prepared.Add(new PreparedDraft(
                providerDocumentId,
                item,
                source,
                validation.Products,
                validation.ProductTables));
        }

        foreach (var draft in prepared)
        {
            var created = await CreateDraftAsync(organizationId, draft, ct);
            if (!created.IsSuccess)
                return Failure(created.Error.Code);

            results.Add(new EdoSaleDraftApplyResultItemDto
            {
                ProviderDocumentId = draft.ProviderDocumentId,
                Status = "CREATED",
                SaleDocId = created.Value,
                SafeErrorCodes = []
            });
        }

        return Result.Success(new EdoSaleDraftApplyResponseDto
        {
            PlanHash = plan.PlanHash,
            CreatedDraftCount = results.Count(x => x.Status == "CREATED"),
            AlreadyImportedCount = results.Count(x => x.Status == "ALREADY_IMPORTED"),
            Items = results.OrderBy(x => x.ProviderDocumentId, StringComparer.Ordinal).ToArray()
        });
    }

    private async Task<(bool IsConflict, string? ErrorCode, long? SaleDocId)?> ResolveExistingLinkAsync(
        int organizationId,
        string providerDocumentId,
        EdoDocument? existing,
        CancellationToken ct)
    {
        if (existing is null)
            return null;

        if (!IsSaleLink(existing))
            return (true, "SALE_PROVIDER_LINK_CONFLICT", null);

        if (existing.InternalDocumentId <= 0)
            return (true, "SALE_PROVIDER_LINK_INVALID", null);

        var sale = await saleDocQuery.GetAsync(
            queryBuilder.For<SaleDoc>()
                .Where(x => x.Id == existing.InternalDocumentId && x.OrganizationId == organizationId)
                .Build(), ct);
        return sale is null
            ? (true, "SALE_PROVIDER_LINK_INVALID", null)
            : (false, null, sale.Id);
    }

    private async Task<SelectionValidation> ValidateSelectionAsync(
        int organizationId,
        EdoSaleDraftApplyItemDto item,
        EdoOutboxProviderDocumentMappingSourceDto source,
        CancellationToken ct)
    {
        var detail = source.Document;
        if (item.CounterpartyId <= 0 || item.ContractId <= 0 || item.CurrencyId <= 0
            || item.WarehouseId <= 0 || item.ExchangeRate <= 0m)
            return SelectionValidation.Error("EDO_SALE_HEADER_MAPPING_REQUIRED");

        var counterparty = await counterpartyQuery.GetAsync(
            queryBuilder.For<CounterpartyCard>()
                .Where(x => x.Id == item.CounterpartyId
                    && x.OrganizationId == organizationId
                    && x.StateId == StateIdConst.ACTIVE
                    && x.IsCustomer)
                .Build(), ct);
        if (counterparty is null
            || !string.Equals(counterparty.Inn?.Trim(), detail.Buyer.TaxIdentifier?.Trim(), StringComparison.Ordinal))
            return SelectionValidation.Error("COUNTERPARTY_MAPPING_INVALID");

        var warehouse = await warehouseQuery.GetAsync(
            queryBuilder.For<Warehouse>()
                .Where(x => x.Id == item.WarehouseId
                    && x.OrganizationId == organizationId
                    && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        if (warehouse is null)
            return SelectionValidation.Error("WAREHOUSE_MAPPING_INVALID");

        var currency = await currencyQuery.GetAsync(
            queryBuilder.For<Currency>()
                .Where(x => x.Id == item.CurrencyId && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        var currencyError = EdoSaleCurrencySelectionPolicy.Validate(currency, detail.CurrencyCode);
        if (currencyError is not null)
            return SelectionValidation.Error(currencyError);

        var contract = await contractQuery.GetAsync(
            queryBuilder.For<Contract>()
                .Where(x => x.Id == item.ContractId
                    && x.OrganizationId == organizationId
                    && x.CounterpartyId == item.CounterpartyId
                    && x.StateId == StateIdConst.ACTIVE)
                .Build(), ct);
        if (contract is null
            || (contract.StartDate.HasValue && DateOnly.FromDateTime(contract.StartDate.Value) > detail.DocumentDate)
            || (contract.EndDate.HasValue && DateOnly.FromDateTime(contract.EndDate.Value) < detail.DocumentDate))
            return SelectionValidation.Error("CONTRACT_MAPPING_INVALID");

        var hasProviderContractNumber = !string.IsNullOrWhiteSpace(detail.ContractNumber);
        var hasProviderContractDate = detail.ContractDate.HasValue;
        if (hasProviderContractNumber != hasProviderContractDate)
            return SelectionValidation.Error("CONTRACT_PROVIDER_DATA_REQUIRED");

        if (hasProviderContractNumber
            && (!string.Equals(contract.ProviderCode, ProviderCode, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(contract.ProviderContractNumber?.Trim(), detail.ContractNumber!.Trim(), StringComparison.Ordinal)
                || contract.ProviderContractDate != detail.ContractDate))
            return SelectionValidation.Error("CONTRACT_PROVIDER_IDENTITY_MISMATCH");

        if (item.Lines.Count != detail.Lines.Count
            || item.Lines.GroupBy(x => x.Number).Any(x => x.Count() != 1))
            return SelectionValidation.Error("SALE_LINE_MAPPING_INVALID");
        var lines = item.Lines.ToDictionary(x => x.Number);
        if (detail.Lines.Any(x => !lines.ContainsKey(x.Number)))
            return SelectionValidation.Error("SALE_LINE_MAPPING_INVALID");

        var productIds = item.Lines.Select(x => x.ProductId).Distinct().ToArray();
        var products = await productQuery.GetAllAsync(
            queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id)
                    && x.OrganizationId == organizationId
                    && x.StateId == StateIdConst.ACTIVE
                    && x.IsSold)
                .Build(), ct);
        var productById = products.ToDictionary(x => x.Id);
        var stockProductIds = products
            .Where(x => !x.IsService)
            .Select(x => x.Id)
            .ToArray();
        var availableByProductId = new Dictionary<int, decimal>();
        var inventoryByProductId = new Dictionary<int, WarehouseProductDto>();
        if (stockProductIds.Length > 0)
        {
            var inventoryResult = await warehouseInventoryService.GetWarehouseProductsAsync(
                new WarehouseProductFilter
                {
                    WarehouseId = item.WarehouseId,
                    ProductIds = stockProductIds
                },
                ct);
            if (!inventoryResult.IsSuccess)
                return SelectionValidation.Error("SALE_PRODUCT_STOCK_MAPPING_REQUIRED");

            inventoryByProductId = inventoryResult.Value
                .GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, x => x.First());
            availableByProductId = inventoryResult.Value
                .GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, x => x.First().AvailableQuantity);
        }
        var productTablesByLine = new Dictionary<int, IReadOnlyCollection<ProductTable>>();

        foreach (var providerLine in detail.Lines)
        {
            var line = lines[providerLine.Number];
            if (!productById.TryGetValue(line.ProductId, out var product)
                || !string.Equals(product.Mxik, providerLine.ProviderProductCode, StringComparison.Ordinal))
                return SelectionValidation.Error("PRODUCT_MAPPING_INVALID");

            if (!product.IsService
                && (!availableByProductId.TryGetValue(product.Id, out var availableQuantity)
                    || availableQuantity < providerLine.Quantity))
                return SelectionValidation.Error("SALE_PRODUCT_STOCK_MAPPING_REQUIRED");

            if (line.UnitId != product.UnitId)
                return SelectionValidation.Error("PRODUCT_UNIT_MAPPING_INVALID");
            if (line.Quantity != providerLine.Quantity || line.UnitPrice != providerLine.UnitPrice)
                return SelectionValidation.Error("SALE_SOURCE_VALUES_CHANGED");
            if (line.CostPrice < 0m
                || (!string.Equals(line.CostPriceSource, ExplicitCostPriceSource, StringComparison.Ordinal)
                    && !string.Equals(line.CostPriceSource, WarehouseStockBatchCostPriceSource, StringComparison.Ordinal)))
                return SelectionValidation.Error("COST_PRICE_SOURCE_REQUIRED");

            if (string.Equals(line.CostPriceSource, WarehouseStockBatchCostPriceSource, StringComparison.Ordinal))
            {
                var batch = inventoryByProductId.GetValueOrDefault(product.Id)?.Batches
                    .Where(x => x.AvailableQuantity > 0m && x.AvailableQuantity >= providerLine.Quantity)
                    .OrderBy(x => x.ReceivedDate)
                    .ThenBy(x => x.BatchId)
                    .FirstOrDefault();
                if (batch is null || Math.Abs(batch.UnitCost - line.CostPrice) > 0.000001m)
                    return SelectionValidation.Error("COST_PRICE_SOURCE_REQUIRED");
            }

            var vatRates = providerLine.VatRate.HasValue
                ? await vatRateQuery.GetAllAsync(
                    queryBuilder.For<VatRate>()
                        .Where(x => x.StateId == StateIdConst.ACTIVE
                            && x.Rate == providerLine.VatRate.Value
                            && (x.EffectiveFrom == null || x.EffectiveFrom <= detail.DocumentDate)
                            && (x.EffectiveTo == null || x.EffectiveTo >= detail.DocumentDate))
                        .Build(), ct)
                : [];
            if (providerLine.VatRate.HasValue
                ? vatRates.Count(x => x.Id == line.VatRateId) != 1
                : line.VatRateId.HasValue)
                return SelectionValidation.Error("VAT_MAPPING_INVALID");

            if (!EdoSaleAmountValidation.IsLineInternallyConsistent(
                    providerLine.Quantity,
                    providerLine.UnitPrice,
                    providerLine.NetAmount,
                    providerLine.VatAmount,
                    providerLine.TotalWithVat))
                return SelectionValidation.Error("SALE_SOURCE_TOTALS_MISMATCH");

            var markingCodes = source.MarkingCodesByLine.GetValueOrDefault(providerLine.Number) ?? [];
            if (product.IsService && markingCodes.Count > 0)
                return SelectionValidation.Error("SERVICE_MARKING_NOT_ALLOWED");
            if (markingCodes.Count > 0 && !product.IsPieceTracked)
                return SelectionValidation.Error("PRODUCT_PIECE_TRACKING_REQUIRED");

            if (markingCodes.Count == 0)
            {
                if (!string.Equals(line.MarkingSource, NoMarkingSource, StringComparison.Ordinal))
                    return SelectionValidation.Error("MARKING_SOURCE_INVALID");
            }
            else if (!string.Equals(line.MarkingSource, ProviderMarkingSource, StringComparison.Ordinal))
            {
                return SelectionValidation.Error("MARKING_SOURCE_REQUIRED");
            }

            var requestedTableIds = line.Items.Select(x => x.ProductTableId).ToArray();
            if (requestedTableIds.Any(x => x <= 0)
                || requestedTableIds.Distinct().Count() != requestedTableIds.Length)
                return SelectionValidation.Error("PRODUCT_TABLE_MAPPING_INVALID");
            if (product.IsService && requestedTableIds.Length > 0)
                return SelectionValidation.Error("SERVICE_PRODUCT_TABLE_NOT_ALLOWED");
            if (!product.IsService && product.IsPieceTracked
                && requestedTableIds.Length != decimal.ToInt32(line.Quantity))
                return SelectionValidation.Error("PRODUCT_TABLE_COUNT_INVALID");
            if (!product.IsPieceTracked && requestedTableIds.Length > 0)
                return SelectionValidation.Error("PRODUCT_TABLE_NOT_ALLOWED");

            var tables = requestedTableIds.Length == 0
                ? []
                : await productTableQuery.GetAllAsync(
                    queryBuilder.For<ProductTable>()
                        .Where(x => requestedTableIds.Contains(x.Id)
                            && x.ProductId == product.Id
                            && x.Product.OrganizationId == organizationId
                            && x.Product.StateId == StateIdConst.ACTIVE
                            && x.WarehouseProductTable != null
                            && x.WarehouseProductTable.WarehouseId == item.WarehouseId
                            && x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK)
                        .Build(), ct);
            if (tables.Count != requestedTableIds.Length)
                return SelectionValidation.Error("PRODUCT_TABLE_NOT_AVAILABLE");

            if (markingCodes.Count > requestedTableIds.Length)
                return SelectionValidation.Error("MARKING_COUNT_MISMATCH");

            if (!EdoPartialMarkingRules.IsValidSelection(
                    requestedTableIds.Length,
                    tables,
                    markingCodes))
            {
                return SelectionValidation.Error("MARKING_MAPPING_INVALID");
            }

            productTablesByLine[providerLine.Number] = tables;
        }

        if (!EdoSaleAmountValidation.AreDocumentTotalsConsistent(
                detail.NetAmount,
                detail.VatAmount,
                detail.TotalAmount,
                detail.Lines.Select(x => (x.NetAmount, x.VatAmount, x.TotalWithVat))))
            return SelectionValidation.Error("SALE_SOURCE_TOTALS_MISMATCH");

        return new SelectionValidation(null, products, productTablesByLine);
    }

    private async Task<Result<long>> CreateDraftAsync(
        int organizationId,
        PreparedDraft draft,
        CancellationToken ct)
    {
        var detail = draft.Source.Document;
        var number = await documentNumberService.GetNextHistoricalAsync(
            organizationId,
            DocumentTypeIdConst.SALE,
            detail.DocumentDate.ToDateTime(TimeOnly.MinValue),
            ct);
        if (!number.IsSuccess)
            return Result.Failure<long>(Error.Business(
                "HISTORICAL_SALE_NUMBER_REQUIRED",
                "Historical sale document numbering is unavailable."));

        var doc = new SaleDoc
        {
            OrganizationId = organizationId,
            DocNumber = number.Value.DocumentNumber,
            DocDate = detail.DocumentDate.ToDateTime(TimeOnly.MinValue),
            CounterpartyId = draft.Item.CounterpartyId,
            ContractId = draft.Item.ContractId,
            WarehouseId = draft.Item.WarehouseId,
            CurrencyId = draft.Item.CurrencyId,
            ExchangeRate = draft.Item.ExchangeRate,
            TotalAmount = detail.NetAmount ?? detail.Lines.Sum(x => x.NetAmount),
            VatAmount = detail.VatAmount ?? detail.Lines.Sum(x => x.VatAmount),
            FinalAmount = detail.TotalAmount,
            StatusId = DocumentStatusIdConst.DRAFT,
            StateId = StateIdConst.ACTIVE,
            Comment = draft.Item.Comment,
            CreatedDate = DateTime.Now
        };
        await saleDocCommand.CreateAsync(doc, ct);

        var requestLines = draft.Item.Lines.ToDictionary(x => x.Number);
        var providerLines = detail.Lines.OrderBy(x => x.Number).ToArray();
        var saleLines = providerLines.Select(providerLine =>
        {
            var line = requestLines[providerLine.Number];
            var amount = line.Quantity * line.UnitPrice;
            var vatAmount = providerLine.VatAmount;
            return new SaleDocProduct
            {
                OwnerId = doc.Id,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                CostPrice = line.CostPrice,
                Amount = amount,
                VatRateId = line.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                UnitId = line.UnitId
            };
        }).ToArray();
        await saleDocProductCommand.CreateAsync(saleLines, ct);

        var tables = new List<SaleDocTable>();
        for (var i = 0; i < providerLines.Length; i++)
        {
            var providerLine = providerLines[i];
            var line = requestLines[providerLine.Number];
            foreach (var item in line.Items)
            {
                var perUnitVat = line.Quantity == 0m ? 0m : Math.Round(providerLine.VatAmount / line.Quantity, 8);
                tables.Add(new SaleDocTable
                {
                    OwnerId = saleLines[i].Id,
                    ProductTableId = item.ProductTableId,
                    CostPrice = line.CostPrice,
                    Amount = line.UnitPrice,
                    VatRateId = line.VatRateId,
                    VatAmount = perUnitVat,
                    TotalAmount = line.UnitPrice + perUnitVat
                });
            }
        }
        if (tables.Count > 0)
            await saleDocTableCommand.CreateAsync(tables, ct);

        await edoDocumentStore.AddAsync(new EdoDocument
        {
            OrganizationId = organizationId,
            Provider = ProviderCode,
            Direction = EdoDirection.OUTBOX.ToString(),
            InternalDocumentType = InternalDocumentType,
            InternalDocumentId = doc.Id,
            ProviderDocumentId = draft.ProviderDocumentId,
            DocumentType = detail.DocumentType,
            DocumentNumber = detail.DocumentNumber,
            DocumentDate = detail.DocumentDate,
            Status = detail.Status.Code.ToString(),
            ProviderStatusCode = detail.Status.Code.ToString(),
            OperationType = OperationType,
            IdempotencyKey = draft.ProviderDocumentId,
            CreatedAt = DateTime.Now
        }, ct);

        return Result.Success(doc.Id);
    }

    private static string? ValidateSourceAgainstPlan(
        EdoSalePreflightCandidateDto candidate,
        EdoOutboxProviderDocumentDetailDto detail)
    {
        if (candidate.Status == "ALREADY_IMPORTED")
            return "SALE_DOCUMENT_ALREADY_LINKED";
        if (!string.Equals(candidate.ProviderDocumentId, detail.ProviderDocumentId, StringComparison.Ordinal)
            || candidate.DocumentNumber != detail.DocumentNumber
            || candidate.DocumentDate != detail.DocumentDate
            || NormalizeDocumentType(candidate.DocumentType) != NormalizeDocumentType(detail.DocumentType)
            || candidate.Lines.Count != detail.Lines.Count)
            return "STALE_SALE_PREFLIGHT_PLAN";
        var candidateLines = candidate.Lines.ToDictionary(x => x.Number);
        if (detail.Lines.Any(line => !candidateLines.TryGetValue(line.Number, out var planned)
                || planned.ProviderProductCode != line.ProviderProductCode
                || planned.Quantity != line.Quantity
                || planned.UnitPrice != line.UnitPrice
                || planned.NetAmount != line.NetAmount
                || planned.VatRate != line.VatRate
                || planned.VatAmount != line.VatAmount
                || planned.TotalWithVat != line.TotalWithVat))
            return "STALE_SALE_PREFLIGHT_PLAN";
        return null;
    }

    private static bool IsSaleLink(EdoDocument document) =>
        document.Provider == ProviderCode
        && string.Equals(document.InternalDocumentType, InternalDocumentType, StringComparison.OrdinalIgnoreCase);

    private static Result<EdoSaleDraftApplyResponseDto> Failure(string code, bool conflict = false) =>
        Result.Failure<EdoSaleDraftApplyResponseDto>(
            conflict
                ? Error.Conflict(code, "The EDO sale draft plan is stale or no longer applicable.")
                : Error.Business(code, "The EDO sale draft selection is invalid."));

    private static string NormalizeDocumentType(string? value) =>
        string.Equals(value?.Trim(), "waybillLocal", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value?.Trim(), "WAYBILL_LOCAL", StringComparison.OrdinalIgnoreCase)
            ? "WAYBILL_LOCAL"
            : string.Equals(value?.Trim(), "FACTURA", StringComparison.OrdinalIgnoreCase)
                ? "FACTURA"
                : value?.Trim().ToUpperInvariant() ?? string.Empty;

    private sealed record PreparedDraft(
        string ProviderDocumentId,
        EdoSaleDraftApplyItemDto Item,
        EdoOutboxProviderDocumentMappingSourceDto Source,
        IReadOnlyCollection<Product> Products,
        IReadOnlyDictionary<int, IReadOnlyCollection<ProductTable>> ProductTables);

    private sealed record SelectionValidation(
        string? ErrorCode,
        IReadOnlyCollection<Product> Products,
        IReadOnlyDictionary<int, IReadOnlyCollection<ProductTable>> ProductTables)
    {
        public static SelectionValidation Error(string code) => new(code, [], new Dictionary<int, IReadOnlyCollection<ProductTable>>());
    }
}
