create table acc_account_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint acc_account_type_pkey primary key (id),
    constraint acc_account_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_acc_account_type_code on acc_account_type using btree (code);

insert into acc_account_type (id, code, name, state_id) values
    ('1', 'active',             'Aktiv',                '1'),
    ('2', 'passive',            'Passiv',               '1'),
    ('3', 'active_passive',     'Aktiv-passiv',         '1'),
    ('4', 'counter_active',     'Kontr-aktiv',          '1'),
    ('5', 'counter_passive',    'Kontr-passiv',         '1'),
    ('6', 'tranzit',            'Tranzit',              '1'),
    ('7', 'off_balance',        'Balansdan tashqari',   '1');

    /*
        public const short Active = 1;

        public const short Passive = 2;

        public const short ActivePassive = 3;

        public const short CounterActive = 4;

        public const short CounterPassive = 5;

        public const short Tranzit = 6;

        public const short OffBalance = 7;*/