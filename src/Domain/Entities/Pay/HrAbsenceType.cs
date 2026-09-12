using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("hr_absence_type")]
public sealed class HrAbsenceType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("timesheet_category")]
    [StringLength(20)]
    public string TimesheetCategory { get; set; } = null!;

    [Column("is_paid")]
    public bool IsPaid { get; set; }

    /// <summary>
    /// Kasallik nafaqаси foizi (0..100). Ta'til turlари uchun 100. O'rtacha ish
    /// haqidан nafaqа hisoblаshда ishlatилади.
    /// </summary>
    [Column("benefit_percent")]
    [Precision(5, 2)]
    public decimal BenefitPercent { get; set; } = 100m;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    public ICollection<HrAbsence> Absences { get; set; } = [];
}
