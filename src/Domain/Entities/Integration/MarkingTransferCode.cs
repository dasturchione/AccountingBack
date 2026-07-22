using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("marking_transfer_code")]
[Index(nameof(OrganizationId), Name = "idx_marking_transfer_code_organization_id")]
[Index(nameof(MarkingTransferId), Name = "idx_marking_transfer_code_marking_transfer_id")]
[Index(nameof(Gtin), Name = "idx_marking_transfer_code_gtin")]
[Index(nameof(MarkingTransferId), nameof(MarkingCode), Name = "uidx_marking_transfer_code_transfer_code", IsUnique = true)]
public partial class MarkingTransferCode
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("marking_transfer_id")]
    public long MarkingTransferId { get; set; }

    [Column("marking_code")]
    [StringLength(1000)]
    public string MarkingCode { get; set; } = null!;

    [Column("gtin")]
    [StringLength(14)]
    public string? Gtin { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(MarkingTransferId))]
    [InverseProperty(nameof(MarkingTransfer.MarkingTransferCodes))]
    public virtual MarkingTransfer MarkingTransfer { get; set; } = null!;
}
