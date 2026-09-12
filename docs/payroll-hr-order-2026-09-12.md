# Kadr hujjat (prikaz/buyruq) qatlami — 1C

Sana: 2026-09-12

## Maqsad
Kadr o'zgarishlari (ishga qabul / o'tkazish / oylik o'zgartirish / bo'shatish) uchun tasdiqlanadigan
**buyruq (prikaz)** hujjati: DRAFT → tasdiqlash → employment interval yoziladi; bosmaga tayyor buyruq DTO.

## Qarorlar (foydalanuvchi bilan)
- **Ikkalasi ham**: prikaz qatlami qo'shiladi, joriy to'g'ridan-to'g'ri endpointlar (transfer/change-pay/dismiss)
  ham qoladi (tezkor amal). Prikaz tasdig'i bir xil ichki logikani qayta ishlatadi.
- **Bosma = strukturalangan JSON DTO** (raqam, sana, F.I.O, eski→yangi lavozim/oklad, asos, imzolar).

## Dizayn
### 1. Shared engine (regressiyasiz qayta ishlatish)
`PersonnelActionEngine` (yangi, tranzaksiyasiz collaborator): guard + validatsiya + interval mutatsiyasi
(`Hire/Transfer/ChangePay/Dismiss`). Tranzaksiya va audit tashqi servisda qoladi. `PayrollEmployeeService`
(darhol) va `PayrollHrOrderService` (tasdiqda) ikkalasi ham shuni chaqiradi — guard/validatsiya yagona manba.
Nesting muammosi yo'q (engine `BeginAsync/CommitAsync` chaqirmaydi).

### 2. Entity `PayHrOrder` (`pay_hr_order`)
id, organization_id, order_number, order_date, order_type (HIRE/TRANSFER/PAY_CHANGE/DISMISSAL),
employee_id, effective_date, status_id (DRAFT/POSTED/CANCELLED), basis (asos), note,
target snapshot (department_id, position_id, employment_type, monthly_salary, employment_rate, weekly_hours,
currency_id, expense_account_id, advance_method, advance_value), employment_id (tasdiqda hosil bo'lgan
interval), audit + confirmed_by/date. Migration `1637`.

### 3. Service `PayrollHrOrderService`
- CRUD DRAFT (Create/Update/Delete — faqat DRAFT tahrirlanadi/o'chiriladi).
- `ConfirmAsync`: DRAFT→POSTED, engine orqali kadr amalini bajaradi, employment_id ni bog'laydi.
- `CancelAsync`: POSTED→CANCELLED, employment o'zgarishini bekor qiladi (predecessorni qayta ochadi /
  hosil bo'lgan intervalni PASSIVE qiladi; dismissda EndDate=null). POSTED payrollда ishlatilган bo'lsa bloklanади.
- `GetPrintAsync`: bosma DTO (eski→yangi qiymatlar current/predecessor employmentдан).
- Avtoraqam: org bo'yicha ketma-ket `K-000001`.

### 4. Endpoint (`api/hr/orders`)
GET (list/by-id/print), POST (create), PUT (update draft), POST confirm/cancel, DELETE.
Permission: yangi `HR_ORDER_*` (view/create/update/confirm/cancel/delete).

## Fayllar
- `PayrollConst.cs` (PayrollHrOrderTypeConst = reuse action const), `PermissionCodeConst.cs`, `AuditLogConst.cs`.
- `Domain/Entities/Pay/PayHrOrder.cs`; `AppDbContext` DbSet; migration `16_pay/1637_create_pay_hr_order.sql` + `_run_order.txt`.
- `Features/Pay/HrOrders/` (Service, IService, DTOs, engine reuse), Validators, Errors.
- `WebApi/Controllers/Hr/HrOrderController.cs`; DI registration.

## Verifikatsiya
- Engine refactor: mavjud direct endpointlar bir xil ishlaydi (guard/validatsiya o'zgармайди).
- Unit: raqam generatsiyasi/holat o'tishlari (pure qismlar).
- Build (temp) + payroll unit 80/80.

## Keyingi
1) Birinchi yarim oy avansi (qoldirilgan).
