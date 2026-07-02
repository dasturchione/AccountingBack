using Domain.Entities;
using Infrastructure.Authentication;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class TokenProviderTests
{
    [Fact]
    public void GenerateAccessToken_ShouldThrowClearError_WhenJwtKeyIsTooShort()
    {
        var provider = new TokenProvider(Options.Create(new JwtOptions
        {
            Key = "short-key-under-32",
            Issuer = "AccountingApi",
            Audience = "AccountingClient",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        }));

        var user = new User
        {
            Id = 1,
            RoleId = 1,
            Role = new Role
            {
                Id = 1,
                FullName = "Super Admin",
                ShortName = "super",
                HasGlobalAccess = true
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GenerateAccessToken(user, 1));
        Assert.Equal("Jwt:Key must be at least 32 bytes for HS256 signing.", exception.Message);
    }
}
