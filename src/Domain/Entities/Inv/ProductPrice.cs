namespace Domain.Entities;

public partial class ProductPrice
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public int ProductId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
