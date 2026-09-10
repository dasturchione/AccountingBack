# Payroll migration manifest

`src/Infrastructure/Persistence/Scripts/_run_order.txt` is the canonical order. `tools/Run-PayrollMigrations.ps1` applies scripts idempotently and records SHA-256 checksums and UTC timestamps in `pay_migration_history`.

Example (PowerShell):

```powershell
./tools/Run-PayrollMigrations.ps1 -ConnectionString 'Host=...;Database=...;Username=...;Password=...'
```

The runner accepts the same semicolon-separated ADO.NET connection string used by the API. By default it starts at `1633` because the current application database already has the payroll baseline (`1618`–`1632`) and some of those scripts intentionally create non-idempotent tables. Use `-ToScript 16_pay/1633_create_pay_migration_history.sql` when bringing only the history migration into an existing database; otherwise the runner continues through later manifest entries. The first run creates `pay_migration_history` and records the history-migration checksum; it does not attempt to recreate already-installed columns. Use `-FromScript 16_pay/1601_create_pay_employee.sql` for a brand-new database. Use `-FromScript 16_pay/1618_add_period_calendar_and_timesheet_days.sql` only after verifying that the baseline tables do not already exist. Use `-DryRun` to validate the manifest without applying scripts. The runner fails on a missing manifest entry, missing file, or checksum mismatch. Existing environments should first run the read-only legacy review and then baseline already-applied scripts in `pay_migration_history` before applying later scripts.
