using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("acc_opening_balance_account")]
public partial class OpeningBalanceAccount
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
    public virtual ICollection<OpeningBalanceAccountDetail> OpeningBalanceAccountDetails { get; set; } = new List<OpeningBalanceAccountDetail>();

    [ForeignKey("ChartAccountId")]
    [InverseProperty(nameof(ChartAccount.OpeningBalanceAccounts))]
    public virtual ChartAccount ChartAccount { get; set; } = null!;

    [ForeignKey("OpeningBalanceId")]
    [InverseProperty(nameof(OpeningBalance.OpeningBalanceAccounts))]
    public virtual OpeningBalance OpeningBalance { get; set; } = null!;
}
