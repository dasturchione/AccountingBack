using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_organization_config")]
public partial class OrgOrganizationConfig
{
    [Key]
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("inventory_valuation_method")]
    [StringLength(20)]
    public string InventoryValuationMethod { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgOrganizationConfig")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
