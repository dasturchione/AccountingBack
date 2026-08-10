using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SharedKernel.Constants;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public virtual DbSet<AccountType> AccountTypes { get; set; }
    public virtual DbSet<ChartAccount> ChartAccounts { get; set; }
    public virtual DbSet<ChartAccountSubkonto> ChartAccountSubkontos { get; set; }
    public virtual DbSet<ChartAccountPreset> ChartAccountPresets { get; set; }
    public virtual DbSet<ChartAccountPresetAccount> ChartAccountPresetAccounts { get; set; }
    public virtual DbSet<ChartAccountPresetAccountSubkonto> ChartAccountPresetAccountSubkontos { get; set; }
    public virtual DbSet<ChartAccountPresetAccountTranslation> ChartAccountPresetAccountTranslations { get; set; }
    public virtual DbSet<ChartAccountPresetTranslation> ChartAccountPresetTranslations { get; set; }
    public virtual DbSet<AccountingPolicy> AccountingPolicies { get; set; }
    public virtual DbSet<AccountingPeriod> AccountingPeriods { get; set; }
    public virtual DbSet<PostingBatch> PostingBatches { get; set; }
    public virtual DbSet<SubkontoType> SubkontoTypes { get; set; }
    public virtual DbSet<SubkontoTypeTranslation> SubkontoTypeTranslations { get; set; }
    public virtual DbSet<AccountTypeTranslation> AccountTypeTranslations { get; set; }
    public virtual DbSet<OpeningBalance> OpeningBalances { get; set; }
    public virtual DbSet<OpeningBalanceAccount> OpeningBalanceAccounts { get; set; }
    public virtual DbSet<OpeningBalanceAccountDetail> OpeningBalanceAccountDetails { get; set; }
    public virtual DbSet<OpeningBalanceAccountDetailSubkonto> OpeningBalanceAccountDetailSubkontos { get; set; }
    public virtual DbSet<BankAccount> BankAccounts { get; set; }
    public virtual DbSet<BankTerminal> BankTerminals { get; set; }
    public virtual DbSet<BankOperation> BankOperations { get; set; }
    public virtual DbSet<CashBox> CashBoxes { get; set; }
    public virtual DbSet<CashOperation> CashOperations { get; set; }
    public virtual DbSet<Bank> Banks { get; set; }
    public virtual DbSet<CounterpartyType> CounterpartyTypes { get; set; }
    public virtual DbSet<CostingMethod> CostingMethods { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }
    public virtual DbSet<CurrencyRevaluation> CurrencyRevaluations { get; set; }
    public virtual DbSet<CurrencyRevaluationLine> CurrencyRevaluationLines { get; set; }
    public virtual DbSet<FaAssetStatus> FaAssetStatuses { get; set; }
    public virtual DbSet<FaDepreciationMethod> FaDepreciationMethods { get; set; }
    public virtual DbSet<FaAsset> FaAssets { get; set; }
    public virtual DbSet<FaReceiptDoc> FaReceiptDocs { get; set; }
    public virtual DbSet<FaReceiptDocLine> FaReceiptDocLines { get; set; }
    public virtual DbSet<FaReceiptDocAsset> FaReceiptDocAssets { get; set; }
    public virtual DbSet<FaMovementDoc> FaMovementDocs { get; set; }
    public virtual DbSet<FaMovementDocLine> FaMovementDocLines { get; set; }
    public virtual DbSet<FaDepreciationRun> FaDepreciationRuns { get; set; }
    public virtual DbSet<FaDepreciationRunLine> FaDepreciationRunLines { get; set; }
    public virtual DbSet<FaDisposalDoc> FaDisposalDocs { get; set; }
    public virtual DbSet<FaDisposalDocLine> FaDisposalDocLines { get; set; }
    public virtual DbSet<FaRevaluationDoc> FaRevaluationDocs { get; set; }
    public virtual DbSet<FaRevaluationDocLine> FaRevaluationDocLines { get; set; }
    public virtual DbSet<FaGroup> FaGroups { get; set; }
    public virtual DbSet<FaOkof> FaOkofs { get; set; }
    public virtual DbSet<District> Districts { get; set; }
    public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<DocumentAccountType> DocumentAccountTypes { get; set; }
    public virtual DbSet<DocumentAccountTypeTranslation> DocumentAccountTypeTranslations { get; set; }
    public virtual DbSet<DocumentAccountRole> DocumentAccountRoles { get; set; }
    public virtual DbSet<DocumentAccountRoleTranslation> DocumentAccountRoleTranslations { get; set; }
    public virtual DbSet<DocumentAccountTypeRole> DocumentAccountTypeRoles { get; set; }
    public virtual DbSet<DocumentAccountSetting> DocumentAccountSettings { get; set; }
    public virtual DbSet<Language> Languages { get; set; }
    public virtual DbSet<InventoryAdjustmentType> InventoryAdjustmentTypes { get; set; }
    public virtual DbSet<MxikCatalog> MxikCatalogs { get; set; }
    public virtual DbSet<OperationType> OperationTypes { get; set; }
    public virtual DbSet<PaymentType> PaymentTypes { get; set; }
    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
    public virtual DbSet<PriceRoundingMethod> PriceRoundingMethods { get; set; }
    public virtual DbSet<PricingCondition> PricingConditions { get; set; }
    public virtual DbSet<PricingMethod> PricingMethods { get; set; }
    public virtual DbSet<Region> Regions { get; set; }
    public virtual DbSet<State> States { get; set; }
    public virtual DbSet<TaxType> TaxTypes { get; set; }
    public virtual DbSet<Translation> Translations { get; set; }
    public virtual DbSet<ContractTypeTranslation> ContractTypeTranslations { get; set; }
    public virtual DbSet<CostingMethodTranslation> CostingMethodTranslations { get; set; }
    public virtual DbSet<CounterpartyTypeTranslation> CounterpartyTypeTranslations { get; set; }
    public virtual DbSet<CurrencyTranslation> CurrencyTranslations { get; set; }
    public virtual DbSet<DocumentStatusTranslation> DocumentStatusTranslations { get; set; }
    public virtual DbSet<DocumentTypeTranslation> DocumentTypeTranslations { get; set; }
    public virtual DbSet<OperationTypeTranslation> OperationTypeTranslations { get; set; }
    public virtual DbSet<PaymentTypeTranslation> PaymentTypeTranslations { get; set; }
    public virtual DbSet<PaymentMethodTranslation> PaymentMethodTranslations { get; set; }
    public virtual DbSet<Unit> Units { get; set; }
    public virtual DbSet<VatRate> VatRates { get; set; }
    public virtual DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }
    public virtual DbSet<CounterpartyCard> CounterpartyCards { get; set; }
    public virtual DbSet<CounterpartyContact> CounterpartyContacts { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductGroup> ProductGroups { get; set; }
    public virtual DbSet<ProductPrice> ProductPrices { get; set; }
    public virtual DbSet<ProductPriceType> ProductPriceTypes { get; set; }
    public virtual DbSet<ProductTable> ProductTables { get; set; }
    public virtual DbSet<ProductTableStatus> ProductTableStatuses { get; set; }
    public virtual DbSet<InventoryAdjustmentDoc> InventoryAdjustmentDocs { get; set; }
    public virtual DbSet<InventoryAdjustmentLine> InventoryAdjustmentLines { get; set; }
    public virtual DbSet<InventoryAdjustmentDocTable> InventoryAdjustmentDocTables { get; set; }
    public virtual DbSet<InventoryCountDoc> InventoryCountDocs { get; set; }
    public virtual DbSet<InventoryCountLine> InventoryCountLines { get; set; }
    public virtual DbSet<InventoryCountDocTable> InventoryCountDocTables { get; set; }
    public virtual DbSet<OpeningInventory> OpeningInventories { get; set; }
    public virtual DbSet<OpeningInventoryProduct> OpeningInventoryProducts { get; set; }
    public virtual DbSet<OpeningInventoryTable> OpeningInventoryTables { get; set; }
    public virtual DbSet<WarehouseTransferDoc> WarehouseTransferDocs { get; set; }
    public virtual DbSet<WarehouseTransferLine> WarehouseTransferLines { get; set; }
    public virtual DbSet<WarehouseTransferDocTable> WarehouseTransferDocTables { get; set; }
    public virtual DbSet<Warehouse> Warehouses { get; set; }
    public virtual DbSet<WarehouseProduct> WarehouseProducts { get; set; }
    public virtual DbSet<WarehouseProductTable> WarehouseProductTables { get; set; }
    public virtual DbSet<WarehouseProductMovement> WarehouseProductMovements { get; set; }
    public virtual DbSet<WarehouseProductBatch> WarehouseProductBatches { get; set; }
    public virtual DbSet<WarehouseProductBatchAllocation> WarehouseProductBatchAllocations { get; set; }
    public virtual DbSet<WarehouseProductBatchTable> WarehouseProductBatchTables { get; set; }
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Position> Positions { get; set; }
    public virtual DbSet<PayEmployee> PayEmployees { get; set; }
    public virtual DbSet<PayEmployment> PayEmployments { get; set; }
    public virtual DbSet<PayComponent> PayComponents { get; set; }
    public virtual DbSet<PayEmployeeComponent> PayEmployeeComponents { get; set; }
    public virtual DbSet<PayPeriod> PayPeriods { get; set; }
    public virtual DbSet<PayTimesheet> PayTimesheets { get; set; }
    public virtual DbSet<PayTimesheetLine> PayTimesheetLines { get; set; }
    public virtual DbSet<PayPayrollDoc> PayPayrollDocs { get; set; }
    public virtual DbSet<PayPayrollLine> PayPayrollLines { get; set; }
    public virtual DbSet<PayPayrollCalcLine> PayPayrollCalcLines { get; set; }
    public virtual DbSet<PayPaymentBatch> PayPaymentBatches { get; set; }
    public virtual DbSet<PayPaymentLine> PayPaymentLines { get; set; }
    public virtual DbSet<HrEmployeeWorkSchedule> HrEmployeeWorkSchedules { get; set; }
    public virtual DbSet<HrEmployeeWorkScheduleDay> HrEmployeeWorkScheduleDays { get; set; }
    public virtual DbSet<HrAbsenceType> HrAbsenceTypes { get; set; }
    public virtual DbSet<HrAbsence> HrAbsences { get; set; }
    public virtual DbSet<HrAbsenceAttachment> HrAbsenceAttachments { get; set; }
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<OrganizationEdoProvider> OrganizationEdoProviders { get; set; }
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
    public virtual DbSet<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; }
    public virtual DbSet<SaleCondition> SaleConditions { get; set; }
    public virtual DbSet<SaleDoc> SaleDocs { get; set; }
    public virtual DbSet<SaleDocProduct> SaleDocProducts { get; set; }
    public virtual DbSet<SaleDocProductBatch> SaleDocProductBatches { get; set; }
    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }
    public virtual DbSet<SaleShipmentDoc> SaleShipmentDocs { get; set; }
    public virtual DbSet<SaleShipmentProduct> SaleShipmentProducts { get; set; }
    public virtual DbSet<SaleShipmentProductBatch> SaleShipmentProductBatches { get; set; }
    public virtual DbSet<SaleShipmentTable> SaleShipmentTables { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleSubGroup> ModuleSubGroups { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleModule> RoleModules { get; set; }
    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<SystemSetting> SystemSettings { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserOrganization> UserOrganizations { get; set; }
    public virtual DbSet<Contract> Contracts { get; set; }
    public virtual DbSet<ContractType> ContractTypes { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<OrganizationConfig> OrganizationConfigs { get; set; }
    public virtual DbSet<NotificationType> NotificationTypes { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<NotificationRead> NotificationReads { get; set; }
    public virtual DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public virtual DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public virtual DbSet<MarkingBusinessPlace> MarkingBusinessPlaces { get; set; }
    public virtual DbSet<MarkingOrder> MarkingOrders { get; set; }
    public virtual DbSet<MarkingUtilization> MarkingUtilizations { get; set; }
    public virtual DbSet<MarkingCode> MarkingCodes { get; set; }
    public virtual DbSet<MarkingAggregation> MarkingAggregations { get; set; }
    public virtual DbSet<MarkingAslBelgiDocument> MarkingAslBelgiDocuments { get; set; }
    public virtual DbSet<MarkingEdocsDocument> MarkingEdocsDocuments { get; set; }
    public virtual DbSet<MarkingDidoxDocument> MarkingDidoxDocuments { get; set; }
    public virtual DbSet<IntegrationCredential> IntegrationCredentials { get; set; }
    public virtual DbSet<EdoDocument> EdoDocuments { get; set; }
    public virtual DbSet<EdoDocumentSigningSession> EdoDocumentSigningSessions { get; set; }
    public virtual DbSet<EdoAuthSigningSession> EdoAuthSigningSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_record_pkey");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("idempotency_record_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingBusinessPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_business_place_pkey");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_business_place_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_order_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_organization_id_fkey");

            entity.HasOne(e => e.BusinessPlace)
                .WithMany()
                .HasForeignKey(e => e.BusinessPlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_business_place_id_fkey");

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_product_id_fkey");

            entity.HasIndex(e => e.CrptOrderId)
                .HasDatabaseName("ux_marking_order_crpt_order_id")
                .IsUnique()
                .HasFilter("crpt_order_id IS NOT NULL");
        });

        modelBuilder.Entity<MarkingUtilization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_utilization_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_utilization_organization_id_fkey");

            entity.HasOne(e => e.BusinessPlace)
                .WithMany()
                .HasForeignKey(e => e.BusinessPlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_utilization_business_place_id_fkey");

            entity.HasIndex(e => e.CrptDocumentId)
                .HasDatabaseName("ux_marking_utilization_crpt_document_id")
                .IsUnique()
                .HasFilter("crpt_document_id IS NOT NULL");
        });

        modelBuilder.Entity<MarkingCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_code_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_organization_id_fkey");

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_product_id_fkey");

            entity.HasOne(e => e.OwnerCounterparty)
                .WithMany()
                .HasForeignKey(e => e.OwnerCounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_owner_counterparty_id_fkey");

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_warehouse_id_fkey");

            entity.HasOne(e => e.ParentMarkingCode)
                .WithMany()
                .HasForeignKey(e => e.ParentMarkingCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_parent_marking_code_id_fkey");

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_order_id_fkey");

            entity.HasOne(e => e.Utilization)
                .WithMany()
                .HasForeignKey(e => e.UtilizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_utilization_id_fkey");
        });

        modelBuilder.Entity<MarkingAggregation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_aggregation_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_organization_id_fkey");

            entity.HasOne(e => e.BusinessPlace)
                .WithMany()
                .HasForeignKey(e => e.BusinessPlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_business_place_id_fkey");

            entity.HasOne(e => e.ParentMarkingCode)
                .WithMany()
                .HasForeignKey(e => e.ParentMarkingCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_parent_marking_code_id_fkey");

            entity.HasIndex(e => e.CrptDocumentId)
                .HasDatabaseName("ux_marking_aggregation_crpt_document_id")
                .IsUnique()
                .HasFilter("crpt_document_id IS NOT NULL");
        });

        modelBuilder.Entity<MarkingAslBelgiDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_aslbelgi_document_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aslbelgi_document_organization_id_fkey");

            entity.HasIndex(e => e.ProviderDocumentId)
                .HasDatabaseName("ux_marking_aslbelgi_document_provider_document_id")
                .IsUnique()
                .HasFilter("provider_document_id IS NOT NULL");
        });

        modelBuilder.Entity<MarkingEdocsDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_edocs_document_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_edocs_document_organization_id_fkey");

            entity.HasIndex(e => e.ProviderDocumentId)
                .HasDatabaseName("ux_marking_edocs_document_provider_document_id")
                .IsUnique()
                .HasFilter("provider_document_id IS NOT NULL");
        });

        modelBuilder.Entity<MarkingDidoxDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_didox_document_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_didox_document_organization_id_fkey");

            entity.HasIndex(e => e.ProviderDocumentId)
                .HasDatabaseName("ux_marking_didox_document_provider_document_id")
                .IsUnique()
                .HasFilter("provider_document_id IS NOT NULL");
        });

        modelBuilder.Entity<IntegrationCredential>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("integration_credential_pkey");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("integration_credential_organization_id_fkey");
        });

        modelBuilder.Entity<OrganizationEdoProvider>(entity =>
        {
            entity.HasKey(e => e.OrganizationId)
                .HasName("organization_edo_provider_pkey");

            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()");

            entity.Property(e => e.UpdatedDate)
                .IsConcurrencyToken();

            entity.ToTable(table => table.HasCheckConstraint(
                "ck_organization_edo_provider_provider",
                "provider IN ('DIDOX', 'FAKTURA', 'EDOCS')"));

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("organization_edo_provider_organization_id_fkey");
        });

        modelBuilder.Entity<EdoAuthSigningSession>(entity =>
        {
            entity.HasKey(e => e.SessionId)
                .HasName("edo_auth_signing_session_pkey");

            entity.Property(e => e.SessionId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.ChallengeId)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.ProviderChallengeId)
                .HasMaxLength(256);

            entity.Property(e => e.CertificateSerialNumber)
                .HasMaxLength(256);

            entity.Property(e => e.SigningMode)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()");

            entity.Property(e => e.ConsumedAt)
                .IsConcurrencyToken();

            entity.HasIndex(e => new { e.OrganizationId, e.Provider })
                .HasDatabaseName("idx_edo_auth_signing_session_organization_provider");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("idx_edo_auth_signing_session_expires_at");

            entity.ToTable(table => table.HasCheckConstraint(
                "ck_edo_auth_signing_session_provider",
                "provider IN ('DIDOX', 'FAKTURA', 'EDOCS')"));

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edo_auth_signing_session_organization_id_fkey");
        });

        modelBuilder.Entity<EdoDocument>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("edo_document_pkey");

            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.Direction)
                .HasMaxLength(10)
                .IsRequired();
            entity.Property(e => e.InternalDocumentType)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .IsRequired();
            entity.Property(e => e.ProviderStatusCode)
                .HasMaxLength(100);
            entity.Property(e => e.RejectReason)
                .HasMaxLength(2000);
            entity.Property(e => e.OperationType)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.OrganizationId, e.Provider, e.ProviderDocumentId })
                .HasDatabaseName("ux_edo_document_organization_provider_document")
                .IsUnique()
                .HasFilter("provider_document_id IS NOT NULL");

            entity.HasIndex(e => new { e.OrganizationId, e.Provider, e.OperationType, e.IdempotencyKey })
                .HasDatabaseName("ux_edo_document_organization_provider_operation_key")
                .IsUnique()
                .HasFilter("idempotency_key IS NOT NULL");

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "ck_edo_document_provider",
                    "provider IN ('DIDOX', 'FAKTURA', 'EDOCS')");
                table.HasCheckConstraint(
                    "ck_edo_document_direction",
                    "direction IN ('OUTBOX', 'INBOX')");
            });

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edo_document_organization_id_fkey");
        });

        modelBuilder.Entity<EdoDocumentSigningSession>(entity =>
        {
            entity.HasKey(e => e.SessionId)
                .HasName("edo_document_signing_session_pkey");

            entity.Property(e => e.SessionId)
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.SigningMode)
                .HasMaxLength(40)
                .IsRequired();
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.ConsumedAt)
                .IsConcurrencyToken();

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edo_document_signing_session_organization_id_fkey");

            entity.HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edo_document_signing_session_document_id_fkey");

            entity.ToTable(table => table.HasCheckConstraint(
                "ck_edo_document_signing_session_provider",
                "provider IN ('DIDOX', 'FAKTURA', 'EDOCS')"));
        });

        modelBuilder.Entity<Warehouse>()
            .HasOne(e => e.BusinessPlace)
            .WithMany()
            .HasForeignKey(e => e.BusinessPlaceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("inv_warehouse_business_place_id_fkey");

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

        modelBuilder.Entity<WarehouseProductTable>()
            .HasIndex(x => x.WarehouseId)
            .HasDatabaseName("idx_inv_warehouse_product_table_warehouse_id");

        modelBuilder.Entity<ChartAccountPresetAccount>(entity =>
            entity.HasOne(e => e.ChartAccountPresetAccountNavigation)
                    .WithMany(e => e.InverseChartAccountPresetAccountNavigation)
                    .HasForeignKey(e => new { e.PresetId, e.ParentPresetAccountId })
                    .HasPrincipalKey(e => new { e.PresetId, e.Id })
                    .HasConstraintName("fk_acc_chart_account_preset_account_parent")
                    );

        modelBuilder.Entity<WarehouseProduct>()
            .Property(x => x.AvailableQuantity)
            .HasComputedColumnSql("quantity - reserved_quantity - blocked_quantity", stored: true);

        modelBuilder.Entity<WarehouseProduct>()
            .Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

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

        modelBuilder.Entity<OpeningBalanceAccountDetail>()
            .HasIndex(x => new { x.SourceDocumentTypeId, x.SourceDocumentId })
            .HasDatabaseName("ix_acc_opening_balance_detail_source_document")
            .HasFilter("source_document_type_id IS NOT NULL AND source_document_id IS NOT NULL");

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

        modelBuilder.Entity<OrganizationConfig>()
            .HasOne(x => x.Organization)
            .WithOne(x => x.OrganizationConfig)
            .HasForeignKey<OrganizationConfig>(x => x.OrganizationId);

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasIndex(x => x.Code)
                .HasDatabaseName("ux_sys_setting_global_code")
                .IsUnique()
                .HasFilter("organization_id IS NULL");

            entity.HasIndex(x => new { x.OrganizationId, x.Code })
                .HasDatabaseName("ux_sys_setting_org_code")
                .IsUnique()
                .HasFilter("organization_id IS NOT NULL");

            entity.HasIndex(x => x.Category)
                .HasDatabaseName("idx_sys_setting_category");

            entity.HasIndex(x => x.OrganizationId)
                .HasDatabaseName("idx_sys_setting_organization_id");

            entity.Property(x => x.IsEditable).HasDefaultValue(true);
            entity.Property(x => x.StateId).HasDefaultValue(StateIdConst.ACTIVE);
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne<State>()
                .WithMany()
                .HasForeignKey(x => x.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_setting_state_id_fkey");

            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .HasConstraintName("sys_setting_organization_id_fkey");
        });

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

        ConfigureIdentityPrimaryKeys(modelBuilder);
        ApplyAccessFilters(modelBuilder);
    }

    private static void ConfigureIdentityPrimaryKeys(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is null || primaryKey.Properties.Count != 1)
                continue;

            var idProperty = primaryKey.Properties[0];
            if (idProperty.Name != "Id")
                continue;

            var clrType = idProperty.ClrType;
            if (clrType != typeof(int) && clrType != typeof(long))
                continue;

            idProperty.ValueGenerated = ValueGenerated.OnAdd;

            modelBuilder.Entity(entityType.ClrType)
                .Property(idProperty.Name)
                .ValueGeneratedOnAdd()
                .UseIdentityByDefaultColumn();
        }
    }
}

