alter table edo_import_candidate_line
    add column provider_product_name character varying(500);

comment on column edo_import_candidate_line.provider_product_name is
    'Normalized provider product name snapshot; nullable for historical rows created before this column.';
