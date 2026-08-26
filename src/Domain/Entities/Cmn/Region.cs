using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_region")]
public partial class Region
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

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Region")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("Region")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty(nameof(BankBranch.Region))]
    public virtual ICollection<BankBranch> BankBranches { get; set; } = new List<BankBranch>();

    [InverseProperty("Region")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    [ForeignKey("StateId")]
    [InverseProperty("Regions")]
    public virtual State State { get; set; } = null!;
}
