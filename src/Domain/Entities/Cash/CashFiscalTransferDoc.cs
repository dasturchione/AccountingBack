using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cash_fiscal_transfer_doc")]
[Index(nameof(OrganizationId), nameof(DocDate), Name = "idx_cash_fiscal_transfer_doc_org_doc_date")]
public class CashFiscalTransferDoc
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

    [Column("fiscal_cash_register_id")]
    public int FiscalCashRegisterId { get; set; }

    [Column("cash_box_id")]
    public int CashBoxId { get; set; }

    [Column("direction_id")]
    public short DirectionId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("exchange_rate")]
    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; }

    [Column("fiscal_cash_account_id")]
    public int? FiscalCashAccountId { get; set; }

    [Column("cash_box_account_id")]
    public int? CashBoxAccountId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(FiscalCashRegisterId))]
    public virtual FiscalCashRegister FiscalCashRegister { get; set; } = null!;

    [ForeignKey(nameof(CashBoxId))]
    public virtual CashBox CashBox { get; set; } = null!;

    [ForeignKey(nameof(DirectionId))]
    public virtual MovementDirection Direction { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(FiscalCashAccountId))]
    public virtual ChartAccount? FiscalCashAccount { get; set; }

    [ForeignKey(nameof(CashBoxAccountId))]
    public virtual ChartAccount? CashBoxAccount { get; set; }

    [ForeignKey(nameof(StatusId))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;
}
