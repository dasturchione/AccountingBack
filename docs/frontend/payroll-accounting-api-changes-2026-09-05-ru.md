# Изменения API зарплаты и бухгалтерских счетов

Дата документа: 05.09.2026

## Что изменилось

Счета для начисления и выплаты зарплаты теперь выбирает пользователь через frontend.

`acc_document_account_setting` используется только как справочник рекомендуемых счетов. Backend больше не берет из него обязательные счета автоматически. Пользователь может выбрать другой активный счет из `chartAccount` текущей организации.

При расчете зарплаты backend:

1. принимает выбранные счета;
2. проверяет, что они существуют и принадлежат текущей организации;
3. сохраняет счета в документе зарплаты;
4. определяет фактическую пару дебет/кредит для каждой строки расчета;
5. сохраняет эту пару в строке;
6. при подтверждении создает проводки именно по сохраненным счетам.

Новых маршрутов не добавлено. Изменены существующие API документов зарплаты, компонентов и выплат.

## Получение рекомендуемых счетов

Тип настройки для начисления зарплаты:

```text
documentTypeId = 9
documentTypeCode = payroll_accrual
```

Получить все роли и настроенные для них счета:

```http
GET /api/document-account-settings/9
```

Получить короткий список счетов для одной роли:

```http
GET /api/document-account-settings/9/chart-accounts?documentRoleCode=salary_expense
```

Ответ:

```json
[
  {
    "id": 145,
    "name": "Административные расходы",
    "code": "9420",
    "number": "9420"
  }
]
```

`id` из ответа передается в account-поле документа. Номер счета `9420`, `6710` и другие номера нельзя передавать вместо `id`, если они не совпадают с внутренним ID записи.

Доступные коды ролей:

| Код роли | Для чего используется |
| --- | --- |
| `salary_expense` | Расход по начисленной зарплате |
| `salary_payable` | Задолженность перед сотрудниками |
| `deduction_payable` | Задолженность по НДФЛ и другим удержаниям |
| `employer_tax_expense` | Расход организации по налогам работодателя |
| `employer_tax_payable` | Задолженность организации по налогам работодателя |
| `advance_receivable` | Ранее выплаченный аванс сотруднику |

### Какой role code использовать для каждого account-поля

Backend не читает `acc_document_account_setting` при расчете автоматически. Следующие запросы выполняет frontend для заполнения выпадающих списков, после чего выбранный `id` отправляется в соответствующем поле.

| Поле API | `documentRoleCode` | Запрос рекомендуемых счетов |
| --- | --- | --- |
| `salaryExpenseAccountId` | `salary_expense` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=salary_expense` |
| `salaryPayableAccountId` | `salary_payable` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=salary_payable` |
| `deductionPayableAccountId` | `deduction_payable` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=deduction_payable` |
| `employerTaxExpenseAccountId` | `employer_tax_expense` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=employer_tax_expense` |
| `employerTaxPayableAccountId` | `employer_tax_payable` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=employer_tax_payable` |
| `advanceReceivableAccountId` | `advance_receivable` | `GET /api/document-account-settings/9/chart-accounts?documentRoleCode=advance_receivable` |

Дополнительные account-поля:

| Поле | Откуда выбирать |
| --- | --- |
| `PayrollEmployment.expenseAccountId` | роль `salary_expense`; это индивидуальный счет расходов места работы сотрудника |
| `PayrollComponent.expenseAccountId` для `EARNING` | роль `salary_expense` |
| `PayrollComponent.liabilityAccountId` для `EARNING` | роль `salary_payable` |
| `PayrollComponent.liabilityAccountId` для `DEDUCTION` | роль `deduction_payable` |
| `PayrollComponent.expenseAccountId` для `EMPLOYER_TAX` | роль `employer_tax_expense` |
| `PayrollComponent.liabilityAccountId` для `EMPLOYER_TAX` | роль `employer_tax_payable` |
| `PayrollComponent.expenseAccountId` для `RECLASSIFICATION` | отдельной роли нет; выбрать дебетовый счет из полного плана счетов |
| `PayrollComponent.liabilityAccountId` для `RECLASSIFICATION` | отдельной роли нет; выбрать кредитовый счет из полного плана счетов |
| `PayrollPayment.sourceChartAccountId` | не из `payroll_accrual`; выбрать бухгалтерский счет выбранного банка или кассы |
| `PayrollPayment.offsetAccountId`, если `paymentKind = FINAL` | роль `salary_payable` |
| `PayrollPayment.offsetAccountId`, если `paymentKind = ADVANCE` | роль `advance_receivable` |

Поля `PayrollCalcLine.debitAccountId` и `PayrollCalcLine.creditAccountId` frontend не выбирает. Их рассчитывает и возвращает backend как итоговую сохраненную пару проводки.

Если для роли ничего не настроено, frontend может получить полный план счетов через:

```http
GET /api/manuals/chart-accounts
```

## Расчет зарплаты

### POST `/api/payroll/documents/calculate`

В request добавлены шесть обязательных полей:

```json
{
  "periodId": 12,
  "docDate": "2026-09-30T00:00:00",
  "documentKind": "REGULAR",
  "correctionOfDocId": null,
  "salaryExpenseAccountId": 145,
  "salaryPayableAccountId": 201,
  "deductionPayableAccountId": 235,
  "employerTaxExpenseAccountId": 145,
  "employerTaxPayableAccountId": 241,
  "advanceReceivableAccountId": 178,
  "note": "Зарплата за сентябрь 2026",
  "adjustments": []
}
```

| Поле | Обязательное | Назначение | Типичный счет |
| --- | --- | --- | --- |
| `salaryExpenseAccountId` | да | Счет расходов по зарплате по умолчанию | `9420`, `9410`, `2010`, `2510` |
| `salaryPayableAccountId` | да | Расчеты с сотрудниками по зарплате | `6710` |
| `deductionPayableAccountId` | да | Счет удержаний, если отдельный счет не задан в компоненте | например `6420.1` |
| `employerTaxExpenseAccountId` | да | Расход организации по социальному налогу и аналогичным начислениям | например `9420` |
| `employerTaxPayableAccountId` | да | Обязательство по налогу работодателя | например `6510.1` |
| `advanceReceivableAccountId` | да | Выданные сотрудникам авансы | счет авансов сотрудникам |

Все значения — внутренние `acc_chart_account.id`.

Успешный ответ содержит ID созданного документа:

```json
125
```

### Как backend выбирает фактические счета

| Тип компонента | Дебет | Кредит |
| --- | --- | --- |
| `EARNING` | `component.expenseAccountId`, затем счет места работы сотрудника, затем `salaryExpenseAccountId` документа | `component.liabilityAccountId`, затем `salaryPayableAccountId` документа |
| `DEDUCTION` | `salaryPayableAccountId` документа | `component.liabilityAccountId`, затем `deductionPayableAccountId` документа |
| `EMPLOYER_TAX` | `component.expenseAccountId`, затем `employerTaxExpenseAccountId` документа | `component.liabilityAccountId`, затем `employerTaxPayableAccountId` документа |
| `RECLASSIFICATION` | `component.expenseAccountId` | `component.liabilityAccountId` |

Выбранная фактическая пара сохраняется в `debitAccountId` и `creditAccountId` строки расчета. Последующее изменение компонента или справочника рекомендуемых счетов не меняет проводку уже рассчитанного документа.

При зачете аванса создается отдельная пара:

```text
Дебет  = salaryPayableAccountId
Кредит = advanceReceivableAccountId
```

## Получение документа зарплаты

### GET `/api/payroll/documents/{id}`

В response добавлены счета документа и фактические счета каждой расчетной строки.

Фрагмент ответа:

```json
{
  "id": 125,
  "docNumber": "9",
  "docDate": "2026-09-30T00:00:00",
  "salaryExpenseAccountId": 145,
  "salaryPayableAccountId": 201,
  "deductionPayableAccountId": 235,
  "employerTaxExpenseAccountId": 145,
  "employerTaxPayableAccountId": 241,
  "advanceReceivableAccountId": 178,
  "grossAmount": 2000000,
  "deductionAmount": 240000,
  "employerTaxAmount": 240000,
  "netAmount": 1760000,
  "payableAmount": 1760000,
  "lines": [
    {
      "employeeId": 7,
      "grossAmount": 2000000,
      "deductionAmount": 240000,
      "employerTaxAmount": 240000,
      "netAmount": 1760000,
      "payableAmount": 1760000,
      "calcLines": [
        {
          "componentId": 1,
          "componentCode": "SALARY",
          "componentType": "EARNING",
          "amount": 2000000,
          "debitAccountId": 145,
          "creditAccountId": 201
        },
        {
          "componentId": 2,
          "componentCode": "PIT",
          "componentType": "DEDUCTION",
          "amount": 240000,
          "debitAccountId": 201,
          "creditAccountId": 235
        }
      ]
    }
  ]
}
```

`debitAccountId` и `creditAccountId` предназначены для просмотра сформированной бухгалтерской пары. Frontend не должен рассчитывать их самостоятельно.

## Новый тип компонента RECLASSIFICATION

API компонентов не изменил маршруты:

```http
POST /api/payroll/components
PUT  /api/payroll/components/{id}
GET  /api/payroll/components/{id}
```

В `componentType` теперь разрешено значение:

```text
RECLASSIFICATION
```

Оно используется, когда сумма должна создать проводку, но не должна уменьшать зарплату к выплате. Например обязательный индивидуальный пенсионный взнос `0,1%`:

```text
Дебет  6420.1
Кредит 6530.2
```

Пример создания компонента:

```json
{
  "code": "INPS_MANDATORY",
  "name": "Обязательный индивидуальный пенсионный взнос",
  "componentType": "RECLASSIFICATION",
  "calculationMethod": "PERCENT_OF_GROSS",
  "defaultAmount": null,
  "defaultRate": 0.1,
  "isMandatory": true,
  "expenseAccountId": 235,
  "liabilityAccountId": 246,
  "effectiveFrom": "2026-01-01",
  "effectiveTo": null,
  "sortOrder": 30
}
```

Для `RECLASSIFICATION`:

- `expenseAccountId` обязателен и используется как дебет;
- `liabilityAccountId` обязателен и используется как кредит;
- сумма входит в проводки;
- сумма не входит в `deductionAmount`;
- сумма не уменьшает `netAmount` и `payableAmount`.

Текущая формула:

```text
netAmount = grossAmount - deductionAmount
payableAmount = netAmount - advanceAmount
```

В `deductionAmount` входят только компоненты типа `DEDUCTION`.

## Выплата зарплаты

### POST `/api/payroll/payments`

В request добавлено обязательное поле `offsetAccountId`.

Пример окончательной выплаты через банк:

```json
{
  "periodId": 12,
  "payrollDocId": 125,
  "docDate": "2026-09-30T15:00:00",
  "paymentKind": "FINAL",
  "sourceType": "BANK",
  "bankAccountId": 4,
  "cashBoxId": null,
  "sourceChartAccountId": 310,
  "offsetAccountId": 201,
  "currencyId": 1,
  "note": "Выплата зарплаты",
  "lines": [
    {
      "employeeId": 7,
      "amount": 1760000,
      "note": null
    }
  ]
}
```

| Поле | Назначение |
| --- | --- |
| `sourceChartAccountId` | Счет источника денег: банковский или кассовый счет организации |
| `offsetAccountId` | Корреспондирующий счет выплаты |

Рекомендуемое значение `offsetAccountId`:

| `paymentKind` | Значение |
| --- | --- |
| `FINAL` | счет задолженности по зарплате, обычно тот же ID, который использован как `salaryPayableAccountId` |
| `ADVANCE` | счет выданных авансов, обычно тот же ID, который используется как `advanceReceivableAccountId` |

Для `sourceType = BANK` передается `bankAccountId`, а `cashBoxId` должен быть `null`.

Для `sourceType = CASH` передается `cashBoxId`, а `bankAccountId` должен быть `null`.

Успешный ответ:

```json
48
```

При подтверждении выплаты backend использует сохраненный `offsetAccountId`:

```http
PUT /api/payroll/payments/{id}/confirm
```

Для окончательной выплаты через банк проводка будет иметь общий смысл:

```text
Дебет  offsetAccountId          — задолженность перед сотрудником
Кредит sourceChartAccountId     — банковский счет организации
```

### GET `/api/payroll/payments/{id}`

В детальном ответе возвращается сохраненное поле:

```json
{
  "id": 48,
  "paymentKind": "FINAL",
  "sourceType": "BANK",
  "sourceChartAccountId": 310,
  "offsetAccountId": 201,
  "totalAmount": 1760000,
  "statusId": 1
}
```

## Что frontend должен изменить

1. На форме расчета зарплаты добавить выбор шести обязательных счетов.
2. Для подсказок использовать роли `payroll_accrual` из `document-account-settings`.
3. Отправлять выбранные ID в `POST /api/payroll/documents/calculate`.
4. В деталях документа показывать сохраненные счета документа и пары `debitAccountId/creditAccountId` расчетных строк.
5. Разрешить `RECLASSIFICATION` в списке типов компонента.
6. Для `RECLASSIFICATION` сделать обязательными `expenseAccountId` и `liabilityAccountId`.
7. Не включать `RECLASSIFICATION` в визуальный итог удержаний и не вычитать его из чистой зарплаты.
8. На форме выплаты добавить обязательный `offsetAccountId`.
9. Не рассчитывать и не подставлять счета автоматически без решения пользователя.

## Обратная совместимость

- Старый frontend без шести account-полей получит `400 Validation.General` при расчете зарплаты.
- Старый frontend без `offsetAccountId` получит `400 Validation.General` при создании выплаты.
- В ответах старых документов account-поля могут быть `null`, потому что поля базы добавлены nullable для сохранения существующих данных.
- Старый черновик без сохраненной фактической пары счетов нельзя подтвердить: backend вернет `Payroll.StoredPostingAccountMissing`. Такой документ нужно пересчитать после обновления.
- Перед использованием нового backend необходимо выполнить SQL-скрипт `1617_add_payroll_account_fields.sql`.
