//using Application.Abstractions.Authentication;
//using Domain.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace Infrastructure.Persistence
//{
//    //public partial class AppDbContext
//    //{
//    //    private IUserContext? _userContext;
//    //    public void SetUserContext(IUserContext userContext)
//    //    {
//    //        _userContext = userContext;
//    //    }

//    //    private int CurrentOrganizationId => _userContext?.OrganizationId ?? 0;
//    //    private int CurrentTenantId => _userContext?.TenantId ?? 0;
//    //    private bool HasCurrentTenant => CurrentTenantId > 0;
//    //    private bool HasCurrentOrganization => CurrentOrganizationId > 0;
//    //    private bool IsSuperAdmin => _userContext?.UserKind == CurrentUserKind.SuperAdmin;
//    //    private bool IsTenantAdmin => _userContext?.UserKind == CurrentUserKind.TenantAdmin;
//    //    private bool IsOrganizationUser => _userContext?.UserKind == CurrentUserKind.TenantUser;
//    //    private bool HasAuthenticatedUser => _userContext?.Id is > 0;

//    //    private IReadOnlyCollection<int> AllowedOrganizationIds =>
//    //        _userContext?.AllowedOrganizationIds is { } organizationIds
//    //            ? organizationIds
//    //            : Array.Empty<int>();


//    //    private void ApplyAccessFilters(ModelBuilder modelBuilder)
//    //    {
//    //        modelBuilder.Entity<Organization>()
//    //                        .HasQueryFilter(e =>
//    //                                            IsSuperAdmin
//    //                                            || (IsTenantAdmin
//    //                                                && HasCurrentTenant
//    //                                                && e.TenantId == CurrentTenantId)
//    //                                            || (IsOrganizationUser
//    //                                                && HasAuthenticatedUser
//    //                                                && AllowedOrganizationIds.Contains(e.Id)
//    //                                                && (
//    //                                                    !HasCurrentOrganization
//    //                                                    || e.Id == CurrentOrganizationId
//    //                                                )));

//    //        modelBuilder.Entity<User>()
//    //                        .HasQueryFilter(e =>
//    //                                            IsSuperAdmin
//    //                                            || (IsTenantAdmin
//    //                                                && HasCurrentTenant
//    //                                                && e.TenantId == CurrentTenantId)
//    //                                            || (IsOrganizationUser
//    //                                                && HasAuthenticatedUser
//    //                                                && e.UserOrganizations.Any(a => AllowedOrganizationIds.Contains(a.OrganizationId))
//    //                                                && (
//    //                                                    !HasCurrentOrganization
//    //                                                    || e.UserOrganizations.Any(a => a.OrganizationId == CurrentOrganizationId)
//    //                                                )));

//    //    }
//    //}
//}
