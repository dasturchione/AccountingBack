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
        var allowSentDocuments = method.GetParameters()
            .Single(x => x.Name == "allowSentDocuments");
        Assert.Equal(typeof(bool), allowSentDocuments.ParameterType);
        Assert.True(allowSentDocuments.HasDefaultValue);
        Assert.False((bool)allowSentDocuments.DefaultValue!);
        Assert.NotNull(allowSentDocuments.GetCustomAttribute<FromQueryAttribute>());
        Assert.Contains(typeof(EdoOutboxProviderDocumentDetailDto),
            method.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select(x => x.Type));
        Assert.Equal(nameof(IEdoInboxService.GetOutboxProviderDocumentDetailsAsync),
            typeof(IEdoInboxService).GetMethod(nameof(IEdoInboxService.GetOutboxProviderDocumentDetailsAsync))!.Name);
        var serviceAllowSentDocuments = typeof(IEdoInboxService)
            .GetMethod(nameof(IEdoInboxService.GetOutboxProviderDocumentDetailsAsync))!
            .GetParameters()
            .Single(x => x.Name == "allowSentDocuments");
        Assert.Equal(typeof(bool), serviceAllowSentDocuments.ParameterType);
        Assert.True(serviceAllowSentDocuments.HasDefaultValue);
        Assert.False((bool)serviceAllowSentDocuments.DefaultValue!);
        Assert.True(typeof(EdoOutboxProviderDocumentDetailMapper).IsAbstract);
    }
}
