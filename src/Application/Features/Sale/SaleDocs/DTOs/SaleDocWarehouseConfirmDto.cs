namespace Application.Features.SaleDocs;

public class SaleDocWarehouseConfirmDto
{
    public List<SaleDocWarehouseConfirmItemDto> Items { get; set; } = new();
}

public class SaleDocWarehouseConfirmItemDto
{
    public int ProductTableId { get; set; }
}
