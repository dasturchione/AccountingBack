# Изменения API для frontend — 28.08.2026

Документ описывает API, добавленные, изменённые и удалённые 28 августа 2026 года. Он охватывает инкассацию, общий реестр документов, точки приёма платежей, операции по ним, связь банковских операций с документами, изменения розничной продажи и упрощение карточки контрагента.

## 1. Общие требования

Все API требуют авторизацию:

```http
Authorization: Bearer {token}
```

Для данных конкретной организации передаются:

```http
X-OrganizationId: 2
X-Language: ru
```

`X-Language` определяет язык переводимых названий: `uz`, `ru` или `en`.

### 1.1. Формат пагинации

Все списочные API с параметрами `page` и `pageSize` возвращают:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### 1.2. Общие значения

Направление движения средств:

| `directionId` | Код | Значение |
|---:|---|---|
| `1` | `IN` | Поступление на источник учёта. |
| `-1` | `OUT` | Списание с источника учёта. |

Статусы документов, используемые новыми API:

| `statusId` | Код | Значение |
|---:|---|---|
| `1` | `DRAFT` | Черновик. |
| `2` | `POSTED` | Проведён. |
| `3` | `CANCELLED` | Отменён. |
| `4` | `PENDING` | Ожидает подтверждения. |
| `5` | `IN_TRANSIT` | Деньги в пути. |
| `6` | `COMPLETED` | Операция завершена. |

### 1.3. Сводка изменений

| Область | Изменение frontend |
|---|---|
| Инкассация | Добавлен полный API `/api/cash-collection-docs`. |
| Общий реестр документов | Добавлен read-only API `/api/documents`. |
| Точки приёма платежей | `/api/bank-terminals` заменён на `/api/payment-acceptance-points`. |
| Операции точек приёма | Добавлен `/api/payment-acceptance-point-operations`. |
| Банковские операции | Добавлен `relatedDocumentId` и данные связанного документа в response. |
| Розничная продажа | `bankTerminalId` заменён на `paymentAcceptancePointId`; при подтверждении автоматически создаются операции точки приёма. |
| Контрагенты | Удалены тип и признаки клиента/поставщика. Карточка стала нейтральной. |
| Manuals | Добавлены справочники точек и их типов; удалены terminals, counterparty-types, suppliers и clients. |

---

## 2. Новые manuals API

### 2.1. Точки приёма платежей

```http
GET /api/manuals/payment-acceptance-points
```

Назначение: заполнение select точки приёма платежа в розничной продаже и фильтрах операций.

Request body отсутствует.

Ответ:

```json
[
  {
    "id": 15,
    "name": "POS-терминал кассы №1",
    "code": "PAP-57A24A865A074C31B1CE927F6FA46062"
  }
]
```

| Поле | Тип | Назначение |
|---|---|---|
| `id` | `number` | Передаётся как `paymentAcceptancePointId`. |
| `name` | `string` | Название для отображения. |
| `code` | `string` | Автоматически созданный системный код. |

### 2.2. Типы точек приёма платежей

```http
GET /api/manuals/payment-acceptance-point-types
```

Назначение: select типа при создании или изменении точки приёма платежей.

Request body отсутствует.

Ответ:

```json
[
  {
    "id": 1,
    "name": "POS-терминал",
    "code": "POS"
  },
  {
    "id": 2,
    "name": "QR-платёж",
    "code": "QR"
  }
]
```

Доступные коды:

| Код | Значение |
|---|---|
| `POS` | Физический POS-терминал. |
| `QR` | QR-платёж. |
| `PAYMENT_LINK` | Платёжная ссылка. |
| `MARKETPLACE` | Маркетплейс. |
| `MOBILE_APP` | Мобильное приложение или платёжный сервис. |
| `OTHER` | Другой способ безналичного приёма. |

Frontend должен сохранять и отправлять `id`; `code` нужен для UI-логики и не заменяет `id`.

---

## 3. Точки приёма платежей — новый CRUD

Точка приёма платежей — универсальная замена банковского терминала. Это может быть POS, QR, платёжная ссылка, merchant в приложении или маркетплейс.

### 3.1. Маршруты

| Метод | URL | Назначение | Успешный ответ |
|---|---|---|---|
| `GET` | `/api/payment-acceptance-points` | Список с фильтрами. | `200`, paged response. |
| `GET` | `/api/payment-acceptance-points/{id}` | Полная карточка. | `200`, объект. |
| `POST` | `/api/payment-acceptance-points` | Создать точку. | `200`, числовой `id`. |
| `PUT` | `/api/payment-acceptance-points/{id}` | Изменить точку. | `204`. |
| `DELETE` | `/api/payment-acceptance-points/{id}` | Логически удалить точку. | `204`. |

### 3.2. Список

```http
GET /api/payment-acceptance-points?typeId=1&bankAccountId=4&stateId=1&search=POS&page=1&pageSize=20
```

| Query-параметр | Тип | Обязательный | Назначение |
|---|---|---|---|
| `typeId` | `number` | Нет | Тип точки. |
| `bankAccountId` | `number` | Нет | Связанный банковский счёт организации. |
| `stateId` | `number` | Нет | Состояние записи. |
| `search` | `string` | Нет | Поиск по основным текстовым полям. |
| `page` | `number` | Нет | Страница, по умолчанию `1`. |
| `pageSize` | `number` | Нет | Размер страницы. |

### 3.3. POST request

```json
{
  "typeId": 1,
  "bankAccountId": 4,
  "name": "POS-терминал кассы №1",
  "merchantId": "MERCHANT-001",
  "externalId": "TERMINAL-0098",
  "serialNumber": "SN-123456"
}
```

### 3.4. PUT request

```json
{
  "typeId": 1,
  "bankAccountId": 4,
  "name": "POS-терминал кассы №1",
  "merchantId": "MERCHANT-001",
  "externalId": "TERMINAL-0098",
  "serialNumber": "SN-123456",
  "stateId": 1
}
```

| Поле request | Тип | Nullable | Назначение |
|---|---|---|---|
| `typeId` | `number` | Нет | ID из `/api/manuals/payment-acceptance-point-types`. |
| `bankAccountId` | `number` | Да | Банковский счёт, на который сервис перечисляет деньги. |
| `name` | `string(250)` | Нет | Пользовательское название. |
| `merchantId` | `string(150)` | Да | Идентификатор merchant у банка/провайдера. |
| `externalId` | `string(150)` | Да | Внешний идентификатор точки. |
| `serialNumber` | `string(150)` | Да | Серийный номер оборудования, если существует. |
| `stateId` | `number` | Нет | Только для `PUT`: состояние записи. |

Поле `code` не передаётся. Backend создаёт его автоматически в формате `PAP-{GUID}`.

### 3.5. GET response

```json
{
  "id": 15,
  "code": "PAP-57A24A865A074C31B1CE927F6FA46062",
  "organizationId": 2,
  "organizationName": "ARTEL",
  "typeId": 1,
  "typeCode": "POS",
  "typeName": "POS-терминал",
  "bankAccountId": 4,
  "bankAccountNumber": "20208000900000000001",
  "name": "POS-терминал кассы №1",
  "merchantId": "MERCHANT-001",
  "externalId": "TERMINAL-0098",
  "serialNumber": "SN-123456",
  "stateId": 1,
  "stateName": "Активный",
  "createdDate": "2026-08-28T16:20:00"
}
```

Поля элемента списка совпадают с полями detail response.

---

## 4. Операции точки приёма платежей — новый API

API хранит приход и расход электронных денег относительно выбранной точки приёма. Наличные платежи сюда не попадают.

### 4.1. Маршруты

| Метод | URL | Назначение | Успешный ответ |
|---|---|---|---|
| `GET` | `/api/payment-acceptance-point-operations` | Список операций. | `200`, paged response. |
| `GET` | `/api/payment-acceptance-point-operations/{id}` | Детали операции. | `200`, объект. |
| `GET` | `/api/payment-acceptance-point-operations/balance` | Остаток в точке. | `200`, объект остатка. |
| `POST` | `/api/payment-acceptance-point-operations` | Создать ручной черновик. | `200`, числовой `id`. |
| `PUT` | `/api/payment-acceptance-point-operations/{id}` | Изменить черновик. | `204`. |
| `POST` | `/api/payment-acceptance-point-operations/{id}/confirm` | Провести DRAFT/PENDING. | `204`. |
| `POST` | `/api/payment-acceptance-point-operations/{id}/cancel` | Отменить операцию. | `204`. |
| `DELETE` | `/api/payment-acceptance-point-operations/{id}` | Логически удалить черновик. | `204`. |

### 4.2. Список и фильтры

```http
GET /api/payment-acceptance-point-operations?paymentAcceptancePointId=15&directionId=1&currencyId=1&statusId=4&relatedDocumentId=9001&dateFrom=2026-08-01&dateTo=2026-08-31&search=&page=1&pageSize=20
```

| Query-параметр | Тип | Обязательный | Назначение |
|---|---|---|---|
| `paymentAcceptancePointId` | `number` | Нет | Фильтр по точке. |
| `directionId` | `number` | Нет | `1` — приход, `-1` — расход. |
| `currencyId` | `number` | Нет | Валюта. |
| `statusId` | `number` | Нет | Статус операции. |
| `relatedDocumentId` | `number` | Нет | ID записи общего реестра документов. |
| `dateFrom`, `dateTo` | `datetime` | Нет | Период по `docDate`. |
| `search` | `string` | Нет | Поиск. |
| `page`, `pageSize` | `number` | Нет | Пагинация. |

Элемент списка:

```json
{
  "id": 701,
  "paymentAcceptancePointId": 15,
  "paymentAcceptancePointCode": "PAP-57A24A865A074C31B1CE927F6FA46062",
  "paymentAcceptancePointName": "POS-терминал кассы №1",
  "directionId": -1,
  "directionName": "Выбытие",
  "docNumber": "2",
  "docDate": "2026-08-28T17:10:00",
  "currencyId": 1,
  "currencyCode": "UZS",
  "amount": 6000000,
  "externalTransactionNumber": "TX-10001",
  "relatedDocumentId": 9001,
  "relatedDocumentNumber": "145",
  "statusId": 4,
  "statusName": "Ожидает",
  "stateId": 1,
  "createdDate": "2026-08-28T17:10:05"
}
```

### 4.3. POST и PUT request

```json
{
  "paymentAcceptancePointId": 15,
  "directionId": -1,
  "docDate": "2026-08-28T17:10:00",
  "currencyId": 1,
  "amount": 6000000,
  "exchangeRate": 1,
  "externalTransactionNumber": "TX-10001",
  "comment": "Ожидается перечисление на банковский счёт"
}
```

| Поле request | Тип | Nullable | Назначение |
|---|---|---|---|
| `paymentAcceptancePointId` | `number` | Нет | Источник электронных денег. |
| `directionId` | `number` | Нет | `1` — приход, `-1` — расход. |
| `docDate` | `datetime` | Нет | Дата и время операции. |
| `currencyId` | `number` | Нет | Валюта. |
| `amount` | `decimal` | Нет | Сумма, строго больше `0`. |
| `exchangeRate` | `decimal` | Нет | Курс, строго больше `0`; обычно `1`. |
| `externalTransactionNumber` | `string(150)` | Да | Номер операции провайдера/терминала. |
| `comment` | `string(1000)` | Да | Комментарий. |

`docNumber`, `statusId`, `relatedDocumentId` и служебные даты в ручном request не передаются. Backend создаёт номер и статус `DRAFT`.

### 4.4. Detail response

```json
{
  "id": 701,
  "organizationId": 2,
  "organizationName": "ARTEL",
  "paymentAcceptancePointId": 15,
  "paymentAcceptancePointCode": "PAP-57A24A865A074C31B1CE927F6FA46062",
  "paymentAcceptancePointName": "POS-терминал кассы №1",
  "directionId": -1,
  "directionName": "Выбытие",
  "docNumber": "2",
  "docDate": "2026-08-28T17:10:00",
  "currencyId": 1,
  "currencyCode": "UZS",
  "currencyName": "Сум",
  "amount": 6000000,
  "exchangeRate": 1,
  "externalTransactionNumber": "TX-10001",
  "relatedDocumentId": 9001,
  "relatedDocumentTypeId": 15,
  "relatedDocumentEntityId": 310,
  "relatedDocumentNumber": "145",
  "relatedDocumentDate": "2026-08-28T17:09:00",
  "comment": "Created from retail sale payment",
  "statusId": 4,
  "statusName": "Ожидает",
  "stateId": 1,
  "stateName": "Активный",
  "createdDate": "2026-08-28T17:10:05",
  "postedAt": null,
  "postedByUserId": null,
  "cancelledAt": null,
  "cancelledByUserId": null
}
```

| Поле связанного документа | Значение |
|---|---|
| `relatedDocumentId` | ID записи в `/api/documents`. |
| `relatedDocumentTypeId` | Тип исходного документа. |
| `relatedDocumentEntityId` | ID исходной сущности, например `rtl_sale_doc.id`. |
| `relatedDocumentNumber` | Номер исходного документа. |
| `relatedDocumentDate` | Дата исходного документа. |

### 4.5. Остаток

```http
GET /api/payment-acceptance-point-operations/balance?paymentAcceptancePointId=15&currencyId=1&asOfDate=2026-08-28T23:59:59
```

`asOfDate` необязателен. Если не передан, используется текущая дата и время.

```json
{
  "paymentAcceptancePointId": 15,
  "currencyId": 1,
  "asOfDate": "2026-08-28T23:59:59",
  "balance": 6000000
}
```

В остаток входят только проведённые (`POSTED`) движения. `PENDING` не уменьшает остаток до подтверждения.

### 4.6. Правила статусов

- Ручное создание всегда создаёт `DRAFT`.
- Изменять и удалять можно только `DRAFT`.
- Подтвердить можно `DRAFT` или `PENDING`; после этого статус `POSTED`.
- Отменить можно `DRAFT`, `PENDING` и `POSTED`; результат — `CANCELLED`.
- При проведении `OUT` backend запрещает сумму больше доступного остатка.
- Отмена проведённого движения создаёт обратное движение в денежном регистре.

---

## 5. Общий реестр документов — новый read-only API

Реестр даёт единый ID для документов разных подсистем. Он используется, чтобы банковская операция или операция точки приёма могла ссылаться на розничную продажу, инкассацию, зарплатный документ и другие документы без отдельных FK для каждого типа.

### 5.1. Список документов

```http
GET /api/documents?documentTypeId=24&currencyId=1&statusId=5&stateId=1&dateFrom=2026-08-01&dateTo=2026-08-31&search=145&page=1&pageSize=20
```

| Query-параметр | Тип | Обязательный | Назначение |
|---|---|---|---|
| `documentTypeId` | `number` | Нет | Тип документа. |
| `currencyId` | `number` | Нет | Валюта. |
| `statusId` | `number` | Нет | Статус документа. |
| `stateId` | `number` | Нет | Состояние; по умолчанию `1` — активный. |
| `dateFrom`, `dateTo` | `datetime` | Нет | Период по дате документа. |
| `search` | `string` | Нет | Поиск по номеру и связанным текстовым данным. |
| `page`, `pageSize` | `number` | Нет | Пагинация. |

### 5.2. Документ по ID

```http
GET /api/documents/{id}
```

Request body отсутствует.

### 5.3. Response

Список возвращает стандартную пагинацию; `GET /{id}` возвращает один объект такого же формата:

```json
{
  "id": 9001,
  "organizationId": 2,
  "documentTypeId": 24,
  "documentTypeCode": "cash_collection",
  "documentTypeName": "Инкассация",
  "documentId": 310,
  "docNumber": "17",
  "docDate": "2026-08-28T10:00:00",
  "amount": 6000000,
  "currencyId": 1,
  "currencyCode": "UZS",
  "currencyName": "Сум",
  "statusId": 5,
  "statusCode": "IN_TRANSIT",
  "statusName": "В пути",
  "stateId": 1,
  "stateName": "Активный",
  "createdDate": "2026-08-28T09:55:00",
  "updatedDate": "2026-08-28T10:00:01"
}
```

Критически важно:

```text
relatedDocumentId = response.id
```

`documentId` — ID строки в исходной таблице документа. Его нельзя передавать как `relatedDocumentId`.

API только для чтения. `POST`, `PUT` и `DELETE` для `/api/documents` отсутствуют: записи создаются и обновляются автоматически бизнес-документами.

---

## 6. Инкассация: касса → деньги в пути → банк

### 6.1. Бизнес-процесс

```text
DRAFT
  -> PUT /send-to-bank
IN_TRANSIT: сумма списана из кассы, но ещё не зачислена в банк
  -> создание входящей bank-operation с relatedDocumentId
  -> POST /api/bank-operations/{id}/confirm
COMPLETED: сумма зачислена на банковский счёт
```

Отмена банковской операции, завершившей инкассацию, возвращает документ инкассации из `COMPLETED` в `IN_TRANSIT`.

### 6.2. Маршруты

| Метод | URL | Назначение | Успешный ответ |
|---|---|---|---|
| `GET` | `/api/cash-collection-docs` | Список документов. | `200`, paged response. |
| `GET` | `/api/cash-collection-docs/in-transit` | Минимальный список ожидающих зачисления. | `200`, массив. |
| `GET` | `/api/cash-collection-docs/{id}` | Полный документ. | `200`, объект. |
| `POST` | `/api/cash-collection-docs` | Создать черновик. | `200`, числовой `id`. |
| `PUT` | `/api/cash-collection-docs/{id}` | Изменить черновик. | `204`. |
| `DELETE` | `/api/cash-collection-docs/{id}` | Логически удалить черновик. | `204`. |
| `PUT` | `/api/cash-collection-docs/{id}/send-to-bank` | Передать деньги в банк. | `204`. |
| `PUT` | `/api/cash-collection-docs/{id}/cancel` | Отменить документ. | `204`. |

### 6.3. Список и фильтры

```http
GET /api/cash-collection-docs?cashBoxId=2&bankAccountId=4&currencyId=1&statusId=5&dateFrom=2026-08-01&dateTo=2026-08-31&search=17&page=1&pageSize=20
```

| Query-параметр | Тип | Обязательный | Назначение |
|---|---|---|---|
| `cashBoxId` | `number` | Нет | Касса-отправитель. |
| `bankAccountId` | `number` | Нет | Банковский счёт-получатель. |
| `currencyId` | `number` | Нет | Валюта. |
| `statusId` | `number` | Нет | Статус документа. |
| `dateFrom`, `dateTo` | `datetime` | Нет | Период. |
| `search` | `string` | Нет | Поиск. |
| `page`, `pageSize` | `number` | Нет | Пагинация. |

### 6.4. POST и PUT request

```json
{
  "cashBoxId": 2,
  "bankAccountId": 4,
  "docDate": "2026-08-28T10:00:00",
  "currencyId": 1,
  "amount": 6000000,
  "exchangeRate": 1,
  "cashChartAccountId": 101,
  "cashInTransitAccountId": 102,
  "bankChartAccountId": 103,
  "comment": "Сдача наличной выручки"
}
```

| Поле request | Тип | Nullable | Назначение |
|---|---|---|---|
| `cashBoxId` | `number` | Нет | Касса, из которой списываются деньги. |
| `bankAccountId` | `number` | Нет | Банковский счёт назначения. |
| `docDate` | `datetime` | Нет | Дата и время документа. |
| `currencyId` | `number` | Нет | Валюта. |
| `amount` | `decimal` | Нет | Сумма, строго больше `0`. |
| `exchangeRate` | `decimal` | Нет | Курс, строго больше `0`; обычно `1`. |
| `cashChartAccountId` | `number` | Да | Бухгалтерский счёт кассы. Обязателен для передачи в банк. |
| `cashInTransitAccountId` | `number` | Да | Счёт «Денежные средства в пути». Обязателен для передачи в банк. |
| `bankChartAccountId` | `number` | Да | Бухгалтерский счёт банковского счёта. Обязателен для завершения через bank operation. |
| `comment` | `string(1000)` | Да | Комментарий. |

`docNumber`, `statusId`, `documentRegistryId` не передаются. Backend создаёт номер, статус `DRAFT` и запись общего реестра.

### 6.5. Detail response

```json
{
  "id": 310,
  "documentRegistryId": 9001,
  "organizationId": 2,
  "organizationName": "ARTEL",
  "docNumber": "17",
  "docDate": "2026-08-28T10:00:00",
  "cashBoxId": 2,
  "cashBoxName": "Основная касса",
  "bankAccountId": 4,
  "bankAccountNumber": "20208000900000000001",
  "bankName": "Uzsanoatqurilishbank",
  "currencyId": 1,
  "currencyCode": "UZS",
  "currencyName": "Сум",
  "amount": 6000000,
  "exchangeRate": 1,
  "cashChartAccountId": 101,
  "cashChartAccountNumber": "5010",
  "cashChartAccountName": "Денежные средства в кассе",
  "cashInTransitAccountId": 102,
  "cashInTransitAccountNumber": "5710",
  "cashInTransitAccountName": "Денежные средства в пути",
  "bankChartAccountId": 103,
  "bankChartAccountNumber": "5110",
  "bankChartAccountName": "Расчётный счёт",
  "bankOperationId": null,
  "statusId": 5,
  "statusName": "В пути",
  "stateId": 1,
  "stateName": "Активный",
  "comment": "Сдача наличной выручки",
  "createdDate": "2026-08-28T09:55:00",
  "inTransitAt": "2026-08-28T10:00:01",
  "inTransitByUserId": 2,
  "completedAt": null,
  "completedByUserId": null,
  "cancelledAt": null,
  "cancelledByUserId": null,
  "cancelledFromStatusId": null
}
```

Список возвращает эти же поля в `items`.

### 6.6. Минимальный список «в пути»

```http
GET /api/cash-collection-docs/in-transit?bankAccountId=4
```

`bankAccountId` необязателен. API возвращает только активные `IN_TRANSIT` документы, которые ещё не связаны с активной банковской операцией.

```json
[
  {
    "id": 310,
    "documentRegistryId": 9001,
    "docNumber": "17",
    "docDate": "2026-08-28T10:00:00",
    "cashBoxId": 2,
    "cashBoxName": "Основная касса",
    "bankAccountId": 4,
    "bankAccountNumber": "20208000900000000001",
    "bankName": "Uzsanoatqurilishbank",
    "currencyId": 1,
    "currencyCode": "UZS",
    "amount": 6000000
  }
]
```

Для связи с банковской операцией frontend должен передать именно `documentRegistryId` как `relatedDocumentId`.

### 6.7. Ограничения жизненного цикла

- `PUT` и `DELETE` разрешены только для `DRAFT`.
- `send-to-bank` разрешён только для `DRAFT` и проверяет остаток кассы.
- После `send-to-bank` сумма больше недоступна в кассе; документ получает `IN_TRANSIT`.
- Отменить можно `DRAFT` или `IN_TRANSIT`.
- Нельзя отменить документ с активной связанной банковской операцией.
- `COMPLETED` сначала требует отмены связанной проведённой банковской операции.

---

## 7. Банковские операции — изменённый API

Маршруты не изменились, но request, response и фильтр расширены связью с общим документом.

### 7.1. Затронутые маршруты

| Метод | URL | Что изменилось |
|---|---|---|
| `GET` | `/api/bank-operations` | Новый фильтр и поля связанного документа в элементах. |
| `GET` | `/api/bank-operations/{id}` | Новые поля связанного документа. |
| `POST` | `/api/bank-operations` | Новый `relatedDocumentId`. |
| `POST` | `/api/bank-operations/many` | Новый `relatedDocumentId` в каждом элементе. |
| `PUT` | `/api/bank-operations/{id}` | Новый `relatedDocumentId`. |
| `POST` | `/api/bank-operations/{id}/confirm` | При связи с инкассацией завершает её. |
| `POST` | `/api/bank-operations/{id}/cancel` | При отмене возвращает инкассацию в `IN_TRANSIT`. |

### 7.2. Новый query-параметр списка

```http
GET /api/bank-operations?relatedDocumentId=9001&page=1&pageSize=20
```

`relatedDocumentId` фильтрует операции по ID записи общего реестра.

### 7.3. POST/PUT request

```json
{
  "bankAccountId": 4,
  "directionId": 1,
  "paymentTypeId": 2,
  "bankChartAccountId": 103,
  "offsetAccountId": 102,
  "counterpartyId": null,
  "counterpartyBankAccountId": null,
  "bankDocumentNumber": "12345",
  "classificationCategoryId": 8,
  "classificationRuleId": null,
  "relatedDocumentId": 9001,
  "docDate": "2026-08-28T14:30:00",
  "currencyId": 1,
  "amount": 6000000,
  "exchangeRate": 1,
  "comment": "Зачисление инкассации",
  "contractId": null
}
```

| Поле request | Тип | Nullable | Назначение |
|---|---|---|---|
| `bankAccountId` | `number` | Нет | Банковский счёт организации. |
| `directionId` | `number` | Нет | `1` — приход, `-1` — расход. |
| `paymentTypeId` | `number` | Да | Тип платежа. |
| `bankChartAccountId` | `number` | Да | Бухгалтерский счёт банка. |
| `offsetAccountId` | `number` | Да | Корреспондирующий бухгалтерский счёт. |
| `counterpartyId` | `number` | Да | Контрагент. |
| `counterpartyBankAccountId` | `number` | Да | Счёт контрагента. |
| `bankDocumentNumber` | `string(150)` | Да | Номер документа в банке. |
| `classificationCategoryId` | `number` | Да | Категория банковской операции. |
| `classificationRuleId` | `number` | Да | Сработавшее правило классификации. |
| `relatedDocumentId` | `number` | Да | **Новый:** `id` из `/api/documents`. |
| `docDate` | `datetime` | Нет | Дата и время операции. |
| `currencyId` | `number` | Нет | Валюта. |
| `amount` | `decimal` | Нет | Сумма, строго больше `0`. |
| `exchangeRate` | `decimal` | Нет | Курс; обычно `1`. |
| `comment` | `string(1000)` | Да | Комментарий/назначение. |
| `contractId` | `number` | Да | Договор. |

Bulk request:

```json
{
  "operations": [
    {
      "bankAccountId": 4,
      "directionId": 1,
      "relatedDocumentId": 9001,
      "docDate": "2026-08-28T14:30:00",
      "currencyId": 1,
      "amount": 6000000,
      "exchangeRate": 1
    }
  ]
}
```

`POST /api/bank-operations` возвращает один числовой ID. `POST /many` возвращает массив ID, например `[501, 502]`.

### 7.4. Новые поля response

Они добавлены и в list item, и в detail response:

```json
{
  "relatedDocumentId": 9001,
  "relatedDocumentTypeId": 24,
  "relatedDocumentTypeName": "Инкассация",
  "relatedDocumentEntityId": 310,
  "relatedDocumentNumber": "17",
  "relatedDocumentDate": "2026-08-28T10:00:00"
}
```

| Поле | Тип | Nullable | Назначение |
|---|---|---|---|
| `relatedDocumentId` | `number` | Да | ID записи общего реестра. |
| `relatedDocumentTypeId` | `number` | Да | Тип связанного документа. |
| `relatedDocumentTypeName` | `string` | Да | Название типа. |
| `relatedDocumentEntityId` | `number` | Да | ID исходного документа. |
| `relatedDocumentNumber` | `string` | Да | Номер связанного документа. |
| `relatedDocumentDate` | `datetime` | Да | Дата связанного документа. |

Остальные ранее существовавшие поля response не изменились.

### 7.5. Связь с инкассацией

Для `documentTypeCode = cash_collection` backend проверяет:

- инкассация активна и имеет статус `IN_TRANSIT`;
- организация, банковский счёт, валюта и сумма полностью совпадают;
- `directionId = 1` (`IN`);
- нет другой активной банковской операции для этой инкассации.

Для корректной инкассации backend принудительно устанавливает категорию `CASH_COLLECTION`, тип платежа `BANK`, счета банка и денег в пути из документа и очищает `counterpartyId`, `counterpartyBankAccountId`, `contractId`.

После подтверждения банковской операции инкассация становится `COMPLETED`. После отмены проведённой банковской операции она снова становится `IN_TRANSIT`.

---

## 8. Розничная продажа — изменённый API

### 8.1. Переименование поля оплаты

Во всех request и detail response:

```text
bankTerminalId   -> paymentAcceptancePointId
bankTerminalName -> paymentAcceptancePointName
```

Старые поля больше не поддерживаются.

### 8.2. Затронутые маршруты

| Метод | URL | Изменение |
|---|---|---|
| `GET` | `/api/retail-sale-docs/{id}` | Новые имена полей в `payments`. |
| `POST` | `/api/retail-sale-docs` | Новое поле в `payments`; проверка cash/non-cash. |
| `PUT` | `/api/retail-sale-docs/{id}` | Новое поле в `payments`; проверка cash/non-cash. |
| `PUT` | `/api/retail-sale-docs/{id}/confirm` | Новое поле в `payments`; автоматические операции точки. |
| `PUT` | `/api/retail-sale-docs/{id}/cancel` | Отмена связанных операций точки. |

### 8.3. POST request

```json
{
  "docDate": "2026-08-28T17:09:00",
  "counterpartyId": null,
  "warehouseId": 1,
  "cashRegisterId": 2,
  "currencyId": 1,
  "exchangeRate": 1,
  "receivableAccountId": 120,
  "vatAccountId": 121,
  "comment": "Розничная продажа",
  "processingMode": 2,
  "lines": [
    {
      "productId": 10,
      "quantity": 1,
      "unitId": 1,
      "unitPrice": 6000000,
      "costPrice": 4500000,
      "vatRateId": 2,
      "vatAmount": 642857.14,
      "inventoryAccountId": 130,
      "incomeAccountId": 131,
      "costAccountId": 132,
      "items": []
    }
  ],
  "payments": [
    {
      "paymentMethodId": 2,
      "paymentAcceptancePointId": 15,
      "debitAccountId": 140,
      "amount": 6000000,
      "transactionNumber": "TX-10001"
    }
  ]
}
```

`processingMode`: `1` — создать черновик, `2` — создать и сразу обработать.

### 8.4. PUT request

Формат совпадает с POST, но:

- `docDate` обязателен;
- передаётся `stateId`;
- `processingMode` отсутствует.

```json
{
  "docDate": "2026-08-28T17:09:00",
  "counterpartyId": null,
  "warehouseId": 1,
  "cashRegisterId": 2,
  "currencyId": 1,
  "exchangeRate": 1,
  "receivableAccountId": 120,
  "vatAccountId": 121,
  "comment": "Исправленная продажа",
  "stateId": 1,
  "lines": [],
  "payments": []
}
```

### 8.5. Поля payment request

| Поле | Тип | Nullable | Назначение |
|---|---|---|---|
| `paymentMethodId` | `number` | Нет | Метод оплаты из `/api/manuals/payment-methods`. |
| `paymentAcceptancePointId` | `number` | Условно | Точка приёма. Для `CASH` должна быть `null`; для любого non-cash обязательна. |
| `debitAccountId` | `number` | Нет | Дебетовый бухгалтерский счёт оплаты. |
| `amount` | `decimal` | Нет | Сумма оплаты. |
| `transactionNumber` | `string(100)` | Да | Номер внешней транзакции. |

### 8.6. Confirm request

```http
PUT /api/retail-sale-docs/{id}/confirm
```

```json
{
  "lines": [
    {
      "id": 1001,
      "unitPrice": 6000000,
      "costPrice": 4500000,
      "vatAmount": 642857.14
    }
  ],
  "payments": [
    {
      "paymentMethodId": 2,
      "paymentAcceptancePointId": 15,
      "debitAccountId": 140,
      "amount": 6000000,
      "transactionNumber": "TX-10001"
    }
  ]
}
```

`payments` может быть `null`; тогда используются платежи, уже сохранённые в документе.

### 8.7. Payment response

```json
{
  "id": 801,
  "paymentMethodId": 2,
  "paymentMethodName": "Карта",
  "paymentAcceptancePointId": 15,
  "paymentAcceptancePointName": "POS-терминал кассы №1",
  "debitAccountId": 140,
  "debitAccountNumber": "5530",
  "debitAccountName": "Средства в платёжных системах",
  "amount": 6000000,
  "transactionNumber": "TX-10001"
}
```

### 8.8. Автоматические операции при подтверждении продажи

| Код метода оплаты | Создаваемые операции |
|---|---|
| `CASH` | Операции точки не создаются; `paymentAcceptancePointId` должен быть `null`. |
| `CARD` | `IN + POSTED` на всю сумму и `OUT + PENDING` на ту же сумму. |
| `TRANSFER`, `CLICK`, `PAYME`, `MOBILE_PAYMENT`, `OTHER` | Только `IN + POSTED`. |

Обе операции `CARD` получают связь с продажей через `relatedDocumentId`. `IN` показывает поступление денег в точку. `OUT/PENDING` показывает сумму, которую банк или платёжный сервис ещё должен перечислить дальше.

Backend запрещает повторное создание операций для одной продажи. При отмене продажи связанные `POSTED` движения сторнируются, а `PENDING` помечаются `CANCELLED`.

---

## 9. Контрагенты — нейтральная карточка

Контрагент больше не классифицируется заранее как клиент или поставщик. Одна карточка может использоваться в покупке, продаже и других документах по выбору пользователя.

### 9.1. Удалённые поля

Из POST/PUT request и GET response удалены:

```text
counterpartyTypeId
counterpartyTypeName
isCustomer
isSupplier
```

Из query `GET /api/counterparty-cards` удалён `counterpartyTypeId`.

### 9.2. Затронутые маршруты

| Метод | URL | Изменение |
|---|---|---|
| `GET` | `/api/counterparty-cards` | Удалён фильтр и role-поля response. |
| `GET` | `/api/counterparty-cards/{id}` | Удалены role-поля response. |
| `POST` | `/api/counterparty-cards` | Удалены role-поля request. |
| `POST` | `/api/counterparty-cards/many` | Удалены role-поля каждого элемента. |
| `PUT` | `/api/counterparty-cards/{id}` | Удалены role-поля request. |

### 9.3. POST request

```json
{
  "code": "CP-001",
  "isVatPayer": true,
  "shortName": "ООО Контрагент",
  "fullName": "Общество с ограниченной ответственностью Контрагент",
  "inn": "309123456",
  "phoneNumber": "+998901234567",
  "email": "info@example.uz",
  "regionId": 1,
  "districtId": 10,
  "address": "Ташкент",
  "oked": "46900",
  "externalId": "EXT-100"
}
```

### 9.4. PUT request

```json
{
  "code": "CP-001",
  "isVatPayer": true,
  "shortName": "ООО Контрагент",
  "fullName": "Общество с ограниченной ответственностью Контрагент",
  "inn": "309123456",
  "phoneNumber": "+998901234567",
  "email": "info@example.uz",
  "regionId": 1,
  "districtId": 10,
  "address": "Ташкент",
  "oked": "46900",
  "externalId": "EXT-100",
  "stateId": 1
}
```

Bulk request:

```json
{
  "counterparties": [
    {
      "isVatPayer": false,
      "shortName": "Контрагент 1",
      "inn": "309123456"
    }
  ]
}
```

| Поле request | Тип | Nullable | Назначение |
|---|---|---|---|
| `code` | `string` | Да | Внутренний код. |
| `isVatPayer` | `boolean` | Нет | Является ли плательщиком НДС. Это налоговый признак, не роль. |
| `shortName` | `string(250)` | Нет | Короткое название. |
| `fullName` | `string(500)` | Да | Полное название. |
| `inn` | `string(20)` | Да | ИНН. |
| `phoneNumber` | `string(50)` | Да | Телефон. |
| `email` | `string(200)` | Да | Email. |
| `regionId`, `districtId` | `number` | Да | Регион и район. |
| `address` | `string(500)` | Да | Адрес. |
| `oked` | `string` | Да | Код ОКЭД. |
| `externalId` | `string` | Да | ID во внешней системе. |
| `stateId` | `number` | Нет | Только для `PUT`. |

Создание возвращает:

```json
{
  "id": 25,
  "inn": "309123456",
  "shortName": "ООО Контрагент"
}
```

Bulk-создание возвращает массив таких объектов.

### 9.5. GET response

```json
{
  "id": 25,
  "organizationId": 2,
  "organizationName": "ARTEL",
  "code": "CP-001",
  "isVatPayer": true,
  "shortName": "ООО Контрагент",
  "fullName": "Общество с ограниченной ответственностью Контрагент",
  "inn": "309123456",
  "phoneNumber": "+998901234567",
  "email": "info@example.uz",
  "regionId": 1,
  "regionName": "Ташкент",
  "districtId": 10,
  "districtName": "Мирабадский район",
  "address": "Ташкент",
  "stateId": 1,
  "stateName": "Активный",
  "oked": "46900",
  "externalId": "EXT-100",
  "createdDate": "2026-08-28T12:00:00"
}
```

Список принимает только `search`, `page`, `pageSize` и возвращает эти поля внутри стандартной пагинации.

Для select следует использовать единый справочник:

```http
GET /api/manuals/counterparties
```

---

## 10. Удалённые API

Frontend не должен вызывать следующие маршруты.

### 10.1. Удалён CRUD банковских терминалов

| Удалённый маршрут | Замена |
|---|---|
| `GET /api/bank-terminals` | `GET /api/payment-acceptance-points` |
| `GET /api/bank-terminals/{id}` | `GET /api/payment-acceptance-points/{id}` |
| `POST /api/bank-terminals` | `POST /api/payment-acceptance-points` |
| `PUT /api/bank-terminals/{id}` | `PUT /api/payment-acceptance-points/{id}` |
| `DELETE /api/bank-terminals/{id}` | `DELETE /api/payment-acceptance-points/{id}` |
| `GET /api/manuals/bank-terminals` | `GET /api/manuals/payment-acceptance-points` |

При миграции формы:

```text
externalTerminalId -> externalId
bankTerminalId     -> paymentAcceptancePointId
```

Дополнительно теперь обязателен `typeId`; `code` создаётся backend.

### 10.2. Удалены role-based справочники контрагентов

| Удалённый маршрут | Замена |
|---|---|
| `GET /api/manuals/counterparty-types` | Не требуется. |
| `GET /api/manuals/suppliers` | `GET /api/manuals/counterparties` |
| `GET /api/manuals/clients` | `GET /api/manuals/counterparties` |

---

## 11. Основные ошибки, которые должен обработать frontend

Ошибки возвращаются стандартным `ProblemDetails`. Для бизнес-ошибок обычно используется `422 Unprocessable Entity`.

| Код/ситуация | Что показать пользователю |
|---|---|
| `CashCollection.NotFound` | Документ инкассации не найден. |
| `CashCollection.InvalidStatus` | Действие недоступно в текущем статусе. |
| `CashCollection.InsufficientBalance` | В кассе недостаточно денег. |
| `CashCollection.AlreadyLinked` | Инкассация уже связана с банковской операцией. |
| `CashCollection.BankAccountMismatch` | Выбран другой банковский счёт. |
| `CashCollection.CurrencyMismatch` | Валюта не совпадает. |
| `CashCollection.AmountMismatch` | Сумма не совпадает с инкассацией. |
| `BankOperation.RelatedDocumentNotFound` | Связанный общий документ не найден. |
| `BankOperation.RelatedDocumentOrganizationMismatch` | Документ принадлежит другой организации. |
| `BankOperation.RelatedDocumentInactive` | Связанный документ неактивен. |
| Недостаточный остаток точки | Нельзя провести расход больше остатка электронных денег. |
| `RetailSaleDoc.InvalidPayment` | Некорректна комбинация метода оплаты и точки приёма. |
| Операции продажи уже существуют | Повторное подтверждение не должно создавать дубликаты. |

Пример:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "CashCollection.AmountMismatch",
  "status": 422,
  "detail": "Amount does not match cash collection document 310.",
  "traceId": "00-..."
}
```

---

## 12. Чек-лист миграции frontend

1. Удалить все вызовы `/api/bank-terminals` и `/api/manuals/bank-terminals`.
2. Перейти на `/api/payment-acceptance-points` и новые manuals.
3. Во всех retail request/response заменить `bankTerminalId` на `paymentAcceptancePointId`.
4. Не передавать точку для `CASH`; обязательно передавать её для non-cash.
5. Добавить экран/список `/api/payment-acceptance-point-operations` и отдельно показывать `PENDING` — это деньги, ещё не перечисленные банком/сервисом.
6. Для связи банковской операции сначала получить общий документ и передать его `id` как `relatedDocumentId`.
7. Для инкассации удобнее брать `documentRegistryId` из `/api/cash-collection-docs/in-transit`.
8. Удалить `counterpartyTypeId`, `isCustomer`, `isSupplier` из форм, моделей и фильтров.
9. Заменить supplier/client selects единым `/api/manuals/counterparties`.

## 13. Изменения без отдельного frontend API

Старый регистр `inv_reg_balance` удалён из backend и SQL. Публичные frontend-маршруты для него не добавлялись и не изменялись; действий на frontend не требуется.
