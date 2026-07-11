using System.Text.Json;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Application.Features.Cmn.Taxes.Integration.Mappers;
using Domain.Entities;

namespace UnitTests;

// Proves the official Didox ЭСФ payload shape (Version, ProductList object, string money fields,
// MeasureId=null, document_json envelope) and the SaleDoc -> DidoxInvoiceRequest mapping.
// Real submit/sign still need partner + company tokens, so only serialization and mapping are asserted.
public sealed class DidoxInvoicePayloadTests
{
    private static readonly JsonSerializerOptions ProviderJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Serialize_MatchesOfficialStructure()
    {
        var invoice = new DidoxInvoiceRequest
        {
            FacturaType = (int)DidoxFacturaType.Standard,
            SellerTin = "301234567",
            Seller = new DidoxParty { Name = "Seller LLC", VatRegStatus = 20 },
            BuyerTin = "409876543",
            Buyer = new DidoxParty { Name = "Buyer LLC" },
            FacturaDoc = new DidoxFacturaDoc { FacturaNo = "INV-1", FacturaDate = "2026-07-10" },
            ContractDoc = new DidoxContractDoc { ContractNo = "C-1", ContractDate = "2026-01-01" },
            ProductList = new DidoxProductList
            {
                Tin = "301234567",
                HasVat = true,
                Products =
                [
                    new DidoxInvoiceProduct
                    {
                        OrdNo = 1,
                        Name = "Widget",
                        CatalogCode = "12345678901234567",
                        Count = "2",
                        Summa = "1000",
                        DeliverySum = "2000.00",
                        VatRate = "12",
                        VatSum = "240.00",
                        DeliverySumWithVat = "2240.00",
                        Origin = 1
                    }
                ]
            }
        };

        var envelope = new DidoxDocumentEnvelope<DidoxInvoiceRequest> { DocumentJson = invoice };
        var json = JsonSerializer.Serialize(envelope, ProviderJsonOptions);

        using var document = JsonDocument.Parse(json);
        var body = document.RootElement.GetProperty("document_json");

        // Version + documented top-level casing.
        Assert.Equal(1, body.GetProperty("Version").GetInt32());
        Assert.Equal(0, body.GetProperty("FacturaType").GetInt32());
        Assert.Equal("301234567", body.GetProperty("SellerTin").GetString());
        Assert.Equal(20, body.GetProperty("Seller").GetProperty("VatRegStatus").GetInt32());
        Assert.Equal("409876543", body.GetProperty("BuyerTin").GetString());

        // ProductList is an OBJECT with the products array nested inside.
        var productList = body.GetProperty("ProductList");
        Assert.True(productList.GetProperty("HasVat").GetBoolean());
        Assert.Equal("301234567", productList.GetProperty("Tin").GetString());

        var line = productList.GetProperty("Products")[0];
        Assert.Equal("12345678901234567", line.GetProperty("CatalogCode").GetString());

        // Money/quantity fields are STRINGS.
        Assert.Equal(JsonValueKind.String, line.GetProperty("Count").ValueKind);
        Assert.Equal(JsonValueKind.String, line.GetProperty("DeliverySumWithVat").ValueKind);
        Assert.Equal("2240.00", line.GetProperty("DeliverySumWithVat").GetString());

        // MeasureId is always null.
        Assert.Equal(JsonValueKind.Null, line.GetProperty("MeasureId").ValueKind);
    }

    [Fact]
    public void Serialize_SignBody_UsesLowercaseSignatureKey()
    {
        var body = new DidoxSignRequest { Signature = "cGtjczdiNjQ=" };

        var json = JsonSerializer.Serialize(body, ProviderJsonOptions);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("cGtjczdiNjQ=", document.RootElement.GetProperty("signature").GetString());
    }

    [Fact]
    public void Map_SaleDoc_ReusesDomainAmountsAsStringsAndMxik()
    {
        var doc = BuildSaleDoc();

        var invoice = SaleDocToDidoxInvoiceMapper.Map(doc, DidoxFacturaType.Standard);

        Assert.Equal(1, invoice.Version);
        Assert.Equal((int)DidoxFacturaType.Standard, invoice.FacturaType);
        Assert.Equal("SF-2026-1", invoice.FacturaDoc.FacturaNo);
        Assert.Equal("2026-07-10", invoice.FacturaDoc.FacturaDate);
        Assert.Equal("C-77", invoice.ContractDoc.ContractNo);

        Assert.Equal("301234567", invoice.SellerTin);
        Assert.Equal("301234567", invoice.ProductList.Tin);
        Assert.Equal("Seller LLC", invoice.Seller.Name);
        Assert.Equal("409876543", invoice.BuyerTin);
        Assert.True(invoice.ProductList.HasVat);

        var line = Assert.Single(invoice.ProductList.Products);
        Assert.Equal(1, line.OrdNo);
        Assert.Equal("Widget", line.Name);
        Assert.Equal("12345678901234567", line.CatalogCode); // Product.Mxik
        Assert.Equal("3", line.Count);                        // Quantity, string
        Assert.Equal("12", line.VatRate);                     // VatRate.Rate, string
        Assert.Equal("360.00", line.VatSum);                  // domain VatAmount, "0.00"
        Assert.Equal("3360.00", line.DeliverySumWithVat);     // domain TotalAmount, "0.00"
        Assert.False(line.WithoutVat);
    }

    [Fact]
    public void Map_ZeroVatLine_SetsWithoutVatAndHasVatFalse()
    {
        var doc = BuildSaleDoc();
        doc.SaleDocProducts.First().VatRate = new VatRate { Rate = 0m };
        doc.SaleDocProducts.First().VatAmount = 0m;

        var invoice = SaleDocToDidoxInvoiceMapper.Map(doc);

        Assert.False(invoice.ProductList.HasVat);
        Assert.True(invoice.ProductList.Products[0].WithoutVat);
    }

    private static SaleDoc BuildSaleDoc()
    {
        return new SaleDoc
        {
            DocNumber = "SF-2026-1",
            DocDate = new DateTime(2026, 7, 10),
            Organization = new Organization { Inn = "301234567", FullName = "Seller LLC", Address = "Tashkent" },
            Counterparty = new CounterpartyCard { Inn = "409876543", FullName = "Buyer LLC" },
            Contract = new Contract { ContractNumber = "C-77", ContractDate = new DateTime(2026, 1, 1) },
            SaleDocProducts =
            [
                new SaleDocProduct
                {
                    Id = 10,
                    Quantity = 3m,
                    UnitPrice = 1000m,
                    Amount = 3000m,
                    VatAmount = 360m,
                    TotalAmount = 3360m,
                    Product = new Product { Name = "Widget", Mxik = "12345678901234567" },
                    Unit = new Unit { Code = "796", Name = "шт." },
                    VatRate = new VatRate { Rate = 12m }
                }
            ]
        };
    }
}
