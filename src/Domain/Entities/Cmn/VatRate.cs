using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_vat_rate")]
[Index("Code", Name = "idx_cmn_vat_rate_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_vat_rate_state_id")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_cmn_vat_rate_effective_dates")]
public partial class VatRate
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("rate")]
    [Precision(5, 2)]
    public decimal Rate { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("effective_from")]
    public DateOnly? EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [InverseProperty(nameof(PurchaseDocProduct.VatRate))]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty(nameof(RetailSaleDocProduct.VatRate))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProducts { get; set; } = new List<RetailSaleDocProduct>();

    [InverseProperty(nameof(RetailSaleDocTable.VatRate))]
    public virtual ICollection<RetailSaleDocTable> RetailSaleDocTables { get; set; } = new List<RetailSaleDocTable>();

    [InverseProperty(nameof(PurchaseDocTable.VatRate))]
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();

    [InverseProperty(nameof(SaleCondition.VatRate))]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty(nameof(SaleDocTable.VatRate))]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [InverseProperty(nameof(SaleDocProduct.VatRate))]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.VatRates))]
    public virtual State State { get; set; } = null!;
}
