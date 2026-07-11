using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cash_operation")]
[Index("CancelledByUserId", Name = "idx_cash_operation_cancelled_by_user_id")]
[Index("CashBoxId", Name = "idx_cash_operation_cash_box_id")]
[Index("CashChartAccountId", Name = "idx_cash_operation_cash_chart_account_id")]
[Index("CounterpartyId", Name = "idx_cash_operation_counterparty_id")]
[Index("DestinationCashBoxId", Name = "idx_cash_operation_destination_cash_box_id")]
[Index("DocDate", Name = "idx_cash_operation_doc_date")]
[Index("OffsetAccountId", Name = "idx_cash_operation_offset_account_id")]
[Index("OperationTypeId", Name = "idx_cash_operation_operation_type_id")]
[Index("OrganizationId", Name = "idx_cash_operation_organization_id")]
[Index("PostedByUserId", Name = "idx_cash_operation_posted_by_user_id")]
[Index("StateId", Name = "idx_cash_operation_state_id")]
[Index("StatusId", Name = "idx_cash_operation_status_id")]
public partial class CashOperation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("cash_box_id")]
    public int CashBoxId { get; set; }

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

    [Column("destination_cash_box_id")]
    public int? DestinationCashBoxId { get; set; }
    [Column("cash_chart_account_id")]
    public int? CashChartAccountId { get; set; }

    [Column("offset_account_id")]
    public int? OffsetAccountId { get; set; }

    [ForeignKey("CashChartAccountId")]
    [InverseProperty("CashOperationCashChartAccounts")]
    public virtual AccChartAccount? CashChartAccount { get; set; }

    [ForeignKey("OffsetAccountId")]
    [InverseProperty("CashOperationOffsetAccounts")]
    public virtual AccChartAccount? OffsetAccount { get; set; }

    [ForeignKey("CashBoxId")]
    [InverseProperty("CashOperationCashBoxes")]
    public virtual CashBox CashBox { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CashOperations")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("CashOperations")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("DestinationCashBoxId")]
    [InverseProperty("CashOperationDestinationCashBoxes")]
    public virtual CashBox? DestinationCashBox { get; set; }

    [ForeignKey("OperationTypeId")]
    [InverseProperty("CashOperations")]
    public virtual CmnOperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CashOperations")]
    public virtual OrgOrganization Organization { get; set; } = null!;
    [ForeignKey("PaymentTypeId")]
    [InverseProperty("CashOperations")]
    public virtual CmnPaymentType? PaymentType { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("CashOperations")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("CashOperations")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;
}
