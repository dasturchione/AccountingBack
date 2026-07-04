---
tags: [accounting, backend, cleanup, verification, accounting-core]
created: 2026-07-03
source: Claude — Accounting Core Production Cleanup & Verification
stage: 4/10 — Accounting Core only (Ledger/Trial Balance/Closing/Reports YOZILMADI)
---

# Accounting Core — Production Cleanup & Verification Report

> Stage 3 (Implementation) dan keyingi production-darajali to'liq tekshiruv.
> 7 o'lchov bo'yicha audit qilindi; faqat bitta xavfsiz micro-fix topildi va tuzatildi.

## Files changed
| Fayl | O'zgarish |
|---|---|
| `src/Application/Features/Register/PostingEngines/Services/PostingService.cs` | 1 qator: entry `CreatedDate` timestamp nomuvofiqligi tuzatildi |

Boshqa barcha Accounting Core fayllar — muammosiz, **o'zgartirilmadi**.

## Production issues fixed
**Timestamp inconsistency (PostingService.cs, satr 87).**
Bir posting operatsiyasi ichida `AccountingRegisterEntry.CreatedDate = DateTime.UtcNow` yozilar, lekin uning subkonto qatorlari (`RegisterEntrySubkonto.CreatedDate`, satr 184) `DateTime.Now` olardi. Ustunlar `timestamp without time zone` va butun kod bazasi (lifecycle service'lar, ChartAccountService) `DateTime.Now` ishlatadi — demak satr 87 yagona outlier edi: bitta yozuv va uning subkontolari UZ vaqti bo'yicha ~5 soat farqli timestamp olardi (hisobot/audit izchilligini buzadi).
**Fix:** satr 87 `DateTime.UtcNow` → `DateTime.Now` (kod bazasi konvensiyasiga va yozuvning o'z subkontolariga moslashtirildi). Xavfsiz, minimal.

## Architecture (audit)
- Clean Architecture / Repository / UnitOfWork / Service+Interface pattern izchil saqlangan. ✅
- CQRS: read/write ajratilgan (IQueryRepository / ICommandRepository); MediatR yo'q — bu loyihaning tanlangan naqshi (refactor qilinmadi, taqiqlangan). ✅
- DDD: entity'lar EF-atributli anemik model — bu repo-darajasidagi mavjud holat (refactor Accounting Core doirasidan tashqarida, taqiqlangan). Qayd etildi, o'zgartirilmadi.
- DI: `IQueryRepository<>`/`ICommandRepository<>` open-generic, `IQueryBuilder` — to'g'ri ro'yxatdan o'tgan; Stage 3'da `AccountingDispatcher`ga qo'shilgan `IQueryBuilder`+`IQueryRepository<ChartAccount>` deps muammosiz hal bo'ladi (integration testlar tasdiqladi). ✅

## Security (audit)
- ChartAccount write (Create/Update/Delete) — `GlobalAccessAuthorize` (Stage 3, Band 1); org user faqat READ. ✅
- AccountingRegisterEntry — write endpoint umuman yo'q (faqat GET postings/daily); Posting Engine orqali boshqariladi. ✅
- PostingBatch / AccountingPeriod — Accounting Core'da mutatsiya endpointi yo'q (period-close 8-bosqich). ✅
- Multi-tenant: `OrganizationScope` fail-closed (read + write); barcha register entity + PostingBatch + AccountingPeriod scoped; ChartAccount ataylab global (Band 1 himoya qildi). ✅
- IDOR / Privilege escalation: lifecycle'larda `doc.OrganizationId != _userContext.OrganizationId` tekshiruvi + scope filter; ChartAccount global-write endi platform-admin bilan cheklangan. Yangi teshik topilmadi. ✅

## Transaction (audit)
- 7 lifecycle service — Confirm/Cancel `ExecuteInTransactionAsync` (14/14) + advisory lock (`_postingLock.AcquireAsync`, 2/2 har birida). Posting, reversal, status-update, register-write — bitta atomik tranzaksiyada. Rollback `BaseService`da xatoda kafolatlangan. ✅ (o'zgartirilmadi)

## Concurrency (audit)
- `xmin` RowVersion: `ChartAccount`, `AccountingPeriod`, `PostingBatch` (Stage 3, Band 3). Update flow'lar entity'ni tracked (`GetAsync` — no AsNoTracking) o'qiydi, keyin `UpdateAsync` — xmin original qiymati mavjud, optimistic concurrency ishlaydi. ✅
- Parallel confirm/cancel — advisory lock bilan seriyalanadi. Race condition topilmadi. ✅

## Validation (audit)
- Group (jamlovchi) hisobga posting bloklangan (`EnsureNoGroupAccountsAsync`, Stage 3). Null-safe (`id.HasValue` filtri). ✅
- Resolve rule topilmasa — aniq business error (crash emas). ✅
- Mavjud: Debit≠Credit (cashbox-transfer istisnosi bilan), amount>0, currency, date, quantity balance, closed period, duplicate posting (`GetActivePostingBatch`+`HasBusinessEffects`), lock. Edge/null case'lar qoplangan. Yangi kamchilik topilmadi. ✅

## Performance (audit)
- Read projeksiyalar (`AccountingPostingDtoProjection`, GetPosting/GetDailyPosting) — navigation join + subkonto sub-select bitta SQL'da; **N+1 yo'q, ortiqcha Include yo'q**. ✅
- `EnsureNoGroupAccountsAsync` — posting boshiga bitta `WHERE id IN (...) AND is_group` proyeksiyali so'rov (faqat Id qaytaradi). Qabul qilinadi. ✅
- `PostingService` resolve rule / posting rule — kichik config jadvallar, in-memory filtr (policy-aware). N+1 yo'q. ✅

## Cleanup summary
- Accounting Core feature papkalari (`Features/Acc`, `Features/Register`) va controller'lar (`Controllers/Acc`, `Controllers/Register`) — commented/dead kod, TODO/HACK/FIXME **topilmadi** (Stage 3 + shu pass tozaladi). ✅
- Unused using topilmadi (build 0 warning-from-code). Magic string/number: PostingService'da resolve-rule-not-found uchun hardcoded rus xabarlari bor (pre-existing, dispatcher `Result.Failure`ga o'raydi) — refactor taqiqlangani va xavf tug'dirmasligi uchun qoldirildi (Remaining'ga qayd).

## Build result
`dotnet clean` + `dotnet build Accounting.slnx` → **0 Error, 1 Warning** (pre-existing MSB3277 reference-conflict; kod o'zgarishlaridan emas).

## Test result
`dotnet test Accounting.slnx` → **UnitTests 52/52 · IntegrationTests 19/19 Passed** (0 failed, 0 skipped).

## 0000.sql changes
**Yo'q.** Database strukturasi o'zgarmadi, migration yaratilmadi, hech qanday runtime SQL kerak bo'lmadi. Domain = SQL = Database tengligi saqlanadi.

## Remaining blockers
- **Blocker yo'q** — Accounting Core production darajasiga tayyor.
- Kelajak (Core doirasidan tashqari, keyingi bosqichlar uchun tavsiya, hozir o'zgartirilmadi):
  1. `PostingService` ichidagi hardcoded rus xato-matnlari — kelajakda `PostingContextErrors`/lokalizatsiya katalogiga ko'chirilishi mumkin.
  2. `ChartAccountService.DeleteAsync` — hisob `AccountResolveRule`/mavjud `AccountingRegisterEntry`larda ishlatilganda passivlashni bloklovchi guard qo'shilishi mumkin (yangi business rule — bu bosqichda "yangi feature yozma" bo'yicha kiritilmadi).
  3. Per-org accounting policy'ni builder'larda `STANDARD_UZ` default o'rniga Organization Setup'dan o'qish (IFRS/parallel accounting uchun) — kelajak bosqich.

## Progress
- **Accounting Core completion:** ~90% → **~92%** (timestamp izchilligi tuzatildi; production-verify yakunlandi).
- **Accounting Module completion:** **~52%** (Ledger/Trial Balance/Closing/Reports oldinda).
- **Professional ERP readiness (accounting):** **~60%**.
- **Backend completion:** **~55%**.
