using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_tax_settings")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_org_tax_settings_effective_dates")]
[Index("OrganizationId", Name = "idx_org_tax_settings_organization_id")]
[Index("TaxTypeId", Name = "idx_org_tax_settings_tax_type_id")]
public partial class OrganizationTaxSetting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("tax_type_id")]
    public short TaxTypeId { get; set; }

    [Column("is_vat_payer")]
    public bool IsVatPayer { get; set; }

    [Column("vat_registration_number")]
    [StringLength(100)]
    public string? VatRegistrationNumber { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
