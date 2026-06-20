using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_table")]
public partial class ProductTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("marking_number")]
    [StringLength(250)]
    public string? MarkingNumber { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("ProductTables")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductTables")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("ProductTable")]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [InverseProperty("ProductTable")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("ProductTables")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("ProductTables")]
    public virtual ProductTableStatus Status { get; set; } = null!;
}
