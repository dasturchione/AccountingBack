create table ref_district
(
	id					SERIAL NOT NULL PRIMARY KEY,
	short_name			VARCHAR(250) NOT NULL,
	full_name			VARCHAR(250) NOT NULL,
	region_id			INT NOT NULL REFERENCES info_region (id),
	state_id			SMALLINT NOT NULL REFERENCES enum_state (id),
	created_date		TIMESTAMP WITHOUT TIME ZONE DEFAULT now() NOT NULL
);

CREATE INDEX idx_info_district_region_id 
	ON info_district (region_id);
