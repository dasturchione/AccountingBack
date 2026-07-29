using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("pay_employee_component")]
[Index(nameof(OrganizationId), Name = "idx_pay_employee_component_organization_id")]
[Index(nameof(EmployeeId), Name = "idx_pay_employee_component_employee_id")]
[Index(nameof(ComponentId), Name = "idx_pay_employee_component_component_id")]
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

    [ForeignKey(nameof(EmployeeId))]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(ComponentId))]
    public virtual PayComponent Component { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;
}
