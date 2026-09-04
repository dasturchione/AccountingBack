using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_contract")]
public sealed class RentalContract
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("contract_number")]
    [StringLength(100)]
    public string ContractNumber { get; set; } = null!;

    [Column("contract_date", TypeName = "date")]
    public DateTime ContractDate { get; set; }

    [Column("start_date", TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Column("is_free_of_charge")]
    public bool IsFreeOfCharge { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

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

    public ICollection<RentalContractObject> Objects { get; set; } = new List<RentalContractObject>();
    public ICollection<RentalContractLessor> Lessors { get; set; } = [];
    public ICollection<RentalAccrualDoc> AccrualDocuments { get; set; } = new List<RentalAccrualDoc>();
}
