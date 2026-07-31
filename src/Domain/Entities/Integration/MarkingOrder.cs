using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_order")]
[Index(nameof(OrganizationId), Name = "idx_marking_order_organization_id")]
[Index(nameof(BusinessPlaceId), Name = "idx_marking_order_business_place_id")]
[Index(nameof(ProductId), Name = "idx_marking_order_product_id")]
[Index(nameof(OrganizationId), nameof(Status), Name = "idx_marking_order_status")]
public partial class MarkingOrder
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("business_place_id")]
    public int BusinessPlaceId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("gtin")]
    [StringLength(14)]
    public string Gtin { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("crpt_order_id")]
    [StringLength(100)]
    public string? CrptOrderId { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(BusinessPlaceId))]
    public virtual MarkingBusinessPlace BusinessPlace { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}
