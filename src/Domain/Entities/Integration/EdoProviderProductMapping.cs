using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_provider_product_mapping")]
public sealed class EdoProviderProductMapping
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = string.Empty;

    [Column("catalog_code")]
    [StringLength(50)]
    public string CatalogCode { get; set; } = string.Empty;

    [Column("package_code")]
    [StringLength(50)]
    public string PackageCode { get; set; } = string.Empty;

    [Column("provider_product_name")]
    [StringLength(500)]
    public string ProviderProductName { get; set; } = string.Empty;

    [Column("provider_product_name_hash")]
    [StringLength(64)]
    public string ProviderProductNameHash { get; set; } = string.Empty;

    [Column("identity_hash")]
    [StringLength(64)]
    public string IdentityHash { get; set; } = string.Empty;

    [Column("is_service")]
    public bool IsService { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    public Product Product { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
