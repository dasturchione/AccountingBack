using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public virtual DbSet<AccountType> AccountTypes { get; set; }
    public virtual DbSet<ChartAccount> ChartAccounts { get; set; }
    public virtual DbSet<ChartAccountSubkonto> ChartAccountSubkontos { get; set; }
    public virtual DbSet<AccountingPolicy> AccountingPolicies { get; set; }
    public virtual DbSet<AccountingPeriod> AccountingPeriods { get; set; }
    public virtual DbSet<PaymentPurpose> PaymentPurposes { get; set; }
    public virtual DbSet<PaymentPurposeTranslation> PaymentPurposeTranslations { get; set; }
    public virtual DbSet<PostingAlias> PostingAliases { get; set; }
    public virtual DbSet<PostingAliasTranslation> PostingAliasTranslations { get; set; }
    public virtual DbSet<PostingBatch> PostingBatches { get; set; }
    public virtual DbSet<PostingRule> PostingRules { get; set; }
    public virtual DbSet<PostingRuleLine> PostingRuleLines { get; set; }
    public virtual DbSet<SubkontoType> SubkontoTypes { get; set; }
    public virtual DbSet<BankAccount> BankAccounts { get; set; }
    public virtual DbSet<BankOperation> BankOperations { get; set; }
    public virtual DbSet<CashBox> CashBoxes { get; set; }
    public virtual DbSet<CashOperation> CashOperations { get; set; }
    public virtual DbSet<Bank> Banks { get; set; }
    public virtual DbSet<CounterpartyType> CounterpartyTypes { get; set; }
    public virtual DbSet<CostingMethod> CostingMethods { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<District> Districts { get; set; }
    public virtual DbSet<DocumentSequence> DocumentSequences { get; set; }
    public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<Language> Languages { get; set; }
    public virtual DbSet<OperationType> OperationTypes { get; set; }
    public virtual DbSet<PaymentType> PaymentTypes { get; set; }
    public virtual DbSet<PriceRoundingMethod> PriceRoundingMethods { get; set; }
    public virtual DbSet<PricingCondition> PricingConditions { get; set; }
    public virtual DbSet<PricingMethod> PricingMethods { get; set; }
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
    public virtual DbSet<OrganizationClaimRequest> OrganizationClaimRequests { get; set; }
    public virtual DbSet<OrganizationDefault> OrganizationDefaults { get; set; }
    public virtual DbSet<OrganizationSetupState> OrganizationSetupStates { get; set; }
    public virtual DbSet<OrganizationTaxSetting> OrganizationTaxSettings { get; set; }
    public virtual DbSet<OrganizationUserInvitation> OrganizationUserInvitations { get; set; }
    public virtual DbSet<PlatformTenant> PlatformTenants { get; set; }
    public virtual DbSet<PurchaseDoc> PurchaseDocs { get; set; }
    public virtual DbSet<PurchaseDocProduct> PurchaseDocProducts { get; set; }
    public virtual DbSet<PurchaseDocTable> PurchaseDocTables { get; set; }
    public virtual DbSet<AccountingRegisterEntry> AccountingRegisterEntries { get; set; }
    public virtual DbSet<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; }
    public virtual DbSet<MoneyRegisterBalance> MoneyRegisterBalances { get; set; }
    public virtual DbSet<RegisterBalance> RegisterBalances { get; set; }
    public virtual DbSet<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; }
    public virtual DbSet<SaleCondition> SaleConditions { get; set; }
    public virtual DbSet<SaleDoc> SaleDocs { get; set; }
    public virtual DbSet<SaleDocProduct> SaleDocProducts { get; set; }
    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleSubGroup> ModuleSubGroups { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleModule> RoleModules { get; set; }
    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserOrganization> UserOrganizations { get; set; }
    public virtual DbSet<Contract> Contracts { get; set; }
    public virtual DbSet<ContractType> ContractTypes { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<OrganizationConfig> OrganizationConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationConfig>()
            .HasOne(x => x.Organization)
            .WithOne(x => x.OrganizationConfig)
            .HasForeignKey<OrganizationConfig>(x => x.OrganizationId);

        modelBuilder.Entity<PricingCondition>(entity =>
        {
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(x => x.RoundingPrecision).HasDefaultValue(1m);
            entity.Property(x => x.StartDate).HasDefaultValueSql("now()");

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.PricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_organization_id_fkey");

            entity.HasOne(x => x.PricingMethod)
                .WithMany(x => x.PricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_pricing_method_id_fkey");

            entity.HasOne(x => x.RoundingMethod)
                .WithMany(x => x.PricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_rounding_method_id_fkey");

            entity.HasOne(x => x.State)
                .WithMany(x => x.PricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_state_id_fkey");
        });

        modelBuilder.Entity<PricingMethod>()
            .Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<SaleCondition>(entity =>
        {
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(x => x.StartDate).HasDefaultValueSql("now()");

            entity.HasOne(x => x.CostingMethod)
                .WithMany(x => x.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_costing_method_id_fkey");

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_organization_id_fkey");

            entity.HasOne(x => x.State)
                .WithMany(x => x.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_state_id_fkey");

            entity.HasOne(x => x.VatRate)
                .WithMany(x => x.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_vat_rate_id_fkey");
        });

        ApplyOrganizationFilters(modelBuilder);
    }
}
