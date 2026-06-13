using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cash_operation")]
[Index("CashBoxId", Name = "idx_cash_operation_cash_box_id")]
[Index("CounterpartyId", Name = "idx_cash_operation_counterparty_id")]
[Index("DocDate", Name = "idx_cash_operation_doc_date")]
[Index("OperationTypeId", Name = "idx_cash_operation_operation_type_id")]
[Index("OrganizationId", Name = "idx_cash_operation_organization_id")]
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

    [ForeignKey("CashBoxId")]
    [InverseProperty("CashOperations")]
    public virtual CashBox CashBox { get; set; } = null!;

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
