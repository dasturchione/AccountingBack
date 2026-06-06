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
}
