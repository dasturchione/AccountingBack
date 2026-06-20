namespace Application.Features.SaleDocs;

public class SaleDocCreateDto
{
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public string? Comment { get; set; }
    public List<SaleDocCreateLineDto> Lines { get; set; } = new();
}

public class SaleDocCreateLineDto
{
    public int Id { get; set; }
}
