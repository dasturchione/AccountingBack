# Accounting Back — Standartlar auditi

## 1. Audit haqida

- Loyiha: `accounting_back`
- Loyiha yo‘li: `D:\Projects\accounting_back`
- Mezon: amaldagi `D:\Projects\accounting_back\STANDARDS.md`
- Qayta baholash manbasi: oldingi `AUDIT_REPORT.md`dagi 40 ta topilma
- Audit qamrovi: faqat mavjud topilmalarni amaldagi standartga solishtirish
- Tekshirilmagan amallar: database, SQL bajarilishi, scaffold, test, git, runtime endpoint va kodni qayta qidirish

## 2. Umumiy statistika

| Jiddiylik | Topilmalar soni |
|---|---:|
| KRITIK | 3 |
| MUHIM | 17 |
| KICHIK | 4 |
| Jami | 24 |

| Qo‘shimcha ko‘rsatkich | Soni |
|---|---:|
| Standart o‘zgarishi yoki talab yo‘qligi sababli olib tashlangan | 13 |
| Aniqlanmagan | 3 |
| Jiddiyligi o‘zgargan | 3 |

## 3. KRITIK holatlar

### AslBelgi transferlarida transaction aniqlanmadi

- Jiddiylik: KRITIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\AslBelgi\Transfers\AslBelgiTransferService.cs`
- Buzilgan STANDARDS.md bandi: 9-bo‘lim, Write operatsiya va Transaction mexanizmi
- Hozirgi holat: `SaveChangesAsync` bevosita chaqirilgan, `BaseService.ExecuteInTransactionAsync` orqali transaction aniqlanmagan.
- Dalil: oldingi auditdagi 76-, 89- va 123-qatorlar; audit log va idempotency kodi ayni faylda qayd etilgan.

### CRUD write metodlarida transaction va audit log aniqlanmadi

- Jiddiylik: KRITIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Application\Features\Cmn\Banks\Services\BankService.cs`; `D:\Projects\accounting_back\src\Application\Features\Cmn\Currencies\Services\CurrencyService.cs`; `D:\Projects\accounting_back\src\Application\Features\Cash\CashBoxes\Services\CashBoxService.cs`; `D:\Projects\accounting_back\src\Application\Features\Counterparty\CounterpartyContacts\Services\CounterpartyContactService.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\Roles\Services\RoleService.cs`; `D:\Projects\accounting_back\src\Application\Features\Inv\ProductPrices\Services\ProductPriceService.cs`; `D:\Projects\accounting_back\src\Application\Features\Org\Branches\Services\BranchService.cs`; `D:\Projects\accounting_back\src\Application\Features\Org\Departments\Services\DepartmentService.cs`
- Buzilgan STANDARDS.md bandi: 9-bo‘lim, Write operatsiya
- Hozirgi holat: `CreateAsync`, `UpdateAsync` yoki `DeleteAsync` repository chaqiruvlari qayd etilgan, transaction va audit log chaqiruvi aniqlanmagan.
- Dalil: oldingi auditda `_command.CreateAsync(...)`, `_command.UpdateAsync(...)`, `_command.DeleteAsync(...)` hamda `ExecuteInTransactionAsync` va `IAuditLogService` qidiruvi natijalari keltirilgan.

### Transaction wrapperi mavjud write metodlarda audit log aniqlanmadi

- Jiddiylik: KRITIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Application\Features\Acc\OpeningBalances\Services\OpeningBalanceService.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\Users\Services\UserService.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\Settings\Services\SettingService.cs`; `D:\Projects\accounting_back\src\Application\Features\Organization\Setup\Services\OrganizationSetupService.cs`; `D:\Projects\accounting_back\src\Application\Features\Sale\SaleShipments\Services\SaleShipmentService.cs`; `D:\Projects\accounting_back\src\Application\Features\Register\Reposting\Services\RepostService.cs`; `D:\Projects\accounting_back\src\Application\Features\Notifications\Services\NotificationService.cs`; `D:\Projects\accounting_back\src\Application\Features\Fa\FaAssets\Services\FaAssetService.cs`; `D:\Projects\accounting_back\src\Application\Features\Platform\Services\PlatformService.cs`; `D:\Projects\accounting_back\src\Application\Features\Cmn\CurrencyRevaluations\Services\CurrencyRevaluationService.cs`
- Buzilgan STANDARDS.md bandi: 9-bo‘lim, Write operatsiya
- Hozirgi holat: transaction wrapperi qayd etilgan, ammo audit log chaqiruvi aniqlanmagan.
- Dalil: oldingi auditdagi `ExecuteInTransactionAsync(nameof(...))` va `IAuditLogService` yoki `AuditLogService.CreateAsync` qidiruvi natijalari.

## 4. MUHIM holatlar

### SQL jadvallari uchun generated entity aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1006_create_acc_account_resolve_rule.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1007_create_acc_posting_rule.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1008_create_acc_posting_rule_line.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1009_create_acc_posting_alias.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1010_create_acc_posting_alias_translation.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1011_create_acc_payment_purpose.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1012_create_acc_payment_purpose_translation.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\05_bank\0503_create_table_bank_operation_line.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0125_create_table_cmn_product_type.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0126_create_table_cmn_product_type_translation.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\03_counterparty\0304_create_table_counterparty_account_payment_purpose_hint.sql`
- Buzilgan STANDARDS.md bandi: 3-bo‘lim, Entity zanjiri va balans qoidasi
- Hozirgi holat: 11 ta SQL jadvali uchun mos generated entity aniqlanmagan.
- Dalil: oldingi auditda `Generated\Entities` ichida mos `[Table("...")]` atributli entity aniqlanmagani qayd etilgan.

### Faktura service’da `new HttpClient()` ishlatilgan

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Services\FakturaService.cs:26`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: `FakturaService` konstruktorida `new HttpClient()` yaratilgan.
- Dalil: `_httpClient = new HttpClient();`.

### WebApi controller Domain namespace’iga bevosita ulangan

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Presentation\WebApi\Controllers\Cmn\ManualController.cs:2`
- Buzilgan STANDARDS.md bandi: 2-bo‘lim, Qatlamlar va bog‘lanish qoidalari
- Hozirgi holat: controller `Domain.Entities` namespace’iga bevosita ulangan.
- Dalil: `using Domain.Entities;`; oldingi auditda shu controller’da `IManualService` ham ishlatilgani qayd etilgan.

### Generated entity uchun mos domain entity aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Generated\Entities\CmnMxikCatalog.cs`
- Buzilgan STANDARDS.md bandi: 3-bo‘lim, Entity zanjiri va balans qoidasi
- Hozirgi holat: `cmn_mxik_catalog` generated entity’si uchun mos domain entity aniqlanmagan.
- Dalil: `[Table("cmn_mxik_catalog")]` atributi va `D:\Projects\accounting_back\src\Domain\Entities` ichidagi oldingi qidiruv natijasi.

### Bitta SQL faylda bir nechta jadval yaratilgan

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1016_create_table_acc_chart_account_preset.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1017_create_table_acc_chart_account_preset_account.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1025_create_table_acc_document_account_type.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1026_create_table_acc_document_account_role.sql`
- Buzilgan STANDARDS.md bandi: 4-bo‘lim, SQL script siyosati
- Hozirgi holat: har bir faylda ikkitadan `CREATE TABLE` statement’i qayd etilgan.
- Dalil: oldingi auditdagi statement sanog‘i.

### ALTER yoki remove scriptlari mavjud

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\00_sys\0011_alter_table_sys_user.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\04_inv\0417_alter_table_inv_product_table.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\05_bank\0504_alter_table_bank_operation.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\06_cash\0603_alter_table_cash_operation.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1015_alter_table_acc_chart_account.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1023_remove_posting_rules.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1024_alter_table_acc_chart_account_subkonto.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\15_integration\1505_remove_deprecated_integrations.sql`
- Buzilgan STANDARDS.md bandi: 4-bo‘lim, SQL script siyosati
- Hozirgi holat: alohida o‘zgartirish yoki o‘chirish scriptlari qayd etilgan.
- Dalil: oldingi auditda `ALTER TABLE`, `DROP INDEX`, `DROP TABLE` yoki `DROP FUNCTION` statement’lari aniqlangani ko‘rsatilgan.

### INSERT statement’lari create faylidan alohida joylashgan

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\04_inv\0417_alter_table_inv_product_table.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1020_insert_default_acc_chart_account_preset.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1021_insert_default_acc_chart_account_preset_account.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\10_acc\1022_insert_default_acc_chart_account_preset_account_subkonto.sql`
- Buzilgan STANDARDS.md bandi: 4-bo‘lim, SQL script siyosati
- Hozirgi holat: `INSERT INTO` statement’lari tegishli create faylidan alohida qayd etilgan.
- Dalil: oldingi auditda ko‘rsatilgan fayllardagi `INSERT INTO` natijalari.

### DROP TABLE scripti va tegishli create fayli birga mavjud

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0135_drop_table_cmn_product_type.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\15_integration\1505_remove_deprecated_integrations.sql`; `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\01_cmn\0134_create_cmn_mxik_catalog.sql`
- Buzilgan STANDARDS.md bandi: 4-bo‘lim, SQL script siyosati
- Hozirgi holat: `DROP TABLE` statement’lari va tegishli create fayli qayd etilgan.
- Dalil: oldingi auditdagi `0135_drop_table_cmn_product_type.sql` hamda `1505_remove_deprecated_integrations.sql` dalili.

### Feature namespace’lari ajratilgan

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Application\Features\Cmn\Taxes\Integration\DTOs\TaxIntegrationRequests.cs`; `D:\Projects\accounting_back\src\Application\Features\Integration\AslBelgi\DTOs\CounterpartyStatusRequestDto.cs`; `D:\Projects\accounting_back\src\Application\Features\Reports\DTOs\DateRangeDto.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\Users\Services\UserService.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\Users\Validators\UserCreateDtoValidator.cs`
- Buzilgan STANDARDS.md bandi: 6-bo‘lim, Namespace qoidalari
- Hozirgi holat: namespace tarkibida `.DTOs`, `.Services` yoki `.Validators` bo‘lgan fayllar qayd etilgan.
- Dalil: `Application.Features.Reports.DTOs`, `Application.Features.Users.Services` va `Application.Features.Integration.AslBelgi.Validators`.

### Faktura provider uchun named HttpClient aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Services\FakturaService.cs`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: provider uchun `AddHttpClient(...)` registration aniqlanmagan.
- Dalil: `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Configs\ServiceCollectionExtensions.cs` faqat Options binding va service registrationini o‘z ichiga olishi qayd etilgan.

### CentralBank typed HttpClient ishlatadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\CentralBank\Configs\ServiceCollectionExtensions.cs:13`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: named client o‘rniga typed client qayd etilgan.
- Dalil: `AddHttpClient<ICurrencyRateProvider, CentralBankCurrencyRateProvider>();`.

### CentralBank handler va client-names tuzilmasi aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\CentralBank\Configs\ServiceCollectionExtensions.cs`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: `Http` papkasi, authorization handler, retry handler va `CentralBankHttpClientNames` aniqlanmagan.
- Dalil: oldingi auditda faqat `Configs` va `Services` papkalari qayd etilgan.

### Tax handler va client-names tuzilmasi aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Tax\Configs\ServiceCollectionExtensions.cs:13`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: auth/retry handlerlar va `TaxHttpClientNames` aniqlanmagan.
- Dalil: `D:\Projects\accounting_back\src\Infrastructure\Integration\Tax\Configs\ServiceCollectionExtensions.cs:13` va `D:\Projects\accounting_back\src\Infrastructure\Integration\Tax\Providers\TaxProviderBase.cs:51`.

### AslBelgi timeout’i Options’dan olinmaydi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\AslBelgi\Configs\ServiceCollectionExtensions.cs:32`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: timeout kod ichida 30 soniya qilib berilgan.
- Dalil: `client.Timeout = TimeSpan.FromSeconds(30);`.

### Faktura provider shablon tuzilmasiga mos emas

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Configs\ServiceCollectionExtensions.cs`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: `Http` papkasi, authorization handler, retry handler, named client, `FakturaHttpClientNames` va `Dtos/Request/*` hamda `Dtos/Response/*` aniqlanmagan.
- Dalil: oldingi auditda `Configs`, `Models` va `Services` papkalari qayd etilgan.

### AslBelgi provider shablon tuzilmasi to‘liq aniqlanmadi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\AslBelgi\Configs\ServiceCollectionExtensions.cs`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: `IAslBelgiService.cs`, `AslBelgiService.cs`, `Dtos/Request/*` va `Dtos/Response/*` aniqlanmagan.
- Dalil: oldingi auditda `Configs`, `Http`, `Services` va `Transfers` qayd etilgan, `Dtos` aniqlanmagan.

### GoogleDrive ServiceCollectionExtensions joylashuvi

- Jiddiylik: MUHIM
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\GoogleDrive\Extensions\ServiceCollectionExtensions.cs`
- Buzilgan STANDARDS.md bandi: 7-bo‘lim, Integratsiya shabloni
- Hozirgi holat: `ServiceCollectionExtensions.cs` `Configs` o‘rniga `Extensions` papkasida qayd etilgan.
- Dalil: to‘liq fayl yo‘li.

## 5. KICHIK holatlar

### SQL `create_table_` nomlash shakli

- Jiddiylik: KICHIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\04_inv\0415_create_table_inv_warehouse_product.sql`
- Buzilgan STANDARDS.md bandi: 4-bo‘lim, SQL script siyosati
- Hozirgi holat: fayl nomida `create_table_` ishlatilgan.
- Dalil: to‘liq fayl nomi.

### DTO nomlari `Dto` suffiksiga mos emas

- Jiddiylik: KICHIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Application\Features\Cash\CashDocuments\DTOs\CashDocumentDtos.cs`; `D:\Projects\accounting_back\src\Application\Features\Inv\WarehouseProducts\DTOs\WarehouseProductBalanceItem.cs`; `D:\Projects\accounting_back\src\Application\Features\Notifications\DTOs\CreateNotificationRequest.cs`; `D:\Projects\accounting_back\src\Application\Features\Notifications\DTOs\NotificationListResponse.cs`; `D:\Projects\accounting_back\src\Application\Features\Notifications\DTOs\NotificationQuery.cs`; `D:\Projects\accounting_back\src\Application\Features\Sys\AuditLogs\DTOs\AuditLogDto.cs`
- Buzilgan STANDARDS.md bandi: 5-bo‘lim, Nomlash qoidalari
- Hozirgi holat: `CashDocumentListFilter`, `CashBookFilter`, `CreateNotificationRequest`, `NotificationListResponse`, `NotificationQuery`, `WarehouseProductBalanceItem` va `ChangeResult` nomlari qayd etilgan.
- Dalil: `D:\Projects\accounting_back\src\Application\Features\Cash\CashDocuments\DTOs\CashDocumentDtos.cs:23` qatoridagi `CashDocumentListFilter` declarationi.

### Options class nomlari `Options` suffiksiga mos emas

- Jiddiylik: KICHIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\CentralBank\Configs\CentralBankSettings.cs`; `D:\Projects\accounting_back\src\Infrastructure\Integration\Tax\Configs\TaxIntegrationSettings.cs`; `D:\Projects\accounting_back\src\Infrastructure\Integration\GoogleDrive\Configs\GoogleDriveSettings.cs`; `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Configs\FakturaAuthSettings.cs`; `D:\Projects\accounting_back\src\Infrastructure\Integration\Email\Configs\EmailSettings.cs`
- Buzilgan STANDARDS.md bandi: 5-bo‘lim, Nomlash qoidalari
- Hozirgi holat: Options sifatida ishlatilayotgan class nomlarida `Settings` yoki `AuthSettings` suffiksi bor.
- Dalil: `D:\Projects\accounting_back\src\Infrastructure\Integration\CentralBank\Configs\CentralBankSettings.cs:3` qatoridagi class declaration.

### Qabul qilinmagan check constraint nomi

- Jiddiylik: KICHIK
- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\13_notification\1302_create_sys_notification.sql`
- Buzilgan STANDARDS.md bandi: 5-bo‘lim, Index va constraint nomlari
- Hozirgi holat: `sys_notification_type_id_key` check constraint nomi qayd etilgan.
- Dalil: oldingi auditda bu nom `ck_*`, `chk_*` yoki `*_check` shakliga kirmasligi ko‘rsatilgan.

## 6. Standart o‘zgarishi sababli olib tashlangan topilmalar

| Nima edi | Nima uchun endi buzilish emas | Asos bo‘lgan STANDARDS.md bandi |
|---|---|---|
| Faktura `GetTokenAsync` tashqi HTTP `POST` so‘rovida transaction, audit log va idempotency key yo‘qligi | Tashqi `POST`ning o‘zi write emas; write faqat database `INSERT`, `UPDATE` yoki `DELETE`. | 9-bo‘lim, Write operatsiya |
| Solution tarkibiga kiritilmagan qo‘shimcha project | Amaldagi standart solution tarkibiga kiritilmagan loyiha uchun taqiq yoki talab belgilamaydi. | 2-bo‘lim, Qatlamlar va bog‘lanish qoidalari |
| Domain entity uchun `DbSet<T>` aniqlanmagani | Amaldagi standart `AppDbContext` domain entity’lardan foydalanishini talab qiladi, alohida `DbSet<T>` deklaratsiyasini majburiy qilmaydi. | 3-bo‘lim, Entity zanjiri va balans qoidasi |
| Domain entity’larning modul namespace’ida emasligi | `Domain.Entities` modul qo‘shilmasdan ishlatilishi amaldagi standartga mos. | 6-bo‘lim, Namespace qoidalari |
| WebApi controller’larning modul namespace’ida emasligi | `WebApi.Controllers` modul qo‘shilmasdan ishlatilishi amaldagi standartga mos. | 6-bo‘lim, Namespace qoidalari |
| Infrastructure integration namespace’lari `Integration.{Provider}.{Bo‘lim}` shaklida bo‘lgani | Bu shakl amaldagi standartning talab qilingan integration namespace’idir. | 6-bo‘lim, Namespace qoidalari |
| Didox, Edocs va OneC root kalitlari yo‘qligi | Hali yaratilmagan yoki loyihada bo‘lmagan integratsiya root kalitisiz bo‘lishi standart buzilishi emas. | 8-bo‘lim, Integratsiya root kalitlari |
| `IUnitOfWork`da `ExecuteInTransactionAsync` yo‘qligi | Amaldagi standart metodni `BaseService`da belgilaydi, `IUnitOfWork`da emas. | 9-bo‘lim, Transaction mexanizmi |
| SQL prefikslarining takrorlanishi yoki bo‘shligi | Amaldagi standart raqamli prefiksni belgilaydi, prefiksning yagonaligi va uzluksizligini talab qilmaydi. | 4-bo‘lim, SQL script siyosati |
| `uidx_*` va `uq_*` unique index nomlari | Amaldagi standart mavjud database obyektlari uchun ikkala shaklni qabul qiladi. | 5-bo‘lim, Index va constraint nomlari |
| `fk_*` foreign key nomlari | Amaldagi standart mavjud database obyektlari uchun `fk_*` shaklini qabul qiladi. | 5-bo‘lim, Index va constraint nomlari |
| `CentralBank`, `TaxIntegration`, `Email` va `GoogleDrive` root kalitlari | Misollar ro‘yxatida yo‘q mavjud integratsiya root kaliti standart buzilishi emas. | 8-bo‘lim, Integratsiya root kalitlari |
| `Domain.Entities` namespace’i bo‘yicha kichik topilma | `Domain.Entities` modul nomisiz bo‘lishi amaldagi standartning bevosita talabidir. | 6-bo‘lim, Namespace qoidalari |

## 7. Aniqlanmaganlar

### Tax provider HTTP POST operatsiyasi

- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Tax\Providers\TaxProviderBase.cs:96`
- Nima aniqlanmagani: `HttpMethod.Post` so‘rovi tashqi tizimga yuboriladigan operatsiyami yoki boshqa HTTP chaqiruvimi aniqlanmagan.
- Sabab: amaldagi standart transaction va audit logni faqat database write uchun, idempotency key’ni esa tashqi tizimga yuborish uchun talab qiladi; oldingi dalil faqat `POST` ishlatilganini ko‘rsatadi.

### Async suffiksisiz metodlar

- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Presentation\WebApi\Controllers\Cmn\BankController.cs:25`; `D:\Projects\accounting_back\src\Presentation\WebApi\Controllers\Sys\AuthController.cs:23`; `D:\Projects\accounting_back\src\Infrastructure\BackgroundServices\BackupJob.cs:28`
- Nima aniqlanmagani: 201 ta metodning qaysilari oddiy metod, qaysilari framework metodi yoki controller action ekani to‘liq ajratilmagan.
- Sabab: oldingi dalil sifatidagi `GetAll`, `Login` controller actionlari va `Execute` framework metodi amaldagi istisnoga kiradi; qolgan metodlar haqida yetarli dalil yo‘q.

### Private field nomlari

- To‘liq fayl yo‘li: `D:\Projects\accounting_back\src\Infrastructure\Integration\Faktura\Services\FakturaService.cs:18`; `D:\Projects\accounting_back\src\Infrastructure\Authentication\TokenProvider.cs:15`; `D:\Projects\accounting_back\src\Application\Features\Reports\Exports\PdfReportExporter.cs:30`
- Nima aniqlanmagani: 90 ta private fieldning qaysilari oddiy field, qaysilari `const` yoki `static readonly` ekani to‘liq ajratilmagan.
- Sabab: amaldagi standart oddiy private field uchun `_camelCase`, `const` va `static readonly` uchun `PascalCase`ni qabul qiladi; oldingi dalil modifierlarni to‘liq bermaydi.
