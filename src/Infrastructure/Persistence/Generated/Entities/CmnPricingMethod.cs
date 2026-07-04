using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_pricing_method")]
[Index("Code", Name = "cmn_pricing_method_code_key", IsUnique = true)]
public partial class CmnPricingMethod
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
    public virtual ICollection<CmnPricingCondition> CmnPricingConditions { get; set; } = new List<CmnPricingCondition>();
}
