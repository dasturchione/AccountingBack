namespace Application.Features.SaleDocs;

public class SaleDocConfirmDto
{
    public DateTime? DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public string? Comment { get; set; }
}
