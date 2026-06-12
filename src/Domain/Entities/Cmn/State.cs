namespace Domain.Entities;

public partial class State
{
    public short Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<AccountType> AccountTypes { get; set; } = new List<AccountType>();
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();
    public virtual ICollection<ChartAccount> ChartAccounts { get; set; } = new List<ChartAccount>();
    public virtual ICollection<PostingRuleLine> PostingRuleLines { get; set; } = new List<PostingRuleLine>();
    public virtual ICollection<PostingRule> PostingRules { get; set; } = new List<PostingRule>();
    public virtual ICollection<SubkontoType> SubkontoTypes { get; set; } = new List<SubkontoType>();
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();
    public virtual ICollection<Bank> Banks { get; set; } = new List<Bank>();
    public virtual ICollection<CounterpartyType> CounterpartyTypes { get; set; } = new List<CounterpartyType>();
    public virtual ICollection<Currency> Currencies { get; set; } = new List<Currency>();
    public virtual ICollection<DocumentStatus> DocumentStatuses { get; set; } = new List<DocumentStatus>();
    public virtual ICollection<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();
    public virtual ICollection<OperationType> OperationTypes { get; set; } = new List<OperationType>();
    public virtual ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();
    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    public virtual ICollection<TaxType> TaxTypes { get; set; } = new List<TaxType>();
    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
    public virtual ICollection<VatRate> VatRates { get; set; } = new List<VatRate>();
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();
    public virtual ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
    public virtual ICollection<ProductTable> ProductTables { get; set; } = new List<ProductTable>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();
    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
