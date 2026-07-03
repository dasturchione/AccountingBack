---
tags: [accounting, backend, implementation, accounting-core]
created: 2026-07-03
source: Claude — Accounting Core Implementation (band 1-8, approved plan)
stage: 3/10 (Implementation) — Accounting Core only
---

# Accounting Core — Implementation Report

> Tasdiqlangan PLAN bo'yicha, bandma-band. Har band tugagach build tekshirildi (hammasi 0 error).
> Ledger / Trial Balance / Closing / Reposting / Reports — YOZILMADI (keyingi bosqichlar).

## Files changed

| # | Fayl | Band |
|---|---|---|
| 1 | `src/Presentation/WebApi/Controllers/Acc/ChartAccountController.cs` | 1 |
| 2 | `src/Application/Features/Register/PostingEngines/Services/PostingService.cs` | 2, 5, 8 |
| 3 | `src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs` | 3 |
| 4 | `src/Application/Features/Register/AccountingRegisterEntries/Services/IAccountingRegisterEntryService.cs` | 4 |
| 5 | `src/Application/Features/Register/AccountingRegisterEntries/Services/AccountingRegisterEntryService.cs` | 4, 8 |
| 6 | `src/Application/Features/Register/AccountingRegisterEntries/DTOs/AccountingRegisterEntryDtos.cs` | 4 |
| 7 | `src/Application/Features/Register/AccountingRegisterEntries/Errors/AccountingRegisterEntryErrors.cs` | 5 |
| 8 | `src/Application/Features/Register/AccountingRegisterEntries/Services/AccountingDispatcher.cs` | 5 |
| 9 | `src/Presentation/WebApi/Controllers/Register/AccountingRegisterEntryController.cs` | 8 |

**Verify-only (kod o'zgarmadi):** Band 6 (7 ta lifecycle service), Band 7 (`AppDbContext.OrganizationScope.cs`).

## Production issues fixed + Why each necessary

**Band 1 — Chart of Accounts Security.** `ChartAccountController` Create/Update/Delete: `[ModuleAuthorize(ChartAccount…)]` → `[GlobalAccessAuthorize]`. GET o'zgarmadi.
*Nega:* ChartAccount global (tashkilotsiz) jadval; oldin istalgan tashkilotning oddiy useri barcha tashkilotlar ishlatadigan umumiy hisoblar rejasini o'zgartira/o'chira olardi. Endi faqat Platform/System admin (HasGlobalAccess) yoza oladi, org user faqat READ. **Route va DTO o'zgarmadi** — API contract buzilmadi, faqat write-authorization qattiqlashdi.

**Band 2 — Accounting Policy real ishladi.** `PostingService.GetRule`: resolve rule endi `PolicyId == context.AccountingPolicyId` bo'yicha ustuvor tanlanadi, topilmasa alias-only fallback (backward-compatible).
*Nega:* `PostingContext.AccountingPolicyId` yozilardi lekin hech qayerda o'qilmasdi (dead). Endi policy amalda hisob tanlashni boshqaradi; IFRS/GAAP kelajakda faqat yangi PolicyId + resolve rule qatorlari bilan qo'shiladi (kod o'zgarmaydi). Fallback tufayli mavjud data buzilmaydi — **SQL kerak bo'lmadi**.

**Band 3 — Optimistic Concurrency.** `ChartAccount`, `AccountingPeriod`, `PostingBatch` ga `xmin` RowVersion (mavjud `ProductTable` patterni).
*Nega:* Concurrency token yo'q edi → `CommandRepository`dagi `DbUpdateConcurrencyException` handling hech qachon ishlamasdi (parallel tahrirlashda "last write wins"). `xmin` — PostgreSQL tizim ustuni, **schema o'zgarishi/SQL kerak emas**.

**Band 4 — Accounting Register Cleanup.** `IAccountingRegisterEntryService` + `AccountingRegisterEntryService` dan `CreateAsync`/`UpdateAsync`/`DeleteAsync` (soxta delete — "Success" qaytarardi, aslida o'chirmasdi) o'chirildi; orphan `AccountingRegisterEntryCreateDto`/`UpdateDto` o'chirildi; endi ishlatilmaydigan `ICommandRepository<AccountingRegisterEntry>` (`_command`) dependency olib tashlandi.
*Nega:* Ledger append-only bo'lishi va faqat Posting Engine (`AccountingDispatcher`) orqali boshqarilishi kerak. Bu metodlar Posting Engine'ni chetlab muvozanatsiz yozuv yaratish/o'chirish imkonini berardi — xavfli o'lik kod.

**Band 5 — Validation.** (a) `AccountingDispatcher.EnsureNoGroupAccountsAsync` — resolve qilingan Debit/Credit hisoblar orasida `IsGroup=true` bo'lsa posting bloklanadi (`GroupAccountNotPostable` xatosi). (b) `PostingService.GetRule` — resolve rule topilmasa `Sequence contains no elements` crash o'rniga aniq business xato (qaysi alias uchun yo'qligini aytadi).
*Nega:* Professional ERP'da faqat "barg" hisoblarga postiruvka ruxsat etiladi; guruh (jamlovchi) hisobga to'g'ridan-to'g'ri yozuv balansni buzadi. Mavjud validatsiyalar (Debit≠Credit, amount>0, currency, quantity balance, closed period, lock, duplicate) o'zgartirilmadi.

**Band 6 — Transaction Safety (verify).** 7 ta lifecycle service (Purchase/Sale/Cash/Bank/WarehouseTransfer/InventoryAdjustment/InventoryCount) — Confirm va Cancel `ExecuteInTransactionAsync` bilan o'ralgan (14/14), har birida advisory lock (`_postingLock.AcquireAsync`, 2/2), posting va reversal tranzaksiya ichida. Atomicity/rollback to'g'ri — **kod o'zgartirilmadi**.

**Band 7 — Multi-Tenant (verify).** `AppDbContext.OrganizationScope` **fail-closed** (`AllowedOrgIds.Count > 0 && …`; write'da bo'sh ro'yxat → exception; Modified/Deleted uchun original OrgId tekshiriladi; OrgId modifikatsiya bloklangan). Barcha register entity + `PostingBatch` + `AccountingPeriod` scoped. `ChartAccount` ataylab global (Band 1 himoya qildi). Auditdagi "fail-open" da'vosi eskirgan edi — **kod o'zgartirilmadi**.

**Band 8 — Production Cleanup.** `AccountingRegisterEntryController` dagi commentlangan endpoint bloklar (GetAll/GetById/Create/Update/Delete), service'dagi commented `//if (!items.Any())`, va `PostingService.IsMatchingRequiredAliases` (hech qayerda chaqirilmagan o'lik metod) o'chirildi.

## Build result
`dotnet clean` + `dotnet build Accounting.slnx` → **Build succeeded, 0 Error(s), 1 Warning** (mavjud MSB3277 reference-conflict ogohlantirishi — bu o'zgarishlardan emas, oldindan bor).

## Test result
`dotnet test Accounting.slnx` → **UnitTests: 52/52 Passed · IntegrationTests: 19/19 Passed** (0 failed, 0 skipped). Mavjud biznes-logika va testlar buzilmadi.

## SQL written to 0000.sql
**Yo'q.** Hech qanday DB o'zgarishi talab qilinmadi: Band 3 `xmin` — PostgreSQL tizim ustuni (schema o'zgarmaydi); Band 2 backward-compatible fallback (seed data'ga tegilmadi). EF migration yaratilmadi. Domain = SQL = Database tengligi saqlanadi.

## Remaining blockers
- Yo'q (Accounting Core doirasida). 
- Kelajak eslatma: Band 2 policy-aware resolution faqat resolve rule'larda `policy_id` to'g'ri to'ldirilgan bo'lsa "kuchayadi"; hozirgi fallback tufayli xavfsiz. IFRS/parallel accounting kerak bo'lsa — kelajak bosqichda per-org policyni Organization Setup'dan o'qish (hozir builder'larda `STANDARD_UZ` default) qo'shiladi.

## Progress (baholash)
- **Accounting Core:** ~72% → **~90%** (security, policy, concurrency, register integrity, validation yakunlandi; per-org policy sourcing va CQRS/DDD chuqurlashtirish keyinга qoldi).
- **Accounting module (umumiy):** ~45% → **~52%** (Ledger/Trial Balance/Closing/Reports hali oldinda).
- **Professional ERP readiness (accounting):** ~48% → **~60%**.
- **Backend (umumiy):** ~52% → **~55%**.
