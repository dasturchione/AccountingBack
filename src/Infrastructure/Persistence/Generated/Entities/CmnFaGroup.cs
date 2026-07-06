using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_fa_group")]
[Index("OrganizationId", Name = "idx_cmn_fa_group_organization_id")]
[Index("StateId", Name = "idx_cmn_fa_group_state_id")]
[Index("OrganizationId", "Code", Name = "uidx_cmn_fa_group_org_code", IsUnique = true)]
public partial class CmnFaGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("FaGroup")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("FaGroup")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("CmnFaGroups")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CmnFaGroups")]
    public virtual CmnState State { get; set; } = null!;
}
