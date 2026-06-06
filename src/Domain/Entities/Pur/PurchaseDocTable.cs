namespace Domain.Entities;

public partial class PurchaseDocTable
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
