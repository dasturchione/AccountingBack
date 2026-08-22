using Application.Features.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApi.Authorization;
using WebApi.Controllers;

public sealed class ProviderContractReconciliationContractTests
{
    [Fact]
    public void ProviderReconciliationRouteIsExplicitPost()
    {
        var method = typeof(ContractController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == nameof(ContractController.ReconcileEdocsProviderIdentity));
        var route = method.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(route);
        Assert.Equal("edo-provider-identity", route!.Template);
        Assert.Equal(typeof(IResult), method.ReturnType.GetGenericArguments().Single());
        Assert.Contains(typeof(ProviderContractReconciliationCreateDto),
            method.GetParameters().Select(x => x.ParameterType));
        Assert.NotNull(method.GetCustomAttribute<ModuleAuthorizeAttribute>());
    }
}
