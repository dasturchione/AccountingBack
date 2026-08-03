create table if not exists fa_disposal_type 
(
	id				smallserial primary key,
	code			varchar(30) not null unique,
	name			varchar(100) not null
);

insert into fa_disposal_type (id, code, name)
values
    (1, 'SALE',      'Sotish'),
    (2, 'WRITEOFF',  'Hisobdan chiqarish'),
    (3, 'BREAKDOWN', 'Buzilish sababli hisobdan chiqarish')
on conflict (id) do update
set
    code = excluded.code,
    name = excluded.name;

select setval(
    pg_get_serial_sequence('fa_disposal_type', 'id'),
    coalesce((select max(id) from fa_disposal_type), 1),
    true
);