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
    public virtual DbSet<InventoryAdjustmentDoc> InventoryAdjustmentDocs { get; set; }
    public virtual DbSet<InventoryAdjustmentLine> InventoryAdjustmentLines { get; set; }
    public virtual DbSet<InventoryAdjustmentDocTable> InventoryAdjustmentDocTables { get; set; }
    public virtual DbSet<InventoryCountDoc> InventoryCountDocs { get; set; }
    public virtual DbSet<InventoryCountLine> InventoryCountLines { get; set; }
    public virtual DbSet<InventoryCountDocTable> InventoryCountDocTables { get; set; }
    public virtual DbSet<WarehouseTransferDoc> WarehouseTransferDocs { get; set; }
    public virtual DbSet<WarehouseTransferLine> WarehouseTransferLines { get; set; }
    public virtual DbSet<WarehouseTransferDocTable> WarehouseTransferDocTables { get; set; }
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
        modelBuilder.Entity<PostingBatch>()
            .HasIndex(x => new { x.DocumentTypeId, x.DocumentId })
            .HasDatabaseName("ux_acc_posting_batch_document_posted")
            .IsUnique()
            .HasFilter("status = 'POSTED'");

        modelBuilder.Entity<PostingBatch>()
            .HasIndex(x => new { x.DocumentTypeId, x.DocumentId })
            .HasDatabaseName("ux_acc_posting_batch_document_reversal")
            .IsUnique()
            .HasFilter("status = 'REVERSAL'");

        modelBuilder.Entity<ProductTable>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        // Optimistic concurrency for mutable Accounting Core entities via PostgreSQL's
        // system "xmin" column (same pattern as ProductTable — no schema change required).
        modelBuilder.Entity<ChartAccount>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        modelBuilder.Entity<AccountingPeriod>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        modelBuilder.Entity<PostingBatch>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        modelBuilder.Entity<ProductTable>()
            .HasIndex(x => x.MarkingNumber)
            .HasDatabaseName("ux_inv_product_table_marking_number_active")
            .IsUnique()
            .HasFilter("state_id = 1 AND marking_number IS NOT NULL");

        modelBuilder.Entity<ProductTable>()
            .HasIndex(x => x.CurrentWarehouseId)
            .HasDatabaseName("idx_inv_product_table_current_warehouse_id");

        modelBuilder.Entity<ProductTable>()
            .HasIndex(x => new { x.OrganizationId, x.CurrentWarehouseId, x.StatusId })
            .HasDatabaseName("idx_inv_product_table_org_warehouse_status");

        modelBuilder.Entity<ProductTable>()
            .HasIndex(x => new { x.OrganizationId, x.CurrentWarehouseId, x.StatusId, x.ProductId })
            .HasDatabaseName("idx_inv_product_table_org_warehouse_status_product");

        modelBuilder.Entity<ProductTable>()
            .HasIndex(x => x.SerialNumber)
            .HasDatabaseName("ux_inv_product_table_serial_number_active")
            .IsUnique()
            .HasFilter("state_id = 1 AND serial_number IS NOT NULL");

        modelBuilder.Entity<ProductTable>()
            .ToTable(t => t.HasCheckConstraint(
                "chk_inv_product_table_active_stock_warehouse",
                "status_id <> 1 OR current_warehouse_id IS NOT NULL"));

        modelBuilder.Entity<SaleDocTable>()
            .HasIndex(x => new { x.OwnerId, x.ProductTableId })
            .HasDatabaseName("ux_sale_doc_table_owner_product_table")
            .IsUnique();

        modelBuilder.Entity<WarehouseTransferDoc>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_transfer_doc_source_destination_diff",
                "source_warehouse_id <> destination_warehouse_id"));

        modelBuilder.Entity<WarehouseTransferDoc>()
            .HasIndex(x => new { x.OrganizationId, x.DocNumber })
            .HasDatabaseName("ux_inv_transfer_doc_doc_number_org")
            .IsUnique();

        modelBuilder.Entity<WarehouseTransferLine>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_transfer_line_quantity_positive",
                "quantity > 0"));

        modelBuilder.Entity<WarehouseTransferDocTable>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_transfer_doc_table_source_destination_diff",
                "source_warehouse_id <> destination_warehouse_id"));

        modelBuilder.Entity<InventoryAdjustmentDoc>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_inventory_adjustment_doc_adjustment_type",
                "adjustment_type in ('POSITIVE_ADJUSTMENT','NEGATIVE_ADJUSTMENT','WRITE_OFF','DAMAGE','LOSS','FOUND_STOCK','CORRECTION')"));

        modelBuilder.Entity<InventoryAdjustmentDoc>()
            .HasIndex(x => new { x.OrganizationId, x.DocNumber })
            .HasDatabaseName("ux_inv_inventory_adjustment_doc_org_doc_number")
            .IsUnique();

        modelBuilder.Entity<InventoryAdjustmentLine>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_inventory_adjustment_line_quantity_positive",
                "quantity > 0"));

        modelBuilder.Entity<InventoryCountLine>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("ck_inv_inventory_count_line_counted_quantity_positive", "counted_quantity > 0");
                t.HasCheckConstraint("ck_inv_inventory_count_line_default_cost_price_nonnegative", "default_cost_price >= 0");
            });

        modelBuilder.Entity<InventoryCountDocTable>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_inv_inventory_count_doc_table_cost_price_nonnegative",
                "cost_price >= 0"));

        modelBuilder.Entity<InventoryCountDocTable>()
            .HasIndex(x => new { x.OwnerId, x.ProductTableId })
            .HasDatabaseName("ux_inv_inventory_count_doc_table_owner_product_table")
            .IsUnique()
            .HasFilter("product_table_id IS NOT NULL");

        modelBuilder.Entity<InventoryCountDoc>()
            .HasIndex(x => new { x.OrganizationId, x.DocNumber })
            .HasDatabaseName("ux_inv_inventory_count_doc_org_doc_number")
            .IsUnique();

        modelBuilder.Entity<InventoryCountDoc>()
            .HasIndex(x => new { x.OrganizationId, x.WarehouseId })
            .HasDatabaseName("ux_inv_inventory_count_doc_active_warehouse")
            .IsUnique()
            .HasFilter("state_id = 1 AND status_id IN (1, 4)");

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
