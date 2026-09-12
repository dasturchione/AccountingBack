# Avans (aванс) 1C bo'yicha + WITH_ADVANCE netting

Sana: 2026-09-12

## Muammo
Avans qo'lда summа edi (hisoblanmasdi); xodimда avans sozlamasi yo'q edi; `WITH_ADVANCE` payout mode
ishlanmagan edi.

## Yechim (1C)

### 1. Xodim (employment) avans sozlamasi
- Migration `16_pay/1635_add_pay_employment_advance.sql`: `advance_method` (PERCENT|FIXED, default PERCENT),
  `advance_value` (>=0, default 0). Entity + `PayrollAdvanceMethodConst`.
- Kadr oqimi: ishga qabul/transfer/change-pay avans maydonларини o'rнатади/ko'chиради; kadr tarixi/GetById
  DTO'да ko'ринади; validator (PERCENT → 0..100).

### 2. Avans hisoblash (prefill)
- `PayrollAdvanceCalculator.Compute(method, value, oklad, rate)` (pure): PERCENT → `oklad×rate×value/100`,
  FIXED → `value`.
- `GET api/payroll/payments/advance-suggestion?periodId=` → har xodim: `baseAdvance` (employment sozlamasidan)
  + `correctionAmount` (WITH_ADVANCE tuzatишлар outstanding) = `suggested`. Admin qаydномани tuzатади.

### 3. WITH_ADVANCE netting
- **Final** to'lovда faqат `SEPARATE` correction to'g'ридан to'lanади; `WITH_SALARY` → regular bilan,
  `WITH_ADVANCE` → avans orqали (ikkаласи ham finaldа bloklanади).
- **Advance** to'lov: `PayrollDocId` = `WITH_ADVANCE` correction bo'lса per-line settle (allocator);
  aks holда oddiy prepayment (PayrollLineId null).
- "Paid" tracking (`GetPaidByLineAsync`) endi istалган posted to'lovни (Advance ham) qator bo'yicha
  hisoblайди. Final offset (`GetPostedAdvancesAsync`) faqат prepayment (null line) avanslarни oladi —
  WITH_ADVANCE correction settlementlари regular final'ни kamaytirmайди.

## Testlar
`PayrollAdvanceCalculatorTests` (4). Jami payroll unit 76/76.

## E2E
employment avans PERCENT 40 → `advance-suggestion` → avans qаydноma create+confirm → final payroll
avansни offset (payable = net − avans) → `WITH_ADVANCE` correction: avans orqали settle, finaldа rad.

## Keyingi
Birinchi yarim oy tabeliga asosланган avans; ta'til/kasal puli; kadr hujjат (prikaz) qatlами.
