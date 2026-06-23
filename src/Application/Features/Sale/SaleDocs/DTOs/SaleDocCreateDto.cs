namespace Application.Features.SaleDocs;

public class SaleDocCreateDto
{
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public string? Comment { get; set; }
    public List<SaleDocCreateProductDto> Products { get; set; } = new();
}

public class SaleDocCreateProductDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
}
