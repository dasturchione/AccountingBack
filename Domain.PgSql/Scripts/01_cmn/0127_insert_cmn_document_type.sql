insert into cmn_document_type (code, name, state_id)
values
('purchase', 'Xarid / Kirim', 1),
('sale', 'Sotuv', 1),
('bank_operation', 'Bank operatsiyasi', 1),
('cash_operation', 'Kassa operatsiyasi', 1),
('salary', 'Ish haqi', 1),
('expense', 'Xarajat', 1)
on conflict do nothing;
