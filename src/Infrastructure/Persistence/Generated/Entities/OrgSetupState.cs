using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_setup_state")]
[Index("IsCompleted", Name = "idx_org_setup_state_is_completed")]
[Index("OrganizationId", Name = "org_setup_state_organization_id_key", IsUnique = true)]
public partial class OrgSetupState
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("current_step")]
    [StringLength(100)]
    public string CurrentStep { get; set; } = null!;

    [Column("organization_completed")]
    public bool OrganizationCompleted { get; set; }

    [Column("accounting_completed")]
    public bool AccountingCompleted { get; set; }

    [Column("defaults_completed")]
    public bool DefaultsCompleted { get; set; }

    [Column("users_completed")]
    public bool UsersCompleted { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
