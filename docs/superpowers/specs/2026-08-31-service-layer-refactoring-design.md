# Дизайн полного рефакторинга сервисного слоя

Дата: 2026-08-31

## Цель

Последовательно привести `src/Application/Features` к единому стилю, проверить каждый публичный GET, исправить выбор переводов через `IUserContext.LanguageId`, разделить публичные DTO по файлам и безопасно упростить сложные сервисы без изменения API, бухгалтерских правил, статусов и транзакций.

Исходная спецификация пользователя является главным источником требований. Этот документ фиксирует выбранную стратегию исполнения и границы безопасности.

## Baseline

- Ветка: `master...origin/master`, HEAD `58187dc7`.
- Существующее пользовательское изменение: untracked `docs/service-layer-deep-analysis.md`; оно сохраняется и не смешивается с production-партиями.
- `dotnet build Accounting.slnx --no-restore`: успешно, 0 ошибок, 2 исходных `CS8629` в EDO.
- `dotnet test Accounting.slnx --no-build`: 134/134 UnitTests успешно; в IntegrationTests нет обнаруживаемых тестов.
- Статический каталог: 30 верхнеуровневых областей, 153 feature-каталога, 98 controllers, 326 `[HttpGet]`, 23 Domain translation classes.
- Docker CLI установлен, daemon на момент baseline не запущен.

## Рассмотренные подходы

### 1. Последовательно по областям — выбран

Каждая область проходит `Audit → Design → Characterization tests → Refactor small batch → Build/tests → Diff/API review → Documentation`. Риск локализуется, прогресс восстанавливается по трём рабочим документам.

### 2. Горизонтальные массовые проходы

Сначала разделить все DTO, затем исправить все projections, затем services. Быстрее механически, но создаёт огромный diff, затрудняет поиск API-регрессии и оставляет feature в промежуточном состоянии. Не используется.

### 3. Big-bang генерация/переписывание

Автоматически стандартизировать все папки и query. Несовместимо с различиями CRUD, документов, integrations и reports; особенно опасно для accounting lifecycle. Не используется.

## Порядок областей

`Cmn → Org → Organization → Counterparty → Inv → Acc → Bank → Cash → Pur → Sale → RetailSaleDocs → Fa → Hr → Pay → Register → Reports → AccountingReports → Platform → Sys → Notifications → Imports → Integration`, затем фактически найденные отдельные области: `AiAssistant`, `BankParsers`, `DocumentNumbers`, `Documents`, `PaymentAcceptancePointOperations`, `PaymentAcceptancePoints`, `Rnt`, `SaleDocs` и другие из каталога.

Фактическое имя/расположение feature сохраняется. Массового namespace rename не будет.

## Рабочие документы

1. `docs/features-refactoring-plan.md` — полный каталог feature и их состояние.
2. `docs/features-refactoring-progress.md` — текущая точка, baseline, последняя проверка и следующий точный шаг.
3. `docs/features-multilanguage-audit.md` — каждый GET, переводимые поля, fallback, search/order/paging и тесты.

Статусы: `NOT_STARTED`, `AUDITED`, `IN_PROGRESS`, `REFACTORED`, `VERIFIED`, `BLOCKED`, `BLOCKED_API_CHANGE`. Статус `VERIFIED` ставится только после build, tests, multilingual verification, DI и API review.

## Инвентаризация

Перед изменением feature автоматически и вручную сопоставляются:

- area/feature/entity/controller/service;
- GET endpoint и GET service method;
- публичные DTO/filter и число типов в файле;
- criteria/projection/order builders;
- translation entity/table, поля, навигации и composite key;
- использование `LanguageId`, fallback, nested translations;
- search/order/paging и место materialization;
- organization/global-filter behavior;
- сложность service и возможная ответственность для выделения.

Автоматический каталог является начальной картой. Решение о refactor/VERIFIED принимается после чтения кода.

## Translation design

Для каждой entity с translation navigation отображаемое поле строится SQL-translatable выражением:

```csharp
var languageId = userContext.LanguageId;

Name = entity.Translations
    .Where(x => x.LanguageId == languageId)
    .Select(x => x.Name)
    .FirstOrDefault()
    ?? entity.Name;
```

Используются реальные поля конкретной translation entity (`Name`, `ShortName`, `FullName`, `Description` и другие). Fallback: запрошенный язык → base field. Произвольного fallback-языка нет.

Проверяются главная и все вложенные display entities. Search и order используют то же отображаемое выражение и сохраняют SQL-side paging со стабильным `ThenBy(Id)`. `Include(Translations)`, client-side sorting, N+1 и materialization до paging запрещены.

Reusable public list/detail DTO получает `IProjectionBuilder<TEntity,TDto>`. Одноразовый internal/select/provider shape может использовать inline SQL projection; исключение документируется.

## DTO/file design

Один public API DTO/filter — один файл того же имени. Namespace, type name, access modifier, inheritance, nullable contract и JSON shape не меняются. Internal/private state-machine models остаются сгруппированными, если это улучшает читаемость.

DTO-перемещения выполняются отдельной партией от multilingual behavior и lifecycle refactoring.

## Service design

- API facade отвечает за CRUD и делегирование.
- Lifecycle service является единственным transaction owner confirm/cancel.
- Posting/register services не открывают независимый commit.
- Expected errors возвращаются через `Result` и feature Errors с `LanguageId`.
- Unexpected infrastructure/provider exceptions не скрываются.
- Все I/O методы передают `CancellationToken`.
- Сложный service дробится только по доказанной ответственности и после characterization tests.

Accounting, inventory, money, counterparty effects, VAT, totals, document statuses и numbering не меняются без воспроизводящего теста подтверждённой ошибки.

## Тестовая стратегия

### Baseline и быстрые проверки

- targeted project build после малой партии;
- targeted unit/characterization tests;
- full build и related/full test suite после области;
- DI smoke test для изменённых services/controllers/projections;
- source/API contract tests при DTO move.

### PostgreSQL integration harness

В `tests/IntegrationTests` добавляется Testcontainers PostgreSQL. Он нужен для реального выполнения EF queries, проверки SQL translation, navigation subquery, sorting и paging. EF InMemory не считается достаточным.

Harness:

- поднимает ephemeral PostgreSQL;
- создаёт минимальную schema/model через EF для тестируемых entities либо использует управляемую test schema;
- создаёт отдельные organization/language fixtures;
- не использует production connection string;
- завершает container после suite;
- при недоступном Docker явно сообщает инфраструктурный blocker, а не выдаёт ложный `VERIFIED`.

Для translation-aware GET проверяются: requested translation, base fallback, разные LanguageId, nested reference, list/detail/select, translated search/order, paging, отсутствие duplicate rows и wrong organization.

## API и транзакционная безопасность

Перед закрытием партии сравниваются routes, HTTP verbs, public signatures и сериализуемые DTO properties. Breaking change получает `BLOCKED_API_CHANGE`.

Для lifecycle изменение допускается только после characterization/integration test. Confirm/cancel должны оставаться симметричными по всем registers, audit — внутри той же transaction, status — последним business change.

## Git и партии

Партии малы и логически однородны:

1. test/documentation infrastructure;
2. DTO file moves;
3. multilingual query behavior;
4. service responsibility extraction;
5. confirmed bugfix.

Существующие пользовательские изменения не удаляются и не перезаписываются. Mass formatting, reset/force-push запрещены. Коммиты создаются только для логически проверенных партий; в staging не включаются несвязанные файлы.

## Definition of Done

Вся задача завершена, когда каждый фактический feature имеет `VERIFIED` либо документированный `BLOCKED/BLOCKED_API_CHANGE`, все 326+ GET проверены, translation tables/entities/keys/navs сопоставлены, public DTO разделены, DI/build/tests проходят, а полный diff не содержит незапланированных API или accounting changes.

Финальный отчёт содержит количественные результаты, blocked list, build/test evidence и перечень логических партий/commits.
