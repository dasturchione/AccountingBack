using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cash_box")]
[Index("BranchId", Name = "idx_cash_box_branch_id")]
[Index("CurrencyId", Name = "idx_cash_box_currency_id")]
[Index("OrganizationId", "Code", Name = "idx_cash_box_org_code", IsUnique = true)]
[Index("OrganizationId", Name = "idx_cash_box_organization_id")]
[Index("StateId", Name = "idx_cash_box_state_id")]
[Index("IsMain", Name = "idx_cash_box_is_main")]
[Index("ResponsibleUserId", Name = "idx_cash_box_responsible_user_id")]
public partial class CashBox
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }


    [Column("is_main")]
    public bool IsMain { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("opening_balance")]
    [Precision(18, 2)]
    public decimal OpeningBalance { get; set; }

    [Column("opening_balance_date")]
    public DateOnly? OpeningBalanceDate { get; set; }
    [ForeignKey("BranchId")]
    [InverseProperty("CashBoxes")]
    public virtual Branch? Branch { get; set; }

    [InverseProperty("CashBox")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [ForeignKey("CurrencyId")]
    [InverseProperty("CashBoxes")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CashBoxes")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CashBoxes")]
    public virtual State State { get; set; } = null!;
}
