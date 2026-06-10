namespace Domain.Entities;

public partial class CounterpartyContact
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int CounterpartyId { get; set; }
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual CounterpartyCard Counterparty { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
