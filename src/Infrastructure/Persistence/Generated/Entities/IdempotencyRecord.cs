using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("idempotency_record")]
[Index("OrganizationId", Name = "idx_idempotency_record_organization_id")]
[Index("OrganizationId", "Status", Name = "idx_idempotency_record_status")]
[Index("OrganizationId", "IdempotencyKey", Name = "uidx_idempotency_record_org_key", IsUnique = true)]
public partial class IdempotencyRecord
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("idempotency_key")]
    [StringLength(200)]
    public string IdempotencyKey { get; set; } = null!;

    [Column("operation_type")]
    [StringLength(100)]
    public string OperationType { get; set; } = null!;

    [Column("request_hash")]
    [StringLength(128)]
    public string? RequestHash { get; set; }

    [Column("request_reference")]
    [StringLength(200)]
    public string? RequestReference { get; set; }

    [Column("result_document_id")]
    [StringLength(100)]
    public string? ResultDocumentId { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("IdempotencyRecords")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
