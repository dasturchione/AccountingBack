using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SharedKernel.Constants;

namespace IntegrationTests;

public sealed class DashboardReadOnlyApiContractTests
{
    private static readonly Assembly WebApiAssembly =
        typeof(WebApi.Controllers.Sys.DashboardController).Assembly;

    [Fact]
    public void Business_dashboard_controller_exposes_only_the_requested_get_routes()
    {
        var controllerType = WebApiAssembly.GetType(
            "WebApi.Controllers.Dashboard.BusinessDashboardController");

        Assert.NotNull(controllerType);

        var controllerRoute = controllerType!.GetCustomAttribute<RouteAttribute>()!.Template!;
        var routes = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(method => method.GetCustomAttributes<HttpGetAttribute>()
                .Select(attribute => $"{controllerRoute}/{attribute.Template}"))
            .ToList();

        Assert.Equal(
            new[]
            {
                "api/dashboard/overview",
                "api/dashboard/cash",
                "api/dashboard/receivables-payables",
                "api/dashboard/electronic-documents",
                "api/dashboard/tax-summary"
            }.OrderBy(x => x),
            routes.OrderBy(x => x));
    }

    [Fact]
    public void Tasks_controller_is_present_under_tasks_calendar_route()
    {
        var controllerType = WebApiAssembly.GetType(
            "WebApi.Controllers.Tasks.TaskCalendarController");

        Assert.NotNull(controllerType);
    }

    [Fact]
    public void Dashboard_view_permission_is_existing_permission()
    {
        Assert.Equal("DASHBOARD_VIEW", PermissionCodeConst.DashboardView);
    }

    [Fact]
    public void Dashboard_service_has_no_provider_or_command_dependency()
    {
        var serviceType = typeof(Application.Abstractions.IQueryRepository<>).Assembly
            .GetType("Application.Features.Dashboard.Services.BusinessDashboardService")!;

        Assert.NotNull(serviceType);
        Assert.DoesNotContain(
            serviceType!.GetConstructors().Single().GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("IEdoProvider", StringComparison.Ordinal) ||
                         parameter.ParameterType.Name.Contains("ICommandRepository", StringComparison.Ordinal));
    }

    [Fact]
    public void Business_dashboard_controller_does_not_require_global_access()
    {
        var controllerType = WebApiAssembly.GetType(
            "WebApi.Controllers.Dashboard.BusinessDashboardController");

        Assert.NotNull(controllerType);
        Assert.DoesNotContain(
            controllerType!.GetCustomAttributes(inherit: true),
            attribute => attribute.GetType().Name == "GlobalAccessAuthorizeAttribute");
    }

    [Fact]
    public void Receivables_payables_route_returns_both_debt_sections()
    {
        var controllerType = WebApiAssembly.GetType(
            "WebApi.Controllers.Dashboard.BusinessDashboardController");
        var method = controllerType!.GetMethod("GetReceivablesPayables");

        Assert.NotNull(method);
        Assert.Equal(
            "DashboardReceivablesPayablesDto",
            method!.ReturnType.GetGenericArguments().Single().Name);
    }
}
