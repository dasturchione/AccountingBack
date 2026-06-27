create table cmn_pricing_method
(
	id					smallint not null primary key,
	code				varchar(20) not null unique,
	name				varchar(200) not null
);
