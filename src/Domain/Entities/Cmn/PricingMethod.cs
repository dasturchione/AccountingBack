using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("cmn_pricing_method")]
[Index("Code", Name = "cmn_pricing_method_code_key", IsUnique = true)]
public partial class PricingMethod
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [InverseProperty("PricingMethod")]
    public virtual ICollection<PricingCondition> PricingConditions { get; set; } = new List<PricingCondition>();
}
