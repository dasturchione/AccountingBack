# Tuzatish (correction) va to'lov: 1C delta + payout mode

Sana: 2026-09-11

## Muammo
- Manual tuzatишда admin FARQ (delta) summани qo'lда kiritарди — xato ehtимоли.
- Payout mode (`WITH_SALARY`/`SEPARATE`/`WITH_ADVANCE`) faqат hisobотда ishlatилар, to'lovда emas.

## (a) 1C delta — server hisoblasин
- `PayrollManualAdjustmentDto`: `Amount` (xom delta) endi nullable; yangi `TargetAmount` (yangi absolut
  qiймат). Aynан bittаси berilади (validator: `Amount ^ TargetAmount`).
- `PayrollCorrectionDeltaResolver.Resolve(target, raw, currentPosted)` — target berilса
  `delta = target − currentPosted`, aks holда raw.
- `CalculateAsync` (correction): manba posted hujjат calc-summаларини yuklab
  (`GetPostedComponentAmountsAsync`), har adjustment uchun deltани hisoblаб saqlaydi. Manfий delta →
  provodkada reversal (mavjud `PayrollDocumentContextBuilder`).
- Yangi: `GET api/payroll/documents/{id}/correction-basis` (`GetCorrectionBasisAsync`) — manba hujjат
  bo'yicha har xodim/komponент joriy summаси; UI "eski qiймат"ни ko'рсатади.

## (b) Payout mode — to'lovда enforce
- `PayrollPaymentService`: final to'lov endi bir necha hujjатни qamраши mumkin.
  - `GetPayableDocIdsAsync`: Regular → o'zi + shu regular'ning `WITH_SALARY` correctionlари; correction
    (`SEPARATE`/`WITH_ADVANCE`) → faqат o'zi.
  - `WITH_SALARY` correction'ни to'g'ридан to'lash bloklanади (`WithSalaryCorrectionNotDirectlyPayable`) —
    u regular final bilan birga to'lanади.
  - Outstanding va "paid" endi **payroll qatори (PayrollLineId)** darajасида hisoblanади (batch.PayrollDocId
    emas) — WITH_SALARY birga to'lovни to'g'ри tracking qiladi.
  - `PayrollPaymentAllocator`: xodim summасини qатор bo'yicha (regular avval, keyin correction)
    taqsimлайди → har biriga alohида `PayPaymentLine`.

## Testlar
- `PayrollCorrectionDeltaResolverTests` (3), `PayrollPaymentAllocatorTests` (3). Jami payroll unit 72/72.

## E2E
regular post → xodim oyligi kam → `POST calculate` (kind=Correction, adjustment `TargetAmount` bilan) →
faqат delta provodka → agar `WITH_SALARY`: regular final to'lovда birga chiqади (outstanding=regular+delta),
`WITH_SALARY` correction'ни alohида to'lash rad etилади; `SEPARATE`: alohида to'lanади.

## Keyingi
WITH_ADVANCE'ни avans qаydномасига haqiqий netting (hozир SEPARATE final sifatида).
