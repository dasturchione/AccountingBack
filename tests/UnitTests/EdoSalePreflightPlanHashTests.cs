using Application.Features.SaleDocs.EdoSalePreflight;
using Application.Features.SaleDocs;

public sealed class EdoSalePreflightPlanHashTests
{
    [Fact]
    public void HashIsDeterministicAndOrderIndependent()
    {
        var first = Candidate("b");
        var second = Candidate("a");

        var hash1 = EdoSalePreflightPlanHash.Compute([first, second]);
        var hash2 = EdoSalePreflightPlanHash.Compute([second, first]);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);
    }

    [Fact]
    public void HashIncludesSafeMarkingMetadataButNoRawCodes()
    {
        var candidate = new EdoSalePreflightCandidateDto
        {
            ProviderDocumentId = "a",
            Status = "BLOCKED",
            SafeErrorCodes = ["SALE_SOURCE_LINK_REQUIRED"],
            Marking = new EdoSaleMarkingStatusDto
            {
                HasMarkings = true,
                MarkingCount = 2,
                MarkingRequired = true,
                Status = "REQUIRES_SELECTION",
                SafeErrorCode = "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN"
            }
        };

        var hash = EdoSalePreflightPlanHash.Compute([candidate]);
        Assert.NotEqual(EdoSalePreflightPlanHash.Compute([Candidate("a")]), hash);
    }

    [Fact]
    public void ApplyContractUsesExplicitSaleFieldsAndNoRawMarkingProperty()
    {
        var item = new EdoSaleDraftApplyItemDto
        {
            ProviderDocumentId = "provider-id",
            CounterpartyId = 1,
            ContractId = 2,
            CurrencyId = 3,
            WarehouseId = 4,
            Lines =
            [
                new EdoSaleDraftApplyLineDto
                {
                    Number = 1,
                    ProductId = 5,
                    UnitId = 6,
                    Quantity = 1m,
                    UnitPrice = 10m,
                    CostPrice = 7m,
                    CostPriceSource = "EXPLICIT_USER",
                    MarkingSource = "NONE"
                }
            ]
        };

        Assert.Equal("provider-id", item.ProviderDocumentId);
        Assert.Equal(5, item.Lines.Single().ProductId);
        Assert.NotNull(typeof(SaleDocCreateProductTableDto)
            .GetProperty(nameof(SaleDocCreateProductTableDto.ProductTableId)));
        Assert.Null(typeof(EdoSaleDraftApplyLineDto).GetProperty("MarkingCodes"));
        Assert.Null(typeof(EdoSaleDraftApplyResponseDto).GetProperty("MarkingCodes"));
    }

    [Fact]
    public void HistoricalNumberingContractRemainsSeparateFromNormalNumbering()
    {
        Assert.NotNull(typeof(Application.Features.DocumentNumbers.IDocumentNumberService)
            .GetMethod(nameof(Application.Features.DocumentNumbers.IDocumentNumberService.GetNextHistoricalAsync)));
        Assert.NotNull(typeof(Application.Features.DocumentNumbers.IDocumentNumberService)
            .GetMethod(nameof(Application.Features.DocumentNumbers.IDocumentNumberService.GetNextAsync)));
    }

    private static EdoSalePreflightCandidateDto Candidate(string providerDocumentId) => new()
    {
        ProviderDocumentId = providerDocumentId,
        Status = "BLOCKED",
        SafeErrorCodes = ["SALE_SOURCE_LINK_REQUIRED"]
    };
}
