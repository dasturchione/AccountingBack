# HR / Payroll API — frontend & AI-agent integratsiya qo'llanmasi

Sana: 2026-09-12
Qamrov: **git'ga commit qilinmagan barcha o'zgarishlar** (kadr modeli, oylik sikli, tuzatish/to'lov,
avans, ta'til/kasal nafaqasi, kadr buyrug'i). Bu hujjat frontend va AI agent uchun: qaysi API qayerda,
qanday DTO bilan, nima uchun ishlatilishini bir joyda beradi.

> Barcha endpointlar `Authorization: Bearer <token>` va tashkilot kontekstini talab qiladi.
> Javoblar `Result` pattern: muvaffaqiyatda `200/204`, xatoda `ProblemDetails` (`title`, `detail`, `status`).
> Ro'yxat endpointlari `PagedResponse<T>` qaytaradi: `{ items: [], page, pageSize, totalCount, totalPages }`.

---

## 0. Umumiy sikl (1C mantiqi) — qaysi tartibda chaqiriladi

```
1. Xodim va kadr amallari      → api/hr/employees (+ api/hr/orders prikaz orqali)
2. Hisoblash davri (period)    → api/payroll/periods            (o'zgarmagan)
3. Tabel (timesheet)           → api/payroll/timesheets         (o'zgarmagan API, xatti-harakat yangilandi)
4. Oylik hujjati (calculate)   → api/payroll/documents/calculate → DRAFT
5. DRAFT tekshirish/tahrir     → api/payroll/documents/{id}/draft
6. Tasdiqlash (provodka)       → api/payroll/documents/{id}/confirm → POSTED
7. Avans / yakuniy to'lov      → api/payroll/payments
8. Tuzatish (correction)       → api/payroll/documents/calculate (kind=CORRECTION)
```

Holat kodlari (`statusId`, hamma hujjatlar uchun umumiy):
`1=DRAFT, 2=POSTED, 3=CANCELLED, 4=PENDING, 5=IN_TRANSIT, 6=COMPLETED`.
`stateId`: `1=ACTIVE, 2=PASSIVE`.

---

## 1. Xodimlar va kadr amallari — `api/hr/employees`

Xodim kartochkasi + **interval-tarixli** ishga qabul yozuvlari (`employment`). Har kadr amali joriy
intervalni yopib, yangisini ochadi — tarix saqlanadi.

> **Muhim (frontendga):** eski `api/payroll/employees` controller **o'chirildi**. Barcha xodim/kadr
> chaqiruvlari endi `api/hr/employees` ostida.

| Method | Path | Permission | Body | Nima uchun |
|---|---|---|---|---|
| GET | `/` | HR_EMPLOYEE_VIEW | `PayrollEmployeeListFilter` (query) | Xodimlar ro'yxati (paged). Filter: `search, departmentId, positionId, stateId, page, pageSize` |
| GET | `/{id}` | HR_EMPLOYEE_VIEW | — | Bitta xodim + `employments[]` + `components[]` |
| POST | `/` | HR_EMPLOYEE_CREATE | `PayrollEmployeeCreateDto` | Yangi xodim + birinchi ishga qabul (HIRE) |
| PUT | `/{id}` | HR_EMPLOYEE_UPDATE | `PayrollEmployeeUpdateDto` | Xodim shaxsiy ma'lumotlari (kadr emas) |
| DELETE | `/{id}` | HR_EMPLOYEE_DELETE | — | Xodimni passiv qilish (soft) |
| POST | `/{employeeId}/employments` | HR_EMPLOYEE_UPDATE | `PayrollEmploymentSaveDto` | Qo'shimcha ishga qabul yozuvi qo'shish |
| PUT | `/{employeeId}/employments/{employmentId}` | HR_EMPLOYEE_UPDATE | `PayrollEmploymentSaveDto` | Yozuvni tahrirlash (POSTED oylikда ishlatilgan bo'lsa bloklanadi) |
| GET | `/{employeeId}/history` | HR_EMPLOYEE_VIEW | — | Kadr tarixi (eskidan yangiga), har intervalда `actionType` |
| POST | `/{employeeId}/transfer` | HR_EMPLOYEE_UPDATE | `PayrollEmploymentTransferDto` | **Boshqa lavozim/bo'limga o'tkazish**. Ko'rsatilmagan maydonlar joriydan ko'chiriladi |
| POST | `/{employeeId}/change-pay` | HR_EMPLOYEE_UPDATE | `PayrollEmploymentPayChangeDto` | **Oylik (oklad) o'zgartirish** |
| POST | `/{employeeId}/dismiss` | HR_EMPLOYEE_UPDATE | `PayrollEmploymentDismissDto` | **Ishdan bo'shatish** (interval yopiladi, xodim passiv) |
| POST | `/{employeeId}/components` | PAYROLL_EMPLOYEE_UPDATE | `PayrollEmployeeComponentSaveDto` | Xodimga hisoblash komponenti biriktirish |
| DELETE | `/{employeeId}/components/{assignmentId}` | PAYROLL_EMPLOYEE_UPDATE | — | Komponentni olib tashlash |

> `work-schedules/*` va `calendar` endpointlari ham shu controllerда (ish grafigi/kalendar) — API o'zgarmagan.

### DTO'lar

**PayrollEmploymentSaveDto** (ishga qabul yozuvi — hire/qo'shish/tahrir uchun umumiy):
```
departmentId?: int, positionId?: int, employmentType: string (PRIMARY|PART_TIME|CONTRACT),
startDate: date, endDate?: date, monthlySalary: decimal, employmentRate: decimal (0..2, default 1),
weeklyHours: decimal (default 40), currencyId: short, expenseAccountId?: int,
advanceMethod: string (PERCENT|FIXED, default PERCENT), advanceValue: decimal (PERCENT=foiz 0..100, FIXED=summa),
note?: string
```

**PayrollEmploymentTransferDto** (transfer — barcha maydon opsional, `effectiveDate`dan tashqari):
```
effectiveDate: date (majburiy; joriy interval boshiga teng yoki oldin bo'lolmaydi),
departmentId?, positionId?, employmentType?, monthlySalary?, employmentRate?, weeklyHours?,
currencyId?, expenseAccountId?, advanceMethod?, advanceValue?, note?
```
> Kamida `positionId` yoki `departmentId` berilishi kerak.

**PayrollEmploymentPayChangeDto**: `effectiveDate: date, monthlySalary: decimal, employmentRate?: decimal, note?`
**PayrollEmploymentDismissDto**: `effectiveDate: date, note?`
**PayrollEmployeeComponentSaveDto**: `componentId: int, amount?: decimal, rate?: decimal, effectiveFrom: date, effectiveTo?: date`

**PayrollEmploymentDto** (javob — `employments[]` va `history`da): SaveDto + `id, actionType (HIRE|TRANSFER|PAY_CHANGE|DISMISSAL), departmentName, positionName, currencyName, expenseAccountNumber, stateId`.

---

## 2. Kadr buyrug'i / prikaz — `api/hr/orders`  🆕 (yangi)

Kadr o'zgarishini **tasdiqlanadigan hujjat** sifatida rasmiylashtiradi: DRAFT → confirm → employment
yoziladi + bosma buyruq. To'g'ridan-to'g'ri (1-bo'lim) endpointlar ham qoladi — prikaz qatlami ustiga qo'shimcha.

| Method | Path | Permission | Body | Nima uchun |
|---|---|---|---|---|
| GET | `/` | HR_ORDER_VIEW | `PayrollHrOrderListFilter` (query) | Buyruqlar ro'yxati. Filter: `search, employeeId, orderType, statusId, dateFrom, dateTo, page, pageSize` |
| GET | `/{id}` | HR_ORDER_VIEW | — | Bitta buyruq (to'liq) |
| GET | `/{id}/print` | HR_ORDER_VIEW | — | **Bosma buyruq DTO** (eski→yangi qiymatlar bilan) |
| POST | `/` | HR_ORDER_CREATE | `PayrollHrOrderSaveDto` | Yangi DRAFT buyruq (employment'ga tegmaydi) |
| PUT | `/{id}` | HR_ORDER_UPDATE | `PayrollHrOrderSaveDto` | Faqat DRAFTni tahrirlash |
| POST | `/{id}/confirm` | HR_ORDER_CONFIRM | — | **Tasdiqlash**: DRAFT→POSTED, employment yoziladi/yopiladi |
| POST | `/{id}/cancel` | HR_ORDER_CANCEL | — | **Bekor qilish**: POSTED→CANCELLED, o'zgarish qaytariladi |
| DELETE | `/{id}` | HR_ORDER_DELETE | — | Faqat DRAFTni o'chirish |

### PayrollHrOrderSaveDto
```
orderDate: date, orderType: string (HIRE|TRANSFER|PAY_CHANGE|DISMISSAL), employeeId: long,
effectiveDate: date, basis?: string (asos), note?: string,
// target snapshot (turga qarab): 
departmentId?, positionId?, employmentType?, monthlySalary?, employmentRate?, weeklyHours?,
currencyId?, expenseAccountId?, advanceMethod?, advanceValue?
```
Turlar bo'yicha majburiy maydonlar:
- **HIRE**: positionId/departmentId, monthlySalary, currencyId, employmentType.
- **TRANSFER**: positionId yoki departmentId (qolgani joriydan ko'chiriladi).
- **PAY_CHANGE**: `monthlySalary` majburiy.
- **DISMISSAL**: faqat `effectiveDate`.

### Javob DTO'lar
**PayrollHrOrderDto** = SaveDto + `id, orderNumber (K-000001), statusId, employeeName, departmentName, positionName, currencyName, expenseAccountNumber, employmentId, createdDate, updatedDate, confirmedDate`.

**PayrollHrOrderPrintDto** (bosma): `orderNumber, orderDate, orderType, effectiveDate, statusId, basis, note, organizationName, employeeNumber, employeeName, pinfl,` **eski**: `fromDepartmentName, fromPositionName, fromMonthlySalary`, **yangi**: `toDepartmentName, toPositionName, toMonthlySalary, currencyName, employmentRate, confirmedByName, confirmedDate`.

> **Frontend UX**: buyruq turi tanlanganда faqat tegishli target maydonlarni ko'rsating. `confirm`dan
> keyin buyruq faqat o'qish (read-only) + print. `cancel` — POSTED oylikда ishlatilmagan bo'lsagina ishlaydi.

---

## 3. Oylik hujjati — `api/payroll/documents`

Tabel tasdiqlangach oylik hisoblanadi (DRAFT). Admin schyot va summalarni tahrirlab, tasdiqlaydi (provodka).

| Method | Path | Permission | Body | Nima uchun |
|---|---|---|---|---|
| GET | `/` | PAYROLL_DOCUMENT_VIEW | `PayrollDocumentListFilter` (query) | Ro'yxat. Filter: `search, periodId, statusId, documentKind, page, pageSize` |
| GET | `/{id}` | PAYROLL_DOCUMENT_VIEW | — | To'liq hujjat + `lines[]` (calcLines, taxLines, segments) |
| POST | `/calculate` | PAYROLL_DOCUMENT_CALCULATE | `PayrollCalculateDto` | **Oylik hisoblash** → DRAFT yaratadi (REGULAR yoki CORRECTION) |
| POST | `/{id}/recalculate` | PAYROLL_DOCUMENT_CALCULATE | — | Qayta hisoblash (kadr/tabel o'zgargach) |
| GET | `/{id}/correction-basis` | PAYROLL_DOCUMENT_VIEW | — | **Tuzatish asosi**: manba hujjatдаги joriy summalar (TargetAmount kiritish uchun) |
| PUT | `/{id}/draft` | PAYROLL_DOCUMENT_CALCULATE | `PayrollDraftUpdateDto` | DRAFTda schyot + summalarni tahrirlash, jami qayta hisoblanadi |
| PUT | `/{id}/confirm` | PAYROLL_DOCUMENT_CONFIRM | — | **Tasdiqlash → provodka** (POSTED) |
| PUT | `/{id}/cancel` | PAYROLL_DOCUMENT_CANCEL | — | Bekor qilish |
| DELETE | `/{id}` | PAYROLL_DOCUMENT_DELETE | — | DRAFTni o'chirish |

### PayrollCalculateDto
```
periodId: long, docDate: datetime,
documentKind: string (REGULAR|CORRECTION, default REGULAR),
correctionOfDocId?: long,                 // CORRECTION uchun majburiy — qaysi POSTED hujjatni tuzatadi
correctionPayoutMode?: string (WITH_SALARY|WITH_ADVANCE|SEPARATE),  // CORRECTION uchun
salaryExpenseAccountId?: int, salaryPayableAccountId?: int,  // ixtiyoriy — berilmasa default (payroll_accrual)
note?: string,
adjustments: PayrollManualAdjustmentDto[]  // qo'lda tuzatish/doначисление
```

**PayrollManualAdjustmentDto**: `employeeId, componentId,` **aynan bittasi**: `amount?` (xom delta) **yoki** `targetAmount?` (yangi absolut qiymat — server delta = target − joriy posted), `note?`.

### PayrollDraftUpdateDto (DRAFT tahrir)
```
salaryExpenseAccountId?: int, salaryPayableAccountId?: int,
lines: [{ lineId, calcLines: [{ calcLineId, amount?, debitAccountId?, creditAccountId? }],
          taxLines:  [{ taxLineId, amount?, liabilityAccountId? }] }]
```
> Faqat berilgan maydonlar o'zgaradi; jami (`gross/net/payable`) server tomonda qayta hisoblanadi.

### Javob: PayrollDocumentDto → PayrollLineDto → (CalcLine/TaxLine/Segment)
Muhim maydonlar:
- **PayrollLineDto**: `employeeId, employeeName, employmentId, departmentName, positionName, workedDays, workedHours, paidLeaveDays, paidSickDays, overtimeHours, ..., grossAmount, deductionAmount, employerTaxAmount, advanceAmount, netAmount, payableAmount`.
- **PayrollCalcLineDto**: `componentId, componentCode, componentType (EARNING|DEDUCTION|EMPLOYER_TAX|RECLASSIFICATION), calculationMethod (SALARY_PRORATED|FIXED|PERCENT_OF_GROSS|PER_HOUR|AVERAGE_LEAVE|AVERAGE_SICK), baseAmount, quantity?, rate?, amount, debitAccountId?, creditAccountId?, isManual, note?`.
- **PayrollTaxLineDto**: `taxCode, taxType (WITHHOLDING|EMPLOYER), baseType (GROSS|TAXABLE_EARNINGS|NET), baseAmount, taxableBase, rate, amount, liabilityAccountId`.
- **Segment**: davr o'rtasida oklad/lavozim o'zgarganda hisoblangan bo'laklar (`monthlySalary, employmentRate, workedDays, normWorkDays, componentSnapshotJson`).

> **Ta'til/kasal nafaqasi**: tashkilot `AVERAGE_LEAVE`/`AVERAGE_SICK` metodли komponent qo'shsa, o'sha
> kunlar oklad proratsiyasidan chiqadi va o'rtacha ish haqi bo'yicha alohida qatorda chiqadi (opt-in).

---

## 4. To'lov (avans / yakuniy) — `api/payroll/payments`

| Method | Path | Permission | Body | Nima uchun |
|---|---|---|---|---|
| GET | `/` | PAYROLL_PAYMENT_VIEW | `PayrollPaymentListFilter` (query) | Ro'yxat. Filter: `search, periodId, paymentKind, sourceType, statusId, page, pageSize` |
| GET | `/{id}` | PAYROLL_PAYMENT_VIEW | — | To'liq to'lov + `lines[]` |
| GET | `/advance-suggestion?periodId=` | PAYROLL_PAYMENT_VIEW | — | **Avans taklifi** (prefill): har xodim uchun hisoblangan avans |
| POST | `/` | PAYROLL_PAYMENT_CREATE | `PayrollPaymentCreateDto` | Yangi to'lov qaydnomasi (ADVANCE/FINAL) |
| PUT | `/{id}/confirm` | PAYROLL_PAYMENT_CONFIRM | — | Tasdiqlash (bank/kassa operatsiyasi + provodka) |
| PUT | `/{id}/cancel` | PAYROLL_PAYMENT_CANCEL | — | Bekor qilish |

### PayrollPaymentCreateDto
```
periodId: long, payrollDocId?: long, docDate: datetime,
paymentKind: string (ADVANCE|FINAL), sourceType: string (BANK|CASH),
bankAccountId?: int, cashBoxId?: int, sourceChartAccountId: int, offsetAccountId: int,
currencyId: short, note?, lines: [{ employeeId, amount, note? }]
```

### PayrollAdvanceSuggestionDto (javob)
`periodId, lines: [{ employeeId, employeeName, advanceMethod, advanceValue, baseAdvance, correctionAmount, suggested }]`.
> `suggested` = base avans + WITH_ADVANCE tuzatishlar. Frontend buni to'lov qatorlariga prefill qiladi.

**To'lov mantiqi (frontendga muhim):**
- **FINAL** regular oylik o'zining `WITH_SALARY` tuzatishlari bilan birga to'lanadi; prepayment avans offset qilinadi.
- **WITH_SALARY** tuzatishni alohida to'lash **bloklanadi**.
- **WITH_ADVANCE** tuzatish faqat avans qaydnomasi orqali; FINALда bloklanadi.
- **SEPARATE** — alohida to'lanadi.

---

## 5. Tabel — `api/payroll/timesheets` (API o'zgarmagan, xatti-harakat yangilandi)

API kontrakt o'zgarmagan, lekin: tabelга xodim tanlash endi **employment-interval** asosida — davr
o'rtasida ishga olingan/bo'shatilgan xodim ham to'g'ri tushadi. Frontend o'zgartirishi shart emas.

---

## 6. Konstantalar (enum) ma'lumotnomasi

| Guruh | Qiymatlar |
|---|---|
| Hujjat holati (`statusId`) | 1=DRAFT, 2=POSTED, 3=CANCELLED, 4=PENDING, 5=IN_TRANSIT, 6=COMPLETED |
| `stateId` | 1=ACTIVE, 2=PASSIVE |
| Kadr amali / buyruq turi | HIRE, TRANSFER, PAY_CHANGE, DISMISSAL |
| Ish turi (employmentType) | PRIMARY, PART_TIME, CONTRACT |
| Avans usuli | PERCENT, FIXED |
| Komponent turi | EARNING, DEDUCTION, EMPLOYER_TAX, RECLASSIFICATION |
| Hisoblash metodi | SALARY_PRORATED, FIXED, PERCENT_OF_GROSS, PER_HOUR, AVERAGE_LEAVE, AVERAGE_SICK |
| Hujjat turi (oylik) | REGULAR, CORRECTION |
| Tuzatish to'lov rejimi | WITH_SALARY, WITH_ADVANCE, SEPARATE |
| To'lov turi | ADVANCE, FINAL |
| To'lov manbai | BANK, CASH |

Yangi ruxsat kodlari (rolларга biriktirilishi kerak): `HR_ORDER_VIEW/CREATE/UPDATE/CONFIRM/CANCEL/DELETE`.

---

## 7. Tipik oqimlar (AI agent uchun ketma-ketlik)

**A. Xodimni o'tkazish (prikaz bilan):**
1. `POST api/hr/orders` (orderType=TRANSFER, effectiveDate, positionId) → DRAFT id.
2. `GET api/hr/orders/{id}/print` → ko'rib chiqish.
3. `POST api/hr/orders/{id}/confirm` → employment yoziladi.
4. `GET api/hr/employees/{employeeId}/history` → yangi interval ko'rinadi.

**B. Oylik hisoblash → to'lov:**
1. `POST api/payroll/documents/calculate` (kind=REGULAR, periodId) → DRAFT.
2. `PUT api/payroll/documents/{id}/draft` (schyot/summa tahrir, ixtiyoriy).
3. `PUT api/payroll/documents/{id}/confirm` → POSTED (provodka).
4. `GET api/payroll/payments/advance-suggestion?periodId=` → avans prefill (agar ADVANCE).
5. `POST api/payroll/payments` → `PUT .../{id}/confirm`.

**C. Tuzatish (correction):**
1. `GET api/payroll/documents/{id}/correction-basis` → joriy summalar.
2. `POST api/payroll/documents/calculate` (kind=CORRECTION, correctionOfDocId, correctionPayoutMode, adjustments[].targetAmount) → faqat **delta** chiqadi.
3. `PUT .../confirm`.

---

## 8. Frontend uchun migratsiya eslatmalari (breaking)

1. **`api/payroll/employees/*` → `api/hr/employees/*`** (controller o'chirildi). Barcha chaqiruvlarni ko'chiring.
2. Xodim javobida yangi maydonlar: `employments[].actionType`, `advanceMethod`, `advanceValue`.
3. `calculate` da schyotlar endi **ixtiyoriy** (default'lardan olinadi).
4. Yangi `api/hr/orders/*` va `HR_ORDER_*` ruxsatlari — menyu/rol UIга qo'shing.
5. Yangi hisoblash metodlari `AVERAGE_LEAVE`/`AVERAGE_SICK` — komponent formasida ko'rsating; `hr_absence_type` да `benefitPercent` sozlamasi.
