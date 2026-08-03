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

        private bool HasGlobalAccess => _userContext?.HasGlobalAccess == true;
        private bool HasAuthenticatedUser => _userContext?.Id is not null;

        // User ruxsat berilgan barcha org IDlar
        private List<int> AllowedOrgIds => _userContext?.AllowedOrganizationIds ?? [];

        private void ApplyScopedFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? EF.Property<int>(e, OrgIdProperty) == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(EF.Property<int>(e, OrgIdProperty)))));
        }

        private void ApplyOrganizationFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Id == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Id))));

            // To'g'ridan-to'g'ri OrganizationId mavjud entitylar
            ApplyScopedFilter<BankAccount>(modelBuilder);
            ApplyScopedFilter<AccountingPeriod>(modelBuilder);
            ApplyScopedFilter<PostingBatch>(modelBuilder);
            ApplyScopedFilter<DocumentNumberSequence>(modelBuilder);
            ApplyScopedFilter<OrganizationSetupState>(modelBuilder);
            ApplyScopedFilter<OrganizationEdoProvider>(modelBuilder);
            ApplyScopedFilter<EdoDocument>(modelBuilder);
            ApplyScopedFilter<EdoDocumentSigningSession>(modelBuilder);
            ApplyScopedFilter<EdoAuthSigningSession>(modelBuilder);
            ApplyScopedFilter<OrganizationTaxSetting>(modelBuilder);
            ApplyScopedFilter<OrganizationDefault>(modelBuilder);
            ApplyScopedFilter<OrganizationUserInvitation>(modelBuilder);
            ApplyScopedFilter<Warehouse>(modelBuilder);
            ApplyScopedFilter<BankOperation>(modelBuilder);
            ApplyScopedFilter<ProductPrice>(modelBuilder);
            ApplyScopedFilter<ProductGroup>(modelBuilder);
            ApplyScopedFilter<FaGroup>(modelBuilder);
            ApplyScopedFilter<PricingCondition>(modelBuilder);
            ApplyScopedFilter<SaleCondition>(modelBuilder);
            ApplyScopedFilter<Product>(modelBuilder);
            ApplyScopedFilter<InventoryAdjustmentDoc>(modelBuilder);
            ApplyScopedFilter<InventoryCountDoc>(modelBuilder);
            ApplyScopedFilter<OpeningInventory>(modelBuilder);
            ApplyScopedFilter<SaleDoc>(modelBuilder);
            ApplyScopedFilter<WarehouseTransferDoc>(modelBuilder);
            ApplyScopedFilter<Branch>(modelBuilder);
            ApplyScopedFilter<Department>(modelBuilder);
            ApplyScopedFilter<PurchaseDoc>(modelBuilder);
            ApplyScopedFilter<CounterpartyCard>(modelBuilder);
            ApplyScopedFilter<CounterpartyBankAccount>(modelBuilder);
            ApplyScopedFilter<CounterpartyContact>(modelBuilder);
            ApplyScopedFilter<AccountingRegisterEntry>(modelBuilder);
            ApplyScopedFilter<DocumentAccountSetting>(modelBuilder);
            ApplyScopedFilter<CounterpartyRegisterBalance>(modelBuilder);
            ApplyScopedFilter<MoneyRegisterBalance>(modelBuilder);
            ApplyScopedFilter<CurrencyRevaluation>(modelBuilder);
            ApplyScopedFilter<CashOperation>(modelBuilder);
            ApplyScopedFilter<Position>(modelBuilder);
            ApplyScopedFilter<CashBox>(modelBuilder);
            ApplyScopedFilter<UserOrganization>(modelBuilder);
            ApplyScopedFilter<FaAsset>(modelBuilder);
            ApplyScopedFilter<FaReceiptDoc>(modelBuilder);
            ApplyScopedFilter<FaMovementDoc>(modelBuilder);
            ApplyScopedFilter<IdempotencyRecord>(modelBuilder);
            ApplyScopedFilter<MarkingBusinessPlace>(modelBuilder);
            ApplyScopedFilter<MarkingOrder>(modelBuilder);
            ApplyScopedFilter<MarkingUtilization>(modelBuilder);
            ApplyScopedFilter<MarkingCode>(modelBuilder);
            ApplyScopedFilter<MarkingAggregation>(modelBuilder);
            ApplyScopedFilter<MarkingAslBelgiDocument>(modelBuilder);
            ApplyScopedFilter<MarkingEdocsDocument>(modelBuilder);
            ApplyScopedFilter<MarkingDidoxDocument>(modelBuilder);
            ApplyScopedFilter<PayEmployee>(modelBuilder);
            ApplyScopedFilter<PayEmployment>(modelBuilder);
            ApplyScopedFilter<PayComponent>(modelBuilder);
            ApplyScopedFilter<PayEmployeeComponent>(modelBuilder);
            ApplyScopedFilter<PayPeriod>(modelBuilder);
            ApplyScopedFilter<PayTimesheet>(modelBuilder);
            ApplyScopedFilter<PayTimesheetLine>(modelBuilder);
            ApplyScopedFilter<PayPayrollDoc>(modelBuilder);
            ApplyScopedFilter<PayPayrollLine>(modelBuilder);
            ApplyScopedFilter<PayPayrollCalcLine>(modelBuilder);
            ApplyScopedFilter<PayPaymentBatch>(modelBuilder);
            ApplyScopedFilter<PayPaymentLine>(modelBuilder);
            ApplyScopedFilter<HrEmployeeWorkSchedule>(modelBuilder);
            ApplyScopedFilter<HrEmployeeWorkScheduleDay>(modelBuilder);
            ApplyScopedFilter<HrAbsence>(modelBuilder);
            ApplyScopedFilter<HrAbsenceAttachment>(modelBuilder);

            modelBuilder.Entity<AuditLog>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (e.OrganizationId.HasValue
                                      && (CurrentOrganizationId != 0
                                          ? e.OrganizationId.Value == CurrentOrganizationId
                                          : AllowedOrgIds.Contains(e.OrganizationId.Value))));

            // Navigation orqali OrganizationId bo'lgan entitylar
            modelBuilder.Entity<PurchaseDocProduct>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<OpeningInventoryProduct>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<OpeningInventoryTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<PurchaseDocTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<FaReceiptDocLine>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<FaReceiptDocAsset>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<FaMovementDocLine>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.MovementDoc.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.MovementDoc.OrganizationId))));

            modelBuilder.Entity<SaleDocTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<WarehouseTransferLine>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<WarehouseTransferDocTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<InventoryAdjustmentLine>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<InventoryAdjustmentDocTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            modelBuilder.Entity<InventoryCountLine>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

            modelBuilder.Entity<InventoryCountDocTable>()
                .HasQueryFilter(e => HasGlobalAccess
                                  || (AllowedOrgIds.Count > 0
                                  && (CurrentOrganizationId != 0
                                      ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

            // Role — OrganizationId nullable: null bo'lsa global (hamma ko'ra oladi)
            modelBuilder.Entity<Role>()
                .HasQueryFilter(e => e.OrganizationId == null
                                  || HasGlobalAccess
                                  || (CurrentOrganizationId != 0
                                      ? e.OrganizationId == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.OrganizationId.Value)));

            // Claim request hali organization bilan bog'lanmagan bo'lishi mumkin.
            modelBuilder.Entity<OrganizationClaimRequest>()
                .HasQueryFilter(e => e.OrganizationId == null
                                  || HasGlobalAccess
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

            if (!HasAuthenticatedUser) return;

            if (HasGlobalAccess) return;

            if (AllowedOrgIds.Count == 0)
                throw new InvalidOperationException("The current user has no active organization assignments.");

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
