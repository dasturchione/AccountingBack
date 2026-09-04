using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_contract_object")]
public sealed class RentalContractObject
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("contract_id")]
    public long ContractId { get; set; }

    [Column("rental_object_type_id")]
    public short RentalObjectTypeId { get; set; }

    [Column("object_name")]
    [StringLength(500)]
    public string ObjectName { get; set; } = null!;

    [Column("object_identifier")]
    [StringLength(250)]
    public string? ObjectIdentifier { get; set; }

    [Column("object_address")]
    [StringLength(1000)]
    public string? ObjectAddress { get; set; }

    [Column("total_area")]
    [Precision(24, 8)]
    public decimal? TotalArea { get; set; }

    [Column("rented_area")]
    [Precision(24, 8)]
    public decimal? RentedArea { get; set; }

    [Column("start_date", TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Column("period_unit")]
    [StringLength(20)]
    public string PeriodUnit { get; set; } = null!;

    [Column("period_value")]
    public int PeriodValue { get; set; }

    [Column("next_accrual_date", TypeName = "date")]
    public DateTime NextAccrualDate { get; set; }

    [Column("contract_amount")]
    [Precision(24, 8)]
    public decimal ContractAmount { get; set; }

    [Column("tax_base_amount")]
    [Precision(24, 8)]
    public decimal TaxBaseAmount { get; set; }

    [Column("tax_rate")]
    [Precision(9, 6)]
    public decimal TaxRate { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(ContractId))]
    public RentalContract Contract { get; set; } = null!;

    [ForeignKey(nameof(RentalObjectTypeId))]
    public RentalObjectType RentalObjectType { get; set; } = null!;

    [ForeignKey(nameof(ExpenseAccountId))]
    public ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

    public ICollection<RentalAccrualDocItem> AccrualItems { get; set; } = new List<RentalAccrualDocItem>();
    public ICollection<RentalContractObjectUtility> Utilities { get; set; } = [];
}
