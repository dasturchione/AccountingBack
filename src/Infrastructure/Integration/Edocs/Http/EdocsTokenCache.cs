using System.Collections.Concurrent;
using SharedKernel.Constants;

namespace Integration.Edocs.Http;

/// <summary>
/// Bearer tokenlarni organization + provider scope bo'yicha, handler'lar orasida saqlaydi.
/// DelegatingHandler'lar HttpClientFactory tomonidan davriy ravishda almashtirilib
/// turadi (odatiy handler lifetime — 2 daqiqa), shuning uchun token holati handler'ning
/// o'zida emas, shu Singleton keshda saqlanadi. <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// o'zi thread-safe — qo'shimcha lock shart emas.
/// </summary>
public sealed class EdocsTokenCache
{
    private readonly ConcurrentDictionary<(int OrganizationId, string ProviderCode), (string Token, DateTimeOffset ExpiresAt)> _entries = new();

    public bool TryGet(int organizationId, string providerCode, out string token)
    {
        var key = (organizationId, providerCode);
        if (_entries.TryGetValue(key, out var entry) && DateTimeOffset.UtcNow < entry.ExpiresAt)
        {
            token = entry.Token;
            return true;
        }

        // Muddati o'tgan (yoki umuman topilmagan) yozuvni shu yerda tozalab qo'yamiz —
        // keshda muddati o'tgan yozuvlar cheksiz to'planib qolmasligi uchun. Tashkilotlar
        // to'plami chekli bo'lgani uchun (2500+ emas, cheksiz emas), bu — passiv,
        // har chaqiruvda o'z-o'zini tozalaydigan yetarli mexanizm; alohida fon
        // tozalash jarayoni shart emas.
        _entries.TryRemove(key, out _);

        token = string.Empty;
        return false;
    }

    public void Set(int organizationId, string providerCode, string token, TimeSpan validFor) =>
        _entries[(organizationId, providerCode)] = (token, DateTimeOffset.UtcNow.Add(validFor));

    public void Clear(int organizationId, string providerCode) =>
        _entries.TryRemove((organizationId, providerCode), out _);

    public void Clear() => _entries.Clear();
}
