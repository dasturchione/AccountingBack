using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_user")]
[Index("Email", Name = "idx_sys_user_email")]
[Index("EmailVerified", Name = "idx_sys_user_email_verified")]
[Index("IsPlatformAdmin", Name = "idx_sys_user_is_platform_admin")]
[Index("LanguageId", Name = "idx_sys_user_language_id")]
[Index("OrganizationId", Name = "idx_sys_user_organization_id")]
[Index("PhoneNumber", Name = "idx_sys_user_phone")]
[Index("RoleId", Name = "idx_sys_user_role_id")]
[Index("UserName", Name = "uidx_sys_user_user_name", IsUnique = true)]
public partial class SysUser
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_name")]
    [StringLength(250)]
    public string UserName { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(250)]
    public string PasswordHash { get; set; } = null!;

    [Column("password_salt")]
    [StringLength(250)]
    public string PasswordSalt { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(50)]
    public string PhoneNumber { get; set; } = null!;

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("first_name")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("last_access_time", TypeName = "timestamp without time zone")]
    public DateTime? LastAccessTime { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("language_id")]
    public short? LanguageId { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("email_verified_at", TypeName = "timestamp without time zone")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("last_login_ip")]
    [StringLength(64)]
    public string? LastLoginIp { get; set; }

    [Column("is_platform_admin")]
    public bool IsPlatformAdmin { get; set; }

    [Column("timezone")]
    [StringLength(100)]
    public string? Timezone { get; set; }

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("CancelledByUser")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRunCancelledByUsers { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRunCreatedByUsers { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("PostedByUser")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRunPostedByUsers { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("UpdatedByUser")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRunUpdatedByUsers { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("CancelledByUser")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocCancelledByUsers { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocCreatedByUsers { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("PostedByUser")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocPostedByUsers { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("UpdatedByUser")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocUpdatedByUsers { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("CancelledByUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocCancelledByUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocCreatedByUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("FromResponsibleUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocFromResponsibleUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("PostedByUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocPostedByUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("ToResponsibleUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocToResponsibleUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("UpdatedByUser")]
    public virtual ICollection<FaMovementDoc> FaMovementDocUpdatedByUsers { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty("CancelledByUser")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocCancelledByUsers { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocCreatedByUsers { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("PostedByUser")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocPostedByUsers { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("UpdatedByUser")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocUpdatedByUsers { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    [ForeignKey("LanguageId")]
    [InverseProperty("SysUsers")]
    public virtual CmnLanguage? Language { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("SysUsers")]
    public virtual OrgOrganization? Organization { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("SysUsers")]
    public virtual SysRole Role { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("SysUsers")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("User")]
    public virtual SysUserOrganization? SysUserOrganization { get; set; }
}
