namespace Application.Features.SaleDocs.EdoSalePreflight;

public static class EdoSaleAmountValidation
{
    public const decimal Tolerance = 0.00000001m;

    public static bool AreEqual(decimal left, decimal right) =>
        Math.Abs(left - right) <= Tolerance;

    public static bool IsLineInternallyConsistent(
        decimal quantity,
        decimal unitPrice,
        decimal netAmount,
        decimal vatAmount,
        decimal totalWithVat) =>
        AreEqual(quantity * unitPrice, netAmount)
        && AreEqual(netAmount + vatAmount, totalWithVat);

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
}
