CREATE TABLE sys_user
(
	id						SERIAL NOT NULL PRIMARY KEY,
	user_name				VARCHAR(250) NOT NULL,
	password_hash			VARCHAR(250) NOT NULL,
	password_salt			VARCHAR(250) NOT NULL,
	phone_number			VARCHAR(50) NOT NULL,
	email					VARCHAR(200),
 	first_name      		VARCHAR(100) NOT NULL,
 	last_name       		VARCHAR(100) NOT NULL,
	role_id					INT NOT NULL REFERENCES sys_role (id),
	last_access_time		TIMESTAMP WITHOUT TIME ZONE NULL,
	state_id				SMALLINT NOT NULL REFERENCES enum_state (id),
	created_date			TIMESTAMP WITHOUT TIME ZONE DEFAULT now() NOT NULL
);

CREATE UNIQUE INDEX uidx_sys_user_user_name 
	ON sys_user (user_name);

CREATE INDEX idx_sys_user_phone 
	ON sys_user (phone_number);

CREATE INDEX idx_sys_user_role_id 
	ON sys_user (role_id);
