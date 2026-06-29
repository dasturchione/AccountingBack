namespace Application.Features.SaleDocs;

public class SaleDocConfirmDto
{
    public List<SaleDocConfirmLineDto> Lines { get; set; } = new();
}

public class SaleDocConfirmLineDto
{
    public long Id { get; set; }
    public decimal CostPrice { get; set; }
    public decimal UnitPrice { get; set; }
}
