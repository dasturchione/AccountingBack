using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_price_rounding_method")]
[Index("Code", Name = "cmn_price_rounding_method_code_key", IsUnique = true)]
public partial class CmnPriceRoundingMethod
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("RoundingMethod")]
    public virtual ICollection<CmnPricingCondition> CmnPricingConditions { get; set; } = new List<CmnPricingCondition>();
}
