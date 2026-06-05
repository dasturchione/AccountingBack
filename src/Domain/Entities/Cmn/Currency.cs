namespace Domain.Entities;

public partial class Currency
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Symbol { get; set; }

    public short StateId { get; set; }

    public virtual State State { get; set; } = null!;
}
