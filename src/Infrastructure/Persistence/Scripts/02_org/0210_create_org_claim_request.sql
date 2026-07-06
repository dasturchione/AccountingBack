
create table org_claim_request 
(
    id bigint   not null,
    organization_id integer,
    requested_by_user_id integer not null,
    inn character varying(20) not null,
    organization_name character varying(500),
    status character varying(30) default 'pending'::character varying not null,
    review_comment character varying(1000),
    reviewed_by_user_id integer,
    reviewed_at timestamp without time zone,
    created_date timestamp without time zone default now() not null,
    constraint org_claim_request_pkey primary key (id),
    constraint org_claim_request_organization_id_fkey foreign key (organization_id) references org_organization(id),
    constraint org_claim_request_requested_by_user_id_fkey foreign key (requested_by_user_id) references sys_user(id),
    constraint org_claim_request_reviewed_by_user_id_fkey foreign key (reviewed_by_user_id) references sys_user(id)
);

create index idx_org_claim_request_inn on org_claim_request using btree (inn);
create index idx_org_claim_request_organization_id on org_claim_request using btree (organization_id);
create index idx_org_claim_request_requested_by_user_id on org_claim_request using btree (requested_by_user_id);
create index idx_org_claim_request_status on org_claim_request using btree (status);
