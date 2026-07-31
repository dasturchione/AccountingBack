using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_employee_component")]
[Index("ComponentId", Name = "idx_pay_employee_component_component_id")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_pay_employee_component_effective_dates")]
[Index("EmployeeId", Name = "idx_pay_employee_component_employee_id")]
[Index("OrganizationId", Name = "idx_pay_employee_component_organization_id")]
[Index("EmployeeId", "ComponentId", "EffectiveFrom", Name = "ux_pay_employee_component_period", IsUnique = true)]
public partial class PayEmployeeComponent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("component_id")]
    public int ComponentId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal? Amount { get; set; }

    [Column("rate")]
    [Precision(9, 4)]
    public decimal? Rate { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("ComponentId")]
    [InverseProperty("PayEmployeeComponents")]
    public virtual PayComponent Component { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    [InverseProperty("PayEmployeeComponents")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayEmployeeComponents")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("PayEmployeeComponents")]
    public virtual CmnState State { get; set; } = null!;
}
