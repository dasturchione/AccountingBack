CREATE TABLE sys_role 
(
    id                  SERIAL PRIMARY KEY,
    short_name          VARCHAR(100) NOT NULL,
    full_name           VARCHAR(255) NOT NULL, 
    state_id            SMALLINT NOT NULL REFERENCES enum_state(id),           
    created_date        TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now()
);
