namespace Application.Features.RetailSaleDocs;

public static class RetailSaleVatCalculator
{
    public static decimal ResolvePerUnit(
        decimal unitPrice,
        decimal? vatAmount,
        decimal? vatRate) =>
        vatAmount ?? (vatRate.HasValue
            ? Math.Round(unitPrice * vatRate.Value / 100m, 8)
            : 0m);
}
