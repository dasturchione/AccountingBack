using Application.Abstractions.Integration.Edo;
using Application.Features.Contracts;
using Application.Features.PurchaseDocs;
using Application.Features.PurchaseDocTables;
using Application.Features.SaleDocs;
using System.Text.Json;

public sealed class DetailResponseSecurityContractTests
{
    [Fact]
    public void PublicDetailContractsExposeSafeFieldsOnly()
    {
        var contract = JsonSerializer.Serialize(new ContractDto
        {
            ProviderCode = "EDOCS",
            ProviderContractNumber = "3",
            ProviderContractDate = new DateOnly(2026, 3, 3)
        });
        var purchase = JsonSerializer.Serialize(new PurchaseDocProductItemDto
        {
            MarkingNumber = "raw-value",
            HasMarking = true,
            MarkingCount = 1
        });
        var sale = JsonSerializer.Serialize(new SaleDocProductTableDto
        {
            MarkingNumber = "raw-value",
            HasMarking = true,
            MarkingCount = 1
        });
        var purchaseTable = JsonSerializer.Serialize(new PurchaseDocTableDto
        {
            MarkingNumber = "raw-value",
            HasMarking = true,
            MarkingCount = 1
        });
        var edo = JsonSerializer.Serialize(new EdoDocumentDto
        {
            MarkingCodes = ["raw-value"]
        });

        Assert.Contains("ProviderContractNumber", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-value", purchase, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-value", sale, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-value", purchaseTable, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-value", edo, StringComparison.Ordinal);
    }
}
