namespace Application.Features.SaleDocs.EdoSalePreflight;

public static class EdoSaleAmountValidation
{
    private const decimal MoneyRoundingHalfUnit = 0.005m;
    public const decimal Tolerance = 0.00000001m;
    public const string LineTotalsMismatchCode = "SALE_SOURCE_LINE_TOTALS_MISMATCH";
    public const string AggregateTotalsMismatchCode = "SALE_SOURCE_AGGREGATE_TOTALS_MISMATCH";

    public static bool AreEqual(decimal left, decimal right) =>
        Math.Abs(left - right) <= Tolerance;

    public static bool IsLineInternallyConsistent(
        decimal quantity,
        decimal unitPrice,
        decimal netAmount,
        decimal vatAmount,
        decimal totalWithVat) =>
        IsLineNetAmountConsistent(quantity, unitPrice, netAmount)
        && AreEqual(netAmount + vatAmount, totalWithVat);

    private static bool IsLineNetAmountConsistent(
        decimal quantity,
        decimal unitPrice,
        decimal netAmount)
    {
        var providerRoundingTolerance = Math.Abs(quantity) * MoneyRoundingHalfUnit;
        return Math.Abs(quantity * unitPrice - netAmount)
            <= providerRoundingTolerance + Tolerance;
    }

    public static bool AreDocumentTotalsConsistent(
        decimal? documentNetAmount,
        decimal? documentVatAmount,
        decimal documentTotalAmount,
        IEnumerable<(decimal NetAmount, decimal VatAmount, decimal TotalWithVat)> lines)
    {
        if (documentNetAmount is null || documentVatAmount is null)
            return false;

        var lineArray = lines.ToArray();
        return AreEqual(lineArray.Sum(x => x.NetAmount), documentNetAmount.Value)
            && AreEqual(lineArray.Sum(x => x.VatAmount), documentVatAmount.Value)
            && AreEqual(lineArray.Sum(x => x.TotalWithVat), documentTotalAmount);
    }

    public static bool AreDocumentTotalsConsistent(
        decimal? documentNetAmount,
        decimal? documentVatAmount,
        decimal documentTotalAmount,
        IEnumerable<(decimal Quantity, decimal UnitPrice, decimal NetAmount, decimal VatAmount, decimal TotalWithVat)> lines)
    {
        if (documentNetAmount is null || documentVatAmount is null)
            return false;

        var lineArray = lines.ToArray();
        if (lineArray.Any(line => !IsLineInternallyConsistent(
                line.Quantity,
                line.UnitPrice,
                line.NetAmount,
                line.VatAmount,
                line.TotalWithVat)))
        {
            return false;
        }

        var accumulatedLineNetRoundingResidual = lineArray.Sum(line =>
            line.Quantity * line.UnitPrice - line.NetAmount);
        var lineNetSum = lineArray.Sum(x => x.NetAmount);
        var documentNetMatchesLineSum = AreEqual(lineNetSum, documentNetAmount.Value);
        var documentNetMatchesResidualAdjustedLineSum = AreEqual(
            lineNetSum - accumulatedLineNetRoundingResidual,
            documentNetAmount.Value);

        return (documentNetMatchesLineSum || documentNetMatchesResidualAdjustedLineSum)
            && AreEqual(lineArray.Sum(x => x.VatAmount), documentVatAmount.Value)
            && AreEqual(lineArray.Sum(x => x.TotalWithVat), documentTotalAmount);
    }

    public static string? GetDocumentTotalsFailureCode(
        decimal? documentNetAmount,
        decimal? documentVatAmount,
        decimal documentTotalAmount,
        IEnumerable<(int Number, decimal Quantity, decimal UnitPrice, decimal NetAmount, decimal VatAmount, decimal TotalWithVat)> lines)
    {
        var lineArray = lines.ToArray();
        if (lineArray.Any(line => !IsLineInternallyConsistent(
                line.Quantity,
                line.UnitPrice,
                line.NetAmount,
                line.VatAmount,
                line.TotalWithVat)))
        {
            return LineTotalsMismatchCode;
        }

        return AreDocumentTotalsConsistent(
                documentNetAmount,
                documentVatAmount,
                documentTotalAmount,
                lineArray.Select(line => (
                    line.Quantity,
                    line.UnitPrice,
                    line.NetAmount,
                    line.VatAmount,
                    line.TotalWithVat)))
            ? null
            : AggregateTotalsMismatchCode;
    }
}
