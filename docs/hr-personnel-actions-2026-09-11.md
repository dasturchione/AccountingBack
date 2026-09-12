# HR kadr amallari va oylik segment tuzatishi

Sana: 2026-09-11

## Nima o'zgardi

1C:ZUP kadr mantiqiga yaqinlashtirish uchun kadr o'zgarishlari endi **interval-tarixli**
(hujjatsiz — hujjat/prikaz qatlami keyingi bosqichga qoldirilgan).

### 1. `pay_employment` — interval-versiyalangan kadr yozuvi
- Migratsiya: `16_pay/1634_add_pay_employment_action.sql` — `action_type`
  (`HIRE|TRANSFER|PAY_CHANGE|DISMISSAL`), `note`, `created_by_user_id`, `updated_by_user_id`.
  `_run_order.txt` oxiriga qo'shildi. Mavjud yozuvlar backfill qilinadi (eng erta interval = HIRE,
  qolganlari = TRANSFER).
- Konstantalar: `PayrollEmploymentActionConst` (`SharedKernel/Constants/PayrollConst.cs`).

### 2. Kadr amallari (`PayrollEmployeeService` / `api/hr/employees`)
Har amal joriy ochiq intervalni `effectiveDate-1`da yopadi va yangi interval ochadi — **tarix
saqlanadi, ustiga yozilmaydi**:
- `POST {id}/transfer` — boshqa lavozim/bo'limga o'tkazish (oklad/stavka ko'rsatilmasa avtomatik
  ko'chiriladi).
- `POST {id}/change-pay` — oylik o'zgartirish (lavozim/bo'lim ko'chiriladi).
- `POST {id}/dismiss` — joriy intervalni yopadi, xodimni passiv qiladi.
- `GET {id}/history` — kadr tarixi taymlayni.
- Muhofaza: amal sanasi tasdiqlangan (POSTED) oylik hujjati qamragan davrga tushsa bloklanadi;
  amal sanasi joriy interval boshlanishidan keyin bo'lishi shart.

### 3. Naming konsolidatsiyasi
- Xodim, employment, kadr amallari va komponent tayinlash endi faqat `api/hr/employees` ostida.
- Dublikat `PayrollEmployeeController` (`api/payroll/employees`) olib tashlandi.
- **Frontend eslatmasi:** `api/payroll/employees*` chaqiruvlari `api/hr/employees*`ga ko'chirilishi
  kerak.

### 4. Oylik hisoblash tuzatishi (asosiy bug)
Ilgari header gross butun davr uchun faqat **eng oxirgi** employment okladidan hisoblanardi, segmentlar
esa alohida — davr o'rtasida oklad/lavozim o'zgarsa summa xato edi. Endi:
- `ComputeSegments` — segment worked/norm/paid qiymatlarini yagona manba sifatida hisoblaydi
  (header ham, saqlanadigan `PayPayrollLineSegment` ham shundan).
- `PayrollSalaryProrationCalculator.CalculateSegmented` — har segmentni o'z okladi/normasi bilan
  proratsiya qilib yig'adi. Bitta segment holatida natija oldingidek (regressiyasiz).
- Qayta hisoblash (recalculation) yo'li ham segmentlardan foydalanadi.

## Testlar
- Yangi: `PayrollProrationTests.CalculateSegmented_*` (bitta segment = butun davr; davr o'rtasida
  oklad o'zgarishi segmentlar yig'indisi).
- Barcha payroll unit testlari o'tadi (63/63).

## Keyingi bosqich (hozir emas)
- `PayHrOrder` kadr hujjati (prikaz) qatlami: tasdiqlash/bekor qilish, bosma buyruq formasi.
- Paid leave/sick'ni segmentlarga kunlik status bo'yicha aniq taqsimlash (hozir normaga proporsional).
