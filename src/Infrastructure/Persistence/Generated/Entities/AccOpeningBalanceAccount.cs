using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_opening_balance_account")]
[Index("OpeningBalanceId", "ChartAccountId", Name = "acc_opening_balance_account_opening_balance_id_chart_accoun_key", IsUnique = true)]
[Index("ChartAccountId", Name = "ix_acc_opening_balance_account_chart_account")]
[Index("OpeningBalanceId", Name = "ix_acc_opening_balance_account_document")]
public partial class AccOpeningBalanceAccount
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("opening_balance_id")]
    public long OpeningBalanceId { get; set; }

    [Column("chart_account_id")]
    public int ChartAccountId { get; set; }

    [Column("debit_amount")]
    [Precision(24, 8)]
    public decimal DebitAmount { get; set; }

    [Column("credit_amount")]
    [Precision(24, 8)]
    public decimal CreditAmount { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("OpeningBalanceAccount")]
    public virtual ICollection<AccOpeningBalanceAccountDetail> AccOpeningBalanceAccountDetails { get; set; } = new List<AccOpeningBalanceAccountDetail>();

    [ForeignKey("ChartAccountId")]
    [InverseProperty("AccOpeningBalanceAccounts")]
    public virtual AccChartAccount ChartAccount { get; set; } = null!;

    [ForeignKey("OpeningBalanceId")]
    [InverseProperty("AccOpeningBalanceAccounts")]
    public virtual AccOpeningBalance OpeningBalance { get; set; } = null!;
}
