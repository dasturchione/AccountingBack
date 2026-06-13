using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public virtual DbSet<AccountType> AccountTypes { get; set; }
    public virtual DbSet<ChartAccount> ChartAccounts { get; set; }
    public virtual DbSet<ChartAccountSubkonto> ChartAccountSubkontos { get; set; }
    public virtual DbSet<PostingRule> PostingRules { get; set; }
    public virtual DbSet<PostingRuleLine> PostingRuleLines { get; set; }
    public virtual DbSet<SubkontoType> SubkontoTypes { get; set; }
    public virtual DbSet<BankAccount> BankAccounts { get; set; }
    public virtual DbSet<BankOperation> BankOperations { get; set; }
    public virtual DbSet<CashBox> CashBoxes { get; set; }
    public virtual DbSet<CashOperation> CashOperations { get; set; }
    public virtual DbSet<Bank> Banks { get; set; }
    public virtual DbSet<CounterpartyType> CounterpartyTypes { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<District> Districts { get; set; }
    public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<Language> Languages { get; set; }
    public virtual DbSet<OperationType> OperationTypes { get; set; }
    public virtual DbSet<PaymentType> PaymentTypes { get; set; }
    public virtual DbSet<Region> Regions { get; set; }
    public virtual DbSet<State> States { get; set; }
    public virtual DbSet<TaxType> TaxTypes { get; set; }
    public virtual DbSet<Translation> Translations { get; set; }
    public virtual DbSet<Unit> Units { get; set; }
    public virtual DbSet<VatRate> VatRates { get; set; }
    public virtual DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }
    public virtual DbSet<CounterpartyCard> CounterpartyCards { get; set; }
    public virtual DbSet<CounterpartyContact> CounterpartyContacts { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductGroup> ProductGroups { get; set; }
    public virtual DbSet<ProductPrice> ProductPrices { get; set; }
    public virtual DbSet<ProductTable> ProductTables { get; set; }
    public virtual DbSet<Warehouse> Warehouses { get; set; }
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Position> Positions { get; set; }
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<PurchaseDoc> PurchaseDocs { get; set; }
    public virtual DbSet<PurchaseDocTable> PurchaseDocTables { get; set; }
    public virtual DbSet<AccountingRegisterEntry> AccountingRegisterEntries { get; set; }
    public virtual DbSet<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; }
    public virtual DbSet<MoneyRegisterBalance> MoneyRegisterBalances { get; set; }
    public virtual DbSet<RegisterBalance> RegisterBalances { get; set; }
    public virtual DbSet<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; }
    public virtual DbSet<SaleDoc> SaleDocs { get; set; }
    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleSubGroup> ModuleSubGroups { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleModule> RoleModules { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserOrganization> UserOrganizations { get; set; }
}
