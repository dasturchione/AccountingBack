namespace Application.Features.SaleDocs;

public class SaleDocProductAssemblyDto
{
    public long Id { get; set; }
    public bool Assembled { get; set; } = false;
    public List<SaleDocAssemblyItemDto> Items { get; set; } = new();
}

public class SaleDocAssemblyItemDto
{
    public int ProductTableId { get; set; }
}
