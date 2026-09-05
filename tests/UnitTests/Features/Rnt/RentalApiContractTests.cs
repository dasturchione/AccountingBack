using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace UnitTests.Features.Rnt;

public sealed class RentalApiContractTests
{
    [Fact]
    public void ManualControllerExposesUtilityServicesRoute()
    {
        var method = typeof(ManualController).GetMethod("GetUtilityServices");

        Assert.NotNull(method);
        var route = Assert.Single(method!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true).Cast<HttpGetAttribute>());
        Assert.Equal("utility-services", route.Template);
    }
}
