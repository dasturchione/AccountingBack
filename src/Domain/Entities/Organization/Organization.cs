using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_organization")]
public partial class Organization
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(500)]
    public string FullName { get; set; } = null!;

    [Column("inn")]
    [StringLength(20)]
    public string Inn { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("region_id")]
    public int RegionId { get; set; }

    [Column("district_id")]
    public int? DistrictId { get; set; }

    [Column("address")]
    [StringLength(1000)]
    public string? Address { get; set; }

    [Column("director")]
    [StringLength(250)]
    public string? Director { get; set; }

    [Column("is_parent")]
    public bool IsParent { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("default_language_id")]
    public short? DefaultLanguageId { get; set; }

    [Column("tenant_id")]
    public int? TenantId { get; set; }

    [Column("setup_status")]
    [StringLength(30)]
    public string SetupStatus { get; set; } = null!;

    [Column("setup_completed_at", TypeName = "timestamp without time zone")]
    public DateTime? SetupCompletedAt { get; set; }

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("website")]
    [StringLength(250)]
    public string? Website { get; set; }

    [Column("oked")]
    [StringLength(20)]
    public string? Oked { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();

    [InverseProperty(nameof(DocumentAccountSetting.Organization))]
    public virtual ICollection<DocumentAccountSetting> DocumentAccountSettings { get; set; } = new List<DocumentAccountSetting>();

    [InverseProperty(nameof(SaleShipmentDoc.Organization))]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [InverseProperty(nameof(OpeningBalance.Organization))]
    public virtual OpeningBalance? OpeningBalance { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<ChartAccount> ChartAccounts { get; set; } = new List<ChartAccount>();

    [InverseProperty(nameof(WarehouseProductMovement.Organization))]
    public virtual ICollection<WarehouseProductMovement> WarehouseProductMovements { get; set; } = new List<WarehouseProductMovement>();

    [InverseProperty(nameof(WarehouseProductBatch.Organization))]
    public virtual ICollection<WarehouseProductBatch> WarehouseProductBatches { get; set; } = new List<WarehouseProductBatch>();

    [InverseProperty("Organization")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Organization")]
    public virtual ICollection<PricingCondition> PricingConditions { get; set; } = new List<PricingCondition>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();

    [ForeignKey("DefaultLanguageId")]
    [InverseProperty("Organizations")]
    public virtual Language? DefaultLanguage { get; set; }

    [ForeignKey("DistrictId")]
    [InverseProperty("Organizations")]
    public virtual District? District { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();

    [InverseProperty("Organization")]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty("Organization")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("Organization")]
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    [InverseProperty("Organization")]
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty("Organization")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    [InverseProperty("Organization")]
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();

    [InverseProperty("Organization")]
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();

    [ForeignKey("RegionId")]
    [InverseProperty("Organizations")]
    public virtual Region Region { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty("Organization")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("Organizations")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [InverseProperty("Organization")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("Organization")]
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    
    [InverseProperty("Organization")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("Organization")]
    public virtual ICollection<CurrencyRevaluation> CurrencyRevaluations { get; set; } = new List<CurrencyRevaluation>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaGroup> FaGroups { get; set; } = new List<FaGroup>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("Organization")]
    public virtual OrganizationConfig? OrganizationConfig { get; set; }
}
