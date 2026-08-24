using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Edo.UnifiedImport;

public sealed class EdoUnifiedImportPlanRulesTests
{
    [Theory]
    [InlineData("FACTURA")]
    [InlineData("factura")]
    [InlineData(" FACTURA ")]
    public void OnlyFacturaIsEligible(string value) =>
        Assert.True(EdoUnifiedImportPlanRules.IsFactura(value));

    [Theory]
    [InlineData("waybillLocal")]
    [InlineData("empowerment")]
    [InlineData("letter")]
    [InlineData("")]
    public void NonFacturaTypesAreExcluded(string value) =>
        Assert.False(EdoUnifiedImportPlanRules.IsFactura(value));

    [Theory]
    [InlineData("waybillLocal")]
    [InlineData("WAYBILL_LOCAL")]
    public void WaybillLocalIsNormalizedAsSupportedSaleType(string value)
    {
        Assert.Equal(EdoUnifiedImportPlanRules.WaybillLocal, EdoUnifiedImportPlanRules.NormalizeDocumentType(value));
        Assert.True(EdoUnifiedImportPlanRules.IsSupportedSaleDocumentType(value));
    }

    [Fact]
    public void DetailLineMarkingsOverrideStaleRootCount()
    {
        var document = new EdoDocumentDto
        {
            MarkingCodes = [],
            PreviewLines =
            [
                new EdoDocumentPreviewLineDto { Number = 1, MarkingCodes = ["internal-only-1", "internal-only-2"] }
            ]
        };

        Assert.Equal(2, EdoUnifiedImportPlanRules.GetMarkingCount(document));
        Assert.True(EdoUnifiedImportPlanRules.HasMarking(document));
    }

    [Fact]
    public void ProviderDocumentSelectionIsExactAndDeterministic()
    {
        var selection = EdoUnifiedImportPlanRules.NormalizeSelection([" doc-2 ", "doc-1", "doc-2"]);

        Assert.NotNull(selection);
        Assert.Equal(2, selection!.Count);
        Assert.Contains("doc-1", selection);
        Assert.Contains("doc-2", selection);
        Assert.DoesNotContain("doc-20", selection);
    }
}
