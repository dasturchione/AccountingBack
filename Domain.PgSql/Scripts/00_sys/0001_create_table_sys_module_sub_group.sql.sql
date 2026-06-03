CREATE TABLE sys_module_sub_group
(
	id					SERIAL NOT NULL PRIMARY KEY,
	code				VARCHAR(100) NOT NULL,
	short_name			VARCHAR(250) NOT NULL,
	full_name			VARCHAR(300) NOT NULL,
	created_date		TIMESTAMP WITHOUT TIME ZONE DEFAULT now() NOT NULL
);

CREATE UNIQUE INDEX sys_module_sub_group_unique_index_code 
	ON sys_module_sub_group (code);
