using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_vat_rate")]
[Index("Code", Name = "idx_cmn_vat_rate_code", IsUnique = true)]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_cmn_vat_rate_effective_dates")]
[Index("StateId", Name = "idx_cmn_vat_rate_state_id")]
public partial class CmnVatRate
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

    [InverseProperty("SelectedVatRate")]
    public virtual ICollection<EdoImportCandidateLine> EdoImportCandidateLines { get; set; } = new List<EdoImportCandidateLine>();

    [InverseProperty("VatRate")]
    public virtual ICollection<FaReceiptDocLine> FaReceiptDocLines { get; set; } = new List<FaReceiptDocLine>();

    [InverseProperty("VatRate")]
    public virtual ICollection<PurDocProduct> PurDocProducts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("VatRate")]
    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    [InverseProperty("VatRate")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty("VatRate")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty("VatRate")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnVatRates")]
    public virtual CmnState State { get; set; } = null!;
}
