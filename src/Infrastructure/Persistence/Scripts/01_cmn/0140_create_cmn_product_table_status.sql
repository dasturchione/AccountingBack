create table cmn_product_table_status
(
	id					smallserial primary key,
	code				varchar(50) not null unique,
	name				varchar(100) not null,
	state_id			smallint not null references cmn_state(id)
);
