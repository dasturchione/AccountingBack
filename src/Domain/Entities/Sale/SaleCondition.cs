using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("sale_condition")]
[Index("CostingMethodId", Name = "idx_sale_condition_costing_method_id")]
[Index("StartDate", "EndDate", Name = "idx_sale_condition_dates")]
[Index("OrganizationId", "StateId", "StartDate", "EndDate", Name = "idx_sale_condition_org_state_dates")]
[Index("OrganizationId", Name = "idx_sale_condition_organization_id")]
[Index("StateId", Name = "idx_sale_condition_state_id")]
[Index("VatRateId", Name = "idx_sale_condition_vat_rate_id")]
public partial class SaleCondition
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("costing_method_id")]
    public short CostingMethodId { get; set; }

    [Column("vat_rate_id")]
    public short VatRateId { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CostingMethodId")]
    [InverseProperty("SaleConditions")]
    public virtual CostingMethod CostingMethod { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("SaleConditions")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("SaleConditions")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("SaleConditions")]
    public virtual VatRate VatRate { get; set; } = null!;
}
