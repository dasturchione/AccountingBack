namespace Application.Features.SaleDocs;

public class SaleDocUpdateDto : SaleDocBaseDto
{
    public short StateId { get; set; }
    public List<SaleDocUpdateProductDto> Lines { get; set; } = new();
}

public class SaleDocUpdateProductDto
{
    public long? Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? CostAccountId { get; set; }
}
