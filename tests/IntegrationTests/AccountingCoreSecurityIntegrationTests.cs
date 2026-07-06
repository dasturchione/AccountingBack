using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Constants;

namespace IntegrationTests;

public class AccountingCoreSecurityIntegrationTests
{
    [Theory]
    [InlineData("POST", "/api/chart-accounts")]
    [InlineData("PUT", "/api/chart-accounts/1")]
    [InlineData("DELETE", "/api/chart-accounts/1")]
    public async Task ChartAccountMutations_ShouldRequireGlobalAccess_EvenWhenPermissionExists(string method, string url)
    {
        await using var factory = new TestWebApplicationFactory(
            overrideServices: services =>
            {
                services.RemoveAll(typeof(IPermissionChecker));
                services.AddSingleton<IPermissionChecker>(new FixedPermissionChecker(
                [
                    PermissionCodeConst.ChartAccountCreate,
                    PermissionCodeConst.ChartAccountUpdate,
                    PermissionCodeConst.ChartAccountDelete
                ]));
            });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "900");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-Test-OrgId", "1");

        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (!string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = JsonContent.Create(new
            {
                code = "5111",
                name = "Test account",
                isGroup = false,
                parentId = (int?)null,
                stateId = StateIdConst.ACTIVE
            });
        }

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    // Global users bypass the module permission check (ModuleAuthorize), so even without the
    // specific chart-account permission the request is NOT forbidden — it reaches the handler.
    // POST creates a new account (200); PUT/DELETE target a non-existent id and return 404.
    [InlineData("POST", "/api/chart-accounts", HttpStatusCode.OK)]
    [InlineData("PUT", "/api/chart-accounts/1", HttpStatusCode.NotFound)]
    [InlineData("DELETE", "/api/chart-accounts/1", HttpStatusCode.NotFound)]
    public async Task ChartAccountMutations_ShouldBypassModulePermission_ForGlobalUsers(string method, string url, HttpStatusCode expectedStatusCode)
    {
        await using var factory = new TestWebApplicationFactory(
            overrideServices: services =>
            {
                services.RemoveAll(typeof(IPermissionChecker));
                services.AddSingleton<IPermissionChecker>(new FixedPermissionChecker([]));
            });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "900");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-Test-GlobalAccess", "true");

        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (!string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = JsonContent.Create(new
            {
                code = "5111",
                name = "Test account",
                isGroup = false,
                parentId = (int?)null,
                stateId = StateIdConst.ACTIVE
            });
        }

        var response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task ChartAccountCreate_ShouldAllowGlobalUserWithChartAccountPermission()
    {
        await using var factory = new TestWebApplicationFactory(
            overrideServices: services =>
            {
                services.RemoveAll(typeof(IPermissionChecker));
                services.AddSingleton<IPermissionChecker>(
                    new FixedPermissionChecker([PermissionCodeConst.ChartAccountCreate]));
            });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "900");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-Test-GlobalAccess", "true");

        var response = await client.PostAsJsonAsync("/api/chart-accounts", new
        {
            code = "5111",
            name = "Test account",
            isGroup = false,
            parentId = (int?)null
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class FixedPermissionChecker(IEnumerable<string> allowedPermissions) : IPermissionChecker
    {
        private readonly HashSet<string> _allowedPermissions = new(allowedPermissions, StringComparer.Ordinal);

        public Task<bool> HasPermissionAsync(int roleId, string permissionCode, CancellationToken ct = default) =>
            Task.FromResult(_allowedPermissions.Contains(permissionCode));

        public Task<bool> HasAnyPermissionAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken ct = default) =>
            Task.FromResult(permissionCodes.Any(_allowedPermissions.Contains));
    }
}
