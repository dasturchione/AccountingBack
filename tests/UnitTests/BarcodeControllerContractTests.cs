using System.Text;
using Application.Abstractions.Authentication;
using Infrastructure.Services.Barcode;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using WebApi.Controllers.Cmn;

namespace UnitTests;

public sealed class BarcodeControllerContractTests
{
    [Theory]
    [InlineData("qr")]
    [InlineData("code128")]
    [InlineData("ean13")]
    public async Task ValidContentReturnsPng(string format)
    {
        var controller = CreateController();
        var result = format switch
        {
            "qr" => controller.GenerateQr("manual-barcode-test", 2),
            "code128" => controller.GenerateCode128("MANUAL-128"),
            "ean13" => controller.GenerateEan13("478000000001"),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        var context = CreateHttpContext();
        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("image/png", context.Response.ContentType);
        Assert.True(context.Response.Body.Length > 8);
        context.Response.Body.Position = 0;
        var signature = new byte[8];
        await context.Response.Body.ReadExactlyAsync(signature);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, signature);
    }

    [Fact]
    public async Task InvalidEan13ReturnsLocalizedValidationProblem()
    {
        var controller = CreateController();
        var context = CreateHttpContext();

        await controller.GenerateEan13("ABC").ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.Contains("Barcode.InvalidEan13", body);
        Assert.Contains("Для EAN-13 требуется 12 или 13 цифр.", body);
    }

    private static BarcodeController CreateController() => new(
        new BarcodeGenerator(
            NullLogger<BarcodeGenerator>.Instance,
            new RussianUserContext()));

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        return new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() }
        };
    }

    private sealed class RussianUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => 1;
        public List<int> AllowedOrganizationIds => [1];
        public int? BranchId => null;
    }
}
