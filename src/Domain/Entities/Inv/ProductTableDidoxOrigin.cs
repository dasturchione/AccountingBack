using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_table_didox_origin")]
[Index("OrganizationId", Name = "idx_inv_product_table_didox_origin_organization_id")]
[Index("OriginCode", Name = "idx_inv_product_table_didox_origin_origin_code")]
[Index("StateId", Name = "idx_inv_product_table_didox_origin_state_id")]
[Index("ProductTableId", Name = "ux_inv_product_table_didox_origin_product_table", IsUnique = true)]
public partial class ProductTableDidoxOrigin
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("origin_code")]
    public short OriginCode { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.ProductTableDidoxOrigins))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(OriginCode))]
    public virtual DidoxOrigin Origin { get; set; } = null!;

    [ForeignKey(nameof(ProductTableId))]
    [InverseProperty(nameof(ProductTable.DidoxOrigin))]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;
}
