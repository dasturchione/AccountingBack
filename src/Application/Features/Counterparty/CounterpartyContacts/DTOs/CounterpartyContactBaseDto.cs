namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactBaseDto
{
    public int CounterpartyId { get; set; }
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Comment { get; set; }
}
