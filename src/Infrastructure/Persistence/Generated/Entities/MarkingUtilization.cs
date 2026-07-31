using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_utilization")]
[Index("BusinessPlaceId", Name = "idx_marking_utilization_business_place_id")]
[Index("OrganizationId", Name = "idx_marking_utilization_organization_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_utilization_status")]
public partial class MarkingUtilization
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("business_place_id")]
    public int BusinessPlaceId { get; set; }

    [Column("crpt_document_id")]
    [StringLength(100)]
    public string? CrptDocumentId { get; set; }

    [Column("production_date")]
    public DateOnly ProductionDate { get; set; }

    [Column("expiration_date")]
    public DateOnly? ExpirationDate { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("BusinessPlaceId")]
    [InverseProperty("MarkingUtilizations")]
    public virtual MarkingBusinessPlace BusinessPlace { get; set; } = null!;

    [InverseProperty("Utilization")]
    public virtual ICollection<MarkingCode> MarkingCodes { get; set; } = new List<MarkingCode>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingUtilizations")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
