# Fixed Assets (FA): инструкция для фронтенда

Документ описывает текущий backend-контракт FA. Фронтенд должен использовать именно эти маршруты и DTO.

## 1. Общие правила

Для всех запросов нужны:

- `Authorization: Bearer <token>`;
- `X-OrganizationId: <organizationId>`;
- `X-Language: ru|uz|en` для локализованных названий.

Дата передается в ISO-формате, например `2026-08-12T00:00:00`.

Состояния документов:

| `statusId` | Значение |
|---:|---|
| `1` | DRAFT — черновик |
| `2` | POSTED — проведен |
| `3` | CANCELLED — отменен |
| `4` | PENDING |

Состояния записи:

| `stateId` | Значение |
|---:|---|
| `1` | ACTIVE |
| `2` | PASSIVE |

Списочные методы документов возвращают пагинацию:

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

После `POST` документ обычно возвращает его `long id`. `PUT`, `DELETE`, `confirm` и `cancel` возвращают `204 No Content`.

## 2. Жизненный цикл основного средства

Основное средство нельзя создать отдельным `POST /api/fa-assets`.

1. Создать приход ОС через `POST /api/fa-receipts`.
2. Провести приход через `PUT /api/fa-receipts/{id}/confirm`.
3. При проведении прихода backend создаст `FaAsset` со статусом `NOT_COMMISSIONED` и заполнит его первоначальную стоимость и счет учета.
4. Создать документ ввода в эксплуатацию через `POST /api/fa-commissionings`, указав созданные `faAssetId`.
5. Провести ввод в эксплуатацию через `PUT /api/fa-commissionings/{id}/confirm`.
6. После этого ОС получает статус `ACTIVE` и участвует в амортизации, переоценке, перемещении и выбытии.

Статусы основного средства:

| `statusId` | Значение |
|---:|---|
| `1` | NOT_COMMISSIONED — не введено в эксплуатацию |
| `2` | ACTIVE — используется |
| `3` | CONSERVATION |
| `4` | DISPOSED — выбыло |

FA не использует `Product`, `ProductTable`, `productTableId`, `sourceProductTableId`, складские остатки или складские партии.

## 3. Справочники FA

Все методы возвращают список с локализованными `id` и `name`.

| Метод | Назначение |
|---|---|
| `GET /api/manuals/fa-groups` | Группы основных средств |
| `GET /api/manuals/fa-okofs` | Коды ОКОФ |
| `GET /api/manuals/fa-depreciation-methods` | Методы амортизации |
| `GET /api/manuals/fa-receipt-types` | Виды поступления ОС |
| `GET /api/manuals/fa-disposal-types` | Виды выбытия ОС |
| `GET /api/manuals/fa-assets` | ОС для выбора в других документах |

Для `GET /api/manuals/fa-assets` доступны query-параметры:

```text
faGroupId, statusId, search
```

Ответ элемента `fa-assets` дополнительно содержит `inventoryNumber`, `faGroupId`, `faGroupName`, `initialCost`, `assetAccountId`, `assetAccountNumber`, `assetAccountName`, `statusId`, `statusName`.

Виды поступления:

| `receiptTypeId` | Значение |
|---:|---|
| `1` | PURCHASE |
| `2` | CONSTRUCTION |
| `3` | OTHER |

Виды выбытия:

| `disposalTypeId` | Значение |
|---:|---|
| `1` | SALE |
| `2` | WRITEOFF |
| `3` | BREAKDOWN |

Методы амортизации выбираются из справочника, а не передаются строкой. Используемые коды: `LINEAR`, `DECLINING_BALANCE`, `UNITS_OF_PRODUCTION`.

## 4. Карточки основных средств — `/api/fa-assets`

### Получить список

```http
GET /api/fa-assets?faGroupId=&statusId=&search=&page=1&pageSize=20
```

### Получить карточку

```http
GET /api/fa-assets/{id}
```

Подробный ответ содержит инвентарный номер, название, группу, ОКОФ, стоимость, параметры амортизации, подразделение, ответственное лицо, статус и счета:

- `assetAccountId` — счет учета ОС;
- `accumulatedDepreciationAccountId` — счет накопленной амортизации;
- `depreciationExpenseAccountId` — счет расходов по амортизации.

### Изменить разрешенные поля

```http
PUT /api/fa-assets/{id}
```

```json
{
  "inventoryNumber": "INV-001",
  "name": "Станок",
  "faGroupId": 1,
  "okofId": 10
}
```

### Сделать карточку неактивной

```http
DELETE /api/fa-assets/{id}
```

Удаление физически не выполняется: запись переводится в `stateId = 2`.

В текущей версии отсутствуют `POST /api/fa-assets`, `PUT /api/fa-assets/{id}/confirm` и `PUT /api/fa-assets/{id}/cancel`.

## 5. Приход ОС — `/api/fa-receipts`

### Методы

```text
GET    /api/fa-receipts
GET    /api/fa-receipts/{id}
POST   /api/fa-receipts
PUT    /api/fa-receipts/{id}
PUT    /api/fa-receipts/{id}/confirm
PUT    /api/fa-receipts/{id}/cancel
DELETE /api/fa-receipts/{id}
```

Фильтры списка:

```text
counterpartyId, statusId, receiptTypeId, dateFrom, dateTo, search, page, pageSize
```

### Создание или изменение

```json
{
  "docDate": "2026-08-12T00:00:00",
  "counterpartyId": 15,
  "currencyId": 1,
  "receiptTypeId": 1,
  "supplierAccountId": 41,
  "lines": [
    {
      "name": "Станок",
      "quantity": 1,
      "price": 10000000,
      "vatRateId": 2,
      "capitalInvestmentAccountId": 36,
      "vatAccountId": 47,
      "assets": [
        {
          "inventoryNumber": "INV-001",
          "name": "Станок",
          "faGroupId": 1,
          "okofId": 10,
          "initialCost": 10000000,
          "assetAccountId": 41
        }
      ]
    }
  ]
}
```

Обязательное правило: `lines[].assets.length` должно быть равно `lines[].quantity`.

`confirm`:

- меняет документ в `POSTED`;
- создает связанные карточки ОС в `NOT_COMMISSIONED`;
- создает бухгалтерские проводки.

Проводки прихода:

```text
Дт capitalInvestmentAccountId  — Кт supplierAccountId — стоимость ОС
Дт vatAccountId                — Кт supplierAccountId — НДС
```

`cancel` отменяет документ и сторнирует проведенные записи. Изменять и удалять можно только черновик.

## 6. Ввод в эксплуатацию — `/api/fa-commissionings`

### Методы

```text
GET  /api/fa-commissionings
GET  /api/fa-commissionings/{id}
POST /api/fa-commissionings
PUT  /api/fa-commissionings/{id}
PUT  /api/fa-commissionings/{id}/confirm
PUT  /api/fa-commissionings/{id}/cancel
```

Фильтры списка:

```text
statusId, dateFrom, dateTo, search, page, pageSize
```

### Создание или изменение

```json
{
  "docDate": "2026-08-12T00:00:00",
  "note": "Ввод в эксплуатацию",
  "lines": [
    {
      "faAssetId": 1001,
      "deprStartDate": "2026-08-12T00:00:00",
      "salvageValue": 0,
      "usefulLifeMonths": 60,
      "depreciationMethodId": 1,
      "plannedUnitsTotal": null,
      "departmentId": 10,
      "responsibleUserId": 25,
      "accumulatedDepreciationAccountId": 42,
      "depreciationExpenseAccountId": 43,
      "note": null
    }
  ]
}
```

В строке указывается ОС в статусе `NOT_COMMISSIONED`. Она должна быть создана проведенным приходом. Одна ОС не может одновременно находиться в нескольких активных документах ввода.

`confirm`:

- меняет документ в `POSTED`;
- меняет статус ОС на `ACTIVE`;
- сохраняет параметры амортизации и счета;
- проводит `Дт assetAccountId — Кт capitalInvestmentAccountId`.

`assetAccountId` и `capitalInvestmentAccountId` берутся backend из карточки учета ОС и связанного проведенного прихода. Фронтенд их в строке ввода не отправляет.

## 7. Амортизация — `/api/fa/depreciation`

### Получить проведенные/созданные расчеты

```http
GET /api/fa/depreciation/run?statusId=&periodFrom=&periodTo=&search=&page=1&pageSize=20
GET /api/fa/depreciation/run/{id}
```

### Запустить расчет за месяц

```http
POST /api/fa/depreciation/run?period=2026-08
```

Тело запроса не нужно. `period` строго передается в формате `yyyy-MM`.

Запуск:

- проверяет открытость учетного периода;
- не допускает второй активный расчет за тот же месяц;
- рассчитывает амортизацию по активным ОС и их параметрам;
- сразу создает документ `POSTED` и бухгалтерские проводки.

Проводка по каждой строке:

```text
Дт expenseAccountId — Кт accumulatedDepreciationAccountId — сумма амортизации
```

### Отменить расчет

```http
PUT /api/fa/depreciation/run/{id}/cancel
```

Отмена создает сторнирующие проводки.

## 8. Переоценка — `/api/fa/revaluations`

### Методы

```text
GET  /api/fa/revaluations
GET  /api/fa/revaluations/{id}
POST /api/fa/revaluations
PUT  /api/fa/revaluations/{id}
PUT  /api/fa/revaluations/{id}/confirm
PUT  /api/fa/revaluations/{id}/cancel
```

Фильтры списка:

```text
statusId, dateFrom, dateTo, search, page, pageSize
```

### Тело создания или изменения

```json
{
  "revaluationDate": "2026-08-12T00:00:00",
  "reason": "Переоценка",
  "stateId": 1,
  "revaluationReserveAccountId": 50,
  "revaluationLossAccountId": 51,
  "lines": [
    {
      "faAssetId": 1001,
      "newValue": 12000000,
      "note": null
    }
  ]
}
```

Backend сохраняет старую и новую стоимость в строке. Счета ОС и накопленной амортизации берутся из учета ОС.

При увеличении стоимости:

```text
Дт assetAccountId — Кт revaluationReserveAccountId
```

При уменьшении стоимости:

```text
Дт revaluationLossAccountId — Кт assetAccountId
```

Проводки создаются только при `confirm`. `cancel` выполняет сторно.

## 9. Выбытие ОС — `/api/fa/disposals`

### Методы

```text
GET  /api/fa/disposals
GET  /api/fa/disposals/{id}
POST /api/fa/disposals
PUT  /api/fa/disposals/{id}
PUT  /api/fa/disposals/{id}/confirm
PUT  /api/fa/disposals/{id}/cancel
```

Фильтры списка:

```text
statusId, disposalTypeId, dateFrom, dateTo, search, page, pageSize
```

### Тело создания или изменения

```json
{
  "disposalDate": "2026-08-12T00:00:00",
  "disposalTypeId": 1,
  "reason": "Продажа",
  "stateId": 1,
  "disposalAccountId": 60,
  "customerAccountId": 61,
  "vatAccountId": 62,
  "gainAccountId": 63,
  "lossAccountId": 64,
  "lines": [
    {
      "faAssetId": 1001,
      "saleAmount": 13000000,
      "note": null
    }
  ]
}
```

Для типа `SALE` хотя бы одна строка должна иметь `saleAmount > 0`.

При проведении backend сам фиксирует счета ОС и накопленной амортизации в строках. Формируются проводки списания первоначальной стоимости, накопленной амортизации, продажи и прибыли/убытка. ОС получает статус `DISPOSED`.

`vatAccountId` сохраняется в документе и возвращается в DTO. В текущем posting builder отдельная НДС-проводка по этому счету не формируется.

## 10. Перемещение ОС — `/api/fa-movements`

### Методы

```text
GET  /api/fa-movements
GET  /api/fa-movements/{id}
POST /api/fa-movements
PUT  /api/fa-movements/{id}
PUT  /api/fa-movements/{id}/confirm
PUT  /api/fa-movements/{id}/cancel
```

Фильтры списка:

```text
statusId, departmentId, responsibleUserId, dateFrom, dateTo, search, page, pageSize
```

### Тело создания или изменения

```json
{
  "docDate": "2026-08-12T00:00:00",
  "toDepartmentId": 20,
  "toResponsibleUserId": 30,
  "note": "Перемещение",
  "lines": [
    {
      "faAssetId": 1001,
      "note": null
    }
  ]
}
```

Должен быть указан хотя бы один адресат: `toDepartmentId` или `toResponsibleUserId`.

Перемещение меняет подразделение и ответственного у ОС. Бухгалтерские проводки и `PostingBatch` для него не создаются. При отмене старые значения подразделения и ответственного восстанавливаются.

## 11. Счета и проводки

Фронтенд передает `chartAccount.id`, а не номер счета. Список счетов можно получить через общий manual API `GET /api/manuals/chart-accounts`.

| Документ | Счета, которые выбирает фронтенд |
|---|---|
| Приход | `supplierAccountId`, `capitalInvestmentAccountId`, `vatAccountId`, `assetAccountId` |
| Ввод в эксплуатацию | `accumulatedDepreciationAccountId`, `depreciationExpenseAccountId` |
| Переоценка | `revaluationReserveAccountId`, `revaluationLossAccountId` |
| Выбытие | `disposalAccountId`, `customerAccountId`, `vatAccountId`, `gainAccountId`, `lossAccountId` |
| Амортизация | счета уже сохранены в учете ОС; в запросе счета не передаются |
| Перемещение | счета не нужны |

Проводки не создаются при обычном `POST` или `PUT` документа. Для приходов, ввода, переоценки и выбытия нужен `confirm`; амортизация проводится сразу при `POST /run`.

## 12. Что изменено относительно прежней версии

- Убрано самостоятельное создание `FaAsset` через API.
- Убраны `confirm` и `cancel` у `FaAsset`.
- Убрана связь FA с `productTable`, `Product`, складом и складскими остатками.
- Убрана возможность создавать ОС из товара на складе.
- Добавлен отдельный документ `FaCommissioning` для ввода ОС в эксплуатацию.
- Строковые поля вида поступления и выбытия заменены на `receiptTypeId` и `disposalTypeId`.
- Добавлены manual API для групп FA, ОКОФ, методов амортизации, видов поступления и видов выбытия.
- Добавлен manual API выбора ОС для документов.
- Счета учета ОС хранятся в `FaAssetAccounting`; часть счетов фиксируется в строках или заголовках документов для воспроизводимости проводок.
- `FaMovement` остался операционным документом без бухгалтерской проводки.

Перед подключением фронтенда в базе должны существовать справочники FA, активные счета организации и тип документа `FA_COMMISSIONING` с переводами. Для него используется `documentTypeId = 17`.
