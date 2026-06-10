using Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public partial class AppDbContext
    {
        public virtual DbSet<AccountType> AccountTypes { get; set; }

        public virtual DbSet<AccountingRegisterEntry> AccountingRegisterEntries { get; set; }

        public virtual DbSet<Bank> Banks { get; set; }

        public virtual DbSet<BankOperation> BankOperations { get; set; }

        public virtual DbSet<Branch> Branches { get; set; }

        public virtual DbSet<CashBox> CashBoxes { get; set; }

        public virtual DbSet<CashOperation> CashOperations { get; set; }

        public virtual DbSet<ChartAccount> ChartAccounts { get; set; }

        public virtual DbSet<ChartAccountSubkonto> ChartAccountSubkontos { get; set; }

        public virtual DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }

        public virtual DbSet<CounterpartyCard> CounterpartyCards { get; set; }

        public virtual DbSet<CounterpartyContact> CounterpartyContacts { get; set; }

        public virtual DbSet<CounterpartyRegisterBalance> CounterpartyRegisterBalances { get; set; }

        public virtual DbSet<Department> Departments { get; set; }

        public virtual DbSet<DocumentType> DocumentTypes { get; set; }

        public virtual DbSet<InventoryRegisterBalance> InventoryRegisterBalances { get; set; }

        public virtual DbSet<MoneyRegisterBalance> MoneyRegisterBalances { get; set; }

        public virtual DbSet<OperationType> OperationTypes { get; set; }

        public virtual DbSet<OrgBankAccount> OrgBankAccounts { get; set; }

        public virtual DbSet<Position> Positions { get; set; }

        public virtual DbSet<PostingRule> PostingRules { get; set; }

        public virtual DbSet<PostingRuleLine> PostingRuleLines { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductGroup> ProductGroups { get; set; }

        public virtual DbSet<ProductPrice> ProductPrices { get; set; }

        public virtual DbSet<PurchaseDoc> PurchaseDocs { get; set; }

        public virtual DbSet<PurchaseDocTable> PurchaseDocTables { get; set; }

        public virtual DbSet<SaleDoc> SaleDocs { get; set; }

        public virtual DbSet<SaleDocTable> SaleDocTables { get; set; }

        public virtual DbSet<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; }

        public virtual DbSet<SubkontoType> SubkontoTypes { get; set; }

        public virtual DbSet<TaxType> TaxTypes { get; set; }

        public virtual DbSet<VatRate> VatRates { get; set; }

        public virtual DbSet<Warehouse> Warehouses { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.AccountTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_account_type_state_id_fkey");
            });

            modelBuilder.Entity<SubkontoType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.SubkontoTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_subkonto_type_state_id_fkey");
            });

            modelBuilder.Entity<ChartAccountSubkonto>(entity =>
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

                entity.HasOne(d => d.Account).WithMany(p => p.ChartAccountSubkontos)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("acc_chart_account_subkonto_account_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.ChartAccountSubkontos)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_chart_account_subkonto_organization_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.ChartAccountSubkontos)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_chart_account_subkonto_state_id_fkey");

                entity.HasOne(d => d.SubkontoType).WithMany(p => p.ChartAccountSubkontos)
                    .HasForeignKey(d => d.SubkontoTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_chart_account_subkonto_subkonto_type_id_fkey");
            });

            modelBuilder.Entity<PostingRule>(entity =>
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

                entity.HasOne(d => d.DocumentType).WithMany(p => p.PostingRules)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_posting_rule_document_type_id_fkey");

                entity.HasOne(d => d.OperationType).WithMany(p => p.PostingRules)
                    .HasForeignKey(d => d.OperationTypeId)
                    .HasConstraintName("acc_posting_rule_operation_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.PostingRules)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("acc_posting_rule_organization_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.PostingRules)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_posting_rule_state_id_fkey");
            });

            modelBuilder.Entity<PostingRuleLine>(entity =>
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

                entity.HasOne(d => d.CreditAccount).WithMany(p => p.PostingRuleLinesCreditAccount)
                    .HasForeignKey(d => d.CreditAccountId)
                    .HasConstraintName("acc_posting_rule_line_credit_account_id_fkey");

                entity.HasOne(d => d.DebitAccount).WithMany(p => p.PostingRulesLineDebitAccount)
                    .HasForeignKey(d => d.DebitAccountId)
                    .HasConstraintName("acc_posting_rule_line_debit_account_id_fkey");

                entity.HasOne(d => d.Rule).WithMany(p => p.PostingRuleLines)
                    .HasForeignKey(d => d.RuleId)
                    .HasConstraintName("acc_posting_rule_line_rule_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.PostingRuleLines)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_posting_rule_line_state_id_fkey");
            });

            modelBuilder.Entity<RegisterEntrySubkonto>(entity =>
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

                entity.HasOne(d => d.Entry).WithMany(p => p.RegisterEntrySubkontos)
                    .HasForeignKey(d => d.EntryId)
                    .HasConstraintName("acc_reg_entry_subkonto_entry_id_fkey");

                entity.HasOne(d => d.SubkontoType).WithMany(p => p.RegisterEntrySubkontos)
                    .HasForeignKey(d => d.SubkontoTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_reg_entry_subkonto_subkonto_type_id_fkey");
            });

            modelBuilder.Entity<Branch>(entity =>
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

                entity.HasOne(d => d.District).WithMany(p => p.Branches)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("org_branch_district_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.Branches)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_branch_organization_id_fkey");

                entity.HasOne(d => d.Region).WithMany(p => p.Branches)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("org_branch_region_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Branches)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_branch_state_id_fkey");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("org_department_pkey");
                entity.ToTable("org_department");
                entity.HasIndex(e => new { e.OrganizationId, e.Code }, "idx_org_department_org_code").IsUnique();
                entity.HasIndex(e => e.OrganizationId, "idx_org_department_organization_id");
                entity.HasIndex(e => e.BranchId, "idx_org_department_branch_id");
                entity.HasIndex(e => e.StateId, "idx_org_department_state_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
                entity.Property(e => e.Name).HasMaxLength(250).HasColumnName("name");
                entity.Property(e => e.StateId).HasColumnName("state_id");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_date");
                entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).HasConstraintName("org_department_branch_id_fkey");
                entity.HasOne(e => e.State).WithMany().HasForeignKey(e => e.StateId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("org_department_state_id_fkey");
            });

            modelBuilder.Entity<Position>(entity =>
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

                entity.HasOne(d => d.Organization).WithMany(p => p.Positions)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_position_organization_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Positions)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_position_state_id_fkey");
            });

            modelBuilder.Entity<Bank>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Banks)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_bank_state_id_fkey");
            });

            modelBuilder.Entity<DocumentType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.DocumentTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_document_type_state_id_fkey");
            });

            modelBuilder.Entity<OperationType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.OperationTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_operation_type_state_id_fkey");
            });

            modelBuilder.Entity<TaxType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.TaxTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_tax_type_state_id_fkey");
            });

            modelBuilder.Entity<VatRate>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.VatRates)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_vat_rate_state_id_fkey");
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

            modelBuilder.Entity<ProductGroup>(entity =>
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

                entity.HasOne(d => d.Organization).WithMany(p => p.ProductGroups)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_group_organization_id_fkey");

                entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("inv_product_group_parent_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.ProductGroups)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_group_state_id_fkey");
            });

            modelBuilder.Entity<Product>(entity =>
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

                entity.HasOne(d => d.Organization).WithMany(p => p.Products)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_organization_id_fkey");

                entity.HasOne(d => d.ProductGroup).WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductGroupId)
                    .HasConstraintName("inv_product_product_group_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Products)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_state_id_fkey");

                entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_unit_id_fkey");
            });

            modelBuilder.Entity<Warehouse>(entity =>
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

                entity.HasOne(d => d.Branch).WithMany(p => p.Warehouses)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("inv_warehouse_branch_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.Warehouses)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_warehouse_organization_id_fkey");

                entity.HasOne(d => d.ResponsibleUser).WithMany(p => p.Warehouses)
                    .HasForeignKey(d => d.ResponsibleUserId)
                    .HasConstraintName("inv_warehouse_responsible_user_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Warehouses)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_warehouse_state_id_fkey");
            });

            modelBuilder.Entity<ProductPrice>(entity =>
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

                entity.HasOne(d => d.Currency).WithMany(p => p.ProductPrices)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_price_currency_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.ProductPrices)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_price_organization_id_fkey");

                entity.HasOne(d => d.Product).WithMany(p => p.ProductPrices)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_price_product_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.ProductPrices)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_product_price_state_id_fkey");
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

            modelBuilder.Entity<PurchaseDoc>(entity =>
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

                entity.HasOne(d => d.Counterparty).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.CounterpartyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_counterparty_id_fkey");

                entity.HasOne(d => d.Currency).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_currency_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_organization_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_state_id_fkey");

                entity.HasOne(d => d.Status).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_status_id_fkey");

                entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseDocs)
                    .HasForeignKey(d => d.WarehouseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_warehouse_id_fkey");
            });

            modelBuilder.Entity<PurchaseDocTable>(entity =>
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

                entity.HasOne(d => d.Owner).WithMany(p => p.Lines)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("pur_doc_table_owner_id_fkey");

                entity.HasOne(d => d.Product).WithMany(p => p.PurchaseDocTables)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pur_doc_table_product_id_fkey");

                entity.HasOne(d => d.VatRate).WithMany(p => p.PurchaseDocTables)
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

                entity.HasOne(d => d.Owner).WithMany(p => p.Lines)
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

            modelBuilder.Entity<InventoryRegisterBalance>(entity =>
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

                entity.HasOne(d => d.DocumentType).WithMany(p => p.InventoryRegisterBalances)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_reg_balance_document_type_id_fkey");

                entity.HasOne(d => d.OperationType).WithMany(p => p.InventoryRegisterBalances)
                    .HasForeignKey(d => d.OperationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_reg_balance_operation_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.InventoryRegisterBalances)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_reg_balance_organization_id_fkey");

                entity.HasOne(d => d.Product).WithMany(p => p.InventoryRegisterBalances)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_reg_balance_product_id_fkey");

                entity.HasOne(d => d.Warehouse).WithMany(p => p.InventoryRegisterBalances)
                    .HasForeignKey(d => d.WarehouseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inv_reg_balance_warehouse_id_fkey");
            });

            modelBuilder.Entity<MoneyRegisterBalance>(entity =>
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

                entity.HasOne(d => d.Currency).WithMany(p => p.MoneyRegisterBalances)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("money_reg_balance_currency_id_fkey");

                entity.HasOne(d => d.DocumentType).WithMany(p => p.MoneyRegisterBalances)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("money_reg_balance_document_type_id_fkey");

                entity.HasOne(d => d.OperationType).WithMany(p => p.MoneyRegisterBalances)
                    .HasForeignKey(d => d.OperationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("money_reg_balance_operation_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.MoneyRegisterBalances)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("money_reg_balance_organization_id_fkey");
            });

            modelBuilder.Entity<CounterpartyRegisterBalance>(entity =>
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

                entity.HasOne(d => d.Counterparty).WithMany(p => p.CounterpartyRegisterBalances)
                    .HasForeignKey(d => d.CounterpartyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("counterparty_reg_balance_counterparty_id_fkey");

                entity.HasOne(d => d.Currency).WithMany(p => p.CounterpartyRegisterBalances)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("counterparty_reg_balance_currency_id_fkey");

                entity.HasOne(d => d.DocumentType).WithMany(p => p.CounterpartyRegisterBalances)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("counterparty_reg_balance_document_type_id_fkey");

                entity.HasOne(d => d.OperationType).WithMany(p => p.CounterpartyRegisterBalances)
                    .HasForeignKey(d => d.OperationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("counterparty_reg_balance_operation_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.CounterpartyRegisterBalances)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("counterparty_reg_balance_organization_id_fkey");
            });

            modelBuilder.Entity<AccountingRegisterEntry>(entity =>
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

                entity.HasOne(d => d.CreditAccount).WithMany(p => p.AccountingRegisterEntriesCreditAccount)
                    .HasForeignKey(d => d.CreditAccountId)
                    .HasConstraintName("acc_reg_entry_credit_account_id_fkey");

                entity.HasOne(d => d.Currency).WithMany(p => p.AccountingRegisterEntries)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_reg_entry_currency_id_fkey");

                entity.HasOne(d => d.DebitAccount).WithMany(p => p.AccountingRegisterEntriesDebitAccount)
                    .HasForeignKey(d => d.DebitAccountId)
                    .HasConstraintName("acc_reg_entry_debit_account_id_fkey");

                entity.HasOne(d => d.DocumentType).WithMany(p => p.AccountingRegisterEntries)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_reg_entry_document_type_id_fkey");

                entity.HasOne(d => d.OperationType).WithMany(p => p.AccountingRegisterEntries)
                    .HasForeignKey(d => d.OperationTypeId)
                    .HasConstraintName("acc_reg_entry_operation_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.AccountingRegisterEntries)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_reg_entry_organization_id_fkey");
            });

            modelBuilder.Entity<ChartAccount>(entity =>
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

                entity.HasOne(d => d.AccountType).WithMany(p => p.ChartAccounts)
                    .HasForeignKey(d => d.AccountTypeId)
                    .HasConstraintName("acc_chart_account_account_type_id_fkey");

                entity.HasOne(d => d.Organization).WithMany(p => p.ChartAccounts)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_chart_account_organization_id_fkey");

                entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("acc_chart_account_parent_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.ChartAccounts)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("acc_chart_account_state_id_fkey");
            });
        }
    }
}
