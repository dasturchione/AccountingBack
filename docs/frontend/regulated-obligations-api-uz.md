# Tartibga solinadigan majburiyatlar API

Frontend uchun qo‘llanma.

## 1. Maqsad

Bu modul tashkilotga tegishli soliqlar, ajratmalar va boshqa tartibga solinadigan majburiyatlarni sozlash uchun ishlatiladi.

Tizimda ikki xil ma’lumot mavjud:

- umumiy ma’lumotnoma — majburiyat nomi, kodi, kategoriyasi va tarjimalari;
- tashkilot sozlamasi — davriylik, klassifikator kodi, stavka, buxgalteriya hisobvarag‘i va amal qilish davri.

Umumiy ma’lumotnomalar barcha tashkilotlar uchun bir xil. Tashkilot sozlamalari esa `X-OrganizationId` bo‘yicha alohida saqlanadi.

## 2. Umumiy talablar

So‘rovlarda quyidagi headerlar yuboriladi:

```http
Authorization: Bearer <token>
X-OrganizationId: 2
X-Language: uz
```

`X-OrganizationId` request body ichida takrorlanmaydi. Backend tashkilotni header yoki foydalanuvchi kontekstidan oladi.

Sana formati:

```text
YYYY-MM-DD
```

Misol: `2026-09-03`.

## 3. Asosiy kodlar

### Majburiyat kategoriyalari

| Kod | Ma’nosi |
|---|---|
| `TAX` | Soliq |
| `CONTRIBUTION` | Ajratma yoki badal |

### Davriylik kodlari

| Kod | Ma’nosi |
|---|---|
| `NOT_REQUIRED` | Hisobot topshirilmaydi |
| `TEN_DAY` | O‘n kunlik |
| `MONTHLY` | Oylik |
| `QUARTERLY` | Choraklik |
| `SEMI_ANNUAL` | Yarim yillik |
| `ANNUAL` | Yillik |

ID qiymatlarini frontendda doimiy yozib qo‘ymang. ID’larni manuals API’dan olish kerak.

## 4. Majburiyatlar ma’lumotnomasi

### `GET /api/manuals/regulated-obligations`

Soliq va ajratmalar ro‘yxatini qaytaradi. Nom `X-Language` tilida qaytariladi.

Query parametrlari:

| Parametr | Turi | Majburiy | Tavsifi |
|---|---|---:|---|
| `categoryCode` | `string` | Yo‘q | `TAX` yoki `CONTRIBUTION` bo‘yicha filtr |

Misol:

```http
GET /api/manuals/regulated-obligations?categoryCode=TAX
```

Response — `200 OK`:

```json
[
  {
    "id": 7,
    "name": "Qo‘shilgan qiymat solig‘i",
    "code": "VAT"
  },
  {
    "id": 3,
    "name": "Aksiz solig‘i",
    "code": "EXCISE_TAX"
  }
]
```

Response maydonlari:

| Maydon | Tavsifi |
|---|---|
| `id` | `cmn_regulated_obligation.id` |
| `name` | Tanlangan tildagi nom |
| `code` | Tizimdagi o‘zgarmas semantik kod |

Permission:

```text
MANUAL_GET_REGULATED_OBLIGATIONS
```

## 5. Davriyliklar ma’lumotnomasi

### `GET /api/manuals/regulated-obligation-periodicities`

Majburiyatni qanchalik tez-tez hisoblash yoki topshirish kerakligini tanlash uchun ishlatiladi.

Response — `200 OK`:

```json
[
  {
    "id": 1,
    "name": "Topshirilmaydi",
    "code": "NOT_REQUIRED"
  },
  {
    "id": 3,
    "name": "Oy",
    "code": "MONTHLY"
  },
  {
    "id": 4,
    "name": "Chorak",
    "code": "QUARTERLY"
  }
]
```

Permission:

```text
MANUAL_GET_REGULATED_OBLIGATION_PERIODICITIES
```

## 6. Tashkilot majburiyatlari ro‘yxati

### `GET /api/regulated-obligation-settings`

Pagination ishlatilmaydi. API barcha faol `cmn_regulated_obligation` yozuvlarini qaytaradi.

Tashkilotda sozlama mavjud bo‘lmasa ham majburiyat response ichida bo‘ladi. Bunday qatorda `settingId` va sozlama maydonlari `null` bo‘ladi.

Query parametrlari:

| Parametr | Turi | Majburiy | Tavsifi |
|---|---|---:|---|
| `categoryCode` | `string` | Yo‘q | `TAX` yoki `CONTRIBUTION` |
| `choosedDate` | `date` | Yo‘q | Shu sanada amal qiladigan sozlamani tanlaydi. Berilmasa bugungi sana ishlatiladi |
| `search` | `string` | Yo‘q | Majburiyat kodi, nomi yoki kategoriya nomidan qidiradi |
| `isConfigured` | `bool` | Yo‘q | `true` — sozlangan, `false` — sozlanmagan majburiyatlar |

Misol:

```http
GET /api/regulated-obligation-settings?categoryCode=TAX&choosedDate=2026-09-03&search=vat&isConfigured=true
```

`choosedDate` tekshiruvida davr chegaralari ham hisobga olinadi:

```text
effectiveFrom <= choosedDate <= effectiveTo
```

`effectiveTo = null` bo‘lsa, sozlama muddatsiz amal qiladi.

`isConfigured=true` shu sana uchun sozlama yozuvi mavjudligini bildiradi. Sozlamaning faol yoki nofaol holati `stateId` orqali alohida ko‘rsatiladi.

Response — `200 OK`:

```json
[
  {
    "regulatedObligationId": 7,
    "code": "VAT",
    "name": "Qo‘shilgan qiymat solig‘i",
    "categoryId": 1,
    "categoryCode": "TAX",
    "categoryName": "Soliq",
    "settingId": 25,
    "organizationId": 2,
    "periodicityId": 3,
    "periodicityCode": "MONTHLY",
    "periodicityName": "Oy",
    "classifierCode": "7",
    "rate": 12,
    "chartAccountId": 145,
    "chartAccountNumber": "6410",
    "chartAccountName": "Byudjetga to‘lovlar bo‘yicha qarz",
    "effectiveFrom": "2026-01-01",
    "effectiveTo": null,
    "stateId": 1,
    "stateName": "Faol",
    "createdDate": "2026-09-03T10:20:30",
    "updatedDate": null
  },
  {
    "regulatedObligationId": 3,
    "code": "EXCISE_TAX",
    "name": "Aksiz solig‘i",
    "categoryId": 1,
    "categoryCode": "TAX",
    "categoryName": "Soliq",
    "settingId": null,
    "organizationId": 2,
    "periodicityId": null,
    "periodicityCode": null,
    "periodicityName": null,
    "classifierCode": null,
    "rate": null,
    "chartAccountId": null,
    "chartAccountNumber": null,
    "chartAccountName": null,
    "effectiveFrom": null,
    "effectiveTo": null,
    "stateId": null,
    "stateName": null,
    "createdDate": null,
    "updatedDate": null
  }
]
```

Permission:

```text
REGULATED_OBLIGATION_SETTING_VIEW
```

## 7. Response DTO maydonlari

| Maydon | Turi | Tavsifi |
|---|---|---|
| `regulatedObligationId` | `number` | Umumiy majburiyat ID’si |
| `code` | `string` | Majburiyatning semantik kodi, masalan `VAT` |
| `name` | `string` | Tanlangan tildagi majburiyat nomi |
| `categoryId` | `number` | Kategoriya ID’si |
| `categoryCode` | `string` | `TAX` yoki `CONTRIBUTION` |
| `categoryName` | `string` | Tanlangan tildagi kategoriya nomi |
| `settingId` | `number \| null` | Tashkilot sozlamasi ID’si. `null` — hali sozlanmagan |
| `organizationId` | `number` | Joriy tashkilot ID’si |
| `periodicityId` | `number \| null` | Davriylik ID’si |
| `periodicityCode` | `string \| null` | Davriylik kodi |
| `periodicityName` | `string \| null` | Davriylik nomi |
| `classifierCode` | `string \| null` | Rasmiy klassifikator kodi |
| `rate` | `number \| null` | Foiz stavkasi. Masalan, 12% uchun `12` yuboriladi |
| `chartAccountId` | `number \| null` | Buxgalter tanlagan hisobvaraq ID’si |
| `chartAccountNumber` | `string \| null` | Hisobvaraq raqami |
| `chartAccountName` | `string \| null` | Hisobvaraq nomi |
| `effectiveFrom` | `date \| null` | Amal qilish boshlanish sanasi |
| `effectiveTo` | `date \| null` | Amal qilish tugash sanasi |
| `stateId` | `number \| null` | Sozlama holati |
| `stateName` | `string \| null` | Holat nomi |
| `createdDate` | `datetime \| null` | Yaratilgan vaqt |
| `updatedDate` | `datetime \| null` | Oxirgi o‘zgartirilgan vaqt |

## 8. Sozlamani ID bo‘yicha olish

### `GET /api/regulated-obligation-settings/{settingId}`

Faqat joriy tashkilotga tegishli sozlamani qaytaradi.

Misol:

```http
GET /api/regulated-obligation-settings/25
```

Response — `200 OK`: 7-bo‘limdagi bitta DTO.

Sozlama topilmasa — `404 Not Found`.

Permission:

```text
REGULATED_OBLIGATION_SETTING_VIEW_DETAIL
```

## 9. Majburiyat kodi bo‘yicha olish

### `GET /api/regulated-obligation-settings/by-code/{obligationCode}`

Majburiyatni kod bo‘yicha topadi va tanlangan sanada joriy tashkilot sozlamasini qo‘shib qaytaradi.

Misol:

```http
GET /api/regulated-obligation-settings/by-code/VAT?choosedDate=2026-09-03
```

`choosedDate` berilmasa bugungi sana ishlatiladi. Kod registrga bog‘liq emas: `vat` ham `VAT` sifatida qidiriladi.

Tashkilotda sozlama mavjud bo‘lmasa ham `200 OK` qaytadi, lekin `settingId` va sozlama maydonlari `null` bo‘ladi.

Majburiyat kodi topilmasa — `404 Not Found`.

Response DTO ro‘yxat API’sidagi DTO bilan bir xil.

Permission:

```text
REGULATED_OBLIGATION_SETTING_VIEW_DETAIL
```

## 10. Yangi sozlama yaratish

### `POST /api/regulated-obligation-settings`

Request:

```json
{
  "regulatedObligationId": 7,
  "periodicityId": 3,
  "classifierCode": "7",
  "rate": 12,
  "chartAccountId": 145,
  "effectiveFrom": "2026-01-01",
  "effectiveTo": null,
  "stateId": 1
}
```

Request maydonlari:

| Maydon | Turi | Majburiy | Tavsifi |
|---|---|---:|---|
| `regulatedObligationId` | `number` | Ha | Manuals API’dan olingan majburiyat ID’si |
| `periodicityId` | `number` | Ha | Davriylik ID’si |
| `classifierCode` | `string` | Yo‘q | Maksimal 30 belgi. Qiymat avtomatik ravishda `000000007` ko‘rinishiga o‘zgartirilmaydi |
| `rate` | `number` | Yo‘q | `0` dan `100` gacha. 12% uchun `12` |
| `chartAccountId` | `number` | Ha | Joriy tashkilotning faol buxgalteriya hisobvarag‘i |
| `effectiveFrom` | `date` | Ha | Amal qilish boshlanishi |
| `effectiveTo` | `date` | Yo‘q | Amal qilish tugashi yoki `null` |
| `stateId` | `number` | Ha | Odatda faol holat uchun `1` |

Response — `200 OK`:

```json
25
```

Response obyekt emas, yaratilgan `settingId` soni qaytariladi.

Permission:

```text
REGULATED_OBLIGATION_SETTING_CREATE
```

## 11. Sozlamani yangilash

### `PUT /api/regulated-obligation-settings/{settingId}`

Request POST bilan bir xil. Barcha maydonlarning yangi qiymatlarini yuborish kerak.

Misol:

```http
PUT /api/regulated-obligation-settings/25
Content-Type: application/json
```

```json
{
  "regulatedObligationId": 7,
  "periodicityId": 4,
  "classifierCode": "7",
  "rate": 12,
  "chartAccountId": 145,
  "effectiveFrom": "2026-01-01",
  "effectiveTo": "2026-12-31",
  "stateId": 1
}
```

Response — `204 No Content`.

Permission:

```text
REGULATED_OBLIGATION_SETTING_UPDATE
```

DELETE API mavjud emas.

## 12. Backend validatsiyasi

Backend quyidagilarni tekshiradi:

- majburiyat mavjud va faol bo‘lishi;
- davriylik mavjud va faol bo‘lishi;
- `chartAccountId` joriy tashkilotga tegishli va faol bo‘lishi;
- `rate` qiymati `0..100` oralig‘ida bo‘lishi;
- `effectiveTo` sanasi `effectiveFrom` sanasidan oldin bo‘lmasligi;
- bir tashkilot va bir majburiyat uchun amal qilish davrlari kesishmasligi.

Davrlar chegarasi ham kesishish hisoblanadi. Masalan, oldingi sozlama `2026-12-31` kuni tugasa, yangi sozlamani `2026-12-31` kunidan boshlash mumkin emas. Uni `2027-01-01` dan boshlash kerak.

## 13. Xatolar formati

Result asosidagi xatolar `ProblemDetails` formatida qaytariladi:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "RegulatedObligationSetting.PeriodOverlap",
  "status": 409,
  "detail": "Ushbu majburiyat uchun amal qilish davrlari kesishmasligi kerak."
}
```

Asosiy xato kodlari:

| HTTP | `title` | Sababi |
|---:|---|---|
| 400 | `RegulatedObligationSetting.OrganizationRequired` | Tashkilot konteksti yo‘q |
| 404 | `RegulatedObligationSetting.NotFound` | Sozlama topilmadi |
| 404 | `RegulatedObligationSetting.ObligationNotFound` | Majburiyat topilmadi yoki faol emas |
| 404 | `RegulatedObligationSetting.PeriodicityNotFound` | Davriylik topilmadi yoki faol emas |
| 404 | `RegulatedObligationSetting.ChartAccountNotFound` | Hisobvaraq topilmadi, faol emas yoki boshqa tashkilotga tegishli |
| 400 | `RegulatedObligationSetting.InvalidPeriod` | Sanalar noto‘g‘ri |
| 400 | `RegulatedObligationSetting.InvalidRate` | Stavka `0..100` oralig‘ida emas |
| 409 | `RegulatedObligationSetting.PeriodOverlap` | Amal qilish davrlari kesishadi |

Autentifikatsiya yoki permission bo‘lmasa `401` yoki `403` qaytadi.

## 14. Frontend uchun tavsiya etilgan ishlash tartibi

1. Kategoriya filtrini ko‘rsatish uchun `TAX` va `CONTRIBUTION` kodlaridan foydalaning.
2. Majburiyatlar ro‘yxatini `GET /api/regulated-obligation-settings` orqali oling.
3. `settingId == null` bo‘lsa “Sozlanmagan” holatini ko‘rsating va POST formasini oching.
4. `settingId != null` bo‘lsa PUT formasini oching.
5. Davriylikni manuals API’dan tanlang.
6. Hisobvaraqni `/api/manuals/chart-accounts` orqali tanlang.
7. `rate` maydonida foizni oddiy son ko‘rinishida yuboring: `12`, `15`, `0.1`.
8. Tarixiy yoki kelajakdagi sozlamani ko‘rish uchun `choosedDate` yuboring.

## 15. O‘chirilgan API va maydonlar

Quyidagi API’lar endi mavjud emas:

```text
GET /api/manuals/tax-types
PUT /api/setup/tax-settings
```

`GET /api/setup` response ichidan quyidagi maydonlar olib tashlandi:

```text
taxCompleted
taxSettings
```

Setup jarayonidagi `tax-settings` bosqichi ham olib tashlandi. Eski `tax-settings` bosqichidagi tashkilotlar `accounting-policy` bosqichiga o‘tkaziladi.

Quyidagi eski request DTO endi ishlatilmaydi:

```json
{
  "taxTypeId": 2,
  "isVatPayer": true,
  "vatRegistrationNumber": "...",
  "effectiveFrom": "2026-01-01",
  "effectiveTo": null,
  "stateId": 1
}
```

Uning o‘rniga `POST/PUT /api/regulated-obligation-settings` ishlatiladi.

## 16. O‘chirilgan va yangi database obyektlari

O‘chirilgan jadvallar:

```text
cmn_tax_type
org_tax_settings
```

O‘chirilgan ustun:

```text
org_setup_state.tax_completed
```

Yangi jadvallar:

```text
cmn_regulated_obligation_category
cmn_regulated_obligation_category_translation
cmn_regulated_obligation
cmn_regulated_obligation_translation
cmn_regulated_obligation_periodicity
cmn_regulated_obligation_periodicity_translation
org_regulated_obligation_setting
```

Eski permission kodlari ham olib tashlandi:

```text
MANUAL_GET_TAX_TYPES
SETUP_UPDATE_TAX_SETTINGS
```

Ularning role bog‘lanishlari yangi permissionlarga ko‘chiriladi.

## 17. Muhim moslik eslatmasi

Bu o‘zgarish manuals va tashkilot majburiyatlari sozlamalariga tegishli.

Mavjud soliq hisoblash API’laridagi `taxTypeId` nomi hozircha orqaga moslik uchun saqlangan. Uning qiymati yangi tizimda `cmn_regulated_obligation.id` ga teng bo‘lishi kerak. Yangi ID’ni `/api/manuals/regulated-obligations` orqali oling.

`/api/manuals/vat-rates` va `cmn_vat_rate` olib tashlanmagan.

## 18. SQL fayllar

Yangi sxema va ma’lumotlar:

```text
01_cmn/0175_create_cmn_regulated_obligation_category.sql
01_cmn/0176_create_cmn_regulated_obligation_category_translation.sql
01_cmn/0177_create_cmn_regulated_obligation.sql
01_cmn/0178_create_cmn_regulated_obligation_translation.sql
01_cmn/0179_create_cmn_regulated_obligation_periodicity.sql
01_cmn/0180_create_cmn_regulated_obligation_periodicity_translation.sql
01_cmn/0181_insert_cmn_regulated_obligation_catalogues.sql
02_org/0211_create_org_regulated_obligation_setting.sql
02_org/0212_drop_legacy_tax_settings.sql
00_sys/0022_migrate_regulated_obligation_permissions.sql
```

Umumiy bajarish tartibi `src/Infrastructure/Persistence/Scripts/_run_order.txt` faylida ko‘rsatilgan.
