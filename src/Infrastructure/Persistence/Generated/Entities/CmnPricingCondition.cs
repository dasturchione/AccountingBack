using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_pricing_condition")]
[Index("StartDate", "EndDate", Name = "idx_cmn_pricing_condition_dates")]
[Index("OrganizationId", Name = "idx_cmn_pricing_condition_organization_id")]
[Index("PricingMethodId", Name = "idx_cmn_pricing_condition_pricing_method_id")]
[Index("RoundingMethodId", Name = "idx_cmn_pricing_condition_rounding_method_id")]
public partial class CmnPricingCondition
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("pricing_method_id")]
    public short PricingMethodId { get; set; }

    [Column("pricing_value")]
    [Precision(18, 2)]
    public decimal PricingValue { get; set; }

    [Column("rounding_method_id")]
    public short RoundingMethodId { get; set; }

    [Column("rounding_precision")]
    [Precision(12, 2)]
    public decimal RoundingPrecision { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("CmnPricingConditions")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PricingMethodId")]
    [InverseProperty("CmnPricingConditions")]
    public virtual CmnPricingMethod PricingMethod { get; set; } = null!;

    [ForeignKey("RoundingMethodId")]
    [InverseProperty("CmnPricingConditions")]
    public virtual CmnPriceRoundingMethod RoundingMethod { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CmnPricingConditions")]
    public virtual CmnState State { get; set; } = null!;
}
