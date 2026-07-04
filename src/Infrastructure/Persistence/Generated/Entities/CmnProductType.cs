using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_product_type")]
[Index("Code", Name = "cmn_product_type_code_key", IsUnique = true)]
public partial class CmnProductType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("is_service")]
    public bool IsService { get; set; }

    [Column("description")]
    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [InverseProperty("ProductType")]
    public virtual ICollection<CmnProductTypeTranslation> CmnProductTypeTranslations { get; set; } = new List<CmnProductTypeTranslation>();

    [InverseProperty("ProductType")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();
}
