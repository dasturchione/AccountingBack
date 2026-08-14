using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_provider_product_mapping")]
[Index("OrganizationId", "IdentityHash", Name = "ux_edo_provider_product_mapping_identity", IsUnique = true)]
[Index("OrganizationId", "ProductId", Name = "idx_edo_provider_product_mapping_product")]
[Index("OrganizationId", "ProviderCode", "CatalogCode", "PackageCode", "ProviderProductNameHash", "IsService",
    Name = "ux_edo_provider_product_mapping_natural_identity", IsUnique = true)]
public partial class EdoProviderProductMapping
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    [Column("organization_id")]
    public int OrganizationId { get; set; }
    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = null!;
    [Column("catalog_code")]
    [StringLength(50)]
    public string CatalogCode { get; set; } = null!;
    [Column("package_code")]
    [StringLength(50)]
    public string PackageCode { get; set; } = null!;
    [Column("provider_product_name")]
    [StringLength(500)]
    public string ProviderProductName { get; set; } = null!;
    [Column("provider_product_name_hash")]
    [StringLength(64)]
    public string ProviderProductNameHash { get; set; } = null!;
    [Column("identity_hash")]
    [StringLength(64)]
    public string IdentityHash { get; set; } = null!;
    [Column("is_service")]
    public bool IsService { get; set; }
    [Column("product_id")]
    public int ProductId { get; set; }
    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }
}
