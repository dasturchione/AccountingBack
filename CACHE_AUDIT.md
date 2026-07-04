# AccountingBack Cache Audit

Audit scope: faqat cache implementatsiyalari va cache bilan bog‘liq xavflar.

Source of Truth: `D:\Projects\backend\accounting_back`

## Topilgan holat

Kod bazasida real ishlayotgan cache implementatsiyasi deyarli faqat bitta joyda topildi:

- `IMemoryCache` - `FakturaService` ichida access token cache qilish uchun ishlatilgan.

Qolgan cache turlari bo‘yicha kodda foydalanish topilmadi:

- `MemoryCache` alohida instance sifatida topilmadi
- `IDistributedCache` topilmadi
- Redis topilmadi
- `HybridCache` topilmadi
- `ResponseCache` topilmadi
- `OutputCache` topilmadi
- `CacheService` topilmadi
- `CacheHelper` topilmadi
- static cache topilmadi
- dictionary-based cache topilmadi
- lazy cache topilmadi
- singleton cache topilmadi
- boshqa custom cache implementatsiyasi topilmadi

## Cache bo‘yicha detal audit

### 1) `IMemoryCache`

- Cache turi: `IMemoryCache`
- Joylashuvi: `src/Infrastructure/Integration/Faktura/Services/FakturaService.cs`
- Nima vazifa bajaradi: Faktura access token’ni vaqtincha xotirada saqlaydi, har so‘rovda qayta token so‘rashni kamaytiradi
- Production uchun kerakmi: `Ha, lekin juda cheklangan holatda`
- Sababi: tashqi servis tokenini qayta-qayta olishni kamaytiradi va latency’ni tushiradi

Status: `REFACTOR`

Sabablar:
- Cache key hardcoded: `FakturaAccessToken`
- Invalidation yo‘q, faqat expiration’ga tayanadi
- `AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60)` ishlatilgan, bu yerda `ExpiresIn < 60` bo‘lsa noto‘g‘ri/negative expiration xavfi bor
- Cache `singleton` `FakturaService` ichida ishlatilmoqda, lekin service o‘zi bir dona token uchun local memory cache bo‘lib qolgan
- Multi-instance production’da bu cache node-local bo‘ladi, ya’ni boshqa instance’lar bilan share qilinmaydi

Stale data xavfi:
- token muddati noto‘g‘ri hisoblangan bo‘lsa, eski token ishlash xavfi bor
- expiration faqat vaqtga bog‘langan, token revoke bo‘lsa darhol invalidatsiya yo‘q

Clean Architecture:
- Infrastructure qatlamida ishlatilgani normal
- Lekin hardcoded key va expiration mantiqi service ichida qattiq bog‘langan

### 2) `MemoryCache`

- Cache turi: `MemoryCache`
- Joylashuvi: alohida `new MemoryCache(...)` topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: bevosita instance ishlatilmagan

Status: `REMOVE`

Izoh:
- `IMemoryCache` orqali framework cache ishlatilgan, ammo alohida `MemoryCache` object yaratilmagan

### 3) `IDistributedCache`

- Cache turi: `IDistributedCache`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: hozircha yo‘q
- Sababi: kodda distributed cache ishlatilmaydi

Status: `REMOVE`

### 4) Redis

- Cache turi: Redis
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: bu kod bazasida hozircha ishlatilmaydi
- Sababi: Redis integratsiyasi yo‘q

Status: `REMOVE`

### 5) `HybridCache`

- Cache turi: `HybridCache`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q, implementatsiya yo‘q
- Sababi: .NET HybridCache foydalanilmagan

Status: `REMOVE`

### 6) `ResponseCache`

- Cache turi: `ResponseCache`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: controller level response cache atributlari ishlatilmagan

Status: `REMOVE`

### 7) `OutputCache`

- Cache turi: `OutputCache`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: output caching middleware/attribute ishlatilmagan

Status: `REMOVE`

### 8) `CacheService`

- Cache turi: custom `CacheService`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: bunday servis yo‘q

Status: `REMOVE`

### 9) `CacheHelper`

- Cache turi: custom `CacheHelper`
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: bunday helper yo‘q

Status: `REMOVE`

### 10) Static cache

- Cache turi: static cache
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: static cache pattern ishlatilmagan

Status: `REMOVE`

### 11) Dictionary orqali cache

- Cache turi: Dictionary-based cache
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: `Dictionary` faqat lokal lookup va aggregation uchun ishlatilgan, cache sifatida emas

Status: `REMOVE`

### 12) Lazy cache

- Cache turi: `Lazy<T>` cache
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: lazy cache pattern yo‘q

Status: `REMOVE`

### 13) Singleton cache

- Cache turi: singleton cache
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: alohida singleton cache store yo‘q

Status: `REMOVE`

### 14) Boshqa custom cache implementatsiyasi

- Cache turi: custom cache
- Joylashuvi: topilmadi
- Nima vazifa bajaradi: topilmadi
- Production uchun kerakmi: yo‘q
- Sababi: boshqa custom cache layer yo‘q

Status: `REMOVE`

## Cache-related infra audit

- `services.AddMemoryCache()` bor: `src/Infrastructure/DependencyInjection.cs`
- `IFakturaService` singleton sifatida ro‘yxatdan o‘tgan: `src/Infrastructure/Integration/Faktura/Configs/ServiceCollectionExtensions.cs`

Bu yerda ehtiyot bo‘lish kerak:
- `IMemoryCache` app process ichida ishlaydi
- bir nechta instance bo‘lsa shared emas
- token cache uchun ishlashi mumkin, lekin production scale’da invalidation va multi-node yuritish bo‘yicha risk bor

## Duplicate cache

- Duplicate cache topilmadi

## Dead code

- Cache bilan bog‘liq dead code topilmadi
- Ammo cache implementatsiya juda tor va bitta servisga yopishib qolgan

## Hardcoded cache key

- Bor: `FakturaAccessToken`
- Risk: namespace/tenant/region bo‘yicha ajratilmagan

## Invalidation

- Explicit invalidation topilmadi
- Faqat expiration ga tayanilgan
- Bu token revocation yoki credential rotation paytida muammo berishi mumkin

## Stale data ehtimoli

- Token expired bo‘lsa yoki `ExpiresIn` noto‘g‘ri bo‘lsa stale/invalid token ishlashi mumkin
- Multi-instance environment’da bir node’dagi cache boshqaga ko‘chmaydi

## Clean Architecture bahosi

- `IMemoryCache` Infrastructure qatlamida ishlatilgani normal
- Lekin cache strategiya tashqi service implementatsiyasi ichiga singib ketgan
- Qo‘shimcha abstraction yo‘q, shuning uchun testability va evolyutsiya cheklangan

## Yakuniy statuslar

### KEEP

- `IMemoryCache` only for Faktura access token caching, lekin refactor sharti bilan

### REMOVE

- `MemoryCache`
- `IDistributedCache`
- Redis
- `HybridCache`
- `ResponseCache`
- `OutputCache`
- `CacheService`
- `CacheHelper`
- static cache
- Dictionary-based cache
- Lazy cache
- Singleton cache
- boshqa custom cache implementatsiyasi

### REFACTOR

- `IMemoryCache` usage inside `FakturaService`

## Removal plan uchun kandidat

Agar keyingi bosqichda `REMOVE` bo‘yicha ishlansa, removal plan faqat topilmagan cache turlariga emas, cache strategy cleanup sifatida quyidagilarga qaratilishi kerak:

- `FakturaService` cache key strategiyasini ajratish
- token expiration hisobini xavfsiz qilish
- invalidation mantiqini aniq qilish
- multi-instance production talabini baholash
- agar kerak bo‘lsa distributed/shared cache strategiyasiga o‘tish

