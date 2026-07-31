using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("integration_credential")]
[Index("OrganizationId", Name = "idx_integration_credential_organization_id")]
[Index("OrganizationId", "Provider", Name = "ux_integration_credential_organization_id_provider", IsUnique = true)]
public partial class IntegrationCredential
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    [StringLength(20)]
    public string Provider { get; set; } = null!;

    [Column("tin")]
    [StringLength(20)]
    public string? Tin { get; set; }

    [Column("api_key")]
    public string? ApiKey { get; set; }

    [Column("login")]
    [StringLength(100)]
    public string? Login { get; set; }

    [Column("password")]
    public string? Password { get; set; }

    [Column("client_id")]
    [StringLength(100)]
    public string? ClientId { get; set; }

    [Column("client_secret")]
    public string? ClientSecret { get; set; }

    [Column("partner_id")]
    [StringLength(100)]
    public string? PartnerId { get; set; }

    [Column("valid_until")]
    public DateTime? ValidUntil { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("IntegrationCredentials")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
