using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_position")]
[Index("OrganizationId", "Code", Name = "idx_org_position_org_code", IsUnique = true)]
[Index("OrganizationId", Name = "idx_org_position_organization_id")]
[Index("StateId", Name = "idx_org_position_state_id")]
public partial class OrgPosition
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
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgPositions")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Position")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [ForeignKey("StateId")]
    [InverseProperty("OrgPositions")]
    public virtual CmnState State { get; set; } = null!;
}
