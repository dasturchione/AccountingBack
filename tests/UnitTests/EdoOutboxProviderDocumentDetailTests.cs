using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Edo;
using Integration.Edocs.Facturas;
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
    public void SentDocumentIsReadOnlyAllowedOnlyWithExplicitOverride()
    {
        var sentDocument = CreateDocument(statusCode: EdoDocumentStatusCode.SENT);

        var blocked = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
                sentDocument,
                "edocs-provider-id",
                "FACTURA",
                allowSentDocuments: false));

        Assert.Equal("EDO_OUTBOX_DETAIL_NOT_SIGNED", blocked.Code);

        var detail = EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
            sentDocument,
            "edocs-provider-id",
            "FACTURA",
            allowSentDocuments: true);

        Assert.Equal(EdoDocumentStatusCode.SENT, detail.Status.Code);
        Assert.Single(detail.Lines);
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

    [Fact]
    public void ExplicitWaybillAndSentOverrideAreAcceptedWithoutChangingProviderStatus()
    {
        var document = CreateDocument(documentType: "waybillLocal", statusCode: EdoDocumentStatusCode.SENT);

        var result = EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
            document,
            "edocs-provider-id",
            "WAYBILL_LOCAL",
            allowSentDocuments: true);

        Assert.Equal("WAYBILL_LOCAL", result.DocumentType);
        Assert.Equal(EdoDocumentStatusCode.SENT, result.Status.Code);
    }

    [Fact]
    public void WaybillLocalUsesTheProviderRouteType()
    {
        Assert.Equal("factura", EdocsEdoOperations.MapStatusDocumentType("FACTURA"));
        Assert.Equal("waybillLocal", EdocsEdoOperations.MapStatusDocumentType("WAYBILL_LOCAL"));
        Assert.Equal("waybillLocal", EdocsEdoOperations.MapStatusDocumentType("waybillLocal"));
    }

    [Fact]
    public void WaybillLocalConsignorAndConsigneeBecomeSellerAndBuyerWithoutUsingCarrier()
    {
        using var json = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": {
            "consignor": { "name": "Consignor", "tinorpinfl": "111111111" },
            "consignee": { "name": "Consignee", "tinorpinfl": "222222222" },
            "carrier": { "name": "Carrier", "tinorpinfl": "333333333" }
          }
        }
        """);

        var seller = EdocsEdoOperations.ReadSellerParty(json.RootElement, "waybillLocal");
        var buyer = EdocsEdoOperations.ReadBuyerParty(json.RootElement, "waybillLocal");

        Assert.NotNull(seller);
        Assert.Equal("Consignor", seller.Name);
        Assert.Equal("111111111", seller.TaxIdentifier);
        Assert.NotNull(buyer);
        Assert.Equal("Consignee", buyer.Name);
        Assert.Equal("222222222", buyer.TaxIdentifier);
        Assert.DoesNotContain("Carrier", new[] { seller.Name, buyer.Name });

        using var carrierOnlyJson = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": {
            "carrier": { "name": "Carrier", "tinorpinfl": "333333333" }
          }
        }
        """);

        Assert.Null(EdocsEdoOperations.ReadSellerParty(carrierOnlyJson.RootElement, "waybillLocal"));
        Assert.Null(EdocsEdoOperations.ReadBuyerParty(carrierOnlyJson.RootElement, "waybillLocal"));
    }

    [Fact]
    public void WaybillLocalProductGroupsBecomeUnifiedPreviewLinesWithoutReturningMarkingValues()
    {
        using var json = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": {
            "productgroups": [
              {
                "productinfo": {
                  "products": [
                    {
                      "ordno": 1,
                      "name": "Waybill product",
                      "catalogcode": "08415001001002073",
                      "catalogname": "MXIK catalog product",
                      "packagecode": "796",
                      "packagename": "dona",
                      "amount": 2,
                      "price": 10.5,
                      "deliverysum": 21,
                      "vat": { "rate": 12, "sum": 2.52 },
                      "deliverysumwithvat": 23.52,
                      "markingCount": 2,
                      "marks": { "markingCodes": ["raw-marking-1", "raw-marking-2"] }
                    }
                  ]
                }
              }
            ]
          }
        }
        """);

        var lines = EdocsEdoOperations.ReadPreviewLines(json.RootElement);

        var line = Assert.Single(lines);
        Assert.Equal("08415001001002073", line.CatalogCode);
        Assert.Equal("Waybill product", line.CatalogName);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal("796", line.PackageCode);
        Assert.Equal("dona", line.PackageName);
        Assert.Equal(10.5m, line.UnitPrice);
        Assert.Equal(12m, line.VatRate);
        Assert.Equal(2, line.MarkingCount);
        Assert.Equal(2, line.MarkingCodes.Count);

        var jsonLine = JsonSerializer.Serialize(line);
        Assert.DoesNotContain("raw-marking", jsonLine, StringComparison.Ordinal);
    }

    [Fact]
    public void WaybillLocalPreviewLinesBecomeNonEmptyUnifiedLines()
    {
        using var json = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": {
            "productgroups": [
              {
                "productinfo": {
                  "products": [
                    {
                      "ordno": 1,
                      "name": "Waybill product",
                      "catalogcode": "08415001001002073",
                      "packagecode": "796",
                      "packagename": "dona",
                      "amount": 2,
                      "price": 10.5,
                      "deliverysum": 21,
                      "vat": { "rate": 12, "sum": 2.52 },
                      "deliverysumwithvat": 23.52
                    }
                  ]
                }
              }
            ]
          }
        }
        """);

        var previewLines = EdocsEdoOperations.ReadPreviewLines(json.RootElement);
        var result = EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
            CreateDocument(documentType: "waybillLocal", lines: previewLines),
            "edocs-provider-id",
            "WAYBILL_LOCAL",
            allowSentDocuments: true);

        var line = Assert.Single(result.Lines);
        Assert.Equal("08415001001002073", line.ProviderProductCode);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal(10.5m, line.UnitPrice);
        Assert.Equal(12m, line.VatRate);
        Assert.Equal(2.52m, line.VatAmount);
        Assert.Equal(23.52m, line.TotalWithVat);
    }

    [Fact]
    public void LiveWaybillLocalRoadwayProductGroupsBecomeNonEmptyUnifiedLines()
    {
        using var json = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": {
            "roadway": {
              "productgroups": [
                {
                  "productinfo": {
                    "products": [
                      {
                        "ordno": 1,
                        "productname": "Waybill product",
                        "catalogcode": "08415001001002073",
                        "catalogname": "MXIK catalog product",
                        "packagecode": "796",
                        "packagename": "dona",
                        "amount": 2,
                        "price": 10.5,
                        "deliverysum": 21,
                        "vat": { "rate": 12, "sum": 2.52 },
                        "deliverysumwithvat": 23.52,
                        "markingCount": 2,
                        "marks": { "markingCodes": ["raw-marking-1", "raw-marking-2"] }
                      }
                    ]
                  }
                }
              ]
            }
          }
        }
        """);

        var lines = EdocsEdoOperations.ReadPreviewLines(json.RootElement);

        var line = Assert.Single(lines);
        Assert.Equal("08415001001002073", line.CatalogCode);
        Assert.Equal("Waybill product", line.CatalogName);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal(10.5m, line.UnitPrice);
        Assert.Equal(12m, line.VatRate);
        Assert.Equal(2.52m, line.VatAmount);
        Assert.Equal(23.52m, line.TotalWithVat);
        Assert.Equal(2, line.MarkingCount);

        var jsonLine = JsonSerializer.Serialize(line);
        Assert.DoesNotContain("raw-marking", jsonLine, StringComparison.Ordinal);
    }

    [Fact]
    public void WaybillLocalWithoutProductsRemainsSafelyBlocked()
    {
        using var json = JsonDocument.Parse("""
        {
          "type": "waybillLocal",
          "data": { "productgroups": [] }
        }
        """);

        var previewLines = EdocsEdoOperations.ReadPreviewLines(json.RootElement);
        Assert.Empty(previewLines);

        var exception = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
                CreateDocument(documentType: "waybillLocal", lines: previewLines),
                "edocs-provider-id",
                "WAYBILL_LOCAL",
                allowSentDocuments: true));

        Assert.Equal("EDO_OUTBOX_LINES_INVALID", exception.Code);
    }

    [Fact]
    public void WaybillLocalMissingLineVatOrTotalWithVatRemainsSafelyBlocked()
    {
        var line = new EdoDocumentPreviewLineDto
        {
            Number = 1,
            CatalogCode = "08415001001002073",
            Quantity = 2m,
            UnitPrice = 10.5m,
            NetAmount = 21m
        };

        var exception = Assert.Throws<EdoOutboxProviderDocumentException>(() =>
            EdoOutboxProviderDocumentDetailMapper.MapAndValidate(
                CreateDocument(documentType: "waybillLocal", lines: [line]),
                "edocs-provider-id",
                "WAYBILL_LOCAL",
                allowSentDocuments: true));

        Assert.Equal("EDO_OUTBOX_LINES_INVALID", exception.Code);
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
