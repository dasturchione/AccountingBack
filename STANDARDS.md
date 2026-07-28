## O‘zgarishlar tarixi

| Sana | O‘zgarish |
|---|---|
| 2026-07-25 | Nomlash qoidalarida mavjud index va constraint variantlari, private field hamda async method istisnolari aniqlashtirildi. |
| 2026-07-25 | Namespace qoidalari koddagi amaliy namespace’lar bilan moslashtirildi. |
| 2026-07-25 | Write operatsiya ta’rifi, idempotency key qamrovi va transaction mexanizmi joylashuvi aniqlashtirildi. |
| 2026-07-25 | Config root kalitlari mavjud integratsiyalar qamroviga moslashtirildi. |
| 2026-07-25 | Hujjatdagi takrorlar birlashtirildi, ziddiyatli va eskirgan qoldiqlar yakuniy qoidalarga moslashtirildi. |
| 2026-07-25 | Qayta tekshiruvda qolgan takrorlar birlashtirildi va matn yaxlitlashtirildi. |
| 2026-07-25 | Hujjat to‘liq qayta yozilib, qoidalar takrorsiz va yaxlit tuzilishga keltirildi. |
| 2026-07-25 | Hujjat amaldagi qoidalar asosida to‘liq yangidan tuzildi. |
| 2026-07-25 | Hujjat bitta to‘liq almashtirish bilan yangidan yozildi. |

# 1. Maqsad va qo‘llanish doirasi

Ushbu hujjat `D:\Projects\accounting_back` loyihasidagi kod, SQL script, entity, integratsiya, konfiguratsiya va testlarga tatbiq etiladigan majburiy standartlar doirasini belgilaydi.

**Shunday qilinadi:** Hujjatlar va yakuniy hisobotlar o‘zbek tilida yoziladi.

**Shunday qilinmaydi:** Hujjatning o‘ziga baho yoki tavsiya qo‘shilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Application\Application.csproj`.

# 2. Qatlamlar va bog‘lanish qoidalari

**Shunday qilinadi:** Qatlamlar `SharedKernel → Domain → Application → Infrastructure → WebApi` ketma-ketligida joylashadi. Ularning yo‘llari mos ravishda `D:\Projects\accounting_back\src\SharedKernel`, `D:\Projects\accounting_back\src\Domain`, `D:\Projects\accounting_back\src\Application`, `D:\Projects\accounting_back\src\Infrastructure` va `D:\Projects\accounting_back\src\Presentation\WebApi` bo‘ladi. Controllerlar `Application` xizmatlariga murojaat qiladi.

**Shunday qilinmaydi:** Controllerga domain yoki persistence tafsilotlari kiritilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Presentation\WebApi\Controllers\Bank\BankOperationController.cs`.

# 3. Entity zanjiri va balans qoidasi

**Shunday qilinadi:** Entity zanjiri `.sql script → database → generated entity → domain entity` ko‘rinishida yuritiladi. Zanjirdagi bitta qism yangilansa, qolgan qismlar ham moslashtiriladi. Generated entity database scaffold natijasi, domain entity esa uning tozalangan ko‘rinishi bo‘ladi; ular alohida saqlanadi. `AppDbContext` domain entity’lardan foydalanadi va generated entity’ni domain entity’ga ko‘chirishni Codex bajaradi.

**Shunday qilinmaydi:** Generated `AppDbContext`, AutoMapper va Mapster ishlatilmaydi; generated papka o‘chirilmaydi. Codex database’ga ulanmaydi, SQL’ni bajarmaydi va EF Core scaffold buyrug‘ini ishga tushirmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Infrastructure\Persistence\Generated\Entities\InvProduct.cs`.

# 4. SQL script siyosati

**Shunday qilinadi:** Har bir jadval uchun bitta `.sql` fayl yuritiladi. Unda ayni jadvalning `create table`, `create index`, `create function` va `insert` matnlari bo‘ladi. Fayl nomi `raqamli prefiks + create_ + snake_case jadval nomi` shaklida bo‘lib, `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\{NN}_{modul}\` yo‘lida saqlanadi. Standart insertlar create fayliga qo‘shiladi. Javobda yangi jadval uchun to‘liq create, mavjud jadval o‘zgarishi uchun alter, faqat insert o‘zgarishi uchun faqat insert matni beriladi.

**Shunday qilinmaydi:** `alter_*` yoki `drop_*` fayli yaratilmaydi. Mavjud jadval uchun alohida alter fayli yozilmaydi, uning `create_` fayli tahrirlanadi; jadval o‘chirilganda tegishli SQL fayli ham qoldirilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Infrastructure\Persistence\Scripts\04_inv\0402_create_inv_product.sql`.

# 5. Nomlash qoidalari

**Shunday qilinadi:** Quyidagi nomlash shakllari ishlatiladi.

| Element | Qoida | Misol |
|---|---|---|
| Class | `PascalCase` | `BankOperationService` |
| Interface | `I + PascalCase` | `IBankOperationService` |
| DTO | `PascalCase + Dto` | `BankOperationCreateDto` |
| Validator | DTO nomi + `Validator` | `BankOperationCreateDtoValidator` |
| Service | `PascalCase + Service` | `BankOperationService` |
| Controller | `PascalCase + Controller` | `BankOperationController` |
| Options | `PascalCase + Options` | `JwtOptions` |
| Middleware | `PascalCase + Middleware` | `CorrelationIdMiddleware` |
| Exception | `PascalCase + Exception` | `DbCommandException` |
| Asinxron metod | Odatdagi metod uchun `PascalCase + Async` | `DeleteAsync` |
| Private field | Oddiy private field uchun `_camelCase` | `_httpClientFactory` |
| SQL jadval | Modul prefiksli `snake_case` | `inv_product` |
| SQL ustun | `snake_case` | `created_date` |
| Primary key | `{jadval}_pkey` | `marking_transfer_pkey` |
| Foreign key | Yangi obyekt uchun `{jadval}_{ustun}_fkey` | `marking_transfer_organization_id_fkey` |
| Index | `idx_*` | `idx_inv_product_code` |
| Unique index | Yangi obyekt uchun `ux_*` | `ux_inv_product_code` |
| Check constraint | Yangi obyekt uchun `ck_*` | `ck_inv_transfer_line_quantity_positive` |

**Shunday qilinmaydi:** Jadvaldagi shakllar ushbu bo‘limda ko‘rsatilgan istisnolardan tashqari o‘zgartirilmaydi.

### Index va constraint nomlari

**Shunday qilinadi:** Mavjud database obyektlarida `ux_*`, `uidx_*`, `uq_*`, `ck_*`, `chk_*`, `*_check` va `fk_*` shakllari qabul qilinadi. Yangi index uchun `idx_*`, unique index uchun `ux_*`, check constraint uchun `ck_*`, foreign key uchun `{jadval}_{ustun}_fkey` ishlatiladi.

**Shunday qilinmaydi:** Mavjud index yoki constraint nomi faqat nomlash shakli sababli almashtirilmaydi va standart buzilishi deb qayd etilmaydi.

### Private field nomlari

**Shunday qilinadi:** `const` va `static readonly` private maydonlar `PascalCase` bilan yoziladi.

**Shunday qilinmaydi:** Bu maydonlar oddiy private field uchun belgilangan `_camelCase` qoidasiga zid deb hisoblanmaydi.

### Async method nomlari

**Shunday qilinadi:** Tashqi interface yoki framework belgilagan metod nomi saqlanadi. Framework metodlari hamda ASP.NET controller action metodlari uchun `Async` suffiksi majburiy emas.

**Shunday qilinmaydi:** Framework, tashqi interface yoki controller action metodining nomi faqat `Async` suffiksi uchun o‘zgartirilmaydi va suffiks yo‘qligi standart buzilishi deb belgilanmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Application\Features\Bank\BankOperations\DTOs\BankOperationCreateDto.cs`.

# 6. Namespace qoidalari

**Shunday qilinadi:** Domain entity fayllari `Domain.Entities`, WebApi controller fayllari `WebApi.Controllers`, Infrastructure integratsiya fayllari `Integration.{Provider}.{Bo‘lim}` namespace’idan foydalanadi. Feature ichidagi barcha fayllar bitta feature namespace’ida bo‘ladi.

**Shunday qilinmaydi:** Domain va controller namespace’iga modul nomi qo‘shilmaydi. Feature namespace’iga `.DTOs`, `.Services`, `.Validators` yoki `.Projections` qo‘shilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Application\Features\Bank\BankOperations\Services\BankOperationService.cs`.

# 7. Integratsiya shabloni

**Shunday qilinadi:** Tashqi HTTP provider `D:\Projects\accounting_back\src\Infrastructure\Integration\{Provider}\` ostida joylashtiriladi. Tuzilmada `Configs/{Provider}Options.cs`, `Configs/ServiceCollectionExtensions.cs`, `Http/{Provider}AuthorizationHandler.cs`, `Http/{Provider}RetryHandler.cs`, `Services/I{Provider}Service.cs`, `Services/{Provider}Service.cs`, `Dtos/Request/*` va `Dtos/Response/*` bo‘ladi. Named HttpClient `AddHttpClient(nom)` bilan ro‘yxatdan o‘tkaziladi, auth va retry `DelegatingHandler` orqali ulanadi, timeout Options’dan, sozlamalar esa `IOptions<T>` orqali olinadi. Client nomlari `{Provider}HttpClientNames` const class’ida saqlanadi.

**Shunday qilinmaydi:** `new HttpClient()` ishlatilmaydi. Auth yoki retry service ichida qo‘lda qo‘shilmaydi va Named HttpClient’siz provider chaqiruvi yozilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Infrastructure\Integration\AslBelgi\Http\AslBelgiAuthorizationHandler.cs`.

# 8. Config qoidalari

**Shunday qilinadi:** Sozlamalar `appsettings.json` va `appsettings.{Environment}.json` fayllarida saqlanadi. Options class `D:\Projects\accounting_back\src\Infrastructure\Integration\{Provider}\Configs\` yo‘lida joylashadi, sozlamalar `IOptions<T>` orqali o‘qiladi.

**Shunday qilinmaydi:** `.env`, Secret Manager va Key Vault ishlatilmaydi. `IConfiguration` bevosita o‘qilmaydi va integratsiya sozlamalari umumiy, nomsiz root kalitga joylashtirilmaydi.

### Integratsiya root kalitlari

**Shunday qilinadi:** appsettings root kaliti faqat loyihada mavjud va ishlatilayotgan integratsiya uchun yaratiladi. Hali yaratilmagan yoki loyihada bo‘lmagan integratsiya root kalitisiz bo‘lishi standart buzilishi emas. Mavjud misollar: `AslBelgi`, `FakturaAuthSettings`, `CentralBank`, `TaxIntegration`, `Email` va `GoogleDrive`.

**Shunday qilinmaydi:** Mavjud integratsiya root kaliti oldingi misollar ro‘yxatida bo‘lmagani uchun standart buzilishi deb belgilanmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Presentation\WebApi\appsettings.json`.

# 9. Cross-cutting

**Shunday qilinadi:** Xatolar `GlobalExceptionHandler` va `ProblemDetails` bilan, logging `Serilog` va `CorrelationIdMiddleware` bilan, validatsiya `FluentValidation` va `FluentValidationFilter` bilan, audit `AuditLogService` bilan, cache `IMemoryCache` bilan bajariladi. DI uchun `D:\Projects\accounting_back\src\Infrastructure\DependencyInjection.cs` va Scrutor assembly scanning, background ishlar uchun Quartz ishlatiladi.

**Shunday qilinmaydi:** Xato oddiy, tuzilmasiz javobga aylantirilmaydi. DI qo‘lda tarqoq ro‘yxat bilan cheklanmaydi, background ish Quartz’dan boshqa mexanizm bilan yozilmaydi.

### Write operatsiya

**Shunday qilinadi:** Write operatsiya faqat database `INSERT`, `UPDATE` yoki `DELETE` bajaradigan operatsiya hisoblanadi. Har bir database write transaction va audit log bilan bajariladi.

**Shunday qilinmaydi:** Tashqi API’ga yuborilgan HTTP so‘rov `POST` bo‘lsa ham, faqat shu sababli write deb olinmaydi. Database’ni o‘zgartirmaydigan tashqi HTTP so‘rovi uchun transaction talab qilinmaydi.

### Idempotency key qamrovi

**Shunday qilinadi:** Idempotency key faqat tashqi tizimga yuboriladigan operatsiyada, request yuborilishidan oldin qo‘llanadi. Asl Belgi transferi, Didox hujjat yuborishi, Edocs operatsiyalari va 1C sinxronizatsiyasi shu qamrovga kiradi.

**Shunday qilinmaydi:** Ichki CRUD operatsiyasida idempotency key talab qilinmaydi; dublikatdan himoya unique index orqali ta’minlanadi. Database’ni o‘zgartirmaydigan tashqi HTTP so‘rovi uchun ham idempotency key talab qilinmaydi.

### Transaction mexanizmi

**Shunday qilinadi:** Database write transactioni `BaseService.ExecuteInTransactionAsync` orqali bajariladi. Bu metod `D:\Projects\accounting_back\src\Application\Features\BaseService.cs` faylida joylashadi. `IUnitOfWork` interfeysi `D:\Projects\accounting_back\src\Application\Abstractions\IUnitOfWork.cs` faylida bo‘lib, `BeginAsync`, `SaveChangesAsync`, `CommitAsync` va `RollbackAsync` metodlarini o‘z ichiga oladi.

**Shunday qilinmaydi:** `ExecuteInTransactionAsync` metodi `IUnitOfWork` interfeysiga joylashtirilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Infrastructure\Repositories\UnitOfWork.cs`.

# 10. Test siyosati

**Shunday qilinadi:** `D:\Projects\accounting_back\tests\UnitTests` va `D:\Projects\accounting_back\tests\IntegrationTests` loyihalari saqlanadi. Testlar qo‘lda bajariladi; vaqtinchalik tekshiruv testi ish yakunida o‘chiriladi.

**Shunday qilinmaydi:** Codex avtomatik test yozmaydi va testlarni avtomatik bajarmaydi. Test loyihalari o‘chirilmaydi.

**Real misol:** `D:\Projects\accounting_back\tests\UnitTests\UnitTests.csproj`.

# 11. Codex ish tartibi

**Shunday qilinadi:** Bir vaqtda faqat bitta jarayon bajariladi. Ish oxirida o‘zgargan fayllar va qolgan ishlar haqida hisobot beriladi. Kod yozishdan oldin `D:\Projects\accounting_back\STANDARDS.md` o‘qiladi. API endpoint faqat rasmiy hujjat yoki ko‘rilgan real kod bilan tasdiqlanganda olinadi.

**Shunday qilinmaydi:** Git commit, branch yoki boshqa tashqi amal bajarilmaydi. Tasdiqlanmagan endpoint yozilmaydi; yo‘l aniqlanmasa `aniqlanmadi` deb qayd etiladi. Standartga zid kod Codex tomonidan o‘z-o‘zidan tuzatilmaydi.

**Real misol:** `D:\Projects\accounting_back\src\Infrastructure\Integration\AslBelgi\Configs\ServiceCollectionExtensions.cs`.

# 12. Tasdiqlanmagan / keyinroq hal qilinadi

**Shunday qilinadi:** Ko‘rilmagan masala `aniqlanmadi` deb qayd etiladi. Database’ning haqiqiy holati, SQL bajarilganligi va scaffold qachon bajarilgani aniqlanmasa, ular tasdiqlangan deb yozilmaydi.

**Shunday qilinmaydi:** Ko‘rilmagan fayl yo‘li, namespace, database holati yoki jarayon natijasi taxmin qilinmaydi. Loyiha kodidan aniqlangan qoida bu bo‘limda qayta yozilmaydi.

**Real misol va aniqlanmagan masalalar:** `D:\Projects\accounting_back\Accounting.slnx` solution fayli mavjud. Rasmiy tashqi API endpointlari, tashqi tizimga yuborishdagi idempotency key amaliyoti va barcha `Dtos/Request/*` hamda `Dtos/Response/*` provider fayllari to‘liq aniqlanmagan.
