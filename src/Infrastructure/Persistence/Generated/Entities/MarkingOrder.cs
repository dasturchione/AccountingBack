using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_order")]
[Index("BusinessPlaceId", Name = "idx_marking_order_business_place_id")]
[Index("OrganizationId", Name = "idx_marking_order_organization_id")]
[Index("ProductId", Name = "idx_marking_order_product_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_order_status")]
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

    [ForeignKey("BusinessPlaceId")]
    [InverseProperty("MarkingOrders")]
    public virtual MarkingBusinessPlace BusinessPlace { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<MarkingCode> MarkingCodes { get; set; } = new List<MarkingCode>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingOrders")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("MarkingOrders")]
    public virtual InvProduct Product { get; set; } = null!;
}
