-- Read-only review for correction documents created before source-aware payout mode.
-- Rows returned require an accountant decision before period close.
select
    correction.id,
    correction.organization_id,
    correction.doc_number,
    correction.period_id,
    correction.status_id,
    correction.correction_of_doc_id,
    correction.correction_payout_mode,
    source.doc_number as source_doc_number,
    source.status_id as source_status_id,
    case
        when correction.correction_of_doc_id is null then 'MISSING_SOURCE'
        when source.id is null then 'SOURCE_NOT_FOUND'
        when source.status_id <> 2 then 'SOURCE_NOT_POSTED'
        when correction.correction_payout_mode is null then 'MISSING_PAYOUT_MODE'
        else 'OK'
    end as review_status
from pay_payroll_doc correction
left join pay_payroll_doc source on source.id = correction.correction_of_doc_id
where correction.document_kind = 'CORRECTION'
  and (
      correction.correction_of_doc_id is null
      or source.id is null
      or source.status_id <> 2
      or correction.correction_payout_mode is null
  )
order by correction.organization_id, correction.period_id, correction.id;
