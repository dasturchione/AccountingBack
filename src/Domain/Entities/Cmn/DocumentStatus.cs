using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_document_status")]
public partial class DocumentStatus
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("Status")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(SaleShipmentDoc.Status))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(DocumentStatusTranslation.Status))]
    public virtual ICollection<DocumentStatusTranslation> DocumentStatusTranslations { get; set; } = new List<DocumentStatusTranslation>();

    [InverseProperty(nameof(OpeningInventory.Status))]
    public virtual ICollection<OpeningInventory> OpeningInventories { get; set; } = new List<OpeningInventory>();

    [InverseProperty("Status")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Status")]
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("DocumentStatuses")]
    public virtual State State { get; set; } = null!;
}
