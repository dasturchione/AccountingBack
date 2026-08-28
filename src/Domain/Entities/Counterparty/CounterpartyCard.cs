using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("counterparty_card")]
public partial class CounterpartyCard
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(500)]
    public string? FullName { get; set; }

    [Column("inn")]
    [StringLength(20)]
    public string? Inn { get; set; }

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("email")]
    [StringLength(250)]
    public string? Email { get; set; }

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("district_id")]
    public int? DistrictId { get; set; }

    [Column("address")]
    [StringLength(1000)]
    public string? Address { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("is_vat_payer")]
    public bool IsVatPayer { get; set; }

    [Column("oked")]
    [StringLength(20)]
    public string? Oked { get; set; }

    [Column("external_id")]
    [StringLength(100)]
    public string? ExternalId { get; set; }

    [Column("crpt_participant_id")]
    public int? CrptParticipantId { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.CounterpartyCards))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(DistrictId))]
    [InverseProperty(nameof(District.CounterpartyCards))]
    public virtual District? District { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.CounterpartyCards))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(RegionId))]
    [InverseProperty(nameof(Region.CounterpartyCards))]
    public virtual Region? Region { get; set; }

    [InverseProperty(nameof(BankOperation.Counterparty))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(CashOperation.Counterparty))]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty(nameof(CounterpartyBankAccount.Counterparty))]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty(nameof(CounterpartyContact.Counterparty))]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty(nameof(CounterpartyRegisterBalance.Counterparty))]
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();

    [InverseProperty(nameof(SaleShipmentDoc.Counterparty))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(OpeningInventory.Counterparty))]
    public virtual ICollection<OpeningInventory> OpeningInventories { get; set; } = new List<OpeningInventory>();

    [InverseProperty(nameof(PurchaseDoc.Counterparty))]
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty(nameof(SaleDoc.Counterparty))]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty(nameof(RetailSaleDoc.Counterparty))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocs { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(Contract.Counterparty))]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

}
