using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("counterparty_card")]
[Index("Code", Name = "idx_counterparty_card_code")]
[Index("DistrictId", Name = "idx_counterparty_card_district_id")]
[Index("ExternalId", Name = "idx_counterparty_card_external_id")]
[Index("Inn", Name = "idx_counterparty_card_inn")]
[Index("OrganizationId", Name = "idx_counterparty_card_organization_id")]
[Index("RegionId", Name = "idx_counterparty_card_region_id")]
[Index("ShortName", Name = "idx_counterparty_card_short_name")]
[Index("StateId", Name = "idx_counterparty_card_state_id")]
[Index("CounterpartyTypeId", Name = "idx_counterparty_card_type_id")]
public partial class CounterpartyCard
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_type_id")]
    public short CounterpartyTypeId { get; set; }

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

    [Column("is_customer")]
    public bool IsCustomer { get; set; }

    [Column("is_supplier")]
    public bool IsSupplier { get; set; }

    [Column("is_vat_payer")]
    public bool IsVatPayer { get; set; }

    [Column("oked")]
    [StringLength(20)]
    public string? Oked { get; set; }

    [Column("external_id")]
    [StringLength(100)]
    public string? ExternalId { get; set; }

    [InverseProperty("Counterparty")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<CmnContract> CmnContracts { get; set; } = new List<CmnContract>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    [ForeignKey("CounterpartyTypeId")]
    [InverseProperty("CounterpartyCards")]
    public virtual CmnCounterpartyType CounterpartyType { get; set; } = null!;

    [ForeignKey("DistrictId")]
    [InverseProperty("CounterpartyCards")]
    public virtual CmnDistrict? District { get; set; }

    [InverseProperty("Counterparty")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("CounterpartyCards")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Counterparty")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [ForeignKey("RegionId")]
    [InverseProperty("CounterpartyCards")]
    public virtual CmnRegion? Region { get; set; }

    [InverseProperty("Counterparty")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("Counterparty")]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("CounterpartyCards")]
    public virtual CmnState State { get; set; } = null!;
}
