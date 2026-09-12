# Oylik sikli: bir bosishli hisoblash + DRAFT tahrir (admin panel)

Sana: 2026-09-11

## Oqim (1C usuli)
Davr → **tabel shabloni avtomatik** (`GET api/payroll/timesheets/calendar/table?periodId=`) → admin
to'ldiradi → **tabel confirm** → admin alohida **"Hisoblash"** → **payroll DRAFT** (provodka schyotlari
avtomatik) → admin draftni tekshiradi, **schyot va summalarni o'zgartiradi** → **confirm** → **provodka**.

## O'zgarishlar

### 1. Bir bosishli hisoblash (avtomatik default schyotlar)
- `PayrollCalculateDto.SalaryExpenseAccountId/SalaryPayableAccountId` endi **ixtiyoriy**.
- Berilmasa, tashkilotning `payroll_accrual` hujjat-schyot sozlamalaridan default olinadi
  (`salary_expense`→debet, `salary_payable`→kredit) — `GetDefaultPostingAccountsAsync`
  (`DocumentAccountSetting` so'rovi). Default topilmasa: `Payroll.DefaultPostingAccountMissing`.
- Deduction/EmployerTax schyotlari avvalgidek komponent konfiguratsiyasidan.

### 2. DRAFT payrollni tahrirlash
- `PUT api/payroll/documents/{id}/draft` → `UpdateDraftAsync(id, PayrollDraftUpdateDto)`.
- Ruxsat: faqat `DRAFT`/`PENDING`; davr `OPEN`; faol posting batch bo'lsa bloklanadi.
- Tahrir: header schyotlari; har `calc` qatorда summa (+`IsManual=true`) va debet/kredit schyot;
  har `tax` qatorда summa va majburiyat schyoti.
- `PayrollDocumentTotalsCalculator` (yangi, public) line va document jamilarini saqlangan qatorlardan
  **qayta hisoblaydi** (gross=Σearning; deduction=Σdeduction+Σwithholding tax; employerTax=Σemployer
  component+Σemployer tax; net=gross−deduction; payable=net−advance). Soliqlar qayta derive qilinmaydi —
  admin kiritgani qoladi.
- Schyotlar `ValidatePostingAccountsAsync` bilan tekshiriladi (mavjud, org, faol, guruh emas).
- Confirm/provodka o'zgarmaydi: `PayrollDocumentContextBuilder` tahrirlangan calc-qator schyot/summasidan
  provodka quradi.

## Testlar
- `PayrollDocumentTotalsTests` (3 ta): line jamilari, komponent summasini tahrirlab qayta hisoblash,
  document jamilari. Barcha payroll unit testlari o'tadi (66/66).

## Tekshirish (E2E)
davr → calendar/table → tabel create+confirm → `POST documents/calculate` (schyotsiz) → draftda default
schyotlar → `PUT documents/{id}/draft` bilan qator schyoti+summasini o'zgartir → jami yangilanadi →
`PUT documents/{id}/confirm` → provodka o'zgartirilgan qiymatlarni aks ettiradi.
