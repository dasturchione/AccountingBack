using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_posting_batch")]
public partial class PostingBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime PostedAt { get; set; }

    [Column("reversed_by_user_id")]
    public int? ReversedByUserId { get; set; }

    [Column("reversed_at", TypeName = "timestamp without time zone")]
    public DateTime? ReversedAt { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }
}
