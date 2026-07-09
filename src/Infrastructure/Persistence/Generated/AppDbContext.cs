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

    public virtual DbSet<AccAccountResolveRule> AccAccountResolveRules { get; set; }

    public virtual DbSet<AccAccountType> AccAccountTypes { get; set; }

    public virtual DbSet<AccAccountingPeriod> AccAccountingPeriods { get; set; }

    public virtual DbSet<AccAccountingPolicy> AccAccountingPolicies { get; set; }

    public virtual DbSet<AccChartAccount> AccChartAccounts { get; set; }

    public virtual DbSet<AccChartAccountPreset> AccChartAccountPresets { get; set; }

    public virtual DbSet<AccChartAccountPresetAccount> AccChartAccountPresetAccounts { get; set; }

    public virtual DbSet<AccChartAccountPresetAccountSubkonto> AccChartAccountPresetAccountSubkontos { get; set; }

    public virtual DbSet<AccChartAccountPresetAccountTranslation> AccChartAccountPresetAccountTranslations { get; set; }

    public virtual DbSet<AccChartAccountPresetTranslation> AccChartAccountPresetTranslations { get; set; }

    public virtual DbSet<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; }

    public virtual DbSet<AccPaymentPurpose> AccPaymentPurposes { get; set; }

    public virtual DbSet<AccPaymentPurposeTranslation> AccPaymentPurposeTranslations { get; set; }

    public virtual DbSet<AccPostingAlias> AccPostingAliases { get; set; }

    public virtual DbSet<AccPostingAliasTranslation> AccPostingAliasTranslations { get; set; }

    public virtual DbSet<AccPostingBatch> AccPostingBatches { get; set; }

    public virtual DbSet<AccPostingRule> AccPostingRules { get; set; }

    public virtual DbSet<AccPostingRuleLine> AccPostingRuleLines { get; set; }

    public virtual DbSet<AccRegEntry> AccRegEntries { get; set; }

    public virtual DbSet<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; }

    public virtual DbSet<AccSubkontoType> AccSubkontoTypes { get; set; }

    public virtual DbSet<BankOperation> BankOperations { get; set; }

    public virtual DbSet<BankOperationLine> BankOperationLines { get; set; }

    public virtual DbSet<CashBox> CashBoxes { get; set; }

    public virtual DbSet<CashOperation> CashOperations { get; set; }

    public virtual DbSet<CmnBank> CmnBanks { get; set; }

    public virtual DbSet<CmnContract> CmnContracts { get; set; }

    public virtual DbSet<CmnContractType> CmnContractTypes { get; set; }

    public virtual DbSet<CmnCostingMethod> CmnCostingMethods { get; set; }

    public virtual DbSet<CmnCounterpartyType> CmnCounterpartyTypes { get; set; }

    public virtual DbSet<CmnCurrency> CmnCurrencies { get; set; }

    public virtual DbSet<CmnCurrencyRate> CmnCurrencyRates { get; set; }

    public virtual DbSet<CmnCurrencyRevaluation> CmnCurrencyRevaluations { get; set; }

    public virtual DbSet<CmnCurrencyRevaluationLine> CmnCurrencyRevaluationLines { get; set; }

    public virtual DbSet<CmnDistrict> CmnDistricts { get; set; }

    public virtual DbSet<CmnDocumentSequence> CmnDocumentSequences { get; set; }

    public virtual DbSet<CmnDocumentStatus> CmnDocumentStatuses { get; set; }

    public virtual DbSet<CmnDocumentType> CmnDocumentTypes { get; set; }

    public virtual DbSet<CmnFaAssetStatus> CmnFaAssetStatuses { get; set; }

    public virtual DbSet<CmnFaDepreciationMethod> CmnFaDepreciationMethods { get; set; }

    public virtual DbSet<CmnFaGroup> CmnFaGroups { get; set; }

    public virtual DbSet<CmnFaOkof> CmnFaOkofs { get; set; }

    public virtual DbSet<CmnInventoryAdjustmentType> CmnInventoryAdjustmentTypes { get; set; }

    public virtual DbSet<CmnLanguage> CmnLanguages { get; set; }

    public virtual DbSet<CmnNotificationType> CmnNotificationTypes { get; set; }

    public virtual DbSet<CmnOperationType> CmnOperationTypes { get; set; }

    public virtual DbSet<CmnPaymentType> CmnPaymentTypes { get; set; }

    public virtual DbSet<CmnPriceRoundingMethod> CmnPriceRoundingMethods { get; set; }

    public virtual DbSet<CmnPricingCondition> CmnPricingConditions { get; set; }

    public virtual DbSet<CmnPricingMethod> CmnPricingMethods { get; set; }

    public virtual DbSet<CmnProductPriceType> CmnProductPriceTypes { get; set; }

    public virtual DbSet<CmnProductTableStatus> CmnProductTableStatuses { get; set; }

    public virtual DbSet<CmnProductType> CmnProductTypes { get; set; }

    public virtual DbSet<CmnProductTypeTranslation> CmnProductTypeTranslations { get; set; }

    public virtual DbSet<CmnRegion> CmnRegions { get; set; }

    public virtual DbSet<CmnState> CmnStates { get; set; }

    public virtual DbSet<CmnTaxType> CmnTaxTypes { get; set; }

    public virtual DbSet<CmnTranslation> CmnTranslations { get; set; }

    public virtual DbSet<CmnUnit> CmnUnits { get; set; }

    public virtual DbSet<CmnVatRate> CmnVatRates { get; set; }

    public virtual DbSet<CounterpartyAccountPaymentPurposeHint> CounterpartyAccountPaymentPurposeHints { get; set; }

    public virtual DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }

    public virtual DbSet<CounterpartyCard> CounterpartyCards { get; set; }

    public virtual DbSet<CounterpartyContact> CounterpartyContacts { get; set; }

    public virtual DbSet<CounterpartyRegBalance> CounterpartyRegBalances { get; set; }

    public virtual DbSet<FaAsset> FaAssets { get; set; }

    public virtual DbSet<FaDepreciationRun> FaDepreciationRuns { get; set; }

    public virtual DbSet<FaDepreciationRunLine> FaDepreciationRunLines { get; set; }

    public virtual DbSet<FaDisposalDoc> FaDisposalDocs { get; set; }

    public virtual DbSet<FaDisposalDocLine> FaDisposalDocLines { get; set; }

    public virtual DbSet<FaMovementDoc> FaMovementDocs { get; set; }

    public virtual DbSet<FaMovementDocLine> FaMovementDocLines { get; set; }

    public virtual DbSet<FaReceiptDoc> FaReceiptDocs { get; set; }

    public virtual DbSet<FaReceiptDocAsset> FaReceiptDocAssets { get; set; }

    public virtual DbSet<FaReceiptDocLine> FaReceiptDocLines { get; set; }

    public virtual DbSet<FaRevaluationDoc> FaRevaluationDocs { get; set; }

    public virtual DbSet<FaRevaluationDocLine> FaRevaluationDocLines { get; set; }

    public virtual DbSet<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; }

    public virtual DbSet<InvInventoryAdjustmentDocTable> InvInventoryAdjustmentDocTables { get; set; }

    public virtual DbSet<InvInventoryAdjustmentLine> InvInventoryAdjustmentLines { get; set; }

    public virtual DbSet<InvInventoryCountDoc> InvInventoryCountDocs { get; set; }

    public virtual DbSet<InvInventoryCountDocTable> InvInventoryCountDocTables { get; set; }

    public virtual DbSet<InvInventoryCountLine> InvInventoryCountLines { get; set; }

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

    public virtual DbSet<PlatformTenant> PlatformTenants { get; set; }

    public virtual DbSet<PurDoc> PurDocs { get; set; }

    public virtual DbSet<PurDocProduct> PurDocProducts { get; set; }

    public virtual DbSet<PurDocTable> PurDocTables { get; set; }

    public virtual DbSet<SaleCondition> SaleConditions { get; set; }

    public virtual DbSet<SaleDoc> SaleDocs { get; set; }

    public virtual DbSet<SaleDocProduct> SaleDocProducts { get; set; }

    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }

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

    public virtual DbSet<SysUserOrganization> SysUserOrganizations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccAccountResolveRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_account_resolve_rule_pkey");

            entity.HasOne(d => d.Account).WithMany(p => p.AccAccountResolveRules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_resolve_rule_account_id_fkey");

            entity.HasOne(d => d.Policy).WithMany(p => p.AccAccountResolveRules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_resolve_rule_policy_id_fkey");
        });

        modelBuilder.Entity<AccAccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_account_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.State).WithMany(p => p.AccAccountTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_type_state_id_fkey");
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
            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Account).WithMany(p => p.AccChartAccountSubkontos).HasConstraintName("acc_chart_account_subkonto_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccChartAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_state_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccChartAccountSubkontos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccPaymentPurpose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_payment_purpose_pkey");

            entity.Property(e => e.RequiresCounterparty).HasDefaultValue(true);

            entity.HasOne(d => d.Alias).WithMany(p => p.AccPaymentPurposes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_payment_purpose_alias_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.AccPaymentPurposes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_payment_purpose_operation_type_id_fkey");
        });

        modelBuilder.Entity<AccPaymentPurposeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.PaymentPurposeId, e.LanguageId }).HasName("acc_payment_purpose_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccPaymentPurposeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_payment_purpose_translation_language_id_fkey");

            entity.HasOne(d => d.PaymentPurpose).WithMany(p => p.AccPaymentPurposeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_payment_purpose_translation_payment_purpose_id_fkey");
        });

        modelBuilder.Entity<AccPostingAlias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_alias_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<AccPostingAliasTranslation>(entity =>
        {
            entity.HasKey(e => new { e.PostingAliasId, e.LanguageId }).HasName("acc_posting_alias_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.AccPostingAliasTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_alias_translation_language_id_fkey");

            entity.HasOne(d => d.PostingAlias).WithMany(p => p.AccPostingAliasTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_alias_translation_posting_alias_id_fkey");
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

        modelBuilder.Entity<AccPostingRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_rule_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<AccPostingRuleLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_rule_line_pkey");

            entity.Property(e => e.IsOptional).HasDefaultValue(true);

            entity.HasOne(d => d.CreditAlias).WithMany(p => p.AccPostingRuleLineCreditAliases)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acc_posting_rule_line_credit_alias");

            entity.HasOne(d => d.DebitAlias).WithMany(p => p.AccPostingRuleLineDebitAliases)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acc_posting_rule_line_debit_alias");

            entity.HasOne(d => d.Template).WithMany(p => p.AccPostingRuleLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_rule_line_template_id_fkey");
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

        modelBuilder.Entity<BankOperation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bank_operation_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.ExchangeRate).HasDefaultValue(1m);
            entity.Property(e => e.PaymentPurposeId).HasDefaultValue((short)1);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_bank_account_id_fkey");

            entity.HasOne(d => d.Contract).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_contract_id_fkey");

            entity.HasOne(d => d.CounterpartyBankAccount).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_counterparty_bank_account_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_currency_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentPurpose).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bank_operation_payment_purpose");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.BankOperations).HasConstraintName("bank_operation_payment_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.BankOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_status_id_fkey");
        });

        modelBuilder.Entity<BankOperationLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bank_operation_line_pkey");

            entity.HasOne(d => d.BankOperation).WithMany(p => p.BankOperationLines).HasConstraintName("bank_operation_line_bank_operation_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.BankOperationLines).HasConstraintName("bank_operation_line_counterparty_id_fkey");

            entity.HasOne(d => d.PaymentPurpose).WithMany(p => p.BankOperationLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_line_payment_purpose_id_fkey");
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

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CashOperations).HasConstraintName("cash_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_currency_id_fkey");

            entity.HasOne(d => d.DestinationCashBox).WithMany(p => p.CashOperationDestinationCashBoxes).HasConstraintName("cash_operation_destination_cash_box_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentPurpose).WithMany(p => p.CashOperations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cash_operation_payment_purpose");

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

        modelBuilder.Entity<CmnCostingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_costing_method_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnCounterpartyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_counterparty_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnCounterpartyTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_counterparty_type_state_id_fkey");
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

        modelBuilder.Entity<CmnDistrict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_district_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CmnDocumentSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_sequence_pkey");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.Padding).HasDefaultValue((short)5);
            entity.Property(e => e.ResetPeriod).HasDefaultValueSql("'yearly'::character varying");
            entity.Property(e => e.StateId).HasDefaultValue((short)1);
        });

        modelBuilder.Entity<CmnDocumentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_status_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnDocumentStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_status_state_id_fkey");
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

        modelBuilder.Entity<CmnPaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_payment_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.CmnPaymentTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_payment_type_state_id_fkey");
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

        modelBuilder.Entity<CmnProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_product_type_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CmnProductTypeTranslation>(entity =>
        {
            entity.HasKey(e => new { e.ProductTypeId, e.LanguageId }).HasName("cmn_product_type_translation_pkey");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnProductTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_product_type_translation_language_id_fkey");

            entity.HasOne(d => d.ProductType).WithMany(p => p.CmnProductTypeTranslations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_product_type_translation_product_type_id_fkey");
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

        modelBuilder.Entity<CounterpartyAccountPaymentPurposeHint>(entity =>
        {
            entity.HasKey(e => new { e.CounterpartyBankAccountId, e.PaymentPurposeId }).HasName("counterparty_account_payment_purpose_hint_pkey");

            entity.Property(e => e.LastUsedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UsageCount).HasDefaultValue(1);

            entity.HasOne(d => d.CounterpartyBankAccount).WithMany(p => p.CounterpartyAccountPaymentPurposeHints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_account_payment__counterparty_bank_account_id_fkey");

            entity.HasOne(d => d.PaymentPurpose).WithMany(p => p.CounterpartyAccountPaymentPurposeHints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_account_payment_purpose_hi_payment_purpose_id_fkey");
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

        modelBuilder.Entity<FaAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_asset_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Department).WithMany(p => p.FaAssets).HasConstraintName("fa_asset_department_id_fkey");

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

            entity.HasOne(d => d.DepreciationRun).WithMany(p => p.FaDepreciationRunLines).HasConstraintName("fa_depreciation_run_line_depreciation_run_id_fkey");

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
        });

        modelBuilder.Entity<FaDisposalDocLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_disposal_doc_line_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.DisposalDoc).WithMany(p => p.FaDisposalDocLines).HasConstraintName("fa_disposal_doc_line_disposal_doc_id_fkey");

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaDisposalDocLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_disposal_doc_line_fa_asset_id_fkey");
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

            entity.HasOne(d => d.State).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.FaReceiptDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_receipt_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.FaReceiptDocs).HasConstraintName("fa_receipt_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<FaReceiptDocAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fa_receipt_doc_asset_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.Department).WithMany(p => p.FaReceiptDocAssets).HasConstraintName("fa_receipt_doc_asset_department_id_fkey");

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

            entity.HasOne(d => d.Owner).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_owner_id_fkey");

            entity.HasOne(d => d.SourceProduct).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_source_product_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.FaReceiptDocLines).HasConstraintName("fa_receipt_doc_line_vat_rate_id_fkey");
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

            entity.HasOne(d => d.FaAsset).WithMany(p => p.FaRevaluationDocLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fa_revaluation_doc_line_fa_asset_id_fkey");

            entity.HasOne(d => d.RevaluationDoc).WithMany(p => p.FaRevaluationDocLines).HasConstraintName("fa_revaluation_doc_line_revaluation_doc_id_fkey");
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

        modelBuilder.Entity<InvProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_pkey");

            entity.HasIndex(e => e.Mxik, "ix_inv_product_mxik").HasFilter("(mxik IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "uidx_inv_product_org_code")
                .IsUnique()
                .HasFilter("(code IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.IsPurchased).HasDefaultValue(true);
            entity.Property(e => e.IsSold).HasDefaultValue(true);
            entity.Property(e => e.ProductTypeId).HasDefaultValue((short)1);

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_organization_id_fkey");

            entity.HasOne(d => d.ProductGroup).WithMany(p => p.InvProducts).HasConstraintName("inv_product_product_group_id_fkey");

            entity.HasOne(d => d.ProductType).WithMany(p => p.InvProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_product_type_id_fkey");

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

            entity.HasIndex(e => new { e.OrganizationId, e.MarkingNumber }, "ux_inv_product_table_org_marking")
                .IsUnique()
                .HasFilter("(marking_number IS NOT NULL)");

            entity.HasIndex(e => new { e.OrganizationId, e.SerialNumber }, "ux_inv_product_table_org_serial")
                .IsUnique()
                .HasFilter("(serial_number IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()");
            entity.Property(e => e.StatusId).HasDefaultValue((short)1);

            entity.HasOne(d => d.CurrentWarehouse).WithMany(p => p.InvProductTables).HasConstraintName("inv_product_table_current_warehouse_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_table_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_table_product_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_table_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.InvProductTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_table_status_id_fkey");
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

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<PurDocProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_product_pkey");

            entity.HasIndex(e => e.ProductId, "ix_pur_doc_product_product_id").HasFilter("(product_id IS NOT NULL)");

            entity.HasOne(d => d.Owner).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.PurDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_product_unit_id_fkey");

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

            entity.HasOne(d => d.Organization).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SaleDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<SaleDocProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_product_pkey");

            entity.Property(e => e.UnitId).HasDefaultValue((short)1);

            entity.HasOne(d => d.Owner).WithMany(p => p.SaleDocProducts).HasConstraintName("sale_doc_product_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_product_product_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.SaleDocProducts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_product_unit_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.SaleDocProducts).HasConstraintName("sale_doc_product_vat_rate_id_fkey");
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

            entity.HasOne(d => d.Language).WithMany(p => p.SysUsers).HasConstraintName("sys_user_language_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.SysUsers).HasConstraintName("sys_user_organization_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.SysUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_role_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_state_id_fkey");
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
