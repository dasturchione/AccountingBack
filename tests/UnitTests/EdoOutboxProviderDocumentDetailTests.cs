using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Edo;
using SharedKernel.Exceptions;
using System.Text.Json;

public sealed class EdoOutboxProviderDocumentDetailTests
{
    [Fact]
    public void MapsSignedFacturaWithoutReturningRawMarkingCodes()
    {
        var result = EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
            CreateDocument(),
            "edocs-provider-id");

        Assert.Equal("edocs-provider-id", result.ProviderDocumentId);
        Assert.Equal("INV-42", result.DocumentNumber);
        Assert.Equal(120m, result.NetAmount);
        Assert.Equal(24m, result.VatAmount);
        Assert.Equal(144m, result.TotalAmount);
        Assert.Single(result.Lines);
        Assert.True(result.Lines.Single().Marking.HasMarkings);
        Assert.Equal(1, result.Lines.Single().Marking.Count);
        var json = JsonSerializer.Serialize(result);
        Assert.DoesNotContain("masked-marking", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsProviderIdentityMismatch()
    {
        var exception = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(CreateDocument(), "other-id"));

        Assert.Equal("EDO_OUTBOX_DETAIL_INVALID", exception.Code);
    }

    [Fact]
    public void RejectsUnsignedDocument()
    {
        var document = CreateDocument(statusCode: EdoDocumentStatusCode.SENT);

        var exception = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(document, "edocs-provider-id"));

        Assert.Equal("EDO_OUTBOX_DETAIL_NOT_SIGNED", exception.Code);
    }

    [Fact]
    public void RejectsWaybillAndMissingLines()
    {
        var waybill = CreateDocument(documentType: "waybillLocal");
        var waybillException = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(waybill, "edocs-provider-id"));
        Assert.Equal("EDO_OUTBOX_DETAIL_INVALID", waybillException.Code);

        var noLines = CreateDocument(lines: []);
        var linesException = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(noLines, "edocs-provider-id"));
        Assert.Equal("EDO_OUTBOX_LINES_INVALID", linesException.Code);
    }

    private static EdoDocumentDto CreateDocument(
        string documentType = "FACTURA",
        EdoDocumentStatusCode statusCode = EdoDocumentStatusCode.SIGNED,
        IReadOnlyCollection<EdoDocumentPreviewLineDto>? lines = null) => new()
    {
        ProviderCode = EdoProviderCode.EDOCS,
        ProviderDocumentId = "edocs-provider-id",
        Direction = EdoDirection.OUTBOX,
        Category = EdoDocumentCategory.OUTBOX,
        DocumentType = documentType,
        DocumentNumber = "INV-42",
        DocumentDate = new DateOnly(2026, 8, 20),
        Status = new EdoDocumentStatusDto
        {
            Code = statusCode,
            IsTerminal = true,
            IsSuccessful = true
        },
        Seller = new EdoPartyDto { Name = "Seller", TaxIdentifier = "111111111" },
        Buyer = new EdoPartyDto { Name = "Buyer", TaxIdentifier = "222222222" },
        TotalAmount = 144m,
        CurrencyCode = "UZS",
        PreviewLines = lines ??
        [
            new EdoDocumentPreviewLineDto
            {
                Number = 1,
                CatalogCode = "0101",
                CatalogName = "Product",
                PackageCode = "1",
                PackageName = "dona",
                Quantity = 2m,
                UnitPrice = 60m,
                NetAmount = 120m,
                VatRate = 20m,
                VatAmount = 24m,
                TotalWithVat = 144m,
                MarkingCodes = ["masked-marking"]
            }
        ]
    };
}
