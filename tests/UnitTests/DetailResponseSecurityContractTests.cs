using Application.Abstractions.Integration.Edo;
using Application.Features.Contracts;
using Application.Features.PurchaseDocs;
using Application.Features.PurchaseDocTables;
using Application.Features.SaleDocs;
using Domain.Entities;
using System.Text.Json;

public sealed class DetailResponseSecurityContractTests
{
    [Fact]
    public void ContractDtoExposesProviderIdentityFields()
    {
        var dto = new ContractDto
        {
            ProviderCode = "EDOCS",
            ProviderContractNumber = "3",
            ProviderContractDate = new DateOnly(2026, 3, 3)
        };

        var json = JsonSerializer.Serialize(dto);

        Assert.Contains("ProviderCode", json, StringComparison.Ordinal);
        Assert.Contains("ProviderContractNumber", json, StringComparison.Ordinal);
        Assert.Contains("ProviderContractDate", json, StringComparison.Ordinal);
    }

    [Fact]
    public void PurchaseAndSaleDetailsRedactRawMarkings()
    {
        var purchaseJson = JsonSerializer.Serialize(new PurchaseDocProductItemDto
        {
            MarkingNumber = "must-not-leak",
            SerialNumber = "must-not-leak",
            HasMarking = true,
            MarkingCount = 1
        });
        var saleJson = JsonSerializer.Serialize(new SaleDocProductTableDto
        {
            MarkingNumber = "must-not-leak",
            SerialNumber = "must-not-leak",
            HasMarking = true,
            MarkingCount = 1
        });
        var purchaseTableJson = JsonSerializer.Serialize(new PurchaseDocTableDto
        {
            MarkingNumber = "must-not-leak",
            SerialNumber = "must-not-leak",
            HasMarking = true,
            MarkingCount = 1
        });
        var edoJson = JsonSerializer.Serialize(new EdoDocumentDto
        {
            MarkingCodes = ["must-not-leak"]
        });

        Assert.DoesNotContain("must-not-leak", purchaseJson, StringComparison.Ordinal);
        Assert.DoesNotContain("must-not-leak", saleJson, StringComparison.Ordinal);
        Assert.DoesNotContain("must-not-leak", purchaseTableJson, StringComparison.Ordinal);
        Assert.DoesNotContain("must-not-leak", edoJson, StringComparison.Ordinal);
        Assert.Contains("hasMarking", purchaseJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("markingCount", saleJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hasMarking", edoJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SourceMetadataContainsOnlySafeLocalLinkFields()
    {
        var source = new EdoDocument
        {
            Id = 12,
            OrganizationId = 2,
            Provider = "EDOCS",
            Direction = "OUTBOX",
            InternalDocumentType = "sale_doc",
            InternalDocumentId = 894,
            ProviderDocumentId = "provider-id",
            DocumentNumber = "3",
            DocumentDate = new DateOnly(2026, 3, 24),
            DocumentType = "FACTURA",
            Status = "SIGNED"
        };

        var dto = Application.Features.Integration.Edo.EdoSourceMetadataMapper.Map(source);
        var json = JsonSerializer.Serialize(dto);

        Assert.Equal(12, dto.EdoDocumentId);
        Assert.Equal("EDOCS", dto.ProviderCode);
        Assert.Equal("provider-id", dto.ProviderDocumentId);
        Assert.DoesNotContain("OrganizationId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw", json, StringComparison.OrdinalIgnoreCase);
    }
}
