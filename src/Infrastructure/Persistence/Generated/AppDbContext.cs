using System;
using System.Collections.Generic;
using Infrastructure.Persistence.Generated.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccAccountType> AccAccountTypes { get; set; }

    public virtual DbSet<AccAccountTypeTranslation> AccAccountTypeTranslations { get; set; }

    public virtual DbSet<AccAccountingPeriod> AccAccountingPeriods { get; set; }

    public virtual DbSet<AccAccountingPolicy> AccAccountingPolicies { get; set; }

    public virtual DbSet<AccChartAccount> AccChartAccounts { get; set; }

    public virtual DbSet<AccChartAccountPreset> AccChartAccountPresets { get; set; }

    public virtual DbSet<AccChartAccountPresetAccount> AccChartAccountPresetAccounts { get; set; }

    public virtual DbSet<AccChartAccountPresetAccountSubkonto> AccChartAccountPresetAccountSubkontos { get; set; }

    public virtual DbSet<AccChartAccountPresetAccountTranslation> AccChartAccountPresetAccountTranslations { get; set; }

    public virtual DbSet<AccChartAccountPresetTranslation> AccChartAccountPresetTranslations { get; set; }

    public virtual DbSet<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; }

    public virtual DbSet<AccDocumentAccountRole> AccDocumentAccountRoles { get; set; }

    public virtual DbSet<AccDocumentAccountRoleTranslation> AccDocumentAccountRoleTranslations { get; set; }

    public virtual DbSet<AccDocumentAccountSetting> AccDocumentAccountSettings { get; set; }

    public virtual DbSet<AccDocumentAccountType> AccDocumentAccountTypes { get; set; }

    public virtual DbSet<AccDocumentAccountTypeRole> AccDocumentAccountTypeRoles { get; set; }

    public virtual DbSet<AccDocumentAccountTypeTranslation> AccDocumentAccountTypeTranslations { get; set; }

    public virtual DbSet<AccOpeningBalance> AccOpeningBalances { get; set; }

    public virtual DbSet<AccOpeningBalanceAccount> AccOpeningBalanceAccounts { get; set; }

    public virtual DbSet<AccOpeningBalanceAccountDetail> AccOpeningBalanceAccountDetails { get; set; }

    public virtual DbSet<AccOpeningBalanceAccountDetailSubkonto> AccOpeningBalanceAccountDetailSubkontos { get; set; }

    public virtual DbSet<AccPostingBatch> AccPostingBatches { get; set; }

    public virtual DbSet<AccRegEntry> AccRegEntries { get; set; }

    public virtual DbSet<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; }

    public virtual DbSet<AccSubkontoType> AccSubkontoTypes { get; set; }

    public virtual DbSet<AccSubkontoTypeTranslation> AccSubkontoTypeTranslations { get; set; }

    public virtual DbSet<BankOperation> BankOperations { get; set; }

    public virtual DbSet<CashBox> CashBoxes { get; set; }

    public virtual DbSet<CashOperation> CashOperations { get; set; }

    public virtual DbSet<CmnBank> CmnBanks { get; set; }

    public virtual DbSet<CmnContract> CmnContracts { get; set; }

    public virtual DbSet<CmnContractType> CmnContractTypes { get; set; }

    public virtual DbSet<CmnContractTypeTranslation> CmnContractTypeTranslations { get; set; }

    public virtual DbSet<CmnCostingMethod> CmnCostingMethods { get; set; }

    public virtual DbSet<CmnCostingMethodTranslation> CmnCostingMethodTranslations { get; set; }

    public virtual DbSet<CmnCounterpartyType> CmnCounterpartyTypes { get; set; }

    public virtual DbSet<CmnCounterpartyTypeTranslation> CmnCounterpartyTypeTranslations { get; set; }

    public virtual DbSet<CmnCurrency> CmnCurrencies { get; set; }

    public virtual DbSet<CmnCurrencyRate> CmnCurrencyRates { get; set; }

    public virtual DbSet<CmnCurrencyRevaluation> CmnCurrencyRevaluations { get; set; }

    public virtual DbSet<CmnCurrencyRevaluationLine> CmnCurrencyRevaluationLines { get; set; }

    public virtual DbSet<CmnCurrencyTranslation> CmnCurrencyTranslations { get; set; }

    public virtual DbSet<CmnDistrict> CmnDistricts { get; set; }

    public virtual DbSet<CmnDocumentNumberSequence> CmnDocumentNumberSequences { get; set; }

    public virtual DbSet<CmnDocumentStatus> CmnDocumentStatuses { get; set; }

    public virtual DbSet<CmnDocumentStatusTranslation> CmnDocumentStatusTranslations { get; set; }

    public virtual DbSet<CmnDocumentType> CmnDocumentTypes { get; set; }

    public virtual DbSet<CmnDocumentTypeTranslation> CmnDocumentTypeTranslations { get; set; }

    public virtual DbSet<CmnFaAssetStatus> CmnFaAssetStatuses { get; set; }

    public virtual DbSet<CmnFaDepreciationMethod> CmnFaDepreciationMethods { get; set; }

    public virtual DbSet<CmnFaGroup> CmnFaGroups { get; set; }

    public virtual DbSet<CmnFaOkof> CmnFaOkofs { get; set; }

    public virtual DbSet<CmnInventoryAdjustmentType> CmnInventoryAdjustmentTypes { get; set; }

    public virtual DbSet<CmnLanguage> CmnLanguages { get; set; }

    public virtual DbSet<CmnMxikCatalog> CmnMxikCatalogs { get; set; }

    public virtual DbSet<CmnNotificationType> CmnNotificationTypes { get; set; }

    public virtual DbSet<CmnOperationType> CmnOperationTypes { get; set; }

    public virtual DbSet<CmnOperationTypeTranslation> CmnOperationTypeTranslations { get; set; }

    public virtual DbSet<CmnPaymentType> CmnPaymentTypes { get; set; }

    public virtual DbSet<CmnPaymentTypeTranslation> CmnPaymentTypeTranslations { get; set; }

    public virtual DbSet<CmnPriceRoundingMethod> CmnPriceRoundingMethods { get; set; }

    public virtual DbSet<CmnPricingCondition> CmnPricingConditions { get; set; }

    public virtual DbSet<CmnPricingMethod> CmnPricingMethods { get; set; }

    public virtual DbSet<CmnProductPriceType> CmnProductPriceTypes { get; set; }

    public virtual DbSet<CmnProductTableStatus> CmnProductTableStatuses { get; set; }

    public virtual DbSet<CmnRegion> CmnRegions { get; set; }

    public virtual DbSet<CmnState> CmnStates { get; set; }

    public virtual DbSet<CmnTaxType> CmnTaxTypes { get; set; }

    public virtual DbSet<CmnTranslation> CmnTranslations { get; set; }

    public virtual DbSet<CmnUnit> CmnUnits { get; set; }

    public virtual DbSet<CmnVatRate> CmnVatRates { get; set; }

    public virtual DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }

    public virtual DbSet<CounterpartyCard> CounterpartyCards { get; set; }

    public virtual DbSet<CounterpartyContact> CounterpartyContacts { get; set; }

    public virtual DbSet<CounterpartyRegBalance> CounterpartyRegBalances { get; set; }

    public virtual DbSet<EdoAuthSigningSession> EdoAuthSigningSessions { get; set; }

    public virtual DbSet<EdoDocument> EdoDocuments { get; set; }

    public virtual DbSet<EdoDocumentSigningSession> EdoDocumentSigningSessions { get; set; }

    public virtual DbSet<FaAsset> FaAssets { get; set; }

    public virtual DbSet<FaDepreciationRun> FaDepreciationRuns { get; set; }

    public virtual DbSet<FaDepreciationRunLine> FaDepreciationRunLines { get; set; }

    public virtual DbSet<FaDisposalDoc> FaDisposalDocs { get; set; }

    public virtual DbSet<FaDisposalDocLine> FaDisposalDocLines { get; set; }

    public virtual DbSet<FaDisposalType> FaDisposalTypes { get; set; }

    public virtual DbSet<FaDisposalTypeTranslation> FaDisposalTypeTranslations { get; set; }

    public virtual DbSet<FaMovementDoc> FaMovementDocs { get; set; }

    public virtual DbSet<FaMovementDocLine> FaMovementDocLines { get; set; }

    public virtual DbSet<FaReceiptDoc> FaReceiptDocs { get; set; }

    public virtual DbSet<FaReceiptDocAsset> FaReceiptDocAssets { get; set; }

    public virtual DbSet<FaReceiptDocLine> FaReceiptDocLines { get; set; }

    public virtual DbSet<FaReceiptType> FaReceiptTypes { get; set; }

    public virtual DbSet<FaReceiptTypeTranslation> FaReceiptTypeTranslations { get; set; }

    public virtual DbSet<FaRevaluationDoc> FaRevaluationDocs { get; set; }

    public virtual DbSet<FaRevaluationDocLine> FaRevaluationDocLines { get; set; }

    public virtual DbSet<HrAbsence> HrAbsences { get; set; }

    public virtual DbSet<HrAbsenceAttachment> HrAbsenceAttachments { get; set; }

    public virtual DbSet<HrAbsenceType> HrAbsenceTypes { get; set; }

    public virtual DbSet<HrEmployeeWorkSchedule> HrEmployeeWorkSchedules { get; set; }

    public virtual DbSet<HrEmployeeWorkScheduleDay> HrEmployeeWorkScheduleDays { get; set; }

    public virtual DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

    public virtual DbSet<IntegrationCredential> IntegrationCredentials { get; set; }

    public virtual DbSet<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; }

    public virtual DbSet<InvInventoryAdjustmentDocTable> InvInventoryAdjustmentDocTables { get; set; }

    public virtual DbSet<InvInventoryAdjustmentLine> InvInventoryAdjustmentLines { get; set; }

    public virtual DbSet<InvInventoryCountDoc> InvInventoryCountDocs { get; set; }

    public virtual DbSet<InvInventoryCountDocTable> InvInventoryCountDocTables { get; set; }

    public virtual DbSet<InvInventoryCountLine> InvInventoryCountLines { get; set; }

    public virtual DbSet<InvOpeningInventory> InvOpeningInventories { get; set; }

    public virtual DbSet<InvOpeningInventoryProduct> InvOpeningInventoryProducts { get; set; }

    public virtual DbSet<InvOpeningInventoryTable> InvOpeningInventoryTables { get; set; }

    public virtual DbSet<InvProduct> InvProducts { get; set; }

    public virtual DbSet<InvProductGroup> InvProductGroups { get; set; }

    public virtual DbSet<InvProductPrice> InvProductPrices { get; set; }

    public virtual DbSet<InvProductTable> InvProductTables { get; set; }

    public virtual DbSet<InvRegBalance> InvRegBalances { get; set; }

    public virtual DbSet<InvTransferDoc> InvTransferDocs { get; set; }

    public virtual DbSet<InvTransferDocTable> InvTransferDocTables { get; set; }

    public virtual DbSet<InvTransferLine> InvTransferLines { get; set; }

    public virtual DbSet<InvWarehouse> InvWarehouses { get; set; }

    public virtual DbSet<InvWarehouseProduct> InvWarehouseProducts { get; set; }

    public virtual DbSet<InvWarehouseProductBatch> InvWarehouseProductBatches { get; set; }

    public virtual DbSet<InvWarehouseProductBatchAllocation> InvWarehouseProductBatchAllocations { get; set; }

    public virtual DbSet<InvWarehouseProductBatchTable> InvWarehouseProductBatchTables { get; set; }

    public virtual DbSet<InvWarehouseProductMovement> InvWarehouseProductMovements { get; set; }

    public virtual DbSet<InvWarehouseProductTable> InvWarehouseProductTables { get; set; }

    public virtual DbSet<MarkingAggregation> MarkingAggregations { get; set; }

    public virtual DbSet<MarkingAslbelgiDocument> MarkingAslbelgiDocuments { get; set; }

    public virtual DbSet<MarkingBusinessPlace> MarkingBusinessPlaces { get; set; }

    public virtual DbSet<MarkingCode> MarkingCodes { get; set; }

    public virtual DbSet<MarkingDidoxDocument> MarkingDidoxDocuments { get; set; }

    public virtual DbSet<MarkingEdocsDocument> MarkingEdocsDocuments { get; set; }

    public virtual DbSet<MarkingOrder> MarkingOrders { get; set; }

    public virtual DbSet<MarkingUtilization> MarkingUtilizations { get; set; }

    public virtual DbSet<MoneyRegBalance> MoneyRegBalances { get; set; }

    public virtual DbSet<OrgBankAccount> OrgBankAccounts { get; set; }

    public virtual DbSet<OrgBranch> OrgBranches { get; set; }

    public virtual DbSet<OrgClaimRequest> OrgClaimRequests { get; set; }

    public virtual DbSet<OrgDefault> OrgDefaults { get; set; }

    public virtual DbSet<OrgDepartment> OrgDepartments { get; set; }

    public virtual DbSet<OrgOrganization> OrgOrganizations { get; set; }

    public virtual DbSet<OrgOrganizationConfig> OrgOrganizationConfigs { get; set; }

    public virtual DbSet<OrgPosition> OrgPositions { get; set; }

    public virtual DbSet<OrgSetupState> OrgSetupStates { get; set; }

    public virtual DbSet<OrgTaxSetting> OrgTaxSettings { get; set; }

    public virtual DbSet<OrgUserInvitation> OrgUserInvitations { get; set; }

    public virtual DbSet<OrganizationEdoProvider> OrganizationEdoProviders { get; set; }

    public virtual DbSet<PayComponent> PayComponents { get; set; }

    public virtual DbSet<PayEmployee> PayEmployees { get; set; }

    public virtual DbSet<PayEmployeeComponent> PayEmployeeComponents { get; set; }

    public virtual DbSet<PayEmployment> PayEmployments { get; set; }

    public virtual DbSet<PayPaymentBatch> PayPaymentBatches { get; set; }

    public virtual DbSet<PayPaymentLine> PayPaymentLines { get; set; }

    public virtual DbSet<PayPayrollCalcLine> PayPayrollCalcLines { get; set; }

    public virtual DbSet<PayPayrollDoc> PayPayrollDocs { get; set; }

    public virtual DbSet<PayPayrollLine> PayPayrollLines { get; set; }

    public virtual DbSet<PayPeriod> PayPeriods { get; set; }

    public virtual DbSet<PayTimesheet> PayTimesheets { get; set; }

    public virtual DbSet<PayTimesheetLine> PayTimesheetLines { get; set; }

    public virtual DbSet<PlatformTenant> PlatformTenants { get; set; }

    public virtual DbSet<PurDoc> PurDocs { get; set; }

    public virtual DbSet<PurDocProduct> PurDocProducts { get; set; }

    public virtual DbSet<PurDocTable> PurDocTables { get; set; }

    public virtual DbSet<SaleCondition> SaleConditions { get; set; }

    public virtual DbSet<SaleDoc> SaleDocs { get; set; }

    public virtual DbSet<SaleDocProduct> SaleDocProducts { get; set; }

    public virtual DbSet<SaleDocProductBatch> SaleDocProductBatches { get; set; }

    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }

    public virtual DbSet<SaleShipmentDoc> SaleShipmentDocs { get; set; }

    public virtual DbSet<SaleShipmentProduct> SaleShipmentProducts { get; set; }

    public virtual DbSet<SaleShipmentProductBatch> SaleShipmentProductBatches { get; set; }

    public virtual DbSet<SaleShipmentTable> SaleShipmentTables { get; set; }

    public virtual DbSet<SysAuditLog> SysAuditLogs { get; set; }

    public virtual DbSet<SysEmailVerificationToken> SysEmailVerificationTokens { get; set; }

    public virtual DbSet<SysModule> SysModules { get; set; }

    public virtual DbSet<SysModuleSubGroup> SysModuleSubGroups { get; set; }

    public virtual DbSet<SysNotification> SysNotifications { get; set; }

    public virtual DbSet<SysNotificationDelivery> SysNotificationDeliveries { get; set; }

    public virtual DbSet<SysNotificationRead> SysNotificationReads { get; set; }

    public virtual DbSet<SysPasswordResetToken> SysPasswordResetTokens { get; set; }

    public virtual DbSet<SysRefreshToken> SysRefreshTokens { get; set; }

    public virtual DbSet<SysRole> SysRoles { get; set; }

    public virtual DbSet<SysRoleModule> SysRoleModules { get; set; }

    public virtual DbSet<SysSetting> SysSettings { get; set; }

    public virtual DbSet<SysUser> SysUsers { get; set; }

    public virtual DbSet<SysUserKind> SysUserKinds { get; set; }

    public virtual DbSet<SysUserKindTranslation> SysUserKindTranslations { get; set; }

    public virtual DbSet<SysUserOrganization> SysUserOrganizations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccAccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_account_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.AccAccountTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_type_state_id_fkey");
        });

        modelBuilder.Entity<AccAccountTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.AccountTypeId, e.LanguageId }).HasName("acc_account_type_translation_pkey");

            entity.HasOne(d => d.AccountType).WithMany(p => p.AccAccountTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_type_translation_account_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccAccountTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<AccAccountingPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_accounting_period_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<AccAccountingPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_accounting_policy_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.AccAccountingPolicies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_accounting_policy_state_id_fkey");
        });

        modelBuilder.Entity<AccChartAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AccountType).WithMany(p => p.AccChartAccounts).HasConstraintName("acc_chart_account_account_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccChartAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_organization_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasConstraintName("acc_chart_account_parent_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_state_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountPreset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_preset_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccountPresets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_state_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountPresetAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_preset_account_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.DisplayOrder).HasDefaultValue(1);

            entity.HasOne(d => d.AccountType).WithMany(p => p.AccChartAccountPresetAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_account_account_type_id_fkey");

            entity.HasOne(d => d.Preset).WithMany(p => p.AccChartAccountPresetAccounts).HasConstraintName("acc_chart_account_preset_account_preset_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccountPresetAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_account_state_id_fkey");

            entity.HasOne(d => d.AccChartAccountPresetAccountNavigation).WithMany(p => p.InverseAccChartAccountPresetAccountNavigation)
                .HasPrincipalKey(p => new { p.PresetId, p.Id })
                .HasForeignKey(d => new { d.PresetId, d.ParentPresetAccountId })
                .HasConstraintName("fk_acc_chart_account_preset_account_parent");
        });

        modelBuilder.Entity<AccChartAccountPresetAccountSubkonto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_preset_account_subkonto_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.PresetAccount).WithMany(p => p.AccChartAccountPresetAccountSubkontos).HasConstraintName("acc_chart_account_preset_account_subkont_preset_account_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccChartAccountPresetAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_account_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountPresetAccountTranslation>(entity =>
        {
            entity.HasKey(e => new { e.PresetAccountId, e.LanguageId }).HasName("acc_chart_account_preset_account_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccChartAccountPresetAccountTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_account_translation_language_id_fkey");

            entity.HasOne(d => d.PresetAccount).WithMany(p => p.AccChartAccountPresetAccountTranslations).HasConstraintName("acc_chart_account_preset_account_transla_preset_account_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountPresetTranslation>(entity =>
        {
            entity.HasKey(e => new { e.PresetId, e.LanguageId }).HasName("acc_chart_account_preset_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccChartAccountPresetTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_preset_translation_language_id_fkey");

            entity.HasOne(d => d.Preset).WithMany(p => p.AccChartAccountPresetTranslations).HasConstraintName("acc_chart_account_preset_translation_preset_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountSubkonto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_subkonto_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithMany(p => p.AccChartAccountSubkontos).HasConstraintName("acc_chart_account_subkonto_account_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_state_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccChartAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_document_account_role_pkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccDocumentAccountRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_role_state_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountRoleTranslation>(entity =>
        {
            entity.HasKey(e => new { e.DocumentAccountRoleId, e.LanguageId }).HasName("acc_document_account_role_translation_pkey");

            entity.HasOne(d => d.DocumentAccountRole).WithMany(p => p.AccDocumentAccountRoleTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_role_transla_document_account_role_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccDocumentAccountRoleTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_role_translation_language_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_document_account_setting_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.DocumentAccountTypeRoleId }, "ux_acc_document_account_setting_default")
                .IsUnique()
                .HasFilter("((is_default = true) AND (state_id = 1))");

            entity.Property(e => e.CanChange).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.SortOrder).HasDefaultValue(1);

            entity.HasOne(d => d.ChartAccount).WithMany(p => p.AccDocumentAccountSettings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_setting_chart_account_id_fkey");

            entity.HasOne(d => d.DocumentAccountTypeRole).WithMany(p => p.AccDocumentAccountSettings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_setting_document_account_type_role_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccDocumentAccountSettings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_setting_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccDocumentAccountSettings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_setting_state_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_document_account_type_pkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccDocumentAccountTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_type_state_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountTypeRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_document_account_type_role_pkey");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(1);

            entity.HasOne(d => d.DocumentAccountRole).WithMany(p => p.AccDocumentAccountTypeRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_type_role_document_account_role_id_fkey");

            entity.HasOne(d => d.DocumentAccountType).WithMany(p => p.AccDocumentAccountTypeRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_type_role_document_account_type_id_fkey");
        });

        modelBuilder.Entity<AccDocumentAccountTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.LanguageId, e.DocumentAccountTypeId }).HasName("acc_document_account_type_translation_pkey");

            entity.HasOne(d => d.DocumentAccountType).WithMany(p => p.AccDocumentAccountTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_type_transla_document_account_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccDocumentAccountTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_document_account_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<AccOpeningBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_opening_balance_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithOne(p => p.AccOpeningBalance)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_opening_balance_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccOpeningBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_opening_balance_state_id_fkey");
        });

        modelBuilder.Entity<AccOpeningBalanceAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_opening_balance_account_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ChartAccount).WithMany(p => p.AccOpeningBalanceAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_opening_balance_account_chart_account_id_fkey");

            entity.HasOne(d => d.OpeningBalance).WithMany(p => p.AccOpeningBalanceAccounts).HasConstraintName("acc_opening_balance_account_opening_balance_id_fkey");
        });

        modelBuilder.Entity<AccOpeningBalanceAccountDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_opening_balance_account_detail_pkey");

            entity.HasIndex(e => new { e.SourceDocumentTypeId, e.SourceDocumentId }, "ix_acc_opening_balance_detail_source_document").HasFilter("((source_document_type_id IS NOT NULL) AND (source_document_id IS NOT NULL))");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.SortOrder).HasDefaultValue(1);

            entity.HasOne(d => d.Currency).WithMany(p => p.AccOpeningBalanceAccountDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_opening_balance_account_detail_currency_id_fkey");

            entity.HasOne(d => d.OpeningBalanceAccount).WithMany(p => p.AccOpeningBalanceAccountDetails).HasConstraintName("acc_opening_balance_account_det_opening_balance_account_id_fkey");
        });

        modelBuilder.Entity<AccOpeningBalanceAccountDetailSubkonto>(entity =>
        {
            entity.HasKey(e => new { e.OpeningBalanceAccountDetailId, e.SubkontoTypeId }).HasName("acc_opening_balance_account_detail_subkonto_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.OpeningBalanceAccountDetail).WithMany(p => p.AccOpeningBalanceAccountDetailSubkontos).HasConstraintName("acc_opening_balance_account_d_opening_balance_account_deta_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccOpeningBalanceAccountDetailSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_opening_balance_account_detail_subkon_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccPostingBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_batch_pkey");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "ux_acc_posting_batch_document_posted")
                .IsUnique()
                .HasFilter("((status)::text = 'POSTED'::text)");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "ux_acc_posting_batch_document_reversal")
                .IsUnique()
                .HasFilter("((status)::text = 'REVERSAL'::text)");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.PostedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'posted'::character varying");
        });

        modelBuilder.Entity<AccRegEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_reg_entry_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreditAccount).WithMany(p => p.AccRegEntryCreditAccounts).HasConstraintName("acc_reg_entry_credit_account_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.AccRegEntries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_currency_id_fkey");

            entity.HasOne(d => d.DebitAccount).WithMany(p => p.AccRegEntryDebitAccounts).HasConstraintName("acc_reg_entry_debit_account_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.AccRegEntries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.AccRegEntries).HasConstraintName("acc_reg_entry_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccRegEntries)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_organization_id_fkey");
        });

        modelBuilder.Entity<AccRegEntrySubkonto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_reg_entry_subkonto_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Entry).WithMany(p => p.AccRegEntrySubkontos).HasConstraintName("acc_reg_entry_subkonto_entry_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccRegEntrySubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccSubkontoType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_subkonto_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.AccSubkontoTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_subkonto_type_state_id_fkey");
        });

        modelBuilder.Entity<AccSubkontoTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.SubkontoTypeId, e.LanguageId }).HasName("acc_subkonto_type_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccSubkontoTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_subkonto_type_translation_language_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccSubkontoTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_subkonto_type_translation_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<BankOperation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bank_operation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ExchangeRate).HasDefaultValue(1m);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_bank_account_id_fkey");

            entity.HasOne(d => d.BankChartAccount).WithMany(p => p.BankOperationBankChartAccounts).HasConstraintName("bank_operation_bank_chart_account_id_fkey");

            entity.HasOne(d => d.Contract).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_contract_id_fkey");

            entity.HasOne(d => d.CounterpartyBankAccount).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_counterparty_bank_account_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_currency_id_fkey");

            entity.HasOne(d => d.OffsetAccount).WithMany(p => p.BankOperationOffsetAccounts).HasConstraintName("bank_operation_offset_account_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_payment_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_status_id_fkey");
        });

        modelBuilder.Entity<CashBox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cash_box_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.CashBoxes).HasConstraintName("cash_box_branch_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CashBoxes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CashBoxes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CashBoxes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_state_id_fkey");
        });

        modelBuilder.Entity<CashOperation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cash_operation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ExchangeRate).HasDefaultValue(1m);

            entity.HasOne(d => d.CashBox).WithMany(p => p.CashOperationCashBoxes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_cash_box_id_fkey");

            entity.HasOne(d => d.CashChartAccount).WithMany(p => p.CashOperationCashChartAccounts).HasConstraintName("cash_operation_cash_chart_account_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CashOperations).HasConstraintName("cash_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_currency_id_fkey");

            entity.HasOne(d => d.DestinationCashBox).WithMany(p => p.CashOperationDestinationCashBoxes).HasConstraintName("cash_operation_destination_cash_box_id_fkey");

            entity.HasOne(d => d.OffsetAccount).WithMany(p => p.CashOperationOffsetAccounts).HasConstraintName("cash_operation_offset_account_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.CashOperations).HasConstraintName("cash_operation_payment_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_status_id_fkey");
        });

        modelBuilder.Entity<CmnBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_bank_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnBanks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_bank_state_id_fkey");
        });

        modelBuilder.Entity<CmnContract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_contract_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ContractType).WithMany(p => p.CmnContracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_contract_type_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CmnContracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_counterparty_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CmnContracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnContracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_state_id_fkey");
        });

        modelBuilder.Entity<CmnContractType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_contract_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnContractTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnContractTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.ContractTypeId, e.LanguageId }).HasName("cmn_contract_type_translation_pkey");

            entity.HasOne(d => d.ContractType).WithMany(p => p.CmnContractTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_type_translation_contract_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnContractTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_contract_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnCostingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_costing_method_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnCostingMethodTranslation>(entity =>
        {
            entity.HasKey(e => new { e.CostingMethodId, e.LanguageId }).HasName("cmn_costing_method_translation_pkey");

            entity.HasOne(d => d.CostingMethod).WithMany(p => p.CmnCostingMethodTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_costing_method_translation_costing_method_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnCostingMethodTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_costing_method_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnCounterpartyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_counterparty_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnCounterpartyTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_counterparty_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnCounterpartyTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.CounterpartyTypeId, e.LanguageId }).HasName("cmn_counterparty_type_translation_pkey");

            entity.HasOne(d => d.CounterpartyType).WithMany(p => p.CmnCounterpartyTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_counterparty_type_translation_counterparty_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnCounterpartyTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_counterparty_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnCurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_currency_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnCurrencies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_state_id_fkey");
        });

        modelBuilder.Entity<CmnCurrencyRate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_currency_rate_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.BaseCurrency).WithMany(p => p.CmnCurrencyRateBaseCurrencies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_rate_base_currency_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnCurrencyRates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_rate_state_id_fkey");

            entity.HasOne(d => d.TargetCurrency).WithMany(p => p.CmnCurrencyRateTargetCurrencies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_rate_target_currency_id_fkey");
        });

        modelBuilder.Entity<CmnCurrencyRevaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_currency_revaluation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.CmnCurrencyRevaluations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnCurrencyRevaluations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_state_id_fkey");
        });

        modelBuilder.Entity<CmnCurrencyRevaluationLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_currency_revaluation_line_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BaseCurrency).WithMany(p => p.CmnCurrencyRevaluationLineBaseCurrencies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_line_base_currency_id_fkey");

            entity.HasOne(d => d.Revaluation).WithMany(p => p.CmnCurrencyRevaluationLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_line_revaluation_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnCurrencyRevaluationLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_line_state_id_fkey");

            entity.HasOne(d => d.TargetCurrency).WithMany(p => p.CmnCurrencyRevaluationLineTargetCurrencies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_revaluation_line_target_currency_id_fkey");
        });

        modelBuilder.Entity<CmnCurrencyTranslation>(entity =>
        {
            entity.HasKey(e => new { e.CurrencyId, e.LanguageId }).HasName("cmn_currency_translation_pkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CmnCurrencyTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_translation_currency_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnCurrencyTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnDistrict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_district_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CmnDocumentNumberSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_number_sequence_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastDocumentDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.CmnDocumentNumberSequences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_number_sequence_document_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CmnDocumentNumberSequences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_number_sequence_organization_id_fkey");
        });

        modelBuilder.Entity<CmnDocumentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_status_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnDocumentStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_status_state_id_fkey");
        });

        modelBuilder.Entity<CmnDocumentStatusTranslation>(entity =>
        {
            entity.HasKey(e => new { e.DocumentStatusId, e.LanguageId }).HasName("cmn_document_status_translation_pkey");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.CmnDocumentStatusTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_status_translation_document_status_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnDocumentStatusTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_status_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnDocumentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnDocumentTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnDocumentTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.DocumentTypeId, e.LanguageId }).HasName("cmn_document_type_translation_pkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.CmnDocumentTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_type_translation_document_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnDocumentTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnFaAssetStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_fa_asset_status_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnFaAssetStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_fa_asset_status_state_id_fkey");
        });

        modelBuilder.Entity<CmnFaDepreciationMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_fa_depreciation_method_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnFaDepreciationMethods)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_fa_depreciation_method_state_id_fkey");
        });

        modelBuilder.Entity<CmnFaGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_fa_group_pkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CmnFaGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_fa_group_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnFaGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_fa_group_state_id_fkey");
        });

        modelBuilder.Entity<CmnFaOkof>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_fa_okof_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnFaOkofs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_fa_okof_state_id_fkey");
        });

        modelBuilder.Entity<CmnInventoryAdjustmentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_inventory_adjustment_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnInventoryAdjustmentTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_inventory_adjustment_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_language_pkey");

            entity.HasIndex(e => e.IsDefault, "idx_cmn_language_default")
                .IsUnique()
                .HasFilter("(is_default = true)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnLanguages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_language_state_id_fkey");
        });

        modelBuilder.Entity<CmnMxikCatalog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_mxik_catalog_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnMxikCatalogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_mxik_catalog_state_id_fkey");
        });

        modelBuilder.Entity<CmnNotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_notification_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CmnOperationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_operation_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnOperationTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_operation_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnOperationTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.OperationTypeId, e.LanguageId }).HasName("cmn_operation_type_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnOperationTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_operation_type_translation_language_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CmnOperationTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_operation_type_translation_operation_type_id_fkey");
        });

        modelBuilder.Entity<CmnPaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_payment_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnPaymentTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_payment_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnPaymentTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.PaymentTypeId, e.LanguageId }).HasName("cmn_payment_type_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnPaymentTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_payment_type_translation_language_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.CmnPaymentTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_payment_type_translation_payment_type_id_fkey");
        });

        modelBuilder.Entity<CmnPriceRoundingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_price_rounding_method_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnPricingCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_pricing_condition_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.RoundingPrecision).HasDefaultValue(1m);
            entity.Property(e => e.StartDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.CmnPricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_organization_id_fkey");

            entity.HasOne(d => d.PricingMethod).WithMany(p => p.CmnPricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_pricing_method_id_fkey");

            entity.HasOne(d => d.RoundingMethod).WithMany(p => p.CmnPricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_rounding_method_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CmnPricingConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_pricing_condition_state_id_fkey");
        });

        modelBuilder.Entity<CmnPricingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_pricing_method_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnProductPriceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_product_price_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnProductTableStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_product_table_status_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnProductTableStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_product_table_status_state_id_fkey");
        });

        modelBuilder.Entity<CmnRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_region_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnRegions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_region_state_id_fkey");
        });

        modelBuilder.Entity<CmnState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_state_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CmnTaxType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_tax_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnTaxTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_tax_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_translation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_unit_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_unit_state_id_fkey");
        });

        modelBuilder.Entity<CmnVatRate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_vat_rate_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.CmnVatRates)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_vat_rate_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_bank_account_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Bank).WithMany(p => p.CounterpartyBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_bank_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CounterpartyBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_card_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_counterparty_card_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsCustomer).HasDefaultValue(true);
            entity.Property(e => e.IsSupplier).HasDefaultValue(true);

            entity.HasOne(d => d.CounterpartyType).WithMany(p => p.CounterpartyCards)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_counterparty_type_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.CounterpartyCards).HasConstraintName("counterparty_card_district_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyCards)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_organization_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.CounterpartyCards).HasConstraintName("counterparty_card_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyCards)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_contact_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyContacts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_counterparty_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyContacts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyContacts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_reg_balance_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CounterpartyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_currency_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.CounterpartyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CounterpartyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_organization_id_fkey");
        });

        modelBuilder.Entity<EdoAuthSigningSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("edo_auth_signing_session_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.EdoAuthSigningSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("edo_auth_signing_session_organization_id_fkey");
        });

        modelBuilder.Entity<EdoDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edo_document_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Provider, e.ProviderDocumentId }, "ux_edo_document_organization_provider_document")
                .IsUnique()
                .HasFilter("(provider_document_id IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.Provider, e.OperationType, e.IdempotencyKey }, "ux_edo_document_organization_provider_operation_key")
                .IsUnique()
                .HasFilter("(idempotency_key IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.EdoDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("edo_document_organization_id_fkey");
        });

        modelBuilder.Entity<EdoDocumentSigningSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("edo_document_signing_session_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Document).WithMany(p => p.EdoDocumentSigningSessions).HasConstraintName("edo_document_signing_session_document_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.EdoDocumentSigningSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("edo_document_signing_session_organization_id_fkey");
        });

        modelBuilder.Entity<FaAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_asset_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AccumulatedDepreciationAccount).WithMany(p => p.FaAssetAccumulatedDepreciationAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_asset_accumulated_depreciation_account_id_fkey");

            entity.HasOne(d => d.AssetAccount).WithMany(p => p.FaAssetAssetAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_asset_asset_account_id_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.FaAssets).HasConstraintName("fa_asset_department_id_fkey");

            entity.HasOne(d => d.DepreciationExpenseAccount).WithMany(p => p.FaAssetDepreciationExpenseAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_asset_depreciation_expense_account_id_fkey");

            entity.HasOne(d => d.DepreciationMethod).WithMany(p => p.FaAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_asset_depreciation_method_id_fkey");

            entity.HasOne(d => d.FaGroup).WithMany(p => p.FaAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_asset_fa_group_id_fkey");

            entity.HasOne(d => d.Okof).WithMany(p => p.FaAssets).HasConstraintName("fa_asset_okof_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_asset_organization_id_fkey");

            entity.HasOne(d => d.ResponsibleUser).WithMany(p => p.FaAssets).HasConstraintName("fa_asset_responsible_user_id_fkey");

            entity.HasOne(d => d.SourceProductTable).WithMany(p => p.FaAssets).HasConstraintName("fa_asset_source_product_table_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_asset_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_asset_status_id_fkey");
        });

        modelBuilder.Entity<FaDepreciationRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_depreciation_run_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.PeriodMonth }, "ux_fa_depreciation_run_org_period_active")
                .IsUnique()
                .HasFilter("(status_id <> 3)");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.FaDepreciationRunCancelledByUsers).HasConstraintName("fa_depreciation_run_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.FaDepreciationRunCreatedByUsers).HasConstraintName("fa_depreciation_run_created_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaDepreciationRuns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_depreciation_run_organization_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.FaDepreciationRunPostedByUsers).HasConstraintName("fa_depreciation_run_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaDepreciationRuns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_depreciation_run_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaDepreciationRuns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_depreciation_run_status_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.FaDepreciationRunUpdatedByUsers).HasConstraintName("fa_depreciation_run_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<FaDepreciationRunLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_depreciation_run_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.AccumulatedDepreciationAccount).WithMany(p => p.FaDepreciationRunLineAccumulatedDepreciationAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_depreciation_run_line_accumulated_depreciation_account__fkey");

            entity.HasOne(d => d.DepreciationRun).WithMany(p => p.FaDepreciationRunLines).HasConstraintName("fa_depreciation_run_line_depreciation_run_id_fkey");

            entity.HasOne(d => d.ExpenseAccount).WithMany(p => p.FaDepreciationRunLineExpenseAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_depreciation_run_line_expense_account_id_fkey");

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaDepreciationRunLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_depreciation_run_line_fa_asset_id_fkey");
        });

        modelBuilder.Entity<FaDisposalDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_disposal_doc_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.FaDisposalDocCancelledByUsers).HasConstraintName("fa_disposal_doc_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.FaDisposalDocCreatedByUsers).HasConstraintName("fa_disposal_doc_created_by_user_id_fkey");

            entity.HasOne(d => d.CustomerAccount).WithMany(p => p.FaDisposalDocCustomerAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_customer_account_id_fkey");

            entity.HasOne(d => d.DisposalAccount).WithMany(p => p.FaDisposalDocDisposalAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_disposal_account_id_fkey");

            entity.HasOne(d => d.DisposalType).WithMany(p => p.FaDisposalDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_disposal_type_id_fkey");

            entity.HasOne(d => d.GainAccount).WithMany(p => p.FaDisposalDocGainAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_gain_account_id_fkey");

            entity.HasOne(d => d.LossAccount).WithMany(p => p.FaDisposalDocLossAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_loss_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaDisposalDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_organization_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.FaDisposalDocPostedByUsers).HasConstraintName("fa_disposal_doc_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaDisposalDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaDisposalDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_status_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.FaDisposalDocUpdatedByUsers).HasConstraintName("fa_disposal_doc_updated_by_user_id_fkey");

            entity.HasOne(d => d.VatAccount).WithMany(p => p.FaDisposalDocVatAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_vat_account_id_fkey");
        });

        modelBuilder.Entity<FaDisposalDocLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_disposal_doc_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.AccumulatedDepreciationAccount).WithMany(p => p.FaDisposalDocLineAccumulatedDepreciationAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_line_accumulated_depreciation_account_id_fkey");

            entity.HasOne(d => d.AssetAccount).WithMany(p => p.FaDisposalDocLineAssetAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_disposal_doc_line_asset_account_id_fkey");

            entity.HasOne(d => d.DisposalDoc).WithMany(p => p.FaDisposalDocLines).HasConstraintName("fa_disposal_doc_line_disposal_doc_id_fkey");

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaDisposalDocLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_line_fa_asset_id_fkey");
        });

        modelBuilder.Entity<FaDisposalType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_disposal_type_pkey");
        });

        modelBuilder.Entity<FaDisposalTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.DisposalTypeId, e.LanguageId }).HasName("fa_disposal_type_translation_pkey");

            entity.HasOne(d => d.DisposalType).WithMany(p => p.FaDisposalTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_type_translation_disposal_type_id_fkey");

            entity.HasOne(d => d.Language).WithMany(p => p.FaDisposalTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_type_translation_language_id_fkey");
        });

        modelBuilder.Entity<FaMovementDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_movement_doc_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.FaMovementDocCancelledByUsers).HasConstraintName("fa_movement_doc_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.FaMovementDocCreatedByUsers).HasConstraintName("fa_movement_doc_created_by_user_id_fkey");

            entity.HasOne(d => d.FromDepartment).WithMany(p => p.FaMovementDocFromDepartments).HasConstraintName("fa_movement_doc_from_department_id_fkey");

            entity.HasOne(d => d.FromResponsibleUser).WithMany(p => p.FaMovementDocFromResponsibleUsers).HasConstraintName("fa_movement_doc_from_responsible_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaMovementDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_movement_doc_organization_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.FaMovementDocPostedByUsers).HasConstraintName("fa_movement_doc_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaMovementDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_movement_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaMovementDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_movement_doc_status_id_fkey");

            entity.HasOne(d => d.ToDepartment).WithMany(p => p.FaMovementDocToDepartments).HasConstraintName("fa_movement_doc_to_department_id_fkey");

            entity.HasOne(d => d.ToResponsibleUser).WithMany(p => p.FaMovementDocToResponsibleUsers).HasConstraintName("fa_movement_doc_to_responsible_user_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.FaMovementDocUpdatedByUsers).HasConstraintName("fa_movement_doc_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<FaMovementDocLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_movement_doc_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaMovementDocLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_movement_doc_line_fa_asset_id_fkey");

            entity.HasOne(d => d.MovementDoc).WithMany(p => p.FaMovementDocLines).HasConstraintName("fa_movement_doc_line_movement_doc_id_fkey");
        });

        modelBuilder.Entity<FaReceiptDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_receipt_doc_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.FaReceiptDocs).HasConstraintName("fa_receipt_doc_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_organization_id_fkey");

            entity.HasOne(d => d.ReceiptType).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_receipt_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_status_id_fkey");

            entity.HasOne(d => d.SupplierAccount).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_supplier_account_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.FaReceiptDocs).HasConstraintName("fa_receipt_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<FaReceiptDocAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_receipt_doc_asset_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.AccumulatedDepreciationAccount).WithMany(p => p.FaReceiptDocAssetAccumulatedDepreciationAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_asset_accumulated_depreciation_account_id_fkey");

            entity.HasOne(d => d.AssetAccount).WithMany(p => p.FaReceiptDocAssetAssetAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_asset_asset_account_id_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_department_id_fkey");

            entity.HasOne(d => d.DepreciationExpenseAccount).WithMany(p => p.FaReceiptDocAssetDepreciationExpenseAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_asset_depreciation_expense_account_id_fkey");

            entity.HasOne(d => d.DepreciationMethod).WithMany(p => p.FaReceiptDocAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_asset_depreciation_method_id_fkey");

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_fa_asset_id_fkey");

            entity.HasOne(d => d.FaGroup).WithMany(p => p.FaReceiptDocAssets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_asset_fa_group_id_fkey");

            entity.HasOne(d => d.Okof).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_okof_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_owner_id_fkey");

            entity.HasOne(d => d.ResponsibleUser).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_responsible_user_id_fkey");
        });

        modelBuilder.Entity<FaReceiptDocLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_receipt_doc_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.CapitalInvestmentAccount).WithMany(p => p.FaReceiptDocLineCapitalInvestmentAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_line_capital_investment_account_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_owner_id_fkey");

            entity.HasOne(d => d.SourceProduct).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_source_product_id_fkey");

            entity.HasOne(d => d.VatAccount).WithMany(p => p.FaReceiptDocLineVatAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_receipt_doc_line_vat_account_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_vat_rate_id_fkey");
        });

        modelBuilder.Entity<FaReceiptType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_receipt_type_pkey");
        });

        modelBuilder.Entity<FaReceiptTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.ReceiptTypeId, e.LanguageId }).HasName("fa_receipt_type_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.FaReceiptTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_type_translation_language_id_fkey");

            entity.HasOne(d => d.ReceiptType).WithMany(p => p.FaReceiptTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_type_translation_receipt_type_id_fkey");
        });

        modelBuilder.Entity<FaRevaluationDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_revaluation_doc_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.FaRevaluationDocCancelledByUsers).HasConstraintName("fa_revaluation_doc_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.FaRevaluationDocCreatedByUsers).HasConstraintName("fa_revaluation_doc_created_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.FaRevaluationDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_revaluation_doc_organization_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.FaRevaluationDocPostedByUsers).HasConstraintName("fa_revaluation_doc_posted_by_user_id_fkey");

            entity.HasOne(d => d.RevaluationLossAccount).WithMany(p => p.FaRevaluationDocRevaluationLossAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_revaluation_doc_revaluation_loss_account_id_fkey");

            entity.HasOne(d => d.RevaluationReserveAccount).WithMany(p => p.FaRevaluationDocRevaluationReserveAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_revaluation_doc_revaluation_reserve_account_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.FaRevaluationDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_revaluation_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaRevaluationDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_revaluation_doc_status_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.FaRevaluationDocUpdatedByUsers).HasConstraintName("fa_revaluation_doc_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<FaRevaluationDocLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_revaluation_doc_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.AccumulatedDepreciationAccount).WithMany(p => p.FaRevaluationDocLineAccumulatedDepreciationAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_revaluation_doc_line_accumulated_depreciation_account_i_fkey");

            entity.HasOne(d => d.AssetAccount).WithMany(p => p.FaRevaluationDocLineAssetAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fa_revaluation_doc_line_asset_account_id_fkey");

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaRevaluationDocLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_revaluation_doc_line_fa_asset_id_fkey");

            entity.HasOne(d => d.RevaluationDoc).WithMany(p => p.FaRevaluationDocLines).HasConstraintName("fa_revaluation_doc_line_revaluation_doc_id_fkey");
        });

        modelBuilder.Entity<HrAbsence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hr_absence_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.AbsenceType).WithMany(p => p.HrAbsences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_absence_type_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.HrAbsenceCreatedByUsers).HasConstraintName("hr_absence_created_by_user_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.HrAbsences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_employee_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.HrAbsences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.HrAbsences)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_state_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.HrAbsenceUpdatedByUsers).HasConstraintName("hr_absence_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<HrAbsenceAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hr_absence_attachment_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Absence).WithMany(p => p.HrAbsenceAttachments).HasConstraintName("hr_absence_attachment_absence_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.HrAbsenceAttachments).HasConstraintName("hr_absence_attachment_created_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.HrAbsenceAttachments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_attachment_organization_id_fkey");
        });

        modelBuilder.Entity<HrAbsenceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hr_absence_type_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.State).WithMany(p => p.HrAbsenceTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_absence_type_state_id_fkey");
        });

        modelBuilder.Entity<HrEmployeeWorkSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hr_employee_work_schedule_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.HrEmployeeWorkScheduleCreatedByUsers).HasConstraintName("hr_employee_work_schedule_created_by_user_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.HrEmployeeWorkSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_employee_work_schedule_employee_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.HrEmployeeWorkSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_employee_work_schedule_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.HrEmployeeWorkSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_employee_work_schedule_state_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.HrEmployeeWorkScheduleUpdatedByUsers).HasConstraintName("hr_employee_work_schedule_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<HrEmployeeWorkScheduleDay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hr_employee_work_schedule_day_pkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.HrEmployeeWorkScheduleDays)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hr_employee_work_schedule_day_organization_id_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.HrEmployeeWorkScheduleDays).HasConstraintName("hr_employee_work_schedule_day_schedule_id_fkey");
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_record_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.IdempotencyRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("idempotency_record_organization_id_fkey");
        });

        modelBuilder.Entity<IntegrationCredential>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("integration_credential_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Organization).WithMany(p => p.IntegrationCredentials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("integration_credential_organization_id_fkey");
        });

        modelBuilder.Entity<InvInventoryAdjustmentDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_adjustment_doc_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvInventoryAdjustmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvInventoryAdjustmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.InvInventoryAdjustmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvInventoryAdjustmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvInventoryAdjustmentDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_adjustment_doc_table_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvInventoryAdjustmentDocTables).HasConstraintName("inv_inventory_adjustment_doc_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvInventoryAdjustmentDocTables).HasConstraintName("inv_inventory_adjustment_doc_table_product_table_id_fkey");
        });

        modelBuilder.Entity<InvInventoryAdjustmentLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_adjustment_line_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvInventoryAdjustmentLines).HasConstraintName("inv_inventory_adjustment_line_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvInventoryAdjustmentLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_line_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvInventoryAdjustmentLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_adjustment_line_unit_id_fkey");
        });

        modelBuilder.Entity<InvInventoryCountDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_count_doc_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NegativeAdjustmentDoc).WithMany(p => p.InvInventoryCountDocNegativeAdjustmentDocs).HasConstraintName("inv_inventory_count_doc_negative_adjustment_doc_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvInventoryCountDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_doc_organization_id_fkey");

            entity.HasOne(d => d.PositiveAdjustmentDoc).WithMany(p => p.InvInventoryCountDocPositiveAdjustmentDocs).HasConstraintName("inv_inventory_count_doc_positive_adjustment_doc_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvInventoryCountDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.InvInventoryCountDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvInventoryCountDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvInventoryCountDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_count_doc_table_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvInventoryCountDocTables).HasConstraintName("inv_inventory_count_doc_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvInventoryCountDocTables).HasConstraintName("inv_inventory_count_doc_table_product_table_id_fkey");
        });

        modelBuilder.Entity<InvInventoryCountLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_inventory_count_line_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvInventoryCountLines).HasConstraintName("inv_inventory_count_line_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvInventoryCountLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_line_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvInventoryCountLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_inventory_count_line_unit_id_fkey");
        });

        modelBuilder.Entity<InvOpeningInventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_opening_inventory_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.InvOpeningInventoryCancelledByUsers).HasConstraintName("inv_opening_inventory_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.Contract).WithMany(p => p.InvOpeningInventories).HasConstraintName("inv_opening_inventory_contract_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.InvOpeningInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_counterparty_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvOpeningInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_organization_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.InvOpeningInventoryPostedByUsers).HasConstraintName("inv_opening_inventory_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvOpeningInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.InvOpeningInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvOpeningInventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvOpeningInventoryProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_opening_inventory_product_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.DebitAccount).WithMany(p => p.InvOpeningInventoryProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_product_debit_account_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvOpeningInventoryProducts).HasConstraintName("inv_opening_inventory_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvOpeningInventoryProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvOpeningInventoryProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_product_unit_id_fkey");
        });

        modelBuilder.Entity<InvOpeningInventoryTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_opening_inventory_table_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.Owner).WithMany(p => p.InvOpeningInventoryTables).HasConstraintName("inv_opening_inventory_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvOpeningInventoryTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_opening_inventory_table_product_table_id_fkey");
        });

        modelBuilder.Entity<InvProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_pkey");

            entity.HasIndex(e => e.Gtin, "idx_inv_product_gtin").HasFilter("(gtin IS NOT NULL)");

            entity.HasIndex(e => e.Mxik, "ix_inv_product_mxik").HasFilter("(mxik IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_inv_product_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsPurchased).HasDefaultValue(true);
            entity.Property(e => e.IsSold).HasDefaultValue(true);

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_organization_id_fkey");

            entity.HasOne(d => d.ProductGroup).WithMany(p => p.InvProducts).HasConstraintName("inv_product_product_group_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_state_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_unit_id_fkey");
        });

        modelBuilder.Entity<InvProductGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_group_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_inv_product_group_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProductGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_group_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProductGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_group_state_id_fkey");
        });

        modelBuilder.Entity<InvProductPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_price_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StartDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Currency).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_organization_id_fkey");

            entity.HasOne(d => d.PriceType).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_price_type_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_product_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_state_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvProductPrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_unit_id_fkey");
        });

        modelBuilder.Entity<InvProductTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_table_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Product).WithMany(p => p.InvProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_table_product_id_fkey");
        });

        modelBuilder.Entity<InvRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_reg_balance_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.InvRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.InvRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_product_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvRegBalances).HasConstraintName("inv_reg_balance_product_table_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvTransferDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_transfer_doc_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.DestinationWarehouse).WithMany(p => p.InvTransferDocDestinationWarehouses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_destination_warehouse_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvTransferDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_organization_id_fkey");

            entity.HasOne(d => d.SourceWarehouse).WithMany(p => p.InvTransferDocSourceWarehouses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_source_warehouse_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvTransferDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.InvTransferDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_status_id_fkey");
        });

        modelBuilder.Entity<InvTransferDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_transfer_doc_table_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvTransferDocTables).HasConstraintName("inv_transfer_doc_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvTransferDocTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_doc_table_product_table_id_fkey");
        });

        modelBuilder.Entity<InvTransferLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_transfer_line_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.InvTransferLines).HasConstraintName("inv_transfer_line_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvTransferLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_line_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvTransferLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_transfer_line_unit_id_fkey");
        });

        modelBuilder.Entity<InvWarehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_warehouse_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_inv_warehouse_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.InvWarehouses).HasConstraintName("inv_warehouse_branch_id_fkey");

            entity.HasOne(d => d.BusinessPlace).WithMany(p => p.InvWarehouses).HasConstraintName("inv_warehouse_business_place_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvWarehouses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_organization_id_fkey");

            entity.HasOne(d => d.ResponsibleUser).WithMany(p => p.InvWarehouses).HasConstraintName("inv_warehouse_responsible_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvWarehouses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_state_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProduct>(entity =>
        {
            entity.HasKey(e => new { e.WarehouseId, e.ProductId }).HasName("inv_warehouse_product_pkey");

            entity.Property(e => e.AvailableQuantity).HasComputedColumnSql("((quantity - reserved_quantity) - blocked_quantity)", true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Product).WithMany(p => p.InvWarehouseProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvWarehouseProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_unit_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvWarehouseProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProductBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_warehouse_product_batch_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.WarehouseId, e.ProductId, e.ReceivedDate, e.Id }, "idx_inv_warehouse_product_batch_available").HasFilter("(remaining_quantity > (0)::numeric)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvWarehouseProductBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvWarehouseProductBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_product_id_fkey");

            entity.HasOne(d => d.ReceiptMovement).WithOne(p => p.InvWarehouseProductBatch)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_receipt_movement_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvWarehouseProductBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProductBatchAllocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_warehouse_product_batch_allocation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Batch).WithMany(p => p.InvWarehouseProductBatchAllocations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_allocation_batch_id_fkey");

            entity.HasOne(d => d.IssueMovement).WithMany(p => p.InvWarehouseProductBatchAllocations).HasConstraintName("inv_warehouse_product_batch_allocation_issue_movement_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProductBatchTable>(entity =>
        {
            entity.HasKey(e => new { e.BatchId, e.ProductTableId }).HasName("pk_inv_warehouse_product_batch_table");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Batch).WithMany(p => p.InvWarehouseProductBatchTables).HasConstraintName("inv_warehouse_product_batch_table_batch_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.InvWarehouseProductBatchTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_batch_table_product_table_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProductMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_warehouse_product_movement_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.InvWarehouseProductMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_movement_document_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvWarehouseProductMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_movement_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvWarehouseProductMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_movement_product_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvWarehouseProductMovements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_movement_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvWarehouseProductTable>(entity =>
        {
            entity.HasKey(e => e.ProductTableId).HasName("inv_warehouse_product_table_pkey");

            entity.Property(e => e.ProductTableId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ReceivedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ProductTable).WithOne(p => p.InvWarehouseProductTable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_table_product_table_id_fkey");

            entity.HasOne(d => d.ProductTableStatus).WithMany(p => p.InvWarehouseProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_table_product_table_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvWarehouseProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_product_table_warehouse_id_fkey");
        });

        modelBuilder.Entity<MarkingAggregation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_aggregation_pkey");

            entity.HasIndex(e => e.CrptDocumentId, "ux_marking_aggregation_crpt_document_id")
                .IsUnique()
                .HasFilter("(crpt_document_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BusinessPlace).WithMany(p => p.MarkingAggregations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_business_place_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingAggregations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_organization_id_fkey");

            entity.HasOne(d => d.ParentMarkingCode).WithMany(p => p.MarkingAggregations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aggregation_parent_marking_code_id_fkey");
        });

        modelBuilder.Entity<MarkingAslbelgiDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_aslbelgi_document_pkey");

            entity.HasIndex(e => e.ProviderDocumentId, "ux_marking_aslbelgi_document_provider_document_id")
                .IsUnique()
                .HasFilter("(provider_document_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingAslbelgiDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_aslbelgi_document_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingBusinessPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_business_place_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingBusinessPlaces)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_business_place_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_code_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Order).WithMany(p => p.MarkingCodes).HasConstraintName("marking_code_order_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_organization_id_fkey");

            entity.HasOne(d => d.OwnerCounterparty).WithMany(p => p.MarkingCodes).HasConstraintName("marking_code_owner_counterparty_id_fkey");

            entity.HasOne(d => d.ParentMarkingCode).WithMany(p => p.InverseParentMarkingCode).HasConstraintName("marking_code_parent_marking_code_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.MarkingCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_code_product_id_fkey");

            entity.HasOne(d => d.Utilization).WithMany(p => p.MarkingCodes).HasConstraintName("marking_code_utilization_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.MarkingCodes).HasConstraintName("marking_code_warehouse_id_fkey");
        });

        modelBuilder.Entity<MarkingDidoxDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_didox_document_pkey");

            entity.HasIndex(e => e.ProviderDocumentId, "ux_marking_didox_document_provider_document_id")
                .IsUnique()
                .HasFilter("(provider_document_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingDidoxDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_didox_document_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingEdocsDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_edocs_document_pkey");

            entity.HasIndex(e => e.ProviderDocumentId, "ux_marking_edocs_document_provider_document_id")
                .IsUnique()
                .HasFilter("(provider_document_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingEdocsDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_edocs_document_organization_id_fkey");
        });

        modelBuilder.Entity<MarkingOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_order_pkey");

            entity.HasIndex(e => e.CrptOrderId, "ux_marking_order_crpt_order_id")
                .IsUnique()
                .HasFilter("(crpt_order_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BusinessPlace).WithMany(p => p.MarkingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_business_place_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.MarkingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_order_product_id_fkey");
        });

        modelBuilder.Entity<MarkingUtilization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marking_utilization_pkey");

            entity.HasIndex(e => e.CrptDocumentId, "ux_marking_utilization_crpt_document_id")
                .IsUnique()
                .HasFilter("(crpt_document_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BusinessPlace).WithMany(p => p.MarkingUtilizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_utilization_business_place_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MarkingUtilizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("marking_utilization_organization_id_fkey");
        });

        modelBuilder.Entity<MoneyRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("money_reg_balance_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Currency).WithMany(p => p.MoneyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_currency_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.MoneyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.MoneyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MoneyRegBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_organization_id_fkey");
        });

        modelBuilder.Entity<OrgBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_bank_account_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_org_bank_account_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Bank).WithMany(p => p.OrgBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_bank_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.OrgBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgBankAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_state_id_fkey");
        });

        modelBuilder.Entity<OrgBranch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_branch_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.District).WithMany(p => p.OrgBranches).HasConstraintName("org_branch_district_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgBranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_branch_organization_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.OrgBranches).HasConstraintName("org_branch_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgBranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_branch_state_id_fkey");
        });

        modelBuilder.Entity<OrgClaimRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_claim_request_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::character varying");
        });

        modelBuilder.Entity<OrgDefault>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_defaults_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<OrgDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_department_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.OrgDepartments).HasConstraintName("org_department_branch_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgDepartments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_department_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgDepartments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_department_state_id_fkey");
        });

        modelBuilder.Entity<OrgOrganization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_organization_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.SetupStatus).HasDefaultValueSql("'not_started'::character varying");

            entity.HasOne(d => d.DefaultLanguage).WithMany(p => p.OrgOrganizations).HasConstraintName("org_organization_default_language_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.OrgOrganizations).HasConstraintName("org_organization_district_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.OrgOrganizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_organization_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgOrganizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_organization_state_id_fkey");
        });

        modelBuilder.Entity<OrgOrganizationConfig>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("org_organization_config_pkey");

            entity.Property(e => e.OrganizationId).ValueGeneratedNever();
            entity.Property(e => e.FiscalYearStartMonth).HasDefaultValue((short)1);
            entity.Property(e => e.InventoryValuationMethod).HasDefaultValueSql("'fifo'::character varying");

            entity.HasOne(d => d.Organization).WithOne(p => p.OrgOrganizationConfig)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_organization_config_organization_id_fkey");
        });

        modelBuilder.Entity<OrgPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_position_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgPositions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_position_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgPositions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_position_state_id_fkey");
        });

        modelBuilder.Entity<OrgSetupState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_setup_state_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.CurrentStep).HasDefaultValueSql("'organization'::character varying");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<OrgTaxSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_tax_settings_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.EffectiveFrom).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<OrgUserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_user_invitation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<OrganizationEdoProvider>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("organization_edo_provider_pkey");

            entity.Property(e => e.OrganizationId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithOne(p => p.OrganizationEdoProvider)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("organization_edo_provider_organization_id_fkey");
        });

        modelBuilder.Entity<PayComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_component_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.ExpenseAccount).WithMany(p => p.PayComponentExpenseAccounts).HasConstraintName("pay_component_expense_account_id_fkey");

            entity.HasOne(d => d.LiabilityAccount).WithMany(p => p.PayComponentLiabilityAccounts).HasConstraintName("pay_component_liability_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_component_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_component_state_id_fkey");
        });

        modelBuilder.Entity<PayEmployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_employee_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Pinfl }, "ux_pay_employee_org_pinfl")
                .IsUnique()
                .HasFilter("(pinfl IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PayEmployeeCreatedByUsers).HasConstraintName("pay_employee_created_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayEmployees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayEmployees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_state_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PayEmployeeUpdatedByUsers).HasConstraintName("pay_employee_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<PayEmployeeComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_employee_component_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.Component).WithMany(p => p.PayEmployeeComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_component_component_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayEmployeeComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_component_employee_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayEmployeeComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_component_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayEmployeeComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employee_component_state_id_fkey");
        });

        modelBuilder.Entity<PayEmployment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_employment_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.EmploymentRate).HasDefaultValue(1m);
            entity.Property(e => e.EmploymentType).HasDefaultValueSql("'PRIMARY'::character varying");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
            entity.Property(e => e.WeeklyHours).HasDefaultValue(40m);

            entity.HasOne(d => d.Currency).WithMany(p => p.PayEmployments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employment_currency_id_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.PayEmployments).HasConstraintName("pay_employment_department_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayEmployments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employment_employee_id_fkey");

            entity.HasOne(d => d.ExpenseAccount).WithMany(p => p.PayEmployments).HasConstraintName("pay_employment_expense_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayEmployments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employment_organization_id_fkey");

            entity.HasOne(d => d.Position).WithMany(p => p.PayEmployments).HasConstraintName("pay_employment_position_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayEmployments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_employment_state_id_fkey");
        });

        modelBuilder.Entity<PayPaymentBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_payment_batch_pkey");

            entity.HasIndex(e => e.BankOperationId, "ux_pay_payment_batch_bank_operation_id")
                .IsUnique()
                .HasFilter("(bank_operation_id IS NOT NULL)");

            entity.HasIndex(e => e.CashOperationId, "ux_pay_payment_batch_cash_operation_id")
                .IsUnique()
                .HasFilter("(cash_operation_id IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.PayPaymentBatches).HasConstraintName("pay_payment_batch_bank_account_id_fkey");

            entity.HasOne(d => d.BankOperation).WithOne(p => p.PayPaymentBatch).HasConstraintName("pay_payment_batch_bank_operation_id_fkey");

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.PayPaymentBatchCancelledByUsers).HasConstraintName("pay_payment_batch_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CashBox).WithMany(p => p.PayPaymentBatches).HasConstraintName("pay_payment_batch_cash_box_id_fkey");

            entity.HasOne(d => d.CashOperation).WithOne(p => p.PayPaymentBatch).HasConstraintName("pay_payment_batch_cash_operation_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PayPaymentBatchCreatedByUsers).HasConstraintName("pay_payment_batch_created_by_user_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.PayPaymentBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_currency_id_fkey");

            entity.HasOne(d => d.OffsetAccount).WithMany(p => p.PayPaymentBatchOffsetAccounts).HasConstraintName("pay_payment_batch_offset_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPaymentBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_organization_id_fkey");

            entity.HasOne(d => d.PayrollDoc).WithMany(p => p.PayPaymentBatches).HasConstraintName("pay_payment_batch_payroll_doc_id_fkey");

            entity.HasOne(d => d.Period).WithMany(p => p.PayPaymentBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_period_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.PayPaymentBatchPostedByUsers).HasConstraintName("pay_payment_batch_posted_by_user_id_fkey");

            entity.HasOne(d => d.SourceChartAccount).WithMany(p => p.PayPaymentBatchSourceChartAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_source_chart_account_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayPaymentBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PayPaymentBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_batch_status_id_fkey");
        });

        modelBuilder.Entity<PayPaymentLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_payment_line_pkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayPaymentLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_line_employee_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPaymentLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payment_line_organization_id_fkey");

            entity.HasOne(d => d.PaymentBatch).WithMany(p => p.PayPaymentLines).HasConstraintName("pay_payment_line_payment_batch_id_fkey");

            entity.HasOne(d => d.PayrollLine).WithMany(p => p.PayPaymentLines).HasConstraintName("pay_payment_line_payroll_line_id_fkey");
        });

        modelBuilder.Entity<PayPayrollCalcLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_payroll_calc_line_pkey");

            entity.HasOne(d => d.Component).WithMany(p => p.PayPayrollCalcLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_calc_line_component_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPayrollCalcLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_calc_line_organization_id_fkey");

            entity.HasOne(d => d.PayrollLine).WithMany(p => p.PayPayrollCalcLines).HasConstraintName("pay_payroll_calc_line_payroll_line_id_fkey");
        });

        modelBuilder.Entity<PayPayrollDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_payroll_doc_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.PeriodId }, "ux_pay_payroll_doc_regular_period")
                .IsUnique()
                .HasFilter("(((document_kind)::text = 'REGULAR'::text) AND (status_id <> 3) AND (state_id = 1))");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.DocumentKind).HasDefaultValueSql("'REGULAR'::character varying");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.PayPayrollDocCancelledByUsers).HasConstraintName("pay_payroll_doc_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CorrectionOfDoc).WithMany(p => p.InverseCorrectionOfDoc).HasConstraintName("pay_payroll_doc_correction_of_doc_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PayPayrollDocCreatedByUsers).HasConstraintName("pay_payroll_doc_created_by_user_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.PayPayrollDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_doc_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPayrollDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_doc_organization_id_fkey");

            entity.HasOne(d => d.Period).WithMany(p => p.PayPayrollDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_doc_period_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.PayPayrollDocPostedByUsers).HasConstraintName("pay_payroll_doc_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayPayrollDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PayPayrollDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_doc_status_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PayPayrollDocUpdatedByUsers).HasConstraintName("pay_payroll_doc_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<PayPayrollLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_payroll_line_pkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayPayrollLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_line_employee_id_fkey");

            entity.HasOne(d => d.Employment).WithMany(p => p.PayPayrollLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_line_employment_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPayrollLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_payroll_line_organization_id_fkey");

            entity.HasOne(d => d.PayrollDoc).WithMany(p => p.PayPayrollLines).HasConstraintName("pay_payroll_line_payroll_doc_id_fkey");
        });

        modelBuilder.Entity<PayPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_period_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'OPEN'::character varying");

            entity.HasOne(d => d.ClosedByUser).WithMany(p => p.PayPeriods).HasConstraintName("pay_period_closed_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayPeriods)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_period_organization_id_fkey");
        });

        modelBuilder.Entity<PayTimesheet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_timesheet_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.PeriodId }, "ux_pay_timesheet_active_period")
                .IsUnique()
                .HasFilter("((status_id <> 3) AND (state_id = 1))");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CancelledByUser).WithMany(p => p.PayTimesheetCancelledByUsers).HasConstraintName("pay_timesheet_cancelled_by_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PayTimesheetCreatedByUsers).HasConstraintName("pay_timesheet_created_by_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayTimesheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_organization_id_fkey");

            entity.HasOne(d => d.Period).WithMany(p => p.PayTimesheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_period_id_fkey");

            entity.HasOne(d => d.PostedByUser).WithMany(p => p.PayTimesheetPostedByUsers).HasConstraintName("pay_timesheet_posted_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PayTimesheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PayTimesheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_status_id_fkey");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PayTimesheetUpdatedByUsers).HasConstraintName("pay_timesheet_updated_by_user_id_fkey");
        });

        modelBuilder.Entity<PayTimesheetLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pay_timesheet_line_pkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayTimesheetLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_line_employee_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PayTimesheetLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay_timesheet_line_organization_id_fkey");

            entity.HasOne(d => d.Timesheet).WithMany(p => p.PayTimesheetLines).HasConstraintName("pay_timesheet_line_timesheet_id_fkey");
        });

        modelBuilder.Entity<PlatformTenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("platform_tenant_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<PurDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ExchangeRate).HasDefaultValue(1m);

            entity.HasOne(d => d.Contract).WithMany(p => p.PurDocs).HasConstraintName("pur_doc_contract_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_status_id_fkey");

            entity.HasOne(d => d.SupplierAccount).WithMany(p => p.PurDocs).HasConstraintName("pur_doc_supplier_account_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<PurDocProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_product_pkey");

            entity.HasIndex(e => e.ProductId, "ix_pur_doc_product_product_id").HasFilter("(product_id IS NOT NULL)");

            entity.HasOne(d => d.DebitAccount).WithMany(p => p.PurDocProductDebitAccounts).HasConstraintName("pur_doc_product_debit_account_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_unit_id_fkey");

            entity.HasOne(d => d.VatAccount).WithMany(p => p.PurDocProductVatAccounts).HasConstraintName("pur_doc_product_vat_account_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.PurDocProducts).HasConstraintName("pur_doc_product_vat_rate_id_fkey");
        });

        modelBuilder.Entity<PurDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_table_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.PurDocTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.PurDocTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_table_product_table_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.PurDocTables).HasConstraintName("pur_doc_table_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SaleCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_condition_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StartDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CostingMethod).WithMany(p => p.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_costing_method_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_state_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.SaleConditions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_condition_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SaleDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_pkey");

            entity.HasIndex(e => e.ContractId, "idx_sale_doc_contract_id").HasFilter("(contract_id IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ExchangeRate).HasDefaultValue(1m);

            entity.HasOne(d => d.Contract).WithMany(p => p.SaleDocs).HasConstraintName("sale_doc_contract_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_currency_id_fkey");

            entity.HasOne(d => d.CustomerAccount).WithMany(p => p.SaleDocCustomerAccounts).HasConstraintName("sale_doc_customer_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_status_id_fkey");

            entity.HasOne(d => d.VatAccount).WithMany(p => p.SaleDocVatAccounts).HasConstraintName("sale_doc_vat_account_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<SaleDocProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_product_pkey");

            entity.Property(e => e.UnitId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CostAccount).WithMany(p => p.SaleDocProductCostAccounts).HasConstraintName("sale_doc_product_cost_account_id_fkey");

            entity.HasOne(d => d.IncomeAccount).WithMany(p => p.SaleDocProductIncomeAccounts).HasConstraintName("sale_doc_product_income_account_id_fkey");

            entity.HasOne(d => d.InventoryAccount).WithMany(p => p.SaleDocProductInventoryAccounts).HasConstraintName("sale_doc_product_inventory_account_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.SaleDocProducts).HasConstraintName("sale_doc_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.SaleDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_product_unit_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.SaleDocProducts).HasConstraintName("sale_doc_product_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SaleDocProductBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_product_batch_pkey");

            entity.HasOne(d => d.SaleDocProduct).WithMany(p => p.SaleDocProductBatches).HasConstraintName("sale_doc_product_batch_sale_doc_product_id_fkey");

            entity.HasOne(d => d.WarehouseProductBatch).WithMany(p => p.SaleDocProductBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_product_batch_warehouse_product_batch_id_fkey");
        });

        modelBuilder.Entity<SaleDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_table_pkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.SaleDocTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_table_owner_id_fkey");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.SaleDocTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_table_product_table_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.SaleDocTables).HasConstraintName("sale_doc_table_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SaleShipmentDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_shipment_doc_pkey");

            entity.HasIndex(e => e.AcceptedUserId, "idx_sale_shipment_doc_accepted_user_id").HasFilter("(accepted_user_id IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.CounterpartyId, e.DocDate }, "idx_sale_shipment_doc_organization_counterparty_date")
                .IsDescending(false, false, true)
                .HasFilter("(counterparty_id IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.DocNumber }, "idx_sale_shipment_doc_organization_doc_number").HasFilter("(doc_number IS NOT NULL)");

            entity.HasIndex(e => e.SaleDocId, "idx_sale_shipment_doc_sale_doc_id").HasFilter("(sale_doc_id IS NOT NULL)");

            entity.HasIndex(e => e.SubmittedUserId, "idx_sale_shipment_doc_submitted_user_id").HasFilter("(submitted_user_id IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.DocDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AcceptedUser).WithMany(p => p.SaleShipmentDocAcceptedUsers).HasConstraintName("sale_shipment_doc_accepted_user_id_fkey");

            entity.HasOne(d => d.CancelledUser).WithMany(p => p.SaleShipmentDocCancelledUsers).HasConstraintName("sale_shipment_doc_cancelled_user_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.SaleShipmentDocs).HasConstraintName("sale_shipment_doc_counterparty_id_fkey");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.SaleShipmentDocCreatedUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_doc_created_user_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.SaleShipmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_doc_organization_id_fkey");

            entity.HasOne(d => d.SaleDoc).WithMany(p => p.SaleShipmentDocs).HasConstraintName("sale_shipment_doc_sale_doc_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.SaleShipmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_doc_status_id_fkey");

            entity.HasOne(d => d.SubmittedUser).WithMany(p => p.SaleShipmentDocSubmittedUsers).HasConstraintName("sale_shipment_doc_submitted_user_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SaleShipmentDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<SaleShipmentProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_shipment_product_pkey");

            entity.HasIndex(e => e.SaleDocProductId, "idx_sale_shipment_product_sale_doc_product_id").HasFilter("(sale_doc_product_id IS NOT NULL)");

            entity.HasIndex(e => e.SaleDocProductId, "uq_sale_shipment_product_sale_doc_product_id")
                .IsUnique()
                .HasFilter("(sale_doc_product_id IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Owner).WithMany(p => p.SaleShipmentProducts).HasConstraintName("sale_shipment_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleShipmentProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_product_product_id_fkey");

            entity.HasOne(d => d.SaleDocProduct).WithOne(p => p.SaleShipmentProduct).HasConstraintName("sale_shipment_product_sale_doc_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.SaleShipmentProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_product_unit_id_fkey");
        });

        modelBuilder.Entity<SaleShipmentProductBatch>(entity =>
        {
            entity.HasKey(e => new { e.ShipmentProductId, e.BatchId }).HasName("sale_shipment_product_batch_pkey");

            entity.HasOne(d => d.Batch).WithMany(p => p.SaleShipmentProductBatches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_product_batch_batch_id_fkey");

            entity.HasOne(d => d.ShipmentProduct).WithMany(p => p.SaleShipmentProductBatches).HasConstraintName("sale_shipment_product_batch_shipment_product_id_fkey");
        });

        modelBuilder.Entity<SaleShipmentTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_shipment_table_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ProductTable).WithMany(p => p.SaleShipmentTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_shipment_table_product_table_id_fkey");

            entity.HasOne(d => d.ShipmentProduct).WithMany(p => p.SaleShipmentTables).HasConstraintName("sale_shipment_table_shipment_product_id_fkey");
        });

        modelBuilder.Entity<SysAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_audit_log_pkey");

            entity.Property(e => e.ChangedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysEmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_email_verification_token_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysModule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_module_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasConstraintName("sys_module_parent_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysModules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_module_state_id_fkey");

            entity.HasOne(d => d.SubGroup).WithMany(p => p.SysModules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_module_sub_group_id_fkey");
        });

        modelBuilder.Entity<SysModuleSubGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_module_sub_group_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_notification_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<SysNotificationDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_notification_delivery_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysNotificationRead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_notification_read_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ReadAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysPasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_password_reset_token_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_refresh_token_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_role_pkey");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_sys_role_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "ux_sys_role_organization_code")
                .IsUnique()
                .HasFilter("(is_system = false)");

            entity.HasIndex(e => e.Code, "ux_sys_role_system_code")
                .IsUnique()
                .HasFilter("(is_system = true)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.SysRoles).HasConstraintName("sys_role_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_state_id_fkey");
        });

        modelBuilder.Entity<SysRoleModule>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ModuleId }).HasName("sys_role_module_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Module).WithMany(p => p.SysRoleModules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_module_module_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.SysRoleModules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_module_role_id_fkey");
        });

        modelBuilder.Entity<SysSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_setting_pkey");

            entity.HasIndex(e => e.Code, "ux_sys_setting_global_code")
                .IsUnique()
                .HasFilter("(organization_id IS NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "ux_sys_setting_org_code")
                .IsUnique()
                .HasFilter("(organization_id IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsEditable).HasDefaultValue(true);
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_user_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.TenantId).HasDefaultValue(1);
            entity.Property(e => e.UserKindId).HasDefaultValue((short)3);

            entity.HasOne(d => d.Language).WithMany(p => p.SysUsers).HasConstraintName("sys_user_language_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_state_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.SysUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_tenant_id_fkey");

            entity.HasOne(d => d.UserKind).WithMany(p => p.SysUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_user_kind_id_fkey");
        });

        modelBuilder.Entity<SysUserKind>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_user_kind_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SysUserKindTranslation>(entity =>
        {
            entity.HasKey(e => new { e.UserKindId, e.LanguageId }).HasName("sys_user_kind_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.SysUserKindTranslations).HasConstraintName("sys_user_kind_translation_language_id_fkey");

            entity.HasOne(d => d.UserKind).WithMany(p => p.SysUserKindTranslations).HasConstraintName("sys_user_kind_translation_user_kind_id_fkey");
        });

        modelBuilder.Entity<SysUserOrganization>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.OrganizationId }).HasName("sys_user_organization_pkey");

            entity.HasIndex(e => e.UserId, "idx_sys_user_organization_default_user")
                .IsUnique()
                .HasFilter("(is_default = true)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.SysUserOrganizations).HasConstraintName("sys_user_organization_organization_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.SysUserOrganizations).HasConstraintName("sys_user_organization_role_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysUserOrganizations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_organization_state_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.SysUserOrganization).HasConstraintName("sys_user_organization_user_id_fkey");
        });
        modelBuilder.HasSequence("contract_number_seq").StartsAt(100000001L);
        modelBuilder.HasSequence("doc_number_bank_operation_seq").StartsAt(100000001L);
        modelBuilder.HasSequence("doc_number_cash_operation_seq").StartsAt(100000001L);
        modelBuilder.HasSequence("doc_number_purchase_seq").StartsAt(100000001L);
        modelBuilder.HasSequence("doc_number_sale_seq").StartsAt(100000001L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
