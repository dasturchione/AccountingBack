using Application.Features.Integration.Edo.UnifiedImport;
using Application.Features.SaleDocs.EdoSalePreflight;

public sealed class EdoUnifiedImportMappingTests
{
    [Fact]
    public void OnlyReadyResolverSelectionsAreCopiedToTheUnifiedPlan()
    {
        var candidate = Candidate(
            counterparty: Mapping("READY", 12),
            contract: Mapping("REQUIRES_SELECTION", candidateIds: [31, 32]),
            currency: Mapping("READY", 1),
            warehouse: Mapping("READY", 4),
            line: new EdoSalePreflightLineDto
            {
                Number = 1,
                ProviderProductCode = "08415001001002073",
                Quantity = 2,
                UnitPrice = 10,
                Product = Mapping("READY", 77),
                Unit = Mapping("READY", 3),
                Vat = Mapping("READY", 5),
                ProductTable = Mapping("REQUIRES_SELECTION", candidateIds: []),
                CostPrice = Mapping("REQUIRES_SELECTION"),
                Marking = new EdoSaleMarkingStatusDto
                {
                    HasMarkings = true,
                    MarkingRequired = true,
                    MarkingCount = 2,
                    Status = "REQUIRES_SELECTION",
                    SafeErrorCode = "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN"
                }
            });

        var result = EdoUnifiedImportPlanMapping.FromSaleCandidate(candidate);

        Assert.Equal(12, result.CounterpartyId);
        Assert.Null(result.ContractId);
        Assert.Equal([31L, 32L], result.ContractCandidateIds);
        Assert.Equal((short)1, result.CurrencyId);
        Assert.Equal(4, result.WarehouseId);
        Assert.Equal(77, result.Lines.Single().ProductId);
        Assert.Equal((short)3, result.Lines.Single().UnitId);
        Assert.Equal((short)5, result.Lines.Single().VatRateId);
        Assert.Empty(result.Lines.Single().ProductTableIds);
        Assert.Equal("REQUIRES_SELECTION", result.Lines.Single().CostPriceStatus);
        Assert.True(result.Lines.Single().MarkingRequired);
        Assert.Equal("PROVIDER_SNAPSHOT", result.Lines.Single().MarkingSource);
    }

    [Fact]
    public void AmbiguousResolverSelectionDoesNotGuessAnId()
    {
        var candidate = Candidate(
            counterparty: Mapping("REQUIRES_SELECTION", candidateIds: [12, 13]),
            contract: Mapping("REQUIRES_SELECTION", candidateIds: [31, 32]),
            currency: Mapping("REQUIRES_SELECTION", candidateIds: [1, 2]),
            warehouse: Mapping("REQUIRES_SELECTION", candidateIds: [4, 5]),
            line: new EdoSalePreflightLineDto
            {
                Number = 1,
                Product = Mapping("REQUIRES_SELECTION", candidateIds: [77, 78]),
                Unit = Mapping("BLOCKED"),
                Vat = Mapping("REQUIRES_SELECTION", candidateIds: [5, 6]),
                ProductTable = Mapping("BLOCKED"),
                CostPrice = Mapping("REQUIRES_SELECTION")
            });

        var result = EdoUnifiedImportPlanMapping.FromSaleCandidate(candidate);

        Assert.Null(result.CounterpartyId);
        Assert.Null(result.ContractId);
        Assert.Null(result.CurrencyId);
        Assert.Null(result.WarehouseId);
        Assert.Null(result.Lines.Single().ProductId);
        Assert.Null(result.Lines.Single().VatRateId);
        Assert.Empty(result.Lines.Single().ProductTableIds);
    }

    [Fact]
    public void MappingPlanContainsOnlySafeMarkingMetadata()
    {
        var candidate = Candidate(
            counterparty: Mapping("READY", 12),
            contract: Mapping("READY", 31),
            currency: Mapping("READY", 1),
            warehouse: Mapping("READY", 4),
            line: new EdoSalePreflightLineDto
            {
                Number = 1,
                Product = Mapping("READY", 77),
                Unit = Mapping("READY", 3),
                Vat = Mapping("READY", 5),
                ProductTable = Mapping("REQUIRES_SELECTION"),
                CostPrice = Mapping("REQUIRES_SELECTION"),
                Marking = new EdoSaleMarkingStatusDto
                {
                    HasMarkings = true,
                    MarkingRequired = true,
                    MarkingCount = 1,
                    Status = "REQUIRES_SELECTION"
                }
            });

        var result = EdoUnifiedImportPlanMapping.FromSaleCandidate(candidate);
        var json = System.Text.Json.JsonSerializer.Serialize(result);

        Assert.DoesNotContain("MarkingCodes", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("markingNumber", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MarkingRequired", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MarkingSource", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolvedUnifiedMappingDropsOnlyStaleSalePreflightBlockers()
    {
        var candidate = Candidate(
            counterparty: Mapping("READY", 12),
            contract: Mapping("READY", 31),
            currency: Mapping("READY", 1),
            warehouse: Mapping("READY", 4),
            line: new EdoSalePreflightLineDto
            {
                Number = 1,
                Product = Mapping("READY", 77),
                Unit = Mapping("READY", 3),
                Vat = Mapping("READY", 5),
                ProductTable = Mapping("REQUIRES_SELECTION"),
                CostPrice = Mapping("REQUIRES_SELECTION"),
                Marking = new EdoSaleMarkingStatusDto
                {
                    HasMarkings = true,
                    MarkingRequired = true,
                    MarkingCount = 1,
                    Status = "REQUIRES_SELECTION",
                    SafeErrorCode = "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN"
                }
            },
            safeErrorCodes:
            [
                "SALE_SOURCE_LINK_REQUIRED",
                "SALE_COST_PRICE_SOURCE_REQUIRED",
                "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN",
                "PRODUCT_TABLE_SELECTION_REQUIRED"
            ]);

        var mapping = new EdoUnifiedImportPlanMappingSnapshot
        {
            CounterpartyId = 12,
            ContractId = 31,
            CurrencyId = 1,
            WarehouseId = 4,
            CounterpartyMappingStatus = "READY",
            ContractMappingStatus = "READY",
            CurrencyMappingStatus = "READY",
            WarehouseMappingStatus = "READY",
            ProductMappingStatus = "READY",
            VatMappingStatus = "READY",
            CostPriceStatus = "READY",
            ProductTableMappingStatus = "READY",
            MarkingMappingStatus = "READY",
            Lines =
            [
                new EdoUnifiedImportPlanLineDto
                {
                    LineNumber = 1,
                    ProductId = 77,
                    UnitId = 3,
                    VatRateId = 5,
                    ProductMappingStatus = "READY",
                    ProductTableMappingStatus = "READY",
                    VatMappingStatus = "READY",
                    CostPriceStatus = "READY",
                    MarkingRequired = true
                }
            ]
        };

        var errors = EdoUnifiedImportPlanMapping.BuildCurrentSafeErrorCodes(candidate, mapping);

        Assert.True(EdoUnifiedImportPlanMapping.IsReady(mapping));
        Assert.Empty(errors);
    }

    [Fact]
    public void UnresolvedUnifiedMappingKeepsSpecificBlockerAndNeverKeepsSourceLinkRequired()
    {
        var candidate = Candidate(
            counterparty: Mapping("READY", 12),
            contract: Mapping("READY", 31),
            currency: Mapping("READY", 1),
            warehouse: Mapping("READY", 4),
            line: new EdoSalePreflightLineDto
            {
                Number = 1,
                Product = Mapping("BLOCKED"),
                Unit = Mapping("BLOCKED"),
                Vat = Mapping("READY", 5),
                ProductTable = Mapping("BLOCKED"),
                CostPrice = Mapping("REQUIRES_SELECTION")
            },
            safeErrorCodes: ["SALE_SOURCE_LINK_REQUIRED", "PRODUCT_MAPPING_AMBIGUOUS"]);

        var mapping = new EdoUnifiedImportPlanMappingSnapshot
        {
            CounterpartyMappingStatus = "READY",
            ContractMappingStatus = "READY",
            CurrencyMappingStatus = "READY",
            WarehouseMappingStatus = "READY",
            ProductMappingStatus = "BLOCKED",
            VatMappingStatus = "READY",
            CostPriceStatus = "REQUIRES_SELECTION",
            ProductTableMappingStatus = "BLOCKED",
            MarkingMappingStatus = "NOT_REQUIRED",
            Lines =
            [
                new EdoUnifiedImportPlanLineDto
                {
                    ProductMappingStatus = "BLOCKED",
                    ProductTableMappingStatus = "BLOCKED",
                    VatMappingStatus = "READY",
                    CostPriceStatus = "REQUIRES_SELECTION"
                }
            ]
        };

        var errors = EdoUnifiedImportPlanMapping.BuildCurrentSafeErrorCodes(candidate, mapping);

        Assert.False(EdoUnifiedImportPlanMapping.IsReady(mapping));
        Assert.Contains("PRODUCT_MAPPING_AMBIGUOUS", errors);
        Assert.Contains("SALE_COST_PRICE_SOURCE_REQUIRED", errors);
        Assert.Contains("PRODUCT_TABLE_SELECTION_REQUIRED", errors);
        Assert.DoesNotContain("SALE_SOURCE_LINK_REQUIRED", errors);
    }

    private static EdoSalePreflightCandidateDto Candidate(
        EdoSaleMappingStatusDto counterparty,
        EdoSaleMappingStatusDto contract,
        EdoSaleMappingStatusDto currency,
        EdoSaleMappingStatusDto warehouse,
        EdoSalePreflightLineDto line,
        IReadOnlyCollection<string>? safeErrorCodes = null) => new()
    {
        ProviderDocumentId = "provider-document",
        Counterparty = counterparty,
        Contract = contract,
        Currency = currency,
        Warehouse = warehouse,
        Lines = [line],
        Marking = line.Marking,
        SafeErrorCodes = safeErrorCodes ?? []
    };

    private static EdoSaleMappingStatusDto Mapping(
        string status,
        long? selectedId = null,
        IReadOnlyCollection<long>? candidateIds = null) => new()
    {
        Status = status,
        SelectedId = selectedId,
        CandidateIds = candidateIds ?? []
    };
}
