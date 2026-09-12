# Ta'til/kasal puli — o'rtacha ish haqi nafaqasi (1C, P1)

Sana: 2026-09-12

## Muammo
Paid ta'til/kasal kunlari oklad proratsiyasiga qo'shilardi (joriy stavkada) — o'rtacha ish haqiga
asoslangan alohida nafaqa (отпускные/больничные) yo'q edi.

## Yechim (opt-in, 1C)
Nafaqa komponent orqali yoqiladi: tashkilot `AVERAGE_LEAVE`/`AVERAGE_SICK` metodli EARNING komponent
qo'shsa, o'sha turdagi paid kunlar oklad proratsiyasidan chiqadi va o'rtacha bo'yicha to'lanadi.
Komponent yo'q bo'lsa — eski xatti-harakat (regressiyasiz).

- **O'rtacha kunlik** = oxirgi 12 oy regular POSTED payroll `Σ gross / Σ worked days`; tarix yo'q bo'lsa
  fallback `oklad×rate / period.NormWorkDays`. `PayrollAverageEarningsCalculator` (pure).
- **Ta'til puli** = o'rtacha × paid ta'til kunlari × 100%.
- **Kasal nafaqasi** = o'rtacha × (paid kasal kunlari, `hr_absence_type.benefit_percent` bo'yicha
  vaznlangan). Migration `1636` `benefit_percent` (default 100) qo'shdi.
- **Proratsiya**: nafaqa komponenti bor turdagi paid kunlar oklad bazasidan CHIQARILADI (double-pay yo'q);
  segment hisoblashda ham (`ComputeSegments` include-flaglari).
- Qayta hisoblash (recalculation) yo'li ham nafaqani hisobga oladi (delta to'g'ri).

## Fayllar
- `PayrollConst.cs` (AVERAGE_LEAVE/AVERAGE_SICK), `1636_add_hr_absence_type_benefit_percent.sql`,
  `HrAbsenceType.cs` (benefit_percent).
- `PayrollAverageEarningsCalculator.cs` (pure), `PayrollDocumentService.cs` (lookback, weighted sick,
  benefit calc, proratsiya exclusion, recalc).

## Testlar
`PayrollAverageEarningsTests` (4). Jami payroll unit 80/80.

## E2E
12 oy tarixli xodim → `AVERAGE_LEAVE`/`AVERAGE_SICK` komponent → ta'til 5 + kasal 3 kun (60% tur) tabel →
payroll: oklad faqat ishlangan kunga, ta'til = o'rtacha×5, kasal = o'rtacha×3×0.6.

## Keyingi
3) Kadr hujjat (prikaz) qatlami. (Birinchi yarim oy avansi — qoldirilgan.)
