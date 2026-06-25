alter table acc_reg_entry
add column if not exists operation_type_id smallint null references cmn_operation_type(id),
add column if not exists debit_quantity numeric(18,3) null,
add column if not exists credit_quantity numeric(18,3) null,
add column if not exists content varchar(1000) null,
add column if not exists journal_number varchar(100) null;

create index if not exists idx_acc_reg_entry_operation_type_id
on acc_reg_entry (operation_type_id);

create index if not exists idx_acc_reg_entry_journal_number
on acc_reg_entry (journal_number);
