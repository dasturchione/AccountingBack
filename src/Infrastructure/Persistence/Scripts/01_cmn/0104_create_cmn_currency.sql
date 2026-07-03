
create table cmn_currency (
    id smallint   not null,
    code character varying(10) not null,
    name character varying(100) not null,
    symbol character varying(10),
    state_id smallint not null,
    constraint cmn_currency_pkey primary key (id),
    constraint cmn_currency_state_id_fkey foreign key (state_id) references cmn_state(id)
);

insert into cmn_currency (id, code, name, symbol, state_id) values
    ('1', 'UZS', 'Uzbek so''m', 'so''m', '1'),
    ('2', 'USD', 'US Dollar', '$', '1'),
    ('3', 'RUB', 'Russian Ruble', '₽', '1'),
    ('4', 'EUR', 'Euro', '€', '1');

create unique index idx_cmn_currency_code on cmn_currency using btree (code);

