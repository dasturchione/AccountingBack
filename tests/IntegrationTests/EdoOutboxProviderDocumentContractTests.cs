using Application.Abstractions.Integration.Edo;
using Application.Features.Integration.Edo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApi.Controllers.Integration;

public sealed class EdoOutboxProviderDocumentContractTests
{
    [Fact]
    public void ProviderDocumentDetailRouteIsReadOnlyAndProviderScoped()
    {
        var method = typeof(EdoController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == nameof(EdoController.GetOutboxProviderDocumentDetails));

        var route = method.GetCustomAttribute<HttpGetAttribute>();
        Assert.NotNull(route);
        Assert.Equal("outbox/provider-documents/{providerDocumentId}", route!.Template);
        Assert.Null(method.GetCustomAttribute<HttpPostAttribute>());
        Assert.Equal(typeof(IResult), method.ReturnType.GetGenericArguments().Single());
        Assert.Contains(typeof(EdoOutboxProviderDocumentDetailDto),
            method.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select(x => x.Type));
        Assert.Equal(nameof(IEdoInboxService.GetOutboxProviderDocumentDetailsAsync),
            typeof(IEdoInboxService).GetMethod(nameof(IEdoInboxService.GetOutboxProviderDocumentDetailsAsync))!.Name);
        Assert.True(typeof(EdoOutboxProviderDocumentDetailMapper).IsAbstract);
    }
}
