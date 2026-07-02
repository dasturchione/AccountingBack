using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("bank_operation_line")]
public partial class BankOperationLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bank_operation_id")]
    public long BankOperationId { get; set; }

    [Column("order_number")]
    public short OrderNumber { get; set; }

    [Column("payment_purpose_id")]
    public short PaymentPurposeId { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("comment")]
    [StringLength(500)]
    public string? Comment { get; set; }

    [ForeignKey("BankOperationId")]
    [InverseProperty("BankOperationLines")]
    public virtual BankOperation BankOperation { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("BankOperationLines")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("PaymentPurposeId")]
    [InverseProperty("BankOperationLines")]
    public virtual PaymentPurpose PaymentPurpose { get; set; } = null!;
}
