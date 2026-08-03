using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_type")]
public partial class FaReceiptType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(30)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty(nameof(FaReceiptDoc.ReceiptType))]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty(nameof(FaReceiptTypeTranslation.ReceiptType))]
    public virtual ICollection<FaReceiptTypeTranslation> FaReceiptTypeTranslations { get; set; } = new List<FaReceiptTypeTranslation>();
}
