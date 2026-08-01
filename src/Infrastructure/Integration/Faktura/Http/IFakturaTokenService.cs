namespace Integration.Faktura.Http;

public interface IFakturaTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
