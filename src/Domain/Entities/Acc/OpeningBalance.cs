using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_opening_balance")]
public partial class OpeningBalance
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("balance_date")]
    public DateOnly BalanceDate { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("OpeningBalance")]
    public virtual ICollection<OpeningBalanceAccount> OpeningBalanceAccounts { get; set; } = new List<OpeningBalanceAccount>();

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.OpeningBalance))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.OpeningBalances))]
    public virtual State State { get; set; } = null!;
}
