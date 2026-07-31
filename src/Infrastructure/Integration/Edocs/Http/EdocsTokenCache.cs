namespace Integration.Edocs.Http;

/// <summary>
/// Bearer tokenni handler'lar orasida saqlaydi. DelegatingHandler'lar HttpClientFactory
/// tomonidan davriy ravishda almashtirilib turadi (odatiy handler lifetime — 2 daqiqa),
/// shuning uchun token holati handler'ning o'zida emas, shu Singleton keshda saqlanadi.
/// </summary>
public sealed class EdocsTokenCache
{
    private readonly object _lock = new();
    private string? _token;
    private DateTimeOffset _expiresAt;

    public bool TryGet(out string token)
    {
        lock (_lock)
        {
            if (_token is not null && DateTimeOffset.UtcNow < _expiresAt)
            {
                token = _token;
                return true;
            }

            token = string.Empty;
            return false;
        }
    }

    public void Set(string token, TimeSpan validFor)
    {
        lock (_lock)
        {
            _token = token;
            _expiresAt = DateTimeOffset.UtcNow.Add(validFor);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _token = null;
        }
    }
}
