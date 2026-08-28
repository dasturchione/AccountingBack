# Fiskal va asosiy kassa o‘rtasida pul o‘tkazish API

Ushbu hujjat frontend uchun `cash-fiscal-transfers` API bilan ishlash tartibini tushuntiradi.

## 1. API maqsadi

API tashkilotning fiskal kassasi va asosiy kassasi o‘rtasida naqd pul ko‘chirish hujjatini boshqaradi.

Ikki yo‘nalish qo‘llab-quvvatlanadi:

| `directionId` | Kod | Yo‘nalish |
|---:|---|---|
| `-1` | `OUT` | Fiskal kassadan asosiy kassaga. |
| `1` | `IN` | Asosiy kassadan fiskal kassaga. |

Muhim: `directionId` fiskal kassaga nisbatan belgilanadi. Shu sababli `OUT` fiskal kassadan chiqim, `IN` esa fiskal kassaga kirim hisoblanadi.

## 2. Umumiy sarlavhalar

Barcha so‘rovlarda quyidagi sarlavhalar yuboriladi:

```http
Authorization: Bearer {token}
X-OrganizationId: 2
X-Language: uz
```

Sana va vaqt ISO 8601 formatida yuboriladi:

```text
2026-08-28T14:30:00
```

## 3. Frontend ishlash ketma-ketligi

1. Fiskal kassalarni `GET /api/manuals/fiscal-cash-registers` orqali olish.
2. Kassalarni `GET /api/cash-boxes` orqali olish va response ichidan `isMain = true` bo‘lgan asosiy kassani tanlash. `GET /api/manuals/cash-boxes` qisqa select beradi, lekin `isMain` maydonini qaytarmaydi.
3. Valyutani `GET /api/manuals/currencies` orqali olish.
4. Buxgalteriya hisobvaraqlarini `GET /api/manuals/chart-accounts` orqali olish.
5. `POST /api/cash-fiscal-transfers` orqali `DRAFT` hujjat yaratish.
6. Zarur bo‘lsa, hujjatni `PUT /api/cash-fiscal-transfers/{id}` orqali tahrirlash.
7. `PUT /api/cash-fiscal-transfers/{id}/confirm` orqali o‘tkazmani tasdiqlash.
8. Tasdiqlangan o‘tkazmani bekor qilish kerak bo‘lsa, `PUT /api/cash-fiscal-transfers/{id}/cancel` ni chaqirish.

## 4. Statuslar

| `statusId` | Kod | Ma’nosi |
|---:|---|---|
| `1` | `DRAFT` | Qoralama. Pul qoldiqlari hali o‘zgarmagan. |
| `2` | `POSTED` | Tasdiqlangan. Pul bir kassadan yechilib, ikkinchisiga qo‘shilgan. |
| `3` | `CANCELLED` | Bekor qilingan. Tasdiqlangan harakatlar storno qilingan. |

Ruxsat etilgan o‘tishlar:

```text
DRAFT -> POSTED
DRAFT -> CANCELLED
POSTED -> CANCELLED
```

## 5. API ro‘yxati

| Metod | URL | Vazifasi | Muvaffaqiyatli javob |
|---|---|---|---|
| `GET` | `/api/cash-fiscal-transfers` | Hujjatlar ro‘yxati. | `200`, paged response. |
| `GET` | `/api/cash-fiscal-transfers/{id}` | Bitta hujjat tafsilotlari. | `200`, obyekt. |
| `POST` | `/api/cash-fiscal-transfers` | Yangi qoralama yaratish. | `200`, yaratilgan `id`. |
| `PUT` | `/api/cash-fiscal-transfers/{id}` | Qoralamani tahrirlash. | `204`. |
| `DELETE` | `/api/cash-fiscal-transfers/{id}` | Qoralamani mantiqiy o‘chirish. | `204`. |
| `PUT` | `/api/cash-fiscal-transfers/{id}/confirm` | O‘tkazmani tasdiqlash. | `204`. |
| `PUT` | `/api/cash-fiscal-transfers/{id}/cancel` | Hujjatni bekor qilish. | `204`. |

## 6. Ro‘yxatni olish

```http
GET /api/cash-fiscal-transfers?fiscalCashRegisterId=3&cashBoxId=2&directionId=-1&currencyId=1&statusId=2&dateFrom=2026-08-01&dateTo=2026-08-31&search=15&page=1&pageSize=20
```

### Query parametrlari

| Parametr | Tip | Majburiy | Vazifasi |
|---|---|---|---|
| `fiscalCashRegisterId` | `number` | Yo‘q | Fiskal kassa bo‘yicha filtr. |
| `cashBoxId` | `number` | Yo‘q | Asosiy kassa bo‘yicha filtr. |
| `directionId` | `number` | Yo‘q | `-1` yoki `1`. |
| `currencyId` | `number` | Yo‘q | Valyuta bo‘yicha filtr. |
| `statusId` | `number` | Yo‘q | Hujjat statusi. |
| `dateFrom` | `datetime` | Yo‘q | Davr boshlanish sanasi. |
| `dateTo` | `datetime` | Yo‘q | Davr tugash sanasi. |
| `search` | `string` | Yo‘q | Hujjat raqami va matnli maydonlar bo‘yicha qidiruv. |
| `page` | `number` | Yo‘q | Sahifa raqami. Standart qiymat `1`. |
| `pageSize` | `number` | Yo‘q | Bir sahifadagi yozuvlar soni. |

### Response

```json
{
  "items": [
    {
      "id": 25,
      "organizationId": 2,
      "organizationName": "ARTEL",
      "docNumber": "15",
      "docDate": "2026-08-28T14:30:00",
      "fiscalCashRegisterId": 3,
      "fiscalCashRegisterName": "1-sonli fiskal kassa",
      "cashBoxId": 2,
      "cashBoxName": "Asosiy kassa",
      "directionId": -1,
      "directionCode": "OUT",
      "directionName": "Chiqim",
      "currencyId": 1,
      "currencyName": "So‘m",
      "amount": 6000000,
      "exchangeRate": 1,
      "fiscalCashAccountId": 101,
      "fiscalCashAccountNumber": "5011",
      "fiscalCashAccountName": "Fiskal kassadagi pul mablag‘lari",
      "cashBoxAccountId": 102,
      "cashBoxAccountNumber": "5010",
      "cashBoxAccountName": "Asosiy kassadagi pul mablag‘lari",
      "statusId": 2,
      "statusName": "O‘tkazilgan",
      "stateId": 1,
      "stateName": "Faol",
      "comment": "Kunlik savdo tushumini asosiy kassaga olish",
      "createdDate": "2026-08-28T14:25:00",
      "postedAt": "2026-08-28T14:31:00",
      "postedByUserId": 7,
      "cancelledAt": null,
      "cancelledByUserId": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## 7. Bitta hujjatni olish

```http
GET /api/cash-fiscal-transfers/25
```

Request body mavjud emas.

Response ro‘yxatdagi `items` obyektining to‘liq formatida qaytadi.

### Response maydonlari

| Maydon | Tip | Nullable | Vazifasi |
|---|---|---|---|
| `id` | `number` | Yo‘q | O‘tkazma hujjati ID si. |
| `organizationId` | `number` | Yo‘q | Tashkilot ID si. |
| `organizationName` | `string` | Yo‘q | Tashkilot nomi. |
| `docNumber` | `string` | Yo‘q | Backend yaratgan hujjat raqami. |
| `docDate` | `datetime` | Yo‘q | Hujjat sanasi va vaqti. |
| `fiscalCashRegisterId` | `number` | Yo‘q | Fiskal kassa ID si. |
| `fiscalCashRegisterName` | `string` | Yo‘q | Fiskal kassa nomi. |
| `cashBoxId` | `number` | Yo‘q | Asosiy kassa ID si. |
| `cashBoxName` | `string` | Yo‘q | Asosiy kassa nomi. |
| `directionId` | `number` | Yo‘q | Fiskal kassaga nisbatan yo‘nalish: `-1` yoki `1`. |
| `directionCode` | `string` | Yo‘q | `OUT` yoki `IN`. |
| `directionName` | `string` | Yo‘q | Yo‘nalish nomi. |
| `currencyId` | `number` | Yo‘q | Valyuta ID si. |
| `currencyName` | `string` | Yo‘q | Valyuta nomi. |
| `amount` | `decimal` | Yo‘q | O‘tkazma summasi. |
| `exchangeRate` | `decimal` | Yo‘q | Valyuta kursi. |
| `fiscalCashAccountId` | `number` | Ha | Fiskal kassa buxgalteriya hisobvarag‘i. |
| `fiscalCashAccountNumber` | `string` | Ha | Fiskal kassa hisobvarag‘i raqami. |
| `fiscalCashAccountName` | `string` | Ha | Fiskal kassa hisobvarag‘i nomi. |
| `cashBoxAccountId` | `number` | Ha | Asosiy kassa buxgalteriya hisobvarag‘i. |
| `cashBoxAccountNumber` | `string` | Ha | Asosiy kassa hisobvarag‘i raqami. |
| `cashBoxAccountName` | `string` | Ha | Asosiy kassa hisobvarag‘i nomi. |
| `statusId` | `number` | Yo‘q | Hujjat statusi. |
| `statusName` | `string` | Yo‘q | Status nomi. |
| `stateId` | `number` | Yo‘q | Yozuv holati. |
| `stateName` | `string` | Yo‘q | Holat nomi. |
| `comment` | `string` | Ha | Izoh. |
| `createdDate` | `datetime` | Yo‘q | Yaratilgan vaqt. |
| `postedAt` | `datetime` | Ha | Tasdiqlangan vaqt. |
| `postedByUserId` | `number` | Ha | Tasdiqlagan foydalanuvchi. |
| `cancelledAt` | `datetime` | Ha | Bekor qilingan vaqt. |
| `cancelledByUserId` | `number` | Ha | Bekor qilgan foydalanuvchi. |

## 8. Hujjat yaratish

```http
POST /api/cash-fiscal-transfers
Content-Type: application/json
```

### Fiskal kassadan asosiy kassaga

```json
{
  "fiscalCashRegisterId": 3,
  "cashBoxId": 2,
  "directionId": -1,
  "docDate": "2026-08-28T14:30:00",
  "currencyId": 1,
  "amount": 6000000,
  "exchangeRate": 1,
  "fiscalCashAccountId": 101,
  "cashBoxAccountId": 102,
  "comment": "Kunlik savdo tushumini asosiy kassaga olish"
}
```

### Asosiy kassadan fiskal kassaga

```json
{
  "fiscalCashRegisterId": 3,
  "cashBoxId": 2,
  "directionId": 1,
  "docDate": "2026-08-28T09:00:00",
  "currencyId": 1,
  "amount": 500000,
  "exchangeRate": 1,
  "fiscalCashAccountId": 101,
  "cashBoxAccountId": 102,
  "comment": "Fiskal kassaga qaytim puli berish"
}
```

### Request maydonlari

| Maydon | Tip | Nullable | Vazifasi |
|---|---|---|---|
| `fiscalCashRegisterId` | `number` | Yo‘q | Fiskal kassa ID si. |
| `cashBoxId` | `number` | Yo‘q | Asosiy kassa ID si. Tanlangan kassa `isMain = true` bo‘lishi kerak. |
| `directionId` | `number` | Yo‘q | `-1`: fiskaldan asosiyga; `1`: asosiydan fiskalga. |
| `docDate` | `datetime` | Yo‘q | Hujjat sanasi va vaqti. |
| `currencyId` | `number` | Yo‘q | O‘tkazma valyutasi. Asosiy kassa valyutasi bilan mos bo‘lishi kerak. |
| `amount` | `decimal` | Yo‘q | Summa. `0` dan katta bo‘lishi kerak. |
| `exchangeRate` | `decimal` | Yo‘q | Kurs. `0` dan katta; UZS uchun odatda `1`. |
| `fiscalCashAccountId` | `number` | Shartli | Fiskal kassa hisobvarag‘i. Qoralamada bo‘sh bo‘lishi mumkin, tasdiqlash uchun majburiy. |
| `cashBoxAccountId` | `number` | Shartli | Asosiy kassa hisobvarag‘i. Qoralamada bo‘sh bo‘lishi mumkin, tasdiqlash uchun majburiy. |
| `comment` | `string(1000)` | Ha | Izoh. |

`fiscalCashAccountId` va `cashBoxAccountId` bir xil bo‘lishi mumkin emas.

Quyidagi maydonlarni frontend yubormaydi:

```text
docNumber
statusId
stateId
postedAt
postedByUserId
cancelledAt
cancelledByUserId
```

Backend `docNumber` ni tashkilot va yil bo‘yicha avtomatik yaratadi. Yangi hujjat `DRAFT` statusida yaratiladi.

### Yaratish response

```json
25
```

Bu qiymat yaratilgan hujjatning `id` si.

## 9. Hujjatni tahrirlash

```http
PUT /api/cash-fiscal-transfers/25
Content-Type: application/json
```

Request formati POST bilan bir xil:

```json
{
  "fiscalCashRegisterId": 3,
  "cashBoxId": 2,
  "directionId": -1,
  "docDate": "2026-08-28T14:35:00",
  "currencyId": 1,
  "amount": 6500000,
  "exchangeRate": 1,
  "fiscalCashAccountId": 101,
  "cashBoxAccountId": 102,
  "comment": "Summa tuzatildi"
}
```

Faqat `DRAFT` hujjatni tahrirlash mumkin.

Muvaffaqiyatli response:

```http
204 No Content
```

## 10. Qoralamani o‘chirish

```http
DELETE /api/cash-fiscal-transfers/25
```

Request body mavjud emas. Faqat `DRAFT` hujjatni o‘chirish mumkin. O‘chirish jismoniy emas: yozuv passiv holatga o‘tkaziladi.

Muvaffaqiyatli response:

```http
204 No Content
```

## 11. O‘tkazmani tasdiqlash

```http
PUT /api/cash-fiscal-transfers/25/confirm
```

Request body mavjud emas.

Tasdiqlash vaqtida backend:

1. Hujjat `DRAFT` ekanini tekshiradi.
2. Hisob davri ochiq ekanini tekshiradi.
3. Fiskal kassa va asosiy kassa faol va shu tashkilotga tegishli ekanini tekshiradi.
4. `cashBoxId` aynan asosiy kassa ekanini tekshiradi.
5. Valyutalar mosligini tekshiradi.
6. Ikki xil buxgalteriya hisobvarag‘i berilganini tekshiradi.
7. Pul chiqadigan kassaning mavjud qoldig‘ini tekshiradi.
8. Buxgalteriya provodkasini va ikki qarama-qarshi pul harakatini yaratadi.
9. Hujjatni `POSTED` statusiga o‘tkazadi.

Qoldiq yetarli bo‘lmasa, hujjat tasdiqlanmaydi.

Muvaffaqiyatli response:

```http
204 No Content
```

## 12. Hujjatni bekor qilish

```http
PUT /api/cash-fiscal-transfers/25/cancel
```

Request body mavjud emas.

- `DRAFT` bekor qilinsa, pul qoldig‘i o‘zgarmaydi.
- `POSTED` bekor qilinsa, buxgalteriya yozuvlari va pul harakatlari storno qilinadi.
- Takroriy `cancel` idempotent: hujjat allaqachon `CANCELLED` bo‘lsa, API yana `204` qaytaradi.

Muvaffaqiyatli response:

```http
204 No Content
```

## 13. Pul qoldiqlarining o‘zgarishi

### `directionId = -1`

```text
Fiskal kassa: -amount
Asosiy kassa: +amount
```

Buxgalteriya yozuvi:

```text
Debet  = cashBoxAccountId
Kredit = fiscalCashAccountId
```

### `directionId = 1`

```text
Asosiy kassa: -amount
Fiskal kassa: +amount
```

Buxgalteriya yozuvi:

```text
Debet  = fiscalCashAccountId
Kredit = cashBoxAccountId
```

## 14. Frontend tugmalari

| Status | Tahrirlash | O‘chirish | Tasdiqlash | Bekor qilish |
|---|---:|---:|---:|---:|
| `DRAFT` | Ha | Ha | Ha | Ha |
| `POSTED` | Yo‘q | Yo‘q | Yo‘q | Ha |
| `CANCELLED` | Yo‘q | Yo‘q | Yo‘q | Yo‘q |

`POSTED` hujjat uchun `confirm` qayta chaqirilsa, backend mavjud posting batch to‘g‘ri bo‘lsa operatsiyani muvaffaqiyatli deb qabul qilishi mumkin. Frontend baribir tasdiqlash tugmasini yashirishi kerak.

## 15. Asosiy xatolar

Xatolar standart `ProblemDetails` formatida qaytadi.

| `title` | HTTP turi | Ma’nosi |
|---|---|---|
| `CashFiscalTransfer.NotFound` | `404` | Hujjat topilmadi yoki boshqa tashkilotga tegishli. |
| `CashFiscalTransfer.InvalidStatus` | `409` | Amal joriy statusda ruxsat etilmagan. |
| `CashFiscalTransfer.AlreadyCancelled` | `409` | Bekor qilingan hujjatni tasdiqlashga urinish. |
| `CashFiscalTransfer.InvalidConfiguration` | `422` | Kassa, valyuta yoki hisobvaraqlar noto‘g‘ri. |
| `CashFiscalTransfer.InsufficientBalance` | `422` | Pul chiqadigan kassada qoldiq yetarli emas. |
| `CashFiscalTransfer.BusinessEffectsAlreadyExist` | `409` | Hujjat uchun harakatlar oldin yaratilgan. |
| `CashFiscalTransfer.MissingPostingBatch` | `409` | Tasdiqlash paketi topilmadi. |
| `CashFiscalTransfer.MissingAccountingEntries` | `409` | Bekor qilish uchun provodkalar topilmadi. |
| `CashFiscalTransfer.MissingMoneyEntries` | `409` | Bekor qilish uchun pul harakatlari topilmadi. |

Misol:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "CashFiscalTransfer.InsufficientBalance",
  "status": 422,
  "detail": "FISCAL_CASH_REGISTER balance 5000000 is less than required amount 6000000.",
  "traceId": "00-..."
}
```

## 16. Frontend uchun muhim qoidalar

1. `directionId` ni har doim fiskal kassaga nisbatan talqin qilish.
2. `docNumber` ni request ichida yubormaslik.
3. `CASH` retail to‘lovlari fiskal kassa qoldig‘ini oshiradi; fiskal qoldiq retail savdolar va keyingi transfer harakatlaridan hisoblanadi.
4. Fiskal kassadan asosiy kassaga o‘tkazishda `directionId = -1` yuborish.
5. Asosiy kassadan fiskal kassaga o‘tkazishda `directionId = 1` yuborish.
6. Tasdiqlashdan oldin ikkala buxgalteriya hisobvarag‘ini to‘ldirish.
7. Backend qoldiqni qayta tekshiradi; frontendda ko‘rsatilgan qoldiq yakuniy kafolat emas.
8. `confirm`, `cancel`, `update` va `delete` javobi `204` bo‘lsa, detail yoki list ma’lumotlarini qayta yuklash.
