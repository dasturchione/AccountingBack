# Точки приёма платежей и операции по ним

## Цель

Заменить узкую сущность `bank_terminal` на универсальную точку приёма платежей организации и добавить отдельный документ движения денег относительно такой точки.

Поддерживаемые точки: POS, QR, веб-оплата, мобильное приложение, виртуальный терминал, платёжная ссылка и другие электронные каналы.

Модель намеренно остаётся простой. Отдельные таблицы провайдеров, merchant-счетов, маршрутов, видов операций и связей с исходными документами не создаются.

## Итоговые таблицы

### `cmn_payment_acceptance_point_type`

Общий системный справочник типов:

```text
id           smallserial primary key
code         varchar(50) not null unique
name         varchar(200) not null
state_id     smallint not null default 1 references cmn_state(id)
created_date timestamp without time zone not null default now()
```

Начальные коды: `POS`, `QR`, `WEB`, `MOBILE_APP`, `VIRTUAL_TERMINAL`, `PAYMENT_LINK`, `OTHER`.

Переводы хранятся в `cmn_payment_acceptance_point_type_translation` по языкам проекта.

### `org_payment_acceptance_point`

Физическая или виртуальная точка приёма платежа:

```text
id                   serial primary key
organization_id      int not null references org_organization(id)
type_id              smallint not null references cmn_payment_acceptance_point_type(id)
bank_account_id      int references org_bank_account(id)
code                 varchar(50) not null
name                 varchar(250) not null
merchant_id          varchar(150)
external_id          varchar(150)
serial_number        varchar(150)
state_id             smallint not null references cmn_state(id)
created_date         timestamp without time zone not null default now()
```

`code` генерируется backend в формате `PAP_<GUID без дефисов>` и не принимается в create/update request.

Ограничения:

- `unique (organization_id, code)`;
- уникальный непустой `(organization_id, external_id)`;
- уникальный непустой `(organization_id, serial_number)`;
- `bank_account_id`, если указан, принадлежит той же организации;

Поле процента комиссии не добавляется: договорная ставка не гарантирует фактическое удержание банка или платёжной системы.

Начальный остаток отдельным полем не хранится, потому что операции точки могут иметь разные валюты. При переходе на новый учёт начальный остаток вводится первой проведённой операцией `IN` в соответствующей валюте.

### `payment_acceptance_point_operation`

Документ одного движения денег относительно точки:

```text
id                          bigserial primary key
organization_id             int not null references org_organization(id)
payment_acceptance_point_id int not null references org_payment_acceptance_point(id)
direction_id                smallint not null references cmn_movement_direction(id)
doc_number                  varchar(100) not null
doc_date                    timestamp without time zone not null
currency_id                 smallint not null references cmn_currency(id)
amount                      numeric(24,8) not null
exchange_rate               numeric(18,6) not null default 1
external_transaction_number varchar(150)
comment                     varchar(1000)
status_id                   smallint not null references cmn_document_status(id)
state_id                    smallint not null references cmn_state(id)
created_date                timestamp without time zone not null default now()
posted_at                   timestamp without time zone
posted_by_user_id           int
cancelled_at                timestamp without time zone
cancelled_by_user_id        int
```

Не добавляются `related_document_id`, `operation_type_id`, `commission_percent`, `commission_amount` и `net_amount`.

Направление определяется существующим `cmn_movement_direction`:

- `1 / IN` — деньги поступили относительно точки;
- `-1 / OUT` — деньги выбыли относительно точки.

`amount` и `exchange_rate` должны быть больше нуля. Точка должна быть активна и принадлежать текущей организации. Документ использует статусы `DRAFT`, `POSTED`, `CANCELLED`.

## Индексы

Помимо индексов внешних ключей создать:

```sql
create index ix_payment_acceptance_point_operation_draft
    on payment_acceptance_point_operation
    (
        organization_id,
        doc_date desc,
        id desc
    )
    where status_id = 1;

create index ix_payment_acceptance_point_operation_point_date
    on payment_acceptance_point_operation
    (
        payment_acceptance_point_id,
        doc_date desc,
        id desc
    );
```

Первый индекс обслуживает отдельный рабочий список черновиков организации.

## Безопасная миграция `bank_terminal`

Миграция сохраняет существующие ID и ссылки:

1. Создаёт типы и переводы.
2. Переименовывает `bank_terminal` в `org_payment_acceptance_point`.
3. Добавляет `type_id`, `code` и недостающие поля.
4. Существующим строкам назначает тип `POS` и код `PAP_<id>`.
5. Переименовывает `external_terminal_id` в `external_id`.
6. Переименовывает `rtl_sale_doc_payment.bank_terminal_id` в `payment_acceptance_point_id` без изменения значений.
7. Переименовывает или пересоздаёт индексы, FK и функцию проверки организации.

Старые create-скрипты обновляются для чистого развёртывания. Новый alter-скрипт должен безопасно выполняться на существующей базе и не удалять данные.

## Нумерация и общий реестр документов

`payment_acceptance_point_operation` получает собственный `cmn_document_type` и использует общий генератор номеров проекта:

```text
1, 2, 3 ... отдельно для organization_id и каждого календарного года
```

Проведённые и отменённые операции синхронизируются с `cmn_document_registry`, как `bank_operation` и `cash_operation`. Отсутствие `related_document_id` означает только то, что операция не содержит ссылки на другой исходный документ.

## Денежный регистр

При проведении создаётся запись `money_reg_balance`:

```text
source_type  = PAYMENT_ACCEPTANCE_POINT
source_id    = payment_acceptance_point_id
direction_id = payment_acceptance_point_operation.direction_id
amount       = payment_acceptance_point_operation.amount
```

Остаток точки:

```text
SUM(direction_id * amount)
```

Отмена не удаляет проведённую запись, а создаёт обратное движение с противоположным `direction_id` и ссылкой `reversal_entry_id`, как в существующих денежных регистрах.

`payment_acceptance_point_operation` не создаёт бухгалтерские проводки. Проводки продажи и фактического банковского поступления остаются владельцами `rtl_sale_doc` и `bank_operation`; это исключает двойное отражение денег в `accounting_register_entry`.

## Domain и Application

Добавляются или переименовываются сущности:

- `PaymentAcceptancePointType`;
- `PaymentAcceptancePointTypeTranslation`;
- `PaymentAcceptancePoint` вместо `BankTerminal`;
- `PaymentAcceptancePointOperation`.

Навигации:

```text
Organization.PaymentAcceptancePoints
Organization.PaymentAcceptancePointOperations
BankAccount.PaymentAcceptancePoints
PaymentAcceptancePoint.Operations
RetailSaleDocPayment.PaymentAcceptancePoint
MovementDirection.PaymentAcceptancePointOperations
```

Feature-папки:

```text
Application/Features/PaymentAcceptancePoints
Application/Features/PaymentAcceptancePointOperations
```

Они следуют текущим шаблонам DTO, validators, criteria builders, projection builders, order builders, services, lifecycle services и errors.

## API точек

```text
GET    /api/payment-acceptance-points
GET    /api/payment-acceptance-points/{id}
POST   /api/payment-acceptance-points
PUT    /api/payment-acceptance-points/{id}
DELETE /api/payment-acceptance-points/{id}
```

Create/update принимают настройки точки, но не принимают `code`.

Manual API:

```text
GET /api/manual/payment-acceptance-points
GET /api/manual/payment-acceptance-point-types
```

Старые маршруты `/api/bank-terminals` и `/api/manual/bank-terminals` удаляются. Permission-записи переименовываются обновлением существующих строк, чтобы сохранить назначенные ролям module ID.

## API операций

```text
GET    /api/payment-acceptance-point-operations
GET    /api/payment-acceptance-point-operations/{id}
POST   /api/payment-acceptance-point-operations
PUT    /api/payment-acceptance-point-operations/{id}
DELETE /api/payment-acceptance-point-operations/{id}
PUT    /api/payment-acceptance-point-operations/{id}/confirm
PUT    /api/payment-acceptance-point-operations/{id}/cancel
```

Правила lifecycle:

- create создаёт `DRAFT` и автоматически назначает `doc_number`;
- менять и удалять можно только `DRAFT`;
- confirm переводит `DRAFT` в `POSTED` и создаёт движение денежного регистра;
- cancel переводит `POSTED` в `CANCELLED` и создаёт обратное движение;
- повторный confirm/cancel отклоняется;
- все запросы ограничиваются текущей организацией.

Фильтры списка: `paymentAcceptancePointId`, `directionId`, `statusId`, `stateId`, период `docDate`, `search`, `page`, `pageSize`. По умолчанию сортировка `docDate desc`, затем `id desc`.

## Розничная продажа

В `rtl_sale_doc_payment` и DTO выполняется переименование:

```text
BankTerminalId   -> PaymentAcceptancePointId
BankTerminalName -> PaymentAcceptancePointName
```

При переданном ID backend проверяет активность и принадлежность точки организации. Поле остаётся nullable для наличных и прямых банковских платежей.

На этом этапе розничная продажа не создаёт `payment_acceptance_point_operation` автоматически, потому что по утверждённой модели у операции нет ссылки на исходный документ. Операции ведутся собственным CRUD и lifecycle.

## Проверка

- SQL миграция на старой схеме сохраняет ID терминалов и ссылки розничных платежей;
- повторный запуск миграционного SQL не повреждает данные;
- сборка solution;
- CRUD точки и автоматическая генерация `code`;
- tenant scope и проверка банковского счёта;
- CRUD операции и автоматическая нумерация по организации/году;
- ограничения изменения статусов;
- confirm создаёт `IN` или `OUT` в `money_reg_balance`;
- cancel создаёт точное обратное движение;
- расчёт остатка включает только проведённые движения через регистр;
- отдельный запрос черновиков использует частичный индекс `where status_id = 1`;
- розничная продажа продолжает работать после переименования FK и DTO;
- отсутствуют двойные бухгалтерские проводки.
