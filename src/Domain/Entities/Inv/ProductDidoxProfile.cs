using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_didox_profile")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_inv_product_didox_profile_effective_dates")]
[Index("OrganizationId", Name = "idx_inv_product_didox_profile_organization_id")]
[Index("ProductId", Name = "idx_inv_product_didox_profile_product_id")]
[Index("StateId", Name = "idx_inv_product_didox_profile_state_id")]
[Index("ProductId", "EffectiveFrom", Name = "ux_inv_product_didox_profile_product_effective_from", IsUnique = true)]
public partial class ProductDidoxProfile
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("default_origin_code")]
    public short DefaultOriginCode { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(DefaultOriginCode))]
    public virtual DidoxOrigin DefaultOrigin { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.ProductDidoxProfiles))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    [InverseProperty(nameof(Product.DidoxProfiles))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;
}
