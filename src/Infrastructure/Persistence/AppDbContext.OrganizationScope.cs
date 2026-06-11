using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public partial class AppDbContext
    {
        private const string OrgIdProperty = "OrganizationId";

        private IUserContext? _userContext;

        public void SetUserContext(IUserContext userContext)
        {
            _userContext = userContext;
        }

        // Header berilgan bo'lsa — o'sha 1 org; berilmasa 0 (ya'ni "hammasi" rejimi)
        private int CurrentOrganizationId => _userContext?.OrganizationId ?? 0;

        // User ruxsat berilgan barcha org IDlar
        private List<int> AllowedOrgIds => _userContext?.AllowedOrganizationIds ?? [];

        private void ApplyScopedFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => AllowedOrgIds.Count == 0
                                  || (CurrentOrganizationId != 0
                                      ? EF.Property<int>(e, OrgIdProperty) == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(EF.Property<int>(e, OrgIdProperty))));
        }

        private void ApplyOrganizationFilters(ModelBuilder modelBuilder)
        {
            // To'g'ridan-to'g'ri OrganizationId mavjud entitylar
            ApplyScopedFilter<OrgBankAccount>(modelBuilder);
            ApplyScopedFilter<Warehouse>(modelBuilder);
            ApplyScopedFilter<BankOperation>(modelBuilder);
            ApplyScopedFilter<ProductPrice>(modelBuilder);
            ApplyScopedFilter<ProductGroup>(modelBuilder);
            ApplyScopedFilter<ChartAccount>(modelBuilder);
            ApplyScopedFilter<ChartAccountSubkonto>(modelBuilder);
            ApplyScopedFilter<Product>(modelBuilder);
            ApplyScopedFilter<SaleDoc>(modelBuilder);
            ApplyScopedFilter<Branch>(modelBuilder);
            ApplyScopedFilter<Department>(modelBuilder);
            ApplyScopedFilter<PurchaseDoc>(modelBuilder);
            ApplyScopedFilter<CounterpartyCard>(modelBuilder);
            ApplyScopedFilter<CounterpartyBankAccount>(modelBuilder);
            ApplyScopedFilter<CounterpartyContact>(modelBuilder);
            ApplyScopedFilter<AccountingRegisterEntry>(modelBuilder);
            ApplyScopedFilter<CounterpartyRegisterBalance>(modelBuilder);
            ApplyScopedFilter<InventoryRegisterBalance>(modelBuilder);
            ApplyScopedFilter<MoneyRegisterBalance>(modelBuilder);
            ApplyScopedFilter<CashOperation>(modelBuilder);
            ApplyScopedFilter<Position>(modelBuilder);
            ApplyScopedFilter<CashBox>(modelBuilder);
            ApplyScopedFilter<UserOrganization>(modelBuilder);
            ApplyScopedFilter<ProductTable>(modelBuilder);

            // Navigation orqali OrganizationId bo'lgan entitylar
            modelBuilder.Entity<PurchaseDocTable>()
                .HasQueryFilter(e => AllowedOrgIds.Count == 0
                                  || (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId)));

            modelBuilder.Entity<SaleDocTable>()
                .HasQueryFilter(e => AllowedOrgIds.Count == 0
                                  || (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId)));

            // Role — OrganizationId nullable: null bo'lsa global (hamma ko'ra oladi)
            modelBuilder.Entity<Role>()
                .HasQueryFilter(e => e.OrganizationId == null
                                  || AllowedOrgIds.Count == 0
                                  || (CurrentOrganizationId != 0
                                      ? e.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.OrganizationId.Value)));

            // PostingRule — OrganizationId nullable
            modelBuilder.Entity<PostingRule>()
                .HasQueryFilter(e => e.OrganizationId == null
                                  || AllowedOrgIds.Count == 0
                                  || (CurrentOrganizationId != 0
                                      ? e.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.OrganizationId.Value)));
        }

        public override int SaveChanges()
        {
            EnforceOrganizationScope();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            EnforceOrganizationScope();
            return base.SaveChangesAsync(ct);
        }

        // Boshqa tashkilot nomidan yozish/o'zgartirish/o'chirishni taqiqlaydi
        private void EnforceOrganizationScope()
        {
            if (AllowedOrgIds.Count == 0) return;

            foreach (var entry in ChangeTracker.Entries())
            {
                var prop = entry.Metadata.FindProperty(OrgIdProperty);
                if (prop == null) continue;

                var orgIdVal = entry.Property(OrgIdProperty).CurrentValue;
                if (orgIdVal is not int orgId) continue;

                if (entry.State == EntityState.Added)
                {
                    if (orgId == 0 && CurrentOrganizationId > 0)
                    {
                        entry.Property(OrgIdProperty).CurrentValue = CurrentOrganizationId;
                        orgId = CurrentOrganizationId;
                    }

                    if (!AllowedOrgIds.Contains(orgId))
                        throw new InvalidOperationException(
                            $"'{entry.Metadata.ClrType.Name}': bu tashkilot uchun yaratish taqiqlangan.");
                }
                else if (entry.State == EntityState.Modified)
                {
                    var originalOrgId = (int?)entry.Property(OrgIdProperty).OriginalValue ?? 0;
                    if (!AllowedOrgIds.Contains(originalOrgId))
                        throw new InvalidOperationException(
                            $"'{entry.Metadata.ClrType.Name}': bu tashkilot ma'lumotini tahrirlash taqiqlangan.");

                    entry.Property(OrgIdProperty).IsModified = false;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var originalOrgId = (int?)entry.Property(OrgIdProperty).OriginalValue ?? 0;
                    if (!AllowedOrgIds.Contains(originalOrgId))
                        throw new InvalidOperationException(
                            $"'{entry.Metadata.ClrType.Name}': bu tashkilot ma'lumotini o'chirish taqiqlangan.");
                }
            }
        }
    }
}
