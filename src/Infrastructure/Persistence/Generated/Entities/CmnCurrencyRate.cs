using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_currency_rate")]
[Index("BaseCurrencyId", Name = "idx_cmn_currency_rate_base_currency_id")]
[Index("EffectiveDate", Name = "idx_cmn_currency_rate_effective_date", AllDescending = true)]
[Index("IsActive", Name = "idx_cmn_currency_rate_is_active")]
[Index("BaseCurrencyId", "TargetCurrencyId", "EffectiveDate", Name = "idx_cmn_currency_rate_pair_effective_date", IsDescending = new[] { false, false, true })]
[Index("StateId", Name = "idx_cmn_currency_rate_state_id")]
[Index("TargetCurrencyId", Name = "idx_cmn_currency_rate_target_currency_id")]
public partial class CmnCurrencyRate
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("base_currency_id")]
    public short BaseCurrencyId { get; set; }

    [Column("target_currency_id")]
    public short TargetCurrencyId { get; set; }

    [Column("effective_date", TypeName = "timestamp without time zone")]
    public DateTime EffectiveDate { get; set; }

    [Column("buy_rate")]
    [Precision(18, 6)]
    public decimal BuyRate { get; set; }

    [Column("sell_rate")]
    [Precision(18, 6)]
    public decimal SellRate { get; set; }

    [Column("official_rate")]
    [Precision(18, 6)]
    public decimal OfficialRate { get; set; }

    [Column("rate_source")]
    [StringLength(100)]
    public string? RateSource { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BaseCurrencyId")]
    [InverseProperty("CmnCurrencyRateBaseCurrencies")]
    public virtual CmnCurrency BaseCurrency { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CmnCurrencyRates")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("TargetCurrencyId")]
    [InverseProperty("CmnCurrencyRateTargetCurrencies")]
    public virtual CmnCurrency TargetCurrency { get; set; } = null!;
}
