namespace Domain.Entities;

public partial class PurchaseDoc
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public string DocNumber { get; set; } = null!;

    public DateTime DocDate { get; set; }

    public int CounterpartyId { get; set; }

    public int WarehouseId { get; set; }

    public short CurrencyId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public short StatusId { get; set; }

    public string? Comment { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }
}
