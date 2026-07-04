namespace Application.Features.Cmn.Taxes;

public class TaxDto : TaxBaseDto
{
    public short Id { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
