create table fa_receipt_doc_line
(
    id bigint generated always as identity
        constraint fa_receipt_doc_line_pkey primary key,
    owner_id bigint not null
        constraint fa_receipt_doc_line_owner_id_fkey references fa_receipt_doc (id) on delete cascade,
    source_product_id integer
        constraint fa_receipt_doc_line_source_product_id_fkey references inv_product (id),
    name character varying(250) not null,
    quantity numeric(19, 6) not null,
    price numeric(24, 8) not null,
    amount numeric(24, 8) default 0 not null,
    vat_rate_id smallint
        constraint fa_receipt_doc_line_vat_rate_id_fkey references cmn_vat_rate (id),
    vat_amount numeric(24, 8) default 0 not null,
    total_amount numeric(24, 8) default 0 not null,
    constraint ck_fa_receipt_doc_line_quantity_positive check (quantity > 0),
    constraint ck_fa_receipt_doc_line_price_nonnegative check (price >= 0)
);

create index idx_fa_receipt_doc_line_owner_id on fa_receipt_doc_line using btree (owner_id);
create index idx_fa_receipt_doc_line_source_product_id on fa_receipt_doc_line using btree (source_product_id);
create index idx_fa_receipt_doc_line_vat_rate_id on fa_receipt_doc_line using btree (vat_rate_id);
