using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_group")]
[Index("OrganizationId", Name = "idx_inv_product_group_organization_id")]
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

    [InverseProperty("ProductGroup")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProductGroups")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("InvProductGroups")]
    public virtual CmnState State { get; set; } = null!;
}
