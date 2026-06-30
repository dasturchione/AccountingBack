using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product")]
[Index("Barcode", Name = "idx_inv_product_barcode")]
[Index("Name", Name = "idx_inv_product_name")]
[Index("OrganizationId", Name = "idx_inv_product_organization_id")]
[Index("ProductGroupId", Name = "idx_inv_product_product_group_id")]
[Index("StateId", Name = "idx_inv_product_state_id")]
[Index("UnitId", Name = "idx_inv_product_unit_id")]
[Index("Article", Name = "idx_inv_product_article")]
[Index("Code", Name = "idx_inv_product_code")]
[Index("CogsAccountId", Name = "idx_inv_product_cogs_account_id")]
[Index("DefaultVatRateId", Name = "idx_inv_product_default_vat_rate_id")]
[Index("ExpenseAccountId", Name = "idx_inv_product_expense_account_id")]
[Index("IncomeAccountId", Name = "idx_inv_product_income_account_id")]
[Index("InventoryAccountId", Name = "idx_inv_product_inventory_account_id")]
[Index("Sku", Name = "idx_inv_product_sku")]
public partial class Product
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

    [Column("is_piece_tracked")]
    public bool IsPieceTracked { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("mxik")]
    [StringLength(17)]
    public string? Mxik { get; set; }


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

    [Column("inventory_account_id")]
    public int? InventoryAccountId { get; set; }

    [Column("income_account_id")]
    public int? IncomeAccountId { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("cogs_account_id")]
    public int? CogsAccountId { get; set; }

    [Column("min_stock")]
    [Precision(18, 3)]
    public decimal? MinStock { get; set; }
    [InverseProperty("Product")]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductTable> ProductTables { get; set; } = new List<ProductTable>();

    [InverseProperty("Product")]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty("Product")]
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("Products")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProductGroupId")]
    [InverseProperty("Products")]
    public virtual ProductGroup? ProductGroup { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("Products")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("Products")]
    public virtual Unit Unit { get; set; } = null!;
}
