namespace Application.Features.RetailSaleDocs;

public static class RetailSaleVatCalculator
{
    public static decimal ResolveTotal(
        decimal unitPrice,
        decimal quantity,
        decimal? vatAmount,
        decimal? vatRate) =>
        vatAmount ?? (vatRate.HasValue
            ? Math.Round(unitPrice * quantity * vatRate.Value / 100m, 8)
            : 0m);

    public static decimal ResolvePerUnit(decimal totalVatAmount, decimal quantity) =>
        quantity > 0m
            ? Math.Round(totalVatAmount / quantity, 8)
            : 0m;
}
