using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_group")]
[Index("Code", Name = "idx_inv_product_group_code")]
[Index("OrganizationId", Name = "idx_inv_product_group_organization_id")]
[Index("ParentId", Name = "idx_inv_product_group_parent_id")]
[Index("SortOrder", Name = "idx_inv_product_group_sort_order")]
[Index("StateId", Name = "idx_inv_product_group_state_id")]
public partial class InvProductGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [InverseProperty("ProductGroup")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProductGroups")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("InvProductGroups")]
    public virtual CmnState State { get; set; } = null!;
}
