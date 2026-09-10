# Oylik maosh moduli: 1C prinsiplariga moslik auditi

Sana: 2026-09-10  
Qamrov: `backend/src/Application/Features/Pay`, `backend/src/Domain/Entities/Pay`, payroll migrations, `frontend/src/modules/payroll` va payrollga bog‘liq HR/accounting qismlari.

## 1. Qisqa xulosa

Joriy modul maosh hisoblashning asosiy skeletini beradi: hisoblash davri, ishlab chiqarish kalendari, xodim tabeli, komponentlar, maosh hujjati, to‘lov vedomosti, provodka va hisobotlar mavjud. Yaqinda kun/soat prorationi, xodimga mos me’yor va kunlik tabel soatini o‘zgartirish imkoniyati qo‘shilgan.

Ammo modulni 1C:ZUP/1C:Accounting’dagi to‘liq maosh sikli deb bo‘lmaydi. Eng muhim moliyaviy xavf — `CORRECTION` hujjatining oddiy maosh bilan avtomatik qo‘shib yuborilishi. Ikkinchi katta xavf — ta’til/kasal kunlari faqat kunlik hisoblagich sifatida qolib, nafaqa yoki to‘lanadigan ta’til summasiga aylanmasligi. Uchinchi xavf — soliq va ushlanmalar qonuniy baza, limit, imtiyoz va davrlar bo‘yicha registr emas, umumiy `PercentOfGross` komponentlari bilan hisoblanishi.

### Holat jadvali

| Yo‘nalish | Holat | Xulosa |
|---|---|---|
| Hisoblash davri va kunlik kalendar | Yashil | Kun turi, ish kuni va kunlik soat snapshot qilinadi; bu qism 1C tamoyiliga yaqin. |
| Tabel va kunlik ishlangan soat | Yashil | Kun turi, reja, actual soat va paid absence/special-hour snapshotlari payroll line’ga ko‘chadi. |
| Xodim normasi va proration | Yashil | Xodim normasi payroll line’da norm/actual snapshot bilan saqlanadi; oy ichidagi employment segmentlari alohida saqlanadi. |
| Maosh komponentlari | Sariq | Formula dependency graph, cycle guard, minimum/maximum cap va taxable flag qo‘shildi; average-base kabi legal formulalar konfiguratsiyaga bog‘liq. |
| Ta’til/kasal/absensiya | Sariq | Paid/unpaid kunlar va special-hour breakdown snapshot/reportda bor; o‘rtacha earnings benefit hisoblash alohida legal policy. |
| Soliq va ushlanmalar | Sariq | Effective-dated tax registry, gross/taxable/net base, exemption/limit, liability snapshot va taxable component flag ishlaydi; employee exemption profile/statutory filing keyingi konfiguratsiya. |
| Tuzatish va qayta hisoblash | Qizil | Frontend dеlta yuboradi; original satr bilan avtomatik farq hisoblanmaydi va vedomostda correction oddiy maoshga qo‘shiladi. |
| Avans va vedomost | Qizil | Avans summasi to‘lov sifatida offset qilinadi, ammo 1Cdagi avans hisoblash va manba-hujjatga taqsimlash yo‘q. |
| Provodka va davr yopilishi | Sariq | Debet/kredit sign va locklar bor, lekin soliq/nafaqa subregistri va pending recalculation nazorati yo‘q. |
| Hisobotlar | Yashil | Register/payslip regular/correction totals, source-aware payments, norm/actual attendance va special-hour breakdownni chiqaradi. |
| Migratsiya boshqaruvi | Yashil | Canonical `_run_order.txt`, checksum history table va PowerShell runner qo‘shildi; legacy correction read-only review mavjud. |

## 2. 1C mezonlari

Bu audit 1C’ning odatiy ZUP/Accounting ish oqimiga tayangan: ishlab chiqarish kalendari va xodim ish grafigi alohida manba bo‘ladi; tabel bo‘yicha haqiqiy vaqt kiritiladi; hisoblash natijasi hujjat snapshotida saqlanadi; qayta hisoblash original hujjatni o‘zgartirmasdan alohida farq hujjati orqali bajariladi; to‘lov qaydnomasi qaysi hisoblash hujjatlarini qoplayotgani bilan bog‘lanadi.

1C hujjatlarida qayta hisoblash keyingi oylik hisoblashga qo‘shilishi yoki alohida “Доначисление, перерасчет” hujjati sifatida chiqarilishi, shuningdek “maosh bilan”, “avans bilan” yoki alohida to‘lanishi mumkin. Shu sababli correctionni shunchaki davr bo‘yicha `SUM(payable)` qilish to‘g‘ri emas. [1C ITS: qayta hisoblash va correction to‘lovi](https://its.1c.ru/db/content/answers1c/src/%D0%B7%D1%83%D0%BF30/%D1%80%D0%B0%D1%81%D1%87%D0%B5%D1%82%D1%8B%20%D1%81%20%D0%BF%D0%B5%D1%80%D1%81%D0%BE%D0%BD%D0%B0%D0%BB%D0%BE%D0%BC/%D0%B7%D1%83%D0%BF30_220914_%D1%87%D0%B2%D0%B7%D0%B2%D0%BE%D0%B7%D0%B2%D1%80%D0%B0%D1%82%D0%B0%D0%BB%D0%B8%D0%BC%D0%B5%D0%BD%D1%82%D0%BE%D0%B2.htm)

1C’da ish grafigi va ishlab chiqarish kalendari normal, qisqartirilgan, dam olish va bayram kunlarini ajratadi. [1C ITS: vaqt hisoblash va ish grafigi](https://its.1c.ru/db/hrmdoc/content/38/hdoc)

Avans qaydnomasi birinchi yarim oy hisoblash hujjatlari yoki to‘lanmagan avanslar asosida shakllanadi; uni oddiy final maoshdan faqat summani ayirish bilan almashtirib bo‘lmaydi. [1C ITS: avans qaydnomasi](https://its.1c.ru/db/content/answers1c/src/%D0%B7%D1%83%D0%BF30/%D1%80%D0%B0%D1%81%D1%87%D0%B5%D1%82%D1%8B%20%D1%81%20%D0%BF%D0%B5%D1%80%D1%81%D0%BE%D0%BD%D0%B0%D0%BB%D0%BE%D0%BC/%D0%B7%D1%83%D0%BF30_230126_%D1%87%D0%B0%D0%B2%D0%B0%D0%BD%D1%81%D0%B1%D0%B0%D0%BD%D0%BA.htm)

## 3. Amaldagi oqim va dalillar

| Qadam | Joriy realizatsiya | Dalil |
|---|---|---|
| Davr | `PayPeriod` yil/oy, norm kun/soat va `PayPeriodWorkDay` kun turi/soatini saqlaydi. | `Periods/Services/PayrollPeriodService.cs`, migrations `1618`, `1622` |
| Tabel | `WORKED`, `PLANNED_WORK`, `DAY_OFF`, `NOT_EMPLOYED`, Leave/Sick/Absent statuslari; kunlik `WorkedHours` 1 dan davr kunlik maksimumigacha tahrirlanadi. | `Timesheets/Services/PayrollTimesheetService.cs`, `PayTimesheetLineDay.cs` |
| Norm | HR calendar va period exceptions asosida xodim normasi snapshot qilinadi. | `PayrollTimesheetNormCalculator.cs`, `PayrollPeriodCalendarOverlay.cs` |
| Regular payroll | Tabeldagi barcha xodimlar, mandatory komponentlar va assignmentlar olinadi; salary prorated, fixed, percent-of-gross, per-hour bor. | `PayrollDocumentService.CalculateAsync`, `CalculateComponent` |
| Correction | Source posted document va manual adjustment majburiy; adjustment summa formulani to‘liq almashtiradi, avtomatik delta hisoblanmaydi. | `PayrollDocumentService.cs` correction branch |
| Vedomost | Final payment outstanding davr bo‘yicha barcha posted payroll lines summasidan barcha final to‘lovlarni ayiradi. | `PayrollPaymentService.GetOutstandingByEmployeeAsync` |
| Hisobot | Register/payslip davr bo‘yicha barcha active/posted payroll linesni, shu jumladan correctionni, birga guruhlaydi. | `PayrollReportService.GetPayrollLinesAsync` |
| Provodka | Component turi bo‘yicha debet/kredit tayinlanadi; manfiy correctionda yo‘nalish teskari qilinadi. | `PayrollDocumentContextBuilder.cs` |

## 4. Nomuvofiqliklar va xavf darajasi

### P0 — darhol tuzatilishi kerak

**P0-1. Correction oddiy maosh bilan aralashmoqda.**

`GetPayrollLinesAsync` faqat period/state/status bilan filter qiladi; `DocumentKind`ni ajratmaydi. `GetOutstandingByEmployeeAsync` ham regular va correction `PayableAmount`ni birga qo‘shadi. Natijada correction “maosh bilan to‘lash” deb belgilanmagan bo‘lsa ham final vedomostda qarzdorlikka kiradi. Payslip va register ham correctionni gross/net/payable ichiga qo‘shadi. Bu double-pay va noto‘g‘ri hisobot xavfi.

**1Cga mos yechim:** correction source hujjat va source line bo‘yicha delta bo‘ladi; `WITH_SALARY`, `WITH_ADVANCE`, `SEPARATE` payout mode saqlanadi; vedomost faqat tanlangan payout mode bo‘yicha manbalarni qoplaydi; hisobot regular/correction va correction source’ni alohida ko‘rsatadi. Original posted document o‘zgarmaydi.

**P0-2. Correction avtomatik farq emas.**

Hozir frontend tayyor adjustment summasini yuboradi. Oldingi hisoblash, yangi basis, soliq va avans farqi serverda qayta hisoblanmaydi. 1C prinsipida correction oldingi natija bilan yangi natija orasidagi farqni hujjatlashtiradi.

### P1 — moliyaviy va qonuniy hisobga bevosita ta’sir qiladi

**P1-1. Ta’til/kasal hisoblash yo‘q.** `LeaveDays` va `SickDays` faqat counter. O‘rtacha ish haqi davri, ish staji, to‘lanadigan/to‘lanmaydigan kun, sick benefit foizi va komponentga aylantirish yo‘q. Hozir salary proration ishlangan kun/soatni kamaytiradi, lekin paid leave o‘rnini alohida earning sifatida bermaydi.

**P1-2. Soliq registri yo‘q.** `DEDUCTION` va `EMPLOYER_TAX` asosan `PercentOfGross` orqali hisoblanadi. Soliq bazasi, rezidentlik, imtiyoz, limit, progressiv stavka, effective date, rounding, hisobot registri va alohida liability line saqlanmaydi. `1620` faqat document-level account selectorlarini olib tashlagan; component-level expense/liability accountlar hali kerak.

**P1-3. Effective-dated ish haqi va stavka o‘zgarishi segmentlanmagan.** Employment tanlashda eng yangi `StartDate` olinadi. Oy o‘rtasidagi ishga kirish/bo‘shash, stavka yoki oylik o‘zgarishi uchun davr ichida ikki segmentga bo‘lib hisoblash va tarixni snapshot qilish mexanizmi yo‘q.

**P1-4. Qayta hisoblash lifecycle’i qisman.** Posted hujjat uchun organization-scoped recalculation queue, source revision hash, processing statusi va immutable source-line/component/tax delta correction mavjud. Oy ichidagi employment segment snapshotlari va alohida worker hali to‘liq ajratilmagan.

### P2 — 1Cga yaqinlashtirish va nazorat

**P2-1. Vaqt tafsiloti yetarli emas.** Har kun status va WorkedHours bor, ammo night hours, holiday/weekend hours, overtime turi/koeffitsienti, transfer-day source va tasdiqlash auditi yo‘q. `OvertimeHours` umumiy satr maydoni sifatida qolgan.

**P2-2. Avans alohida hisoblash hujjati emas.** Joriy tizim posted `ADVANCE` payment summasini regular payrolldan ayiradi. Birinchi yarim oy normasi, avans bazasi, to‘lanmagan avansni tanlash va correction bilan taqsimlash yo‘q.

**P2-3. Komponent formulalari soddalashtirilgan.** Fixed/percent/per-hour/salary-prorated bor, lekin 1Cdagi formula dependency graph, priority, cap/floor, taxable/non-taxable flag, average-base, retroactivity va overlap validation yo‘q.

**P2-4. Hisobot va frontend correction kontekstini ko‘rsatmaydi.** Documents, payments, register va payslip sahifalarida document kind, source document, payout mode, correction delta, tax base va recalculation revision alohida ko‘rinmaydi.

**P2-5. Davr yopilishi qisman checklist.** Period close endi unfinished hujjatlarni, pending/processing/failed recalculation so‘rovlarini va correction draftlarini bloklaydi. Tax liability va bank payment reconciliation hali alohida checklist sifatida qo‘shilmagan.

**P2-6. Migratsiya history yo‘q.** `1618`–`1622` bazada bajarilganini tekshirdik; `1621` va `1622` joriy `AccountingTest` bazasida mavjud. Lekin scriptlar alohida qo‘lda ishlatiladi, schema history/runner barcha muhitlarda bir xil holatni kafolatlamaydi.

## 5. Maqsadli 1C-mos ish oqimi

```text
Xodim/Employment tarixi
        ↓
Ish grafigi + yil/oy ishlab chiqarish kalendari
        ↓
Tabel (har kun status, reja, haqiqiy soat, absence/overtime tafsiloti)
        ↓
Regular accrual snapshot
        ↓
Ta’til/kasal + soliq/ushlanma registrlari
        ↓
Avansni source hujjat bo‘yicha offset qilish
        ↓
Correction: eski natija ↔ yangi natija farqi
        ↓
Vedomost (qaysi hujjat va payout mode bo‘yicha)
        ↓
Provodka + register/payslip reconciliation
```

Har bir bosqich keyingi bosqichga immutable snapshot va source ID beradi. Posted hujjat qayta yozilmaydi; keyingi o‘zgarish yangi revision yoki correction hujjatini yaratadi.

## 6. Bartaraf etish rejasi

Batafsil bajariladigan plan: [2026-09-10-payroll-1c-alignment.md](superpowers/plans/2026-09-10-payroll-1c-alignment.md).

### 1-bosqich — correction va vedomost xavfini yopish (P0)

1. `PayPayrollDoc`ga correction payout mode, source document va source revisionni qo‘shish.
2. Regular/correction line’larni source line/component bo‘yicha delta sifatida hisoblash; manual adjustment faqat override emas, yangi hisoblash parametri bo‘lishi.
3. Payment line’ni payroll document/line bilan majburiy bog‘lash; `GetOutstandingByEmployeeAsync`ni payout mode va source document bo‘yicha ajratish.
4. Reports’da `REGULAR`, `CORRECTION`, correction source, gross/deduction/employer tax/advance/payable alohida ustunlarda ko‘rsatilishi.
5. Test: regular 2 000 000 + separate correction 200 000 bo‘lsa, regular vedomost 2 000 000, correction vedomosti 200 000; `WITH_SALARY` bo‘lsa bitta vedomostda 2 200 000.

### 2-bosqich — ta’til, kasal va maxsus vaqtlar (P1)

1. Absence type konfiguratsiyasiga paid/unpaid, calculation method, benefit rate, average-base period va employer/insurance source qo‘shish.
2. `PayTimesheetLineDay`ga planned hours, night/holiday/weekend/overtime quantity va absence reference snapshotlarini qo‘shish.
3. Paid leave/sick earning komponentlarini salary prorationdan alohida hisoblash.
4. Test: paid leave ishlangan kunni kamaytirmaydi va alohida earning beradi; unpaid leave kamaytiradi; sick benefit limit/rounding bilan chiqadi; overtime koeffitsienti alohida ko‘rinadi.

### 3-bosqich — soliq va ushlanma registri (P1)

1. Organization-scoped, effective-dated tax definition, tax base, exemption/limit va employee tax profile jadvallarini yaratish.
2. Payroll tax line’da employee, base, rate, amount, source component, period, liability account va roundingni snapshot qilish.
3. Employee deduction, employer tax va net payni alohida registr sifatida hisoblash/post qilish.
4. Test: baza, imtiyoz, limit, stavka sanasi, nol baza, rounding va qayta hisoblash holatlari.

### 4-bosqich — effective-dated history va recalculation (P1)

1. Employment, salary, rate va component assignment overlaplarini taqiqlash yoki aniq priority bilan yechish.
2. Oy ichidagi o‘zgarishni date segmentlarga bo‘lib hisoblash.
3. Tabel/component/salary revision hashini payroll documentga yozish; o‘zgarishda recalculation queue yaratish.
4. Posted documentni o‘zgartirmasdan reversal/delta correction chain va audit user/time/source saqlash.

### 5-bosqich — avans va to‘lov lifecycle’i (P2)

1. Avansni first-half accrual yoki tanlangan source payroll documentdan yaratish.
2. Final/correction payment line’larini aniq source document/linega allocation qilish.
3. Partial payment, overpayment va qaytarilgan to‘lovni alohida statuslar bilan yuritish.

### 6-bosqich — provodka, yopilish, hisobot va frontend (P2)

1. Component/tax/absence hisobvarag‘i mappingini majburiy va organization-scoped qilish.
2. Period close’da pending recalculation, unpaid correction, tax liability va payment reconciliationni tekshirish.
3. Documents, payments, register, payslip va tabel UI’larida correction source, payout mode, norm/actual hours, tax base va revisionni ko‘rsatish.
4. Register jami = payroll lines = accounting postings = payments reconciliation testini qo‘shish.

### 7-bosqich — migratsiya va ishlab chiqarish nazorati

1. Migration history/runner joriy qilish; 1618–1622 checksum va applied-at ma’lumotini saqlash.
2. Legacy `CORRECTION` hujjatlarini source/payout mode aniqlanmaguncha “review required” holatiga chiqarish; avtomatik ravishda regularga qo‘shmaslik.
3. Oldingi posted hujjatlar uchun backward-compatible snapshot va reconciliation report yaratish.

## 7. Qabul qilish mezonlari

- Bir xodimning regular, advance va correction hujjatlari vedomostda manba va payout mode bo‘yicha alohida yoki aniq birlashtirilgan holda ko‘rinadi.
- Posted hujjatning summasi keyin o‘zgarmaydi; yangi hisob faqat yangi revision/correction hujjatidir.
- Paid leave, sick, unpaid absence, overtime, holiday/night quantities va summalar payslip’da ajraladi.
- Soliq summasi component nomidan emas, effective-dated tax rule va tax base registridan chiqadi.
- Register, payslip, payment outstanding va accounting posting bir xil source set bo‘yicha reconcile bo‘ladi.
- Davrni yopishdan oldin pending recalculation yoki unresolved correction qolmaydi.
- Backend Unit/Integration testlari, frontend typecheck/test/build muvaffaqiyatli o‘tadi; git commit/push qilinmaydi.

## 8. Bajarilgan birinchi tuzatishlar

- `CORRECTION` uchun `SEPARATE`, `WITH_SALARY`, `WITH_ADVANCE` payout mode qo‘shildi va 1623 SQL migration tayyorlandi.
- Final vedomost outstanding hisobida tanlangan payroll documentdan tashqari boshqa hujjatlar endi aralashtirilmaydi.
- Correction payout mode documents API va frontend hisoblash/detail oynalarida ko‘rsatiladi.
- `HrAbsenceType.IsPaid` asosida paid leave/sick kunlari tabel snapshotiga yoziladi va salary proration bazasiga qaytariladi.
- Qisqartirilgan/rejalashtirilgan kunlarning `PlannedHours` qiymati tabel kun snapshotida saqlanadi; global kunlik soatga qaytib ketmaydi.
- Tabel kunlarida overtime/night/holiday/weekend soatlari snapshot qilinadi, tekshiriladi va qator jami bilan agregatsiya qilinadi; ishlanmagan kunlarga maxsus soat yozish bloklanadi.
- Bir xil komponent kodi uchun kesishuvchi effective-date versiyalar servisda rad etiladi; `effective_to >= effective_from` bazaviy cheklovi va indeks 1629 migratsiyasi bilan qo‘shildi.
- Soliq registrining boshlang‘ich qatlami qo‘shildi: effective-dated withholding/employer qoidalari, gross/taxable/net baza, exemption/limit, liability account snapshot va `/api/payroll/taxes` CRUD API. Oylik hisoblashda faol registry qoidalari payroll line tax snapshotlariga olinadi va postingga chiqariladi.
- Qo‘shimcha regression testlar qo‘shildi; backend UnitTests 43/43 o‘tdi, frontend typecheck/build muvaffaqiyatli.
- Lokal `AccountingTest` bazasida `1623_add_correction_payout_mode.sql` va `1624_add_paid_absence_days.sql` muvaffaqiyatli bajarildi.
- Lokal bazada `1625_add_planned_hours_to_timesheet_day.sql` ham muvaffaqiyatli bajarildi.
- Lokal `AccountingTest` bazasida `1626_add_timesheet_special_hours.sql` muvaffaqiyatli bajarildi.
- Lokal `AccountingTest` bazasida komponent effective-date tekshiruvi uchun `1629_add_component_effective_range_checks.sql` muvaffaqiyatli bajarildi.
- Lokal `AccountingTest` bazasida `1627_add_payroll_tax_registry.sql` muvaffaqiyatli bajarildi.
- Lokal `AccountingTest` bazasida `1628_add_payroll_recalculation.sql` muvaffaqiyatli bajarildi.
- Posted payroll qayta hisoblashida source revision hash, queue statusi va `SEPARATE` DRAFT delta correction yaratish qo‘shildi; period close pending/failed queue va draft correctionni bloklaydi.
- Final payment outstanding tanlangan payroll document/line bilan cheklangan; posting faqat faol leaf hisobvaraqlarga ruxsat beradi.
- Oylik hujjati qatoriga 1C dagi “расчётная база”ga yaqin segment snapshot qo‘shildi: ishga qabul/bo‘shashish yoki stavka o‘zgarishi bo‘yicha sana oralig‘i, oylik maosh, stavka, segmentdagi tabel kun/soat va normativlar saqlanadi; shu segmentdagi komponent assignmentlari JSON snapshotda muzlatiladi. `1630_add_payroll_line_segments.sql` migratsiyasi tayyorlandi.
- Payroll line’ga paid leave/sick, overtime, night, holiday va weekend quantity snapshotlari qo‘shildi (`1631`); register/payslip shu qiymatlarni agregatsiya qiladi.
- `1631_add_payroll_line_attendance_totals.sql` va `1632_add_pay_component_formula_rules.sql` lokal `AccountingTest` bazasida muvaffaqiyatli bajarildi; `1633` history jadvali uchun runner orqali qo‘llanadi.
- Component dependency, cycle/missing-reference guard, cap/floor va taxable flag hisoblashga qo‘shildi (`1632`); component modalida dependency/limit/taxable sozlamalari bor.
- `pay_migration_history`, checksumli `tools/Run-PayrollMigrations.ps1` va legacy correction review SQL/report qo‘shildi.
- Posting transaction wrapper failure/exception rollback test bilan yopildi; source-aware payment endi null yoki boshqa payroll line’larni period bo‘yicha aralashtirmaydi.

## 9. Tavsiya etiladigan navbat

Correction payout/source ajratish, tabel paid-absence/maxsus soat snapshotlari, komponent effective-date/dependency nazorati, soliq registrining bazaviy qatlami, source revision hash, posted payroll uchun immutable delta correction, employment segment snapshotlari va final UI/report reconciliation bajarildi. Qolganlar legal parametrlar bilan to‘ldiriladigan extensionlar: employee-specific tax exemption profile, o‘rtacha earnings benefit, first-half advance document lifecycle va davlat shaklidagi statutory filing.

O‘zgarishlar ishchi daraxtda lokal qoldirildi; commit yoki push qilinmadi.

## 10. Tekshiruv dalili

- Payrollga tegishli UnitTests: **61/61 passed** (`dotnet test tests/UnitTests/UnitTests.csproj --no-restore`).
- Frontend `npm run build` muvaffaqiyatli yakunlandi (Vite chunk hajmi bo‘yicha warning mavjud, build xatosi emas).
- `git diff --check` hujjatlarda xatolik bermadi.
- IntegrationTests: **8 passed, 1 skipped** (PostgreSQL container lock testi muhit sabab skip).
- Frontend tests: **13 passed**; `npm run lint` xatosiz; `npm run build` muvaffaqiyatli.
