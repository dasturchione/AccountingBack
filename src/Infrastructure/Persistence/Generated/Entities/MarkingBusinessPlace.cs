using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_business_place")]
[Index("OrganizationId", Name = "idx_marking_business_place_organization_id")]
[Index("OrganizationId", "ExternalId", Name = "ux_marking_business_place_org_external_id", IsUnique = true)]
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

    [InverseProperty("BusinessPlace")]
    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    [InverseProperty("BusinessPlace")]
    public virtual ICollection<MarkingAggregation> MarkingAggregations { get; set; } = new List<MarkingAggregation>();

    [InverseProperty("BusinessPlace")]
    public virtual ICollection<MarkingOrder> MarkingOrders { get; set; } = new List<MarkingOrder>();

    [InverseProperty("BusinessPlace")]
    public virtual ICollection<MarkingUtilization> MarkingUtilizations { get; set; } = new List<MarkingUtilization>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingBusinessPlaces")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
