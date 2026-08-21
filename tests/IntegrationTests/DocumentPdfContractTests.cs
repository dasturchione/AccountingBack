using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApi.Controllers;
using WebApi.Controllers.Integration;

public sealed class DocumentPdfContractTests
{
    [Fact]
    public void PurchaseAndSalePdfRoutesDeclarePdfContentType()
    {
        AssertPdfRoute(typeof(PurchaseDocController), "GetPdfAsync", "{id:long}/pdf");
        AssertPdfRoute(typeof(SaleDocController), "GetPdfAsync", "{id:long}/pdf");
    }

    [Fact]
    public void EdoOriginalFileRouteRemainsPdfAndOrganizationScopedByServiceContract()
    {
        var method = typeof(EdoController).GetMethod("GetFile");
        Assert.NotNull(method);

        var route = method!.GetCustomAttributes<HttpGetAttribute>().Single();
        Assert.Equal("files/{id:long}", route.Template);

        var produces = method.GetCustomAttribute<ProducesAttribute>();
        Assert.NotNull(produces);
        Assert.Contains("application/pdf", produces!.ContentTypes);
    }

    private static void AssertPdfRoute(Type controllerType, string methodName, string routeTemplate)
    {
        var method = controllerType.GetMethod(methodName);
        Assert.NotNull(method);

        var route = method!.GetCustomAttributes<HttpGetAttribute>().Single();
        Assert.Equal(routeTemplate, route.Template);

        var produces = method.GetCustomAttribute<ProducesAttribute>();
        Assert.NotNull(produces);
        Assert.Contains("application/pdf", produces!.ContentTypes);
    }
}
