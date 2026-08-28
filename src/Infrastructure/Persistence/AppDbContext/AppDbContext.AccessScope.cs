using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext
{
    private IUserContext? _userContext;

    public void SetUserContext(IUserContext userContext)
    {
        _userContext = userContext;
    }

    private int CurrentOrganizationId => _backgroundOrganizationScope?.OrganizationId
        ?? _userContext?.OrganizationId
        ?? 0;
    private int CurrentTenantId => _userContext?.TenantId ?? 0;
    private bool HasCurrentTenant => CurrentTenantId > 0;
    private bool HasCurrentOrganization => CurrentOrganizationId > 0;
    private bool IsSuperAdmin => _userContext?.UserKind == CurrentUserKind.SuperAdmin;
    private bool IsTenantAdmin => _userContext?.UserKind == CurrentUserKind.TenantAdmin;
    private bool IsTenantUser => _userContext?.UserKind == CurrentUserKind.TenantUser;
    private bool HasBackgroundOrganizationScope => _backgroundOrganizationScope?.IsActive == true;
    private bool HasAuthenticatedUser => _userContext?.Id is > 0 || HasBackgroundOrganizationScope;

    private List<int> AllowedOrgIds => _backgroundOrganizationScope?.OrganizationId is { } organizationId
        ? [organizationId]
        : _userContext?.AllowedOrganizationIds ?? [];
    private const string OrgIdProperty = "OrganizationId";

    private void ApplyScopedFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? EF.Property<int>(e, OrgIdProperty) == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(EF.Property<int>(e, OrgIdProperty)))));
    }

    private void ApplyAccessFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>()
            .HasQueryFilter(organization =>
                IsSuperAdmin
                || (HasBackgroundOrganizationScope
                    && organization.Id == CurrentOrganizationId)
                || (IsTenantAdmin
                    && HasCurrentTenant
                    && organization.TenantId == CurrentTenantId)
                || (IsTenantUser
                    && HasAuthenticatedUser
                    && AllowedOrgIds.Contains(organization.Id)));

        modelBuilder.Entity<User>()
            .HasQueryFilter(user =>
                IsSuperAdmin
                || (IsTenantAdmin
                    && HasCurrentTenant
                    && user.TenantId == CurrentTenantId)
                || (IsTenantUser
                    && HasAuthenticatedUser
                    && user.UserOrganizations.Any(assignment =>
                        AllowedOrgIds.Contains(assignment.OrganizationId))
                    && (!HasCurrentOrganization || user.UserOrganizations.Any(assignment =>
                        assignment.OrganizationId == CurrentOrganizationId))));

// To'g'ridan-to'g'ri OrganizationId mavjud entitylar
        ApplyScopedFilter<BankAccount>(modelBuilder);
        ApplyScopedFilter<BankTerminal>(modelBuilder);
        ApplyScopedFilter<AccountingPeriod>(modelBuilder);
        ApplyScopedFilter<ChartAccount>(modelBuilder);
        ApplyScopedFilter<PostingBatch>(modelBuilder);
        ApplyScopedFilter<DocumentNumberSequence>(modelBuilder);
        ApplyScopedFilter<DocumentRegistry>(modelBuilder);
        ApplyScopedFilter<OrganizationSetupState>(modelBuilder);
        ApplyScopedFilter<OrganizationEdoProvider>(modelBuilder);
        ApplyScopedFilter<EdoDocument>(modelBuilder);
        ApplyScopedFilter<EdoDocumentSigningSession>(modelBuilder);
        ApplyScopedFilter<EdoAuthSigningSession>(modelBuilder);
        ApplyScopedFilter<EdoImportJob>(modelBuilder);
        ApplyScopedFilter<EdoImportBatch>(modelBuilder);
        ApplyScopedFilter<EdoImportBatchDocument>(modelBuilder);
        ApplyScopedFilter<EdoImportCandidate>(modelBuilder);
        ApplyScopedFilter<EdoProviderProductMapping>(modelBuilder);
        ApplyScopedFilter<OrganizationTaxSetting>(modelBuilder);
        ApplyScopedFilter<OrganizationDefault>(modelBuilder);
        ApplyScopedFilter<OrganizationUserInvitation>(modelBuilder);
        ApplyScopedFilter<Warehouse>(modelBuilder);
        ApplyScopedFilter<BankOperation>(modelBuilder);
        ApplyScopedFilter<ProductPrice>(modelBuilder);
        ApplyScopedFilter<FaGroup>(modelBuilder);
        ApplyScopedFilter<PricingCondition>(modelBuilder);
        ApplyScopedFilter<SaleCondition>(modelBuilder);
        ApplyScopedFilter<Product>(modelBuilder);
        ApplyScopedFilter<InventoryAdjustmentDoc>(modelBuilder);
        ApplyScopedFilter<InventoryCountDoc>(modelBuilder);
        ApplyScopedFilter<OpeningInventory>(modelBuilder);
        ApplyScopedFilter<SaleDoc>(modelBuilder);
        ApplyScopedFilter<RetailSaleDoc>(modelBuilder);
        ApplyScopedFilter<WarehouseTransferDoc>(modelBuilder);
        ApplyScopedFilter<Branch>(modelBuilder);
        ApplyScopedFilter<Department>(modelBuilder);
        ApplyScopedFilter<PurchaseDoc>(modelBuilder);
        ApplyScopedFilter<Contract>(modelBuilder);
        ApplyScopedFilter<CounterpartyCard>(modelBuilder);
        ApplyScopedFilter<CounterpartyBankAccount>(modelBuilder);
        ApplyScopedFilter<CounterpartyContact>(modelBuilder);
        ApplyScopedFilter<AccountingRegisterEntry>(modelBuilder);
        ApplyScopedFilter<DocumentAccountSetting>(modelBuilder);
        ApplyScopedFilter<CounterpartyRegisterBalance>(modelBuilder);
        ApplyScopedFilter<MoneyRegisterBalance>(modelBuilder);
        ApplyScopedFilter<CurrencyRevaluation>(modelBuilder);
        ApplyScopedFilter<CashOperation>(modelBuilder);
        ApplyScopedFilter<CashFiscalTransferDoc>(modelBuilder);
        ApplyScopedFilter<CashCollectionDoc>(modelBuilder);
        ApplyScopedFilter<Position>(modelBuilder);
        ApplyScopedFilter<CashBox>(modelBuilder);
        ApplyScopedFilter<FiscalCashRegister>(modelBuilder);
        //ApplyScopedFilter<UserOrganization>(modelBuilder);
        ApplyScopedFilter<FaAsset>(modelBuilder);
        ApplyScopedFilter<FaCommissioningDoc>(modelBuilder);
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
            .HasQueryFilter(e => IsSuperAdmin
                              || (e.OrganizationId.HasValue
                                  && (CurrentOrganizationId != 0
                                      ? e.OrganizationId.Value == CurrentOrganizationId
                                      : AllowedOrgIds.Contains(e.OrganizationId.Value))));

        // Navigation orqali OrganizationId bo'lgan entitylar
        modelBuilder.Entity<PurchaseDocProduct>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<EdoImportJobProvider>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Job.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Job.OrganizationId))));

        modelBuilder.Entity<EdoImportCandidateLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Candidate.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Candidate.OrganizationId))));

        modelBuilder.Entity<EdoImportCandidateMarking>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.CandidateLine.Candidate.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.CandidateLine.Candidate.OrganizationId))));

        modelBuilder.Entity<OpeningInventoryProduct>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<OpeningInventoryTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<PurchaseDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<FaAssetAccounting>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Asset.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Asset.OrganizationId))));

        modelBuilder.Entity<FaReceiptDocLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.ReceiptDoc.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.ReceiptDoc.OrganizationId))));

        modelBuilder.Entity<FaReceiptDocAsset>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.ReceiptDocLine.ReceiptDoc.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.ReceiptDocLine.ReceiptDoc.OrganizationId))));

        modelBuilder.Entity<FaCommissioningDocLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.CommissioningDoc.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.CommissioningDoc.OrganizationId))));

        modelBuilder.Entity<FaMovementDocLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.MovementDoc.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.MovementDoc.OrganizationId))));

        modelBuilder.Entity<SaleDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<RetailSaleDocProduct>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<RetailSaleDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<RetailSaleDocPayment>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<WarehouseTransferLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<WarehouseTransferDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<InventoryAdjustmentLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<InventoryAdjustmentDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        modelBuilder.Entity<InventoryCountLine>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.OrganizationId))));

        modelBuilder.Entity<InventoryCountDocTable>()
            .HasQueryFilter(e => IsSuperAdmin
                              || (AllowedOrgIds.Count > 0
                              && (CurrentOrganizationId != 0
                                  ? e.Owner.Owner.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.Owner.Owner.OrganizationId))));

        // Role — OrganizationId nullable: null bo'lsa global (hamma ko'ra oladi)
        modelBuilder.Entity<Role>()
            .HasQueryFilter(e => e.OrganizationId == null
                              || IsSuperAdmin
                              || (CurrentOrganizationId != 0
                                  ? e.OrganizationId == CurrentOrganizationId
                                  : AllowedOrgIds.Contains(e.OrganizationId.Value)));

        // Claim request hali organization bilan bog'lanmagan bo'lishi mumkin.
        modelBuilder.Entity<OrganizationClaimRequest>()
            .HasQueryFilter(e => e.OrganizationId == null
                              || IsSuperAdmin
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

        if (IsSuperAdmin) return;

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
    }}
