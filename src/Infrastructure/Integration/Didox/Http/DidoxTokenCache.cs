using System.Collections.Concurrent;

namespace Integration.Didox.Http;

/// <summary>
/// user-key tokenlarni tashkilot (organization_id) bo'yicha, handler'lar orasida
/// saqlaydi. DelegatingHandler'lar HttpClientFactory tomonidan davriy ravishda
/// almashtirilib turadi, shuning uchun token holati handler'ning o'zida emas, shu
/// Singleton keshda saqlanadi (EdocsTokenCache bilan bir xil naqsh — 6.5.6-bosqichda
/// EdocsTokenCache ATAYLAB kalitsiz boshlanib keyin tuzatilgan edi; bu yerda boshidanoq
/// tashkilot bo'yicha kalitlangan). INT_DIDOX.md §2.2: token — UUID, amal muddati
/// 360 daqiqa. <see cref="ConcurrentDictionary{TKey,TValue}"/> o'zi thread-safe.
/// </summary>
public sealed class DidoxTokenCache
{
    private readonly ConcurrentDictionary<int, (string Token, DateTimeOffset ExpiresAt)> _entries = new();

    public bool TryGet(int organizationId, out string token)
    {
        if (_entries.TryGetValue(organizationId, out var entry) && DateTimeOffset.UtcNow < entry.ExpiresAt)
        {
            token = entry.Token;
            return true;
        }

        // Muddati o'tgan (yoki topilmagan) yozuvni shu yerda tozalab qo'yamiz —
        // keshda muddati o'tgan yozuvlar cheksiz to'planib qolmasligi uchun.
        _entries.TryRemove(organizationId, out _);

        token = string.Empty;
        return false;
    }

    public void Set(int organizationId, string token, TimeSpan validFor) =>
        _entries[organizationId] = (token, DateTimeOffset.UtcNow.Add(validFor));

    public void Clear(int organizationId) => _entries.TryRemove(organizationId, out _);

    public void Clear() => _entries.Clear();
}
