using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_document_status")]
[Index("Code", Name = "idx_cmn_document_status_code", IsUnique = true)]
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

    [InverseProperty("Status")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Status")]
    public virtual ICollection<InventoryAdjustmentDoc> InventoryAdjustmentDocs { get; set; } = new List<InventoryAdjustmentDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<InventoryCountDoc> InventoryCountDocs { get; set; } = new List<InventoryCountDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<WarehouseTransferDoc> WarehouseTransferDocs { get; set; } = new List<WarehouseTransferDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("DocumentStatuses")]
    public virtual State State { get; set; } = null!;
}
