---
tags: [accounting, backend, audit, erp, accounting-core]
created: 2026-07-03
source: Claude — Accounting Core deep audit (read-only, no code changed)
scope: Chart of Accounts, Journal Entries (Provodka/AccountingRegisterEntry), Posting Engine, Accounting Domain (Period/Transaction/Tenant)
---

# Accounting Core Audit

> Metodologiya: faqat o'qish. Hech qanday fayl o'zgartirilmadi, hech qanday feature/entity/API qo'shilmadi. Har bir topilma haqiqiy fayldan o'qilgan kodga asoslangan.
>
> **Muhim eslatma:** Ushbu audit oldingi umumiy (ChatGPT Codex) auditdan farqli, ko'proq batafsil manba kodini o'qib chiqarilgan. Ba'zi joylarda oldingi auditning xulosalari **noto'g'ri yoki eskirgan** ekanligi aniqlandi (masalan tenant izolyatsiya va tranzaksiya xavfsizligi haqida) — bu farqlar quyida aniq ko'rsatilgan.

---

## 1. Chart of Accounts

**Fayllar:** `Domain/Entities/Acc/ChartAccount.cs`, `ChartAccountSubkonto.cs`, `AccountType.cs`, `Application/Features/Acc/ChartAccounts/**`, `Presentation/WebApi/Controllers/Acc/ChartAccountController.cs`

- 🟢 **Domain Model** — `ChartAccount` (parent/child ierarxiya, `IsGroup`, `AccountTypeId`, `IsQuantity`, `IsCurrency` flags) — standart COA modeli sifatida to'g'ri tuzilgan.
- 🟢 **Entity Relationships** — `ParentId` self-reference, `AccountResolveRules`, `ChartAccountSubkontos`, debit/credit register entry inverse collections — to'g'ri FK va indexlar bilan.
- 🔴 **Multi-tenant / Organization Scope** — `ChartAccount.cs`da `OrganizationId` ustuni umuman yo'q, va `AppDbContext.OrganizationScope.cs:54` da `//ApplyScopedFilter<ChartAccount>(modelBuilder);` **qasddan comment qilingan** — ya'ni Chart of Accounts butun platforma bo'ylab **global/umumiy** jadval. Muammo: `ChartAccountController.cs` dagi Create/Update/Delete harakatlari oddiy tashkilot-darajasidagi `ModuleAuthorize(PermissionCodeConst.ChartAccountCreate/Update/Delete)` bilan himoyalangan (`GlobalAccessAuthorize` emas) — ya'ni bitta tashkilotning oddiy buxgalteri o'ziga berilgan ruxsat bilan **barcha boshqa tashkilotlar ishlatayotgan umumiy hisoblar rejasini** o'zgartirishi yoki o'chirib qo'yishi mumkin. Bu production uchun jiddiy xavf.
- 🔴 **Business Rules / Validation** — `ChartAccountBaseDtoValidator.cs`: faqat `Code`/`Name` NotEmpty+MaxLength tekshiriladi. Yo'q narsalar: (1) `IsGroup=true` hisobga to'g'ridan-to'g'ri provodka tushishini bloklovchi qoida yo'q (professional ERP'larda guruh hisobiga postiruvka taqiqlanadi — pastda Posting Engine bo'limida ham tasdiqlangan), (2) hisob o'chirilishi/passivlanishidan oldin u qaysidir `AccountResolveRule` yoki mavjud `AccountingRegisterEntry`larda ishlatilganini tekshirish yo'q (`ChartAccountService.cs:48-59` `DeleteAsync`), (3) `Code` formati/ierarxiyaviy izchillik tekshiruvi yo'q (masalan bola kodning ota kod prefiksiga mosligi).
- 🟡 **Soft Delete** — `DeleteAsync` haqiqiy o'chirish emas, `StateId = PASSIVE` qiladi (yaxshi amaliyot), lekin `DeletedAt`/`DeletedByUserId` maydonlari yo'q — kim va qachon passivlaganini bilib bo'lmaydi.
- 🔴 **Audit Log** — `ChartAccountService.cs` da `IAuditLogService` umuman chaqirilmaydi (Purchase/Sale/Cash/Bank lifecycle servislarida bor). Hisoblar rejasidagi o'zgarishlar (kod, nom, holat) audit izsiz qoladi — moliyaviy tizim uchun bu muhim kamchilik.
- 🟢 **Permissions** — CRUD amallari uchun granular `ModuleAuthorize` kodlari bor (`ChartAccountView/ViewDetail/Create/Update/Delete`), lekin yuqoridagi multi-tenant band bilan birga o'qilishi kerak.
- 🟢 **Error Handling** — `Result`/`Error` pattern izchil ishlatilgan, `ChartAccountErrors.NotFound/CodeConflict` aniq xato turlari bilan.
- 🔴 **Clean Architecture / DDD** — `ChartAccount.cs` to'g'ridan-to'g'ri `Microsoft.EntityFrameworkCore` atributlari (`[Table]`, `[Column]`, `[ForeignKey]`, `[InverseProperty]`, `[Index]`) bilan belgilangan; Domain'da hech qanday biznes metodi yo'q (faqat public setter'lar) — anemik domain model, barcha mantiq `ChartAccountService`da.
- 🔴 **CQRS** — MediatR/IRequestHandler yo'q, oddiy service+interface pattern (butun repo bo'ylab izchil, faqat shu yerda qayd etildi).

---

## 2. Journal Entries (Provodka / `AccountingRegisterEntry`)

**Fayllar:** `Domain/Entities/Register/AccountingRegisterEntry.cs`, `RegisterEntrySubkonto.cs`, `Domain/Entities/Acc/PostingBatch.cs`, `Application/Features/Register/AccountingRegisterEntries/**`

- 🟢 **Domain Model** — `AccountingRegisterEntry`: debit/credit hisob, valyuta, summa, miqdor (debit/credit quantity), `PostingBatchId`, `SourceLineId`, `ReversalEntryId` — professional darajadagi ledger yozuvi strukturasi (1C/SAP'dagi "проводка" konseptiga juda yaqin).
- 🟢 **Business Rules — Reversal** — `PurchaseLifecycleService.cs:426-472` (`ReverseAccountingEntriesAsync`): bekor qilishda yozuvlar **o'chirilmaydi**, aksincha debit/credit almashtirilgan yangi "reversal" yozuv yaratiladi va asl yozuvga `ReversalEntryId` orqali bog'lanadi. Bu to'g'ri buxgalteriya amaliyoti (haqiqiy auditga bardoshli tarix saqlanadi) — bir nechta modulda (Purchase, va grep natijasiga ko'ra Sale/Bank/Cash/Warehouse Transfer/Inventory Adjustment/Count) izchil qo'llanilgan.
- 🔴 **Dangerous dead code** — `AccountingRegisterEntryService.cs`:
  - `DeleteAsync` (satr 48-57): haqiqiy o'chirish qatori commentlangan (`//await _command.DeleteAsync(entity, ct);`) lekin baribir `Result.Success()` qaytaradi — ya'ni chaqiruvchiga "o'chirildi" deb yolg'on javob beradi. Hozircha controller'da bu route ochilmagani uchun xavfsiz, lekin kelajakda birov shu metodni route'ga bog'lasa, foydalanuvchi "o'chirdim" deb o'ylaydi, aslida hech narsa o'zgarmaydi.
  - `CreateAsync`/`UpdateAsync` (satr 29-46, 104-122): ledger yozuvini **Posting Engine'ni chetlab o'tib**, to'g'ridan-to'g'ri qo'lda yaratish/tahrirlash imkonini beruvchi metodlar servis darajasida mavjud. Bular ham hech qanday controller'ga bog'lanmagan (o'lik kod), lekin mavjudligi o'zi xavf — muvozanatlashtirilmagan yoki noto'g'ri hisobli yozuv yaratish imkoniyati servis qatlamida ochiq turibdi.
- 🟢 **Multi-tenant / Organization Scope** — `AccountingRegisterEntry` uchun `AppDbContext.OrganizationScope.cs:67`da `ApplyScopedFilter<AccountingRegisterEntry>(modelBuilder)` **faol** — ChartAccount'dan farqli, bu yerda tashkilot izolyatsiyasi to'g'ri ishlaydi.
- 🟡 **Audit Log** — Yozuv yaratilishi alohida audit qilinmaydi (faqat uni keltirib chiqargan hujjatning Confirm/Cancel harakati audit qilinadi) — bilvosita yetarli, lekin ledger darajasida to'g'ridan-to'g'ri audit yo'q.
- 🟢 **Error Handling** — `AccountingRegisterEntryErrors.NotFound` va h.k. — Result pattern izchil.
- 🔴 **Clean Architecture / DDD** — xuddi Chart of Accounts kabi, EF-bog'liq anemik entity.

---

## 3. Posting Engine (`PostingContext` / `PostingService` / `PostingContextDispatcher` / `AccountingPostingValidator` / Context Builder'lar)

**Fayllar:** `Application/Features/Register/PostingEngines/**` (`PostingService.cs`, `PostingContextDispatcher.cs`, `AccountingPostingValidator.cs`, `Models/PostingContext.cs`, `Builders/SaleDocContextBuilder.cs`, `Builders/CashOperationContextBuilder.cs`, va boshqalar)

- 🟢 **Architecture** — Alias + Template (`PostingRule`/`PostingRuleLine`) + Resolve Rule (`AccountResolveRule`, dimension-based: `ProductCategory`/`ServiceType`/`PaymentMethod`/`AssetType` → `_default` fallback) arxitekturasi — 1C'dagi "Регистр бухгалтерии + План счетов" yondashuviga juda yaqin, yaxshi factoring. Har hujjat turi uchun alohida `IPostingContextBuilder<T>` (Sale, Purchase, Cash, Bank va h.k.) — Strategy pattern.
- 🟢 **Debit/Credit Logic** — `PostingService.BuildEntriesAsync` (`PostingService.cs:22-122`): har bir shablon qatori uchun alohida Debit/Credit alias'ni haqiqiy `ChartAccount.Id`ga aylantiradi, `AmountSource` orqali summa moslashtiriladi, subkonto (o'lchov) qatorlari to'g'ri tomonlarga (`DT`/`CT`) biriktiriladi. Har bir yozuv bitta `Amount` bilan ham debit, ham kredit tomonini birga ifodalaydi — shu sababli batch darajasida alohida "debit jami = kredit jami" tekshiruvi shart emas (struktura bo'yicha muvozanatlangan).
- 🟡 **Business Rules — Missing** — Hech qayerda (`AccountingPostingValidator.cs`, `PostingService.cs`) quyidagi tekshiruvlar yo'q:
  1. Debit/Credit sifatida tanlangan hisob `IsGroup = true` (guruh/sarlavha hisob) emasligi — professional ERP'larda faqat "barg" hisoblarga postiruvka ruxsat etiladi, bu yerda bunday himoya yo'q.
  2. `ChartAccount.IsQuantity`/`IsCurrency` bayroqlari bilan yozuvdagi `DebitQuantity`/`CurrencyId` mosligi tekshirilmaydi.
- 🔴 **Error Handling — Unhandled exception risk** — `PostingService.cs:144-161` (`GetRule` metodi): agar berilgan `alias` uchun `AccountResolveRule` jadvalida **hech qanday** qoida topilmasa (na aniq dimension, na `_default`), kod `.OrderBy(o => o.Priority).First()` ni bo'sh ro'yxatga chaqiradi → `InvalidOperationException: Sequence contains no elements` — bu tipik `Result.Failure` business xatosi emas, balki qo'lga olinmagan runtime exception. Tranzaksiya `BaseService`da ushlanib rollback qilinadi (ma'lumot buzilmaydi), lekin foydalanuvchiga tushunarsiz 500-xato qaytadi (masalan yangi tashkilot `AccountResolveRule`larini to'liq sozlamagan bo'lsa, birinchi hujjatni tasdiqlashda tushunarsiz crash bo'ladi).
- 🔴 **Hardcoded Accounting Policy — funksional bo'shliq** — Barcha ko'rilgan Context Builder'larda (`SaleDocContextBuilder.cs:159`, `CashOperationContextBuilder.cs:32`, va boshqalarda ham bir xil pattern) `AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ` **qattiq yozilgan**. Ammo `Organization Setup` oqimida (`OrganizationSetupAccountingPolicyDto.AccountingPolicyId`) tashkilot o'ziga policy tanlashi mumkin (NSBU/IFRS va h.k. uchun poydevor bor). Demak schema/DTO darajasida ko'p-siyosat (multi-policy) qo'llab-quvvatlanadi deb ko'rinsa-da, **Posting Engine buni hech qachon o'qimaydi** — barcha tashkilotlar amalda faqat STANDARD_UZ bo'yicha provodka oladi. Bu "professional ERP" (IFRS parallel accounting) darajasiga yetmaydi.
- 🟡 **SOLID / Open-Closed** — Yangi hujjat turi qo'shish uchun: yangi `IPostingContextBuilder<T>` klassi + DI registratsiyasi + `PostingRuleIdConst` bilan C# switch mantig'i kerak (`CashOperationContextBuilder.cs:52-57`dagi kabi) — qaysi shablon ishlatilishini tanlash logikasi ma'lumotlar bazasida emas, kodda. Hisob mapping (AccountResolveRule) konfiguratsiya bo'lsa-da, "qaysi shablon qachon ishlatiladi" hali kod darajasida.
- 🟡 **Architecture — Reflection-based dispatch** — `PostingContextDispatcher.cs:26-57`: builder DI orqali topiladi, lekin `BuildAsync` chaqiruvi `GetMethod`+`Invoke` (reflection) orqali amalga oshiriladi — ishlaydi, lekin kompilyatsiya vaqtida tip xavfsizligi yo'q va oddiy DI-based strategy'ga nisbatan sekinroq/fragile.
- 🟢 **Transaction Safety / Multi-tenant** — Engine o'zi holatsiz (stateless); tranzaksiya va tashkilot konteksti chaqiruvchi lifecycle servisdan keladi — to'g'ri ajratilgan.

---

## 4. Accounting Domain (Period Close, Transaction Safety, Tenant Isolation — kesishuvchi)

**Fayllar:** `Domain/Entities/Acc/AccountingPeriod.cs`, `Application/Features/Acc/AccountingPeriods/Services/AccountingPeriodValidator.cs`, `Application/Features/BaseService.cs`, `Infrastructure/Repositories/UnitOfWork.cs`, `Infrastructure/Services/DocumentPostingLock.cs`, `Infrastructure/Persistence/AppDbContext/AppDbContext.OrganizationScope.cs`, `Infrastructure/Repositories/CommandRepository.cs`

> ⚠️ **Oldingi (ChatGPT) auditni tuzatish:** Oldingi audit hujjatida "Transaction zaif — Unit of Work buziladi" va "Tenant izolyatsiyasi fail-open" deb yozilgan edi. Kodni chuqur o'qib chiqqanda, bu ikkala band ham **hozirda to'g'irlangan/noto'g'ri** ekanligi aniqlandi (pastda isboti bilan). Bu — yangi audit, eski xulosalarni ko'r-ko'rona takrorlamaslik kerak.

- 🟢 **Business Rules — Period Close** — `AccountingPeriodValidator.EnsureOpenAsync` (`AccountingPeriodValidator.cs:20-37`) **barcha 7 ta** hujjat lifecycle servisida chaqiriladi (grep tasdiqlagan: `PurchaseLifecycleService`, `SaleLifecycleService`, `CashLifecycleService`, `BankLifecycleService`, `WarehouseTransferLifecycleService`, `InventoryAdjustmentLifecycleService`, `InventoryCountLifecycleService`). Masalan `PurchaseLifecycleService.cs:128` (Confirm) va `203-209` (Cancel) — Cancel'da **ikkalasi** ham tekshiriladi: asl hujjat sanasi VA joriy (bekor qilish) sanasi uchun davr ochiqligi — bu professional darajadagi to'g'ri amaliyot.
- 🟢 **Transaction Safety** — `UnitOfWork.cs:16-22` (`BeginAsync`) haqiqiy `_context.Database.BeginTransactionAsync` chaqiradi; `BaseService.cs:61-88` (`ExecuteWithTransactionAsync`) butun Confirm/Cancel operatsiyasini shu tranzaksiya ichiga oladi va xato bo'lsa `RollbackAsync` qiladi. Bir nechta `CommandRepository.SaveChangesAsync` chaqiruvi (masalan posting batch yaratish + register entry yaratish + hujjat statusini yangilash) bitta atomik tranzaksiyaga birlashgan — bu haqiqatda **Unit of Work to'g'ri ishlaydi** degani, oldingi audit xulosasidan farqli.
- 🟡 **Transaction Safety — dizayn zaifligi** — Yuqoridagi himoya **majburiy emas, kelishuv asosida (opt-in)**: har bir yangi service o'zi `ExecuteInTransactionAsync` bilan o'rashni "eslab qolishi" kerak; arxitektura buni kompilyator darajasida majburlamaydi. Kimdir yangi lifecycle service yozib shu wrapper'ni unutsa, tranzaksiya himoyasiz qoladi.
- 🟢 **Concurrency (Document-level)** — `DocumentPostingLock.cs:16-20`: PostgreSQL `pg_advisory_xact_lock` orqali har bir hujjat (`documentTypeId`+`documentId`) uchun tranzaksiya-darajasidagi lock olinadi (Confirm/Cancel boshida) — ikki foydalanuvchi bir hujjatni bir vaqtda tasdiqlashga urinishining oldini oladi, avtomatik tranzaksiya tugagach bo'shaydi. Yaxshi tanlov.
- 🔴 **Concurrency (Row-level) — yo'q** — Butun `Domain` bo'ylab (grep bilan tekshirildi) hech qanday `RowVersion`/`[Timestamp]`/`[ConcurrencyCheck]` maydoni topilmadi. `CommandRepository.cs:74-77, 113-116` da `DbUpdateConcurrencyException` ushlab `OptimisticConcurrencyException`ga aylantiradigan kod bor, lekin concurrency token yo'qligi sababli bu catch-blok amalda **hech qachon ishga tushmaydi** (EF Core konkurentlikni faqat token bo'lsa aniqlaydi) — oddiy "draft" hujjatni ikki foydalanuvchi bir vaqtda tahrirlasa, "last write wins" bo'ladi, hech qanday ogohlantirishsiz.
- 🟢 **Multi-tenant / Organization Scope — umumiy mexanizm** — `AppDbContext.OrganizationScope.cs:27-35` (`ApplyScopedFilter`): `AllowedOrgIds.Count > 0 && (...)` sharti bilan — agar foydalanuvchining ruxsat berilgan tashkilot ro'yxati **bo'sh bo'lsa, hech qanday yozuv qaytarilmaydi** (fail-closed), oldingi audit aytgan "bo'sh = cheklovsiz kirish" holati **hozir mavjud emas**. Yozish tomonida `EnforceOrganizationScope` (`AppDbContext.OrganizationScope.cs:179-225`) yanada qattiqroq: `AllowedOrgIds.Count == 0` bo'lsa **exception tashlaydi**, Modified/Deleted yozuvlar uchun **original** OrganizationId ruxsat etilgan ro'yxatda ekanligini tekshiradi, va `OrganizationId`ni Modified holatda **o'zgartirib bo'lmaydigan** qilib belgilaydi (`entry.Property(OrgIdProperty).IsModified = false`) — bu boshqa tashkilot yozuvini "qayta egallab olish" (re-assign) hujumidan himoya qiladi. Bu — professional darajadagi tenant izolyatsiya.
- 🔴 **Multi-tenant — Chart of Accounts istisnosi** — Yuqoridagi mexanizm **ChartAccount'ga qo'llanilmaydi** (1-bo'limda batafsil yozilgan) — bu Accounting Domain doirasidagi yagona, lekin jiddiy tenant-izolyatsiya teshigi.
- 🟢 **Audit Log** — `PurchaseLifecycleService.cs:143-145, 170-175, 215-217, 264-269` namunasida: Confirm/Cancel'dan oldin `SetOldValues`, keyin `SetNewValues` + `CreateAsync(AuditLogTableConst..., ..., "Confirmed"/"Cancelled")` — barcha 7 ta lifecycle servisda izchil ishlatilgan pattern (grep bilan tasdiqlangan struktura bir xil).
- 🔴 **Clean Architecture / DDD** — `Domain.csproj` to'g'ridan-to'g'ri `Microsoft.EntityFrameworkCore` (10.0.0) paketiga bog'liq (`<PackageReference Include="Microsoft.EntityFrameworkCore" .../>`). Butun `Domain/Entities/Acc` va `Domain/Entities/Register` papkalaridagi barcha klasslar EF Core atributlari bilan yozilgan — Domain qatlami ORM'dan mustaqil emas (Clean Architecture'ning asosiy qoidasi buzilgan). Bundan tashqari entity'larda hech qanday invariant-enforcing konstruktor/metod yo'q — hammasi public settable property (anemik domain model, DDD taktik naqshlari — Aggregate Root, Value Object, invariant — qo'llanilmagan).
- 🔴 **CQRS** — Butun repo bo'ylab (shu jumladan Accounting) MediatR/IRequestHandler/IMediator topilmadi — oddiy Service+Interface (transaction script) uslubi.
- 🟢 **Permissions** — Confirm/Cancel uchun alohida granular ruxsat kodlari mavjud (masalan `PermissionCodeConst.ConfirmWarehouseTransfer`, va shunga o'xshash boshqa modullarda) — oddiy CRUD ruxsatlaridan ajratilgan, yaxshi amaliyot.
- 🟡 **Production Readiness — Accounting Domain darajasida qolgan bo'shliqlar** — Trial Balance (aylanma-saldo qaydnomasi), Hisob kartochkasi (account card/statement ko'rinishi), Balans hisoboti (Balance Sheet) va Foyda-zarar hisoboti kabi standart moliyaviy hisobotlar uchun controller/endpoint topilmadi (faqat xom register harakatlari GET orqali mavjud — bular hisobotga aylantirilmagan).

---

## Summary

=================================
Accounting Core Audit

Chart of Accounts
🟡

Journal Entries
🟡

Posting Engine
🟡

Accounting Domain
🟡

Production Readiness: 62/100
Professional ERP Readiness: 48/100
Accounting Completion: 45/100
=================================

### Qisqa izoh (nima uchun bu ballar)

**Kuchli tomonlar (kutilganidan yaxshi chiqdi):**
- Real DB tranzaksiya (Unit of Work) + PostgreSQL advisory lock orqali document-darajasidagi concurrency himoyasi — professional darajada.
- Accounting Period Close barcha 7 ta hujjat turida (Purchase/Sale/Cash/Bank/WarehouseTransfer/InventoryAdjustment/InventoryCount) izchil enforce qilingan, jumladan bekor qilishda ikkala sana (original + reversal) tekshiriladi.
- Tenant izolyatsiya (fail-closed read + write) — ChartAccount'dan tashqari — to'g'ri ishlaydi, oldingi umumiy audit xulosasi eskirgan.
- Posting Engine (alias/template/resolve-rule) arxitekturasi 1C-darajasidagi tushunchalarni to'g'ri modellagan, reversal orqali tuzatish (o'chirish emas) qat'iy qo'llanilgan.

**Zaif tomonlar (professional ERP darajasiga to'sqinlik qiladi):**
- Chart of Accounts global (tashkilotsiz) bo'lib, oddiy tashkilot-darajasidagi ruxsat bilan tahrirlanadi — cross-tenant butunlik xavfi.
- `AccountingPolicyId` kodda qattiq yozilgan (`STANDARD_UZ`) — sxema ko'p-siyosat (IFRS va h.k.)ni va'da qilsa-da, amalda bitta siyosat bilan ishlaydi.
- Row-level concurrency (RowVersion) yo'q — parallel tahrirlashda "last write wins".
- Domain qatlami EF Core'ga to'liq bog'langan, anemik model, MediatR/CQRS yo'q — Clean Architecture/DDD nuqtai nazaridan repo darajasidagi tanish muammo.
- Trial Balance, Account Card, moliyaviy hisobotlar (P&L, Balance Sheet) hali yo'q — Accounting modul "to'liqligi" past.
- `AccountingRegisterEntry` uchun ishlatilmayotgan, lekin xavfli Create/Update/(yolg'on)Delete metodlari servis qatlamida qoldirilgan.
