# Legacy payroll correction review

Run `payroll-legacy-correction-review.sql` against each organization database before closing a period. It is read-only and finds corrections that cannot be safely allocated to a posted source document or payout mode.

Rows with `MISSING_SOURCE` or `SOURCE_NOT_FOUND` must be reversed/voided according to the organization’s accounting policy. `SOURCE_NOT_POSTED` corrections must wait until the source is posted. `MISSING_PAYOUT_MODE` records should be explicitly classified as `WITH_SALARY`, `WITH_ADVANCE`, or `SEPARATE`; they must not be silently included in the regular payroll total.

New corrections created by the API always store `correction_of_doc_id` and `correction_payout_mode`, and posted source documents remain immutable.
