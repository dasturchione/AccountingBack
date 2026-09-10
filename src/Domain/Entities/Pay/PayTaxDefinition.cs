using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel.Constants;

namespace Domain.Entities;

[Table("pay_tax_definition")]
[Index(nameof(OrganizationId), nameof(Code), nameof(EffectiveFrom), Name = "ux_pay_tax_definition_org_code_from", IsUnique = true)]
[Index(nameof(OrganizationId), nameof(EffectiveFrom), nameof(EffectiveTo), Name = "idx_pay_tax_definition_effective_dates")]
public partial class PayTaxDefinition
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("tax_type")]
    [StringLength(20)]
    public string TaxType { get; set; } = PayrollTaxTypeConst.Withholding;

    [Column("base_type")]
    [StringLength(30)]
    public string BaseType { get; set; } = PayrollTaxBaseTypeConst.Gross;

    [Column("rate")]
    [Precision(9, 4)]
    public decimal Rate { get; set; }

    [Column("exemption_amount")]
    [Precision(18, 2)]
    public decimal? ExemptionAmount { get; set; }

    [Column("limit_amount")]
    [Precision(18, 2)]
    public decimal? LimitAmount { get; set; }

    [Column("liability_account_id")]
    public int LiabilityAccountId { get; set; }

    [Column("effective_from", TypeName = "date")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to", TypeName = "date")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(LiabilityAccountId))]
    public virtual ChartAccount LiabilityAccount { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    public virtual ICollection<PayPayrollTaxLine> PayrollTaxLines { get; set; } = new List<PayPayrollTaxLine>();
}
