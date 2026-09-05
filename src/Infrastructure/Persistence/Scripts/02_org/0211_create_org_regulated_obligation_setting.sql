create table org_regulated_obligation_setting
(
    id                          serial primary key,
    organization_id             int not null references org_organization(id),
    regulated_obligation_id     smallint not null references cmn_regulated_obligation(id),
    periodicity_id              smallint not null references cmn_regulated_obligation_periodicity(id),
    classifier_code             varchar(30),
    rate                        numeric(9, 6),
    chart_account_id            int not null references acc_chart_account(id),
    effective_from              date not null,
    effective_to                date,
    state_id                    smallint not null default 1 references cmn_state(id),
    created_date                timestamp without time zone not null default now(),
    updated_date                timestamp without time zone,

    constraint ck_org_regulated_obligation_setting_rate
        check (rate is null or rate between 0 and 100),
    constraint ck_org_regulated_obligation_setting_dates
        check (effective_to is null or effective_to >= effective_from),
    constraint ck_org_regulated_obligation_setting_classifier_code
        check (classifier_code is null or nullif(btrim(classifier_code), '') is not null),
    constraint uq_org_regulated_obligation_setting_version
        unique (organization_id, regulated_obligation_id, effective_from)
);

create index ix_org_regulated_obligation_setting_organization_id
    on org_regulated_obligation_setting(organization_id);

create index ix_org_regulated_obligation_setting_obligation_id
    on org_regulated_obligation_setting(regulated_obligation_id);

create index ix_org_regulated_obligation_setting_periodicity_id
    on org_regulated_obligation_setting(periodicity_id);

create index ix_org_regulated_obligation_setting_chart_account_id
    on org_regulated_obligation_setting(chart_account_id);

create index ix_org_regulated_obligation_setting_effective_dates
    on org_regulated_obligation_setting(effective_from, effective_to);

create unique index ux_org_regulated_obligation_setting_current
    on org_regulated_obligation_setting(organization_id, regulated_obligation_id)
    where effective_to is null
      and state_id = 1;
