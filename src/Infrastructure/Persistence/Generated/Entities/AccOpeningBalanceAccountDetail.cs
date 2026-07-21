using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_opening_balance_account_detail")]
[Index("OpeningBalanceAccountId", Name = "ix_acc_opening_balance_account_detail_account")]
public partial class AccOpeningBalanceAccountDetail
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
    public virtual ICollection<AccOpeningBalanceAccountDetailSubkonto> AccOpeningBalanceAccountDetailSubkontos { get; set; } = new List<AccOpeningBalanceAccountDetailSubkonto>();

    [ForeignKey("CurrencyId")]
    [InverseProperty("AccOpeningBalanceAccountDetails")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OpeningBalanceAccountId")]
    [InverseProperty("AccOpeningBalanceAccountDetails")]
    public virtual AccOpeningBalanceAccount OpeningBalanceAccount { get; set; } = null!;
}
