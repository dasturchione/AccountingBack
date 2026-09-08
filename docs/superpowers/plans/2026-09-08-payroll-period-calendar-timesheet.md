# Payroll Period Calendar and Editable Timesheet Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build calendar-defined payroll periods and editable, persisted employee daily statuses whose timesheet totals are always calculated by the backend.

**Architecture:** Add period-workday and timesheet-line-day child entities while retaining aggregate columns for current payroll consumers. Pure calculators own derived totals; backend services snapshot HR templates, synchronize children transactionally, and expose edit-lock/status-option contracts. React keeps daily rows in Formik state and recalculates locally only for immediate display.

**Tech Stack:** .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL, FluentValidation, xUnit, React 19, TypeScript 6, Formik, Yup, TanStack Query, Ant Design 6, Tailwind CSS.

**Spec:** `docs/superpowers/specs/2026-09-08-payroll-period-calendar-timesheet-design.md`

## Global Constraints

- A period spans its complete month; selected dates define normative workdays only.
- `NormWorkDays = selected unique dates` and `NormWorkHours = round(NormWorkDays * DailyWorkHours, 2)`.
- `WORKED` contributes the period's fixed `DailyWorkHours`; per-day hours are not editable.
- HR data initializes new lines, but saved timesheets are independent snapshots.
- Every employee's norms come from `pay_period`, never from the employee HR schedule.
- Backend code derives all aggregate values and never trusts aggregate client inputs.
- Any active timesheet reference blocks period update regardless of document status.
- The two folders are separate Git repositories. Never stage the user's modified `frontend/package-lock.json` or unrelated untracked backend documents.

---

### Task 1: Add database and domain persistence

**Files:**
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1618_add_period_calendar_and_timesheet_days.sql`
- Create: `backend/src/Domain/Entities/Pay/PayPeriodWorkDay.cs`
- Create: `backend/src/Domain/Entities/Pay/PayTimesheetLineDay.cs`
- Create: `backend/tests/IntegrationTests/PayrollCalendarSchemaContractTests.cs`
- Modify: `backend/tests/IntegrationTests/IntegrationTests.csproj`
- Modify: `backend/src/Domain/Entities/Pay/PayPeriod.cs`
- Modify: `backend/src/Domain/Entities/Pay/PayTimesheetLine.cs`
- Modify: `backend/src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs`

**Interfaces:**
- Produces: `PayPeriod.DailyWorkHours`, `PayPeriod.WorkDays`, `PayTimesheetLine.Days`, and corresponding `DbSet` properties.
- Consumes: existing payroll parent tables and HR absence/schedule keys.

- [ ] **Step 1: Write a failing SQL contract test**

Embed the new script in the integration-test project and assert its required schema:

```csharp
[Fact]
public void Migration_DefinesPayrollCalendarConstraints()
{
    var sql = ReadEmbedded("1618_add_period_calendar_and_timesheet_days.sql");
    Assert.Contains("daily_work_hours numeric(8,4)", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("create table pay_period_work_day", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("unique (period_id, work_date)", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("create table pay_timesheet_line_day", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("unique (timesheet_line_id, work_date)", sql, StringComparison.OrdinalIgnoreCase);
}

private static string ReadEmbedded(string suffix)
{
    var assembly = typeof(PayrollCalendarSchemaContractTests).Assembly;
    var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
    using var stream = assembly.GetManifestResourceStream(name)!;
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
```

- [ ] **Step 2: Run it and confirm failure**

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter PayrollCalendarSchemaContractTests
```

Expected: failure because the script is missing.

- [ ] **Step 3: Implement the additive migration**

The script must add `pay_period.daily_work_hours numeric(8,4)`, preflight that legacy `norm_work_days` values are whole and fit their month, then backfill:

```sql
update pay_period
set daily_work_hours = round(norm_work_hours / norm_work_days, 4);
```

Create `pay_period_work_day` and deterministically backfill the first N Monday-Friday dates, then remaining chronological dates if N is larger. Create `pay_timesheet_line_day` with source/current status fields, source HR IDs, `absence_type_id`, and snapshotted `timesheet_category`. Add tenant/date indexes, unique child/date keys, cascade deletes, and daily-hours range checks exactly as defined by the spec.

- [ ] **Step 4: Add the mapped entities**

Use these core shapes with matching `[Table]`, `[Column]`, `[Index]`, `[ForeignKey]`, precision, and length attributes:

```csharp
public partial class PayPeriodWorkDay
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public long PeriodId { get; set; }
    public DateOnly WorkDate { get; set; }
    public virtual PayPeriod Period { get; set; } = null!;
}

public partial class PayTimesheetLineDay
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public long TimesheetLineId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string SourceStatusCode { get; set; } = null!;
    public long? SourceAbsenceId { get; set; }
    public long? SourceScheduleId { get; set; }
    public short? SourceAbsenceTypeId { get; set; }
    public string StatusCode { get; set; } = null!;
    public short? AbsenceTypeId { get; set; }
    public string? TimesheetCategory { get; set; }
    public virtual PayTimesheetLine TimesheetLine { get; set; } = null!;
}
```

- [ ] **Step 5: Verify and commit**

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter PayrollCalendarSchemaContractTests
dotnet build Accounting.slnx --no-restore
git add src/Infrastructure/Persistence/Scripts/16_pay/1618_add_period_calendar_and_timesheet_days.sql src/Domain/Entities/Pay/PayPeriodWorkDay.cs src/Domain/Entities/Pay/PayTimesheetLineDay.cs src/Domain/Entities/Pay/PayPeriod.cs src/Domain/Entities/Pay/PayTimesheetLine.cs src/Infrastructure/Persistence/AppDbContext/AppDbContext.cs tests/IntegrationTests/IntegrationTests.csproj tests/IntegrationTests/PayrollCalendarSchemaContractTests.cs
git commit -m "feat: add payroll calendar persistence"
```

Expected: focused test and build pass.

### Task 2: Implement backend period calendar behavior

**Files:**
- Create: `backend/src/Application/Features/Pay/Periods/Services/PayrollPeriodCalendarCalculator.cs`
- Create: `backend/tests/UnitTests/PayrollPeriodCalendarTests.cs`
- Modify: `backend/src/Application/Features/Pay/Periods/DTOs/PayrollPeriodDtos.cs`
- Modify: `backend/src/Application/Features/Pay/Periods/Services/IPayrollPeriodService.cs`
- Modify: `backend/src/Application/Features/Pay/Periods/Services/PayrollPeriodService.cs`
- Modify: `backend/src/Application/Features/Pay/Validators/PayrollValidators.cs`
- Modify: `backend/src/Presentation/WebApi/Controllers/Pay/PayrollPeriodController.cs`

**Interfaces:**
- Produces: calendar save DTOs, derived totals, edit-lock response fields, and `PUT /api/payroll/periods/{id}`.
- Consumes: Task 1 period entity and workday collection.

- [ ] **Step 1: Write failing calculator and policy tests**

```csharp
[Fact]
public void Calculate_DeduplicatesSortsAndDerivesTotals()
{
    var result = PayrollPeriodCalendarCalculator.Calculate(
        2026, 9, 8m, [new(2026, 9, 3), new(2026, 9, 1), new(2026, 9, 3)]);
    Assert.Equal([new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)], result.WorkDates);
    Assert.Equal(2m, result.NormWorkDays);
    Assert.Equal(16m, result.NormWorkHours);
}

[Theory]
[InlineData("OPEN", false, true)]
[InlineData("OPEN", true, false)]
[InlineData("CLOSED", false, false)]
public void EditPolicy_RequiresOpenAndUnused(string status, bool used, bool expected) =>
    Assert.Equal(expected, PayrollPeriodEditPolicy.CanEdit(status, used));
```

- [ ] **Step 2: Run and confirm failure**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollPeriodCalendarTests
```

Expected: compile failure because calculator/policy types are absent.

- [ ] **Step 3: Implement contracts, calculator, and validation**

```csharp
public abstract class PayrollPeriodSaveDto
{
    public short Year { get; set; }
    public short Month { get; set; }
    public decimal DailyWorkHours { get; set; }
    public List<DateOnly> WorkDates { get; set; } = [];
}

public sealed record PayrollPeriodCalendarResult(
    IReadOnlyList<DateOnly> WorkDates, decimal NormWorkDays, decimal NormWorkHours);
```

Calculator uses `Distinct().Order()` and `decimal.Round(count * dailyHours, 2)`. FluentValidation requires year 2000-2200, month 1-12, daily hours `(0,24]`, nonempty unique dates, and every date in the submitted month. Detail adds `DailyWorkHours`, sorted `WorkDates`, `IsUsedInTimesheet`, `CanEdit`, and `EditBlockedReason`.

- [ ] **Step 4: Implement create/update/list/detail**

Create and update derive totals and replace workday children. Update requires current organization, `OPEN`, no active timesheet, and unique organization/year/month excluding itself. Both period update and timesheet create must serialize on the period row and recheck eligibility inside their transactions.

Add:

```csharp
Task<Result> UpdateAsync(long id, PayrollPeriodUpdateDto dto, CancellationToken ct = default);

[HttpPut("{id:long}")]
[ModuleAuthorize(PermissionCodeConst.PayrollPeriodManage)]
public async Task<IResult> UpdateAsync(long id, PayrollPeriodUpdateDto dto, CancellationToken ct)
```

- [ ] **Step 5: Verify and commit**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollPeriodCalendarTests
dotnet build Accounting.slnx --no-restore
git add src/Application/Features/Pay/Periods src/Application/Features/Pay/Validators/PayrollValidators.cs src/Presentation/WebApi/Controllers/Pay/PayrollPeriodController.cs tests/UnitTests/PayrollPeriodCalendarTests.cs
git commit -m "feat: manage calendar based payroll periods"
```

Expected: tests/build pass and list queries do not perform one query per period.

### Task 3: Implement frontend period modal and actions

**Files:**
- Create: `frontend/src/modules/payroll/pages/periods/utils/periodCalendar.ts`
- Create: `frontend/src/modules/payroll/pages/periods/hooks/useUpdatePayrollPeriod.ts`
- Create: `frontend/tests/payroll-period-calendar.test.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/types/type.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/types/form.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/types/schema.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/constants/endpoints.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/services/payrollPeriodService.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/hooks/index.ts`
- Modify: `frontend/src/modules/payroll/pages/periods/components/PayrollPeriodModal.tsx`
- Modify: `frontend/src/modules/payroll/pages/periods/screens/PayrollPeriodListPage.tsx`
- Modify: `frontend/src/config/locales/uz.json`, `ru.json`, `en.json`

**Interfaces:**
- Produces: create/edit/view/locked modal, frontend calendar helpers, and update mutation.
- Consumes: Task 2 API.

- [ ] **Step 1: Write failing pure helper tests**

```typescript
test("totals use unique selected dates", () => {
  assert.deepEqual(calculatePeriodTotals(["2026-09-01", "2026-09-01", "2026-09-02"], 8), {
    normWorkDays: 2,
    normWorkHours: 16,
  });
});

test("toggle keeps dates sorted", () => {
  assert.deepEqual(toggleWorkDate(["2026-09-03"], "2026-09-01"), ["2026-09-01", "2026-09-03"]);
});
```

- [ ] **Step 2: Run and confirm failure**

```powershell
node --experimental-strip-types --test tests/payroll-period-calendar.test.ts
```

Expected: module-not-found failure.

- [ ] **Step 3: Implement types, helpers, schema, and API**

```typescript
export interface PayrollPeriodForm {
  year: number | null;
  month: number | null;
  dailyWorkHours: number | null;
  workDates: string[];
}

export const calculatePeriodTotals = (dates: string[], hours: number | null) => {
  const normWorkDays = new Set(dates).size;
  return { normWorkDays, normWorkHours: Math.round(normWorkDays * (hours ?? 0) * 100) / 100 };
};
```

Add weekday proposal, date toggle, backend response fields, Yup parity validation, update URL/service, and a mutation that invalidates list/detail/lookup queries.

- [ ] **Step 4: Build the approved modal and list actions**

Use this modal contract:

```typescript
interface Props {
  open: boolean;
  mode: "create" | "edit" | "view";
  periodId?: number | null;
  onClose: () => void;
}
```

Use Ant Design `Calendar` for direct date toggles and a live summary for selected count, fixed daily hours, and total norm hours. Create mode proposes weekdays; edit loads and preserves saved dates; view/locked modes are read-only and show the backend reason. Add View/Edit actions to the list and disable Edit when `canEdit` is false. Add all new text in Uzbek, Russian, and English.

- [ ] **Step 5: Verify and commit without staging the existing lockfile change**

```powershell
node --experimental-strip-types --test tests/payroll-period-calendar.test.ts
npm run build
npx eslint src/modules/payroll/pages/periods tests/payroll-period-calendar.test.ts
git add src/modules/payroll/pages/periods src/config/locales/uz.json src/config/locales/ru.json src/config/locales/en.json tests/payroll-period-calendar.test.ts
git commit -m "feat: add payroll period calendar modal"
```

Expected: tests/build/lint pass; `package-lock.json` stays unstaged.

### Task 4: Define backend daily timesheet calculations

**Files:**
- Create: `backend/src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetDayCalculator.cs`
- Create: `backend/tests/UnitTests/PayrollTimesheetDayCalculatorTests.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/DTOs/PayrollTimesheetDtos.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/DTOs/PayrollTimesheetCalendarDtos.cs`
- Modify: `backend/src/Application/Features/Pay/Validators/PayrollValidators.cs`

**Interfaces:**
- Produces: daily save/read DTOs, status option DTO, resolver, and aggregate calculator.
- Consumes: fixed HR status and timesheet-category constants.

- [ ] **Step 1: Write failing aggregate and resolver tests**

```csharp
[Fact]
public void Calculate_UsesFixedHoursAndAbsenceCategories()
{
    var totals = PayrollTimesheetDayCalculator.Calculate(8m,
    [
        new("WORKED", null), new("WORKED", null), new("PLANNED_WORK", null),
        new("SICK_LEAVE", "SICK"), new("ANNUAL_LEAVE", "LEAVE"),
        new("UNEXCUSED_ABSENCE", "ABSENT"), new("DAY_OFF", null)
    ]);
    Assert.Equal((2m, 16m, 1m, 8m, 1m, 1m, 1m),
        (totals.WorkedDays, totals.WorkedHours, totals.PlannedWorkDays,
         totals.PlannedWorkHours, totals.LeaveDays, totals.SickDays, totals.AbsentDays));
}

[Fact]
public void Resolver_RejectsAbsenceIdForWorked() =>
    Assert.Throws<ArgumentException>(() =>
        PayrollAttendanceStatusResolver.Resolve("WORKED", 2, []));
```

- [ ] **Step 2: Run and confirm failure**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollTimesheetDayCalculatorTests
```

Expected: compile failure.

- [ ] **Step 3: Implement calculator, resolver, and contracts**

```csharp
public sealed record PayrollTimesheetDayValue(string StatusCode, string? TimesheetCategory);
public sealed record PayrollTimesheetDayTotals(
    decimal WorkedDays, decimal WorkedHours,
    decimal PlannedWorkDays, decimal PlannedWorkHours,
    decimal LeaveDays, decimal SickDays, decimal AbsentDays);

public sealed class PayrollTimesheetDaySaveDto
{
    public DateOnly Date { get; set; }
    public string StatusCode { get; set; } = null!;
    public short? AbsenceTypeId { get; set; }
}
```

Remove writable norm/work/absence aggregates from line save DTOs, retain `EmployeeId`, `OvertimeHours`, `Note`, and `Days`, and retain aggregates in read DTOs. Define dynamic status options as four fixed entries plus HR absence entries.

- [ ] **Step 4: Implement validation and verify**

Require a nonempty unique day list, nonempty status codes of at most 50 characters, and nonnegative overtime. Database-aware full-month coverage/status-option checks remain in the service.

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollTimesheetDayCalculatorTests
dotnet build Accounting.slnx --no-restore
git add src/Application/Features/Pay/Timesheets/DTOs src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetDayCalculator.cs src/Application/Features/Pay/Validators/PayrollValidators.cs tests/UnitTests/PayrollTimesheetDayCalculatorTests.cs
git commit -m "feat: define daily timesheet calculations"
```

Expected: pass.

### Task 5: Persist timesheet snapshots and expose status options

**Files:**
- Modify: `backend/src/Application/Features/Pay/Timesheets/Services/IPayrollTimesheetService.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetService.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/DTOs/PayrollTimesheetCalendarDtos.cs`
- Modify: `backend/src/Presentation/WebApi/Controllers/Pay/PayrollTimesheetController.cs`
- Test: `backend/tests/UnitTests/PayrollTimesheetDayCalculatorTests.cs`
- Test: `backend/tests/IntegrationTests/PayrollPeriodTimesheetConcurrencyTests.cs`

**Interfaces:**
- Produces: `GET /api/payroll/timesheets/attendance-status-options` and persisted day projection.
- Consumes: Tasks 1 and 4 persistence/contracts.

- [ ] **Step 1: Add failing coverage-policy tests**

```csharp
[Fact]
public void Coverage_RequiresEveryPeriodDateExactlyOnce()
{
    var expected = PayrollTimesheetDayCoverage.CreateExpected(new(2026, 9, 1), new(2026, 9, 3));
    Assert.True(PayrollTimesheetDayCoverage.IsComplete(expected, [new(2026, 9, 1), new(2026, 9, 2), new(2026, 9, 3)]));
    Assert.False(PayrollTimesheetDayCoverage.IsComplete(expected, [new(2026, 9, 1), new(2026, 9, 3)]));
}
```

- [ ] **Step 2: Run and confirm failure**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollTimesheetDayCalculatorTests
```

Expected: missing coverage helper.

- [ ] **Step 3: Implement option endpoint and day validation**

Add:

```csharp
Task<Result<IReadOnlyList<PayrollAttendanceStatusOptionDto>>>
    GetAttendanceStatusOptionsAsync(CancellationToken ct = default);
```

The controller route is `attendance-status-options`. The service returns localized fixed options plus active absence types. Fixed statuses require null `AbsenceTypeId`; absence selections require matching code/id and snapshot `TimesheetCategory`.

- [ ] **Step 4: Replace aggregate-based line building**

For each submitted employee, require exactly one unique entry for every period date. On new lines, rebuild the HR calendar on the backend and persist source status and source HR IDs rather than accepting them from the browser. On updates, synchronize by date and preserve existing source metadata. Resolve current statuses, calculate totals with Task 4, copy period norms, and save all children in the transaction.

Reject changing an existing timesheet's period. Serialize timesheet creation with period update using the same period-row lock/recheck.

- [ ] **Step 5: Project persisted snapshots and preserve legacy reads**

For saved days return fixed worked/planned hours and:

```csharp
IsOverridden = day.StatusCode != day.SourceStatusCode ||
               day.AbsenceTypeId != day.SourceAbsenceTypeId;
```

Saved rows take precedence over recalculated HR data. Old lines without child rows keep stored aggregates, return `IsLegacy = true`, and remain read-only until explicitly initialized. Add this required service/controller contract:

```csharp
Task<Result> InitializeDaysAsync(long id, CancellationToken ct = default);

[HttpPost("{id:long}/initialize-days")]
[ModuleAuthorize(PermissionCodeConst.PayrollTimesheetUpdate)]
public async Task<IResult> InitializeDaysAsync(long id, CancellationToken ct)
```

It accepts draft legacy documents only, generates complete backend HR snapshots for all existing employee lines, recalculates aggregates, and commits atomically.

- [ ] **Step 6: Add a period-row-lock concurrency integration test**

Create a PostgreSQL-backed test that inserts one period, opens transaction A and locks it with the exact SQL/helper used by `PayrollPeriodService.UpdateAsync`, then starts transaction B through the lock used by `PayrollTimesheetService.CreateAsync`:

```csharp
var firstLock = await LockPeriodAsync(connectionA, transactionA, periodId);
var secondLockTask = LockPeriodAsync(connectionB, transactionB, periodId);
await Task.Delay(200);
Assert.False(secondLockTask.IsCompleted);
await transactionA.CommitAsync();
await secondLockTask.WaitAsync(TimeSpan.FromSeconds(5));
```

Also test the business recheck: after transaction A inserts an active timesheet and commits, the waiting period update must return `PeriodUsedInTimesheet` without modifying workdays.

- [ ] **Step 7: Verify and commit**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter "PayrollTimesheetDayCalculatorTests|PayrollPeriodCalendarTests"
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "PayrollCalendarSchemaContractTests|PayrollPeriodTimesheetConcurrencyTests"
dotnet build Accounting.slnx --no-restore
git add src/Application/Features/Pay/Timesheets src/Presentation/WebApi/Controllers/Pay/PayrollTimesheetController.cs tests/UnitTests/PayrollTimesheetDayCalculatorTests.cs tests/IntegrationTests/PayrollPeriodTimesheetConcurrencyTests.cs
git commit -m "feat: persist editable timesheet day statuses"
```

Expected: all pass.

### Task 6: Implement frontend daily status state and editable grid

**Files:**
- Create: `frontend/src/modules/payroll/pages/timesheets/hooks/useAttendanceStatusOptions.ts`
- Create: `frontend/src/modules/payroll/pages/timesheets/hooks/useInitializePayrollTimesheetDays.ts`
- Create: `frontend/src/modules/payroll/pages/timesheets/components/TimesheetStatusPicker.tsx`
- Create: `frontend/tests/payroll-timesheet-days.test.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/types/type.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/types/form.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/types/schema.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/constants/endpoints.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/constants/queryKeys.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/services/payrollTimesheetService.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/hooks/index.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/utils/timesheet.ts`
- Modify: `frontend/src/modules/payroll/pages/timesheets/components/TimesheetLinesEditor.tsx`
- Modify: `frontend/src/modules/payroll/pages/timesheets/components/TimesheetCalendarView.tsx`
- Modify: `frontend/src/modules/payroll/pages/timesheets/screens/PayrollTimesheetDetailPage.tsx`
- Modify: `frontend/src/config/locales/uz.json`, `ru.json`, `en.json`

**Interfaces:**
- Produces: day arrays in Formik, lean save payload, editable status chips, immediate derived totals.
- Consumes: Task 5 APIs.

- [ ] **Step 1: Write failing day-state tests**

```typescript
test("worked totals use fixed hours", () => {
  const totals = calculateLineFromDays([
    { date: "2026-09-01", statusCode: "WORKED", absenceTypeId: null, timesheetCategory: null },
    { date: "2026-09-02", statusCode: "WORKED", absenceTypeId: null, timesheetCategory: null },
    { date: "2026-09-03", statusCode: "DAY_OFF", absenceTypeId: null, timesheetCategory: null },
  ], 8);
  assert.equal(totals.workedDays, 2);
  assert.equal(totals.workedHours, 16);
});

test("status replacement changes only one date", () => {
  const changed = replaceLineDayStatus(
    [{ date: "2026-09-01", statusCode: "WORKED", absenceTypeId: null, timesheetCategory: null }],
    "2026-09-01",
    { code: "SICK_LEAVE", name: "Sick", kind: "ABSENCE", absenceTypeId: 2, timesheetCategory: "SICK" });
  assert.equal(changed[0].statusCode, "SICK_LEAVE");
});
```

- [ ] **Step 2: Run and confirm failure**

```powershell
node --experimental-strip-types --test tests/payroll-timesheet-days.test.ts
```

Expected: missing exports/types.

- [ ] **Step 3: Implement dynamic types, API, schema, and pure mapping**

```typescript
export interface PayrollTimesheetDayForm {
  date: string;
  statusCode: string;
  sourceStatusCode?: string | null;
  absenceTypeId: number | null;
  sourceAbsenceTypeId?: number | null;
  timesheetCategory?: "LEAVE" | "SICK" | "ABSENT" | null;
  isOverridden?: boolean;
}
```

Each line owns `days`. Template/detail mapping populates it. Calculation mirrors backend rules. Save mapping sends only employee ID, overtime, note, and `{date,statusCode,absenceTypeId}`. Add status-option endpoint/query and Yup rules for nonempty unique days/statuses.

- [ ] **Step 4: Build `TimesheetStatusPicker` and refactor the grid**

Use this contract:

```typescript
interface TimesheetStatusPickerProps {
  day: PayrollTimesheetDayForm;
  options: PayrollAttendanceStatusOption[];
  dailyWorkHours: number;
  disabled?: boolean;
  onChange: (option: PayrollAttendanceStatusOption) => void;
}
```

Use an Ant Design popover/dropdown with keyboard-selectable items. Show category colors, fixed hours for `WORKED`, and an override dot/tooltip. Make Formik `line.days` the sole mutable status source. On selection, replace the day and immediately patch derived totals. Render norm/work/leave/sick/absent aggregates as text; retain editable overtime/note.

- [ ] **Step 5: Wire create, draft edit, read-only, and legacy views**

New employee selection maps HR template days into the line. Editable saved drafts use the same grid. Period selection is immutable after creation. Posted/cancelled documents use the read-only calendar. Legacy rows show stored totals and a warning with an `Initialize from HR` action backed by `POST /api/payroll/timesheets/{id}/initialize-days`; after success, invalidate detail/calendar queries and render the editable snapshot. Add Uzbek, Russian, and English labels for picker, override, fixed hours, legacy state, and initialization confirmation.

- [ ] **Step 6: Verify and commit without staging `package-lock.json`**

```powershell
node --experimental-strip-types --test tests/payroll-period-calendar.test.ts tests/payroll-timesheet-days.test.ts
npm run build
npx eslint src/modules/payroll/pages/periods src/modules/payroll/pages/timesheets tests/payroll-period-calendar.test.ts tests/payroll-timesheet-days.test.ts
git add src/modules/payroll/pages/timesheets src/config/locales/uz.json src/config/locales/ru.json src/config/locales/en.json tests/payroll-timesheet-days.test.ts
git commit -m "feat: edit employee day statuses in timesheets"
```

Expected: tests/build/lint pass and the user's lockfile remains unstaged.

### Task 7: Cross-repository verification

**Files:**
- Verify: every file changed by Tasks 1-6.
- Modify: only files with a concrete verification failure.

**Interfaces:**
- Consumes: complete backend/frontend feature.
- Produces: matching contracts and evidence-backed acceptance.

- [ ] **Step 1: Audit exact JSON names**

```text
period write: year, month, dailyWorkHours, workDates
timesheet line write: employeeId, overtimeHours, note, days
timesheet day write: date, statusCode, absenceTypeId
```

Read contracts must include period editability, saved source/current daily statuses, category/override metadata, period norms, and derived employee totals.

- [ ] **Step 2: Run backend verification**

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --filter "PayrollPeriodCalendarTests|PayrollTimesheetDayCalculatorTests"
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "PayrollCalendarSchemaContractTests|PayrollPeriodTimesheetConcurrencyTests"
dotnet build Accounting.slnx --no-restore
```

Expected: exit code 0.

- [ ] **Step 3: Run frontend verification**

```powershell
node --experimental-strip-types --test tests/payroll-period-calendar.test.ts tests/payroll-timesheet-days.test.ts
npm run build
npx eslint src/modules/payroll/pages/periods src/modules/payroll/pages/timesheets tests/payroll-period-calendar.test.ts tests/payroll-timesheet-days.test.ts
```

Expected: exit code 0.

- [ ] **Step 4: Perform manual acceptance checks**

```text
1. Create a period with three selected dates and 8 hours; result is 3 days / 24 hours.
2. Edit an unused open period and observe both totals update.
3. Create a timesheet; employee days initialize from HR.
4. Change WORKED to SICK_LEAVE; worked drops by 1 day / 8 hours and sick rises by 1.
5. Save/reopen; status and override marker persist.
6. Change source HR data; the saved document stays unchanged.
7. Try editing the used period; UI blocks it and direct PUT returns a conflict.
8. Confirm the timesheet; every daily cell and line input becomes read-only.
9. Open a legacy no-day document; old aggregates remain visible without fabricated history.
```

- [ ] **Step 5: Inspect diffs and commit only verification corrections**

```powershell
git -C backend status --short
git -C backend diff --check
git -C frontend status --short
git -C frontend diff --check
```

If corrections were required, stage only their exact paths in the owning repository and commit with `fix: align payroll calendar contracts`. If no correction was needed, create no empty commit.
