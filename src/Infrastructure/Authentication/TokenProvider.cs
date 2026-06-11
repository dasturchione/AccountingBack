using Application.Abstractions.Authentication;
using Domain.Entities;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication
{
    public class TokenProvider : ITokenProvider
    {
        private readonly JwtOptions _jwt;
        public TokenProvider(IOptions<JwtOptions> options)
        {
            _jwt = options.Value;
        }

        public string GenerateAccessToken(User user, int organizationId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = GetClaims(user, organizationId);

            var token = new JwtSecurityToken(
                                    issuer: _jwt.Issuer,
                                    audience: _jwt.Audience,
                                    claims: claims,
                                    expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
                                    signingCredentials: credentials
                                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public int GetRefreshTokenExpirationDays()
        {
            return _jwt.RefreshTokenExpirationDays;
        }

        public string GetRefreshTokenHash(string refreshToken)
        {
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = SHA256.HashData(bytes);

            return Convert.ToBase64String(hash);
        }

        private List<Claim> GetClaims(User user, int organizationId)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim("OrganizationId", organizationId.ToString()),
            };
        }
    }
}
