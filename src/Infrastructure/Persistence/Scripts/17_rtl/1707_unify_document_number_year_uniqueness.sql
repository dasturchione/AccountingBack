begin;

drop index if exists ux_inv_inventory_adjustment_doc_org_doc_number;
drop index if exists ux_inv_inventory_count_doc_org_doc_number;
drop index if exists ux_inv_transfer_doc_doc_number_org;
drop index if exists ux_inv_opening_inventory_org_doc_number;

alter table fa_receipt_doc
    drop constraint if exists uq_fa_receipt_doc_org_number;
drop index if exists ux_fa_receipt_doc_org_doc_number;
drop index if exists ux_fa_movement_doc_org_doc_number;
drop index if exists ux_fa_depreciation_run_org_doc_number;
drop index if exists ux_fa_disposal_doc_org_doc_number;
drop index if exists ux_fa_revaluation_doc_org_doc_number;
alter table fa_commissioning_doc
    drop constraint if exists uq_fa_commissioning_doc_org_number;

alter table pay_timesheet
    drop constraint if exists ux_pay_timesheet_org_doc_number;
alter table pay_payroll_doc
    drop constraint if exists ux_pay_payroll_doc_org_doc_number;
alter table pay_payment_batch
    drop constraint if exists ux_pay_payment_batch_org_doc_number;
alter table hr_absence
    drop constraint if exists ux_hr_absence_org_doc_number;

create unique index if not exists ux_bank_operation_org_year_doc_number
    on bank_operation (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_cash_operation_org_year_doc_number
    on cash_operation (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_cash_fiscal_transfer_doc_org_year_doc_number
    on cash_fiscal_transfer_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_pur_doc_org_year_doc_number
    on pur_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_sale_doc_org_year_doc_number
    on sale_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_sale_shipment_doc_org_year_doc_number
    on sale_shipment_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_rtl_sale_doc_org_year_doc_number
    on rtl_sale_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_inv_inventory_adjustment_doc_org_year_doc_number
    on inv_inventory_adjustment_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_inv_inventory_count_doc_org_year_doc_number
    on inv_inventory_count_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_inv_transfer_doc_org_year_doc_number
    on inv_transfer_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_inv_opening_inventory_org_year_doc_number
    on inv_opening_inventory (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_fa_receipt_doc_org_year_doc_number
    on fa_receipt_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_fa_movement_doc_org_year_doc_number
    on fa_movement_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_fa_depreciation_run_org_year_doc_number
    on fa_depreciation_run (organization_id, (extract(year from period_month)), doc_number);

create unique index if not exists ux_fa_disposal_doc_org_year_doc_number
    on fa_disposal_doc (organization_id, (extract(year from disposal_date)), doc_number);

create unique index if not exists ux_fa_revaluation_doc_org_year_doc_number
    on fa_revaluation_doc (organization_id, (extract(year from revaluation_date)), doc_number);

create unique index if not exists ux_fa_commissioning_doc_org_year_doc_number
    on fa_commissioning_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_pay_timesheet_org_year_doc_number
    on pay_timesheet (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_pay_payroll_doc_org_year_doc_number
    on pay_payroll_doc (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_pay_payment_batch_org_year_doc_number
    on pay_payment_batch (organization_id, (extract(year from doc_date)), doc_number);

create unique index if not exists ux_hr_absence_org_year_doc_number
    on hr_absence (organization_id, (extract(year from doc_date)), doc_number);

commit;
