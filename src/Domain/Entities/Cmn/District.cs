using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_district")]
public partial class District
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(250)]
    public string FullName { get; set; } = null!;

    [Column("code")]
    [StringLength(10)]
    public string? Code { get; set; }

    [Column("region_id")]
    public int RegionId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("District")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("District")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty(nameof(BankBranch.District))]
    public virtual ICollection<BankBranch> BankBranches { get; set; } = new List<BankBranch>();

    [InverseProperty("District")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
