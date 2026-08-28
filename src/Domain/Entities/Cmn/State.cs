using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_state")]
public partial class State
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(250)]
    public string FullName { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty(nameof(AccountType.State))]
    public virtual ICollection<AccountType> AccountTypes { get; set; } = new List<AccountType>();

    [InverseProperty(nameof(RetailSaleDoc.State))]
    public virtual ICollection<RetailSaleDoc> RetailSaleDocs { get; set; } = new List<RetailSaleDoc>();

    [InverseProperty(nameof(ChartAccountSubkonto.State))]
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();

    [InverseProperty(nameof(FiscalCashRegister.State))]
    public virtual ICollection<FiscalCashRegister> FiscalCashRegisters { get; set; } = new List<FiscalCashRegister>();

    [InverseProperty(nameof(DocumentAccountRole.State))]
    public virtual ICollection<DocumentAccountRole> DocumentAccountRoles { get; set; } = new List<DocumentAccountRole>();

    [InverseProperty(nameof(PaymentAcceptancePoint.State))]
    public virtual ICollection<PaymentAcceptancePoint> PaymentAcceptancePoints { get; set; } = [];

    [InverseProperty(nameof(PaymentAcceptancePointType.State))]
    public virtual ICollection<PaymentAcceptancePointType> PaymentAcceptancePointTypes { get; set; } = [];

    [InverseProperty(nameof(PaymentAcceptancePointOperation.State))]
    public virtual ICollection<PaymentAcceptancePointOperation> PaymentAcceptancePointOperations { get; set; } = [];

    [InverseProperty(nameof(DocumentAccountSetting.State))]
    public virtual ICollection<DocumentAccountSetting> DocumentAccountSettings { get; set; } = new List<DocumentAccountSetting>();

    [InverseProperty(nameof(OpeningInventory.State))]
    public virtual ICollection<OpeningInventory> OpeningInventories { get; set; } = new List<OpeningInventory>();

    [InverseProperty(nameof(OpeningBalance.State))]
    public virtual ICollection<OpeningBalance> OpeningBalances { get; set; } = new List<OpeningBalance>();

    [InverseProperty(nameof(DocumentAccountType.State))]
    public virtual ICollection<DocumentAccountType> DocumentAccountTypes { get; set; } = new List<DocumentAccountType>();

    [InverseProperty("State")]
    public virtual ICollection<ChartAccount> ChartAccounts { get; set; } = new List<ChartAccount>();

    [InverseProperty("State")]
    public virtual ICollection<SubkontoType> SubkontoTypes { get; set; } = new List<SubkontoType>();

    [InverseProperty("State")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(ChartAccountPreset.State))]
    public virtual ICollection<ChartAccountPreset> ChartAccountPresets { get; set; } = new List<ChartAccountPreset>();

    [InverseProperty(nameof(ChartAccountPresetAccount.State))]
    public virtual ICollection<ChartAccountPresetAccount> ChartAccountPresetAccounts { get; set; } = new List<ChartAccountPresetAccount>();

    [InverseProperty("State")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("State")]
    public virtual ICollection<AccountingPolicy> AccountingPolicies { get; set; } = new List<AccountingPolicy>();

    [InverseProperty("State")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("State")]
    public virtual ICollection<Bank> Banks { get; set; } = new List<Bank>();

    [InverseProperty(nameof(BankBranch.State))]
    public virtual ICollection<BankBranch> BankBranches { get; set; } = new List<BankBranch>();

    [InverseProperty(nameof(BankStatementTemplate.State))]
    public virtual ICollection<BankStatementTemplate> BankStatementTemplates { get; set; } = new List<BankStatementTemplate>();

    [InverseProperty(nameof(BankOperationCategory.State))]
    public virtual ICollection<BankOperationCategory> BankOperationCategories { get; set; } = [];

    [InverseProperty(nameof(BankOperationClassificationRuleSet.State))]
    public virtual ICollection<BankOperationClassificationRuleSet> BankOperationClassificationRuleSets { get; set; } = [];

    [InverseProperty(nameof(BankOperationClassificationRule.State))]
    public virtual ICollection<BankOperationClassificationRule> BankOperationClassificationRules { get; set; } = [];

    [InverseProperty("State")]
    public virtual ICollection<ProductTableStatus> ProductTableStatuses { get; set; } = new List<ProductTableStatus>();

    [InverseProperty("State")]
    public virtual ICollection<Currency> Currencies { get; set; } = new List<Currency>();

    [InverseProperty("State")]
    public virtual ICollection<DocumentStatus> DocumentStatuses { get; set; } = new List<DocumentStatus>();

    [InverseProperty("State")]
    public virtual ICollection<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();

    [InverseProperty(nameof(DocumentRegistry.State))]
    public virtual ICollection<DocumentRegistry> DocumentRegistries { get; set; } = new List<DocumentRegistry>();

    [InverseProperty("State")]
    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();

    [InverseProperty("State")]
    public virtual ICollection<OperationType> OperationTypes { get; set; } = new List<OperationType>();

    [InverseProperty("State")]
    public virtual ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();

    [InverseProperty("State")]
    public virtual ICollection<PricingCondition> PricingConditions { get; set; } = new List<PricingCondition>();

    [InverseProperty("State")]
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();

    [InverseProperty("State")]
    public virtual ICollection<TaxType> TaxTypes { get; set; } = new List<TaxType>();

    [InverseProperty("State")]
    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();

    [InverseProperty("State")]
    public virtual ICollection<VatRate> VatRates { get; set; } = new List<VatRate>();

    [InverseProperty("State")]
    public virtual ICollection<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();

    [InverseProperty("State")]
    public virtual ICollection<CurrencyRevaluation> CurrencyRevaluations { get; set; } = new List<CurrencyRevaluation>();

    [InverseProperty("State")]
    public virtual ICollection<CurrencyRevaluationLine> CurrencyRevaluationLines { get; set; } = new List<CurrencyRevaluationLine>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty("State")]
    public virtual ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();

    [InverseProperty("State")]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty("State")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("State")]
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    [InverseProperty("State")]
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    [InverseProperty("State")]
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    [InverseProperty("State")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    [InverseProperty("State")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    [InverseProperty("State")]
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();

    [InverseProperty("State")]
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();

    [InverseProperty("State")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty("State")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("State")]
    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    [InverseProperty("State")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [InverseProperty("State")]
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

    [InverseProperty("State")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    [InverseProperty("State")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("State")]
    public virtual ICollection<ContractType> ContractTypes { get; set; } = new List<ContractType>();

    [InverseProperty("State")]
    public virtual ICollection<FaDepreciationMethod> FaDepreciationMethods { get; set; } = new List<FaDepreciationMethod>();

    [InverseProperty("State")]
    public virtual ICollection<FaGroup> FaGroups { get; set; } = new List<FaGroup>();

    [InverseProperty("State")]
    public virtual ICollection<FaOkof> FaOkofs { get; set; } = new List<FaOkof>();

    [InverseProperty("State")]
    public virtual ICollection<FaAssetStatus> FaAssetStatuses { get; set; } = new List<FaAssetStatus>();

    [InverseProperty("State")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty(nameof(FaCommissioningDoc.State))]
    public virtual ICollection<FaCommissioningDoc> FaCommissioningDocs { get; set; } = new List<FaCommissioningDoc>();

    [InverseProperty(nameof(MxikCatalog.State))]
    public virtual ICollection<MxikCatalog> MxikCatalogs { get; set; } = new List<MxikCatalog>();
}
