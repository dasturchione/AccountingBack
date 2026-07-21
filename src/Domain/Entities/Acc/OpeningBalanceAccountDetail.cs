using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("acc_opening_balance_account_detail")]
public partial class OpeningBalanceAccountDetail
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("opening_balance_account_id")]
    public long OpeningBalanceAccountId { get; set; }

    [Column("debit_amount")]
    [Precision(24, 8)]
    public decimal DebitAmount { get; set; }

    [Column("credit_amount")]
    [Precision(24, 8)]
    public decimal CreditAmount { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal? Quantity { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("currency_amount")]
    [Precision(24, 8)]
    public decimal? CurrencyAmount { get; set; }

    [Column("exchange_rate")]
    [Precision(24, 8)]
    public decimal? ExchangeRate { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("OpeningBalanceAccountDetail")]
    public virtual ICollection<OpeningBalanceAccountDetailSubkonto> OpeningBalanceAccountDetailSubkontos { get; set; } = new List<OpeningBalanceAccountDetailSubkonto>();

    [ForeignKey("CurrencyId")]
    [InverseProperty(nameof(Currency.OpeningBalanceAccountDetails))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OpeningBalanceAccountId")]
    [InverseProperty("OpeningBalanceAccountDetails")]
    public virtual OpeningBalanceAccount OpeningBalanceAccount { get; set; } = null!;
}
