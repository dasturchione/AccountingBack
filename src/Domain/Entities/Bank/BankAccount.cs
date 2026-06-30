using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_bank_account")]
[Index("BankId", Name = "idx_org_bank_account_bank_id")]
[Index("CurrencyId", Name = "idx_org_bank_account_currency_id")]
[Index("OrganizationId", Name = "idx_org_bank_account_organization_id")]
[Index("StateId", Name = "idx_org_bank_account_state_id")]
[Index("Code", Name = "idx_org_bank_account_code")]
[Index("Name", Name = "idx_org_bank_account_name")]
public partial class BankAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("bank_id")]
    public int BankId { get; set; }

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

    [InverseProperty("BankAccount")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

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
