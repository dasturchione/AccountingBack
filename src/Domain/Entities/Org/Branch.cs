using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_branch")]
[Index("DistrictId", Name = "idx_org_branch_district_id")]
[Index("OrganizationId", "Code", Name = "idx_org_branch_org_code", IsUnique = true)]
[Index("OrganizationId", Name = "idx_org_branch_organization_id")]
[Index("RegionId", Name = "idx_org_branch_region_id")]
[Index("StateId", Name = "idx_org_branch_state_id")]
public partial class Branch
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

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("district_id")]
    public int? DistrictId { get; set; }

    [Column("address")]
    [StringLength(1000)]
    public string? Address { get; set; }

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [ForeignKey("DistrictId")]
    [InverseProperty("Branches")]
    public virtual District? District { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    [InverseProperty("Branch")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("Branches")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("RegionId")]
    [InverseProperty("Branches")]
    public virtual Region? Region { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("Branches")]
    public virtual State State { get; set; } = null!;
}
