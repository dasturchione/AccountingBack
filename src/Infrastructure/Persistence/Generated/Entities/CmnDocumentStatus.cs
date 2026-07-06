using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_document_status")]
[Index("Code", Name = "idx_cmn_document_status_code", IsUnique = true)]
public partial class CmnDocumentStatus
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
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; } = new List<InvInventoryAdjustmentDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocs { get; set; } = new List<InvInventoryCountDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<InvTransferDoc> InvTransferDocs { get; set; } = new List<InvTransferDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("Status")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnDocumentStatuses")]
    public virtual CmnState State { get; set; } = null!;
}
