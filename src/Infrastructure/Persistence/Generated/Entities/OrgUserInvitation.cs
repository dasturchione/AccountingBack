using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_user_invitation")]
[Index("Email", Name = "idx_org_user_invitation_email")]
[Index("OrganizationId", Name = "idx_org_user_invitation_organization_id")]
[Index("RoleId", Name = "idx_org_user_invitation_role_id")]
[Index("TokenHash", Name = "org_user_invitation_token_hash_key", IsUnique = true)]
public partial class OrgUserInvitation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("email")]
    [StringLength(200)]
    public string Email { get; set; } = null!;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("invited_by_user_id")]
    public int? InvitedByUserId { get; set; }

    [Column("token_hash")]
    [StringLength(512)]
    public string TokenHash { get; set; } = null!;

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("accepted_at", TypeName = "timestamp without time zone")]
    public DateTime? AcceptedAt { get; set; }

    [Column("accepted_by_user_id")]
    public int? AcceptedByUserId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
