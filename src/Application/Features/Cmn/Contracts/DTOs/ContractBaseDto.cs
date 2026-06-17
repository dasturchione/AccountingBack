namespace Application.Features.Contracts;

public class ContractBaseDto
{
    public int OrganizationId { get; set; }
    public int CounterpartyId { get; set; }
    public short ContractTypeId { get; set; }
    public DateTime ContractDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
