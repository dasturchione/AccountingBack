using Domain.Entities;
﻿

namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string GenerateAccessToken(User user, int organizationId);
    string GenerateRefreshToken();
    int GetRefreshTokenExpirationDays();
    string GetRefreshTokenHash(string refreshToken);
}
