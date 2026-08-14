using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Didox.Facturas;
using Integration.Didox.Configs;
using Integration.Didox.Historical;
using Integration.Didox.Http;
using Integration.Didox.Services;
using Integration.Edo.Historical;
using Integration.Edo.Providers;
using Integration.Edocs.Facturas;
using Integration.Edocs.Configs;
using Integration.Edocs.Historical;
using Integration.Edocs.Http;
using Integration.Shared.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Text;
using System.Net;
using System.Text;
using System.Text.Json;

namespace UnitTests;

public sealed class EdoHistoricalDocumentSourceTests
{
    [Fact]
    public void Provider_product_name_normalization_preserves_unicode_and_rejects_oversized_text()
    {
        Assert.True(Utf8MojibakeNormalizer.TryNormalizeProviderProductName(
            "  Кирпич\t\n силикатный  ",
            out var normalized));
        Assert.Equal("Кирпич силикатный", normalized);

        Assert.False(Utf8MojibakeNormalizer.TryNormalizeProviderProductName(
            new string('A', Utf8MojibakeNormalizer.ProviderProductNameMaxLength + 1),
            out _));
    }

    [Fact]
    public async Task Didox_list_uses_confirmed_signed_inbox_query_and_maps_pagination()
    {
        HttpRequestMessage? captured = null;
        var operations = CreateDidoxOperations(request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, """
                {
                  "data": [{
                    "doc_id": "D-1", "doc_status": 3, "doctype": "002",
                    "documentNumber": "42", "documentDate": "2026-07-31",
                    "Seller": { "Name": "Supplier" }, "SellerTin": "300000001",
                    "Buyer": { "Name": "Buyer" }, "BuyerTin": "309142275"
                  }],
                  "total": 201, "page": 2, "limit": 100,
                  "next_page_url": "https://provider.test/v2/documents?page=3&limit=100"
                }
                """);
        });

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto { Page = 2, PageSize = 100 },
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("/v2/documents?page=2&limit=100&owner=0&status=3&doctype=002", captured.RequestUri!.PathAndQuery);
        Assert.True(captured.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var organizationId));
        Assert.Equal(11, organizationId);
        Assert.Equal(201, result.ProviderTotal);
        Assert.True(result.HasNextPage);
        Assert.Equal(3, result.NextPage);
        Assert.Equal(EdoDocumentStatusCode.SIGNED, Assert.Single(result.Items).Status);
        Assert.False(result.IsCompletenessConfirmed);
        Assert.True(result.RequiresOverlapRescan);
    }

    [Fact]
    public async Task Didox_non_confirmed_status_is_not_import_ready()
    {
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, """
            { "data": [{ "doc_id": "D-33", "doc_status": 33, "doctype": "002" }], "total": 1, "next_page_url": null }
            """));

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto(),
            CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.UNKNOWN, Assert.Single(result.Items).Status);
    }

    [Fact]
    public async Task Didox_empty_and_missing_metadata_are_partial()
    {
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, "{\"data\":[]}"));

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto(),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Null(result.ProviderTotal);
        Assert.Null(result.HasNextPage);
        Assert.Equal(EdoHistoricalReadState.PARTIAL, result.State);
        Assert.True(result.RequiresOverlapRescan);
    }

    [Fact]
    public async Task Duplicate_and_repeated_page_are_detected_without_count_invention()
    {
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, """
            {
              "data": [
                { "doc_id": "D-1", "doc_status": 3, "doctype": "002" },
                { "doc_id": "D-1", "doc_status": 3, "doctype": "002" }
              ],
              "total": 9, "next_page_url": null
            }
            """));

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto
            {
                PreviousProviderTotal = 10,
                PreviousPageProviderDocumentIds = ["D-1"]
            },
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(1, result.DuplicateItemCount);
        Assert.True(result.IsRepeatedPage);
        Assert.True(result.ProviderTotalChanged);
        Assert.Equal(9, result.ProviderTotal);
    }

    [Fact]
    public async Task Edocs_list_uses_confirmed_query_without_date_filters()
    {
        HttpRequestMessage? captured = null;
        var operations = CreateEdocsOperations(request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, """
                {
                  "docs": [{
                    "_id": "E-1", "type": "factura", "status": "signed",
                    "docNumber": "7", "docDate": "2026-07-31T00:00:00Z",
                    "ownerTin": "300000001", "totalSumWithVat": 120,
                    "Buyer": { "Name": "Buyer", "VatRegCode": "309142275" }
                  }],
                  "totalDocs": 35, "totalPages": 2, "page": 1, "limit": 20,
                  "hasNextPage": true, "nextPage": 2
                }
                """);
        });

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(
            "/documents?sort=createdAt&order=-1&page=1&limit=20&io=in&status=signed&type=all",
            captured.RequestUri!.PathAndQuery);
        Assert.DoesNotContain("date", captured.RequestUri.Query, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(35, result.ProviderTotal);
        Assert.True(result.HasNextPage);
        Assert.Equal(2, result.NextPage);
        Assert.True(result.IsCompletenessConfirmed);
        Assert.Equal(EdoDocumentStatusCode.SIGNED, Assert.Single(result.Items).Status);
    }

    [Fact]
    public async Task Edocs_null_metadata_and_empty_page_are_safe()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            { "docs": [], "totalDocs": null, "hasNextPage": null, "nextPage": null }
            """));

        var result = await operations.ListHistoricalSignedInboxAsync(
            11,
            new EdoHistoricalPageRequestDto(),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Null(result.ProviderTotal);
        Assert.Null(result.HasNextPage);
        Assert.Null(result.NextPage);
        Assert.Equal(EdoHistoricalReadState.PARTIAL, result.State);
    }

    [Fact]
    public async Task Edocs_historical_list_preserves_provider_422_for_safe_classification()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.UnprocessableEntity, "{}"));

        var exception = await Assert.ThrowsAsync<IntegrationHttpException>(() =>
            operations.ListHistoricalSignedInboxAsync(
                11,
                new EdoHistoricalPageRequestDto { Page = 16, PageSize = 100 },
                CancellationToken.None));

        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public async Task Didox_detail_uses_owner_zero_and_reuses_confirmed_payload_parser()
    {
        HttpRequestMessage? captured = null;
        var operations = CreateDidoxOperations(request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, """
                {
                  "data": { "json": {
                    "doc_id": "D-DETAIL", "doc_status": 3, "doctype": "002",
                    "documentNumber": "4686", "documentDate": "2026-07-31",
                    "Seller": { "Name": "Supplier" }, "SellerTin": "300000001",
                    "Buyer": { "Name": "Buyer" }, "BuyerTin": "309142275",
                    "productlist": { "products": [{
                      "ordno": 1, "catalogcode": "10306002005000000",
                      "count": 2, "summa": 100, "deliverysum": 200,
                      "vatrate": 12, "vatsum": 24, "deliverysumwithvat": 224
                    }]}
                  }}
                }
                """);
        });

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "002",
            "D-DETAIL",
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("/v1/documents/D-DETAIL?owner=0", captured.RequestUri!.PathAndQuery);
        Assert.Equal("3", document.Status.ProviderStatusCode);
        Assert.Equal(new DateOnly(2026, 7, 31), document.DocumentDate);
        var line = Assert.Single(document.PreviewLines);
        Assert.Equal(200m, line.NetAmount);
        Assert.Equal(24m, line.VatAmount);
        Assert.Equal(224m, line.TotalWithVat);
    }

    [Theory]
    [InlineData("json", false)]
    [InlineData("json", true)]
    [InlineData("document_json", false)]
    [InlineData("document_json", true)]
    [InlineData("document", false)]
    [InlineData("document", true)]
    public async Task Didox_historical_detail_accepts_object_and_encoded_document_payloads(
        string payloadProperty,
        bool jsonEncoded)
    {
        var payload = """
            {
              "doc_id":"D-ENVELOPE","doc_status":"3","doctype":"002",
              "documentNumber":"42","documentDate":"2026-08-13",
              "Seller":{"Name":"Supplier"},"SellerTin":"300000001",
              "Buyer":{"Name":"Buyer"},"BuyerTin":"309142275",
              "productlist":{"products":[{"ordno":"1","catalogcode":"123","count":1,"summa":100,"deliverysumwithvat":100}]}
            }
            """;
        var nestedPayload = jsonEncoded ? JsonSerializer.Serialize(payload) : payload;
        var envelope = $$"""{ "data": { "{{payloadProperty}}": {{nestedPayload}} } }""";
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, envelope));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "002",
            "D-ENVELOPE",
            CancellationToken.None);

        Assert.Equal("3", document.Status.ProviderStatusCode);
        Assert.Equal(new DateOnly(2026, 8, 13), document.DocumentDate);
        Assert.Equal(1, Assert.Single(document.PreviewLines).Number);
    }

    [Theory]
    [InlineData("json", false, "contractNumber", "contractDate")]
    [InlineData("document_json", true, "ContractNo", "ContractDate")]
    [InlineData("document", false, "ContractDoc", "ContractDate")]
    public async Task Didox_historical_detail_reads_explicit_contract_metadata_from_bounded_payload_variants(
        string payloadProperty,
        bool jsonEncoded,
        string contractNumberProperty,
        string contractDateProperty)
    {
        var contract = contractNumberProperty == "ContractDoc"
            ? "\"ContractDoc\":{\"ContractNo\":\"CN-42\",\"ContractDate\":\"2026-08-01\"}"
            : $"\"{contractNumberProperty}\":\"CN-42\",\"{contractDateProperty}\":\"2026-08-01\"";
        var payload = $$"""
            {
              "doc_id":"D-CONTRACT","doc_status":3,"doctype":"002",
              "documentNumber":"42","documentDate":"2026-08-13",
              "Seller":{"Name":"Supplier"},"SellerTin":"300000001",
              "Buyer":{"Name":"Buyer"},"BuyerTin":"309142275",
              {{contract}},
              "productlist":{"products":[{"ordno":"1","catalogcode":"123","count":1,"summa":100,"deliverysumwithvat":100}]}
            }
            """;
        var nestedPayload = jsonEncoded ? JsonSerializer.Serialize(payload) : payload;
        var envelope = $$"""{ "data": { "{{payloadProperty}}": {{nestedPayload}} } }""";
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, envelope));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "002",
            "D-CONTRACT",
            CancellationToken.None);

        Assert.Equal("CN-42", document.PreviewContractNumber);
        Assert.Equal(new DateOnly(2026, 8, 1), document.PreviewContractDate);
    }

    [Theory]
    [InlineData("{\"data\":[]}", "DIDOX_DETAIL_ENVELOPE_INVALID")]
    [InlineData("{\"data\":{\"json\":\"not-json\"}}", "DIDOX_DETAIL_DOCUMENT_JSON_INVALID")]
    [InlineData("{\"data\":{\"json\":{\"doc_id\":\"D-DETAIL-CODE\",\"doc_status\":\"signed\"}}}", "DIDOX_DETAIL_STATUS_INVALID")]
    [InlineData("{\"data\":{\"json\":{\"doc_id\":\"D-OTHER\",\"doc_status\":3}}}", "DIDOX_DETAIL_IDENTITY_MISMATCH")]
    [InlineData("{\"data\":{\"json\":{\"doc_id\":\"D-DETAIL-CODE\",\"doc_status\":3,\"productlist\":{\"products\":{}}}}}", "DIDOX_DETAIL_LINES_INVALID")]
    public async Task Didox_historical_detail_reports_structural_safe_codes(
        string response,
        string expectedSafeCode)
    {
        var operations = CreateDidoxOperations(_ => Json(HttpStatusCode.OK, response));

        var exception = await Assert.ThrowsAsync<EdoHistoricalMappingException>(() =>
            operations.GetHistoricalDocumentDetailsAsync(
                11,
                "002",
                "D-DETAIL-CODE",
                CancellationToken.None));

        Assert.Equal(expectedSafeCode, exception.SafeFailureCode);
        Assert.DoesNotContain("not-json", exception.SafeFailureCode, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Edocs_detail_reuses_root_and_nested_parser_without_raw_result()
    {
        HttpRequestMessage? captured = null;
        var operations = CreateEdocsOperations(request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, """
                {
                  "_id": "E-DETAIL", "type": "factura", "status": "signed",
                  "docNumber": "3759", "docDate": "2026-07-31T00:00:00Z",
                  "ownerName": "Supplier", "ownerTin": "300000001",
                  "targetTins": [{ "side": "buyer", "name": "Buyer", "tin": "309142275" }],
                  "data": {
                    "contractdoc": { "contractno": "NAV-192", "contractdate": "2026-07-01T00:00:00Z" },
                    "productlist": { "products": [{
                      "ordno": 1, "catalogcode": "09904002001000000",
                      "count": 1.67, "summa": 85500, "deliverysum": 142785,
                      "vatrate": 12, "vatsum": 17134.2, "deliverysumwithvat": 159919.2
                    }]}
                  },
                  "totalSumWithVat": 159919.2
                }
                """);
        });

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-DETAIL",
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("/documents/factura/E-DETAIL", captured.RequestUri!.PathAndQuery);
        Assert.Equal("300000001", document.Seller!.TaxIdentifier);
        Assert.Equal("309142275", document.Buyer!.TaxIdentifier);
        Assert.Equal("NAV-192", document.PreviewContractNumber);
        var line = Assert.Single(document.PreviewLines);
        Assert.Equal(142785m, line.NetAmount);
        Assert.Equal(17134.2m, line.VatAmount);
        Assert.NotEmpty(document.ProviderFields);

        var normalized = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-DETAIL"),
            document);
        Assert.True(normalized.IsImportReady);
        Assert.DoesNotContain("ProviderFields", normalized.Document!.GetType().GetProperties().Select(x => x.Name));
    }

    [Fact]
    public async Task Edocs_detail_prefers_explicit_seller_tin_over_vat_registration_code()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-SELLER-EXPLICIT", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "buyertin": "309142275",
              "Seller": {
                "Name": "Supplier", "VatRegCode": "326010089277",
                "BankId": "BANK", "Account": "ACCOUNT", "Address": "ADDRESS"
              },
              "data": {
                "sellertin": " 207164728 ",
                "productlist": { "products": [{ "ordno": 1 }] }
              }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-SELLER-EXPLICIT",
            CancellationToken.None);

        Assert.Equal("207164728", document.Seller!.TaxIdentifier);
        Assert.Equal("207164728", document.PreviewSellerTin);
        Assert.Equal("Supplier", document.Seller.Name);
        Assert.Equal("BANK", document.Seller.BankCode);
        Assert.Equal("ACCOUNT", document.Seller.AccountNumber);
        Assert.Equal("ADDRESS", document.Seller.Address);
    }

    [Fact]
    public async Task Edocs_detail_uses_product_list_seller_tin_before_compatibility_fallbacks()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-SELLER-PRODUCTLIST", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "buyertin": "309142275",
              "ownerTin": "300000001",
              "Seller": { "Name": "Supplier", "VatRegCode": "326010089277" },
              "data": {
                "productlist": {
                  "tin": "207164728",
                  "products": [{ "ordno": 1 }]
                }
              }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-SELLER-PRODUCTLIST",
            CancellationToken.None);

        Assert.Equal("207164728", document.Seller!.TaxIdentifier);
        Assert.Equal("Supplier", document.Seller.Name);
    }

    [Fact]
    public async Task Edocs_detail_prefers_seller_object_tin_over_vat_registration_code()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-SELLER-OBJECT", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "buyertin": "309142275",
              "Seller": {
                "Name": "Supplier", "tin": "207164728",
                "VatRegCode": "326010089277"
              },
              "data": { "productlist": { "products": [{ "ordno": 1 }] } }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-SELLER-OBJECT",
            CancellationToken.None);

        Assert.Equal("207164728", document.Seller!.TaxIdentifier);
    }

    [Fact]
    public async Task Edocs_detail_rejects_invalid_explicit_seller_tin_without_hiding_it_with_fallbacks()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-SELLER-INVALID", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "buyertin": "309142275",
              "sellertin": "20716A728", "ownerTin": "207164728",
              "Seller": { "Name": "Supplier", "VatRegCode": "326010089277" },
              "targetTins": [{ "side": "seller", "tin": "207164728" }],
              "data": { "productlist": { "products": [{ "ordno": 1 }] } }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-SELLER-INVALID",
            CancellationToken.None);
        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-SELLER-INVALID"),
            document);

        Assert.Equal(string.Empty, document.Seller!.TaxIdentifier);
        Assert.False(result.IsImportReady);
        Assert.Equal("SELLER_TIN_REQUIRED", result.SafeFailureCode);
    }

    [Theory]
    [InlineData("\"buyertin\": \" 309142275 \"")]
    [InlineData("\"buyerTin\": \"309142275\"")]
    [InlineData("\"data\": { \"buyertin\": \"309142275\" }")]
    [InlineData("\"data\": { \"buyerTin\": \"309142275\" }")]
    public async Task Edocs_detail_maps_explicit_buyer_tin_before_compatibility_fallbacks(
        string buyerField)
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, $$"""
            {
              "_id": "E-BUYER-TIN", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "catalogCode": "12345678901234567",
              {{buyerField}},
              "Buyer": { "Name": "Buyer", "VatRegCode": "998877665" },
              "targetTins": [{ "side": "buyer", "tin": "998877664" }]
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-BUYER-TIN",
            CancellationToken.None);

        Assert.Equal("300000001", document.Seller!.TaxIdentifier);
        Assert.Equal("309142275", document.Buyer!.TaxIdentifier);

        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-BUYER-TIN"),
            document);

        Assert.True(result.IsImportReady);
    }

    [Fact]
    public async Task Edocs_detail_keeps_explicit_buyer_tin_mismatch()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-BUYER-MISMATCH", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "buyertin": "309142276"
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-BUYER-MISMATCH",
            CancellationToken.None);
        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-BUYER-MISMATCH"),
            document);

        Assert.False(result.IsImportReady);
        Assert.Equal("BUYER_ORGANIZATION_MISMATCH", result.SafeFailureCode);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("\"30914A275\"")]
    [InlineData("309142275")]
    public async Task Edocs_detail_rejects_invalid_explicit_buyer_tin_as_controlled_exclusion(
        string invalidBuyerTin)
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, $$"""
            {
              "_id": "E-BUYER-INVALID", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "buyertin": {{invalidBuyerTin}},
              "targetTins": [{ "side": "buyer", "tin": "309142275" }]
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-BUYER-INVALID",
            CancellationToken.None);
        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-BUYER-INVALID"),
            document);

        Assert.False(result.IsImportReady);
        Assert.Equal("BUYER_ORGANIZATION_MISMATCH", result.SafeFailureCode);
    }

    [Fact]
    public async Task Edocs_detail_accepts_invariant_numeric_strings_without_throwing()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-STRING-AMOUNTS", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "targetTins": [{ "side": "buyer", "tin": "309142275" }],
              "totalSumWithVat": "159919.20",
              "data": {
                  "productlist": { "products": [{
                  "ordno": "1", "count": "1.67", "summa": "85500",
                  "deliverysum": "142785", "vatrate": "12",
                  "vatsum": "17134.20", "deliverysumwithvat": "159919.20"
                }]}
              }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-STRING-AMOUNTS",
            CancellationToken.None);

        Assert.Equal(159919.20m, document.TotalAmount);
        var line = Assert.Single(document.PreviewLines);
        Assert.Equal(1, line.Number);
        Assert.Equal(1.67m, line.Quantity);
        Assert.Equal(142785m, line.NetAmount);
        Assert.Equal(17134.20m, line.VatAmount);
        Assert.Equal(159919.20m, line.TotalWithVat);
    }

    [Fact]
    public async Task Edocs_detail_preserves_nested_and_direct_marking_order_and_multiplicity()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-MARKINGS", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "sellertin": "300000001", "buyertin": "309142275",
              "data": {
                "productlist": { "products": [{
                  "ordno": 1, "count": 7,
                  "marks": {
                    "identtransupak": [" A ", "", "DUP"],
                    "kiz": ["B"],
                    "nomupak": ["C"]
                  },
                  "identtransupak": ["DUP"],
                  "kiz": ["D"],
                  "nomupak": ["E"]
                }]}
              }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11, "factura", "E-MARKINGS", CancellationToken.None);
        var line = Assert.Single(document.PreviewLines);

        Assert.Equal(["A", "DUP", "B", "C", "DUP", "D", "E"], line.MarkingCodes);
        var historical = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"),
            document);
        Assert.Equal(line.MarkingCodes, Assert.Single(historical.Document!.Lines).MarkingNumbers);
    }

    [Fact]
    public async Task Edocs_detail_keeps_missing_and_null_optional_decimals()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-OPTIONAL-AMOUNTS", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "targetTins": [{ "side": "buyer", "tin": "309142275" }],
              "totalSumWithVat": 100,
              "data": {
                "productlist": { "products": [{
                  "ordno": "1", "count": null, "vatrate": null,
                  "deliverysumwithvat": 100
                }]}
              }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-OPTIONAL-AMOUNTS",
            CancellationToken.None);

        Assert.Equal(100m, document.TotalAmount);
        var line = Assert.Single(document.PreviewLines);
        Assert.Equal(1, line.Number);
        Assert.Null(line.Quantity);
        Assert.Null(line.UnitPrice);
        Assert.Null(line.VatRate);
        Assert.Null(line.VatAmount);
        Assert.Equal(100m, line.TotalWithVat);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("1,25")]
    [InlineData("1,234.56")]
    public async Task Edocs_historical_detail_rejects_ambiguous_or_malformed_decimal_strings(
        string invalidDecimal)
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, $$"""
            {
              "_id": "E-INVALID-AMOUNT", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "targetTins": [{ "side": "buyer", "tin": "309142275" }],
              "data": {
                "productlist": { "products": [{
                  "ordno": "1", "count": "{{invalidDecimal}}"
                }]}
              }
            }
            """));

        var exception = await Assert.ThrowsAsync<EdoHistoricalMappingException>(() =>
            operations.GetHistoricalDocumentDetailsAsync(
                11,
                "factura",
                "E-INVALID-AMOUNT",
                CancellationToken.None));

        Assert.Equal("EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID", exception.SafeFailureCode);
    }

    [Fact]
    public async Task Readiness_is_organization_scoped_and_never_returns_credentials()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edo-historical-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var environment = new StubHostEnvironment(root);
            var protection = new EphemeralDataProtectionProvider();
            var didoxCache = new DidoxTokenCache(
                protection,
                Options.Create(new DidoxTokenStorageOptions { RootPath = "didox" }),
                environment);
            var edocsCache = new EdocsTokenCache(
                protection,
                Options.Create(new EdocsTokenStorageOptions { RootPath = "edocs" }),
                environment);
            didoxCache.Set(11, "not-exposed", TimeSpan.FromHours(1));
            edocsCache.Set(11, IntegrationProviderConst.Edocs, "not-exposed", TimeSpan.FromHours(1));
            var organizationReader = new StubOrganizationSourceReader(
                new Dictionary<int, string> { [11] = "309142275", [22] = "309142276" });
            var noNetwork = new Func<HttpRequestMessage, HttpResponseMessage>(_ =>
                throw new InvalidOperationException("Readiness must not make a provider request."));
            var didox = new DidoxHistoricalDocumentSource(
                CreateDidoxOperations(noNetwork), didoxCache, organizationReader);
            var edocs = new EdocsHistoricalDocumentSource(
                CreateEdocsOperations(noNetwork), edocsCache, organizationReader);

            var didoxReady = await didox.CheckReadinessAsync(new EdoHistoricalExecutionContextDto(11));
            var didoxOtherOrganization = await didox.CheckReadinessAsync(new EdoHistoricalExecutionContextDto(22));
            var edocsReady = await edocs.CheckReadinessAsync(new EdoHistoricalExecutionContextDto(11));

            Assert.True(didoxReady.IsSessionReady);
            Assert.True(edocsReady.IsSessionReady);
            Assert.False(didoxOtherOrganization.IsSessionReady);
            Assert.Equal(EdoHistoricalReadState.WAITING_AUTH, didoxOtherOrganization.State);
            Assert.DoesNotContain("Token", didoxReady.GetType().GetProperties().Select(x => x.Name));

            var unauthorizedSource = new DidoxHistoricalDocumentSource(
                CreateDidoxOperations(_ => Json(HttpStatusCode.Unauthorized, "{}")),
                didoxCache,
                organizationReader);
            var unauthorized = await unauthorizedSource.ReadInboxPageAsync(
                new EdoHistoricalExecutionContextDto(11),
                new EdoHistoricalPageRequestDto(),
                CancellationToken.None);
            Assert.Equal(EdoHistoricalReadState.WAITING_AUTH, unauthorized.State);
            Assert.Equal(EdoHistoricalSourceSupport.AuthRequired, unauthorized.SafeFailureCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(EdoProviderCode.DIDOX, "3", "002", true)]
    [InlineData(EdoProviderCode.DIDOX, "33", "002", false)]
    [InlineData(EdoProviderCode.EDOCS, "signed", "factura", true)]
    [InlineData(EdoProviderCode.EDOCS, "signed", "contract", false)]
    public void Detail_revalidates_signed_factura(
        EdoProviderCode provider,
        string providerStatus,
        string documentType,
        bool expectedReady)
    {
        var request = HistoricalDetailRequest(provider, documentType);
        var document = ValidDocument(provider, providerStatus, documentType);

        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            provider,
            "309142275",
            request,
            document);

        Assert.Equal(expectedReady, result.IsImportReady);
        Assert.DoesNotContain("ProviderFields", result.Document!.GetType().GetProperties().Select(x => x.Name));
    }

    [Fact]
    public void Detail_date_range_uses_document_date()
    {
        var request = WithDates(HistoricalDetailRequest(EdoProviderCode.DIDOX, "002"),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31));
        var document = WithDate(
            ValidDocument(EdoProviderCode.DIDOX, "3", "002"),
            new DateOnly(2026, 7, 31));

        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.DIDOX,
            "309142275",
            request,
            document);

        Assert.False(result.IsImportReady);
        Assert.Equal("DOCUMENT_OUTSIDE_DATE_RANGE", result.SafeFailureCode);
    }

    [Fact]
    public void Detail_revalidates_direction_identity_buyer_and_seller()
    {
        var request = HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura");
        var wrongDirection = Copy(ValidDocument(EdoProviderCode.EDOCS, "signed", "factura"), direction: EdoDirection.OUTBOX);
        var directionResult = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS, "309142275", request, wrongDirection);

        Assert.Equal("DOCUMENT_DIRECTION_NOT_INBOX", directionResult.SafeFailureCode);

        var wrongBuyer = Copy(
            ValidDocument(EdoProviderCode.EDOCS, "signed", "factura"),
            buyer: new EdoPartyDto { Name = "Other", TaxIdentifier = "999" });
        var buyerResult = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS, "309142275", request, wrongBuyer);
        Assert.Equal("BUYER_ORGANIZATION_MISMATCH", buyerResult.SafeFailureCode);

        var missingSeller = Copy(
            ValidDocument(EdoProviderCode.EDOCS, "signed", "factura"),
            seller: null,
            previewSellerTin: null);
        var sellerResult = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS, "309142275", request, missingSeller);
        Assert.Equal("SELLER_TIN_REQUIRED", sellerResult.SafeFailureCode);
    }

    [Theory]
    [InlineData("[]", "DOCUMENT_LINES_REQUIRED")]
    [InlineData("[{ \"ordno\": 0 }]", "PROVIDER_LINE_NUMBER_INVALID")]
    [InlineData("[{ \"ordno\": -1 }]", "PROVIDER_LINE_NUMBER_INVALID")]
    [InlineData("[{ \"ordno\": 1 }, { \"ordno\": 1 }]", "PROVIDER_LINE_NUMBER_DUPLICATE")]
    public async Task Edocs_historical_detail_rejects_invalid_line_integrity(
        string products,
        string expectedSafeCode)
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, $$"""
            {
              "_id": "E-LINE-INTEGRITY", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "buyertin": "309142275",
              "data": { "productlist": { "products": {{products}} } }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-LINE-INTEGRITY",
            CancellationToken.None);
        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-LINE-INTEGRITY"),
            document);

        Assert.False(result.IsImportReady);
        Assert.Equal(expectedSafeCode, result.SafeFailureCode);
        Assert.Equal(products.Count(character => character == '{'), document.PreviewLines.Count);
    }

    [Fact]
    public async Task Edocs_historical_detail_keeps_unique_line_numbers_unchanged()
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, """
            {
              "_id": "E-UNIQUE-LINES", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "buyertin": "309142275",
              "data": { "productlist": { "products": [
                { "ordno": 2 }, { "ordno": 7 }
              ] } }
            }
            """));

        var document = await operations.GetHistoricalDocumentDetailsAsync(
            11,
            "factura",
            "E-UNIQUE-LINES",
            CancellationToken.None);
        var result = EdoHistoricalSourceSupport.ValidateAndMapDetail(
            EdoProviderCode.EDOCS,
            "309142275",
            WithItem(HistoricalDetailRequest(EdoProviderCode.EDOCS, "factura"), "E-UNIQUE-LINES"),
            document);

        Assert.True(result.IsImportReady);
        Assert.Equal([2, 7], result.Document!.Lines.Select(line => line.Number));
    }

    [Theory]
    [InlineData("{ \"ordno\": \"invalid\" }")]
    [InlineData("{ \"catalogcode\": \"12345678901234567\" }")]
    public async Task Edocs_historical_detail_reports_malformed_line_number_with_safe_code(
        string product)
    {
        var operations = CreateEdocsOperations(_ => Json(HttpStatusCode.OK, $$"""
            {
              "_id": "E-MALFORMED-LINE", "type": "factura", "status": "signed",
              "docDate": "2026-07-31", "ownerTin": "300000001",
              "buyertin": "309142275",
              "data": { "productlist": { "products": [{{product}}] } }
            }
            """));

        var exception = await Assert.ThrowsAsync<EdoHistoricalMappingException>(() =>
            operations.GetHistoricalDocumentDetailsAsync(
                11,
                "factura",
                "E-MALFORMED-LINE",
                CancellationToken.None));
        var failure = EdoHistoricalSourceSupport.BuildFailureDetail(
            EdoProviderCode.EDOCS,
            exception,
            "EDOCS_HISTORICAL_DETAIL");

        Assert.Equal(EdoHistoricalReadState.VALIDATION_FAILURE, failure.State);
        Assert.Equal("PROVIDER_LINE_NUMBER_INVALID", failure.SafeFailureCode);
    }

    [Fact]
    public void Provider_failures_are_classified_without_exposing_messages()
    {
        var request = new EdoHistoricalPageRequestDto();
        var auth = EdoHistoricalSourceSupport.BuildFailurePage(
            EdoProviderCode.DIDOX,
            request,
            new IntegrationUnauthorizedException("contains-sensitive-provider-message"));
        var transient = EdoHistoricalSourceSupport.BuildFailurePage(
            EdoProviderCode.DIDOX,
            request,
            new IntegrationHttpException("provider body", 503));
        var terminal = EdoHistoricalSourceSupport.BuildFailurePage(
            EdoProviderCode.DIDOX,
            request,
            new IntegrationHttpException("provider body", 404));
        var timeout = EdoHistoricalSourceSupport.BuildFailurePage(
            EdoProviderCode.DIDOX,
            request,
            new TaskCanceledException("provider timeout"));

        Assert.Equal(EdoHistoricalReadState.WAITING_AUTH, auth.State);
        Assert.Equal(EdoHistoricalSourceSupport.AuthRequired, auth.SafeFailureCode);
        Assert.Equal(EdoHistoricalReadState.TRANSIENT_FAILURE, transient.State);
        Assert.Equal(EdoHistoricalReadState.TRANSIENT_FAILURE, timeout.State);
        Assert.Equal(EdoHistoricalReadState.TERMINAL_PROVIDER_FAILURE, terminal.State);
        Assert.DoesNotContain("provider body", terminal.SafeFailureCode, StringComparison.Ordinal);

        var edocsList = EdoHistoricalSourceSupport.BuildFailurePage(
            EdoProviderCode.EDOCS,
            request,
            new IntegrationHttpException("provider body", 422),
            "EDOCS_HISTORICAL_LIST");
        var edocsDetail = EdoHistoricalSourceSupport.BuildFailureDetail(
            EdoProviderCode.EDOCS,
            new IntegrationHttpException("provider body", 404),
            "EDOCS_HISTORICAL_DETAIL");
        Assert.Equal("EDOCS_HISTORICAL_LIST_HTTP_422", edocsList.SafeFailureCode);
        Assert.Equal("EDOCS_HISTORICAL_DETAIL_HTTP_404", edocsDetail.SafeFailureCode);
    }

    [Fact]
    public async Task Explicit_organization_scope_does_not_leak_between_parallel_calls()
    {
        var observed = new List<int>();
        var gate = new object();
        var operations = CreateDidoxOperations(request =>
        {
            Assert.True(request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var organizationId));
            lock (gate)
                observed.Add(organizationId);
            return Json(HttpStatusCode.OK, "{\"data\":[],\"total\":0,\"next_page_url\":null}");
        });

        await Task.WhenAll(
            operations.ListHistoricalSignedInboxAsync(11, new EdoHistoricalPageRequestDto(), CancellationToken.None),
            operations.ListHistoricalSignedInboxAsync(22, new EdoHistoricalPageRequestDto(), CancellationToken.None));

        Assert.Equal([11, 22], observed.Order());
    }

    [Fact]
    public void Historical_registry_resolution_uses_snapshot_code_not_active_provider()
    {
        var didox = new StubHistoricalSource(EdoProviderCode.DIDOX);
        var edocs = new StubHistoricalSource(EdoProviderCode.EDOCS);
        var registry = new EdoProviderRegistry([], [didox, edocs]);

        Assert.Same(didox, registry.ResolveHistoricalSource(EdoProviderCode.DIDOX));
        Assert.Same(edocs, registry.ResolveHistoricalSource(EdoProviderCode.EDOCS));
    }

    [Theory]
    [InlineData(429, 2)]
    [InlineData(503, 2)]
    [InlineData(400, 1)]
    [InlineData(401, 1)]
    [InlineData(403, 1)]
    [InlineData(404, 1)]
    public async Task Didox_retry_policy_retries_only_transient_statuses(int statusCode, int expectedAttempts)
    {
        var attempts = 0;
        var retry = new DidoxGetRetryHandler(NullLogger<DidoxGetRetryHandler>.Instance)
        {
            InnerHandler = new LambdaHandler(_ =>
            {
                attempts++;
                return attempts == 1
                    ? new HttpResponseMessage((HttpStatusCode)statusCode)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            })
        };
        using var client = new HttpClient(retry);

        using var response = await client.GetAsync("https://provider.test/read", CancellationToken.None);

        Assert.Equal(expectedAttempts, attempts);
        Assert.Equal(expectedAttempts == 1 ? (HttpStatusCode)statusCode : HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Edocs_retry_policy_honors_retry_after_and_retries_429()
    {
        var attempts = 0;
        var retry = new EdocsGetRetryHandler(NullLogger<EdocsGetRetryHandler>.Instance)
        {
            InnerHandler = new LambdaHandler(_ =>
            {
                attempts++;
                if (attempts > 1)
                    return new HttpResponseMessage(HttpStatusCode.OK);

                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                return response;
            })
        };
        using var client = new HttpClient(retry);

        using var response = await client.GetAsync("https://provider.test/read", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attempts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Network_and_timeout_failures_are_retried(bool timeout)
    {
        var attempts = 0;
        var retry = new DidoxGetRetryHandler(NullLogger<DidoxGetRetryHandler>.Instance)
        {
            InnerHandler = new LambdaHandler(_ =>
            {
                attempts++;
                if (attempts == 1)
                {
                    if (timeout)
                        throw new TaskCanceledException("provider timeout");

                    throw new HttpRequestException("provider network failure");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            })
        };
        using var client = new HttpClient(retry);

        using var response = await client.GetAsync("https://provider.test/read", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task Retry_delay_honors_cancellation()
    {
        var retry = new DidoxGetRetryHandler(NullLogger<DidoxGetRetryHandler>.Instance)
        {
            InnerHandler = new LambdaHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
        };
        using var client = new HttpClient(retry);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(25));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetAsync("https://provider.test/read", cts.Token));
    }

    private static DidoxEdoOperations CreateDidoxOperations(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var factory = new StubHttpClientFactory(new LambdaHandler(responder));
        return new DidoxEdoOperations(new StubUserContext(), factory, new DidoxTimestampClient(factory));
    }

    private static EdocsEdoOperations CreateEdocsOperations(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new StubUserContext(), new StubHttpClientFactory(new LambdaHandler(responder)));

    private static HttpResponseMessage Json(HttpStatusCode status, string value) => new(status)
    {
        Content = new StringContent(value, Encoding.UTF8, "application/json")
    };

    private static EdoHistoricalDetailRequestDto HistoricalDetailRequest(
        EdoProviderCode provider,
        string documentType) => new()
    {
        DateFrom = new DateOnly(2026, 1, 1),
        DateTo = new DateOnly(2026, 12, 31),
        Item = new EdoHistoricalDocumentSummaryDto
        {
            ProviderDocumentId = "DOC-1",
            Direction = EdoDirection.INBOX,
            Status = EdoDocumentStatusCode.SIGNED,
            DocumentType = documentType
        }
    };

    private static EdoHistoricalDetailRequestDto WithDates(
        EdoHistoricalDetailRequestDto source,
        DateOnly from,
        DateOnly to) => new()
    {
        Item = source.Item,
        DateFrom = from,
        DateTo = to
    };

    private static EdoHistoricalDetailRequestDto WithItem(
        EdoHistoricalDetailRequestDto source,
        string providerDocumentId) => new()
    {
        DateFrom = source.DateFrom,
        DateTo = source.DateTo,
        Item = new EdoHistoricalDocumentSummaryDto
        {
            ProviderDocumentId = providerDocumentId,
            Direction = source.Item.Direction,
            Status = source.Item.Status,
            DocumentType = source.Item.DocumentType
        }
    };

    private static EdoDocumentDto WithDate(EdoDocumentDto source, DateOnly date) => Copy(source, date: date);

    private static EdoDocumentDto ValidDocument(
        EdoProviderCode provider,
        string status,
        string documentType) => new()
    {
        ProviderCode = provider,
        ProviderDocumentId = "DOC-1",
        Direction = EdoDirection.INBOX,
        DocumentType = documentType,
        DocumentNumber = "42",
        DocumentDate = new DateOnly(2026, 7, 31),
        Status = new EdoDocumentStatusDto { ProviderStatusCode = status },
        Seller = new EdoPartyDto { Name = "Supplier", TaxIdentifier = "300000001" },
        Buyer = new EdoPartyDto { Name = "Buyer", TaxIdentifier = "309142275" },
        TotalAmount = 112m,
        PreviewLines =
        [
            new EdoDocumentPreviewLineDto
            {
                Number = 1,
                CatalogCode = "MXIK",
                Quantity = 1,
                UnitPrice = 100,
                NetAmount = 100,
                VatRate = 12,
                VatAmount = 12,
                TotalWithVat = 112
            }
        ]
    };

    private static EdoDocumentDto Copy(
        EdoDocumentDto source,
        EdoDirection? direction = null,
        EdoPartyDto? seller = default,
        EdoPartyDto? buyer = default,
        string? previewSellerTin = "__preserve__",
        DateOnly? date = null) => new()
    {
        ProviderCode = source.ProviderCode,
        ProviderDocumentId = source.ProviderDocumentId,
        Direction = direction ?? source.Direction,
        DocumentType = source.DocumentType,
        DocumentNumber = source.DocumentNumber,
        DocumentDate = date ?? source.DocumentDate,
        Status = source.Status,
        Seller = seller ?? (previewSellerTin == "__preserve__" ? source.Seller : null),
        Buyer = buyer ?? source.Buyer,
        TotalAmount = source.TotalAmount,
        PreviewSellerTin = previewSellerTin == "__preserve__" ? source.PreviewSellerTin : previewSellerTin,
        PreviewLines = source.PreviewLines,
        MarkingCodes = source.MarkingCodes
    };

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        private readonly HttpClient _client = new(handler) { BaseAddress = new Uri("https://provider.test/") };
        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class LambdaHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(responder(request));
        }
    }

    private sealed class StubUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId => 999;
        public List<int> AllowedOrganizationIds => [999];
        public int? BranchId => null;
    }

    private sealed class StubHistoricalSource(EdoProviderCode providerCode) : IEdoHistoricalDocumentSource
    {
        public EdoProviderCode ProviderCode { get; } = providerCode;
        public ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(EdoHistoricalExecutionContextDto context, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(EdoHistoricalExecutionContextDto context, EdoHistoricalPageRequestDto request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<EdoHistoricalDetailResultDto> ReadDetailAsync(EdoHistoricalExecutionContextDto context, EdoHistoricalDetailRequestDto request, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class StubOrganizationSourceReader(IReadOnlyDictionary<int, string> inns) : IOrganizationSourceReader
    {
        public Task<int?> GetProductOrganizationIdAsync(int productId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<int?> GetCounterpartyOrganizationIdAsync(int counterpartyId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<int?> GetProductTableOrganizationIdAsync(int productTableId, CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<string?> GetOrganizationInnAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult(inns.TryGetValue(organizationId, out var inn) ? inn : null);
    }

    private sealed class StubHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "UnitTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
