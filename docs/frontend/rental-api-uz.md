# Ijara shartnomalari va ijara hisob-kitoblari API

Frontend uchun amaldagi backend bo‘yicha qo‘llanma.

## 1. Modulning maqsadi

Ijara moduli quyidagi vazifalarni bajaradi:

- jismoniy yoki yuridik shaxs bilan ijara shartnomasini saqlash;
- bitta shartnomaga bir yoki bir nechta ijaraga beruvchini biriktirish;
- bitta shartnomada bir yoki bir nechta ijara obyektini yuritish;
- obyektning umumiy va ijaraga olingan maydonini saqlash;
- kommunal xizmat turini va uni kim to‘lashini ko‘rsatish;
- ijara va soliq uchun hisoblash davrlarini avtomatik yaratish;
- hisoblash hujjatini buxgalteriya o‘tkazmalariga aylantirish;
- bepul ijarani alohida yuritish va u bo‘yicha qarz yaratmaslik.

Frontendda modul kamida ikkita asosiy bo‘limdan iborat bo‘lishi kerak:

1. **Ijara shartnomalari** — shartnoma, tomonlar, obyektlar, summalar va hisobvaraqlar.
2. **Ijara hisob-kitoblari** — avtomatik yaratilgan davriy hisoblashlar, ularni tekshirish va o‘tkazish.

## 2. Asosiy biznes oqimi

```text
Shartnoma yaratish
        ↓
DRAFT holatida tekshirish va tahrirlash
        ↓
Shartnomani activate qilish
        ↓
POSTED shartnoma bo‘yicha muddat kelishi
        ↓
Avtomatik yoki qo‘lda ijara hisoblash hujjatini yaratish
        ↓
Hisobvaraqlarni tekshirish/tahrirlash
        ↓
Hisoblash hujjatini post qilish
        ↓
Buxgalteriya o‘tkazmalari
```

Bepul ijara uchun oqim shartnoma `POSTED` holatiga kelganda tugaydi. Undan hisoblash hujjati va qarz yaratilmaydi.

## 3. Umumiy request talablari

```http
Authorization: Bearer <token>
X-OrganizationId: 2
X-Language: uz
Content-Type: application/json
```

`organizationId` create/update request ichida yuborilmaydi. Backend uni foydalanuvchi kontekstidagi `X-OrganizationId` orqali oladi.

Sana formati:

```text
YYYY-MM-DD
```

Masalan: `2026-09-04`.

## 4. Kodlar va statuslar

### Ijaraga beruvchi turi

| Kod | Ma’nosi | Frontenddagi qo‘llanishi |
|---|---|---|
| `INDIVIDUAL` | Jismoniy shaxs | PINFL yoki INN so‘raladi |
| `LEGAL_ENTITY` | Yuridik shaxs | INN majburiy; backend kontragentni topadi yoki yaratadi |

Bu kodlar uchun alohida manuals API yo‘q. Frontend ularni enum sifatida ishlatishi mumkin.

### Hisoblash davri birligi

| Kod | Ma’nosi |
|---|---|
| `DAY` | Bir kun uchun belgilangan summa |
| `MONTH` | Bir kalendar oy uchun belgilangan summa |

`periodValue` ishlatilmaydi. Hisoblash har doim tanlangan kalendar oy bo‘yicha amalga oshiriladi.

### Kommunal xizmatni to‘lovchi

| Kod | Ma’nosi |
|---|---|
| `LESSOR` | Ijaraga beruvchi to‘laydi |
| `LESSEE` | Ijarachi, ya’ni joriy tashkilot to‘laydi |

### Hujjat statuslari

| `statusId` | Kod | Ma’nosi |
|---:|---|---|
| `1` | `DRAFT` | Qoralama; tahrirlash mumkin |
| `2` | `POSTED` | Faollashtirilgan yoki buxgalteriya hisobiga o‘tkazilgan |
| `3` | `CANCELLED` | Bekor qilingan |

Frontend biznes qarorini faqat `statusName` matniga bog‘lamasligi kerak. Tugmalarni `statusId` bo‘yicha boshqarish kerak.

## 5. Kerakli manuals API’lar

### `GET /api/manuals/rental-object-types`

Ijara obyekti turini tanlash uchun ishlatiladi.

Amaldagi kodlar:

| Kod | Ma’nosi |
|---|---|
| `REAL_ESTATE` | Ko‘chmas mulk |
| `VEHICLE` | Transport |
| `EQUIPMENT` | Uskuna |
| `OTHER` | Boshqa |

Response:

```json
[
  {
    "id": 1,
    "name": "Ko‘chmas mulk",
    "code": "REAL_ESTATE"
  }
]
```

### `GET /api/manuals/utility-services`

Obyektga tegishli kommunal xizmatlarni tanlash uchun ishlatiladi.

Amaldagi kodlar:

| Kod | Ma’nosi |
|---|---|
| `NATURAL_GAS` | Tabiiy gaz |
| `HOT_WATER` | Issiq suv |
| `COLD_WATER` | Sovuq suv |
| `ELECTRICITY` | Elektr energiyasi |

Response:

```json
[
  {
    "id": 1,
    "name": "Tabiiy gaz",
    "code": "NATURAL_GAS"
  },
  {
    "id": 4,
    "name": "Elektr energiyasi",
    "code": "ELECTRICITY"
  }
]
```

Nom `X-Language` tilida qaytariladi. Requestda `utilityServiceId` yuboriladi; ID qiymatini frontendda doimiy yozib qo‘ymaslik kerak.

Permission: `RENTAL_CONTRACT_VIEW`.

Qo‘shimcha kerakli manuals API’lar:

- `GET /api/manuals/currencies` — valyuta;
- `GET /api/manuals/chart-accounts` — buxgalteriya hisobvaraqlari.

## 6. Ijara shartnomasi request modeli

`POST /api/rental-contracts` va `PUT /api/rental-contracts/{id}` bir xil asosiy body modelidan foydalanadi.

```json
{
  "isFreeOfCharge": false,
  "contractNumber": "IJ-25/2026",
  "contractDate": "2026-09-04",
  "startDate": "2026-09-04",
  "endDate": "2027-09-03",
  "currencyId": 1,
  "lessorPayableAccountId": 210,
  "taxPayableAccountId": 220,
  "comment": "Bir yillik ijara",
  "lessors": [
    {
      "lessorKindCode": "INDIVIDUAL",
      "fullName": "Ali Valiyev",
      "inn": "301111111",
      "pinfl": "12345678901234",
      "phoneNumber": "+998901234567",
      "registeredAddress": "Navoiy viloyati, Karmana tumani",
      "residentialAddress": "Navoiy shahri"
    }
  ],
  "objects": [
    {
      "id": null,
      "rentalObjectTypeId": 1,
      "objectName": "Ofis binosi",
      "objectIdentifier": "21:09:40:02:01:0571/0003",
      "objectAddress": "Navoiy shahri, Islom Karimov ko‘chasi, 50-a-uy",
      "totalArea": 48.22,
      "rentedArea": 20,
      "startDate": "2026-09-04",
      "endDate": "2027-09-03",
      "periodUnit": "MONTH",
      "periodAmount": 5000000,
      "taxBaseAmount": 6000000,
      "taxRate": 12,
      "expenseAccountId": 200,
      "utilities": [
        {
          "utilityServiceId": 1,
          "payerCode": "LESSOR"
        },
        {
          "utilityServiceId": 4,
          "payerCode": "LESSEE"
        }
      ]
    }
  ]
}
```

### Shartnoma maydonlari

| Maydon | Turi | Majburiy | Maqsadi |
|---|---|---:|---|
| `isFreeOfCharge` | `bool` | Ha | `true` — bepul ijara; `false` — pullik ijara |
| `contractNumber` | `string` | Ha | Tashqi ijara shartnomasi raqami |
| `contractDate` | `date` | Ha | Shartnoma tuzilgan sana |
| `startDate` | `date` | Ha | Shartnoma amal qilish boshlanishi |
| `endDate` | `date` | Ha | Shartnoma amal qilish tugashi |
| `currencyId` | `number` | Ha | Valyuta ID’si |
| `lessorPayableAccountId` | `number \| null` | Shartli | Ijaraga beruvchiga qarzning kredit hisobvarag‘i |
| `taxPayableAccountId` | `number \| null` | Shartli | Soliq majburiyatining kredit hisobvarag‘i |
| `comment` | `string \| null` | Yo‘q | Izoh, maksimal 1000 belgi |
| `lessors` | `array` | Ha | Kamida bitta ijaraga beruvchi |
| `objects` | `array` | Ha | Kamida bitta ijara obyekti |

Hisobvaraqlar create vaqtida `null` bo‘lishi mumkin. Pullik shartnomani activate qilishdan oldin ular to‘liq tanlangan bo‘lishi shart. Bepul shartnomada hisobvaraqlar majburiy emas.

### `lessors[]` maydonlari

| Maydon | Turi | Majburiy | Maqsadi |
|---|---|---:|---|
| `lessorKindCode` | `string` | Ha | `INDIVIDUAL` yoki `LEGAL_ENTITY` |
| `fullName` | `string` | Ha | Jismoniy shaxs F.I.Sh. yoki yuridik shaxs nomi |
| `inn` | `string \| null` | Shartli | Soliq identifikatori, maksimal 20 belgi |
| `pinfl` | `string \| null` | Shartli | Jismoniy shaxs PINFL’i, maksimal 14 belgi |
| `phoneNumber` | `string \| null` | Yo‘q | Telefon raqami |
| `registeredAddress` | `string \| null` | Yo‘q | Ro‘yxatdan o‘tgan manzil |
| `residentialAddress` | `string \| null` | Yo‘q | Yashash manzili |

Qoidalar:

- kamida `inn` yoki `pinfl` berilishi kerak;
- `LEGAL_ENTITY` uchun `inn` majburiy;
- bir request ichida bir xil PINFL yoki bir xil INN ikki marta berilmaydi;
- frontend `lessorId` yoki `counterpartyId` yubormaydi;
- backend avval PINFL, keyin INN bo‘yicha mavjud `rnt_lessor` yozuvini qidiradi;
- topilmasa yangi ijaraga beruvchi yaratiladi;
- yuridik shaxs uchun INN bo‘yicha `counterparty_card` qidiriladi, topilmasa avtomatik yaratiladi;
- `counterpartyId` faqat response ichida ma’lumot sifatida qaytadi.

Bitta shartnomada bir nechta ijaraga beruvchi bo‘lishi mumkin. Hozirgi tizim summani ular orasida avtomatik taqsimlamaydi. `lessors[]` shartnoma tomonlarini ko‘rsatadi, hisoblash esa obyekt summasi bo‘yicha umumiy amalga oshiriladi.

### `objects[]` maydonlari

| Maydon | Turi | Majburiy | Maqsadi |
|---|---|---:|---|
| `id` | `number \| null` | Update’da shartli | Mavjud obyektni saqlash uchun uning ID’si. Create’da `null` yoki yuborilmaydi |
| `rentalObjectTypeId` | `number` | Ha | Obyekt turi manuals ID’si |
| `objectName` | `string` | Ha | Foydalanuvchiga ko‘rinadigan obyekt nomi |
| `objectIdentifier` | `string \| null` | Yo‘q | Kadastr raqami yoki boshqa yagona tashqi raqam |
| `objectAddress` | `string \| null` | Yo‘q | Obyekt manzili |
| `totalArea` | `number \| null` | Yo‘q | Obyektning umumiy maydoni |
| `rentedArea` | `number \| null` | Yo‘q | Ijaraga berilgan maydon |
| `startDate` | `date` | Ha | Shu obyekt bo‘yicha ijara boshlanishi |
| `endDate` | `date \| null` | Yo‘q | Shu obyekt bo‘yicha ijara tugashi; muddatsiz bo‘lsa `null` |
| `periodUnit` | `string` | Ha | `DAY` yoki `MONTH` |
| `periodAmount` | `number` | Ha | `MONTH` uchun bir kalendar oylik, `DAY` uchun bir kunlik ijara summasi |
| `taxBaseAmount` | `number` | Ha | Bir oy yoki bir kun uchun soliq bazasi, `periodUnit`ga bog‘liq |
| `taxRate` | `number` | Ha | Foiz stavkasi; 12% uchun `12` |
| `expenseAccountId` | `number \| null` | Shartli | Shu obyekt xarajatining debet hisobvarag‘i |
| `utilities` | `array` | Yo‘q | Kommunal xizmatlar. Xizmat bo‘lmasa `[]` yuboriladi |

Obyekt sanalari shartnoma sanalari ichida bo‘lishi kerak. `rentedArea` qiymati `totalArea`dan katta bo‘lishi mumkin emas.

Maydon va kommunal xizmat ma’lumotlari hozircha hisoblash summasi yoki provodkaga ta’sir qilmaydi. Ular shartnoma shartlarini saqlash va frontendda ko‘rsatish uchun ishlatiladi.

### `utilities[]` maydonlari

| Maydon | Turi | Majburiy | Maqsadi |
|---|---|---:|---|
| `utilityServiceId` | `number` | Ha | `/api/manuals/utility-services`dan olingan ID |
| `payerCode` | `string` | Ha | `LESSOR` yoki `LESSEE` |

Bitta obyekt ichida bir kommunal xizmat faqat bir marta berilishi mumkin.

## 7. Pullik va bepul ijara

### Pullik ijara

`isFreeOfCharge = false` bo‘lsa:

- har bir obyekt uchun `periodAmount > 0`;
- `taxBaseAmount >= periodAmount`;
- `taxRate` `0..100` oralig‘ida;
- activate vaqtida uch turdagi hisobvaraq to‘liq bo‘lishi kerak;
- davriy hisoblash hujjatlari yaratiladi.

### Bepul ijara

`isFreeOfCharge = true` bo‘lsa, har bir obyekt uchun:

```json
{
  "periodAmount": 0,
  "taxBaseAmount": 0,
  "taxRate": 0,
  "expenseAccountId": null
}
```

Shartnoma darajasidagi `lessorPayableAccountId` va `taxPayableAccountId` ham `null` bo‘lishi mumkin. Bunday shartnoma activate qilinadi, lekin undan ijara hisoblash hujjati yaratilmaydi.

## 8. Summalar va formulalar

Frontend `contractAmount`, `taxAmount`, `payableAmount` va yakuniy `amount`ni shartnoma requestida yubormaydi. `periodAmount`, `taxBaseAmount` va `taxRate` yuboriladi; oylik hisoblash summalari backend tomonidan aniqlanadi.

Amaldagi formula:

```text
monthlyContractAmount = round(periodAmount × activeDayCount / calendarMonthDayCount, 2)

monthlyTaxBaseAmount = round(taxBaseAmount × activeDayCount / calendarMonthDayCount, 2)

taxAmount = round(monthlyTaxBaseAmount × taxRate / 100, 8)

withheldFromContract = round(monthlyContractAmount × taxRate / 100, 8)

payableAmount = round(monthlyContractAmount − withheldFromContract, 8)

amount = round(taxAmount + payableAmount, 8)
```

Misol:

```text
periodAmount    = 5 000 000
taxBaseAmount   = 6 000 000
taxRate         = 12
taxAmount       =   720 000
payableAmount   = 4 400 000
amount          = 5 120 000
```

Maydonlarning ma’nosi:

| Maydon | Ma’nosi |
|---|---|
| `periodAmount` | Shartnomada bir oy yoki bir kun uchun ko‘rsatilgan summa |
| `contractAmount` | GET yoki hisoblash hujjatida backend hisoblagan davr/jami summa |
| `taxBaseAmount` | Bir davr uchun soliq bazasi; hisoblash hujjatida oylik hisoblangan baza |
| `taxAmount` | Byudjetga hisoblangan soliq |
| `payableAmount` | Ijaraga beruvchiga to‘lanadigan summa |
| `amount` | Tashkilot tan oladigan jami xarajat: `taxAmount + payableAmount` |

Soliq bazasi, stavkasi va shartnoma summasi har bir obyekt uchun alohida kiritiladi. Backend ijaraga beruvchi turiga qarab stavkani avtomatik tanlamaydi.

## 9. Shartnomalar ro‘yxati

### `GET /api/rental-contracts`

Query parametrlari:

| Parametr | Turi | Majburiy | Tavsifi |
|---|---|---:|---|
| `statusId` | `number` | Yo‘q | Status bo‘yicha filtr |
| `dateFrom` | `date` | Yo‘q | Shu sanada yoki undan keyin tugaydigan shartnomalar |
| `dateTo` | `date` | Yo‘q | Shu sanada yoki undan oldin boshlangan shartnomalar |
| `search` | `string` | Yo‘q | Shartnoma raqami, F.I.Sh./nom, INN yoki PINFL |
| `page` | `number` | Yo‘q | Standart qiymat `1` |
| `pageSize` | `number` | Yo‘q | Sahifa hajmi |

`dateFrom` va `dateTo` birga yuborilsa, tanlangan davr bilan kesishadigan shartnomalar qaytadi.

Misol:

```http
GET /api/rental-contracts?statusId=2&dateFrom=2026-01-01&dateTo=2026-12-31&search=Ali&page=1&pageSize=20
```

Response:

```json
{
  "items": [
    {
      "id": 15,
      "contractNumber": "IJ-25/2026",
      "contractDate": "2026-09-04T00:00:00",
      "isFreeOfCharge": false,
      "startDate": "2026-09-04T00:00:00",
      "endDate": "2027-09-03T00:00:00",
      "currencyId": 1,
      "currencyCode": "UZS",
      "statusId": 2,
      "statusName": "Posted",
      "objectCount": 1,
      "lessors": [
        {
          "id": 8,
          "lessorKindCode": "INDIVIDUAL",
          "counterpartyId": null,
          "fullName": "Ali Valiyev",
          "inn": "301111111",
          "pinfl": "12345678901234",
          "phoneNumber": "+998901234567",
          "registeredAddress": "Navoiy viloyati, Karmana tumani",
          "residentialAddress": "Navoiy shahri"
        }
      ]
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

Permission: `RENTAL_CONTRACT_VIEW`.

## 10. Shartnomani ID bo‘yicha olish

### `GET /api/rental-contracts/{id}`

To‘liq shartnomani, barcha faol ijaraga beruvchilarni, obyektlarni, maydonlarni, kommunal xizmatlarni va hisobvaraqlarni qaytaradi.

Response’dagi qo‘shimcha server maydonlari:

| Maydon | Maqsadi |
|---|---|
| `organizationId` | Shartnoma tegishli tashkilot |
| `currencyCode` | Valyuta kodi |
| `lessorPayableAccountNumber/Name` | Tanlangan qarz hisobvarag‘i |
| `taxPayableAccountNumber/Name` | Tanlangan soliq hisobvarag‘i |
| `statusId/statusName` | Joriy holat |
| `createdDate` | Yaratilgan vaqt |
| `postedAt` | Faollashtirilgan vaqt |
| `cancelledAt` | Bekor qilingan vaqt |
| `objects[].nextAccrualDate` | Keyingi hisoblash boshlanadigan sana |
| `objects[].periodAmount` | Bir oy yoki bir kun uchun requestda berilgan summa |
| `objects[].contractAmount` | Butun shartnoma muddati uchun backend hisoblagan ijara summasi; muddatsiz shartnomada `null` |
| `objects[].contractTaxBaseAmount` | Butun muddat uchun hisoblangan soliq bazasi; muddatsiz shartnomada `null` |
| `objects[].contractTaxAmount` | Butun muddat uchun hisoblangan soliq; muddatsiz shartnomada `null` |
| `objects[].expenseAccountNumber/Name` | Xarajat hisobvarag‘i |
| `utilities[].utilityServiceCode/Name` | Kommunal xizmat kodi va tarjima qilingan nomi |

Permission: `RENTAL_CONTRACT_VIEW_DETAIL`.

## 11. Shartnoma yaratish

### `POST /api/rental-contracts`

6-bo‘limdagi request yuboriladi.

Response — `200 OK`:

```json
15
```

Bu yaratilgan shartnoma ID’si. Shartnoma avtomatik ravishda `DRAFT` statusida yaratiladi.

`contractNumber` joriy tashkilot va `contractDate` yili ichida takrorlanmasligi kerak.

Permission: `RENTAL_CONTRACT_CREATE`.

## 12. Shartnomani yangilash

### `PUT /api/rental-contracts/{id}`

To‘liq yangi holat yuboriladi. Qisman PATCH emas.

Muhim qoidalar:

- faqat `DRAFT` shartnoma yangilanadi;
- mavjud obyektni saqlash/tahrirlash uchun `objects[].id` qayta yuboriladi;
- requestdan olib tashlangan mavjud obyekt nofaol qilinadi;
- yangi obyekt uchun `id` yuborilmaydi;
- `lessors[]` va `utilities[]` ham to‘liq yangi ro‘yxat sifatida yuboriladi;
- ijaraga beruvchi ma’lumotlari PINFL/INN bo‘yicha topilib yangilanadi.

Response — `204 No Content`.

Permission: `RENTAL_CONTRACT_UPDATE`.

## 13. Shartnomani o‘chirish

### `DELETE /api/rental-contracts/{id}`

Faqat `DRAFT` va hisoblash hujjati mavjud bo‘lmagan shartnomani o‘chiradi. Amalda yozuv `stateId = PASSIVE` orqali nofaol qilinadi.

Response — `204 No Content`.

Permission: `RENTAL_CONTRACT_DELETE`.

## 14. Shartnomani faollashtirish

### `PUT /api/rental-contracts/{id}/activate`

Shartnomani `DRAFT`dan `POSTED` holatiga o‘tkazadi.

Pullik shartnomada backend quyidagilarni tekshiradi:

- shartnoma, ijaraga beruvchi va obyekt ma’lumotlari to‘g‘ri;
- valyuta va barcha ma’lumotnoma yozuvlari faol;
- `lessorPayableAccountId` berilgan;
- `taxPayableAccountId` berilgan;
- har bir obyektning `expenseAccountId` qiymati berilgan;
- hisobvaraqlar joriy tashkilotga tegishli va faol;
- xarajat, ijaraga beruvchi qarzi va soliq hisobvaraqlari bir-biridan farqli.

Hatto `taxRate = 0` bo‘lgan pullik shartnomada ham joriy kod `taxPayableAccountId`ni talab qiladi.

Bepul shartnomada hisobvaraqlar talab qilinmaydi.

Response — `204 No Content`.

Permission: `RENTAL_CONTRACT_ACTIVATE`.

## 15. Shartnomani bekor qilish

### `PUT /api/rental-contracts/{id}/cancel`

`DRAFT` yoki `POSTED` shartnomani `CANCELLED` holatiga o‘tkazadi. `CANCELLED` shartnoma bo‘yicha yangi avtomatik hisoblash yaratilmaydi.

Shartnomani bekor qilish oldin yaratilgan ijara hisoblash hujjatlarini avtomatik bekor qilmaydi. Ular kerak bo‘lsa alohida `rental-accrual-docs/{id}/cancel` orqali bekor qilinadi.

Response — `204 No Content`.

Permission: `RENTAL_CONTRACT_CANCEL`.

## 16. Hisoblash hujjati qanday yaratiladi

Faqat quyidagi shartlarga mos obyektlar olinadi:

- shartnoma `POSTED`;
- shartnoma va obyekt faol;
- shartnoma pullik;
- obyekt tanlangan yil va oy bilan kesishadi;
- tanlangan oy uchun oldin hisoblash yaratilmagan;
- tanlangan oy shartnoma, obyekt yoki bekor qilish sanasidan keyin emas.

Hisoblash davri kalendar oy chegaralari bo‘yicha yaratiladi. `MONTH` uchun to‘liq oyda `periodAmount` to‘liq olinadi, to‘liq bo‘lmagan oyda esa faol kunlar soniga mutanosib hisoblanadi. `DAY` uchun `periodAmount × faol kunlar soni` ishlatiladi.

Masalan:

```text
startDate    = 2024-07-09
endDate      = 2024-10-09
periodUnit   = MONTH
periodAmount = 470 000
```

Shartnomaning birinchi yoki oxirgi oyi to‘liq bo‘lmasa, `contractAmount` va `taxBaseAmount` kalendar oydagi kunlar soniga nisbatan proporsional kamaytiriladi va 2 kasr xonasigacha yaxlitlanadi:

```text
partialAmount = round(fullAmount × actualDayCount / scheduledDayCount, 2)
```

Natija:

```text
09.07–31.07: 23 / 31 × 470 000 = 348 709,68
01.08–31.08:                  = 470 000,00
01.09–30.09:                  = 470 000,00
01.10–09.10:  9 / 31 × 470 000 = 136 451,61
jami:                         = 1 425 161,29
```

Bir obyekt uchun tanlangan kalendar oyda boshqa hisoblash davri mavjud bo‘lsa, u ikkinchi marta yaratilmaydi.

Har oyning birinchi kuni Toshkent vaqti bilan `00:00` da fon jarayoni barcha tashkilotlar uchun oldingi kalendar oy hisoblashlarini yaratadi. Masalan, `2024-08-01 00:00` da 2024-yil iyul oyi hisoblanadi. Hujjat `DRAFT` statusida yaratiladi.

## 17. Muddati kelgan hisoblashlarni qo‘lda yaratish

### `POST /api/rental-accrual-docs/generate-due`

Request:

```json
{
  "year": 2024,
  "month": 7
}
```

`year` `1..9999`, `month` esa `1..12` oralig‘ida bo‘lishi kerak. Backend faqat tanlangan kalendar oy hisoblashini yaratadi. Bitta shartnomaning shu oyga tegishli bir nechta obyektlari bitta hisoblash hujjatining alohida `items[]` satrlariga tushadi.

Response:

```json
{
  "createdDocumentCount": 1,
  "createdItemCount": 2,
  "documentIds": [31]
}
```

`docNumber` backend tomonidan tashkilot, hujjat turi va yil bo‘yicha avtomatik yaratiladi.

Permission: `RENTAL_ACCRUAL_GENERATE`.

## 18. Hisoblash hujjatlari ro‘yxati

### `GET /api/rental-accrual-docs`

Query parametrlari:

| Parametr | Turi | Majburiy | Tavsifi |
|---|---|---:|---|
| `contractId` | `number` | Yo‘q | Bitta shartnoma hisoblashlari |
| `statusId` | `number` | Yo‘q | Status filtri |
| `dateFrom` | `date` | Yo‘q | Hujjat sanasi boshi |
| `dateTo` | `date` | Yo‘q | Hujjat sanasi oxiri, to‘liq kun qo‘shiladi |
| `search` | `string` | Yo‘q | Hujjat raqami, shartnoma raqami, ijaraga beruvchi nomi, INN yoki PINFL |
| `page` | `number` | Yo‘q | Sahifa |
| `pageSize` | `number` | Yo‘q | Sahifa hajmi |

Response:

```json
{
  "items": [
    {
      "id": 31,
      "contractId": 15,
      "contractNumber": "IJ-25/2026",
      "lessors": [
        {
          "id": 8,
          "lessorKindCode": "INDIVIDUAL",
          "counterpartyId": null,
          "fullName": "Ali Valiyev",
          "inn": "301111111",
          "pinfl": "12345678901234",
          "phoneNumber": "+998901234567",
          "registeredAddress": "Navoiy viloyati, Karmana tumani",
          "residentialAddress": "Navoiy shahri"
        }
      ],
      "docNumber": "1",
      "docDate": "2026-09-30T00:00:00",
      "currencyCode": "UZS",
      "taxAmount": 648000,
      "payableAmount": 3960000,
      "amount": 4608000,
      "statusId": 1,
      "statusName": "Draft"
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

Permission: `RENTAL_ACCRUAL_VIEW`.

## 19. Hisoblash hujjatini ID bo‘yicha olish

### `GET /api/rental-accrual-docs/{id}`

Response:

```json
{
  "id": 31,
  "organizationId": 2,
  "contractId": 15,
  "contractNumber": "IJ-25/2026",
  "lessors": [
    {
      "id": 8,
      "lessorKindCode": "INDIVIDUAL",
      "counterpartyId": null,
      "fullName": "Ali Valiyev",
      "inn": "301111111",
      "pinfl": "12345678901234",
      "phoneNumber": "+998901234567",
      "registeredAddress": "Navoiy viloyati, Karmana tumani",
      "residentialAddress": "Navoiy shahri"
    }
  ],
  "docNumber": "1",
  "docDate": "2026-09-30T00:00:00",
  "currencyId": 1,
  "currencyCode": "UZS",
  "exchangeRate": 1,
  "contractAmount": 4500000,
  "taxBaseAmount": 5400000,
  "taxAmount": 648000,
  "payableAmount": 3960000,
  "amount": 4608000,
  "lessorPayableAccountId": 210,
  "lessorPayableAccountNumber": "6990",
  "lessorPayableAccountName": "Boshqa majburiyatlar",
  "taxPayableAccountId": 220,
  "taxPayableAccountNumber": "6410",
  "taxPayableAccountName": "Byudjetga to‘lovlar",
  "statusId": 1,
  "statusName": "Draft",
  "comment": null,
  "createdDate": "2026-09-04T00:00:01",
  "postedAt": null,
  "cancelledAt": null,
  "items": [
    {
      "id": 50,
      "contractObjectId": 19,
      "objectName": "Ofis binosi",
      "periodFrom": "2026-09-04T00:00:00",
      "periodTo": "2026-09-30T00:00:00",
      "contractAmount": 4500000,
      "taxBaseAmount": 5400000,
      "taxRate": 12,
      "taxAmount": 648000,
      "payableAmount": 3960000,
      "amount": 4608000,
      "expenseAccountId": 200,
      "expenseAccountNumber": "9430",
      "expenseAccountName": "Boshqa operatsion xarajatlar"
    }
  ]
}
```

Hujjat darajasidagi summalar `items[]` satrlaridagi summalar yig‘indisidir.

Permission: `RENTAL_ACCRUAL_VIEW_DETAIL`.

## 20. Hisoblash hujjatini tahrirlash

### `PUT /api/rental-accrual-docs/{id}`

Bu API hisoblangan summalarni o‘zgartirmaydi. Faqat kurs, izoh va provodka hisobvaraqlarini yangilaydi.

Request:

```json
{
  "exchangeRate": 1,
  "lessorPayableAccountId": 210,
  "taxPayableAccountId": 220,
  "comment": "Hisobvaraqlar tekshirildi",
  "items": [
    {
      "itemId": 50,
      "expenseAccountId": 200
    }
  ]
}
```

Qoidalar:

- faqat `DRAFT` hujjat yangilanadi;
- requestda hujjatning barcha `items[]` ID’lari aynan bir marta yuboriladi;
- hisobvaraqlar joriy tashkilotga tegishli va faol bo‘lishi kerak;
- xarajat, ijaraga beruvchi qarzi va soliq hisobvaraqlari bir-biriga teng bo‘lmaydi;
- `exchangeRate > 0`.

Response — `204 No Content`.

Permission: `RENTAL_ACCRUAL_UPDATE`.

## 21. Hisoblash hujjatini o‘chirish

### `DELETE /api/rental-accrual-docs/{id}`

Faqat `DRAFT` hujjatni o‘chiradi. O‘chirilgan hujjatdagi eng eski davr obyektning `nextAccrualDate` maydoniga qaytariladi. Shu sababli keyingi `generate-due` chaqiruvida davr qaytadan yaratilishi mumkin.

Response — `204 No Content`.

Permission: `RENTAL_ACCRUAL_DELETE`.

## 22. Hisoblash hujjatini buxgalteriya hisobiga o‘tkazish

### `PUT /api/rental-accrual-docs/{id}/post`

`DRAFT` hujjatni `POSTED` holatiga o‘tkazadi va provodkalarni yaratadi.

Backend quyidagilarni qayta tekshiradi:

- hujjat sanasidagi buxgalteriya davri ochiq;
- summa formulalari o‘zgarmagan;
- barcha satrlar shartnoma obyektlariga tegishli;
- barcha hisobvaraqlar mavjud, faol va joriy tashkilotga tegishli;
- ushbu hujjat uchun oldingi faol provodkalar mavjud emas.

Har bir hisoblash satri uchun ko‘pi bilan ikkita provodka yaratiladi:

| Debet | Kredit | Summa | Maqsadi |
|---|---|---:|---|
| `items[].expenseAccountId` | `lessorPayableAccountId` | `payableAmount` | Ijaraga beruvchi oldidagi qarz |
| `items[].expenseAccountId` | `taxPayableAccountId` | `taxAmount` | Soliq majburiyati |

Summa `0` bo‘lgan provodka yaratilmaydi.

Yuqoridagi misolda:

```text
Dt ijara xarajati  4 400 000  Kt ijaraga beruvchiga qarz
Dt ijara xarajati    720 000  Kt soliq majburiyati
Jami xarajat       5 120 000
```

Response — `204 No Content`.

Permission: `RENTAL_ACCRUAL_POST`.

## 23. Hisoblash hujjatini bekor qilish

### `PUT /api/rental-accrual-docs/{id}/cancel`

`DRAFT` yoki `POSTED` hujjatni `CANCELLED` holatiga o‘tkazadi.

- `DRAFT` hujjatda faqat status o‘zgaradi;
- `POSTED` hujjatda dastlabki provodkalarga qarama-qarshi storno provodkalari yaratiladi;
- dastlabki posting batch `REVERSED` holatiga o‘tadi;
- dastlabki hujjat sanasi va bekor qilish sanasidagi buxgalteriya davrlari ochiq bo‘lishi kerak.

Response — `204 No Content`.

`DRAFT` hujjatni cancel qilish obyektning `nextAccrualDate` qiymatini orqaga qaytarmaydi. Shu davrni qayta generatsiya qilish kerak bo‘lsa, cancel emas, `DELETE` ishlatiladi.

Permission: `RENTAL_ACCRUAL_CANCEL`.

## 24. Frontendda qanday ekranlar bo‘lishi kerak

### Ijara shartnomalari ro‘yxati

Ko‘rsatish tavsiya etiladi:

- shartnoma raqami va sanasi;
- amal qilish davri;
- bepul/pullik belgisi;
- barcha ijaraga beruvchilarning qisqa ro‘yxati;
- obyektlar soni;
- valyuta;
- status.

Amallar:

- ko‘rish;
- yangi shartnoma;
- `DRAFT` uchun tahrirlash, activate va o‘chirish;
- `DRAFT` yoki `POSTED` uchun cancel;
- shu shartnoma bo‘yicha hisoblashlarni ochish.

### Shartnoma formasi

Forma uchta mantiqiy blokka bo‘linishi kerak:

1. **Shartnoma** — raqam, sana, davr, valyuta, bepul/pullik, hisobvaraqlar.
2. **Ijaraga beruvchilar** — tur, nom/F.I.Sh., INN/PINFL, telefon va ikki manzil.
3. **Obyektlar** — tur, nom, kadastr/tashqi raqam, manzil, maydon, davriylik, summalar, xarajat hisobi va kommunal xizmatlar.

`isFreeOfCharge = true` bo‘lganda frontend summa va soliq maydonlarini `0` qilib yuborishi, hisobvaraqlarni majburiy qilmasligi kerak.

### Ijara hisoblashlari ro‘yxati

Ko‘rsatish tavsiya etiladi:

- tizim hujjat raqami va sanasi;
- shartnoma raqami;
- ijaraga beruvchilar;
- ijaraga beruvchiga to‘lov;
- soliq;
- jami xarajat;
- status.

### Hisoblash hujjati formasi

Summalar faqat o‘qish uchun ko‘rsatiladi. `DRAFT` holatida quyidagilar tahrirlanadi:

- kurs;
- ijaraga beruvchiga qarz hisobvarag‘i;
- soliq hisobvarag‘i;
- har bir satrning xarajat hisobvarag‘i;
- izoh.

## 25. O‘zgargan API kontrakti

Eski shartnoma request/response maydonlari olib tashlandi:

```text
lessorFullName
lessorInn
lessorPinfl
```

Ularning o‘rniga:

```json
{
  "lessors": [
    {
      "lessorKindCode": "INDIVIDUAL",
      "fullName": "...",
      "inn": "...",
      "pinfl": "...",
      "phoneNumber": "...",
      "registeredAddress": "...",
      "residentialAddress": "..."
    }
  ]
}
```

qo‘llaniladi.

Shartnoma va hisoblash ro‘yxati response’laridagi eski bitta ijaraga beruvchi maydonlari ham `lessors[]` ro‘yxatiga almashtirildi.

Yangi maydonlar:

```text
isFreeOfCharge
objects[].totalArea
objects[].rentedArea
objects[].utilities[]
```

Yangi API:

```text
GET /api/manuals/utility-services
```

## 26. Frontend uchun muhim cheklovlar

- Ijaraga beruvchilar o‘rtasida summa yoki ulush taqsimoti yo‘q.
- Kommunal xizmatlar hisob-kitob summasiga avtomatik qo‘shilmaydi.
- Maydonlar bo‘yicha ijara summasi avtomatik hisoblanmaydi.
- Soliq stavkasi tashkilot yoki ijaraga beruvchi turidan avtomatik olinmaydi.
- Yuridik shaxsning schyot-fakturasi bu modulda avtomatik yaratilmaydi yoki bog‘lanmaydi.
- Pullik shartnomada soliq stavkasi `0` bo‘lsa ham activate/post uchun soliq hisobvarag‘i talab qilinadi.
- Oxirgi qisqa davr summasi rejalashtirilgan davr kunlariga nisbatan avtomatik proporsional kamaytiriladi.
- Shartnomani cancel qilish ilgari yaratilgan hisoblashlarni cancel qilmaydi.
- Shartnoma raqami frontenddan olinadi; hisoblash hujjati raqami backendda avtomatik yaratiladi.
- `exchangeRate` saqlanadi va musbatligi tekshiriladi, lekin amaldagi ijara posting builder summani kursga ko‘paytirmaydi.

Frontend bu cheklovlarni foydalanuvchiga tushunarli izoh bilan ko‘rsatishi kerak.

## 27. Xatolar formati

Biznes xatolari `ProblemDetails` formatida qaytariladi:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "RentalContract.InvalidReference",
  "status": 422,
  "detail": "Valyuta, obyekt turi, kommunal xizmat yoki hisobvaraq faol emas yoxud tashkilotga tegishli emas."
}
```

Asosiy xato kodlari:

| HTTP | `title` | Sababi |
|---:|---|---|
| `404` | `RentalContract.NotFound` | Shartnoma topilmadi yoki boshqa tashkilotga tegishli |
| `409` | `RentalContract.InvalidStatus` | Amal joriy statusda mumkin emas |
| `409` | `RentalContract.DuplicateNumber` | Shu tashkilot va yil uchun shartnoma raqami mavjud |
| `409` | `RentalContract.ExistingAccruals` | Shartnomada hisoblash mavjudligi sababli o‘chirib bo‘lmaydi |
| `422` | `RentalContract.MissingAccounts` | Kerakli hisobvaraqlar to‘liq emas |
| `422` | `RentalContract.InvalidReference` | Valyuta, tur, xizmat yoki hisobvaraq noto‘g‘ri |
| `422` | `RentalContract.InvalidLessor` | Ijaraga beruvchi turi yoki identifikatori noto‘g‘ri |
| `404` | `RentalAccrual.NotFound` | Hisoblash hujjati topilmadi |
| `409` | `RentalAccrual.InvalidStatus` | Amal joriy statusda mumkin emas |
| `409` | `RentalAccrual.EffectsAlreadyExist` | Hujjat uchun provodka allaqachon mavjud |
| `422` | `RentalAccrual.InvalidAmounts` | Summalar backend formulalariga mos emas |
| `422` | `RentalAccrual.InvalidAccounts` | Hisobvaraqlar noto‘g‘ri yoki boshqa tashkilotga tegishli |
| `422` | `RentalAccrual.ItemMismatch` | Update request satrlari hujjat satrlariga mos emas |

FluentValidation xatolari odatda `400 Bad Request`, autentifikatsiya va permission xatolari `401/403` qaytaradi.

## 28. Tavsiya etilgan frontend ketma-ketligi

1. Valyuta, obyekt turi, kommunal xizmat va hisobvaraqlar manuals’larini yuklang.
2. Shartnomani `POST /api/rental-contracts` orqali `DRAFT` holatida yarating.
3. `GET /api/rental-contracts/{id}` orqali saqlangan natijani qayta oling.
4. Pullik shartnomada barcha hisobvaraqlar tanlanganini tekshiring.
5. `PUT /api/rental-contracts/{id}/activate` orqali faollashtiring.
6. Hisoblashlar avtomatik yaratilishini kuting yoki `generate-due`ni ishga tushiring.
7. Hisoblash hujjatini ochib summalar va hisobvaraqlarni tekshiring.
8. Kerak bo‘lsa faqat hisobvaraqlar/kurs/izohni update qiling.
9. `PUT /api/rental-accrual-docs/{id}/post` orqali provodka yarating.
10. Xato bo‘lsa posted hisoblashni `cancel` qilib storno qiling.

## 29. Permissionlar

```text
RENTAL_CONTRACT_VIEW
RENTAL_CONTRACT_VIEW_DETAIL
RENTAL_CONTRACT_CREATE
RENTAL_CONTRACT_UPDATE
RENTAL_CONTRACT_DELETE
RENTAL_CONTRACT_ACTIVATE
RENTAL_CONTRACT_CANCEL

RENTAL_ACCRUAL_VIEW
RENTAL_ACCRUAL_VIEW_DETAIL
RENTAL_ACCRUAL_UPDATE
RENTAL_ACCRUAL_DELETE
RENTAL_ACCRUAL_GENERATE
RENTAL_ACCRUAL_POST
RENTAL_ACCRUAL_CANCEL
```
