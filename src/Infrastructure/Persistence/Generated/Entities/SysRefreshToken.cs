using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_refresh_token")]
[Index("ExpiresAt", Name = "idx_sys_refresh_token_expires_at")]
[Index("RevokedAt", Name = "idx_sys_refresh_token_revoked_at")]
[Index("UserId", Name = "idx_sys_refresh_token_user_id")]
[Index("TokenHash", Name = "sys_refresh_token_token_hash_key", IsUnique = true)]
public partial class SysRefreshToken
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("token_hash")]
    [StringLength(512)]
    public string TokenHash { get; set; } = null!;

    [Column("jwt_id")]
    [StringLength(128)]
    public string? JwtId { get; set; }

    [Column("device_name")]
    [StringLength(250)]
    public string? DeviceName { get; set; }

    [Column("ip_address")]
    [StringLength(64)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(500)]
    public string? UserAgent { get; set; }

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("revoked_at", TypeName = "timestamp without time zone")]
    public DateTime? RevokedAt { get; set; }

    [Column("replaced_by_token_hash")]
    [StringLength(512)]
    public string? ReplacedByTokenHash { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
