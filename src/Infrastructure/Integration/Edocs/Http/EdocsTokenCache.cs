using System.Collections.Concurrent;

namespace Integration.Edocs.Http;

/// <summary>
/// Bearer tokenlarni tashkilot (organization_id) bo'yicha, handler'lar orasida saqlaydi.
/// DelegatingHandler'lar HttpClientFactory tomonidan davriy ravishda almashtirilib
/// turadi (odatiy handler lifetime — 2 daqiqa), shuning uchun token holati handler'ning
/// o'zida emas, shu Singleton keshda saqlanadi. <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// o'zi thread-safe — qo'shimcha lock shart emas.
/// </summary>
public sealed class EdocsTokenCache
{
    private readonly ConcurrentDictionary<int, (string Token, DateTimeOffset ExpiresAt)> _entries = new();

    public bool TryGet(int organizationId, out string token)
    {
        if (_entries.TryGetValue(organizationId, out var entry) && DateTimeOffset.UtcNow < entry.ExpiresAt)
        {
            token = entry.Token;
            return true;
        }

        // Muddati o'tgan (yoki umuman topilmagan) yozuvni shu yerda tozalab qo'yamiz —
        // keshda muddati o'tgan yozuvlar cheksiz to'planib qolmasligi uchun. Tashkilotlar
        // to'plami chekli bo'lgani uchun (2500+ emas, cheksiz emas), bu — passiv,
        // har chaqiruvda o'z-o'zini tozalaydigan yetarli mexanizm; alohida fon
        // tozalash jarayoni shart emas.
        _entries.TryRemove(organizationId, out _);

        token = string.Empty;
        return false;
    }

    public void Set(int organizationId, string token, TimeSpan validFor) =>
        _entries[organizationId] = (token, DateTimeOffset.UtcNow.Add(validFor));

    public void Clear(int organizationId) => _entries.TryRemove(organizationId, out _);

    public void Clear() => _entries.Clear();
}
