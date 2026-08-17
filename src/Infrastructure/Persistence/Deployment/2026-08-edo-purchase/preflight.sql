-- Read-only release gate. Run against the target database before deploying binaries.
select current_database() as database_name,
       current_user as database_user,
       count(*) filter (where table_type = 'BASE TABLE') as public_table_count
from information_schema.tables
where table_schema = 'public';

select expected.object_name,
       to_regclass(expected.object_name) is not null as exists
from (values
    ('public.edo_import_job'),
    ('public.edo_import_job_provider'),
    ('public.edo_import_candidate'),
    ('public.edo_import_candidate_line'),
    ('public.edo_import_candidate_marking'),
    ('public.edo_provider_product_mapping'),
    ('public.idx_edo_import_job_bulk_status')
) expected(object_name)
order by expected.object_name;

select expected.table_name,
       expected.column_name,
       actual.column_name is not null as exists
from (values
    ('edo_import_job', 'bulk_import_status'),
    ('edo_import_candidate_line', 'provider_product_name'),
    ('cmn_contract', 'provider_code'),
    ('cmn_contract', 'provider_contract_number'),
    ('cmn_contract', 'provider_contract_date')
) expected(table_name, column_name)
left join information_schema.columns actual
  on actual.table_schema = 'public'
 and actual.table_name = expected.table_name
 and actual.column_name = expected.column_name
order by expected.table_name, expected.column_name;

select to_regclass('public.cmn_document_number_sequence') is not null as document_number_sequence_exists,
       to_regprocedure('public.set_pur_doc_number()') is null as legacy_purchase_generator_removed;
