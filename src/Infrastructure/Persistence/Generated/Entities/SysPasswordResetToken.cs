using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_password_reset_token")]
[Index("ExpiresAt", Name = "idx_sys_password_reset_token_expires_at")]
[Index("UserId", Name = "idx_sys_password_reset_token_user_id")]
[Index("TokenHash", Name = "sys_password_reset_token_token_hash_key", IsUnique = true)]
public partial class SysPasswordResetToken
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("token_hash")]
    [StringLength(512)]
    public string TokenHash { get; set; } = null!;

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at", TypeName = "timestamp without time zone")]
    public DateTime? UsedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("SysPasswordResetTokens")]
    public virtual SysUser User { get; set; } = null!;
}
