namespace Application.Features.SaleDocs;

public class SaleDocWarehouseConfirmDto
{
    public List<SaleDocWarehouseConfirmItemDto> Items { get; set; } = new();
}

public class SaleDocWarehouseConfirmItemDto
{
    public long SaleDocProductId { get; set; }
    public List<int> ProductTableIds { get; set; } = new();
}
