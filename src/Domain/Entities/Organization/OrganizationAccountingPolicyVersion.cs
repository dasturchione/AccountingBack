using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_accounting_policy_version")]
public sealed class OrganizationAccountingPolicyVersion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("version")]
    public int Version { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("inventory_valuation_method")]
    [StringLength(20)]
    public string InventoryValuationMethod { get; set; } = null!;

    [Column("base_currency_id")]
    public short BaseCurrencyId { get; set; }

    [Column("vat_payer")]
    public bool VatPayer { get; set; }

    [Column("tax_type_id")]
    public short? TaxTypeId { get; set; }

    [Column("vat_tax_period")]
    [StringLength(20)]
    public string VatTaxPeriod { get; set; } = null!;

    [Column("vat_base_moment")]
    [StringLength(20)]
    public string VatBaseMoment { get; set; } = null!;

    [Column("closed_period_policy")]
    [StringLength(40)]
    public string ClosedPeriodPolicy { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("created_by_user_id")]
    public int CreatedByUserId { get; set; }
}
