namespace Application.Features.CounterpartyCards;

public class CounterpartyCardCreateManyDto
{
    public List<CounterpartyCardCreateDto> Counterparties { get; set; } = new();
}
