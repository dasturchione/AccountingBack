# 1C Payroll Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring the payroll workflow closer to the standard 1C:ZUP/1C:Accounting model while preserving the current draft/post/cancel document lifecycle and the existing organization data isolation.

**Architecture:** Keep payroll documents as immutable calculation snapshots after posting. Add explicit calculation basis and source-period snapshots before extending formulas; keep statutory taxes, recalculations, and accounting postings as separate services/registries instead of embedding them in one formula. Every behavioral change is introduced with a failing unit or integration test first.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core, PostgreSQL, xUnit, React/TypeScript, existing payroll posting engine.

**Spec:** `backend/docs/hr-payroll-full-current-implementation.md` and the 1C comparison findings recorded in the task conversation.

## Global Constraints

- Do not commit or push changes; leave all work in the local working tree.
- Existing posted payroll documents must remain reproducible from their stored snapshots.
- Organization, period, document, and posting locks must remain enforced.
- No tax rate or legal assumption is hard-coded without an explicit organization configuration.
- Existing `SALARY_PRORATED` day-based documents must not silently change result; hourly proration is an explicit selectable basis.
- All production behavior changes require a test that was observed failing before implementation.

---

### Task 1: Add explicit day/hour salary proration and make daily hour edits affect payroll

**Files:**
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1621_add_pay_component_proration_basis.sql`
- Modify: `backend/src/Domain/Entities/Pay/PayComponent.cs`
- Modify: `backend/src/Application/Features/Pay/Components/DTOs/PayrollComponentDtos.cs`
- Modify: `backend/src/Application/Features/Pay/Components/Services/PayrollComponentService.cs`
- Modify: `backend/src/Application/Features/Pay/Validators/PayrollValidators.cs`
- Modify: `backend/src/SharedKernel/Constants/PayrollConst.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs:625-649`
- Create: `backend/tests/UnitTests/PayrollProrationTests.cs`
- Modify: `frontend/src/modules/payroll/constants/options.ts`
- Modify: `frontend/src/modules/settings/pages/payrollComponents/types/type.ts`
- Modify: `frontend/src/modules/settings/pages/payrollComponents/types/form.ts`
- Modify: `frontend/src/modules/settings/pages/payrollComponents/types/schema.ts`
- Modify: `frontend/src/modules/settings/pages/payrollComponents/screens/PayrollComponentAddEditPage.tsx`
- Modify: `frontend/src/config/locales/uz.json`, `ru.json`, `en.json`

**Progress:** Day/hour salary proration is persisted and uses edited worked hours when the component basis is `HOURS`; existing components default to `DAYS`. Migration `1621_add_pay_component_proration_basis.sql` was applied to local `AccountingTest`.

**Interfaces:**
- Add `ProrationBasis` with `DAYS` and `HOURS`; default existing components to `DAYS` during migration.
- The payroll calculator must use `monthlySalary * employmentRate * workedHours / normWorkHours` only for `HOURS`, and retain the current day formula for `DAYS`.
- The regular-payroll base-component check accepts a mandatory `SALARY_PRORATED` component with either basis.

- [x] Write a failing unit test proving two 8-hour worked days and one 4-hour worked day produce 20 worked hours and hourly-prorated salary uses `workedHours / normWorkHours`.
- [x] Run `dotnet test tests/UnitTests/UnitTests.csproj --filter PayrollProrationTests`; verify failure is caused by the missing basis/formula.
- [x] Add the migration, entity/DTO field, validation, UI option, and minimal calculator branch.
- [x] Run the focused unit tests and the existing payroll accounting tests.
- [x] Confirm a draft payroll created from an edited timesheet stores the edited `WorkedHours` in its line and hourly calculation line.

### Task 2: Use employee schedule norms and a real production calendar

**Progress:** Employee-specific norm resolution is now in place for new timesheet lines: active HR-calendar planned hours are used, period work-date exceptions are applied when available, existing line norms remain immutable during draft edits, and the frontend now maps the calendar response instead of overwriting employee norms with period defaults. The period modal now stores a complete production calendar (normal, holiday, transferred, and shortened dates with per-day hours), and the backend overlays it onto employee calendars before tabel snapshots are created.

**Files:**
- Modify: `backend/src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetService.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetDayCalculator.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Modify: `backend/src/Application/Features/Hr/Calendar/*`
- Create: `backend/src/Domain/Entities/Pay/PayProductionCalendarDay.cs`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1622_add_production_calendar_days.sql`
- Create: `backend/tests/UnitTests/PayrollEmployeeNormTests.cs`
- Create: `backend/tests/IntegrationTests/PayrollProductionCalendarTests.cs`

**Interfaces:**
- Preserve period norms as the selected period template, but calculate and snapshot employee-specific norm days/hours from the active schedule and production-calendar exceptions.
- Future planned days remain planned; actual worked days come from saved daily status, not the system clock.
- Payroll prorating consumes the employee snapshot stored on `pay_timesheet_line`.

- [x] Write failing tests for employee-specific norms and holiday/shortened calendar behavior.
- [x] Implement production-calendar day records, schedule norm calculation, and payroll-line snapshot consumption.
- [x] Run focused unit tests and the full backend unit test project.

### Task 3: Separate correction documents and make payment allocation source-aware (P0)

**Progress:** Correction payout mode (`SEPARATE`, `WITH_SALARY`, `WITH_ADVANCE`) is persisted and exposed in the backend/frontend. Final-payment outstanding and main report totals are restricted to the selected/eligible payroll document, preventing regular and separate correction documents from being silently merged. Migration `1623` was applied to the local `AccountingTest` database. Automatic source-line delta calculation is now shared with the recalculation processor; payment/report integration coverage remains.

**Files:**
- Modify: `backend/src/Domain/Entities/Pay/PayPayrollDoc.cs`
- Modify: `backend/src/Domain/Entities/Pay/PayPaymentLine.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/DTOs/PayrollDocumentDtos.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Modify: `backend/src/Application/Features/Pay/Payments/Services/PayrollPaymentService.cs`
- Modify: `backend/src/Application/Features/Pay/Reports/Services/PayrollReportService.cs`
- Modify: `frontend/src/modules/payroll/pages/documents/*`
- Modify: `frontend/src/modules/payroll/pages/payments/*`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1623_add_correction_payout_mode.sql`
- Create: `backend/tests/UnitTests/PayrollCorrectionTests.cs`
- Create: `backend/tests/IntegrationTests/PayrollCorrectionPaymentTests.cs`

**Interfaces:**
- Add `CorrectionPayoutMode` (`WITH_SALARY`, `WITH_ADVANCE`, `SEPARATE`) and immutable source document/revision references.
- A correction is calculated as a source-line/component delta; the original posted document is never overwritten.
- Final outstanding and payment lines are filtered by source document and payout mode; regular and separate corrections cannot be silently summed.
- Register and payslip expose regular, correction, source document and delta totals separately.

- [x] Write unit tests for payout-mode defaults and main-payroll inclusion policy.
- [x] Add schema, DTOs, payout-mode policy and source-aware payment allocation.
- [x] Add UI selection and visible source/payout mode; reject ambiguous legacy corrections.
- [x] Run focused correction/payment/report tests and verify no double-pay.

### Task 4: Implement paid leave, sick leave, overtime, night, holiday, and weekend rules (P1)

**Progress:** Paid/unpaid leave and sick status are now identified from `HrAbsenceType.IsPaid`, snapshotted into timesheet totals, and paid absence days are included in salary proration. Daily planned hours and overtime/night/holiday/weekend hour snapshots are persisted into payroll-line snapshots and aggregated in register/payslip responses. Migration `1631` adds immutable payroll-line attendance breakdown columns. Average-earnings benefit calculation remains a separate legal-policy extension.

**Files:**
- Modify: `backend/src/Domain/Entities/Pay/PayTimesheetLineDay.cs`
- Modify: `backend/src/Application/Features/Pay/Timesheets/Services/PayrollTimesheetService.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Modify: `backend/src/Domain/Entities/Pay/PayComponent.cs`
- Create: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollAbsenceCalculationService.cs`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1626_add_timesheet_special_hours.sql`
- Create: `backend/tests/UnitTests/PayrollAbsenceCalculationTests.cs`

**Interfaces:**
- Absence types expose paid/unpaid behavior, average-base method, benefit rate and effective dates.
- Paid leave/sick amounts are separate earning components and do not masquerade as worked days.
- Daily snapshots include planned, worked, night, holiday/weekend and overtime quantities with an audit source.

- [x] Write failing tests for paid leave, unpaid leave, sick leave, shortened holiday and daily special-hour aggregation.
- [x] Implement category-aware day snapshots and aggregate quantities.
- [x] Verify payroll totals and payslip breakdowns.

### Task 5: Replace generic tax components with configurable tax bases and tax registries (P1)

**Progress:** Tax registry foundation is now present: effective-dated organization tax definitions, explicit withholding/employer type, gross/taxable/net base type, exemption/limit, liability-account snapshot lines, deterministic tax calculator, CRUD API at `/api/payroll/taxes`, and migration `1627` applied locally. Regular payroll calculation snapshots active registry taxes into payroll lines and posting emits their salary-payable/salary-expense entries; corrections intentionally wait for source-tax delta logic. Employee-specific exemptions/profiles, tax reporting and statutory filing remain.

**Files:**
- Create: `backend/src/Domain/Entities/Pay/PayPayrollTaxLine.cs`
- Create: `backend/src/Domain/Entities/Pay/PayTaxDefinition.cs`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1627_add_payroll_tax_registry.sql`
- Create: `backend/src/Application/Features/Pay/Taxes/*`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Modify: `backend/src/Application/Features/Pay/Reports/Services/PayrollReportService.cs`
- Create: `backend/tests/UnitTests/PayrollTaxCalculationTests.cs`
- Create: `backend/tests/IntegrationTests/PayrollTaxPostingTests.cs`

**Interfaces:**
- Tax definitions are organization-scoped and effective-dated; employee tax profile stores residency, exemptions and limits.
- Each tax line stores base, rate, amount, period, employee, source component and liability account.
- Withholdings, employer taxes and employee net pay remain separate totals; no rate is inferred from a component name.

- [x] Write failing tests for tax base, exemption/limit and rounding.
- [x] Implement tax registry models, validation, CRUD API, and deterministic tax calculator.
- [x] Run focused and full tests.

### Task 6: Add effective-dated employment history and recalculation queue (P1)

**Files:**
- Modify: `backend/src/Domain/Entities/Pay/PayEmployment.cs`
- Modify: `backend/src/Domain/Entities/Pay/PayEmployeeComponent.cs`
- Modify: `backend/src/Application/Features/Pay/Employees/Services/PayrollEmployeeService.cs`
- Modify: `backend/src/Application/Features/Pay/Components/Services/PayrollComponentService.cs`
- Modify: `backend/src/Presentation/WebApi/Controllers/Pay/PayrollDocumentController.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/IPayrollDocumentService.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Create: `backend/src/Domain/Entities/Pay/PayPayrollRecalculation.cs`
- Create: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollRecalculationPolicy.cs`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1628_add_payroll_recalculation.sql`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1630_add_payroll_line_segments.sql`
- Create: `backend/src/Domain/Entities/Pay/PayPayrollLineSegment.cs`
- Create: `backend/src/Application/Features/Pay/PayrollDocuments/PayrollEmploymentSegmentCalculator.cs`
- Create: `backend/tests/UnitTests/PayrollEmploymentHistoryTests.cs`
- Create: `backend/tests/IntegrationTests/PayrollRecalculationSchemaContractTests.cs`

**Progress:** Queue/source revision/hash/linked `SEPARATE` correction processing is complete. Posted documents remain immutable and failed requests persist for period-close review. Mid-month employment/component input segmentation is now snapshotted per payroll line with effective salary/rate, per-segment work/norm quantities, and a JSON component-assignment snapshot (migration `1630`).

**Interfaces:**
- Salary, employment rate and component assignments are effective-dated; overlapping records are rejected or resolved by an explicit policy.
- Month-internal changes are split into date segments and snapshotted into the payroll document.
- Add `POST /api/payroll/documents/{id}/recalculate`; posted documents remain immutable and create linked reversal/delta corrections.

- [x] Write failing/green policy and schema contract tests for posted-only recalculation and active-request idempotency.
- [x] Implement the organization-scoped queue, unique active-request guard, API endpoint, and admin action.
- [x] Implement source revision comparison and linked delta correction processing.
- [x] Verify period locks and audit user/time/source end-to-end.
- [x] Implement mid-month employment/component segment snapshots.

### Task 7: Tighten posting, advance, and period-close controls (P2)

**Progress:** Period close now rejects unresolved recalculation requests and existing unfinished correction documents. Final-payment outstanding is allocated to the exact payroll document, the posting dispatcher rejects inactive or group accounts, and the shared transaction wrapper has failure/exception rollback coverage.

**Files:**
- Modify: `backend/src/Application/Features/Pay/Payments/Services/PayrollPaymentService.cs`
- Modify: `backend/src/Application/Features/Register/AccountingRegisterEntries/Services/AccountingDispatcher.cs`
- Modify: `backend/src/Application/Features/Register/PostingEngines/Services/AccountingPostingValidator.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Modify: `backend/src/Application/Features/Pay/Periods/Services/PayrollPeriodService.cs`
- Create: `backend/tests/UnitTests/PayrollPostingIntegrityTests.cs`
- Create: `backend/tests/IntegrationTests/PayrollPeriodCloseTests.cs`

**Interfaces:**
- Advance is a first-half accrual/payment source, not an unclassified subtraction from final payroll.
- Payment lines always allocate to a payroll document/line; partial payment, overpayment and refund are explicit states.
- Only active organization-owned leaf accounts receive payroll postings.
- Period close requires no pending recalculation, unresolved correction, unpaid required tax liability or unreconciled payment.

- [x] Write focused policy tests for unresolved recalculation and source allocation behavior.
- [x] Implement period-close, exact-source payment, and leaf-account controls.
- [x] Add end-to-end rollback coverage for posting failure.

### Task 8: Correct component versioning and formula dependency rules (P2)

**Progress:** Component effective date ranges reject inverted ranges and overlapping versions for the same organization/code. Formula dependencies now have deterministic topological ordering and cycle/missing-reference checks; explicit minimum/maximum caps and taxable flags are persisted by migration `1632` and applied during calculation. Existing posted lines are immutable and the component update guard prevents source changes after posting.

**Files:**
- Modify: `backend/src/Application/Features/Pay/Components/Services/PayrollComponentService.cs`
- Modify: `backend/src/Application/Features/Pay/PayrollDocuments/Services/PayrollDocumentService.cs`
- Create: `backend/src/Infrastructure/Persistence/Scripts/16_pay/1629_add_component_overlap_constraints.sql`
- Create: `backend/tests/UnitTests/PayrollComponentVersionTests.cs`

**Interfaces:**
- One component code resolves to exactly one effective version for a payroll date.
- Formula dependencies have deterministic order and cycle detection; caps, floors and taxable/non-taxable flags are explicit.
- Multiple active employments are represented as separate lines when policy allows; otherwise return a conflict.

- [x] Write failing tests for effective-date overlap and invalid ranges.
- [x] Implement deterministic effective-range validation and explicit overlap policy.
- [x] Verify existing posted snapshots remain unchanged.

### Task 9: Align reports, frontend workflow, migration history, and documentation (P2)

**Files:**
- Modify: `frontend/src/modules/payroll/pages/documents/screens/PayrollDocumentDetailPage.tsx`
- Modify: `frontend/src/modules/payroll/pages/payments/screens/PayrollPaymentDetailPage.tsx`
- Modify: `frontend/src/modules/payroll/pages/reports/screens/*`
- Modify: `frontend/src/modules/payroll/pages/timesheets/components/TimesheetCalendarView.tsx`
- Modify: `backend/src/Application/Features/Pay/Reports/Services/PayrollReportService.cs`
- Modify: `backend/docs/hr-payroll-full-current-implementation.md`
- Create: `backend/docs/payroll-1c-gap-analysis-2026-09-10.md`
- Create: `frontend/tests/payroll-1c-workflow.test.ts`

**Interfaces:**
- UI clearly shows day/hour basis, employee norm, actual/special hours, paid absence, tax lines, correction source/payout mode and posting status.
- Reports reconcile payroll lines, source-specific payments and accounting entries.
- Migration history/runner records applied script, checksum and applied timestamp; 1618–1622 are verified before later scripts run.

- [x] Write failing frontend tests for correction mode, source-aware payment and recalculated totals.
- [x] Implement UI/report changes after backend contracts stabilize.
- [x] Add migration manifest/runner and legacy-correction review report.
- [x] Run frontend typecheck/lint/tests, backend unit/integration tests and `git diff --check`.

### Delivery order

Execute Tasks 3 → 4 → 5 → 6 → 7 → 8 → 9. Task 1 and Task 2 are complete in the current working tree. Do not commit or push; after each task leave the changes local and record test output.
