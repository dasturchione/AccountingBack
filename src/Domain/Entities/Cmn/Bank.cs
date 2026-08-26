using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank")]
[Index("Code", Name = "idx_cmn_bank_code", IsUnique = true)]
public partial class Bank
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("legal_name")]
    [StringLength(500)]
    public string? LegalName { get; set; }

    [Column("license_number")]
    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [Column("license_date")]
    public DateOnly? LicenseDate { get; set; }

    [Column("address")]
    [StringLength(500)]
    public string? Address { get; set; }

    [Column("opened_date")]
    public DateOnly? OpenedDate { get; set; }

    [Column("source_updated_date")]
    public DateOnly? SourceUpdatedDate { get; set; }

    [Column("inn")]
    [StringLength(20)]
    public string? Inn { get; set; } 

    [Column("mfo")]
    [StringLength(20)]
    public string? Mfo { get; set; }

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

    [InverseProperty("Bank")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Bank")]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty(nameof(BankBranch.Bank))]
    public virtual ICollection<BankBranch> BankBranches { get; set; } = new List<BankBranch>();

    [ForeignKey("StateId")]
    [InverseProperty("Banks")]
    public virtual State State { get; set; } = null!;
}
