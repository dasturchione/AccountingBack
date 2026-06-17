alter table cmn_contract
	add column if not exists contract_type_id smallint null;

do $$
begin
	if exists (
		select 1
		from information_schema.columns
		where table_schema = 'public'
			and table_name = 'cmn_contract'
			and column_name = 'contract_type'
	) then
		execute '
			update cmn_contract c
			set contract_type_id = t.id
			from cmn_contract_type t
			where c.contract_type_id is null
				and t.code = c.contract_type
		';
	end if;
end;
$$;

alter table cmn_contract
	alter column contract_type_id set not null;

alter table cmn_contract
	drop constraint if exists chk_cmn_contract_type;

drop index if exists idx_cmn_contract_contract_type;

alter table cmn_contract
	drop column if exists contract_type;

alter table cmn_contract
	drop constraint if exists cmn_contract_contract_type_id_fkey;

alter table cmn_contract
	add constraint cmn_contract_contract_type_id_fkey
		foreign key (contract_type_id) references cmn_contract_type(id);

create index if not exists idx_cmn_contract_contract_type_id on cmn_contract (contract_type_id);
