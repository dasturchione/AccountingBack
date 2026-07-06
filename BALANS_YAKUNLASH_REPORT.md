# Balans Yakunlash Report

## 1. Bajarilgan xavfsiz o'zgarishlar

- O'chirildi: `CounterpartyBankAccountSelectListDto`, `CurrencyRevaluationCancelDto`, `CurrencyRevaluationConfirmDto`, `InventoryRegisterBalanceCreateDto`, `InventoryRegisterBalanceUpdateDto`, `ReportSectionDto`, `SaleDocLineDto`, `UserOrganizationAssignDto`.
- O'chirildi: `src/Infrastructure/Persistence/Scripts/10_acc/1011_create_table_acc_posting_alias.sql`.
- `AppDbContext`ga qo'shildi: `BankOperationLine`, `ProductPriceType`, `ProductTableStatus` uchun `DbSet<>`.
- `Scaffold.txt` credential'lari placeholder bilan almashtirildi va `.gitignore`ga qo'shildi.
- CREATE ichiga singdirildi va o'chirildi:
  - `01_cmn/0113_add_currency_revaluation_document_type.sql` -> `01_cmn/0112_create_cmn_document_type.sql`
  - `04_inv/0415_alter_table_inv_product.sql` -> `04_inv/0402_create_inv_product.sql`
  - `10_acc/1012_add_currency_revaluation_aliases.sql` -> `10_acc/1009_create_acc_posting_alias.sql`, `10_acc/1010_create_acc_posting_alias_translation.sql`
  - `10_acc/1014_add_currency_revaluation_resolve_rules.sql` -> `10_acc/1006_create_acc_account_resolve_rule.sql`
- CREATE ichidagi redundant `ALTER TABLE` olib tashlandi:
  - `00_sys/0001_create_sys_module_sub_group.sql`
  - `00_sys/0002_create_sys_module.sql`
  - `10_acc/1006_create_acc_account_resolve_rule.sql`

## 2. Cache tozalash

- Oldin: `368.51 MB`
- Keyin: `31.75 MB`
- Bo'shatildi: `336.77 MB`
- O'chirildi: `bin/`, `obj/`, `*.log`, `*.tmp`, `TestResults/`, bo'sh build artefaktlari.
- `\.vs\` to'liq o'chmadi: `FileContentIndex/*.vsidx` fayllari boshqa process tomonidan lock holatda.

## 3. Build tekshiruvi

- `dotnet build src/Infrastructure/Infrastructure.csproj` -> muvaffaqiyatli (`0 warning`, `0 error`)
- Build 3 marta tekshirildi:
  - DTO/script o'chirishdan keyin
  - cache tozalashdan keyin
  - SQL konsolidatsiyadan keyin

## 4. Hozirgi script holati

- Jami `.sql` fayl: `107`
- `CREATE TABLE` soni: `104`
- Qolgan alohida `ALTER` script: `10_acc/1015_alter_table_acc_posting_rule_line.sql`

## 5. Qolgan isbotlangan schema mismatch'lar

### `bank_operation`
- Script: `05_bank/0502_create_bank_operation.sql`
- Entity: `src/Domain/Entities/Bank/BankOperation.cs`
- Muammo:
  - `payment_purpose_id` scriptda `smallint null`
  - entity'da `short` (`not null`)
  - entity'da FK bor (`[ForeignKey("PaymentPurposeId")]`)
  - scriptda `acc_payment_purpose`ga FK yo'q
- Scriptning o'zida izoh bor: bu FK/keyinroq `00_sys/0000.sql` orqali qo'yilishi kerak bo'lgan.

### `cash_operation`
- Script: `06_cash/0602_create_cash_operation.sql`
- Entity: `src/Domain/Entities/Cash/CashOperation.cs`
- Muammo:
  - `payment_purpose_id` scriptda `smallint null`
  - entity'da `short` (`not null`)
  - entity'da FK bor (`[ForeignKey("PaymentPurposeId")]`)
  - scriptda `acc_payment_purpose`ga FK yo'q
- Scriptning o'zida izoh bor: bu FK/keyinroq `00_sys/0000.sql` orqali qo'yilishi kerak bo'lgan.

### `acc_posting_rule_line`
- Script: `10_acc/1008_create_acc_posting_rule_line.sql`
- Entity: `src/Domain/Entities/Acc/PostingRuleLine.cs`
- Muammo:
  - script ustunlari: `debit_alias`, `credit_alias` (`varchar`)
  - entity ustunlari: `debit_alias_id`, `credit_alias_id` (`smallint`, FK -> `acc_posting_alias`)
  - bu farqni yopadigan yagona script: `10_acc/1015_alter_table_acc_posting_rule_line.sql`

## 6. Nega `1015_alter_table_acc_posting_rule_line.sql` hozircha singdirilmadi

- `1015` migratsiyasi `debit_alias`/`credit_alias` qiymatlarini `acc_posting_alias.code` bilan map qiladi.
- Lekin `1008_create_acc_posting_rule_line.sql` seedida quyidagi kod bor:
  - `('20', '6', '1', 'TaxAuthority', 'PaymentAccount', 'Total', 't')`
- `TaxAuthority` kodi repository ichida boshqa hech qayerda yo'q.
- `acc_posting_alias` seedlarida ham `TaxAuthority` yo'q.
- Demak, `1015`ni CREATE ichiga ko'r-ko'rona singdirish uchun:
  - yo bu qatorni o'chirish,
  - yo unga yangi alias o'ylab qo'shish,
  - yo mavjud aliaslardan biriga taxminan map qilish
  kerak bo'ladi.
- Bu uchalasining ham biznes oqibati bor; ular 100% xavfsiz deb isbotlanmagan.

## 7. Nega to'liq FK-safe blank-DB run bundle hali chiqarilmadi

- CREATE-level sikl mavjud:
  - `org_organization.tenant_id -> platform_tenant`
  - `platform_tenant.owner_user_id -> sys_user`
  - `sys_user.organization_id -> org_organization`
  - `sys_user.role_id -> sys_role`
  - `sys_role.organization_id -> org_organization`
- Shuning uchun oddiy `CREATE TABLE ... REFERENCES ...` ketma-ketligida barcha jadvallarni `ALTER`siz va FK xatosiz yaratishning o'zi hozircha imkonsiz.
- Ustiga-ustak `bank_operation` va `cash_operation` scriptlari yo'qolgan `00_sys/0000.sql`ga tayanadi.

## 8. Scaffold buyruqlari

User secret orqali connection string saqlash:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=<DB_HOST>;Port=<DB_PORT>;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>" --project src/Presentation/WebApi/WebApi.csproj
```

Scaffold:

```powershell
dotnet ef dbcontext scaffold "Host=<DB_HOST>;Port=<DB_PORT>;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>" Npgsql.EntityFrameworkCore.PostgreSQL --project src/Infrastructure/Infrastructure.csproj --startup-project src/Infrastructure/Infrastructure.csproj --output-dir Persistence/Generated/Entities --context-dir Persistence/Generated --context AppDbContext --data-annotations --no-onconfiguring --force
```

## 9. Keyingi qaror talab qiladigan punktlar

- `10_acc/1015_alter_table_acc_posting_rule_line.sql` uchun `TaxAuthority` biznes qarori
- `05_bank/0502` va `06_cash/0602` uchun `payment_purpose_id` FK/NOT NULL siyosati
- `00_sys/0000.sql` yo'qolganligi sababli uning o'rniga nima bo'lishi
- FK sikllarni qanday buzish:
  - vaqtincha nullable FK,
  - deferrable FK,
  - ikki bosqichli CREATE+ALTER,
  - yoki seed/model qayta qurish
