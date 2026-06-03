create table sys_module 
(
    id                  SERIAL NOT NULL PRIMARY KEY,
    code                VARCHAR(100) NOT NULL,
    short_name          VARCHAR(250) NOT NULL,
    full_name           VARCHAR(300) NOT NULL,
    sub_group_id        INT NOT NULL REFERENCES sys_module_sub_group(id),
    state_id            SMALLINT NOT NULL REFERENCES enum_state(id),
    created_date	    TIMESTAMP WITHOUT TIME ZONE DEFAULT now() NOT NULL         
);
    
CREATE UNIQUE INDEX sys_module_unique_index_code 
    ON sys_module (code);
    
CREATE INDEX sys_module_unique_index_sub_group_id 
    ON sys_module (sub_group_id);
