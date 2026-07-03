
create table acc_accounting_policy (
    id smallint   not null,
    code character varying(50) not null,
    name character varying(250) not null,
    state_id smallint not null,
    constraint acc_accounting_policy_code_key UNIQUE (code),
    constraint acc_accounting_policy_pkey primary key (id),
    constraint acc_accounting_policy_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into acc_accounting_policy (id, code, name, state_id) values
    ('1', 'STANDARD', 'Стандартная политика РУз', '1');

