namespace Domain.Entities;

public partial class RegisterBalance
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public short OperationTypeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;
    public virtual OperationType OperationType { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}
