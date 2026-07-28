# Integratsiya qatlami auditi (2C.1)

Ushbu hujjat `D:\Projects\accounting_back\src` dagi tashqi integratsiya kodini `STANDARDS.md` ga muvofiqlik bo'yicha tekshirish natijasidir. Bosqich faqat o'qish rejimida bajarildi — birorta kod fayli o'zgartirilmadi.

Jiddiylik shkalasi: **KRITIK** — standartda aniq taqiqlangan yoki ma'lumot yo'qolishiga olib keladi; **MUHIM** — standart talabi bajarilmagan, xatti-harakat xavf ostida; **KICHIK** — tuzilma yoki nomlash chetlanishi.

Topilmalar: **3 KRITIK, 7 MUHIM, 6 KICHIK** — jami 16 ta.

---

## Tekshiruv 1 — Integratsiyalar ro'yxati

`src\` bo'ylab tashqi tizimga chiquvchi barcha kod qidirildi. Oltita integratsiya topildi.

| Integratsiya | Papka | Transport | Fayllar |
|---|---|---|---|
| **AslBelgi (CRPT)** | `Infrastructure\Integration\AslBelgi` | HTTP (named client) | `Configs\AslBelgiOptions.cs`, `Configs\AslBelgiOptionsValidator.cs`, `Configs\ServiceCollectionExtensions.cs`, `Http\AslBelgiAuthorizationHandler.cs`, `Http\AslBelgiGetRetryHandler.cs`, `Http\AslBelgiHttpClientNames.cs`, `Services\AslBelgiVerificationService.cs`, `Transfers\AslBelgiTransferService.cs` |
| **CentralBank** | `Infrastructure\Integration\CentralBank` | HTTP (typed client) | `Configs\CentralBankSettings.cs`, `Configs\ServiceCollectionExtensions.cs`, `Services\CentralBankCurrencyRateProvider.cs`, `Services\CurrencyRateImportService.cs` |
| **Faktura** | `Infrastructure\Integration\Faktura` | HTTP (`new HttpClient()`) | `Configs\FakturaAuthSettings.cs`, `Configs\ServiceCollectionExtensions.cs`, `Models\*.cs` (6 ta), `Services\FakturaService.cs` |
| **Tax (Soliq / Mxik / EFaktura)** | `Infrastructure\Integration\Tax` | HTTP (named client) | `Configs\TaxIntegrationSettings.cs`, `Configs\ServiceCollectionExtensions.cs`, `Providers\TaxProviderBase.cs`, `Providers\SoliqApiTaxProvider.cs`, `Providers\MxikTaxProvider.cs`, `Providers\EFakturaTaxProvider.cs`, `Providers\TaxProviderFactory.cs` |
| **Email** | `Infrastructure\Integration\Email` | SMTP (MailKit) | `Configs\EmailSettings.cs`, `Configs\ServiceCollectionExtensions.cs`, `Services\EmailSender.cs` |
| **GoogleDrive** | `Infrastructure\Integration\GoogleDrive` | Google SDK | `Configs\GoogleDriveSettings.cs`, `Configs\GoogleDriveSettingsValidator.cs`, `Extensions\ServiceCollectionExtensions.cs`, `Services\GoogleDriveUploader.cs`, `Services\IGoogleDriveUploader.cs`, `Services\GoogleDriveUploadExceptions.cs` |

Qo'shimcha qatlamlar (HTTP chaqiruv qilmaydi, lekin integratsiya qamroviga kiradi):

- `Application\Abstractions\Integration\` — `IEmailSender`, `IFakturaService`, `ICurrencyRateProvider`, `ITaxProvider` interfeyslari va modellar.
- `Application\Features\Integration\AslBelgi\` — DTO, validator, imzolash abstraksiyalari (`IEImzoSigningClient`, `UnavailableEImzoSigningClient` — E-Imzo hozircha ishlamaydi).
- `Application\Features\Cmn\Taxes\Integration\` — `TaxIntegrationService` (provayderlar ustidan fasad).
- `Presentation\WebApi\Controllers\Integration\` — controllerlar.

**Eslatma:** `Infrastructure\Services\Barcode\BarcodeGenerator.cs` faylida ham `HttpClient` uchraydi, lekin u tashqi provayder integratsiyasi emas — audit qamrovidan tashqarida qoldirildi.

Email va GoogleDrive `HttpClient` ishlatmaydi (SMTP va Google SDK), shuning uchun named client / handler talablari ularga to'g'ridan-to'g'ri tegishli emas.

---

## Tekshiruv 2 — Papka tuzilmasi

Standart §7 talab qiladigan shakl: `Configs/` + `Http/` + `Services/` + `Dtos/Request/*` + `Dtos/Response/*`.

| Integratsiya | Configs | Http | Services | Dtos | Chetlanish |
|---|---|---|---|---|---|
| **AslBelgi** | ✅ | ✅ | ✅ | ⚠️ | DTO'lar `Application` qatlamida (`Features\Integration\AslBelgi\DTOs`), `Dtos/Request` va `Dtos/Response` bo'linishi yo'q. Qo'shimcha `Transfers/` papkasi bor. |
| **CentralBank** | ✅ | ❌ | ✅ | ❌ | `Http/` yo'q — retry servis ichida. DTO'lar `Application\Abstractions\Integration` da. |
| **Faktura** | ✅ | ❌ | ✅ | ⚠️ | `Http/` yo'q. `Models/` papkasi `Dtos/` o'rniga, Request/Response bo'linishisiz. |
| **Tax** | ✅ | ❌ | ⚠️ | ❌ | `Http/` yo'q. `Services/` o'rniga `Providers/`. DTO'lar `Application` da. |
| **Email** | ✅ | — | ✅ | — | HTTP emas, `Http/` talab qilinmaydi. |
| **GoogleDrive** | ✅ | — | ✅ | — | `ServiceCollectionExtensions.cs` `Configs/` emas, **`Extensions/`** papkasida. |

**Etalon: AslBelgi.** Yagona integratsiya bo'lib, `Configs/` + `Http/` + `Services/` uchligini to'liq bajaradi, named client, Options validatori va ikkita `DelegatingHandler` ga ega. Qolgan integratsiyalarni tuzatishda shu tuzilma namuna bo'lishi kerak.

---

## Tekshiruv 3 — HttpClient

### 3.1 `new HttpClient()` — taqiqlangan

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-01** | **KRITIK** | `Infrastructure\Integration\Faktura\Services\FakturaService.cs:26` | §7 — "`new HttpClient()` ishlatilmaydi" |

Bu loyihadagi yagona `new HttpClient()`. Servis singleton emas, lekin `HttpClient` konstruktоrda yaratilib maydonda saqlanadi — bu socket exhaustion va DNS o'zgarishini sezmaslik muammosini keltiradi.

### 3.2 Named HttpClient

| Integratsiya | Ro'yxatdan o'tish | Nom | Baho |
|---|---|---|---|
| AslBelgi | `AddHttpClient(AslBelgiHttpClientNames.Client, ...)` | const class orqali | ✅ standartga mos |
| Tax | `AddHttpClient("TaxIntegration")` | **satr literal** | ⚠️ `{Provider}HttpClientNames` const class yo'q |
| CentralBank | `AddHttpClient<ICurrencyRateProvider, CentralBankCurrencyRateProvider>()` | **typed client** | ❌ named emas |
| Faktura | yo'q | — | ❌ umuman ro'yxatdan o'tmagan |

| # | Jiddiylik | Joy | Standart bandi |
|---|---|---|---|
| **F-02** | **MUHIM** | `CentralBank\Configs\ServiceCollectionExtensions.cs:13` | §7 — "Named HttpClient `AddHttpClient(nom)` bilan ro'yxatdan o'tkaziladi" |
| **F-03** | **KICHIK** | `Tax\Configs\ServiceCollectionExtensions.cs:13` va `Tax\Providers\TaxProviderBase.cs:51` | §7 — "Client nomlari `{Provider}HttpClientNames` const class'ida saqlanadi" |

### 3.3 Timeout

| Integratsiya | Manba | Baho |
|---|---|---|
| AslBelgi | **kodda qattiq: `TimeSpan.FromSeconds(30)`** | ❌ `AslBelgiOptions` da `Timeout` xossasi umuman yo'q |
| Tax | `providerSettings.TimeoutSeconds ?? Settings.TimeoutSeconds` | ✅ Options'dan |
| CentralBank | `_settings.TimeoutSeconds` | ✅ Options'dan (lekin servis konstruktorida) |
| Email | `_settings.TimeoutSeconds` | ✅ Options'dan |
| Faktura | **yo'q** — standart 100 soniya | ❌ |

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-04** | **MUHIM** | `AslBelgi\Configs\ServiceCollectionExtensions.cs:32` | §7 — "timeout Options'dan" |
| **F-05** | **MUHIM** | `Faktura\Services\FakturaService.cs` (timeout umuman berilmagan) | §7 |

### 3.4 BaseAddress

| Integratsiya | Manba | Baho |
|---|---|---|
| AslBelgi | `options.BaseUrl` (ro'yxatdan o'tishda) | ✅ |
| Tax | `providerSettings.BaseUrl` (`BuildUri`) | ✅ |
| CentralBank | `_settings.BaseUrl` (servis konstruktorida) | ⚠️ Options'dan, lekin ro'yxatdan o'tishda emas |
| Faktura | **`const string BaseUrl = "https://api.faktura.uz"` va `const string AuthUrl = "https://account.faktura.uz/token"`** | ❌ |

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-06** | **MUHIM** | `Faktura\Services\FakturaService.cs:18-19` | §8 — sozlamalar `appsettings.json` da saqlanadi, `IOptions<T>` orqali o'qiladi |

---

## Tekshiruv 4 — Handlerlar

### 4.1 Mavjud DelegatingHandler'lar

| Handler | Integratsiya | Vazifa |
|---|---|---|
| `AslBelgiAuthorizationHandler` | AslBelgi | Auth (Bearer ApiKey) + HTTPS majburiyligini tekshirish + xavfsiz debug log |
| `AslBelgiGetRetryHandler` | AslBelgi | Retry (faqat GET uchun) |

Butun loyihada faqat **2 ta** `DelegatingHandler` bor, ikkalasi ham AslBelgi'ga tegishli.

### 4.2 Auth ehtiyoji

| Integratsiya | Auth kerakmi | Hozirgi holat |
|---|---|---|
| AslBelgi | Ha (Bearer ApiKey) | ✅ handler orqali |
| Faktura | Ha (OAuth token) | ❌ **servis ichida qo'lda** |
| Tax (Soliq/Mxik/EFaktura) | **Yo'q — API ochiq** | Kodda birorta `Authorization` header yoki ApiKey yo'q; ochiq API sifatida tasdiqlandi |
| CentralBank | **Yo'q — API ochiq** | Auth yo'q, kerak emas |
| Email | SMTP login | MailKit ichida, handler tushunchasi qo'llanmaydi |
| GoogleDrive | Service account | Google SDK ichida |

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-07** | **KRITIK** | `Faktura\Services\FakturaService.cs:76-77` | §7 — "Auth yoki retry service ichida qo'lda qo'shilmaydi" |

Qo'shimcha xavf: `_httpClient.DefaultRequestHeaders.Authorization` maydonga saqlangan umumiy `HttpClient` da o'zgartiriladi. Parallel so'rovlarda bir foydalanuvchining tokeni boshqasining so'roviga tushishi mumkin.

### 4.3 Retry

| Integratsiya | Retry bormi | Qanday |
|---|---|---|
| AslBelgi | ✅ | `AslBelgiGetRetryHandler` (DelegatingHandler) |
| Tax | ⚠️ | `TaxProviderBase.SendWithRetryAsync` — **servis ichida qo'lda** (108–158-qatorlar), `Retry-After` ni hisobga oladi, exponential backoff |
| CentralBank | ⚠️ | `CentralBankCurrencyRateProvider.SendWithRetryAsync` — **servis ichida qo'lda** (87–110-qatorlar) |
| Faktura | ❌ | umuman yo'q |
| Email / GoogleDrive | — | SDK darajasida |

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-08** | **MUHIM** | `Tax\Providers\TaxProviderBase.cs:108-158` | §7 — "retry `DelegatingHandler` orqali ulanadi" |
| **F-09** | **MUHIM** | `CentralBank\Services\CentralBankCurrencyRateProvider.cs:87-110` | §7 |

Ikkala amalga oshirish ham funksional jihatdan yaxshi yozilgan (backoff, `Retry-After`, transient status filtri) — muammo joylashuvda, mantiqda emas. Handler'ga ko'chirishda mavjud mantiqni saqlab qolish mumkin.

### 4.4 DI da handler tartibi

`AslBelgi\Configs\ServiceCollectionExtensions.cs:34-35`:

```
.AddHttpMessageHandler<AslBelgiGetRetryHandler>()      // tashqi
.AddHttpMessageHandler<AslBelgiAuthorizationHandler>() // ichki
```

`HttpClient` pipeline'ida ro'yxatdan o'tish tartibi tashqidan ichkariga qarab bo'ladi, ya'ni hozir **retry tashqarida, auth ichkarida**. Amalda bu ishlaydi va hatto foydali: har bir qayta urinishda auth handler qaytadan ishlaydi va header yangilanadi. Ammo standart va odatiy amaliyot `auth → retry` ketma-ketligini nazarda tutadi.

| # | Jiddiylik | Fayl va qator | Izoh |
|---|---|---|---|
| **F-10** | **KICHIK** | `AslBelgi\Configs\ServiceCollectionExtensions.cs:34-35` | Tartib teskari; hozirgi holat ishlaydi, lekin qaror hujjatlashtirilishi yoki tartib almashtirilishi kerak |

---

## Tekshiruv 5 — Options

### 5.1 va 5.2 — Config klasslari va tegilishi kerak bo'lgan nuqtalar

Standart §5 `*Options` shaklini talab qiladi. Oltitadan **faqat bittasi** (`AslBelgiOptions`) unga mos.

| Klass | Fayl | appsettings bo'limi | DI ro'yxatdan o'tish | `IOptions<T>` ishlatiladigan joylar |
|---|---|---|---|---|
| `AslBelgiOptions` ✅ | `AslBelgi\Configs\AslBelgiOptions.cs` | `AslBelgi` (`SectionName` const) | `AslBelgi\Configs\ServiceCollectionExtensions.cs:16-18` (`AddOptions` + `ValidateOnStart`) | `AslBelgiAuthorizationHandler.cs:14`, `AslBelgiOptionsValidator.cs`, `ServiceCollectionExtensions.cs:30` |
| `CentralBankSettings` ❌ | `CentralBank\Configs\CentralBankSettings.cs` | `CentralBank` | `CentralBank\Configs\ServiceCollectionExtensions.cs:12` (`Configure<T>`) | `CentralBankCurrencyRateProvider.cs:14,17,20` |
| `EmailSettings` ❌ | `Email\Configs\EmailSettings.cs` | `Email` | `Email\Configs\ServiceCollectionExtensions.cs:12` | `EmailSender.cs` |
| `FakturaAuthSettings` ❌ | `Faktura\Configs\FakturaAuthSettings.cs` | **`FakturaAuthSettings`** | `Faktura\Configs\ServiceCollectionExtensions.cs:14-15` | `FakturaService.cs:16,22,25` |
| `GoogleDriveSettings` ❌ | `GoogleDrive\Configs\GoogleDriveSettings.cs` | `GoogleDrive` | `GoogleDrive\Extensions\ServiceCollectionExtensions.cs:16-17` (`AddOptions`) | `GoogleDriveUploader.cs`, `GoogleDriveSettingsValidator.cs` |
| `TaxIntegrationSettings` ❌ | `Tax\Configs\TaxIntegrationSettings.cs` | `TaxIntegration` | `Tax\Configs\ServiceCollectionExtensions.cs:12` | `TaxProviderBase.cs:19,22,26`, `TaxProviderFactory.cs`, provayderlar |

| # | Jiddiylik | Qamrov | Standart bandi |
|---|---|---|---|
| **F-11** | **MUHIM** | 5 ta klass: `CentralBankSettings`, `EmailSettings`, `FakturaAuthSettings`, `GoogleDriveSettings`, `TaxIntegrationSettings` | §5 — `Options` uchun `PascalCase + Options` |

Qayta nomlashda tegiladigan nuqtalar: klass fayli nomi va ichidagi tur nomi, `Configure<T>` / `AddOptions<T>` chaqiruvi, har bir `IOptions<T>` inyeksiyasi, validator klasslari (`GoogleDriveSettingsValidator`), va `TaxIntegrationSettings.ProviderSettings` ichki klassi. **`appsettings.json` bo'lim nomlari o'zgarmasligi kerak** — §8 ularni mavjud holicha qabul qiladi, faqat `FakturaAuthSettings` bo'lim nomi klass nomiga bog'liq ko'rinadi va uni o'zgartirish `appsettings.json` ga tegishni talab qiladi (bu audit qamrovidan tashqarida).

### 5.3 `IConfiguration` to'g'ridan-to'g'ri o'qilishi

`IConfiguration` faqat `ServiceCollectionExtensions` metodlarining parametri sifatida ishlatiladi — ya'ni ro'yxatdan o'tish nuqtasida, standart ruxsat bergan joyda. **Birorta servis ichida `IConfiguration` dan to'g'ridan-to'g'ri qiymat o'qilmaydi.** ✅ §8 ga mos.

### 5.4 `.env` / environment variable

`GetEnvironmentVariable`, `DotNetEnv` yoki `.env` fayliga murojaat **topilmadi**. ✅ §8 ga mos.

---

## Tekshiruv 6 — Tranzaksiya

### 6.1 `AslBelgiTransferService`

Servis `AppDbContext` ni **to'g'ridan-to'g'ri** inyeksiya qiladi (`AslBelgiTransferService.cs:24,31`) va `IUnitOfWork` dan umuman foydalanmaydi.

`SubmitTransferRequestAsync` da **uchta alohida `SaveChangesAsync`** bor, ularning har biri o'z implicit tranzaksiyasi:

| Qadam | Qator | Nima saqlanadi |
|---|---|---|
| 1 | 142 | `IdempotencyRecord` (`PENDING`) |
| — | 59 | **Tashqi CRPT so'rovi yuboriladi** |
| 2 | 76 | `MarkingTransfer` |
| 3 | 89 | `MarkingTransferCode` yozuvlari + idempotency `COMPLETED` |

**Xatolik ssenariysi.** Agar 3-qadam xato bersa (masalan kodlar ko'p va biror constraint buzilsa), 2-qadamda saqlangan `MarkingTransfer` allaqachon commit qilingan va **rollback qilinmaydi**. `catch` bloki (95-99-qatorlar) faqat `MarkFailedAsync` ni chaqiradi, u esa idempotency yozuvini `FAILED` qiladi va yana alohida `SaveChangesAsync` bajaradi (171-qator). Natijada bazada **kodlarsiz yetim `marking_transfer` qatori** qoladi, holbuki CRPT tomonda hujjat allaqachon yaratilgan.

`SubmitTransferConfirmationAsync` biroz yaxshiroq: status o'zgarishi va idempotency yakuni bitta `SaveChangesAsync` da (123-qator), lekin idempotency `PENDING` yozuvi baribir alohida commit qilingan.

Audit log `SaveChangesAsync` dan **keyin** chaqiriladi (92 va 125-qatorlar) — tranzaksiyadan tashqarida. Audit yozish xato bersa, yozuv saqlangan holicha auditsiz qoladi.

**Ijobiy tomoni:** idempotency key tashqi so'rovdan **oldin** qo'llanadi (53-qator, 59-qatordan avval) — bu §9 talabiga to'liq mos.

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-12** | **KRITIK** | `AslBelgi\Transfers\AslBelgiTransferService.cs:57-99` va `113-133` | §9 — "Har bir database write transaction va audit log bilan bajariladi"; "Database write transactioni `BaseService.ExecuteInTransactionAsync` orqali bajariladi" |

### 6.2 Boshqa integratsiya servislaridagi yozish amallari

| Servis | Yozish amali | Tranzaksiya holati |
|---|---|---|
| `CurrencyRateImportService` | Valyuta kurslarini import qiladi | ✅ **To'g'ri**: `_unitOfWork.BeginAsync` (62) → `CommitAsync` (186) → `catch` → `RollbackAsync` (193) |
| `TaxIntegrationService` | Yozish amali yo'q (faqat o'qish va provayder holati) | — |
| `AslBelgiVerificationService` | Yozish amali yo'q | — |
| `FakturaService` | Yozish amali yo'q | — |
| `EmailSender`, `GoogleDriveUploader` | Bazaga yozmaydi | — |

`CurrencyRateImportService` tranzaksiyani to'g'ri boshqaradi, lekin `BaseService.ExecuteInTransactionAsync` o'rniga `IUnitOfWork` ni to'g'ridan-to'g'ri ishlatadi.

| # | Jiddiylik | Fayl va qator | Standart bandi |
|---|---|---|---|
| **F-13** | **KICHIK** | `CentralBank\Services\CurrencyRateImportService.cs:62,186,193` | §9 — transaction `BaseService.ExecuteInTransactionAsync` orqali |

**Eslatma:** 18 ta biznes servisining tranzaksiya muammosi 9-jarayonga qoldirilgan va bu auditga kiritilmadi.

---

## Tekshiruv 7 — Qolganlari

### 7.1 Yutib yuborilgan xatolar

| # | Jiddiylik | Fayl va qator | Izoh |
|---|---|---|---|
| **F-14** | **KICHIK** | `AslBelgi\Transfers\AslBelgiTransferService.cs:188` | `catch (Exception ex) when (ex is FormatException or JsonException) { }` — bo'sh blok. Ataylab qilingan (keyin aniq `InvalidOperationException` otiladi), lekin asl parse xatosining tafsiloti yo'qoladi va diagnostika qiyinlashadi. |

Boshqa `catch` bloklari (`AslBelgiTransferService.cs:95,128`; `CurrencyRateImportService.cs:191`) xatoni qayta otadi yoki rollback qiladi — yutib yuborish yo'q. Kichik nuance: `catch` ichidagi `MarkFailedAsync` o'zi xato bersa, asl istisno niqoblanadi.

### 7.2 Maxfiy ma'lumotning logga tushishi

**Loglar toza.** `AslBelgiAuthorizationHandler.cs:38-41` faqat HTTP metod va hostni yozadi, ApiKey loglanmaydi. Tax provayderlari faqat provayder kodi, status va urinish raqamini yozadi.

Ammo **istisno xabarlarida** muammo bor:

| # | Jiddiylik | Fayl va qator | Izoh |
|---|---|---|---|
| **F-15** | **MUHIM** | `Faktura\Services\FakturaService.cs:50-52` va `84-86` | Javob tanasi (`error`) istisno xabariga **tozalanmasdan** qo'shiladi. 50-52-qatorlar aynan token (login/parol/client_secret yuboriladigan) endpointiga tegishli — provayder javobida so'rov tafsilotlari qaytsa, ular istisno matni orqali log va ProblemDetails'ga tushadi. |

Taqqoslash uchun `AslBelgiVerificationService.cs:98` xuddi shu vaziyatda `RedactSensitiveValues(...)` qo'llaydi — ya'ni loyihada to'g'ri namuna allaqachon bor.

### 7.3 Namespace'lar

Barcha 16 ta integratsiya namespace'i tekshirildi: `Integration.AslBelgi.Configs`, `Integration.AslBelgi.Http`, `Integration.CentralBank.Services`, `Integration.Faktura.Models`, `Integration.Tax.Providers` va h.k.

**Hammasi §6 dagi `Integration.{Provider}.{Bo'lim}` shakliga mos.** ✅ Namespace bo'yicha birorta buzilish topilmadi.

### 7.4 Papka joylashuvi (qo'shimcha)

| # | Jiddiylik | Joy | Standart bandi |
|---|---|---|---|
| **F-16** | **KICHIK** | `GoogleDrive\Extensions\ServiceCollectionExtensions.cs` | §7 — `Configs/ServiceCollectionExtensions.cs` bo'lishi kerak |

---

## Topilmalar jamlanmasi

| # | Jiddiylik | Qisqacha | Fayl |
|---|---|---|---|
| F-01 | KRITIK | `new HttpClient()` | `Faktura\Services\FakturaService.cs:26` |
| F-07 | KRITIK | Auth servis ichida qo'lda, umumiy client'da header o'zgartiriladi | `Faktura\Services\FakturaService.cs:76-77` |
| F-12 | KRITIK | Tranzaksiya yo'q, 3 ta alohida `SaveChanges`, xatoda yetim yozuv | `AslBelgi\Transfers\AslBelgiTransferService.cs` |
| F-02 | MUHIM | Named client o'rniga typed client | `CentralBank\Configs\ServiceCollectionExtensions.cs:13` |
| F-04 | MUHIM | Timeout kodda qattiq yozilgan | `AslBelgi\Configs\ServiceCollectionExtensions.cs:32` |
| F-05 | MUHIM | Timeout umuman berilmagan | `Faktura\Services\FakturaService.cs` |
| F-06 | MUHIM | BaseUrl/AuthUrl `const` sifatida qattiq yozilgan | `Faktura\Services\FakturaService.cs:18-19` |
| F-08 | MUHIM | Retry handler o'rniga servis ichida | `Tax\Providers\TaxProviderBase.cs:108-158` |
| F-09 | MUHIM | Retry handler o'rniga servis ichida | `CentralBank\Services\CentralBankCurrencyRateProvider.cs:87-110` |
| F-11 | MUHIM | 5 ta config klass `*Settings` nomlangan | 5 ta `Configs\*Settings.cs` |
| F-15 | MUHIM | Javob tanasi tozalanmasdan istisnoga qo'shiladi | `Faktura\Services\FakturaService.cs:50-52,84-86` |
| F-03 | KICHIK | Client nomi satr literal, const class yo'q | `Tax\Configs\ServiceCollectionExtensions.cs:13` |
| F-10 | KICHIK | Handler tartibi retry → auth | `AslBelgi\Configs\ServiceCollectionExtensions.cs:34-35` |
| F-13 | KICHIK | `IUnitOfWork` to'g'ridan-to'g'ri, `ExecuteInTransactionAsync` emas | `CentralBank\Services\CurrencyRateImportService.cs` |
| F-14 | KICHIK | Bo'sh `catch` bloki | `AslBelgi\Transfers\AslBelgiTransferService.cs:188` |
| F-16 | KICHIK | SCE `Extensions/` papkasida | `GoogleDrive\Extensions\ServiceCollectionExtensions.cs` |

---

## ISH RO'YXATI

Ishlar bir-biriga bog'liqligi bo'yicha guruhlangan. Guruhlar ichida tartib muhim, guruhlar orasida — mustaqil (A dan tashqari, u eng katta va alohida bajarilishi kerak).

### Guruh A — Faktura integratsiyasini standartga keltirish

**Qamrov: ~8 fayl.** F-01, F-05, F-06, F-07, F-15 ni birdaniga yopadi — ularning hammasi bitta servisga tegishli va alohida tuzatib bo'lmaydi.

1. `FakturaOptions` yaratish (`BaseUrl`, `AuthUrl`, `TimeoutSeconds`, mavjud credential xossalari) — `FakturaAuthSettings` o'rniga.
2. `Http\FakturaHttpClientNames.cs` const class.
3. `Http\FakturaAuthorizationHandler.cs` — token olish va keshlash mantiqini servisdan handler'ga ko'chirish.
4. `Http\FakturaRetryHandler.cs` — hozir retry umuman yo'q.
5. `ServiceCollectionExtensions.cs` — named client, `BaseAddress` va `Timeout` Options'dan, handlerlarni ulash.
6. `FakturaService.cs` — `IHttpClientFactory` ga o'tkazish, `new HttpClient()` va qo'lda auth'ni olib tashlash, istisno xabarlarida `RedactSensitiveValues` qo'llash.
7. `appsettings.json` bo'limini kengaytirish (bu audit qamrovidan tashqarida — alohida qaror).

### Guruh B — Retry va timeout'ni handler'ga ko'chirish

**Qamrov: ~9 fayl.** F-02, F-04, F-08, F-09, F-03.

1. `CentralBank\Http\CentralBankRetryHandler.cs` + `CentralBankHttpClientNames.cs`, named client'ga o'tish, `SendWithRetryAsync` ni provayderdan olib tashlash (~4 fayl).
2. `Tax\Http\TaxRetryHandler.cs` + `TaxHttpClientNames.cs`, `TaxProviderBase.SendWithRetryAsync` ni handler'ga ko'chirish (~4 fayl). Diqqat: `retrySafe` bayrog'i GET/POST farqini boshqaradi — handler'da bu mantiq saqlanishi kerak.
3. `AslBelgiOptions` ga `TimeoutSeconds` qo'shib, qattiq yozilgan 30 soniyani almashtirish (~2 fayl).

### Guruh C — Options nomlash

**Qamrov: ~16 fayl.** F-11. Mexanik, lekin keng tarqalgan o'zgarish.

`CentralBankSettings`, `EmailSettings`, `GoogleDriveSettings`, `TaxIntegrationSettings` (+ ichki `ProviderSettings`) → `*Options`. Har biri uchun: klass fayli, `Configure<T>`/`AddOptions<T>`, barcha `IOptions<T>` inyeksiyalari, validator klasslari.

**Bog'liqlik:** `FakturaAuthSettings` Guruh A ichida qayta nomlanadi — bu yerda takrorlanmasin. Guruh B dan keyin bajarilgani ma'qul, aks holda B da yaratilgan yangi fayllar ham qayta nomlanishi kerak bo'ladi.

### Guruh D — `AslBelgiTransferService` tranzaksiyasi

**Qamrov: 1–2 fayl.** F-12, F-14.

Uchala `SaveChangesAsync` ni bitta tranzaksiyaga o'rash: `BaseService.ExecuteInTransactionAsync` yoki `IUnitOfWork` orqali. Diqqat qilinadigan nuqtalar: idempotency yozuvi tashqi so'rovdan oldin **alohida** commit bo'lishi kerak (aks holda idempotency himoyasi ishlamaydi), qolgan ikkita yozuv esa bitta tranzaksiyada bo'lishi lozim. Audit log ham shu qamrovga kiritilishi kerak. Bo'sh `catch` blokiga tafsilot qo'shish.

**Eslatma:** bu guruh eng nozigi — idempotency va tranzaksiya chegarasi bir-biriga zid talab qo'yadi, shuning uchun alohida ko'rib chiqishni talab qiladi.

### Guruh E — Tuzilma va tartib

**Qamrov: ~3 fayl.** F-10, F-13, F-16.

1. `GoogleDrive\Extensions\` → `Configs\` ga ko'chirish.
2. AslBelgi handler tartibi bo'yicha qaror (almashtirish yoki izoh bilan hujjatlashtirish).
3. `CurrencyRateImportService` ni `BaseService.ExecuteInTransactionAsync` ga o'tkazish.

---

## Holat tasdig'i

Audit paytida kod o'zgartirilmadi. `dotnet build` — **0 error, 0 warning**.
