namespace Application.Features.Contracts;

public class ContractDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public string ContractType { get; set; } = null!;
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
