using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_table_didox_origin")]
[Index("OrganizationId", Name = "idx_inv_product_table_didox_origin_organization_id")]
[Index("OriginCode", Name = "idx_inv_product_table_didox_origin_origin_code")]
[Index("StateId", Name = "idx_inv_product_table_didox_origin_state_id")]
[Index("ProductTableId", Name = "ux_inv_product_table_didox_origin_product_table", IsUnique = true)]
public partial class InvProductTableDidoxOrigin
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

    [ForeignKey("OrganizationId")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("OriginCode")]
    public virtual CmnDidoxOrigin Origin { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("StateId")]
    public virtual CmnState State { get; set; } = null!;
}
