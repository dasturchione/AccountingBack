create table ref_region
(
	id					SERIAL NOT NULL PRIMARY KEY,
	short_name			VARCHAR(250) NOT NULL,
	full_name			VARCHAR(250) NOT NULL,
	state_id			SMALLINT NOT NULL REFERENCES enum_state ( id ),
	created_date		TIMESTAMP WITHOUT TIME ZONE DEFAULT now() NOT NULL
);
 