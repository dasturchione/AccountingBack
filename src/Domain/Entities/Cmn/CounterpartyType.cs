namespace Domain.Entities;

public partial class CounterpartyType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();
}