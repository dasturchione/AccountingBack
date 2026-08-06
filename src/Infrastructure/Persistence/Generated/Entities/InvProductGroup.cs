using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_group")]
[Index("Code", Name = "idx_inv_product_group_code")]
[Index("ParentId", Name = "idx_inv_product_group_parent_id")]
[Index("SortOrder", Name = "idx_inv_product_group_sort_order")]
[Index("StateId", Name = "idx_inv_product_group_state_id")]
[Index("Code", Name = "uq_inv_product_group_code", IsUnique = true)]
public partial class InvProductGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_assignable")]
    public bool IsAssignable { get; set; }

    [InverseProperty("ProductGroup")]
    public virtual ICollection<InvProductGroupTranslation> InvProductGroupTranslations { get; set; } = new List<InvProductGroupTranslation>();

    [InverseProperty("ProductGroup")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("InvProductGroups")]
    public virtual CmnState State { get; set; } = null!;
}
