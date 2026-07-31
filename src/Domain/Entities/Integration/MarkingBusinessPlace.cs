using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_business_place")]
[Index(nameof(OrganizationId), Name = "idx_marking_business_place_organization_id")]
[Index(nameof(OrganizationId), nameof(ExternalId), Name = "ux_marking_business_place_org_external_id", IsUnique = true)]
public partial class MarkingBusinessPlace
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("external_id")]
    [StringLength(50)]
    public string ExternalId { get; set; } = null!;

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("address")]
    [StringLength(500)]
    public string? Address { get; set; }

    [Column("is_manufacturing")]
    public bool IsManufacturing { get; set; }

    [Column("is_storage")]
    public bool IsStorage { get; set; }

    [Column("is_sale")]
    public bool IsSale { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;
}
