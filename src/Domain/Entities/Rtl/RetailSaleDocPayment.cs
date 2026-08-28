using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("rtl_sale_doc_payment")]
public partial class RetailSaleDocPayment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("payment_method_id")]
    public short PaymentMethodId { get; set; }

    [Column("payment_acceptance_point_id")]
    public int? PaymentAcceptancePointId { get; set; }

    [Column("debit_account_id")]
    public int DebitAccountId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("transaction_number")]
    [StringLength(100)]
    public string? TransactionNumber { get; set; }

    [ForeignKey(nameof(PaymentAcceptancePointId))]
    [InverseProperty(nameof(PaymentAcceptancePoint.RetailSaleDocPayments))]
    public virtual PaymentAcceptancePoint? PaymentAcceptancePoint { get; set; }

    [ForeignKey(nameof(DebitAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocPayments))]
    public virtual ChartAccount DebitAccount { get; set; } = null!;

    [ForeignKey(nameof(OwnerId))]
    [InverseProperty(nameof(RetailSaleDoc.RetailSaleDocPayments))]
    public virtual RetailSaleDoc Owner { get; set; } = null!;

    [ForeignKey(nameof(PaymentMethodId))]
    [InverseProperty(nameof(PaymentMethod.RetailSaleDocPayments))]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
