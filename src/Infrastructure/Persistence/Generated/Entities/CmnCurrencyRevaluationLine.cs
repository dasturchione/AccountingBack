using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_currency_revaluation_line")]
[Index("RevaluationId", Name = "idx_cmn_currency_revaluation_line_revaluation_id")]
[Index("TargetCurrencyId", Name = "idx_cmn_currency_revaluation_line_target_currency_id")]
public partial class CmnCurrencyRevaluationLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("revaluation_id")]
    public long RevaluationId { get; set; }

    [Column("base_currency_id")]
    public short BaseCurrencyId { get; set; }

    [Column("target_currency_id")]
    public short TargetCurrencyId { get; set; }

    [Column("balance_amount")]
    [Precision(18, 2)]
    public decimal BalanceAmount { get; set; }

    [Column("opening_rate")]
    [Precision(18, 6)]
    public decimal OpeningRate { get; set; }

    [Column("current_rate")]
    [Precision(18, 6)]
    public decimal CurrentRate { get; set; }

    [Column("difference_amount")]
    [Precision(18, 2)]
    public decimal DifferenceAmount { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BaseCurrencyId")]
    [InverseProperty("CmnCurrencyRevaluationLineBaseCurrencies")]
    public virtual CmnCurrency BaseCurrency { get; set; } = null!;

    [ForeignKey("RevaluationId")]
    [InverseProperty("CmnCurrencyRevaluationLines")]
    public virtual CmnCurrencyRevaluation Revaluation { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CmnCurrencyRevaluationLines")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("TargetCurrencyId")]
    [InverseProperty("CmnCurrencyRevaluationLineTargetCurrencies")]
    public virtual CmnCurrency TargetCurrency { get; set; } = null!;
}
