CREATE TABLE sys_role_module
(                   
    role_id INT NOT NULL REFERENCES sys_role(id),                      
    module_id INT NOT NULL REFERENCES sys_module(id), 
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),           

    PRIMARY KEY (role_id, module_id)
);
