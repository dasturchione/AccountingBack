namespace Application.Features.SaleDocs;

public class SaleDocBaseDto
{
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public long? ContractId { get; set; }
    public string? Comment { get; set; }
}
