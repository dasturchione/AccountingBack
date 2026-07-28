using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("cmn_costing_method")]
[Index("Code", Name = "cmn_costing_method_code_key", IsUnique = true)]
public partial class CostingMethod
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

    [InverseProperty("CostingMethod")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty(nameof(CostingMethodTranslation.CostingMethod))]
    public virtual ICollection<CostingMethodTranslation> CostingMethodTranslations { get; set; } = new List<CostingMethodTranslation>();
}
