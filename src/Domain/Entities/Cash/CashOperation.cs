using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cash_operation")]
public partial class CashOperation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("cash_box_id")]
    public int CashBoxId { get; set; }

    [Column("destination_cash_box_id")]
    public int? DestinationCashBoxId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("payment_type_id")]
    public short? PaymentTypeId { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("exchange_rate")]
    [Precision(18, 6)]
    public decimal ExchangeRate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [Column("cash_chart_account_id")]
    public int? CashChartAccountId { get; set; }

    [Column("offset_account_id")]
    public int? OffsetAccountId { get; set; }

    [ForeignKey("CashChartAccountId")]
    [InverseProperty(nameof(ChartAccount.CashOperationCashChartAccounts))]
    public virtual ChartAccount? CashChartAccount { get; set; }

    [ForeignKey("OffsetAccountId")]
    [InverseProperty(nameof(ChartAccount.CashOperationOffsetAccounts))]
    public virtual ChartAccount? OffsetAccount { get; set; }

    [ForeignKey("CashBoxId")]
    [InverseProperty("CashOperations")]
    public virtual CashBox CashBox { get; set; } = null!;

    [ForeignKey("DestinationCashBoxId")]
    public virtual CashBox? DestinationCashBox { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CashOperations")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("CashOperations")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("CashOperations")]
    public virtual OperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CashOperations")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("CashOperations")]
    public virtual PaymentType? PaymentType { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("CashOperations")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("CashOperations")]
    public virtual DocumentStatus Status { get; set; } = null!;
}
