using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_bank_account")]
public partial class BankAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("bank_id")]
    public int BankId { get; set; }

    [Column("bank_branch_id")]
    public int? BankBranchId { get; set; }

    [Column("account_number")]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("is_main")]
    public bool IsMain { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string? Name { get; set; }

    [Column("opening_balance")]
    [Precision(18, 2)]
    public decimal OpeningBalance { get; set; }

    [Column("opening_balance_date")]
    public DateOnly? OpeningBalanceDate { get; set; }
    [ForeignKey("BankId")]
    [InverseProperty("BankAccounts")]
    public virtual Bank Bank { get; set; } = null!;

    [ForeignKey(nameof(BankBranchId))]
    [InverseProperty(nameof(BankBranch.BankAccounts))]
    public virtual BankBranch? BankBranch { get; set; }

    [InverseProperty("BankAccount")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(PaymentAcceptancePoint.BankAccount))]
    public virtual ICollection<PaymentAcceptancePoint> PaymentAcceptancePoints { get; set; } = [];

    [ForeignKey("CurrencyId")]
    [InverseProperty("BankAccounts")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("BankAccounts")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("BankAccounts")]
    public virtual State State { get; set; } = null!;
}
