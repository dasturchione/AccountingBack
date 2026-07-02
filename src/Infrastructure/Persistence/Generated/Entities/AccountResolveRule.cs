using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_account_resolve_rule")]
public partial class AccountResolveRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("policy_id")]
    public short PolicyId { get; set; }

    [Column("alias")]
    [StringLength(250)]
    public string Alias { get; set; } = null!;

    [Column("dimension_key")]
    [StringLength(250)]
    public string DimensionKey { get; set; } = null!;

    [Column("dimension_value")]
    [StringLength(250)]
    public string DimensionValue { get; set; } = null!;

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("AccountResolveRules")]
    public virtual ChartAccount Account { get; set; } = null!;

    [ForeignKey("PolicyId")]
    [InverseProperty("AccountResolveRules")]
    public virtual AccountingPolicy Policy { get; set; } = null!;
}
