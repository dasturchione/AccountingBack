using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_aggregation")]
[Index("BusinessPlaceId", Name = "idx_marking_aggregation_business_place_id")]
[Index("OrganizationId", Name = "idx_marking_aggregation_organization_id")]
[Index("ParentMarkingCodeId", Name = "idx_marking_aggregation_parent_marking_code_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_aggregation_status")]
public partial class MarkingAggregation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("business_place_id")]
    public int BusinessPlaceId { get; set; }

    [Column("parent_marking_code_id")]
    public long ParentMarkingCodeId { get; set; }

    [Column("packing_date")]
    public DateOnly PackingDate { get; set; }

    [Column("crpt_document_id")]
    [StringLength(100)]
    public string? CrptDocumentId { get; set; }

    [Column("planned_capacity")]
    public int PlannedCapacity { get; set; }

    [Column("actual_capacity")]
    public int ActualCapacity { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("BusinessPlaceId")]
    [InverseProperty("MarkingAggregations")]
    public virtual MarkingBusinessPlace BusinessPlace { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingAggregations")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ParentMarkingCodeId")]
    [InverseProperty("MarkingAggregations")]
    public virtual MarkingCode ParentMarkingCode { get; set; } = null!;
}
