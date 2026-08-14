using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_user")]
public partial class User
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

    [Column("tenant_id")]
    public int TenantId { get; set; }

    [Column("user_kind_id")]
    public short UserKindId { get; set; }

    [Column("last_access_time", TypeName = "timestamp without time zone")]
    public DateTime? LastAccessTime { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("language_id")]
    public short? LanguageId { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("email_verified_at", TypeName = "timestamp without time zone")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("last_login_ip")]
    [StringLength(64)]
    public string? LastLoginIp { get; set; }

    [Column("timezone")]
    [StringLength(100)]
    public string? Timezone { get; set; }

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty(nameof(FaCommissioningDoc.CancelledByUser))]
    public virtual ICollection<FaCommissioningDoc> FaCommissioningDocCancelledByUsers { get; set; } = new List<FaCommissioningDoc>();

    [InverseProperty(nameof(FaCommissioningDoc.CreatedByUser))]
    public virtual ICollection<FaCommissioningDoc> FaCommissioningDocCreatedByUsers { get; set; } = new List<FaCommissioningDoc>();

    [InverseProperty(nameof(FaCommissioningDocLine.ResponsibleUser))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLines { get; set; } = new List<FaCommissioningDocLine>();

    [InverseProperty(nameof(FaCommissioningDoc.PostedByUser))]
    public virtual ICollection<FaCommissioningDoc> FaCommissioningDocPostedByUsers { get; set; } = new List<FaCommissioningDoc>();

    [InverseProperty(nameof(FaCommissioningDoc.UpdatedByUser))]
    public virtual ICollection<FaCommissioningDoc> FaCommissioningDocUpdatedByUsers { get; set; } = new List<FaCommissioningDoc>();

    [InverseProperty(nameof(FaReceiptDoc.CancelledByUser))]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocCancelledByUsers { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty(nameof(FaReceiptDoc.PostedByUser))]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocPostedByUsers { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    [ForeignKey("LanguageId")]
    [InverseProperty("Users")]
    public virtual Language? Language { get; set; }

    [ForeignKey(nameof(TenantId))]
    [InverseProperty(nameof(PlatformTenant.Users))]
    public virtual PlatformTenant PlatformTenant { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.Users))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(UserOrganization.User))]
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

    [InverseProperty(nameof(RetailSaleDoc.PostedByUser))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocPostedByUsers { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(RetailSaleDoc.CancelledByUser))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocCancelledByUsers { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(SaleShipmentDoc.AcceptedUser))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocAcceptedUsers { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(OpeningInventory.CancelledByUser))]
    public virtual ICollection<OpeningInventory> OpeningInventoryCancelledByUsers { get; set; } = new List<OpeningInventory>();

    [InverseProperty(nameof(OpeningInventory.PostedByUser))]
    public virtual ICollection<OpeningInventory> OpeningInventoryPostedByUsers { get; set; } = new List<OpeningInventory>();

    [InverseProperty(nameof(SaleShipmentDoc.CancelledUser))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocCancelledUsers { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(SaleShipmentDoc.CreatedUser))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocCreatedUsers { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(SaleShipmentDoc.SubmittedUser))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocSubmittedUsers { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(UserKind.Users))]
    public virtual UserKind UserKind { get; set; } = null!;
}
