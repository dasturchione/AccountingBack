using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_claim_request")]
[Index("Inn", Name = "idx_org_claim_request_inn")]
[Index("OrganizationId", Name = "idx_org_claim_request_organization_id")]
[Index("RequestedByUserId", Name = "idx_org_claim_request_requested_by_user_id")]
[Index("Status", Name = "idx_org_claim_request_status")]
public partial class OrganizationClaimRequest
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("requested_by_user_id")]
    public int RequestedByUserId { get; set; }

    [Column("inn")]
    [StringLength(20)]
    public string Inn { get; set; } = null!;

    [Column("organization_name")]
    [StringLength(500)]
    public string? OrganizationName { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("review_comment")]
    [StringLength(1000)]
    public string? ReviewComment { get; set; }

    [Column("reviewed_by_user_id")]
    public int? ReviewedByUserId { get; set; }

    [Column("reviewed_at", TypeName = "timestamp without time zone")]
    public DateTime? ReviewedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
