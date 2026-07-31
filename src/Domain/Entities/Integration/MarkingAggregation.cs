using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_aggregation")]
[Index(nameof(OrganizationId), Name = "idx_marking_aggregation_organization_id")]
[Index(nameof(BusinessPlaceId), Name = "idx_marking_aggregation_business_place_id")]
[Index(nameof(ParentMarkingCodeId), Name = "idx_marking_aggregation_parent_marking_code_id")]
[Index(nameof(OrganizationId), nameof(Status), Name = "idx_marking_aggregation_status")]
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

    [Column("packing_date", TypeName = "date")]
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

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(BusinessPlaceId))]
    public virtual MarkingBusinessPlace BusinessPlace { get; set; } = null!;

    [ForeignKey(nameof(ParentMarkingCodeId))]
    public virtual MarkingCode ParentMarkingCode { get; set; } = null!;
}
