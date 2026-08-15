using System.Globalization;

namespace Application.Features.DocumentNumbers;

internal static class HistoricalDocumentNumberPolicy
{
    private const string Prefix = "H";

    public static string Format(short documentYear, long sequenceNumber)
    {
        if (documentYear <= 0)
            throw new ArgumentOutOfRangeException(nameof(documentYear));
        if (sequenceNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber));

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Prefix}-{documentYear:0000}-{sequenceNumber}");
    }
}
