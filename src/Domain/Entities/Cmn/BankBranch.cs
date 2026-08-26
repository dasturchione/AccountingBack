using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_branch")]
[Index(nameof(Mfo), Name = "uq_cmn_bank_branch_mfo", IsUnique = true)]
[Index(nameof(BankId), Name = "idx_cmn_bank_branch_bank_id")]
[Index(nameof(BranchType), Name = "idx_cmn_bank_branch_branch_type")]
[Index(nameof(RegionId), Name = "idx_cmn_bank_branch_region_id")]
[Index(nameof(DistrictId), Name = "idx_cmn_bank_branch_district_id")]
[Index(nameof(Inn), Name = "idx_cmn_bank_branch_inn")]
[Index(nameof(StateId), Name = "idx_cmn_bank_branch_state_id")]
public class BankBranch
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bank_id")]
    public int BankId { get; set; }

    [Column("mfo")]
    [StringLength(5)]
    public string Mfo { get; set; } = null!;

    [Column("branch_type")]
    public short BranchType { get; set; }

    [Column("name")]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("address")]
    [StringLength(500)]
    public string? Address { get; set; }

    [Column("opened_date")]
    public DateOnly? OpenedDate { get; set; }

    [Column("source_updated_date")]
    public DateOnly? SourceUpdatedDate { get; set; }

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("district_id")]
    public int? DistrictId { get; set; }

    [Column("city")]
    [StringLength(250)]
    public string? City { get; set; }

    [Column("inn")]
    [StringLength(20)]
    public string? Inn { get; set; }

    [Column("website")]
    [StringLength(250)]
    public string? Website { get; set; }

    [Column("latitude", TypeName = "numeric(9,6)")]
    public decimal? Latitude { get; set; }

    [Column("longitude", TypeName = "numeric(9,6)")]
    public decimal? Longitude { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(BankId))]
    [InverseProperty(nameof(Bank.BankBranches))]
    public virtual Bank Bank { get; set; } = null!;

    [InverseProperty(nameof(BankAccount.BankBranch))]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty(nameof(CounterpartyBankAccount.BankBranch))]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [ForeignKey(nameof(RegionId))]
    [InverseProperty(nameof(Region.BankBranches))]
    public virtual Region? Region { get; set; }

    [ForeignKey(nameof(DistrictId))]
    [InverseProperty(nameof(District.BankBranches))]
    public virtual District? District { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankBranches))]
    public virtual State State { get; set; } = null!;
}
