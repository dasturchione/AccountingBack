using Application.Features.SaleDocs;
using System.Text.Json;

public sealed class SaleAvailableProductRedactionContractTests
{
    [Fact]
    public void AvailableProductResponseHasOnlySafeMarkingMetadata()
    {
        var dto = new SaleDocAvailableProductTableDto
        {
            ProductTableId = 7,
            HasMarking = true,
            MarkingCount = 1,
            AvailabilityStatus = "AVAILABLE"
        };

        var json = JsonSerializer.Serialize(dto);

        Assert.DoesNotContain("markingNumber", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-marking", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hasMarking", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("markingCount", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("availabilityStatus", json, StringComparison.OrdinalIgnoreCase);
    }
}
