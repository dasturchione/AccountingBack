using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product")]
[Index("Article", Name = "idx_inv_product_article")]
[Index("Barcode", Name = "idx_inv_product_barcode")]
[Index("Code", Name = "idx_inv_product_code")]
[Index("DefaultVatRateId", Name = "idx_inv_product_default_vat_rate_id")]
[Index("Name", Name = "idx_inv_product_name")]
[Index("OrganizationId", Name = "idx_inv_product_organization_id")]
[Index("ProductGroupId", Name = "idx_inv_product_product_group_id")]
[Index("ProductTypeId", Name = "idx_inv_product_product_type_id")]
[Index("Sku", Name = "idx_inv_product_sku")]
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

    [Column("mxik")]
    [StringLength(17)]
    public string? Mxik { get; set; }

    [Column("gtin")]
    [StringLength(14)]
    public string? Gtin { get; set; }

    [Column("is_piece_tracked")]
    public bool IsPieceTracked { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("sku")]
    [StringLength(100)]
    public string? Sku { get; set; }

    [Column("article")]
    [StringLength(100)]
    public string? Article { get; set; }

    [Column("default_vat_rate_id")]
    public short? DefaultVatRateId { get; set; }

    [Column("min_stock")]
    [Precision(18, 3)]
    public decimal? MinStock { get; set; }

    [Column("product_type_id")]
    public short ProductTypeId { get; set; }

    [Column("is_sold")]
    public bool IsSold { get; set; }

    [Column("is_purchased")]
    public bool IsPurchased { get; set; }

    [InverseProperty("SourceProduct")]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLines { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty("Product")]
    public virtual ICollection<InvInventoryAdjustmentLine> InvInventoryAdjustmentLines { get; set; } = new List<InvInventoryAdjustmentLine>();

    [InverseProperty("Product")]
    public virtual ICollection<InvInventoryCountLine> InvInventoryCountLines { get; set; } = new List<InvInventoryCountLine>();

    [InverseProperty("Product")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Product")]
    public virtual ICollection<InvProductTable> InvProductTables { get; set; } = new List<InvProductTable>();

    [InverseProperty("Product")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("Product")]
    public virtual ICollection<InvTransferLine> InvTransferLines { get; set; } = new List<InvTransferLine>();

    [InverseProperty("Product")]
    public virtual ICollection<InvWarehouseProduct> InvWarehouseProducts { get; set; } = new List<InvWarehouseProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProducts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductGroupId")]
    [InverseProperty("InvProducts")]
    public virtual InvProductGroup? ProductGroup { get; set; }

    [ForeignKey("ProductTypeId")]
    [InverseProperty("InvProducts")]
    public virtual CmnProductType ProductType { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<PurDocProduct> PurDocProducts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("Product")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("InvProducts")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
