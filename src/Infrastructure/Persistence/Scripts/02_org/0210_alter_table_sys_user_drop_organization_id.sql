do $$
begin
	if exists (
		select 1
		from information_schema.columns
		where table_schema = 'public'
			and table_name = 'sys_user'
			and column_name = 'organization_id'
	) then
		execute $sql$
			insert into sys_user_organization
			(user_id, organization_id, role_id, is_default, state_id)
			select u.id,
				u.organization_id,
				u.role_id,
				case
					when exists (
						select 1
						from sys_user_organization suo
						where suo.user_id = u.id
							and suo.is_default = true
					) then false
					else true
				end,
				u.state_id
			from sys_user u
			where u.organization_id is not null
				and not exists (
					select 1
					from sys_user_organization existing
					where existing.user_id = u.id
						and existing.organization_id = u.organization_id
				);
		$sql$;
	end if;
end $$;

alter table sys_user
drop constraint if exists sys_user_organization_id_fkey;

drop index if exists idx_sys_user_organization_id;

alter table sys_user
drop column if exists organization_id;
