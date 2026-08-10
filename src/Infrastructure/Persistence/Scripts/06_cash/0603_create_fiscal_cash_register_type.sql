create table fiscal_cash_register_type
(
    id      smallserial primary key,
    code    varchar(50) not null unique,
    name    varchar(250) not null
);

insert into fiscal_cash_register_type 
    (id, code, name)
values
    (1, 'ONLINE_KKM', 'Onlayn-NKM'),
    (2, 'VIRTUAL_CASH_REGISTER', 'Virtual kassa');