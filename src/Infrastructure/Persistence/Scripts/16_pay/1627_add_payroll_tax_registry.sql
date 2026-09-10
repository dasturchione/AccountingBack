begin;

create table if not exists pay_tax_definition
(
    id                    serial primary key,
    organization_id       integer not null references org_organization(id),
    code                  varchar(50) not null,
    name                  varchar(250) not null,
    tax_type              varchar(20) not null,
    base_type             varchar(30) not null,
    rate                  numeric(9,4) not null,
    exemption_amount      numeric(18,2),
    limit_amount          numeric(18,2),
    liability_account_id  integer not null references acc_chart_account(id),
    effective_from        date not null,
    effective_to          date,
    state_id              smallint not null references cmn_state(id),
    created_date          timestamp without time zone not null default now(),
    updated_date          timestamp without time zone,
    constraint ux_pay_tax_definition_org_code_from unique (organization_id, code, effective_from),
    constraint ck_pay_tax_definition_type check (tax_type in ('WITHHOLDING', 'EMPLOYER')),
    constraint ck_pay_tax_definition_base check (base_type in ('GROSS', 'TAXABLE_EARNINGS', 'NET')),
    constraint ck_pay_tax_definition_rate check (rate >= 0 and rate <= 100),
    constraint ck_pay_tax_definition_exemption check (exemption_amount is null or exemption_amount >= 0),
    constraint ck_pay_tax_definition_limit check (limit_amount is null or limit_amount >= 0),
    constraint ck_pay_tax_definition_dates check (effective_to is null or effective_to >= effective_from)
);

create index if not exists idx_pay_tax_definition_effective_dates
    on pay_tax_definition (organization_id, effective_from, effective_to);

create table if not exists pay_payroll_tax_line
(
    id                    bigserial primary key,
    organization_id       integer not null references org_organization(id),
    payroll_line_id       bigint not null references pay_payroll_line(id) on delete cascade,
    tax_definition_id     integer not null references pay_tax_definition(id),
    base_amount           numeric(18,2) not null,
    exemption_amount      numeric(18,2) not null default 0,
    taxable_base          numeric(18,2) not null,
    rate                  numeric(9,4) not null,
    amount                numeric(18,2) not null,
    liability_account_id  integer not null references acc_chart_account(id),
    created_date          timestamp without time zone not null default now(),
    constraint ck_pay_payroll_tax_line_amounts check (
        base_amount >= 0 and exemption_amount >= 0 and taxable_base >= 0 and rate >= 0 and amount >= 0
    )
);

create index if not exists idx_pay_payroll_tax_line_organization_id
    on pay_payroll_tax_line (organization_id);
create index if not exists idx_pay_payroll_tax_line_payroll_line_id
    on pay_payroll_tax_line (payroll_line_id);
create index if not exists idx_pay_payroll_tax_line_tax_definition_id
    on pay_payroll_tax_line (tax_definition_id);

commit;
