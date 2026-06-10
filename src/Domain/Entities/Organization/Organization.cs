namespace Domain.Entities;

public partial class Organization
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public bool IsParent { get; set; }
    public short StateId { get; set; }
    public short? DefaultLanguageId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Region Region { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual Language? DefaultLanguage { get; set; }
    public virtual ICollection<ChartAccount> ChartAccounts { get; set; } = new List<ChartAccount>();
    public virtual ICollection<ChartAccountSubkonto> ChartAccountSubkontos { get; set; } = new List<ChartAccountSubkonto>();
    public virtual ICollection<PostingRule> PostingRules { get; set; } = new List<PostingRule>();
    public virtual ICollection<AccountingRegisterEntry> AccountingRegisterEntries { get; set; } = new List<AccountingRegisterEntry>();
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();
    public virtual ICollection<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; } = new List<CounterpartyRegisterBalance>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public virtual ICollection<InventoryRegisterBalance> InventoryRegisterBalances { get; set; } = new List<InventoryRegisterBalance>();
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
