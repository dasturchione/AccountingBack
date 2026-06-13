using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_warehouse")]
[Index("BranchId", Name = "idx_inv_warehouse_branch_id")]
[Index("OrganizationId", Name = "idx_inv_warehouse_organization_id")]
[Index("ResponsibleUserId", Name = "idx_inv_warehouse_responsible_user_id")]
[Index("StateId", Name = "idx_inv_warehouse_state_id")]
public partial class Warehouse
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("Warehouses")]
    public virtual Branch? Branch { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("Warehouses")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Warehouse")]
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("Warehouses")]
    public virtual User? ResponsibleUser { get; set; }

    [InverseProperty("Warehouse")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("Warehouses")]
    public virtual State State { get; set; } = null!;
}
