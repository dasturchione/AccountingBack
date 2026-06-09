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

    public virtual DbSet<AccChartAccount> AccChartAccounts { get; set; }

    public virtual DbSet<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; }

    public virtual DbSet<AccPostingRule> AccPostingRules { get; set; }

    public virtual DbSet<AccPostingRuleLine> AccPostingRuleLines { get; set; }

    public virtual DbSet<AccRegEntry> AccRegEntries { get; set; }

    public virtual DbSet<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; }

    public virtual DbSet<AccSubkontoType> AccSubkontoTypes { get; set; }

    public virtual DbSet<BankOperation> BankOperations { get; set; }

    public virtual DbSet<CashBox> CashBoxes { get; set; }

    public virtual DbSet<CashOperation> CashOperations { get; set; }

    public virtual DbSet<CmnBank> CmnBanks { get; set; }

    public virtual DbSet<CmnCounterpartyType> CmnCounterpartyTypes { get; set; }

    public virtual DbSet<CmnCurrency> CmnCurrencies { get; set; }

    public virtual DbSet<CmnDistrict> CmnDistricts { get; set; }

    public virtual DbSet<CmnDocumentStatus> CmnDocumentStatuses { get; set; }

    public virtual DbSet<CmnDocumentType> CmnDocumentTypes { get; set; }

    public virtual DbSet<CmnLanguage> CmnLanguages { get; set; }

    public virtual DbSet<CmnOperationType> CmnOperationTypes { get; set; }

    public virtual DbSet<CmnPaymentType> CmnPaymentTypes { get; set; }

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

    public virtual DbSet<InvProduct> InvProducts { get; set; }

    public virtual DbSet<InvProductGroup> InvProductGroups { get; set; }

    public virtual DbSet<InvProductPrice> InvProductPrices { get; set; }

    public virtual DbSet<InvRegBalance> InvRegBalances { get; set; }

    public virtual DbSet<InvWarehouse> InvWarehouses { get; set; }

    public virtual DbSet<MoneyRegBalance> MoneyRegBalances { get; set; }

    public virtual DbSet<OrgBankAccount> OrgBankAccounts { get; set; }

    public virtual DbSet<OrgBranch> OrgBranches { get; set; }

    public virtual DbSet<OrgDepartment> OrgDepartments { get; set; }

    public virtual DbSet<OrgOrganization> OrgOrganizations { get; set; }

    public virtual DbSet<OrgPosition> OrgPositions { get; set; }

    public virtual DbSet<PurDoc> PurDocs { get; set; }

    public virtual DbSet<PurDocTable> PurDocTables { get; set; }

    public virtual DbSet<SaleDoc> SaleDocs { get; set; }

    public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }

    public virtual DbSet<SysModule> SysModules { get; set; }

    public virtual DbSet<SysModuleSubGroup> SysModuleSubGroups { get; set; }

    public virtual DbSet<SysRole> SysRoles { get; set; }

    public virtual DbSet<SysRoleModule> SysRoleModules { get; set; }

    public virtual DbSet<SysUser> SysUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccAccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_account_type_pkey");

            entity.ToTable("acc_account_type");

            entity.HasIndex(e => e.Code, "idx_acc_account_type_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_acc_account_type_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.AccAccountTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_account_type_state_id_fkey");
        });

        modelBuilder.Entity<AccChartAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_pkey");

            entity.ToTable("acc_chart_account");

            entity.HasIndex(e => e.AccountTypeId, "idx_acc_chart_account_account_type_id");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_acc_chart_account_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_acc_chart_account_organization_id");

            entity.HasIndex(e => e.ParentId, "idx_acc_chart_account_parent_id");

            entity.HasIndex(e => e.StateId, "idx_acc_chart_account_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountTypeId).HasColumnName("account_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsCurrency).HasColumnName("is_currency");
            entity.Property(e => e.IsGroup).HasColumnName("is_group");
            entity.Property(e => e.IsQuantity).HasColumnName("is_quantity");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.AccountType).WithMany(p => p.AccChartAccounts)
                .HasForeignKey(d => d.AccountTypeId)
                .HasConstraintName("acc_chart_account_account_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccChartAccounts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_organization_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("acc_chart_account_parent_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccounts)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_state_id_fkey");
        });

        modelBuilder.Entity<AccChartAccountSubkonto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_chart_account_subkonto_pkey");

            entity.ToTable("acc_chart_account_subkonto");

            entity.HasIndex(e => e.AccountId, "idx_acc_chart_account_subkonto_account_id");

            entity.HasIndex(e => e.OrganizationId, "idx_acc_chart_account_subkonto_organization_id");

            entity.HasIndex(e => e.SubkontoTypeId, "idx_acc_chart_account_subkonto_type_id");

            entity.HasIndex(e => new { e.OrganizationId, e.AccountId, e.SubkontoTypeId }, "idx_acc_chart_account_subkonto_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(true)
                .HasColumnName("is_required");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.SubkontoTypeId).HasColumnName("subkonto_type_id");

            entity.HasOne(d => d.Account).WithMany(p => p.AccChartAccountSubkontos)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("acc_chart_account_subkonto_account_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccChartAccountSubkontos)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccChartAccountSubkontos)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_state_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccChartAccountSubkontos)
                .HasForeignKey(d => d.SubkontoTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_chart_account_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccPostingRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_rule_pkey");

            entity.ToTable("acc_posting_rule");

            entity.HasIndex(e => e.DocumentTypeId, "idx_acc_posting_rule_document_type_id");

            entity.HasIndex(e => e.OperationTypeId, "idx_acc_posting_rule_operation_type_id");

            entity.HasIndex(e => new { e.OrganizationId, e.DocumentTypeId, e.OperationTypeId, e.Code }, "idx_acc_posting_rule_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.AccPostingRules)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_rule_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.AccPostingRules)
                .HasForeignKey(d => d.OperationTypeId)
                .HasConstraintName("acc_posting_rule_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccPostingRules)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("acc_posting_rule_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccPostingRules)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_rule_state_id_fkey");
        });

        modelBuilder.Entity<AccPostingRuleLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_posting_rule_line_pkey");

            entity.ToTable("acc_posting_rule_line");

            entity.HasIndex(e => e.CreditAccountId, "idx_acc_posting_rule_line_credit_account_id");

            entity.HasIndex(e => e.DebitAccountId, "idx_acc_posting_rule_line_debit_account_id");

            entity.HasIndex(e => e.RuleId, "idx_acc_posting_rule_line_rule_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountSource)
                .HasMaxLength(100)
                .HasColumnName("amount_source");
            entity.Property(e => e.ContentTemplate)
                .HasMaxLength(1000)
                .HasColumnName("content_template");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CreditAccountId).HasColumnName("credit_account_id");
            entity.Property(e => e.DebitAccountId).HasColumnName("debit_account_id");
            entity.Property(e => e.QuantitySource)
                .HasMaxLength(100)
                .HasColumnName("quantity_source");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.CreditAccount).WithMany(p => p.AccPostingRuleLineCreditAccounts)
                .HasForeignKey(d => d.CreditAccountId)
                .HasConstraintName("acc_posting_rule_line_credit_account_id_fkey");

            entity.HasOne(d => d.DebitAccount).WithMany(p => p.AccPostingRuleLineDebitAccounts)
                .HasForeignKey(d => d.DebitAccountId)
                .HasConstraintName("acc_posting_rule_line_debit_account_id_fkey");

            entity.HasOne(d => d.Rule).WithMany(p => p.AccPostingRuleLines)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("acc_posting_rule_line_rule_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.AccPostingRuleLines)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_posting_rule_line_state_id_fkey");
        });

        modelBuilder.Entity<AccRegEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_reg_entry_pkey");

            entity.ToTable("acc_reg_entry");

            entity.HasIndex(e => e.CreditAccountId, "idx_acc_reg_entry_credit_account_id");

            entity.HasIndex(e => e.CurrencyId, "idx_acc_reg_entry_currency_id");

            entity.HasIndex(e => e.DebitAccountId, "idx_acc_reg_entry_debit_account_id");

            entity.HasIndex(e => e.DocDate, "idx_acc_reg_entry_doc_date");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "idx_acc_reg_entry_document");

            entity.HasIndex(e => e.JournalNumber, "idx_acc_reg_entry_journal_number");

            entity.HasIndex(e => e.OperationTypeId, "idx_acc_reg_entry_operation_type_id");

            entity.HasIndex(e => e.OrganizationId, "idx_acc_reg_entry_organization_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CreditAccountId).HasColumnName("credit_account_id");
            entity.Property(e => e.CreditQuantity)
                .HasPrecision(18, 3)
                .HasColumnName("credit_quantity");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DebitAccountId).HasColumnName("debit_account_id");
            entity.Property(e => e.DebitQuantity)
                .HasPrecision(18, 3)
                .HasColumnName("debit_quantity");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.JournalNumber)
                .HasMaxLength(100)
                .HasColumnName("journal_number");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");

            entity.HasOne(d => d.CreditAccount).WithMany(p => p.AccRegEntryCreditAccounts)
                .HasForeignKey(d => d.CreditAccountId)
                .HasConstraintName("acc_reg_entry_credit_account_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.AccRegEntries)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_currency_id_fkey");

            entity.HasOne(d => d.DebitAccount).WithMany(p => p.AccRegEntryDebitAccounts)
                .HasForeignKey(d => d.DebitAccountId)
                .HasConstraintName("acc_reg_entry_debit_account_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.AccRegEntries)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.AccRegEntries)
                .HasForeignKey(d => d.OperationTypeId)
                .HasConstraintName("acc_reg_entry_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.AccRegEntries)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_organization_id_fkey");
        });

        modelBuilder.Entity<AccRegEntrySubkonto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_reg_entry_subkonto_pkey");

            entity.ToTable("acc_reg_entry_subkonto");

            entity.HasIndex(e => new { e.SubkontoTypeId, e.EntityId }, "idx_acc_reg_entry_subkonto_entity");

            entity.HasIndex(e => e.EntryId, "idx_acc_reg_entry_subkonto_entry_id");

            entity.HasIndex(e => e.Side, "idx_acc_reg_entry_subkonto_side");

            entity.HasIndex(e => e.SubkontoTypeId, "idx_acc_reg_entry_subkonto_type_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DisplayValue)
                .HasMaxLength(500)
                .HasColumnName("display_value");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.Side)
                .HasMaxLength(2)
                .HasColumnName("side");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.SubkontoTypeId).HasColumnName("subkonto_type_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.AccRegEntrySubkontos)
                .HasForeignKey(d => d.EntryId)
                .HasConstraintName("acc_reg_entry_subkonto_entry_id_fkey");

            entity.HasOne(d => d.SubkontoType).WithMany(p => p.AccRegEntrySubkontos)
                .HasForeignKey(d => d.SubkontoTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_reg_entry_subkonto_subkonto_type_id_fkey");
        });

        modelBuilder.Entity<AccSubkontoType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("acc_subkonto_type_pkey");

            entity.ToTable("acc_subkonto_type");

            entity.HasIndex(e => e.Code, "idx_acc_subkonto_type_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_acc_subkonto_type_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.SourceTable)
                .HasMaxLength(100)
                .HasColumnName("source_table");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.AccSubkontoTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acc_subkonto_type_state_id_fkey");
        });

        modelBuilder.Entity<BankOperation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bank_operation_pkey");

            entity.ToTable("bank_operation");

            entity.HasIndex(e => e.BankAccountId, "idx_bank_operation_bank_account_id");

            entity.HasIndex(e => e.CounterpartyId, "idx_bank_operation_counterparty_id");

            entity.HasIndex(e => e.DocDate, "idx_bank_operation_doc_date");

            entity.HasIndex(e => e.OperationTypeId, "idx_bank_operation_operation_type_id");

            entity.HasIndex(e => e.OrganizationId, "idx_bank_operation_organization_id");

            entity.HasIndex(e => e.StateId, "idx_bank_operation_state_id");

            entity.HasIndex(e => e.StatusId, "idx_bank_operation_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocNumber)
                .HasMaxLength(100)
                .HasColumnName("doc_number");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_bank_account_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.CounterpartyId)
                .HasConstraintName("bank_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_currency_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("bank_operation_payment_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.BankOperations)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_operation_status_id_fkey");
        });

        modelBuilder.Entity<CashBox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cash_box_pkey");

            entity.ToTable("cash_box");

            entity.HasIndex(e => e.BranchId, "idx_cash_box_branch_id");

            entity.HasIndex(e => e.CurrencyId, "idx_cash_box_currency_id");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_cash_box_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_cash_box_organization_id");

            entity.HasIndex(e => e.StateId, "idx_cash_box_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.CashBoxes)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("cash_box_branch_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CashBoxes)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CashBoxes)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CashBoxes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_box_state_id_fkey");
        });

        modelBuilder.Entity<CashOperation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cash_operation_pkey");

            entity.ToTable("cash_operation");

            entity.HasIndex(e => e.CashBoxId, "idx_cash_operation_cash_box_id");

            entity.HasIndex(e => e.CounterpartyId, "idx_cash_operation_counterparty_id");

            entity.HasIndex(e => e.DocDate, "idx_cash_operation_doc_date");

            entity.HasIndex(e => e.OperationTypeId, "idx_cash_operation_operation_type_id");

            entity.HasIndex(e => e.OrganizationId, "idx_cash_operation_organization_id");

            entity.HasIndex(e => e.StateId, "idx_cash_operation_state_id");

            entity.HasIndex(e => e.StatusId, "idx_cash_operation_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CashBoxId).HasColumnName("cash_box_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocNumber)
                .HasMaxLength(100)
                .HasColumnName("doc_number");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.CashBox).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.CashBoxId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_cash_box_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.CounterpartyId)
                .HasConstraintName("cash_operation_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_currency_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_organization_id_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("cash_operation_payment_type_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.CashOperations)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cash_operation_status_id_fkey");
        });

        modelBuilder.Entity<CmnBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_bank_pkey");

            entity.ToTable("cmn_bank");

            entity.HasIndex(e => e.Code, "idx_cmn_bank_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_cmn_bank_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Mfo)
                .HasMaxLength(20)
                .HasColumnName("mfo");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnBanks)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_bank_state_id_fkey");
        });

        modelBuilder.Entity<CmnCounterpartyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_counterparty_type_pkey");

            entity.ToTable("cmn_counterparty_type");

            entity.HasIndex(e => e.Code, "idx_cmn_counterparty_type_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnCounterpartyTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_counterparty_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnCurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_currency_pkey");

            entity.ToTable("cmn_currency");

            entity.HasIndex(e => e.Code, "idx_cmn_currency_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.Symbol)
                .HasMaxLength(10)
                .HasColumnName("symbol");

            entity.HasOne(d => d.State).WithMany(p => p.CmnCurrencies)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_currency_state_id_fkey");
        });

        modelBuilder.Entity<CmnDistrict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_district_pkey");

            entity.ToTable("cmn_district");

            entity.HasIndex(e => e.RegionId, "idx_cmn_district_region_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(250)
                .HasColumnName("full_name");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");
        });

        modelBuilder.Entity<CmnDocumentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_status_pkey");

            entity.ToTable("cmn_document_status");

            entity.HasIndex(e => e.Code, "idx_cmn_document_status_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnDocumentStatuses)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_status_state_id_fkey");
        });

        modelBuilder.Entity<CmnDocumentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_document_type_pkey");

            entity.ToTable("cmn_document_type");

            entity.HasIndex(e => e.Code, "idx_cmn_document_type_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_cmn_document_type_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnDocumentTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_document_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_language_pkey");

            entity.ToTable("cmn_language");

            entity.HasIndex(e => e.Code, "idx_cmn_language_code").IsUnique();

            entity.HasIndex(e => e.IsDefault, "idx_cmn_language_default")
                .IsUnique()
                .HasFilter("(is_default = true)");

            entity.HasIndex(e => e.StateId, "idx_cmn_language_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.NativeName)
                .HasMaxLength(100)
                .HasColumnName("native_name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnLanguages)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_language_state_id_fkey");
        });

        modelBuilder.Entity<CmnOperationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_operation_type_pkey");

            entity.ToTable("cmn_operation_type");

            entity.HasIndex(e => e.Code, "idx_cmn_operation_type_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_cmn_operation_type_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnOperationTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_operation_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnPaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_payment_type_pkey");

            entity.ToTable("cmn_payment_type");

            entity.HasIndex(e => e.Code, "idx_cmn_payment_type_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnPaymentTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_payment_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_region_pkey");

            entity.ToTable("cmn_region");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(250)
                .HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnRegions)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_region_state_id_fkey");
        });

        modelBuilder.Entity<CmnState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_state_pkey");

            entity.ToTable("cmn_state");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(250)
                .HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
        });

        modelBuilder.Entity<CmnTaxType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_tax_type_pkey");

            entity.ToTable("cmn_tax_type");

            entity.HasIndex(e => e.Code, "idx_cmn_tax_type_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_cmn_tax_type_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnTaxTypes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_tax_type_state_id_fkey");
        });

        modelBuilder.Entity<CmnTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_translation_pkey");

            entity.ToTable("cmn_translation");

            entity.HasIndex(e => e.LanguageId, "idx_cmn_translation_language_id");

            entity.HasIndex(e => new { e.TableName, e.RecordId, e.ColumnName }, "idx_cmn_translation_lookup");

            entity.HasIndex(e => new { e.LanguageId, e.TableName, e.RecordId, e.ColumnName }, "idx_cmn_translation_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColumnName)
                .HasMaxLength(100)
                .HasColumnName("column_name");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Language).WithMany(p => p.CmnTranslations)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_translation_language_id_fkey");
        });

        modelBuilder.Entity<CmnUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_unit_pkey");

            entity.ToTable("cmn_unit");

            entity.HasIndex(e => e.Code, "idx_cmn_unit_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnUnits)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_unit_state_id_fkey");
        });

        modelBuilder.Entity<CmnVatRate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cmn_vat_rate_pkey");

            entity.ToTable("cmn_vat_rate");

            entity.HasIndex(e => e.Code, "idx_cmn_vat_rate_code").IsUnique();

            entity.HasIndex(e => e.StateId, "idx_cmn_vat_rate_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Rate)
                .HasPrecision(5, 2)
                .HasColumnName("rate");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.CmnVatRates)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cmn_vat_rate_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_bank_account_pkey");

            entity.ToTable("counterparty_bank_account");

            entity.HasIndex(e => e.BankId, "idx_counterparty_bank_account_bank_id");

            entity.HasIndex(e => e.CounterpartyId, "idx_counterparty_bank_account_counterparty_id");

            entity.HasIndex(e => e.CurrencyId, "idx_counterparty_bank_account_currency_id");

            entity.HasIndex(e => e.OrganizationId, "idx_counterparty_bank_account_organization_id");

            entity.HasIndex(e => e.StateId, "idx_counterparty_bank_account_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasColumnName("account_number");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Bank).WithMany(p => p.CounterpartyBankAccounts)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_bank_id_fkey");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyBankAccounts)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CounterpartyBankAccounts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyBankAccounts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyBankAccounts)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_bank_account_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_card_pkey");

            entity.ToTable("counterparty_card");

            entity.HasIndex(e => e.DistrictId, "idx_counterparty_card_district_id");

            entity.HasIndex(e => e.Inn, "idx_counterparty_card_inn");

            entity.HasIndex(e => e.OrganizationId, "idx_counterparty_card_organization_id");

            entity.HasIndex(e => e.RegionId, "idx_counterparty_card_region_id");

            entity.HasIndex(e => e.ShortName, "idx_counterparty_card_short_name");

            entity.HasIndex(e => e.StateId, "idx_counterparty_card_state_id");

            entity.HasIndex(e => e.CounterpartyTypeId, "idx_counterparty_card_type_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(1000)
                .HasColumnName("address");
            entity.Property(e => e.CounterpartyTypeId).HasColumnName("counterparty_type_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(500)
                .HasColumnName("full_name");
            entity.Property(e => e.Inn)
                .HasMaxLength(20)
                .HasColumnName("inn");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.CounterpartyType).WithMany(p => p.CounterpartyCards)
                .HasForeignKey(d => d.CounterpartyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_counterparty_type_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.CounterpartyCards)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("counterparty_card_district_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyCards)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_organization_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.CounterpartyCards)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("counterparty_card_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyCards)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_card_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_contact_pkey");

            entity.ToTable("counterparty_contact");

            entity.HasIndex(e => e.CounterpartyId, "idx_counterparty_contact_counterparty_id");

            entity.HasIndex(e => e.OrganizationId, "idx_counterparty_contact_organization_id");

            entity.HasIndex(e => e.StateId, "idx_counterparty_contact_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(250)
                .HasColumnName("full_name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.Position)
                .HasMaxLength(250)
                .HasColumnName("position");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyContacts)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_counterparty_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyContacts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.CounterpartyContacts)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_contact_state_id_fkey");
        });

        modelBuilder.Entity<CounterpartyRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counterparty_reg_balance_pkey");

            entity.ToTable("counterparty_reg_balance");

            entity.HasIndex(e => e.CounterpartyId, "idx_counterparty_reg_balance_counterparty_id");

            entity.HasIndex(e => e.CurrencyId, "idx_counterparty_reg_balance_currency_id");

            entity.HasIndex(e => e.DocDate, "idx_counterparty_reg_balance_doc_date");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "idx_counterparty_reg_balance_document");

            entity.HasIndex(e => e.OrganizationId, "idx_counterparty_reg_balance_organization_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyRegBalances)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CounterpartyRegBalances)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_currency_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.CounterpartyRegBalances)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.CounterpartyRegBalances)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyRegBalances)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("counterparty_reg_balance_organization_id_fkey");
        });

        modelBuilder.Entity<InvProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_pkey");

            entity.ToTable("inv_product");

            entity.HasIndex(e => e.Barcode, "idx_inv_product_barcode");

            entity.HasIndex(e => e.Name, "idx_inv_product_name");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_inv_product_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_inv_product_organization_id");

            entity.HasIndex(e => e.ProductGroupId, "idx_inv_product_product_group_id");

            entity.HasIndex(e => e.StateId, "idx_inv_product_state_id");

            entity.HasIndex(e => e.UnitId, "idx_inv_product_unit_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .HasColumnName("barcode");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsService).HasColumnName("is_service");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ProductGroupId).HasColumnName("product_group_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_organization_id_fkey");

            entity.HasOne(d => d.ProductGroup).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.ProductGroupId)
                .HasConstraintName("inv_product_product_group_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_state_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.InvProducts)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_unit_id_fkey");
        });

        modelBuilder.Entity<InvProductGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_group_pkey");

            entity.ToTable("inv_product_group");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_inv_product_group_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_inv_product_group_organization_id");

            entity.HasIndex(e => e.ParentId, "idx_inv_product_group_parent_id");

            entity.HasIndex(e => e.StateId, "idx_inv_product_group_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProductGroups)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_group_organization_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("inv_product_group_parent_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProductGroups)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_group_state_id_fkey");
        });

        modelBuilder.Entity<InvProductPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_product_price_pkey");

            entity.ToTable("inv_product_price");

            entity.HasIndex(e => e.CurrencyId, "idx_inv_product_price_currency_id");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "idx_inv_product_price_dates");

            entity.HasIndex(e => e.OrganizationId, "idx_inv_product_price_organization_id");

            entity.HasIndex(e => e.ProductId, "idx_inv_product_price_product_id");

            entity.HasIndex(e => e.StateId, "idx_inv_product_price_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.InvProductPrices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvProductPrices)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvProductPrices)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_product_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvProductPrices)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_product_price_state_id_fkey");
        });

        modelBuilder.Entity<InvRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_reg_balance_pkey");

            entity.ToTable("inv_reg_balance");

            entity.HasIndex(e => e.DocDate, "idx_inv_reg_balance_doc_date");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "idx_inv_reg_balance_document");

            entity.HasIndex(e => e.OrganizationId, "idx_inv_reg_balance_organization_id");

            entity.HasIndex(e => e.ProductId, "idx_inv_reg_balance_product_id");

            entity.HasIndex(e => e.WarehouseId, "idx_inv_reg_balance_warehouse_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.InvRegBalances)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.InvRegBalances)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvRegBalances)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_organization_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.InvRegBalances)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_product_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InvRegBalances)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_reg_balance_warehouse_id_fkey");
        });

        modelBuilder.Entity<InvWarehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inv_warehouse_pkey");

            entity.ToTable("inv_warehouse");

            entity.HasIndex(e => e.BranchId, "idx_inv_warehouse_branch_id");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_inv_warehouse_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_inv_warehouse_organization_id");

            entity.HasIndex(e => e.ResponsibleUserId, "idx_inv_warehouse_responsible_user_id");

            entity.HasIndex(e => e.StateId, "idx_inv_warehouse_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ResponsibleUserId).HasColumnName("responsible_user_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.InvWarehouses)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("inv_warehouse_branch_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvWarehouses)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_organization_id_fkey");

            entity.HasOne(d => d.ResponsibleUser).WithMany(p => p.InvWarehouses)
                .HasForeignKey(d => d.ResponsibleUserId)
                .HasConstraintName("inv_warehouse_responsible_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.InvWarehouses)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inv_warehouse_state_id_fkey");
        });

        modelBuilder.Entity<MoneyRegBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("money_reg_balance_pkey");

            entity.ToTable("money_reg_balance");

            entity.HasIndex(e => e.CurrencyId, "idx_money_reg_balance_currency_id");

            entity.HasIndex(e => e.DocDate, "idx_money_reg_balance_doc_date");

            entity.HasIndex(e => new { e.DocumentTypeId, e.DocumentId }, "idx_money_reg_balance_document");

            entity.HasIndex(e => e.OrganizationId, "idx_money_reg_balance_organization_id");

            entity.HasIndex(e => new { e.SourceType, e.SourceId }, "idx_money_reg_balance_source");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .HasColumnName("source_type");

            entity.HasOne(d => d.Currency).WithMany(p => p.MoneyRegBalances)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_currency_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.MoneyRegBalances)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_document_type_id_fkey");

            entity.HasOne(d => d.OperationType).WithMany(p => p.MoneyRegBalances)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_operation_type_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.MoneyRegBalances)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("money_reg_balance_organization_id_fkey");
        });

        modelBuilder.Entity<OrgBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_bank_account_pkey");

            entity.ToTable("org_bank_account");

            entity.HasIndex(e => e.BankId, "idx_org_bank_account_bank_id");

            entity.HasIndex(e => e.CurrencyId, "idx_org_bank_account_currency_id");

            entity.HasIndex(e => e.OrganizationId, "idx_org_bank_account_organization_id");

            entity.HasIndex(e => e.StateId, "idx_org_bank_account_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasColumnName("account_number");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Bank).WithMany(p => p.OrgBankAccounts)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_bank_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.OrgBankAccounts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgBankAccounts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgBankAccounts)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_bank_account_state_id_fkey");
        });

        modelBuilder.Entity<OrgBranch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_branch_pkey");

            entity.ToTable("org_branch");

            entity.HasIndex(e => e.DistrictId, "idx_org_branch_district_id");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_org_branch_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_org_branch_organization_id");

            entity.HasIndex(e => e.RegionId, "idx_org_branch_region_id");

            entity.HasIndex(e => e.StateId, "idx_org_branch_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(1000)
                .HasColumnName("address");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.District).WithMany(p => p.OrgBranches)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("org_branch_district_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgBranches)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_branch_organization_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.OrgBranches)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("org_branch_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgBranches)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_branch_state_id_fkey");
        });

        modelBuilder.Entity<OrgDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_department_pkey");

            entity.ToTable("org_department");

            entity.HasIndex(e => e.BranchId, "idx_org_department_branch_id");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_org_department_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_org_department_organization_id");

            entity.HasIndex(e => e.StateId, "idx_org_department_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.OrgDepartments)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("org_department_branch_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgDepartments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_department_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgDepartments)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_department_state_id_fkey");
        });

        modelBuilder.Entity<OrgOrganization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_organization_pkey");

            entity.ToTable("org_organization");

            entity.HasIndex(e => e.DefaultLanguageId, "idx_org_organization_default_language_id");

            entity.HasIndex(e => e.DistrictId, "idx_org_organization_district_id");

            entity.HasIndex(e => e.FullName, "idx_org_organization_full_name");

            entity.HasIndex(e => e.Inn, "idx_org_organization_inn");

            entity.HasIndex(e => e.RegionId, "idx_org_organization_region_id");

            entity.HasIndex(e => e.ShortName, "idx_org_organization_short_name");

            entity.HasIndex(e => e.StateId, "idx_org_organization_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(1000)
                .HasColumnName("address");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.DefaultLanguageId).HasColumnName("default_language_id");
            entity.Property(e => e.Director)
                .HasMaxLength(250)
                .HasColumnName("director");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(500)
                .HasColumnName("full_name");
            entity.Property(e => e.Inn)
                .HasMaxLength(20)
                .HasColumnName("inn");
            entity.Property(e => e.IsParent).HasColumnName("is_parent");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.DefaultLanguage).WithMany(p => p.OrgOrganizations)
                .HasForeignKey(d => d.DefaultLanguageId)
                .HasConstraintName("org_organization_default_language_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.OrgOrganizations)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("org_organization_district_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.OrgOrganizations)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_organization_region_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgOrganizations)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_organization_state_id_fkey");
        });

        modelBuilder.Entity<OrgPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_position_pkey");

            entity.ToTable("org_position");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_org_position_org_code").IsUnique();

            entity.HasIndex(e => e.OrganizationId, "idx_org_position_organization_id");

            entity.HasIndex(e => e.StateId, "idx_org_position_state_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgPositions)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_position_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.OrgPositions)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_position_state_id_fkey");
        });

        modelBuilder.Entity<PurDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_pkey");

            entity.ToTable("pur_doc");

            entity.HasIndex(e => e.CounterpartyId, "idx_pur_doc_counterparty_id");

            entity.HasIndex(e => e.DocDate, "idx_pur_doc_doc_date");

            entity.HasIndex(e => e.OrganizationId, "idx_pur_doc_organization_id");

            entity.HasIndex(e => e.StateId, "idx_pur_doc_state_id");

            entity.HasIndex(e => e.StatusId, "idx_pur_doc_status_id");

            entity.HasIndex(e => e.WarehouseId, "idx_pur_doc_warehouse_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocNumber)
                .HasMaxLength(100)
                .HasColumnName("doc_number");
            entity.Property(e => e.FinalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("final_amount");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.VatAmount)
                .HasPrecision(18, 2)
                .HasColumnName("vat_amount");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurDocs)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<PurDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pur_doc_table_pkey");

            entity.ToTable("pur_doc_table");

            entity.HasIndex(e => e.OwnerId, "idx_pur_doc_table_owner_id");

            entity.HasIndex(e => e.ProductId, "idx_pur_doc_table_product_id");

            entity.HasIndex(e => e.VatRateId, "idx_pur_doc_table_vat_rate_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.VatAmount)
                .HasPrecision(18, 2)
                .HasColumnName("vat_amount");
            entity.Property(e => e.VatRateId).HasColumnName("vat_rate_id");

            entity.HasOne(d => d.Owner).WithMany(p => p.PurDocTables)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("pur_doc_table_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.PurDocTables)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pur_doc_table_product_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.PurDocTables)
                .HasForeignKey(d => d.VatRateId)
                .HasConstraintName("pur_doc_table_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SaleDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_pkey");

            entity.ToTable("sale_doc");

            entity.HasIndex(e => e.CounterpartyId, "idx_sale_doc_counterparty_id");

            entity.HasIndex(e => e.DocDate, "idx_sale_doc_doc_date");

            entity.HasIndex(e => e.OrganizationId, "idx_sale_doc_organization_id");

            entity.HasIndex(e => e.StateId, "idx_sale_doc_state_id");

            entity.HasIndex(e => e.StatusId, "idx_sale_doc_status_id");

            entity.HasIndex(e => e.WarehouseId, "idx_sale_doc_warehouse_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DocDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("doc_date");
            entity.Property(e => e.DocNumber)
                .HasMaxLength(100)
                .HasColumnName("doc_number");
            entity.Property(e => e.FinalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("final_amount");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.VatAmount)
                .HasPrecision(18, 2)
                .HasColumnName("vat_amount");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

            entity.HasOne(d => d.Counterparty).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_counterparty_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_currency_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_organization_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_state_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_status_id_fkey");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SaleDocs)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_warehouse_id_fkey");
        });

        modelBuilder.Entity<SaleDocTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_doc_table_pkey");

            entity.ToTable("sale_doc_table");

            entity.HasIndex(e => e.OwnerId, "idx_sale_doc_table_owner_id");

            entity.HasIndex(e => e.ProductId, "idx_sale_doc_table_product_id");

            entity.HasIndex(e => e.VatRateId, "idx_sale_doc_table_vat_rate_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasPrecision(18, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.VatAmount)
                .HasPrecision(18, 2)
                .HasColumnName("vat_amount");
            entity.Property(e => e.VatRateId).HasColumnName("vat_rate_id");

            entity.HasOne(d => d.Owner).WithMany(p => p.SaleDocTables)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("sale_doc_table_owner_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleDocTables)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_doc_table_product_id_fkey");

            entity.HasOne(d => d.VatRate).WithMany(p => p.SaleDocTables)
                .HasForeignKey(d => d.VatRateId)
                .HasConstraintName("sale_doc_table_vat_rate_id_fkey");
        });

        modelBuilder.Entity<SysModule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_module_pkey");

            entity.ToTable("sys_module");

            entity.HasIndex(e => e.Code, "sys_module_unique_index_code").IsUnique();

            entity.HasIndex(e => e.SubGroupId, "sys_module_unique_index_sub_group_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(300)
                .HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.SubGroupId).HasColumnName("sub_group_id");

            entity.HasOne(d => d.State).WithMany(p => p.SysModules)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_module_state_id_fkey");

            entity.HasOne(d => d.SubGroup).WithMany(p => p.SysModules)
                .HasForeignKey(d => d.SubGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_module_sub_group_id_fkey");
        });

        modelBuilder.Entity<SysModuleSubGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_module_sub_group_pkey");

            entity.ToTable("sys_module_sub_group");

            entity.HasIndex(e => e.Code, "sys_module_sub_group_unique_index_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(300)
                .HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(250)
                .HasColumnName("short_name");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_role_pkey");

            entity.ToTable("sys_role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(100)
                .HasColumnName("short_name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.SysRoles)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_state_id_fkey");
        });

        modelBuilder.Entity<SysRoleModule>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ModuleId }).HasName("sys_role_module_pkey");

            entity.ToTable("sys_role_module");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");

            entity.HasOne(d => d.Module).WithMany(p => p.SysRoleModules)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_module_module_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.SysRoleModules)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_role_module_role_id_fkey");
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sys_user_pkey");

            entity.ToTable("sys_user");

            entity.HasIndex(e => e.LanguageId, "idx_sys_user_language_id");

            entity.HasIndex(e => e.PhoneNumber, "idx_sys_user_phone");

            entity.HasIndex(e => e.RoleId, "idx_sys_user_role_id");

            entity.HasIndex(e => e.UserName, "uidx_sys_user_user_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.LastAccessTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_access_time");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(250)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(250)
                .HasColumnName("password_salt");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(250)
                .HasColumnName("user_name");

            entity.HasOne(d => d.Language).WithMany(p => p.SysUsers)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("sys_user_language_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.SysUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_role_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SysUsers)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sys_user_state_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
