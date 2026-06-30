using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_email_verification_token")]
[Index("Email", Name = "idx_sys_email_verification_token_email")]
[Index("UserId", Name = "idx_sys_email_verification_token_user_id")]
[Index("TokenHash", Name = "sys_email_verification_token_token_hash_key", IsUnique = true)]
public partial class EmailVerificationToken
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("token_hash")]
    [StringLength(512)]
    public string TokenHash { get; set; } = null!;

    [Column("email")]
    [StringLength(200)]
    public string Email { get; set; } = null!;

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("verified_at", TypeName = "timestamp without time zone")]
    public DateTime? VerifiedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
