using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_accrual_doc_item")]
public sealed class RentalAccrualDocItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("accrual_doc_id")]
    public long AccrualDocId { get; set; }

    [Column("contract_object_id")]
    public long ContractObjectId { get; set; }

    [Column("period_from", TypeName = "date")]
    public DateTime PeriodFrom { get; set; }

    [Column("period_to", TypeName = "date")]
    public DateTime PeriodTo { get; set; }

    [Column("contract_amount")]
    [Precision(24, 8)]
    public decimal ContractAmount { get; set; }

    [Column("tax_base_amount")]
    [Precision(24, 8)]
    public decimal TaxBaseAmount { get; set; }

    [Column("tax_rate")]
    [Precision(9, 6)]
    public decimal TaxRate { get; set; }

    [Column("tax_amount")]
    [Precision(24, 8)]
    public decimal TaxAmount { get; set; }

    [Column("payable_amount")]
    [Precision(24, 8)]
    public decimal PayableAmount { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [ForeignKey(nameof(AccrualDocId))]
    public RentalAccrualDoc AccrualDoc { get; set; } = null!;

    [ForeignKey(nameof(ContractObjectId))]
    public RentalContractObject ContractObject { get; set; } = null!;

    [ForeignKey(nameof(ExpenseAccountId))]
    public ChartAccount? ExpenseAccount { get; set; }
}
