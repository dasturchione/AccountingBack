using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("counterparty_bank_account")]
[Index("BankId", Name = "idx_counterparty_bank_account_bank_id")]
[Index("CounterpartyId", Name = "idx_counterparty_bank_account_counterparty_id")]
[Index("CurrencyId", Name = "idx_counterparty_bank_account_currency_id")]
[Index("OrganizationId", Name = "idx_counterparty_bank_account_organization_id")]
[Index("StateId", Name = "idx_counterparty_bank_account_state_id")]
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
    public virtual CmnBank Bank { get; set; } = null!;

    [InverseProperty("CounterpartyBankAccount")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CounterpartyBankAccounts")]
    public virtual CmnState State { get; set; } = null!;
}
