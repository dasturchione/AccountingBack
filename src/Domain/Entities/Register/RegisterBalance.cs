using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_reg_balance")]
[Index("DocDate", Name = "idx_inv_reg_balance_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_inv_reg_balance_document")]
[Index("OrganizationId", Name = "idx_inv_reg_balance_organization_id")]
[Index("ProductId", Name = "idx_inv_reg_balance_product_id")]
[Index("WarehouseId", Name = "idx_inv_reg_balance_warehouse_id")]
[Index("PostingBatchId", Name = "idx_inv_reg_balance_posting_batch_id")]
[Index("ReversalEntryId", Name = "idx_inv_reg_balance_reversal_entry_id")]
public partial class RegisterBalance
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }


    [Column("posting_batch_id")]
    public long? PostingBatchId { get; set; }

    [Column("source_line_id")]
    public long? SourceLineId { get; set; }

    [Column("reversal_entry_id")]
    public long? ReversalEntryId { get; set; }
    [ForeignKey("DocumentTypeId")]
    [InverseProperty("RegisterBalances")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("RegisterBalances")]
    public virtual OperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("RegisterBalances")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("RegisterBalances")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("RegisterBalances")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
