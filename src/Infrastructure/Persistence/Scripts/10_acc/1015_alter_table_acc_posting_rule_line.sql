begin;

alter table acc_posting_rule_line
    add column debit_alias_id smallint;

update acc_posting_rule_line r
set debit_alias_id = a.id
from acc_posting_alias a
where r.debit_alias = a.code;

-- Проверить строки, для которых не нашлось соответствия
select *
from acc_posting_rule_line
where debit_alias_id is null;

-- Если проверка выше ничего не вернула, можно ставить not null
alter table acc_posting_rule_line
    alter column debit_alias_id set not null;

alter table acc_posting_rule_line
    add constraint fk_acc_posting_rule_line_debit_alias
    foreign key (debit_alias_id)
    references acc_posting_alias(id);

commit;

begin;

alter table acc_posting_rule_line
    add column credit_alias_id smallint;

update acc_posting_rule_line r
set credit_alias_id = a.id
from acc_posting_alias a
where r.credit_alias = a.code;

-- Проверить строки, для которых не нашлось соответствия
select *
from acc_posting_rule_line
where credit_alias_id is null;

-- Если проверка выше ничего не вернула, можно ставить not null
alter table acc_posting_rule_line
    alter column credit_alias_id set not null;

alter table acc_posting_rule_line
    add constraint fk_acc_posting_rule_line_credit_alias
    foreign key (credit_alias_id)
    references acc_posting_alias(id);

commit;

begin;

-- Контроль: все строки должны иметь новые id
do $$
begin
    if exists (
        select 1
        from public.acc_posting_rule_line
        where debit_alias_id is null
           or credit_alias_id is null
    ) then
        raise exception 'Cannot drop debit_alias/credit_alias: debit_alias_id or credit_alias_id contains NULL';
    end if;
end $$;

alter table public.acc_posting_rule_line
    drop column if exists debit_alias,
    drop column if exists credit_alias;

commit;

begin;

alter table public.acc_posting_rule_line
    drop column if exists debit_alias,
    drop column if exists credit_alias;

commit;
