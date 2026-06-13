using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sale_doc")]
[Index("CounterpartyId", Name = "idx_sale_doc_counterparty_id")]
[Index("DocDate", Name = "idx_sale_doc_doc_date")]
[Index("OrganizationId", Name = "idx_sale_doc_organization_id")]
[Index("StateId", Name = "idx_sale_doc_state_id")]
[Index("StatusId", Name = "idx_sale_doc_status_id")]
[Index("WarehouseId", Name = "idx_sale_doc_warehouse_id")]
public partial class SaleDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("total_amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Column("vat_amount")]
    [Precision(18, 2)]
    public decimal VatAmount { get; set; }

    [Column("final_amount")]
    [Precision(18, 2)]
    public decimal FinalAmount { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("SaleDocs")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("SaleDocs")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("SaleDocs")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("StateId")]
    [InverseProperty("SaleDocs")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("SaleDocs")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("SaleDocs")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
