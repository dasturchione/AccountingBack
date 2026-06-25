namespace Application.Features.CounterpartyCards;

public class CounterpartyCardCreateResultDto
{
    public int Id { get; set; }
    public string? Inn { get; set; }
    public string ShortName { get; set; } = null!;
}
