namespace Application.Features.PurchaseServices;

public class PurchaseServiceListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = null!;
    public int AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool VatApplicable { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
