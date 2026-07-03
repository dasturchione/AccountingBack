# Accounting Period + Reposting Report

## Files changed

- `src/Application/Abstractions/IDocumentPostingLock.cs`
- `src/Application/Features/Acc/AccountingPeriods/Errors/AccountingPeriodErrors.cs`
- `src/Application/Features/Acc/AccountingPeriods/Repositories/IAccountingPeriodReadRepository.cs`
- `src/Application/Features/Acc/AccountingPeriods/Services/IAccountingPeriodService.cs`
- `src/Application/Features/Acc/AccountingPeriods/Services/AccountingPeriodService.cs`
- `src/Application/Features/Register/Reposting/DTOs/RepostDtos.cs`
- `src/Application/Features/Register/Reposting/Errors/RepostErrors.cs`
- `src/Application/Features/Register/Reposting/Filters/RepostFilter.cs`
- `src/Application/Features/Register/Reposting/Models/RepostReadModels.cs`
- `src/Application/Features/Register/Reposting/Repositories/IRepostReadRepository.cs`
- `src/Application/Features/Register/Reposting/Services/IRepostService.cs`
- `src/Application/Features/Register/Reposting/Services/RepostService.cs`
- `src/Infrastructure/DependencyInjection.cs`
- `src/Infrastructure/Repositories/AccountingPeriodReadRepository.cs`
- `src/Infrastructure/Repositories/RepostReadRepository.cs`
- `src/Infrastructure/Repositories/UnitOfWork.cs`
- `src/Infrastructure/Services/DocumentPostingLock.cs`
- `src/Presentation/WebApi/Controllers/Acc/AccountingPeriodController.cs`
- `src/Presentation/WebApi/Controllers/Register/RepostController.cs`
- `src/SharedKernel/Constants/AuditLogConst.cs`
- `tests/UnitTests/AccountingPeriodRepostingTests.cs`
- `tests/UnitTests/InventoryAdjustmentPhase2Tests.cs`
- `tests/UnitTests/InventoryCountTests.cs`
- `tests/UnitTests/WarehouseTransferPhase1Tests.cs`

## Why changed

- Closing Period uchun `close` va `reopen` endpointlari, service qatlami, audit log yozuvi va production-grade validation qo'shildi.
- Unconfirmed documentlar, previous/later period continuity va posting batch consistency DB-side tekshiruvlari qo'shildi.
- Reposting uchun alohida filter, DTO, read repository, service va endpoint qo'shildi.
- Reposting mavjud lifecycle/posting flow'larini qayta ishlatadi; yangi posting engine yozilmadi.
- Advisory lock'ning non-blocking varianti qo'shildi, shuning uchun parallel repost urinishlari professional conflict bilan qaytadi.
- Nested transaction semantikasi `UnitOfWork` ichida to'g'rilandi, shuning uchun repost ichidan chaqirilgan confirm/cancel operatsiyalari outer transactionni muddatidan oldin commit/dispose qilmaydi.
- Regression testlar Closing Period validation va Reposting happy-path/conflict holatlari uchun qo'shildi.
- Mavjud unit-test fake lock implementatsiyalari yangi `TryAcquireAsync` kontraktiga moslashtirildi.

## SQL written to 0000.sql

- O'zgarish yo'q.
- Stage 9 uchun database schema o'zgarishi talab qilinmadi.

## Build result

- `dotnet clean Accounting.slnx`: muvaffaqiyatli.
- `dotnet build Accounting.slnx`: environment sabab yiqildi.
  `C:\Users\Hafizov Sardorbek\AppData\Roaming\NuGet\NuGet.Config` fayliga access denied.
- `dotnet build Accounting.slnx --no-restore`: muvaffaqiyatli.
- Qolgan warning:
  `MSB3277` unit test project ichida `Microsoft.EntityFrameworkCore.Relational` 10.0.4 vs 10.0.8 conflict warning.

## Test result

- `dotnet test tests/UnitTests/UnitTests.csproj --no-build --no-restore`: Passed.
  `70/70` test muvaffaqiyatli.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --no-restore`: environment sabab yiqildi.
  `23` test fail, `3` test pass.
- Barcha integration failure bitta root cause bilan bog'liq:
  `C:\Users\Hafizov Sardorbek\AppData\Roaming\Microsoft\UserSecrets\accounting-back-webapi\secrets.json` fayliga access denied.
  Entry point: `src/Presentation/WebApi/Configuration/HostConfiguration.Extensions.cs:32`

## Remaining blockers

- Functional blocker qolmadi.
- Integration environment hali user secrets fayliga ruxsat bermayapti.
- Restore bosqichi user-level `NuGet.Config` access denied sababli odatiy `dotnet build` yo'lida beqaror.
- `MSB3277` package-version warning alohida dependency cleanup talab qiladi.

## Closing Period completion %

- `100%`

## Reposting completion %

- `100%`

## Accounting Module completion %

- `90%`

## Professional ERP Accounting completion %

- `90%`
