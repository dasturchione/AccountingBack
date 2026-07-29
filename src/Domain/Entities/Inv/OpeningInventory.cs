using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_opening_inventory")]
public partial class OpeningInventory
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

    [Column("contract_id")]
    public long? ContractId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal TotalAmount { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty(nameof(User.OpeningInventoryCancelledByUsers))]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty(nameof(Contract.OpeningInventories))]
    public virtual Contract? Contract { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty(nameof(CounterpartyCard.OpeningInventories))]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [InverseProperty(nameof(OpeningInventoryProduct.Owner))]
    public virtual ICollection<OpeningInventoryProduct> OpeningInventoryProducts { get; set; } = new List<OpeningInventoryProduct>();

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.OpeningInventories))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty(nameof(User.OpeningInventoryPostedByUsers))]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.OpeningInventories))]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty(nameof(DocumentStatus.OpeningInventories))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty(nameof(Warehouse.OpeningInventories))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
