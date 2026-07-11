namespace Integration.AslBelgi.Abstractions;

public interface IAslBelgiAuthClient
{
    Task<AslBelgiAuthResponse> AuthenticateAsync(AslBelgiAuthRequest request, CancellationToken ct = default);

    Task<AslBelgiAuthResponse> RefreshAsync(AslBelgiRefreshRequest request, CancellationToken ct = default);
}

public sealed class AslBelgiAuthRequest
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public sealed class AslBelgiRefreshRequest
{
    public string RefreshToken { get; set; } = null!;
}

public sealed class AslBelgiAuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string AccessTokenType { get; set; } = null!;
    public int AccessTokenExpiresIn { get; set; }
    public string RefreshToken { get; set; } = null!;
}
