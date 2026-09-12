using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Kadr buyrug'i (prikaz). 1C ZUP kadr hujjatlariga mos: DRAFT holatida yaratiladi,
/// tasdiqlanganda (POSTED) tegishli <see cref="PayEmployment"/> intervalini hosil qiladi
/// (yoki bo'shatishда yopadi). Buyruq target snapshot'ini saqlaydi — bosma buyruq va
/// tasdiqda bajarish deterministik bo'lishi uchun.
/// </summary>
[Table("pay_hr_order")]
[Index(nameof(OrganizationId), Name = "idx_pay_hr_order_organization_id")]
[Index(nameof(EmployeeId), Name = "idx_pay_hr_order_employee_id")]
[Index(nameof(OrganizationId), nameof(OrderNumber), Name = "uq_pay_hr_order_number", IsUnique = true)]
public partial class PayHrOrder
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("order_number")]
    [StringLength(30)]
    public string OrderNumber { get; set; } = null!;

    [Column("order_date", TypeName = "date")]
    public DateOnly OrderDate { get; set; }

    /// <summary>HIRE / TRANSFER / PAY_CHANGE / DISMISSAL. See <see cref="SharedKernel.Constants.PayrollHrOrderTypeConst"/>.</summary>
    [Column("order_type")]
    [StringLength(20)]
    public string OrderType { get; set; } = null!;

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("effective_date", TypeName = "date")]
    public DateOnly EffectiveDate { get; set; }

    /// <summary>DRAFT / POSTED / CANCELLED. See <see cref="SharedKernel.Constants.DocumentStatusIdConst"/>.</summary>
    [Column("status_id")]
    public short StatusId { get; set; }

    /// <summary>Buyruq asosi (masalan "Ariza asosida").</summary>
    [Column("basis")]
    [StringLength(500)]
    public string? Basis { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    // ── Target snapshot (yangi qiymatlar) ──────────────────────────────────
    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("position_id")]
    public int? PositionId { get; set; }

    [Column("employment_type")]
    [StringLength(30)]
    public string? EmploymentType { get; set; }

    [Column("monthly_salary")]
    [Precision(18, 2)]
    public decimal? MonthlySalary { get; set; }

    [Column("employment_rate")]
    [Precision(5, 4)]
    public decimal? EmploymentRate { get; set; }

    [Column("weekly_hours")]
    [Precision(6, 2)]
    public decimal? WeeklyHours { get; set; }

    [Column("currency_id")]
    public short? CurrencyId { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("advance_method")]
    [StringLength(10)]
    public string? AdvanceMethod { get; set; }

    [Column("advance_value")]
    [Precision(18, 2)]
    public decimal? AdvanceValue { get; set; }

    /// <summary>Tasdiqda hosil bo'lgan (yoki yopilgan) employment intervali.</summary>
    [Column("employment_id")]
    public long? EmploymentId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

    [Column("confirmed_date", TypeName = "timestamp without time zone")]
    public DateTime? ConfirmedDate { get; set; }

    [Column("confirmed_by_user_id")]
    public int? ConfirmedByUserId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }

    [ForeignKey(nameof(PositionId))]
    public virtual Position? Position { get; set; }

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency? Currency { get; set; }

    [ForeignKey(nameof(ExpenseAccountId))]
    public virtual ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey(nameof(EmploymentId))]
    public virtual PayEmployment? Employment { get; set; }

    [ForeignKey(nameof(StatusId))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey(nameof(ConfirmedByUserId))]
    public virtual User? ConfirmedByUser { get; set; }
}
