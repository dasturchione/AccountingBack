create table sys_audit_log
(
	id bigserial primary key,
	organization_id int null,
	schema_name varchar(100) not null,
	table_name varchar(100) not null,
	record_id varchar(100) null,
	action varchar(10) not null,
	old_data jsonb null,
	new_data jsonb null,
	changed_user_id int null,
	request_id varchar(100) null,
	client_addr inet null,
	application_name varchar(200) null,
	changed_date timestamp without time zone default now() not null,
	constraint chk_sys_audit_log_action
		check (action in ('INSERT', 'UPDATE', 'DELETE'))
);

create index idx_sys_audit_log_organization_id on sys_audit_log (organization_id);
create index idx_sys_audit_log_table_record on sys_audit_log (table_name, record_id);
create index idx_sys_audit_log_action on sys_audit_log (action);
create index idx_sys_audit_log_changed_user_id on sys_audit_log (changed_user_id);
create index idx_sys_audit_log_changed_date on sys_audit_log (changed_date);
