namespace Application.Features.Platform;

public class DashboardStatsDto
{
    public int TenantsCount { get; set; }
    public int ActiveTenantsCount { get; set; }
    public int InactiveTenantsCount { get; set; }
    public int OrganizationsCount { get; set; }
    public int ActiveOrganizationsCount { get; set; }
    public int InactiveOrganizationsCount { get; set; }
    public int UsersCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int BlockedUsersCount { get; set; }
    public DashboardTenantStatsDto TenantStats { get; set; } = new();
    public DashboardUserStatsDto UserStats { get; set; } = new();
    public DashboardOrganizationStatsDto OrganizationStats { get; set; } = new();
    public DashboardActivityStatsDto ActivityStats { get; set; } = new();
    public DashboardNotificationStatsDto NotificationStats { get; set; } = new();
    public DashboardSystemStatsDto SystemStats { get; set; } = new();
}

public sealed class DashboardTenantStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
}

public sealed class DashboardUserStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Blocked { get; set; }
    public int RecentSignupsLast30Days { get; set; }
    public int RecentlyActiveLast30Days { get; set; }
}

public sealed class DashboardOrganizationStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
}

public sealed class DashboardActivityStatsDto
{
    public int RecentAuditsLast7Days { get; set; }
}

public sealed class DashboardNotificationStatsDto
{
    public int Total { get; set; }
    public int Last7Days { get; set; }
    public int EmailSent { get; set; }
    public int EmailFailed { get; set; }
}

public sealed class DashboardSystemStatsDto
{
    public int SettingsCount { get; set; }
    public int RolesCount { get; set; }
}
