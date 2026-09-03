using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("counterparty_bank_account")]
public partial class CounterpartyBankAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

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

    [ForeignKey("BankId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual Bank Bank { get; set; } = null!;

    [ForeignKey(nameof(BankBranchId))]
    [InverseProperty(nameof(Domain.Entities.BankBranch.CounterpartyBankAccounts))]
    public virtual BankBranch? BankBranch { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(BankOperation.CounterpartyBankAccount))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
}
