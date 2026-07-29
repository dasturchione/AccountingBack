using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_contract")]
public partial class Contract
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("contract_type_id")]
    public short ContractTypeId { get; set; }

    [Column("contract_number")]
    [StringLength(100)]
    public string ContractNumber { get; set; } = null!;

    [Column("contract_date", TypeName = "timestamp without time zone")]
    public DateTime ContractDate { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime? StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("Contracts")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("ContractTypeId")]
    [InverseProperty("Contracts")]
    public virtual ContractType ContractType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("Contracts")]
    public virtual Organization Organization { get; set; } = null!;

    [InverseProperty("Contract")]
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty("Contract")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("Contract")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(OpeningInventory.Contract))]
    public virtual ICollection<OpeningInventory> OpeningInventories { get; set; } = new List<OpeningInventory>();

    [ForeignKey("StateId")]
    [InverseProperty("Contracts")]
    public virtual State State { get; set; } = null!;
}
