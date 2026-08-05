create table sys_user_kind_translation 
(
    user_kind_id        smallint        not null references sys_user_kind(id) on delete cascade,
    language_id         smallint        not null references cmn_language(id) on delete cascade,
	name                varchar(100)    not null unique,
	description         varchar(500)    null,

    primary key (user_kind_id, language_id)
);

insert into sys_user_kind_translation (user_kind_id, language_id, name, description)
select k.id, l.id, v.name, v.description
from (values
    ('super_admin', 'ru', 'Суперадминистратор',
        'Сотрудник компании-владельца продукта. Управляет тенантами, организациями, ролями; может отслеживать процессы, отключать пользователей или тенант целиком. Доступа к бизнес-данным внутри тенантов не имеет.'),
    ('super_admin', 'en', 'Super Administrator',
        'Employee of the product company. Manages tenants, organizations, and roles; can monitor processes and disable users or entire tenants. Has no access to business data inside tenants.'),
    ('super_admin', 'uz', 'Super administrator',
        'Mahsulot egasi kompaniyasi xodimi. Tenantlar, tashkilotlar va rollarni boshqaradi; jarayonlarni kuzatishi, foydalanuvchi yoki butun tenantni o‘chirib qo‘yishi mumkin. Tenant ichidagi biznes ma''lumotlariga kirish huquqi yo‘q.'),

    ('tenant_owner', 'ru', 'Владелец тенанта',
        'Пользователь, купивший продукт. Видит все организации и всех пользователей своего тенанта напрямую по владению тенантом, без зависимости от sys_user_organization.'),
    ('tenant_owner', 'en', 'Tenant Owner',
        'User who purchased the product. Sees all organizations and users within their tenant directly through tenant ownership, independent of sys_user_organization.'),
    ('tenant_owner', 'uz', 'Tenant egasi',
        'Mahsulotni sotib olgan foydalanuvchi. O‘z tenantidagi barcha tashkilotlar va foydalanuvchilarni sys_user_organization dan qat''i nazar, to‘g‘ridan-to‘g‘ri tenant egaligi orqali ko‘radi.'),

    ('tenant_user', 'ru', 'Пользователь',
        'Обычный участник. Доступ определяется ролью в конкретной организации через sys_user_organization.role_id.'),
    ('tenant_user', 'en', 'Tenant User',
        'Regular member within a tenant. Access is determined by the role in a specific organization via sys_user_organization.role_id.'),
    ('tenant_user', 'uz', 'Tenant foydalanuvchi',
        'Oddiy ishtirokchi. Kirish huquqi sys_user_organization.role_id orqali muayyan tashkilotdagi roli bilan belgilanadi.')
) as v(kind_code, lang_code, name, description)
join sys_user_kind k on k.code = v.kind_code
join cmn_language  l on l.code = v.lang_code;