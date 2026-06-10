namespace Domain.Entities;

public partial class Unit
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
