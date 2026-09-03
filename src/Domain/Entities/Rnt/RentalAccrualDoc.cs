using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_accrual_doc")]
public sealed class RentalAccrualDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("contract_id")]
    public long ContractId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("exchange_rate")]
    [Precision(24, 8)]
    public decimal ExchangeRate { get; set; }

    [Column("contract_amount")]
    [Precision(24, 8)]
    public decimal ContractAmount { get; set; }

    [Column("tax_base_amount")]
    [Precision(24, 8)]
    public decimal TaxBaseAmount { get; set; }

    [Column("tax_amount")]
    [Precision(24, 8)]
    public decimal TaxAmount { get; set; }

    [Column("payable_amount")]
    [Precision(24, 8)]
    public decimal PayableAmount { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("lessor_payable_account_id")]
    public int? LessorPayableAccountId { get; set; }

    [Column("tax_payable_account_id")]
    public int? TaxPayableAccountId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ContractId))]
    public RentalContract Contract { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    public Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(LessorPayableAccountId))]
    public ChartAccount? LessorPayableAccount { get; set; }

    [ForeignKey(nameof(TaxPayableAccountId))]
    public ChartAccount? TaxPayableAccount { get; set; }

    [ForeignKey(nameof(StatusId))]
    public DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

    public ICollection<RentalAccrualDocItem> Items { get; set; } = new List<RentalAccrualDocItem>();
}
