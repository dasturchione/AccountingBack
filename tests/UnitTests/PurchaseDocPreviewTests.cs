using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features.PurchaseDocs;
using Domain.Entities;
using Integration.Didox.Facturas;
using Integration.Didox.Services;
using Integration.Edocs.Facturas;
using Integration.Edo.Providers;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;
using SharedKernel.Text;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace UnitTests;

public sealed class PurchaseDocPreviewTests
{
    [Fact]
    public void Marked_non_piece_goods_have_a_distinct_controlled_error()
    {
        var product = new Product
        {
            Id = 30,
            IsService = false,
            IsPieceTracked = false
        };

        var failure = PurchaseDocService.GetMarkingValidationErrorCode(
            product,
            quantity: 1,
            providerMarkings: ["mark-1"],
            requestItems: [new PurchaseDocLineItemDto { MarkingNumber = "mark-1" }]);

        Assert.Equal("PRODUCT_PIECE_TRACKING_REQUIRED", failure);
    }

    [Fact]
    public void Historical_draft_items_follow_selected_local_product_type()
    {
        var document = new EdoDocumentDto
        {
            PreviewLines =
            [
                new EdoDocumentPreviewLineDto
                {
                    Number = 1,
                    IsService = false,
                    Quantity = 2m,
                    UnitPrice = 50m,
                    NetAmount = 100m,
                    VatAmount = 12m,
                    TotalWithVat = 112m
                }
            ]
        };
        var requestLines = new[]
        {
            new PurchaseDocFromEdoLineDto
            {
                LineNumber = 1,
                ProductId = 30,
                UnitId = 1,
                VatRateId = 2,
                DebitAccountId = 101,
                VatAccountId = 102,
                Items =
                [
                    new PurchaseDocLineItemDto
                    {
                        MarkingNumber = "marking-1",
                        SerialNumber = "serial-1"
                    },
                    new PurchaseDocLineItemDto
                    {
                        MarkingNumber = "marking-2",
                        SerialNumber = "serial-2"
                    }
                ]
            }
        };
        var errors = new List<PurchaseDocPreviewValidationErrorDto>();

        var serviceLines = PurchaseDocService.BuildPurchaseLinesFromEdo(
            document,
            requestLines,
            [new PurchaseDocPreviewLineDto
            {
                Number = 1,
                ProductId = 30,
                UnitId = 1,
                VatRateId = 2,
                IsService = true,
                IsResolved = true
            }],
            errors);
        var goodsLines = PurchaseDocService.BuildPurchaseLinesFromEdo(
            document,
            requestLines,
            [new PurchaseDocPreviewLineDto
            {
                Number = 1,
                ProductId = 30,
                UnitId = 1,
                VatRateId = 2,
                IsService = false,
                IsResolved = true
            }],
            errors);

        var serviceLine = Assert.Single(serviceLines);
        Assert.Empty(serviceLine.Items);
        Assert.Equal(100m, serviceLine.ProviderNetAmount);
        Assert.Equal(12m, serviceLine.ProviderVatAmount);
        Assert.Equal(112m, serviceLine.ProviderTotalAmount);
        Assert.Equal(101, serviceLine.DebitAccountId);
        Assert.Equal(102, serviceLine.VatAccountId);
        var goodsItems = Assert.Single(goodsLines).Items;
        Assert.Equal(["marking-1", "marking-2"],
            goodsItems.Select(item => item.MarkingNumber));
        Assert.Equal(["serial-1", "serial-2"],
            goodsItems.Select(item => item.SerialNumber));
        Assert.Empty(errors);
    }

    [Fact]
    public async Task FromEdoRequestRequiresContractAndLocalMappings()
    {
        var validator = new PurchaseDocFromEdoRequestDtoValidator();
        var result = await validator.ValidateAsync(new PurchaseDocFromEdoRequestDto
        {
            DocumentIdentity = "edo-1",
            CounterpartyId = 10,
            WarehouseId = 20,
            CurrencyId = 1,
            Lines =
            [new PurchaseDocFromEdoLineDto
            {
                LineNumber = 1,
                ProductId = 30,
                UnitId = 1
            }]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "ContractId");
    }

    [Fact]
    public async Task FromEdoRequestRejectsDuplicateLineNumbersAndMarkings()
    {
        var validator = new PurchaseDocFromEdoRequestDtoValidator();
        var result = await validator.ValidateAsync(new PurchaseDocFromEdoRequestDto
        {
            DocumentIdentity = "edo-1",
            CounterpartyId = 10,
            ContractId = 40,
            WarehouseId = 20,
            CurrencyId = 1,
            Lines =
            [
                new PurchaseDocFromEdoLineDto
                {
                    LineNumber = 1,
                    ProductId = 30,
                    UnitId = 1,
                    Items = [new PurchaseDocLineItemDto { MarkingNumber = "mark-1" }, new PurchaseDocLineItemDto { MarkingNumber = "mark-1" }]
                },
                new PurchaseDocFromEdoLineDto
                {
                    LineNumber = 1,
                    ProductId = 31,
                    UnitId = 1
                }
            ]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Lines");
        Assert.Contains(result.Errors, error => error.PropertyName.Contains("Items", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PreviewRequestAcceptsMarkingNumberAndSerialNumber()
    {
        var validator = new PurchaseDocPreviewRequestDtoValidator();
        var result = await validator.ValidateAsync(new PurchaseDocPreviewRequestDto
        {
            DocumentIdentity = "didox-goods",
            Lines =
            [
                new PurchaseDocPreviewLineMappingDto
                {
                    LineNumber = 1,
                    ProductId = 63,
                    UnitId = 1,
                    VatRateId = 2,
                    Items =
                    [
                        new PurchaseDocLineItemDto
                        {
                            MarkingNumber = "marking-1",
                            SerialNumber = "serial-1"
                        }
                    ]
                }
            ]
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FromEdoRequestDoesNotAcceptProviderOrServerGeneratedIdentityFields()
    {
        var names = typeof(PurchaseDocFromEdoRequestDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("ProviderCode", names);
        Assert.DoesNotContain("ProviderStatus", names);
        Assert.DoesNotContain("DocDate", names);
        Assert.DoesNotContain("ProcessingMode", names);
    }

    [Fact]
    public void DraftImportMustUseStepByStepMode()
    {
        var createDto = new PurchaseDocCreateDto();

        Assert.Equal(PurchaseProcessingMode.StepByStep, createDto.ProcessingMode);
        Assert.NotEqual(PurchaseProcessingMode.Immediate, createDto.ProcessingMode);
    }

    [Fact]
    public async Task EdocsSignedDetailEvidenceIsMappedForPreview()
    {
        var operations = new EdocsEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("""
                {
                  "_id": "edocs-1",
                  "type": "factura",
                  "status": "signed",
                  "documentNumber": "3759",
                  "documentDate": "2026-07-31",
                  "sellerTin": "207164728",
                  "contractNumber": "NAV-192",
                  "contractDate": "2026-07-01",
                  "catalogCode": "09904002001000000",
                  "packageCode": "1500123",
                  "quantity": 1.67,
                  "unitPrice": 85500,
                  "vatRate": 12,
                  "totalWithVat": 159919.2
                }
                """)));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX, "FACTURA", "edocs-1", CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.SIGNED, document.Status.Code);
        Assert.Equal("3759", document.DocumentNumber);
        Assert.Equal(new DateOnly(2026, 7, 31), document.DocumentDate);
        Assert.Equal("207164728", document.PreviewSellerTin);
        Assert.Equal("NAV-192", document.PreviewContractNumber);
        Assert.Equal(159919.2m, document.TotalAmount);
        Assert.Single(document.PreviewLines);
    }

    [Fact]
    public async Task EdocsNestedFacturaDetailIsMappedForPreview()
    {
        var operations = new EdocsEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("""
                {
                  "_id": "6a6da4810cb5d64f5f7cc69b",
                  "type": "factura",
                  "status": "signed",
                  "docDate": "2026-07-31T00:00:00.000Z",
                  "docNumber": "3759",
                  "ownerTin": "207164728",
                  "totalSumWithVat": 159919.2,
                  "data": {
                    "sellertin": "207164728",
                    "contractdoc": {
                      "contractno": "NAV-192",
                      "contractdate": "2026-07-01T00:00:00Z"
                    },
                    "productlist": {
                      "products": [
                        {
                          "ordno": 1,
                          "catalogcode": "09904002001000000",
                          "packagename": "кубический метр",
                          "packagecode": "1500123",
                          "count": 1.67,
                          "summa": 85500,
                          "vatrate": 12,
                          "deliverysumwithvat": 159919.2,
                          "warehouseid": null,
                          "marks": null
                        }
                      ]
                    }
                  }
                }
                """)));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "6a6da4810cb5d64f5f7cc69b",
            CancellationToken.None);

        var line = Assert.Single(document.PreviewLines);
        Assert.Equal("6a6da4810cb5d64f5f7cc69b", document.ProviderDocumentId);
        Assert.Equal("3759", document.DocumentNumber);
        Assert.Equal(new DateOnly(2026, 7, 31), document.DocumentDate);
        Assert.Equal("207164728", document.PreviewSellerTin);
        Assert.Equal("NAV-192", document.PreviewContractNumber);
        Assert.Equal(new DateOnly(2026, 7, 1), document.PreviewContractDate);
        Assert.Equal(159919.2m, document.TotalAmount);
        Assert.Equal(1, line.Number);
        Assert.Equal("09904002001000000", line.CatalogCode);
        Assert.Equal("1500123", line.PackageCode);
        Assert.Equal("кубический метр", line.PackageName);
        Assert.Equal(1.67m, line.Quantity);
        Assert.Equal(85500m, line.UnitPrice);
        Assert.Equal(12m, line.VatRate);
        Assert.Equal(159919.2m, line.TotalWithVat);
    }

    [Fact]
    public async Task DidoxSignedDetailEvidenceIsMappedForPreview()
    {
        var operations = new DidoxEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("""
                {
                  "doc_id": "didox-1",
                  "data": {
                    "document": {
                      "doc_status": 3,
                      "documentNumber": "4686",
                      "documentDate": "2026-07-31",
                      "sellerTin": "300711863",
                      "contractNumber": "Public offer",
                      "contractDate": "2020-01-06",
                      "catalogCode": "10306002005000000",
                      "packageCode": "1545642",
                      "quantity": 475,
                      "unitPrice": 450,
                      "vatRate": 0,
                      "totalWithVat": 213750
                    }
                  }
                }
                """)),
            new DidoxTimestampClient(new StubHttpClientFactory(new StubHandler("{}"))));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX, "FACTURA", "didox-1", CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.SIGNED, document.Status.Code);
        Assert.Equal("3", document.Status.ProviderStatusCode);
        Assert.Equal("3", document.Status.ProviderRawStatus);
        Assert.Equal("4686", document.DocumentNumber);
        Assert.Equal(213750m, document.TotalAmount);
        Assert.Single(document.PreviewLines);
    }

    [Fact]
    public async Task DidoxNestedProductListParsesAllEighteenServiceLines()
    {
        var products = string.Join(",", Enumerable.Range(1, 18).Select(number => $$"""
            {
              "ordno": {{number}},
              "catalogcode": "10306002005000000",
              "catalogname": "Service item",
              "packagecode": "1545642",
              "packagename": "услуга (сум)",
              "count": 1.67,
              "summa": 450.25,
              "vatrate": 12,
              "vatsum": 90.05,
              "deliverysum": 751.92,
              "deliverysumwithvat": 841.97,
              "measureid": null
            }
            """));

        var operations = new DidoxEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler($$"""
                {
                  "doc_id": "1A085C428E4511F1A4A7FA163EF6A82D",
                  "data": {
                    "document": { "doc_status": 3 },
                    "json": {
                      "documentNumber": "163518/1082",
                      "documentDate": "2026-07-31",
                      "Seller": { "Name": "KAPITALBANK" },
                      "SellerTin": "207127843",
                      "Buyer": { "Name": "organization" },
                      "BuyerTin": "309142275",
                      "productlist": { "products": [{{products}}] }
                    }
                  }
                }
                """)),
            new DidoxTimestampClient(new StubHttpClientFactory(new StubHandler("{}"))));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "1A085C428E4511F1A4A7FA163EF6A82D",
            CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.SIGNED, document.Status.Code);
        Assert.Equal("163518/1082", document.DocumentNumber);
        Assert.Equal(new DateOnly(2026, 7, 31), document.DocumentDate);
        Assert.Equal("207127843", document.PreviewSellerTin);
        Assert.Equal("KAPITALBANK", document.Seller?.Name);
        Assert.Equal("309142275", document.Buyer?.TaxIdentifier);
        Assert.Equal(18, document.PreviewLines.Count);

        var first = document.PreviewLines.OrderBy(line => line.Number).First();
        Assert.Equal(1, first.Number);
        Assert.Equal("10306002005000000", first.CatalogCode);
        Assert.Equal("Service item", first.CatalogName);
        Assert.Equal("1545642", first.PackageCode);
        Assert.Equal("услуга (сум)", first.PackageName);
        Assert.True(first.IsService);
        Assert.Equal(1.67m, first.Quantity);
        Assert.Equal(450.25m, first.UnitPrice);
        Assert.Equal(12m, first.VatRate);
        Assert.Equal(751.92m, first.NetAmount);
        Assert.Equal(90.05m, first.VatAmount);
        Assert.Equal(841.97m, first.TotalWithVat);
    }

    [Fact]
    public async Task DidoxGoodsLinePreservesMarkingsAndProviderFinancialTotals()
    {
        const string firstMarking = "0104780105847408217m>_ikv9_AS7Cm%2dU3:";
        const string secondMarking = "0104780105847408217VP5rr,wKh6v%vpLfjs6";
        var operations = new DidoxEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler($$"""
                {
                  "doc_id": "8E02D7AC8FF911F183C6FA163EF6A82D",
                  "data": {
                    "document": { "doc_status": 3 },
                    "json": {
                      "documentNumber": "154",
                      "documentDate": "2026-08-04",
                      "productlist": {
                        "products": [
                          {
                            "ordno": 1,
                            "catalogcode": "08415001007001099",
                            "packagecode": "1533353",
                            "packagename": "шт. (потребительская коробка)",
                            "count": 2,
                            "summa": 13130000,
                            "deliverysum": 26260000,
                            "vatrate": 12,
                            "vatsum": 3151200,
                            "deliverysumwithvat": 29411200,
                            "hasmarking": true,
                            "producttype": 6,
                            "identtransupak": ["{{firstMarking}}"],
                            "kiz": ["{{secondMarking}}"],
                            "nomupak": []
                          }
                        ]
                      }
                    }
                  }
                }
                """)),
            new DidoxTimestampClient(new StubHttpClientFactory(new StubHandler("{}"))));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "8E02D7AC8FF911F183C6FA163EF6A82D",
            CancellationToken.None);

        var line = Assert.Single(document.PreviewLines);
        Assert.Equal("154", document.DocumentNumber);
        Assert.Equal(new DateOnly(2026, 8, 4), document.DocumentDate);
        Assert.Equal(2, line.MarkingCodes.Count);
        Assert.Contains(firstMarking, line.MarkingCodes);
        Assert.Contains(secondMarking, line.MarkingCodes);
        Assert.Equal(26260000m, line.NetAmount);
        Assert.Equal(3151200m, line.VatAmount);
        Assert.Equal(29411200m, line.TotalWithVat);

        var amounts = PurchaseDocService.ResolveLineAmounts(new PurchaseDocLineDto
        {
            Quantity = line.Quantity!.Value,
            UnitPrice = line.UnitPrice!.Value,
            ProviderNetAmount = line.NetAmount,
            ProviderVatAmount = line.VatAmount,
            ProviderTotalAmount = line.TotalWithVat
        }, line.VatRate);

        Assert.True(amounts.IsProviderSourced);
        Assert.Equal(26260000m, amounts.Amount);
        Assert.Equal(3151200m, amounts.VatAmount);
        Assert.Equal(29411200m, amounts.TotalAmount);
    }

    [Theory]
    [InlineData("identtransupak")]
    [InlineData("kiz")]
    [InlineData("nomupak")]
    public void DidoxConfirmedMarkingFieldsAreParsed(string propertyName)
    {
        using var payload = JsonDocument.Parse($$"""
            { "{{propertyName}}": ["marking-value"] }
            """);

        var marking = Assert.Single(
            DidoxDocumentResponseMapper.ReadMarkingCodes(payload.RootElement));

        Assert.Equal("marking-value", marking);
    }

    [Fact]
    public void DidoxProviderLineTotalsArePreservedAtTwoDecimalPrecision()
    {
        var providerLines = Enumerable.Range(1, 17)
            .Select(_ => new PurchaseDocLineDto
            {
                Quantity = 1m,
                UnitPrice = 1m,
                ProviderNetAmount = 20000m,
                ProviderVatAmount = 2400m,
                ProviderTotalAmount = 22400m
            })
            .Append(new PurchaseDocLineDto
            {
                Quantity = 1m,
                UnitPrice = 1m,
                ProviderNetAmount = 43035.76m,
                ProviderVatAmount = 5164.24m,
                ProviderTotalAmount = 48200m
            });

        var amounts = providerLines
            .Select(line => PurchaseDocService.ResolveLineAmounts(line, 12m))
            .ToList();

        Assert.All(amounts, amount => Assert.True(amount.IsProviderSourced));
        Assert.Equal(383035.76m, amounts.Sum(amount => amount.Amount));
        Assert.Equal(45964.24m, amounts.Sum(amount => amount.VatAmount));
        Assert.Equal(429000.00m, amounts.Sum(amount => amount.TotalAmount));

        var rounded = PurchaseDocService.ResolveLineAmounts(new PurchaseDocLineDto
        {
            Quantity = 1m,
            UnitPrice = 1m,
            ProviderNetAmount = 10.005m,
            ProviderVatAmount = 1.005m,
            ProviderTotalAmount = 11.005m
        }, 12m);

        Assert.Equal(10.01m, rounded.Amount);
        Assert.Equal(1.01m, rounded.VatAmount);
        Assert.Equal(11.01m, rounded.TotalAmount);
    }

    [Fact]
    public async Task DidoxMojibakeCyrillicProductAndPackageNamesAreNormalized()
    {
        const string expectedName = "услуга (сум)";
        const string mojibake = "\u00D1\u0192\u00D1\u0081\u00D0\u00BB\u00D1\u0192\u00D0\u00B3\u00D0\u00B0 (\u00D1\u0081\u00D1\u0192\u00D0\u00BC)";
        var payload = JsonSerializer.Serialize(new
        {
            doc_id = "didox-encoding",
            data = new
            {
                document = new { doc_status = 3 },
                json = new
                {
                    productlist = new
                    {
                        products = new[]
                        {
                            new
                            {
                                ordno = 1,
                                catalogcode = "10306002005000000",
                                catalogname = mojibake,
                                packagecode = "1545642",
                                packagename = mojibake,
                                count = 1.0m,
                                summa = 100m,
                                vatrate = 12m,
                                vatsum = 12m,
                                deliverysum = 100m,
                                deliverysumwithvat = 112m,
                                measureid = (int?)null
                            }
                        }
                    }
                }
            }
        });
        var operations = new DidoxEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler(payload)),
            new DidoxTimestampClient(new StubHttpClientFactory(new StubHandler("{}"))));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "didox-encoding",
            CancellationToken.None);

        var line = Assert.Single(document.PreviewLines);
        Assert.Equal(expectedName, line.CatalogName);
        Assert.Equal(expectedName, line.PackageName);
        Assert.True(line.IsService);
    }

    [Fact]
    public void Utf8MojibakeNormalizerNormalizesReportedLiveValuesAtThePreviewResponseBoundary()
    {
        const string validProductName = "Беспроцентные услуги";
        const string livePackageName = "ÑƒÑÐ»ÑƒÐ³Ð° (ÑÑƒÐ¼)";
        const string liveProductNamePrefix = "Ð‘ÐµÑÐ¿Ñ€Ð¾Ñ†ÐµÐ½Ñ‚Ð½Ñ‹Ðµ";

        Assert.Equal(validProductName, Utf8MojibakeNormalizer.Normalize(validProductName));
        Assert.Equal("услуга (сум)", Utf8MojibakeNormalizer.Normalize(livePackageName));
        Assert.Equal("Беспроцентные", Utf8MojibakeNormalizer.Normalize(liveProductNamePrefix));

        var preview = new PurchaseDocPreviewDto
        {
            Lines =
            [new PurchaseDocPreviewLineDto
            {
                ProductName = liveProductNamePrefix,
                PackageName = livePackageName,
                IsResolved = true
            }, new PurchaseDocPreviewLineDto
            {
                ProductName = validProductName,
                PackageName = "услуга (сум)",
                IsResolved = true
            }]
        };

        var json = JsonSerializer.Serialize(preview);
        using var response = JsonDocument.Parse(json);
        var mojibakeLine = response.RootElement.GetProperty("Lines")[0];
        Assert.Equal("Беспроцентные", mojibakeLine.GetProperty("ProductName").GetString());
        Assert.Equal("услуга (сум)", mojibakeLine.GetProperty("PackageName").GetString());

        var validLine = response.RootElement.GetProperty("Lines")[1];
        Assert.Equal(validProductName, validLine.GetProperty("ProductName").GetString());
        Assert.Equal("услуга (сум)", validLine.GetProperty("PackageName").GetString());
    }

    [Theory]
    [InlineData(0, EdoDocumentStatusCode.DRAFT)]
    [InlineData(1, EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING)]
    [InlineData(2, EdoDocumentStatusCode.PENDING_SIGNATURE)]
    [InlineData(3, EdoDocumentStatusCode.SIGNED)]
    [InlineData(4, EdoDocumentStatusCode.REJECTED)]
    [InlineData(5, EdoDocumentStatusCode.DELETED)]
    [InlineData(50, EdoDocumentStatusCode.ARCHIVED)]
    [InlineData(55, EdoDocumentStatusCode.DELETED)]
    [InlineData(60, EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING)]
    public void DidoxStatusMappingIsStable(int rawStatus, EdoDocumentStatusCode expected)
    {
        var mapped = EdoProviderStatusMapper.MapDidoxStatus(rawStatus);

        Assert.Equal(expected, mapped.Code);
        Assert.Equal(rawStatus.ToString(), mapped.ProviderStatusCode);
        Assert.Equal(rawStatus.ToString(), mapped.ProviderRawStatus);
    }

    [Theory]
    [InlineData("signed", EdoDocumentStatusCode.SIGNED)]
    [InlineData("drafts", EdoDocumentStatusCode.DRAFT)]
    [InlineData("rejected", EdoDocumentStatusCode.REJECTED)]
    [InlineData("deleted", EdoDocumentStatusCode.DELETED)]
    public void EdocsStatusMappingIsStable(string rawStatus, EdoDocumentStatusCode expected)
    {
        Assert.Equal(expected, EdoProviderStatusMapper.MapEdocsStatus(rawStatus).Code);
    }

    [Fact]
    public async Task Didox_numeric_status_string_is_accepted()
    {
        var operations = new DidoxEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("""{"doc_id":"didox-1","data":{"document":{"doc_status":"3"}}}""")),
            new DidoxTimestampClient(new StubHttpClientFactory(new StubHandler("{}"))));

        var document = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "didox-1",
            CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.SIGNED, document.Status.Code);
    }

    [Fact]
    public async Task InvalidEdocsStatusTypeIsControlled502()
    {
        var operations = new EdocsEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("""{"_id":"edocs-1","type":"factura","status":3}""")));

        var exception = await Assert.ThrowsAsync<SharedKernel.Exceptions.IntegrationHttpException>(() =>
            operations.GetDocumentDetailsAsync(EdoDirection.INBOX, "FACTURA", "edocs-1", CancellationToken.None));

        Assert.Equal(StatusCodes.Status502BadGateway, exception.StatusCode);
    }

    [Fact]
    public async Task ProviderNotFoundIsControlled404()
    {
        var operations = new EdocsEdoOperations(
            new TestUserContext(),
            new StubHttpClientFactory(new StubHandler("{}", HttpStatusCode.NotFound)));

        var exception = await Assert.ThrowsAsync<SharedKernel.Exceptions.IntegrationHttpException>(() =>
            operations.GetDocumentDetailsAsync(EdoDirection.INBOX, "FACTURA", "missing", CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public void PreviewRequestContainsOnlyMappingSelections()
    {
        var names = typeof(PurchaseDocPreviewRequestDto)
            .GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(
            ["DocumentIdentity", "CounterpartyId", "ContractId", "WarehouseId", "CurrencyId", "Lines"],
            names);
    }

    [Fact]
    public void PreviewResponseDefaultsToNonCreatableUntilMappingsAreResolved()
    {
        var preview = new PurchaseDocPreviewDto();

        Assert.False(preview.CanCreateDraft);
        Assert.Empty(preview.ValidationErrors);
        Assert.False(preview.Duplicate.IsDuplicate);
    }

    [Fact]
    public void UnlinkedEdoDocumentIsNotPreviewDuplicate()
    {
        var duplicate = PurchaseDocService.ResolvePreviewDuplicate(new EdoDocument
        {
            InternalDocumentType = "EDO_INBOX",
            InternalDocumentId = 0
        });

        Assert.False(duplicate.IsDuplicate);
        Assert.Null(duplicate.ExistingPurchaseId);
    }

    [Fact]
    public void PurchaseLinkedEdoDocumentIsPreviewDuplicate()
    {
        var duplicate = PurchaseDocService.ResolvePreviewDuplicate(new EdoDocument
        {
            InternalDocumentType = "PURCHASE",
            InternalDocumentId = 123
        });

        Assert.True(duplicate.IsDuplicate);
        Assert.Equal(123, duplicate.ExistingPurchaseId);
    }

    [Fact]
    public void SelectedContractWithGeneratedLocalNumberIsValidWhenScopeAndDatesMatch()
    {
        var contract = new Contract
        {
            OrganizationId = 11,
            CounterpartyId = 47,
            ContractNumber = "100000048",
            StateId = StateIdConst.ACTIVE,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Comment = "Provider contract NAV-192"
        };

        Assert.True(PurchaseDocService.IsSelectedContractValid(
            contract,
            organizationId: 11,
            counterpartyId: 47,
            documentDate: new DateTime(2026, 7, 31)));
    }

    [Fact]
    public void ExpiredContractIsRejectedByPreviewScopeRules()
    {
        var contract = new Contract
        {
            OrganizationId = 11,
            CounterpartyId = 47,
            StateId = StateIdConst.ACTIVE,
            EndDate = new DateTime(2026, 7, 30)
        };

        Assert.False(PurchaseDocService.IsSelectedContractValid(
            contract, 11, 47, new DateTime(2026, 7, 31)));
    }

    [Theory]
    [InlineData(12, 47)]
    [InlineData(11, 48)]
    public void WrongOrganizationOrCounterpartyContractIsRejected(int organizationId, int counterpartyId)
    {
        var contract = new Contract
        {
            OrganizationId = 11,
            CounterpartyId = 47,
            StateId = StateIdConst.ACTIVE,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31)
        };

        Assert.False(PurchaseDocService.IsSelectedContractValid(
            contract, organizationId, counterpartyId, new DateTime(2026, 7, 31)));
    }

    [Fact]
    public void ServiceUnitSelectionUsesTheExplicitActiveProductUnit()
    {
        var product = new Product { IsService = true, UnitId = 5 };
        var unit = new Unit { Id = 5, Code = "Xizmat", StateId = StateIdConst.ACTIVE };

        Assert.True(PurchaseDocService.IsSelectedUnitValid(product, unit));
    }

    [Fact]
    public void GoodsExplicitUnitIsValidWhenProductUnitMatchesRegardlessOfProviderPackageCode()
    {
        var sourceLine = new EdoDocumentPreviewLineDto
        {
            PackageCode = "1533353",
            PackageName = "шт. (потребительская коробка)"
        };
        var product = new Product { IsService = false, UnitId = 1 };
        var unit = new Unit { Id = 1, Code = "dona", StateId = StateIdConst.ACTIVE };

        Assert.True(PurchaseDocService.IsSelectedUnitValid(product, unit));
        Assert.Equal("1533353", sourceLine.PackageCode);
    }

    [Fact]
    public void GoodsSelectedUnitIsRejectedWhenItDoesNotMatchProductUnit()
    {
        var product = new Product { IsService = false, UnitId = 1 };
        var unit = new Unit { Id = 5, Code = "Xizmat", StateId = StateIdConst.ACTIVE };

        Assert.False(PurchaseDocService.IsSelectedUnitValid(product, unit));
    }

    [Fact]
    public void PieceTrackedQuantityTwoAcceptsTwoExactProviderMarkings()
    {
        var error = PurchaseDocService.GetMarkingValidationErrorCode(
            CreatePieceTrackedProduct(),
            2m,
            ["marking-1", "marking-2"],
            CreateMarkingItems("marking-1", "marking-2"));

        Assert.Null(error);
    }

    [Fact]
    public void PieceTrackedLineRejectsMissingMarking()
    {
        var error = PurchaseDocService.GetMarkingValidationErrorCode(
            CreatePieceTrackedProduct(),
            2m,
            ["marking-1", "marking-2"],
            CreateMarkingItems("marking-1"));

        Assert.Equal("MARKING_MAPPING_COUNT_MISMATCH", error);
    }

    [Fact]
    public void PieceTrackedLineRejectsExtraMarking()
    {
        var error = PurchaseDocService.GetMarkingValidationErrorCode(
            CreatePieceTrackedProduct(),
            2m,
            ["marking-1", "marking-2"],
            CreateMarkingItems("marking-1", "marking-2", "marking-3"));

        Assert.Equal("MARKING_MAPPING_COUNT_MISMATCH", error);
    }

    [Fact]
    public void PieceTrackedLineRejectsDuplicateMarking()
    {
        var error = PurchaseDocService.GetMarkingValidationErrorCode(
            CreatePieceTrackedProduct(),
            2m,
            ["marking-1", "marking-2"],
            CreateMarkingItems("marking-1", "marking-1"));

        Assert.Equal("MARKING_MAPPING_DUPLICATE", error);
    }

    [Fact]
    public void PieceTrackedLineRejectsMarkingOutsideProviderSet()
    {
        var error = PurchaseDocService.GetMarkingValidationErrorCode(
            CreatePieceTrackedProduct(),
            2m,
            ["marking-1", "marking-2"],
            CreateMarkingItems("marking-1", "different-marking"));

        Assert.Equal("MARKING_MAPPING_MISMATCH", error);
    }

    [Fact]
    public void EdocsServicePreviewLineCanUseExplicitLocalUnit()
    {
        var line = new PurchaseDocPreviewLineDto
        {
            Number = 1,
            ProductId = 61,
            UnitId = 5,
            IsService = true,
            ItemType = PurchaseDocPreviewItemType.SERVICE,
            Quantity = 1.67m,
            UnitPrice = 85500m,
            VatRate = 12m,
            TotalWithVat = 159919.2m,
            IsResolved = true,
            RequiresManualMapping = false
        };

        Assert.True(line.IsResolved);
        Assert.False(line.RequiresManualMapping);
        Assert.Equal(PurchaseDocPreviewItemType.SERVICE, line.ItemType);
        Assert.Equal((short)5, line.UnitId);
    }

    [Fact]
    public void PreviewInternalProviderMetadataIsNotSerialized()
    {
        var document = new EdoDocumentDto
        {
            ProviderCode = EdoProviderCode.EDOCS,
            ProviderDocumentId = "provider-id",
            Direction = EdoDirection.INBOX,
            Category = EdoDocumentCategory.INBOX,
            PreviewSellerTin = "hidden-tin",
            PreviewContractNumber = "hidden-contract",
            ProviderFields = new Dictionary<string, JsonElement>
            {
                ["token"] = JsonDocument.Parse("\"hidden\"").RootElement.Clone()
            }
        };

        var json = JsonSerializer.Serialize(document);

        Assert.DoesNotContain("PreviewSellerTin", json);
        Assert.DoesNotContain("hidden-tin", json);
        Assert.DoesNotContain("ProviderFields", json);
        Assert.DoesNotContain("hidden", json);
    }

    [Fact]
    public void PreviewDtoDoesNotExposeProviderSecurityFields()
    {
        var names = typeof(PurchaseDocPreviewDto)
            .GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Token", names);
        Assert.DoesNotContain("Cookie", names);
        Assert.DoesNotContain("Authorization", names);
        Assert.DoesNotContain("Pkcs7", names);
        Assert.DoesNotContain("Signature", names);
        Assert.DoesNotContain("ProviderFields", names);
    }

    [Fact]
    public void PreviewValidationErrorsArePublicAndActionable()
    {
        var dto = new PurchaseDocPreviewDto
        {
            ValidationErrors =
            [new PurchaseDocPreviewValidationErrorDto
            {
                Code = "PRODUCT_MAPPING_REQUIRED",
                Field = "lines[1].productId",
                Message = "Manual mapping is required."
            }]
        };

        Assert.False(dto.CanCreateDraft);
        Assert.Equal("PRODUCT_MAPPING_REQUIRED", dto.ValidationErrors.Single().Code);
    }

    private static Product CreatePieceTrackedProduct() => new()
    {
        IsService = false,
        IsPieceTracked = true,
        UnitId = 1
    };

    private static IReadOnlyCollection<PurchaseDocLineItemDto> CreateMarkingItems(params string[] markings) =>
        markings
            .Select(marking => new PurchaseDocLineItemDto { MarkingNumber = marking })
            .ToArray();

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId => 11;
        public List<int> AllowedOrganizationIds => [11];
        public int? BranchId => null;
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler)
        {
            BaseAddress = new Uri("https://provider.test/")
        };
    }

    private sealed class StubHandler(string body, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(JsonDocument.Parse(body).RootElement.Clone())
            };
            return Task.FromResult(response);
        }
    }
}
