# FRONTEND MIGRATION GUIDE

> Bu hujjat **frontend dasturchilari** uchun. Maqsad: frontend-ni hozirgi backend bilan **to'liq mos** qilish.
>
> Hujjatda faqat **🔴 Missing** (frontendda yo'q) va **🟡 Changed** (o'zgargan) qismlar yoziladi.
> Allaqachon mos bo'lgan (✅ Matched) sahifalar bu yerda **tavsiflanmaydi**.
>
> Har bir yo'qolgan sahifa uchun 8 ta bo'lim beriladi: Biznes maqsad, User workflow, UI layout, API, DTO mapping, Frontend logika, Sidebar joyi, Checklist.
>
> DTO maydonlari backend kodidan **o'qib** yozilgan — taxmin qilinmagan.

**Baza URL eslatma:** Frontenddagi Axios `baseURL` ichida `/api` bor. Shu sabab quyida endpointlar `/register/...` ko'rinishida (mavjud frontend kodidagi kabi) yoziladi. Backenddagi to'liq yo'l `/api/register/...`.

---

## 1. ACCOUNTING (Buxgalteriya) moduli

### Modul holati (status)

| Endpoint | Method | Holat |
|---|---|---|
| `/register/accounting-register-entries/postings` | GET | ✅ Matched |
| `/posting-template-views`, `/posting-template-views/{id}` | GET | ✅ Matched (dashboard modulida bor) |
| `/register/ledger` | GET | 🔴 Missing |
| `/register/trial-balance` | GET | 🔴 Missing |
| `/register/repost` | POST | 🔴 Missing |
| `/accounting-periods/{id}/close` | POST | 🔴 Missing |
| `/accounting-periods/{id}/reopen` | POST | 🔴 Missing |
| `/register/accounting-register-entries/postings/daily` | GET | 🔴 Missing |

> ⚠️ **Backend cheklovi:** Hisobot davrlari (Accounting Period) uchun backendda faqat **close/reopen** bor. Davrlar **ro'yxatini** qaytaradigan endpoint **yo'q**. Shu sabab "davr tanlash" ro'yxati uchun backendga alohida endpoint kerak bo'lishi mumkin — buni backend jamoasi bilan aniqlashtiring. Quyida davr `id` mavjud deb hisoblanadi.

---

### 1.1 🔴 Hisob kartochkasi (Ledger / Bosh daftar bo'yicha hisob harakati)

#### 1. Business Purpose (Biznes maqsad)
Bu sahifa **bitta buxgalteriya hisobi** (masalan, "5010 - Kassa" yoki "6010 - Xaridorlar") bo'yicha barcha harakatni ko'rsatadi: boshlang'ich qoldiq, davr ichidagi debet/kredit provodkalar va yakuniy qoldiq.

Nega kerak: Buxgalter bitta hisob bo'yicha "pul qayerdan kelib, qayerga ketdi" degan savolga javob topadi. Masalan: "5010 Kassa hisobida oy davomida qancha kirim, qancha chiqim bo'lgan va oy oxirida qancha qolgan?" — shu sahifada har bir provodka, hujjat raqami, kontragent va running-balance (yugurib boruvchi qoldiq) ko'rinadi.

#### 2. User Workflow
```
Foydalanuvchi "Hisob kartochkasi" sahifasini ochadi
        ↓
Hisobni tanlaydi (AccountId) — bu MAJBURIY
        ↓
Filtrlarni to'ldiradi (davr yoki sana oralig'i, valyuta, kontragent, ombor)
        ↓
"Ko'rish / Filtrlash" tugmasini bosadi
        ↓
API: GET /register/ledger
        ↓
Natija: yuqorida qoldiqlar (opening/closing), pastda provodkalar jadvali
        ↓
Foydalanuvchi keyingi sahifaga o'tadi (pagination)
```

#### 3. UI Layout
- **Page Layout:** Yuqorida filtr paneli, uning ostida "Summary" (qoldiqlar) kartochkasi, eng pastda provodkalar jadvali + pagination.
- **Filters (yuqori panel):**
  - Hisob tanlash (Select/Autocomplete) — **majburiy**, ChartAccounts ro'yxatidan.
  - Davr (PeriodId) yoki Sana oralig'i (DateFrom–DateTo) — ikkovidan biri.
  - Valyuta (CurrencyId) — Select.
  - Kontragent (CounterpartyId) — Autocomplete (ixtiyoriy).
  - Ombor (WarehouseId) — Select (ixtiyoriy).
- **Summary kartochka:** AccountCode + AccountName, OpeningBalance, TotalDebit, TotalCredit, ClosingBalance.
- **Table (provodkalar):** ustunlar — Sana (PostingDate), Jurnal raqami, Hujjat raqami, Hujjat turi, Izoh, Debet, Kredit, Qoldiq (RunningBalance), Valyuta, Kontragent, Ombor.
- **Buttons:** "Filtrlash", "Tozalash", (ixtiyoriy) "Excel export".
- **Pagination:** jadval ostida (Page, PageSize).
- **Validation:** Hisob tanlanmasa "Filtrlash" tugmasi ishlamasin (AccountId majburiy).

#### 4. API Usage
| Xususiyat | Qiymat |
|---|---|
| Endpoint | `/register/ledger` |
| Method | GET |
| Maqsad | Bitta hisob bo'yicha qoldiq + provodkalar ro'yxatini olish |
| Qachon chaqiriladi | "Filtrlash" bosilganda va sahifa/pagination o'zgarganda |
| Success | 200 — LedgerDto qaytadi, jadval va summary to'ldiriladi |
| Error | 4xx/5xx — Problem object; xatoni toast bilan ko'rsatish, jadvalni bo'shatmaslik |
| Permission | `AccRegEntryView` (ko'rish huquqi) |

#### 5. DTO Mapping — So'rov (LedgerFilter, query params)
| Field | Required | Source | Frontend qayerdan oladi |
|---|---|---|---|
| `AccountId` (int) | ✅ Ha | Select List | ChartAccounts ro'yxatidan tanlanadi |
| `PeriodId` (int?) | Yo'q | Select List | Davrlar ro'yxatidan (backend endpoint aniqlanishi kerak) |
| `DateFrom` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `DateTo` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `CurrencyId` (short?) | Yo'q | Select List | Valyutalar ro'yxati |
| `CounterpartyId` (int?) | Yo'q | Lookup | Kontragent autocomplete |
| `WarehouseId` (int?) | Yo'q | Select List | Omborlar ro'yxati |
| `Page` (int) | Yo'q (default 1) | User Input | Pagination |
| `PageSize` (int?) | Yo'q (default 50) | User Input | Pagination |

#### 5b. DTO Mapping — Javob (LedgerDto)
Summary maydonlari: `AccountId`, `AccountCode`, `AccountName`, `PeriodId`, `DateFrom`, `DateTo`, `CurrencyId`, `OpeningBalance`, `ClosingBalance`, `TotalDebit`, `TotalCredit`.
Pagination maydonlari: `Page`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage`.
`Transactions[]` (LedgerTransactionDto) — har biri:
| Field | Izoh |
|---|---|
| `Id` (long) | Provodka ID |
| `PostingDate` (DateTime) | Provodka sanasi |
| `JournalNumber` (string?) | Jurnal raqami |
| `DocumentNumber` (string?) | Hujjat raqami |
| `DocumentTypeId` (short) + `DocumentType` (string) | Hujjat turi (id + nomi) |
| `Reference` (string?) | Havola |
| `Description` (string?) | Izoh |
| `Debit` (decimal) | Debet summa |
| `Credit` (decimal) | Kredit summa |
| `RunningBalance` (decimal) | Yugurib boruvchi qoldiq |
| `CurrencyId` (short) + `Currency` (string) | Valyuta |
| `OrganizationId` (int) + `Organization` (string) | Tashkilot |
| `CounterpartyId` (int?) + `Counterparty` (string?) | Kontragent |
| `WarehouseId` (int?) + `Warehouse` (string?) | Ombor |

> Barcha javob maydonlari **Generated by Backend** — frontend faqat ko'rsatadi.

#### 6. Frontend Logic
- **API chaqirish:** "Filtrlash" bosilganda yoki `Page`/`PageSize` o'zgarganda GET yuboriladi.
- **Refresh:** Har filtr o'zgarishida `Page`ni 1 ga qaytarish.
- **Validation:** `AccountId` bo'sh bo'lsa so'rov yuborilmasin.
- **Confirm/Cancel/Delete:** Bu **read-only** sahifa — Confirm/Cancel/Delete **yo'q**.
- **Pagination:** `HasNextPage`/`HasPreviousPage` bilan tugmalarni yoqish/o'chirish.
- **Search/Sorting/Filters:** Server tomonida filtrlanadi (query params). Sorting bo'lsa mahalliy (client) qilish mumkin.

#### 7. Sidebar
```
Buxgalteriya (Accounting)
└── Hisob kartochkasi (Ledger)
```

#### 8. Checklist (dasturchi uchun)
- [ ] `modules/accounting/pages/ledger/` sahifasini yaratish
- [ ] Filtr paneli (AccountId majburiy) + summary kartochka + provodkalar jadvali
- [ ] `GET /register/ledger` uchun service + hook
- [ ] LedgerFilter query paramlarini to'g'ri yuborish
- [ ] Pagination (Page/PageSize, Has*Page)
- [ ] `routes.tsx` + Sidebar menyusiga qo'shish
- [ ] `AccRegEntryView` permission bilan menyuni yashirish/ko'rsatish

---

### 1.2 🔴 Aylanma-saldo qaydnomasi (Trial Balance)

#### 1. Business Purpose
Bu sahifa **barcha hisoblar** bo'yicha bitta jadvalda boshlang'ich qoldiq, davr aylanmasi (debet/kredit) va yakuniy qoldiqni ko'rsatadi. Bu buxgalteriyaning eng asosiy nazorat hisoboti — debet va kredit jami **teng** bo'lishi kerak.

Nega kerak: Oy/chorak yakunida buxgalter "hamma hisob to'g'ri yopilganmi, balans tengmi?" ni tekshiradi. Masalan, oy oxirida umumiy debet aylanma = umumiy kredit aylanma bo'lishi shart; teng bo'lmasa xatolik bor.

#### 2. User Workflow
```
Foydalanuvchi "Aylanma-saldo qaydnomasi" sahifasini ochadi
        ↓
Davr yoki sana oralig'ini tanlaydi, valyuta, "nol qoldiqlarni ko'rsatish" checkbox
        ↓
"Hisoblash / Ko'rish" tugmasini bosadi
        ↓
API: GET /register/trial-balance
        ↓
Natija: hisoblar jadvali + pastda jami (totals) qatori
```

#### 3. UI Layout
- **Page Layout:** Yuqorida filtr paneli, ostida jadval, jadval ostida "Jami" (totals) qatori.
- **Filters:** Davr (PeriodId) yoki Sana oralig'i (DateFrom–DateTo); Valyuta (CurrencyId); "Nol qoldiqlarni ko'rsatish" (IncludeZeroBalance) — checkbox.
- **Table (Items):** ustunlar — Hisob kodi (AccountCode), Hisob nomi (AccountName), Boshlang'ich Debet/Kredit, Davr Debet/Kredit, Yakuniy Debet/Kredit.
- **Totals qatori (footer):** OpeningDebitTotal, OpeningCreditTotal, PeriodDebitTotal, PeriodCreditTotal, ClosingDebitTotal, ClosingCreditTotal.
- **Buttons:** "Hisoblash", "Tozalash", (ixtiyoriy) "Excel export".
- **Validation:** Davr yoki sana oralig'idan kamida bittasi bo'lishi tavsiya etiladi.

#### 4. API Usage
| Xususiyat | Qiymat |
|---|---|
| Endpoint | `/register/trial-balance` |
| Method | GET |
| Maqsad | Barcha hisoblar bo'yicha aylanma-saldo jadvalini olish |
| Qachon | "Hisoblash" bosilganda |
| Success | 200 — TrialBalanceDto (Items + totallar) |
| Error | Problem — toast bilan ko'rsatish |
| Permission | `AccRegEntryView` |

#### 5. DTO Mapping — So'rov (TrialBalanceFilter, query params)
| Field | Required | Source | Frontend qayerdan oladi |
|---|---|---|---|
| `PeriodId` (int?) | Yo'q | Select List | Davrlar ro'yxati |
| `DateFrom` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `DateTo` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `CurrencyId` (short?) | Yo'q | Select List | Valyutalar |
| `IncludeZeroBalance` (bool) | Yo'q (default false) | User Input | Checkbox |

#### 5b. DTO Mapping — Javob (TrialBalanceDto)
Totallar: `OpeningDebitTotal`, `OpeningCreditTotal`, `PeriodDebitTotal`, `PeriodCreditTotal`, `ClosingDebitTotal`, `ClosingCreditTotal` (barchasi Generated by Backend).
`Items[]` (TrialBalanceItemDto): `AccountId`, `AccountCode`, `AccountName`, `OpeningDebit`, `OpeningCredit`, `PeriodDebit`, `PeriodCredit`, `ClosingDebit`, `ClosingCredit`.

#### 6. Frontend Logic
- **API:** "Hisoblash" bosilganda GET.
- **Refresh:** Filtr o'zgarsa qayta yuklash.
- **Confirm/Cancel/Delete/Pagination:** **Yo'q** — bu read-only, pagination yo'q (butun ro'yxat qaytadi).
- **Search/Sorting:** Client tomonida jadval bo'yicha qidiruv/saralash mumkin.
- **Filters:** Server tomonida.

#### 7. Sidebar
```
Buxgalteriya (Accounting)
└── Aylanma-saldo qaydnomasi (Trial Balance)
```

#### 8. Checklist
- [ ] `modules/accounting/pages/trial-balance/` sahifasi
- [ ] Filtr paneli + jadval + totals footer
- [ ] `GET /register/trial-balance` service + hook
- [ ] IncludeZeroBalance checkbox
- [ ] routes.tsx + Sidebar
- [ ] `AccRegEntryView` permission

---

### 1.3 🔴 Qayta provodka (Repost)

#### 1. Business Purpose
Bu funksiya buxgalteriya provodkalarini **qayta hisoblab, qayta yozadi** (re-posting). Hujjat summasi/hisob o'zgarsa yoki eski provodkalar noto'g'ri bo'lsa, tanlangan davr/sana/hujjat bo'yicha provodkalar qaytadan generatsiya qilinadi.

Nega kerak: Masalan, oy o'rtasida narx yoki hisob sozlamasi o'zgardi. Buxgalter shu davrdagi hujjatlarni "qayta provodka" qilib, registrni to'g'rilaydi. Yoki bitta hujjatning provodkasi buzilgan bo'lsa — faqat o'sha hujjatni qayta provodka qiladi.

#### 2. User Workflow
```
Foydalanuvchi "Qayta provodka" oynasini ochadi
        ↓
Qamrovni tanlaydi: davr, yoki sana oralig'i, yoki aniq hujjat (turi + id)
        ↓
"Qayta provodka qilish" tugmasini bosadi
        ↓
Tasdiqlash dialogi: "Ushbu qamrov qayta provodka qilinsinmi?"
        ↓
API: POST /register/repost
        ↓
Natija: ProcessedCount (nechta hujjat qayta yozildi) + ro'yxat
```

#### 3. UI Layout
- **Page/Dialog Layout:** Alohida sahifa yoki modal oyna.
- **Form:** Davr (PeriodId) — Select; Sana oralig'i (DateFrom–DateTo); Hujjat turi (DocumentType) — Select; Hujjat ID (DocumentId) — raqam kiritish.
- **Buttons:** "Qayta provodka qilish" (asosiy), "Bekor qilish".
- **Confirm Dialog:** Bu **yozuvchi** amal — tasdiqlash dialogi majburiy.
- **Result panel:** "N ta hujjat qayta provodka qilindi" + jadval (DocumentType, DocumentId, PostingDate).
- **Validation:** Kamida bitta qamrov (davr yoki sana yoki hujjat) berilishi kerak.

#### 4. API Usage
| Xususiyat | Qiymat |
|---|---|
| Endpoint | `/register/repost` |
| Method | POST (body: RepostFilter) |
| Maqsad | Tanlangan qamrov bo'yicha provodkalarni qayta yozish |
| Qachon | Tasdiqlashdan keyin |
| Success | 200 — RepostDto (ProcessedCount + Documents); natija panelida ko'rsatiladi |
| Error | Problem — toast; hech narsa o'zgarmagani haqida xabar |
| Permission | `AccRegEntryUpdate` (o'zgartirish huquqi) |

#### 5. DTO Mapping — So'rov (RepostFilter, JSON body)
| Field | Required | Source | Frontend qayerdan oladi |
|---|---|---|---|
| `PeriodId` (int?) | Yo'q | Select List | Davrlar ro'yxati |
| `DateFrom` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `DateTo` (DateTime?) | Yo'q | User Input | Sana tanlagich |
| `DocumentType` (short?) | Yo'q | Select List | Hujjat turlari ro'yxati |
| `DocumentId` (long?) | Yo'q | User Input / Existing Document | Aniq hujjatdan yoki qo'lda |

#### 5b. DTO Mapping — Javob (RepostDto)
`ProcessedCount` (int) — qayta yozilgan hujjatlar soni.
`Documents[]` (RepostDocumentDto): `DocumentType` (short), `DocumentId` (long), `PostingDate` (DateTime).

#### 6. Frontend Logic
- **API:** Tasdiqlash tugmasidan keyin POST.
- **Confirm:** Majburiy tasdiq dialogi (bu registrni o'zgartiradi).
- **Cancel:** Dialogni yopadi, hech narsa yubormaydi.
- **Refresh:** Muvaffaqiyatdan keyin natija panelini yangilash; agar Ledger/TrialBalance ochiq bo'lsa qayta yuklashni taklif qilish.
- **Delete/Pagination/Sorting/Search:** Yo'q.
- **Validation:** Bo'sh forma yuborilmasin.

#### 7. Sidebar
```
Buxgalteriya (Accounting)
└── Qayta provodka (Repost)
```
> Eslatma: bu ko'pincha faqat admin/bosh buxgalter uchun bo'ladi (`AccRegEntryUpdate`).

#### 8. Checklist
- [ ] `modules/accounting/pages/repost/` sahifa yoki modal
- [ ] Forma (davr/sana/hujjat) + tasdiq dialogi
- [ ] `POST /register/repost` service + hook
- [ ] Natija paneli (ProcessedCount + Documents)
- [ ] routes.tsx + Sidebar
- [ ] `AccRegEntryUpdate` permission

---

### 1.4 🔴 Hisobot davrini yopish / ochish (Accounting Period close / reopen)

#### 1. Business Purpose
Hisobot davrini (masalan, "2026-yil Iyul") **yopish** — o'sha davrga yangi provodka kiritish yoki o'zgartirishni bloklaydi. **Reopen** (qayta ochish) — bloklikni olib tashlaydi.

Nega kerak: Oy yakunlangach buxgalter davrni yopadi, shunda hisobotlar "muzlatiladi" va tasodifan o'zgartirib bo'lmaydi. Agar keyin tuzatish kerak bo'lsa — davr qayta ochiladi, tuzatiladi va yana yopiladi.

#### 2. User Workflow
```
Foydalanuvchi davrlar ro'yxatini ochadi
        ↓
Kerakli davrni tanlaydi
        ↓
"Yopish" (yoki "Qayta ochish") tugmasini bosadi
        ↓
Tasdiqlash dialogi
        ↓
API: POST /accounting-periods/{id}/close  (yoki /reopen)
        ↓
Natija: davr holati "Yopiq/Ochiq" ga o'zgaradi
```

#### 3. UI Layout
- **Page Layout:** Davrlar jadvali (davr nomi, boshi-oxiri, holat) + har qatorda "Yopish"/"Qayta ochish" tugmasi.
- **Buttons:** Holatga qarab bitta tugma — davr ochiq bo'lsa "Yopish", yopiq bo'lsa "Qayta ochish".
- **Confirm Dialog:** Har ikki amal uchun ham majburiy.
- **Validation:** Yo'q (id bo'yicha ishlaydi).

> ⚠️ **Diqqat:** Davrlar ro'yxatini beruvchi endpoint hozircha backendda **yo'q**. Bu sahifa ishlashi uchun backend "GET accounting-periods" endpointini qo'shishi kerak — buni backend jamoasi bilan aniqlashtiring. Frontend faqat close/reopen amallarini bajara oladi.

#### 4. API Usage
| Endpoint | Method | Maqsad | Success | Error | Permission |
|---|---|---|---|---|---|
| `/accounting-periods/{id}/close` | POST | Davrni yopish | 200 OK (body yo'q) | Problem — toast | `AccRegEntryUpdate` |
| `/accounting-periods/{id}/reopen` | POST | Davrni qayta ochish | 200 OK (body yo'q) | Problem — toast | `AccRegEntryUpdate` |

#### 5. DTO Mapping
So'rov: faqat route param `id` (int) — **Existing Document / Select List** (davrlar ro'yxatidan). Body **yo'q**. Javob body **yo'q** (faqat 200 OK).

#### 6. Frontend Logic
- **API:** Tasdiqdan keyin POST.
- **Confirm:** Majburiy dialog.
- **Refresh:** Muvaffaqiyatdan keyin davrlar ro'yxatini qayta yuklash (list endpoint tayyor bo'lganda).
- **Cancel:** Dialogni yopadi.
- **Delete/Pagination/Search/Sorting:** Yo'q.

#### 7. Sidebar
```
Buxgalteriya (Accounting)
└── Hisobot davrlari (Accounting Periods)
```

#### 8. Checklist
- [ ] (Backendga bog'liq) Davrlar ro'yxati endpointini aniqlashtirish
- [ ] `modules/accounting/pages/accounting-periods/` sahifasi
- [ ] Davrlar jadvali + close/reopen tugmalari + tasdiq dialog
- [ ] `POST /accounting-periods/{id}/close` va `/reopen` service
- [ ] routes.tsx + Sidebar
- [ ] `AccRegEntryUpdate` permission

---

### 1.5 🔴 Kunlik provodkalar (Daily postings)

#### 1. Business Purpose
Bu endpoint tanlangan **sana oralig'idagi barcha provodkalarni** (ixtiyoriy: hujjat turi bo'yicha) ro'yxat qilib beradi. Mavjud "Provodka hisobot" sahifasi esa faqat **bitta hujjat** bo'yicha provodkalarni ko'rsatadi.

Nega kerak: Buxgalter "1–31 iyul oralig'ida qanaqa provodkalar bo'lgan?" ni bir joyda ko'rishi kerak — bu kunlik provodka jurnali (posting journal).

#### 2. User Workflow
```
Foydalanuvchi "Provodkalar jurnali" sahifasini ochadi
        ↓
startDate va endDate ni tanlaydi (majburiy), ixtiyoriy: hujjat turi
        ↓
"Ko'rish" tugmasini bosadi
        ↓
API: GET /register/accounting-register-entries/postings/daily
        ↓
Natija: sana oralig'idagi provodkalar jadvali
```

#### 3. UI Layout
- **Filters:** startDate (majburiy), endDate (majburiy), documentTypeId (ixtiyoriy, Select).
- **Table:** Sana (DocDate), Debet hisob (kod+nomi), Kredit hisob (kod+nomi), Summa (Amount), Valyuta, Hujjat turi/ID. Har provodkaning ichida Subkonto jadvali (`Tables[]`) — kengaytiriladigan (expandable) qator.
- **Buttons:** "Ko'rish", "Tozalash".
- **Validation:** startDate va endDate majburiy; endDate ≥ startDate.

#### 4. API Usage
| Xususiyat | Qiymat |
|---|---|
| Endpoint | `/register/accounting-register-entries/postings/daily` |
| Method | GET |
| Query | `startDate` (majburiy), `endDate` (majburiy), `documentTypeId` (ixtiyoriy) |
| Maqsad | Sana oralig'idagi barcha provodkalar ro'yxati |
| Success | 200 — `AccountingPostingDto[]` |
| Error | Problem — toast |
| Permission | `AccRegEntryView` |

#### 5. DTO Mapping — Javob (AccountingPostingDto[])
| Field | Izoh |
|---|---|
| `Id` (long) | Provodka ID |
| `OrganizationId` (int) | Tashkilot |
| `DocumentTypeId` (short) + `DocumentId` (long) | Manba hujjat |
| `DebitAccountId` (int?) + `DebitAccountCode/Name` | Debet hisob |
| `CreditAccountId` (int?) + `CreditAccountCode/Name` | Kredit hisob |
| `CurrencyId` (short) + `CurrencyCode/Name` | Valyuta |
| `Amount` (decimal) | Summa |
| `DocDate` (DateTime) | Hujjat sanasi |
| `PostingBatchId` (long?), `SourceLineId` (long?), `ReversalEntryId` (long?) | Texnik havolalar |
| `CreatedDate` (DateTime) | Yaratilgan sana |
| `DebitQuantity` / `CreditQuantity` (decimal?) | Miqdor (ixtiyoriy) |
| `Tables[]` (AccountingPostingTableDto) | Subkonto: `Side`, `SubkontoTypeCode/Name`, `SortOrder`, `EntityId`, `DisplayValue` |

So'rov paramlari: `startDate` (User Input), `endDate` (User Input), `documentTypeId` (Select List). Javob — Generated by Backend.

> **Eslatma:** Bu javob mavjud "Provodka hisobot" sahifasidagi normalize bilan bir xil DTO — `normalizeAccountingEntriesReport` funksiyasini qayta ishlatish mumkin.

#### 6. Frontend Logic
- **API:** "Ko'rish" bosilganda GET.
- **Validation:** startDate/endDate majburiy.
- **Confirm/Cancel/Delete/Pagination:** Yo'q (read-only). Katta oraliqda pagination kerak bo'lsa backend bilan kelishish.
- **Search/Sorting:** Client tomonida.

#### 7. Sidebar
```
Buxgalteriya (Accounting)
└── Provodkalar jurnali (Daily postings)
```

#### 8. Checklist
- [ ] `modules/accounting/pages/daily-postings/` sahifasi
- [ ] Sana oralig'i + hujjat turi filtri
- [ ] `GET .../postings/daily` service + hook (mavjud normalize'ni qayta ishlatish)
- [ ] Subkonto (`Tables[]`) uchun expandable qator
- [ ] routes.tsx + Sidebar
- [ ] `AccRegEntryView` permission
