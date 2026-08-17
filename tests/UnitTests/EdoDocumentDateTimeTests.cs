using Application.Abstractions.Integration.Edo;
using Application.Features.PurchaseDocs;
using Integration.Edocs.Facturas;

namespace UnitTests;

public sealed class EdoDocumentDateTimeTests
{
    [Fact]
    public void ParseDocumentDateTime_PreservesProviderClockTime()
    {
        var result = EdocsEdoOperations.ParseDocumentDateTime("2026-08-13T14:27:31+05:00");

        Assert.Equal(new DateTime(2026, 8, 13, 14, 27, 31), result);
        Assert.Equal(DateTimeKind.Unspecified, result!.Value.Kind);
    }

    [Fact]
    public void ResolveEdoDocumentDateTime_PrefersTimestampOverDateOnly()
    {
        var document = new EdoDocumentDto
        {
            DocumentDate = new DateOnly(2026, 8, 13),
            DocumentDateTime = new DateTime(2026, 8, 13, 16, 45, 12)
        };

        var result = PurchaseDocService.ResolveEdoDocumentDateTime(document);

        Assert.Equal(new DateTime(2026, 8, 13, 16, 45, 12), result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    [Fact]
    public void ResolveEdoDocumentDateTime_FallsBackToMidnightForDateOnlyProviderValue()
    {
        var document = new EdoDocumentDto
        {
            DocumentDate = new DateOnly(2026, 8, 13)
        };

        var result = PurchaseDocService.ResolveEdoDocumentDateTime(document);

        Assert.Equal(new DateTime(2026, 8, 13, 0, 0, 0), result);
    }
}
