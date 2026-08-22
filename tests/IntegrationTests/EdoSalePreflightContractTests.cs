using Application.Features.SaleDocs.EdoSalePreflight;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApi.Controllers;

public sealed class EdoSalePreflightContractTests
{
    [Fact]
    public void PlanEndpointIsReadOnly()
    {
        var methods = typeof(EdoSalePreflightController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public);
        var method = methods.Single(x => x.Name == nameof(EdoSalePreflightController.GetPlan));
        var route = method.GetCustomAttribute<HttpGetAttribute>();

        Assert.NotNull(route);
        Assert.Equal("plan", route!.Template);
        Assert.Null(method.GetCustomAttribute<HttpPostAttribute>());
        Assert.Equal(typeof(IResult), method.ReturnType.GetGenericArguments().Single());
        Assert.Contains(typeof(EdoSalePreflightPlanDto),
            method.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(x => x.Type));
    }

    [Fact]
    public void ApplyEndpointIsExplicitPost()
    {
        var method = typeof(EdoSalePreflightController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == nameof(EdoSalePreflightController.Apply));

        var route = method.GetCustomAttribute<HttpPostAttribute>();
        Assert.NotNull(route);
        Assert.Equal("apply", route!.Template);
        Assert.Contains(typeof(EdoSaleDraftApplyResponseDto),
            method.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(x => x.Type));

        var parameter = method.GetParameters()
            .Single(x => x.ParameterType == typeof(EdoSaleDraftApplyRequestDto));
        Assert.NotNull(parameter);
    }
}
