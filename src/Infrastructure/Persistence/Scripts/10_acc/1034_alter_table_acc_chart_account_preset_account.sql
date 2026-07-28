alter table acc_chart_account_preset_account
add column name varchar(255);

update acc_chart_account_preset_account as account
set name = translation.name
from acc_chart_account_preset_account_translation as translation
where translation.preset_account_id = account.id
  and translation.language_id = 1;

alter table acc_chart_account_preset_account
alter column name set not null;
