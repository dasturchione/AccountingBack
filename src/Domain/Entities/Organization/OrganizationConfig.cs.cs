using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_organization_config")]
public partial class OrganizationConfig
{
    [Key]
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("inventory_valuation_method")]
    [StringLength(20)]
    public string InventoryValuationMethod { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrganizationConfig")]
    public virtual Organization Organization { get; set; } = null!;
}
