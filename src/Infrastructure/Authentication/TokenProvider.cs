using Application.Abstractions.Authentication;
using Domain.Entities;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication;

public class TokenProvider : ITokenProvider
{
    private const int MinimumHs256KeySizeInBytes = 32;
    private readonly JwtOptions _jwt;

    public TokenProvider(IOptions<JwtOptions> options)
    {
        _jwt = options.Value;
    }

    public string GenerateAccessToken(User user, int organizationId)
    {
        var keyBytes = GetValidatedSigningKeyBytes();
        var key = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: GetClaims(user),
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public int GetRefreshTokenExpirationDays() => _jwt.RefreshTokenExpirationDays;

    public string GetRefreshTokenHash(string refreshToken)
    {
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private byte[] GetValidatedSigningKeyBytes()
    {
        if (string.IsNullOrWhiteSpace(_jwt.Key))
            throw new InvalidOperationException("Jwt:Key is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(_jwt.Key);
        if (keyBytes.Length < MinimumHs256KeySizeInBytes)
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HS256 signing.");

        return keyBytes;
    }

    private static List<Claim> GetClaims(User user) =>
    [
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("TenantId", user.TenantId.ToString()),
        new Claim("UserKindCode", user.UserKind.Code)
    ];
}