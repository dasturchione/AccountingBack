using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product")]
[Index("Barcode", Name = "idx_inv_product_barcode")]
[Index("Name", Name = "idx_inv_product_name")]
[Index("OrganizationId", Name = "idx_inv_product_organization_id")]
[Index("ProductGroupId", Name = "idx_inv_product_product_group_id")]
[Index("StateId", Name = "idx_inv_product_state_id")]
[Index("UnitId", Name = "idx_inv_product_unit_id")]
public partial class InvProduct
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("product_group_id")]
    public int? ProductGroupId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("barcode")]
    [StringLength(100)]
    public string? Barcode { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Column("is_service")]
    public bool IsService { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Product")]
    public virtual ICollection<InvProductTable> InvProductTables { get; set; } = new List<InvProductTable>();

    [InverseProperty("Product")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProducts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductGroupId")]
    [InverseProperty("InvProducts")]
    public virtual InvProductGroup? ProductGroup { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("InvProducts")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
