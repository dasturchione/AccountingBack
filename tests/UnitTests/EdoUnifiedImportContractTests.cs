using Application.Features.Integration.Edo.UnifiedImport;
using Domain.Entities;

public sealed class EdoUnifiedImportContractTests
{
    [Fact]
    public void PlanRequestDefaultsToReadOnlySelection()
    {
        var request = new EdoUnifiedImportPlanRequestDto();

        Assert.Empty(request.ProviderDocumentIds);
        Assert.False(request.AllowSentDocuments);
    }

    [Fact]
    public void ApplyContractRequiresExplicitConfirmationAndPlanHashFields()
    {
        var request = new EdoUnifiedImportApplyRequestDto();

        Assert.False(request.Confirm);
        Assert.Empty(request.ExpectedPlanHash);
        Assert.Empty(request.Items);
        Assert.False(request.AllowSentDocuments);
        Assert.Equal("FACTURA", new EdoUnifiedImportApplyItemDto().DocumentType);
    }

    [Fact]
    public void PublicUnifiedDtosContainOnlyMarkingMetadata()
    {
        var publicTypes = new[]
        {
            typeof(EdoUnifiedImportPlanItemDto),
            typeof(EdoUnifiedImportBatchDocumentDto)
        };

        foreach (var type in publicTypes)
        {
            Assert.DoesNotContain(type.GetProperties(), property =>
                property.Name.Contains("Code", StringComparison.OrdinalIgnoreCase) &&
                property.Name.Contains("Marking", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(type.GetProperties(), property =>
                property.Name.Contains("Raw", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void BatchDocumentCannotLinkBothPurchaseAndSale()
    {
        var document = new EdoImportBatchDocument(
            1,
            2,
            "provider-document",
            "INBOX",
            EdoImportBatchDocumentStatus.Signed,
            DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => document.SetResult(
            EdoImportBatchDocumentStatus.Imported,
            10,
            20,
            null,
            DateTime.UtcNow));
    }

    [Fact]
    public void BatchDocumentPreservesNormalizedWaybillAndSentOverrideMetadata()
    {
        var document = new EdoImportBatchDocument(
            1,
            2,
            "provider-document",
            "OUTBOX",
            EdoImportBatchDocumentStatus.Signed,
            DateTime.UtcNow,
            "waybillLocal",
            sentOverrideApplied: true);

        Assert.Equal("WAYBILL_LOCAL", document.DocumentType);
        Assert.True(document.SentOverrideApplied);
    }
}
