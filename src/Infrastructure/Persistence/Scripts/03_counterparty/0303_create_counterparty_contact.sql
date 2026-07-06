create table counterparty_contact 
(
    id integer   not null,
    organization_id integer not null,
    counterparty_id integer not null,
    full_name character varying(250) not null,
    phone_number character varying(50),
    email character varying(250),
    "position" character varying(250),
    comment character varying(1000),
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint counterparty_contact_pkey primary key (id),
    constraint counterparty_contact_counterparty_id_fkey foreign key (counterparty_id) references counterparty_card(id),
    constraint counterparty_contact_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint counterparty_contact_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create index idx_counterparty_contact_counterparty_id on counterparty_contact using btree (counterparty_id);
create index idx_counterparty_contact_organization_id on counterparty_contact using btree (organization_id);
create index idx_counterparty_contact_state_id on counterparty_contact using btree (state_id);
