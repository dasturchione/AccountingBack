using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("counterparty_didox_profile")]
[Index("CounterpartyId", Name = "idx_counterparty_didox_profile_counterparty_id")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_counterparty_didox_profile_effective_dates")]
[Index("OrganizationId", Name = "idx_counterparty_didox_profile_organization_id")]
[Index("StateId", Name = "idx_counterparty_didox_profile_state_id")]
[Index("VatRegStatusCode", Name = "idx_counterparty_didox_profile_vat_reg_status_code")]
[Index("CounterpartyId", "EffectiveFrom", Name = "ux_counterparty_didox_profile_counterparty_effective_from", IsUnique = true)]
public partial class CounterpartyDidoxProfile
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("vat_reg_code")]
    [StringLength(100)]
    public string VatRegCode { get; set; } = null!;

    [Column("vat_reg_status_code")]
    public short VatRegStatusCode { get; set; }

    [Column("legal_address")]
    [StringLength(1000)]
    public string LegalAddress { get; set; } = null!;

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CounterpartyId")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("VatRegStatusCode")]
    public virtual CmnDidoxVatRegStatus VatRegStatus { get; set; } = null!;
}
