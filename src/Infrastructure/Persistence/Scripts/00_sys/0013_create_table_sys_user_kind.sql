create table sys_user_kind
(
	id					smallint not null primary key,
	code                varchar(50) not null unique,
	name                varchar(100) not null unique,
	description         varchar(500) null
);

insert into sys_user_kind 
	(id, code, name, description) 
values 
	(1, 'super_admin', 'Super administrator',
		'Mahsulot egasi kompaniyasi xodimi. Tenantlar, tashkilotlar va rollarni boshqaradi; jarayonlarni kuzatishi, foydalanuvchi yoki butun tenantni o‘chirib qo‘yishi mumkin. Tenant ichidagi biznes ma\''lumotlariga kirish huquqi yo‘q.'),

	(2, 'tenant_owner', 'Tenant egasi',
		'Mahsulotni sotib olgan foydalanuvchi. O‘z tenantidagi barcha tashkilotlar va foydalanuvchilarni sys_user_organization dan qat\''i nazar, to‘g‘ridan-to‘g‘ri tenant egaligi orqali ko‘radi.'),

	(3, 'tenant_user', 'Foydalanuvchi',
		'Oddiy ishtirokchi. Kirish huquqi sys_user_organization.role_id orqali muayyan tashkilotdagi roli bilan belgilanadi.');