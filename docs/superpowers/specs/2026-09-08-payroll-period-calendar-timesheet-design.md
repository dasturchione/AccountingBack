# Payroll period calendar and editable timesheet design

## Scope

Replace manually entered payroll-period totals with a calendar-based workday definition and a single fixed number of work hours per selected day. Extend payroll timesheets so that every employee has a persisted status for every calendar day, the daily statuses are editable directly in the timesheet grid, and all employee totals are calculated from those statuses.

The feature covers:

- create, view, and update payroll periods;
- automatic calculation of period norm workdays and norm work hours;
- blocking period updates after the period is used by a timesheet;
- HR-calendar-based initial timesheet templates;
- persisted and editable employee day statuses;
- automatic employee-level timesheet totals;
- frontend forms, grids, validation, API contracts, backend services, entities, and database migration.

Payroll calculation, posting, and payment formulas outside the source of timesheet totals remain unchanged.

## Confirmed business rules

1. A payroll period belongs to one organization and one year/month.
2. The user selects the normative work dates from a calendar and enters the work hours for one day.
3. `NormWorkDays` is the number of selected unique dates.
4. `NormWorkHours` is `NormWorkDays * DailyWorkHours`, rounded to two decimal places.
5. The period still spans the complete calendar month. `StartDate` is the first day of the month and `EndDate` is the last day of the month; selected work dates do not shorten the timesheet calendar.
6. Every timesheet employee line receives `NormWorkDays` and `NormWorkHours` from the selected payroll period, not from the employee's HR work schedule.
7. HR employment, work schedules, and absence documents produce the initial daily-status template.
8. After the template is loaded, the user can change any daily status in the timesheet grid.
9. A day with status `WORKED` always contributes exactly the period's `DailyWorkHours`. Daily worked hours are not separately editable.
10. Employee totals are derived from persisted daily statuses. The client cannot submit authoritative aggregate totals.
11. Once a period is referenced by any active timesheet, the period cannot be updated. The document status of that timesheet does not matter.
12. A saved timesheet is a snapshot. Later HR schedule or absence changes do not silently rewrite its saved daily statuses.
13. Existing document-state rules continue to apply: daily statuses can be edited only while the timesheet itself is editable.

## Payroll period data model

### Changes to `pay_period`

Add:

| Column | Type | Rules |
|---|---|---|
| `daily_work_hours` | `numeric(8,4)` | Required, greater than 0 and no greater than 24 |

Existing `norm_work_days` and `norm_work_hours` columns remain. They are persisted derived values for compatibility with existing payroll queries and reports; the backend is their only writer.

### New `pay_period_work_day`

| Column | Type | Rules |
|---|---|---|
| `id` | `bigint identity` | Primary key |
| `organization_id` | `integer` | Required, references `org_organization(id)` |
| `period_id` | `bigint` | Required, references `pay_period(id)` with cascade delete |
| `work_date` | `date` | Required |

Constraints and indexes:

- unique `(period_id, work_date)`;
- index on `(organization_id, work_date)`;
- service validation requires every date to be inside the period's year/month;
- at least one selected date is required.

`organization_id` is retained on the child row to follow the existing tenant-scoped payroll model and to support efficient organization filtering.

## Payroll period API

### Create and update request

Both create and update use the same writable shape:

```json
{
  "year": 2026,
  "month": 9,
  "dailyWorkHours": 8,
  "workDates": ["2026-09-01", "2026-09-02", "2026-09-03"]
}
```

The API does not accept `NormWorkDays` or `NormWorkHours` as inputs.

Endpoints:

- `POST /api/payroll/periods` creates a period;
- `PUT /api/payroll/periods/{id}` updates an unused open period;
- existing list and detail endpoints remain;
- close and reopen endpoints remain.

### Response

The period detail includes:

```json
{
  "id": 1,
  "year": 2026,
  "month": 9,
  "startDate": "2026-09-01",
  "endDate": "2026-09-30",
  "dailyWorkHours": 8,
  "workDates": ["2026-09-01", "2026-09-02", "2026-09-03"],
  "normWorkDays": 3,
  "normWorkHours": 24,
  "status": "OPEN",
  "isUsedInTimesheet": false,
  "canEdit": true,
  "editBlockedReason": null
}
```

The list response may omit `workDates`, but includes `DailyWorkHours`, `IsUsedInTimesheet`, `CanEdit`, and `EditBlockedReason`. The frontend loads detail before opening view/edit mode.

### Validation and locking

Create and update validate:

- year is between 2000 and 2200;
- month is between 1 and 12;
- `DailyWorkHours` is greater than 0 and no greater than 24;
- `WorkDates` is not empty and contains no duplicates;
- every work date belongs to the submitted year/month;
- organization/year/month remains unique.

Update additionally requires:

- the period belongs to the current organization;
- status is `OPEN`;
- no active `pay_timesheet` references the period.

The backend is authoritative. The disabled frontend button is only a usability feature. Period update and timesheet creation both lock or serialize access to the same period row and recheck eligibility inside the transaction, preventing a timesheet from being created concurrently with a period update.

## Payroll period frontend

The approved modal has three modes:

- **Create** — editable year, month, daily work hours, and calendar;
- **Edit** — the same controls populated from period detail;
- **View** — read-only values and selected dates;
- **Locked view** — read-only view with the backend's reason that a used or closed period cannot be edited.

The calendar supports direct day toggling and clear visual states for selected workdays, unselected days, today, and days outside the selected month. A summary area updates immediately:

- selected day count;
- one-day work hours;
- calculated total norm hours.

Changing year or month clears dates that no longer belong to the selected month and proposes that month's Monday-Friday dates. The proposal is only a convenience; the user remains responsible for holidays and special workdays. Editing an existing period never replaces saved selections automatically.

The list page adds View and Edit actions. Edit is disabled when `CanEdit` is false, and its tooltip displays `EditBlockedReason`.

## Timesheet daily data model

### New `pay_timesheet_line_day`

One row is stored for every calendar date for every employee line in the timesheet.

| Column | Type | Rules |
|---|---|---|
| `id` | `bigint identity` | Primary key |
| `organization_id` | `integer` | Required, references `org_organization(id)` |
| `timesheet_line_id` | `bigint` | Required, references `pay_timesheet_line(id)` with cascade delete |
| `work_date` | `date` | Required |
| `source_status_code` | `varchar(50)` | Required; status originally produced by the HR calendar |
| `source_absence_id` | `bigint` | Optional reference to the HR absence that produced the template status |
| `source_schedule_id` | `bigint` | Optional reference to the HR work schedule used by the template |
| `source_absence_type_id` | `smallint` | Optional reference to the absence type originally produced by HR |
| `status_code` | `varchar(50)` | Required; current saved status |
| `absence_type_id` | `smallint` | Optional reference to the selected HR absence type |
| `timesheet_category` | `varchar(20)` | Optional saved category: `LEAVE`, `SICK`, or `ABSENT` |

Constraints and indexes:

- unique `(timesheet_line_id, work_date)`;
- index on `(organization_id, work_date)`;
- fixed statuses require `absence_type_id = null`;
- an absence status requires a valid allowed absence type and snapshots that type's code and timesheet category;
- every line must contain exactly one row for every date from period `StartDate` through `EndDate`.

The original HR source columns support traceability. `StatusCode`, `AbsenceTypeId`, and `TimesheetCategory` represent the user-editable snapshot. A manual change does not modify the underlying HR schedule or absence document. Aggregate calculations use the saved category, so later catalogue changes cannot reclassify a saved timesheet silently.

## Attendance status options

The editor obtains allowed options from a payroll endpoint instead of relying only on a hardcoded frontend union:

- fixed options: `WORKED`, `PLANNED_WORK`, `DAY_OFF`, and `NOT_EMPLOYED`;
- active HR absence types, each with its code, localized name, `AbsenceTypeId`, and timesheet category (`LEAVE`, `SICK`, or `ABSENT`).

Suggested endpoint:

`GET /api/payroll/timesheets/attendance-status-options`

Example item:

```json
{
  "code": "SICK_LEAVE",
  "name": "Kasallik ta'tili",
  "kind": "ABSENCE",
  "absenceTypeId": 2,
  "timesheetCategory": "SICK"
}
```

The API validates each submitted option against the same source used to build this list.

## Timesheet API contract

### Save request

Aggregate work and absence totals are removed from the writable line contract. `OvertimeHours` and `Note` remain writable because overtime is not represented by a variable number of hours on a normal daily status.

```json
{
  "periodId": 1,
  "docDate": "2026-09-30",
  "note": null,
  "lines": [
    {
      "employeeId": 15,
      "overtimeHours": 0,
      "note": null,
      "days": [
        {
          "date": "2026-09-01",
          "statusCode": "WORKED",
          "absenceTypeId": null
        }
      ]
    }
  ]
}
```

Create and update run in a transaction. For every employee the backend:

1. validates employment and organization ownership;
2. validates a complete, unique set of period dates;
3. validates statuses and absence types;
4. builds the HR source template itself for a new employee line instead of trusting source metadata from the client;
5. copies period norms into the employee line;
6. calculates aggregate fields from the submitted days;
7. saves the day rows and their source snapshot;
8. saves the aggregate line values.

On update, existing daily rows are synchronized by date so their original HR source metadata is preserved; only the current status selection changes. A newly added employee receives a new backend-generated HR source snapshot. Removing an employee line deletes its daily rows through cascade delete.

The period of an existing timesheet is immutable. The update request's `PeriodId` must equal the saved period, and the frontend renders the period selector read-only in edit mode. A different period requires a new timesheet document.

### Read responses

Timesheet detail and calendar responses include the persisted day collection for document employees. HR calendar endpoints remain the source for a new, unsaved employee template. Once a timesheet has been saved, its persisted day rows take precedence over a newly recalculated HR calendar.

Each returned day contains display-ready status metadata and calculated hours:

- `WorkedHours = DailyWorkHours` only for `WORKED`, otherwise 0;
- `PlannedHours = DailyWorkHours` only for `PLANNED_WORK`, otherwise 0;
- `IsOverridden = StatusCode != SourceStatusCode` or the selected absence type differs from the source absence type.

## Aggregate calculations

For each employee line:

- `NormWorkDays = period.NormWorkDays`;
- `NormWorkHours = period.NormWorkHours`;
- `WorkedDays = count(StatusCode == WORKED)`;
- `WorkedHours = WorkedDays * period.DailyWorkHours`, rounded to two decimal places;
- `LeaveDays = count(absence type TimesheetCategory == LEAVE)`;
- `SickDays = count(absence type TimesheetCategory == SICK)`;
- `AbsentDays = count(absence type TimesheetCategory == ABSENT)`;
- `PlannedWorkDays = count(StatusCode == PLANNED_WORK)` for response summaries;
- `PlannedWorkHours = PlannedWorkDays * period.DailyWorkHours` for response summaries;
- `DAY_OFF` and `NOT_EMPLOYED` do not contribute to the totals above;
- `OvertimeHours` remains the explicitly entered nonnegative line value.

The service recalculates these values on every create or update. Client-supplied aggregate values, if sent by an older client, are ignored or rejected rather than trusted.

## Timesheet frontend interaction

The existing employee-by-day table remains the main editor.

- Each day cell is a color-coded compact status chip.
- Clicking an editable cell opens a small status picker anchored to that cell.
- Choosing an option closes the picker and immediately recalculates that employee's displayed totals.
- A marker and tooltip identify statuses that differ from the original HR template.
- A legend explains status colors.
- Norm workdays, norm work hours, worked days, worked hours, leave days, sick days, and absent days are read-only calculated columns.
- Overtime hours and line note remain editable while the document is editable.
- Posted/cancelled or otherwise non-editable documents render the same grid without interactive controls.
- Horizontal scrolling and sticky employee/summary columns are retained so a full month remains usable.

When an employee is added, the frontend loads that employee's HR calendar for the selected period and initializes all calendar days. Changing the period clears existing employee lines after a confirmation because their dates and norms are no longer valid.

## Error handling

Feature-specific localized errors cover:

- period not found or owned by another organization;
- duplicate year/month;
- empty, duplicate, or out-of-month work dates;
- invalid daily work hours;
- closed period update;
- period already used by a timesheet;
- incomplete, duplicate, or out-of-period employee days;
- unknown status or invalid absence type;
- employee not available to the organization;
- non-editable timesheet document;
- concurrent period update/timesheet creation conflict.

Backend error codes are stable so the frontend can show inline validation when appropriate and a notification for document-level conflicts.

## Migration and backward compatibility

The database rollout is additive:

1. Add nullable `daily_work_hours` to `pay_period`.
2. Create `pay_period_work_day`.
3. Preflight existing periods: `norm_work_days` must be a whole number between 1 and the number of dates in its month. Invalid legacy rows are reported and must be corrected rather than silently rounded.
4. Backfill `daily_work_hours = norm_work_hours / norm_work_days` with four-decimal precision.
5. Backfill selected dates deterministically. Prefer Monday-Friday dates in chronological order up to `norm_work_days`; if more dates are required, append remaining month dates in chronological order.
6. Make `daily_work_hours` required and add its check constraint.
7. Create `pay_timesheet_line_day`.

The exact historic selected dates cannot be reconstructed from aggregate-only legacy data. The deterministic period backfill is therefore an approximation. Existing periods already used by a timesheet are locked and preserve their stored aggregate norms. Unused periods can be corrected through the new calendar editor.

Legacy timesheet lines also contain only aggregates. They remain readable. A saved document with no day rows is identified as a legacy document and keeps its existing aggregate totals in read-only mode. It is not fabricated into editable daily history. New timesheets and newly updated eligible draft timesheets use day rows. If an old draft must become editable, the user explicitly initializes it from the current HR template, after which the backend stores a complete daily snapshot and recalculates totals.

## Verification

Backend tests must verify:

- period create derives both norm totals from selected dates and daily hours;
- period update replaces selected dates and recalculates totals;
- duplicate and out-of-month dates are rejected;
- a closed period cannot be updated;
- any active timesheet reference blocks period update;
- concurrent period update and timesheet creation cannot both commit against different period definitions;
- HR data produces the initial employee day template;
- every saved employee requires one status for each calendar date;
- `WORKED` days use the fixed period daily hours;
- work, planned, leave, sick, and absent totals are derived correctly;
- a manual daily override is persisted and does not modify HR data;
- later HR changes do not alter an already saved timesheet;
- invalid status/absence combinations are rejected;
- non-editable timesheets reject daily changes;
- legacy timesheets without day rows remain readable.

Frontend tests must verify:

- calendar day selection and deselection update period totals;
- create/edit/view/locked period modes render correctly;
- the edit action respects backend `CanEdit`;
- an employee template populates every date;
- changing a day status immediately recalculates row totals;
- a `WORKED` day always displays the fixed period daily hours;
- aggregate fields cannot be edited;
- overridden cells are visually identifiable;
- non-editable documents disable status changes;
- request mapping sends daily rows and does not send authoritative aggregates.
