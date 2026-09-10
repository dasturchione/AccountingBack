# Аренда: изменения API для frontend от 04.09.2026

Этот документ описывает только то, что frontend должен изменить после обновления модуля аренды.

## 1. Что обязательно изменить

### Удалить старые поля

Frontend больше не должен отправлять или ожидать:

```text
lessorFullName
lessorInn
lessorPinfl
objects[].periodValue
objects[].contractAmount          // только в POST и PUT договора
generate-due.asOfDate
```

### Использовать новые поля

```text
isFreeOfCharge
lessors[]
objects[].periodAmount
objects[].totalArea
objects[].rentedArea
objects[].utilities[]
confirmationDate
terminationDate
generate-due.year
generate-due.month
```

Главные изменения:

- договор теперь может иметь несколько арендодателей;
- арендодателем может быть физическое или юридическое лицо;
- добавлена бесплатная аренда;
- дата окончания договора и объекта может быть `null`;
- сумма за период передаётся в `periodAmount`;
- общая сумма договора рассчитывается backend и возвращается только в GET;
- начисление формируется за выбранный календарный месяц;
- неполный месяц рассчитывается пропорционально календарным дням;
- добавлены коммунальные услуги и указание плательщика;
- добавлены даты подтверждения и расторжения договора.

## 2. Справочники для формы

### Типы объектов аренды

```http
GET /api/manuals/rental-object-types
```

### Коммунальные услуги

```http
GET /api/manuals/utility-services
```

Одинаковый формат response:

```json
[
  {
    "id": 1,
    "name": "Природный газ",
    "code": "NATURAL_GAS"
  }
]
```

Коды коммунальных услуг:

| `code` | Значение |
|---|---|
| `NATURAL_GAS` | Природный газ |
| `HOT_WATER` | Горячая вода |
| `COLD_WATER` | Холодная вода |
| `ELECTRICITY` | Электроэнергия |

Frontend должен передавать полученный `id`, а не хранить ID справочника в коде.

## 3. Создание договора

```http
POST /api/rental-contracts
```

Пример платного договора:

```json
{
  "isFreeOfCharge": false,
  "contractNumber": "IJ-25/2026",
  "contractDate": "2026-09-04",
  "startDate": "2026-09-04",
  "endDate": null,
  "currencyId": 1,
  "lessorPayableAccountId": 210,
  "taxPayableAccountId": 220,
  "comment": "Аренда офиса",
  "lessors": [
    {
      "lessorKindCode": "INDIVIDUAL",
      "fullName": "Ali Valiyev",
      "inn": "301111111",
      "pinfl": "12345678901234",
      "phoneNumber": "+998901234567",
      "registeredAddress": "Адрес регистрации",
      "residentialAddress": "Адрес проживания"
    }
  ],
  "objects": [
    {
      "id": null,
      "rentalObjectTypeId": 1,
      "objectName": "Офис",
      "objectIdentifier": "21:09:40:02:01:0571/0003",
      "objectAddress": "Адрес объекта",
      "totalArea": 48.22,
      "rentedArea": 20,
      "startDate": "2026-09-04",
      "endDate": null,
      "periodUnit": "MONTH",
      "periodAmount": 470000,
      "taxBaseAmount": 470000,
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

Успешный response — ID созданного договора:

```json
25
```

### Поля договора

| Поле | Обязательность | Назначение |
|---|---:|---|
| `isFreeOfCharge` | Да | `true` — бесплатная аренда |
| `contractNumber` | Да | Номер договора, введённый пользователем |
| `contractDate` | Да | Дата заключения договора |
| `startDate` | Да | Начало действия договора |
| `endDate` | Нет | Конец действия. `null` — бессрочный договор |
| `currencyId` | Да | Валюта договора |
| `lessorPayableAccountId` | Условно | Кредитовый счёт задолженности арендодателю |
| `taxPayableAccountId` | Условно | Кредитовый счёт задолженности по налогу |
| `comment` | Нет | Комментарий, максимум 1000 символов |
| `lessors` | Да | Минимум один арендодатель |
| `objects` | Да | Минимум один объект аренды |

Для сохранения `DRAFT` бухгалтерские счета могут быть `null`. Для активации платного договора все счета должны быть заполнены.

## 4. Арендодатели

### Поля `lessors[]`

| Поле | Обязательность | Назначение |
|---|---:|---|
| `lessorKindCode` | Да | `INDIVIDUAL` или `LEGAL_ENTITY` |
| `fullName` | Да | Ф.И.О. физлица или название юрлица |
| `inn` | Условно | ИНН |
| `pinfl` | Условно | PINFL физлица |
| `phoneNumber` | Нет | Телефон |
| `registeredAddress` | Нет | Адрес регистрации |
| `residentialAddress` | Нет | Адрес проживания |

Правила:

- должен быть заполнен хотя бы один из `inn` или `pinfl`;
- для `LEGAL_ENTITY` поле `inn` обязательно;
- одинаковые ИНН или PINFL нельзя повторять в одном request;
- frontend не передаёт `lessorId` или `counterpartyId`;
- backend сам находит арендодателя по PINFL, затем по ИНН;
- если арендодатель не найден, backend создаёт его;
- для юрлица backend также сам находит или создаёт контрагента.

Отдельного CRUD API для арендодателей нет.

## 5. Объекты аренды

### Поля `objects[]`

| Поле | Обязательность | Назначение |
|---|---:|---|
| `id` | Для update | ID существующего объекта; для нового объекта `null` |
| `rentalObjectTypeId` | Да | ID из `/api/manuals/rental-object-types` |
| `objectName` | Да | Название объекта |
| `objectIdentifier` | Нет | Кадастровый или другой внешний номер |
| `objectAddress` | Нет | Адрес объекта |
| `totalArea` | Нет | Общая площадь |
| `rentedArea` | Нет | Арендуемая площадь |
| `startDate` | Да | Начало аренды объекта |
| `endDate` | Нет | Конец аренды объекта; `null` — без конечной даты |
| `periodUnit` | Да | `MONTH` или `DAY` |
| `periodAmount` | Да | Сумма за один месяц или один день |
| `taxBaseAmount` | Да | Налоговая база за такой же период |
| `taxRate` | Да | Ставка в процентах: для 12% передаётся `12` |
| `expenseAccountId` | Условно | Дебетовый счёт расходов |
| `utilities` | Нет | Коммунальные услуги объекта |

Ограничения:

- `rentedArea` не может быть больше `totalArea`;
- дата начала объекта не может быть раньше даты начала договора;
- при заполненной дате окончания договора объект не может выходить за её пределы;
- `taxBaseAmount` не может быть меньше `periodAmount`;
- один вид коммунальной услуги нельзя повторять дважды в одном объекте.

### Значение `periodAmount`

```text
periodUnit = MONTH → periodAmount — сумма за один календарный месяц
periodUnit = DAY   → periodAmount — сумма за один день
```

Frontend не рассчитывает и не отправляет общую сумму договора.

### Коммунальные услуги

`payerCode` принимает:

```text
LESSOR — платит арендодатель
LESSEE — платит текущая организация-арендатор
```

Коммунальные услуги пока не участвуют в расчёте аренды и проводках. Они хранят условия договора для отображения пользователю.

## 6. Бесплатная аренда

Для бесплатной аренды:

```json
{
  "isFreeOfCharge": true,
  "lessorPayableAccountId": null,
  "taxPayableAccountId": null,
  "objects": [
    {
      "periodAmount": 0,
      "taxBaseAmount": 0,
      "taxRate": 0,
      "expenseAccountId": null
    }
  ]
}
```

По бесплатному договору можно выполнить активацию, но документы начисления и задолженность не создаются.

## 7. Изменение договора

```http
PUT /api/rental-contracts/{id}
```

Request совпадает с POST. Изменять можно только договор в статусе `DRAFT`.

Важно: frontend должен отправлять полные актуальные массивы:

- `lessors`;
- `objects`;
- `objects[].utilities`.

Если существующий элемент не передать в PUT, связь или объект будет удалён/деактивирован.

Успешный response:

```http
204 No Content
```

## 8. Получение договоров

### Список

```http
GET /api/rental-contracts?statusId=1&dateFrom=2026-01-01&dateTo=2026-12-31&search=Ali&page=1&pageSize=50
```

Фильтры:

| Параметр | Назначение |
|---|---|
| `statusId` | Статус договора |
| `dateFrom`, `dateTo` | Договоры, действующие в выбранном интервале |
| `search` | Номер договора, имя/название, ИНН или PINFL любого арендодателя |
| `page`, `pageSize` | Пагинация |

Общий формат списка:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalCount": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

В каждом договоре вместо старых полей одного арендодателя возвращается `lessors[]`.

### Один договор

```http
GET /api/rental-contracts/{id}
```

Новые важные поля response:

```json
{
  "id": 25,
  "isFreeOfCharge": false,
  "endDate": null,
  "confirmationDate": "2026-09-04T00:00:00",
  "terminationDate": null,
  "lessors": [
    {
      "id": 10,
      "lessorKindCode": "INDIVIDUAL",
      "counterpartyId": null,
      "fullName": "Ali Valiyev",
      "inn": "301111111",
      "pinfl": "12345678901234",
      "phoneNumber": "+998901234567",
      "registeredAddress": "Адрес регистрации",
      "residentialAddress": "Адрес проживания"
    }
  ],
  "objects": [
    {
      "id": 41,
      "periodUnit": "MONTH",
      "periodAmount": 470000,
      "taxBaseAmount": 470000,
      "taxRate": 12,
      "contractAmount": null,
      "contractTaxBaseAmount": null,
      "contractTaxAmount": null,
      "utilities": []
    }
  ]
}
```

Response-only поля объекта:

| Поле | Назначение |
|---|---|
| `contractAmount` | Общая аренда за весь известный срок |
| `contractTaxBaseAmount` | Общая налоговая база за весь срок |
| `contractTaxAmount` | Общий налог за весь срок |
| `nextAccrualDate` | Следующая дата, с которой ещё возможно начисление |

Если ни договор, ни объект, ни расторжение не задают конечную дату, итоговые суммы возвращаются как `null`.

## 9. Активация договора

```http
PUT /api/rental-contracts/{id}/activate?confirmationDate=2026-09-04
```

`confirmationDate` необязателен. Без параметра backend использует сегодняшнюю дату по времени Ташкента.

Ограничения:

- активировать можно только `DRAFT`;
- дата подтверждения не может быть раньше `contractDate`;
- дата подтверждения не может быть в будущем;
- для платной аренды должны быть заполнены оба счёта договора и `expenseAccountId` всех объектов.

Успешный response: `204 No Content`.

## 10. Расторжение договора

```http
PUT /api/rental-contracts/{id}/cancel?terminationDate=2026-12-15
```

Для активного договора `terminationDate` необязателен. Без параметра используется сегодняшний день.

После расторжения:

- ранее созданные и проведённые начисления остаются;
- начисления после `terminationDate` больше не создаются;
- за неполный последний месяц сумма рассчитывается до даты расторжения включительно;
- существующие документы начисления автоматически не отменяются.

Если отменяется черновик, `terminationDate` не записывается.

Успешный response: `204 No Content`.

## 11. Удаление договора

```http
DELETE /api/rental-contracts/{id}
```

Удалить можно только `DRAFT`, у которого нет документов начисления.

Успешный response: `204 No Content`.

## 12. Формирование начисления за месяц

```http
POST /api/rental-accrual-docs/generate-due
```

Новый request:

```json
{
  "year": 2024,
  "month": 7
}
```

Старое поле `asOfDate` удалено.

Response:

```json
{
  "createdDocumentCount": 1,
  "createdItemCount": 2,
  "documentIds": [31]
}
```

Backend:

- рассчитывает только указанный календарный месяц;
- создаёт один документ на один договор;
- создаёт отдельную позицию для каждого объекта;
- не создаёт повторную позицию объекта за тот же месяц;
- не обрабатывает бесплатные договоры;
- для расторгнутого договора может создать пропущенные начисления только до даты расторжения;
- создаёт документ в статусе `DRAFT`;
- устанавливает `docDate` на последний день выбранного месяца.

Повторный вызов безопасен: уже созданные объект-месяцы будут пропущены.

## 13. Сумма неполного месяца

Для `periodUnit = MONTH`:

```text
начисление = periodAmount × активные дни / дни в календарном месяце
```

Начальная и конечная даты включаются.

Пример: договор действует с `09.07.2024` по `09.10.2024`, месячная сумма — `470 000`.

| Месяц | Расчёт | Сумма |
|---|---|---:|
| Июль | `470 000 × 23 / 31` | `348 709,68` |
| Август | полный месяц | `470 000,00` |
| Сентябрь | полный месяц | `470 000,00` |
| Октябрь | `470 000 × 9 / 31` | `136 451,61` |
| Итого | | `1 425 161,29` |

Для `periodUnit = DAY`:

```text
начисление = periodAmount × количество активных дней
```

## 14. Налог и сумма задолженности

Для каждого начисленного периода backend рассчитывает:

```text
taxAmount = taxBaseAmount × taxRate / 100

withheldFromContract = contractAmount × taxRate / 100

payableAmount = contractAmount − withheldFromContract

amount = payableAmount + taxAmount
```

Пример:

```text
contractAmount = 5 000 000
taxBaseAmount  = 6 000 000
taxRate        = 12

taxAmount      =   720 000
payableAmount  = 4 400 000
amount         = 5 120 000
```

Значение полей в документе начисления:

| Поле | Значение |
|---|---|
| `contractAmount` | Начисленная аренда за период |
| `taxBaseAmount` | Налоговая база за период |
| `taxAmount` | Задолженность по налогу |
| `payableAmount` | Задолженность арендодателю |
| `amount` | Общая сумма обязательств организации |

## 15. Просмотр и проведение начислений

```http
GET /api/rental-accrual-docs
GET /api/rental-accrual-docs/{id}
PUT /api/rental-accrual-docs/{id}
PUT /api/rental-accrual-docs/{id}/post
PUT /api/rental-accrual-docs/{id}/cancel
DELETE /api/rental-accrual-docs/{id}
```

В списке и деталях начисления поля одного арендодателя заменены массивом:

```json
"lessors": [
  {
    "id": 10,
    "lessorKindCode": "INDIVIDUAL",
    "counterpartyId": null,
    "fullName": "Ali Valiyev",
    "inn": "301111111",
    "pinfl": "12345678901234",
    "phoneNumber": "+998901234567",
    "registeredAddress": "Адрес регистрации",
    "residentialAddress": "Адрес проживания"
  }
]
```

`generate-due` создаёт только `DRAFT`. Задолженность и бухгалтерские проводки появляются после:

```http
PUT /api/rental-accrual-docs/{id}/post
```

Проводки:

```text
Дт expenseAccountId / Кт lessorPayableAccountId = payableAmount
Дт expenseAccountId / Кт taxPayableAccountId    = taxAmount
```

## 16. Автоматическое начисление

Фоновая задача запускается первого числа каждого месяца в `00:00` по времени Ташкента и создаёт черновики за предыдущий месяц.

```text
01.08.2026 → создаются DRAFT-начисления за июль 2026
```

Автоматическое проведение не выполняется. Пользователь должен проверить документ и вызвать `/post`.

## 17. Что изменить в интерфейсе

1. Заменить три поля одного арендодателя на редактируемый массив `lessors`.
2. Добавить выбор `INDIVIDUAL`/`LEGAL_ENTITY`.
3. Добавить `isFreeOfCharge`.
4. Разрешить `endDate = null` для договора и объекта.
5. Удалить `periodValue`.
6. Заменить ввод общей суммы договора на `periodAmount` за месяц или день.
7. Показывать рассчитанные общие суммы только из GET response.
8. Добавить общую и арендуемую площадь.
9. Добавить коммунальные услуги и плательщика.
10. Добавить выбор даты подтверждения перед активацией.
11. Добавить выбор даты расторжения перед отменой активного договора.
12. В форме ручного начисления использовать выбор месяца и отправлять `year`/`month`.
13. В списках заменить старые поля арендодателя на `lessors[]`.
14. После генерации показывать созданные `documentIds`; для появления долга отдельно выполнять `/post`.

