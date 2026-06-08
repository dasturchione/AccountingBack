using Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CounterpartyType> CounterpartyTypes { get; set; }

        public virtual DbSet<Currency> Currencies { get; set; }

        public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }

        public virtual DbSet<District> Districts { get; set; }

        public virtual DbSet<Language> Languages { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<PaymentType> PaymentTypes { get; set; }

        public virtual DbSet<Region> Regions { get; set; }

        public virtual DbSet<State> States { get; set; }

        public virtual DbSet<Translation> Translations { get; set; }

        public virtual DbSet<Unit> Units { get; set; }

        public virtual DbSet<Module> Modules { get; set; }

        public virtual DbSet<ModuleSubGroup> ModuleSubGroups { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<RoleModule> RoleModules { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("org_organization_pkey");

                entity.ToTable("org_organization");

                entity.HasIndex(e => e.ShortName, "idx_org_organization_short_name");

                entity.HasIndex(e => e.FullName, "idx_org_organization_full_name");

                entity.HasIndex(e => e.Inn, "idx_org_organization_inn");

                entity.HasIndex(e => e.RegionId, "idx_org_organization_region_id");

                entity.HasIndex(e => e.DistrictId, "idx_org_organization_district_id");

                entity.HasIndex(e => e.DefaultLanguageId, "idx_org_organization_default_language_id");

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
                entity.Property(e => e.IsParent)
                    .HasDefaultValue(false)
                    .HasColumnName("is_parent");
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .HasColumnName("phone_number");
                entity.Property(e => e.RegionId).HasColumnName("region_id");
                entity.Property(e => e.ShortName)
                    .HasMaxLength(250)
                    .HasColumnName("short_name");
                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.HasOne(d => d.District).WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("org_organization_district_id_fkey");

                entity.HasOne(d => d.DefaultLanguage)
                    .WithMany()
                    .HasForeignKey(d => d.DefaultLanguageId)
                    .HasConstraintName("org_organization_default_language_id_fkey");

                entity.HasOne(d => d.Region).WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_organization_region_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("org_organization_state_id_fkey");
            });

            modelBuilder.Entity<District>(entity =>
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

                entity.HasOne(d => d.Region).WithMany(p => p.Districts)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_district_region_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Districts)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_district_state_id_fkey");
            });

            modelBuilder.Entity<Region>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Regions)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_region_state_id_fkey");
            });

            modelBuilder.Entity<State>(entity =>
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

            modelBuilder.Entity<Currency>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Currencies)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_currency_state_id_fkey");
            });

            modelBuilder.Entity<Unit>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Units)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_unit_state_id_fkey");
            });

            modelBuilder.Entity<DocumentStatus>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.DocumentStatuses)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_document_status_state_id_fkey");
            });

            modelBuilder.Entity<CounterpartyType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.CounterpartyTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_counterparty_type_state_id_fkey");
            });

            modelBuilder.Entity<PaymentType>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.PaymentTypes)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_payment_type_state_id_fkey");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("cmn_language_pkey");

                entity.ToTable("cmn_language");

                entity.HasIndex(e => e.Code, "idx_cmn_language_code").IsUnique();

                entity.HasIndex(e => e.StateId, "idx_cmn_language_state_id");

                entity.HasIndex(e => e.IsDefault, "idx_cmn_language_default")
                    .IsUnique()
                    .HasFilter("is_default = true");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .HasColumnName("code");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("now()")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");
                entity.Property(e => e.IsDefault)
                    .HasDefaultValue(false)
                    .HasColumnName("is_default");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
                entity.Property(e => e.NativeName)
                    .HasMaxLength(100)
                    .HasColumnName("native_name");
                entity.Property(e => e.SortOrder)
                    .HasDefaultValue(0)
                    .HasColumnName("sort_order");
                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.HasOne(d => d.State).WithMany(p => p.Languages)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_language_state_id_fkey");
            });

            modelBuilder.Entity<Translation>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("cmn_translation_pkey");

                entity.ToTable("cmn_translation");

                entity.HasIndex(e => new { e.LanguageId, e.TableName, e.RecordId, e.ColumnName }, "idx_cmn_translation_unique")
                    .IsUnique();

                entity.HasIndex(e => new { e.TableName, e.RecordId, e.ColumnName }, "idx_cmn_translation_lookup");

                entity.HasIndex(e => e.LanguageId, "idx_cmn_translation_language_id");

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

                entity.HasOne(d => d.Language).WithMany(p => p.Translations)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cmn_translation_language_id_fkey");
            });

            modelBuilder.Entity<Module>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Modules)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_module_state_id_fkey");

                entity.HasOne(d => d.SubGroup).WithMany(p => p.Modules)
                    .HasForeignKey(d => d.SubGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_module_sub_group_id_fkey");
            });

            modelBuilder.Entity<ModuleSubGroup>(entity =>
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

            modelBuilder.Entity<Role>(entity =>
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

                entity.HasOne(d => d.State).WithMany(p => p.Roles)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_role_state_id_fkey");
            });

            modelBuilder.Entity<RoleModule>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ModuleId }).HasName("sys_role_module_pkey");

                entity.ToTable("sys_role_module");

                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.ModuleId).HasColumnName("module_id");
                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("created_date");

                entity.HasOne(d => d.Module).WithMany(p => p.RoleModules)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_role_module_module_id_fkey");

                entity.HasOne(d => d.Role).WithMany(p => p.RoleModules)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_role_module_role_id_fkey");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("sys_user_pkey");

                entity.ToTable("sys_user");

                entity.HasIndex(e => e.PhoneNumber, "idx_sys_user_phone");

                entity.HasIndex(e => e.RoleId, "idx_sys_user_role_id");

                entity.HasIndex(e => e.LanguageId, "idx_sys_user_language_id");

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
                entity.Property(e => e.LastAccessTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("last_access_time");
                entity.Property(e => e.LanguageId).HasColumnName("language_id");
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

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_user_role_id_fkey");

                entity.HasOne<Language>()
                    .WithMany()
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("sys_user_language_id_fkey");

                entity.HasOne(d => d.State).WithMany(p => p.Users)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sys_user_state_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

