using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_department")]
[Index("BranchId", Name = "idx_org_department_branch_id")]
[Index("OrganizationId", "Code", Name = "idx_org_department_org_code", IsUnique = true)]
[Index("OrganizationId", Name = "idx_org_department_organization_id")]
[Index("StateId", Name = "idx_org_department_state_id")]
public partial class OrgDepartment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("branch_id")]
    public int? BranchId { get; set; }

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

    [ForeignKey("BranchId")]
    [InverseProperty("OrgDepartments")]
    public virtual OrgBranch? Branch { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgDepartments")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("OrgDepartments")]
    public virtual CmnState State { get; set; } = null!;
}
