using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cash_collection_doc")]
public sealed class CashCollectionDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("cash_box_id")]
    public int CashBoxId { get; set; }

    [Column("bank_account_id")]
    public int BankAccountId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("exchange_rate")]
    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; }

    [Column("cash_chart_account_id")]
    public int? CashChartAccountId { get; set; }

    [Column("cash_in_transit_account_id")]
    public int? CashInTransitAccountId { get; set; }

    [Column("bank_chart_account_id")]
    public int? BankChartAccountId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("in_transit_at", TypeName = "timestamp without time zone")]
    public DateTime? InTransitAt { get; set; }

    [Column("in_transit_by_user_id")]
    public int? InTransitByUserId { get; set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; set; }

    [Column("completed_by_user_id")]
    public int? CompletedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [Column("cancelled_from_status_id")]
    public short? CancelledFromStatusId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(CashBoxId))]
    public CashBox CashBox { get; set; } = null!;

    [ForeignKey(nameof(BankAccountId))]
    public BankAccount BankAccount { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    public Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(CashChartAccountId))]
    public ChartAccount? CashChartAccount { get; set; }

    [ForeignKey(nameof(CashInTransitAccountId))]
    public ChartAccount? CashInTransitAccount { get; set; }

    [ForeignKey(nameof(BankChartAccountId))]
    public ChartAccount? BankChartAccount { get; set; }

    [ForeignKey(nameof(StatusId))]
    public DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

}
