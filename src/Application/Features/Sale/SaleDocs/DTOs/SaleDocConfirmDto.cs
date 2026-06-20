namespace Application.Features.SaleDocs;

public class SaleDocConfirmDto
{
    public int CounterpartyId { get; set; }
    public DateTime DocDate { get; set; }
    public List<SaleDocConfirmLineDto> Lines { get; set; } = new();
}

public class SaleDocConfirmLineDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
}
