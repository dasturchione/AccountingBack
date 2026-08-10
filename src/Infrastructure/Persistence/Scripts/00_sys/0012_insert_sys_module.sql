begin;

delete from sys_module;
select setval('sys_module_id_seq', 1, false);

-- SYS
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'SYS'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('AUDIT_LOG_VIEW', 'Audit log', 'Audit loglarni ko''rish', 90, false),
        ('AUTH_CHECK_TOKEN', 'Auth Check Token', 'Auth Check Token', 0, false),
        ('DASHBOARD_VIEW', 'Dashboard', 'Super Admin dashboard statistikasi', 91, false),
        ('NOTIFICATIONS_GET_FOR_CURRENT_USER', 'Notifications Get For Current User', 'Notifications Get For Current User', 0, false),
        ('NOTIFICATIONS_GET_UNREAD_COUNT', 'Notifications Get Unread Count', 'Notifications Get Unread Count', 0, false),
        ('NOTIFICATIONS_MARK_ALL_AS_READ', 'Notifications Mark All As Read', 'Notifications Mark All As Read', 0, false),
        ('NOTIFICATIONS_MARK_AS_READ', 'Notifications Mark As Read', 'Notifications Mark As Read', 0, false),
        ('PLATFORM_ACTIVATE_ORGANIZATION', 'Platform Activate Organization', 'Platform Activate Organization', 0, false),
        ('PLATFORM_ACTIVATE_TENANT', 'Platform Activate Tenant', 'Platform Activate Tenant', 0, false),
        ('PLATFORM_ARCHIVE_ORGANIZATION', 'Platform Archive Organization', 'Platform Archive Organization', 0, false),
        ('PLATFORM_ATTACH_USER_TO_ORGANIZATION', 'Platform Attach User To Organization', 'Platform Attach User To Organization', 0, false),
        ('PLATFORM_BLOCK_USER', 'Platform Block User', 'Platform Block User', 0, false),
        ('PLATFORM_CREATE_TENANT', 'Platform Create Tenant', 'Platform Create Tenant', 0, false),
        ('PLATFORM_CREATE_USER', 'Platform Create User', 'Platform Create User', 0, false),
        ('PLATFORM_DEACTIVATE_ORGANIZATION', 'Platform Deactivate Organization', 'Platform Deactivate Organization', 0, false),
        ('PLATFORM_DEACTIVATE_TENANT', 'Platform Deactivate Tenant', 'Platform Deactivate Tenant', 0, false),
        ('PLATFORM_GET_AUDIT_LOGS', 'Platform Get Audit Logs', 'Platform Get Audit Logs', 0, false),
        ('PLATFORM_GET_DASHBOARD', 'Platform Get Dashboard', 'Platform Get Dashboard', 0, false),
        ('PLATFORM_GET_ORGANIZATION_BY_ID', 'Platform Get Organization By Id', 'Platform Get Organization By Id', 0, false),
        ('PLATFORM_GET_ORGANIZATIONS', 'Platform Get Organizations', 'Platform Get Organizations', 0, false),
        ('PLATFORM_GET_TENANT_BY_ID', 'Platform Get Tenant By Id', 'Platform Get Tenant By Id', 0, false),
        ('PLATFORM_GET_TENANTS', 'Platform Get Tenants', 'Platform Get Tenants', 0, false),
        ('PLATFORM_GET_USER_BY_ID', 'Platform Get User By Id', 'Platform Get User By Id', 0, false),
        ('PLATFORM_GET_USERS', 'Platform Get Users', 'Platform Get Users', 0, false),
        ('PLATFORM_REMOVE_USER_FROM_ORGANIZATION', 'Platform Remove User From Organization', 'Platform Remove User From Organization', 0, false),
        ('PLATFORM_SET_USER_PASSWORD', 'Platform Set User Password', 'Platform Set User Password', 0, false),
        ('PLATFORM_UNBLOCK_USER', 'Platform Unblock User', 'Platform Unblock User', 0, false),
        ('PLATFORM_UPDATE_ORGANIZATION', 'Platform Update Organization', 'Platform Update Organization', 0, false),
        ('PLATFORM_UPDATE_TENANT', 'Platform Update Tenant', 'Platform Update Tenant', 0, false),
        ('PLATFORM_UPDATE_USER', 'Platform Update User', 'Platform Update User', 0, false),
        ('PLATFORM_UPDATE_USER_ORGANIZATION', 'Platform Update User Organization', 'Platform Update User Organization', 0, false),
        ('ROLE_CREATE', 'Rol yaratish', 'Yangi rol qo''shish', 0, false),
        ('ROLE_DELETE', 'Rol o''chirish', 'Rolni o''chirish', 0, false),
        ('ROLE_UPDATE', 'Rol tahrirlash', 'Rolni tahrirlash', 0, false),
        ('ROLE_VIEW', 'Rollar ro''yxati', 'Rollarni ko''rish', 0, false),
        ('ROLE_VIEW_DETAIL', 'Rol detail', 'Rolni batafsil ko''rish', 0, false),
        ('SETTINGS_GET_ALL', 'Settings Get All', 'Settings Get All', 0, false),
        ('SETTINGS_GET_BY_CODE', 'Settings Get By Code', 'Settings Get By Code', 0, false),
        ('SETTINGS_UPDATE', 'Settings Update', 'Settings Update', 0, false),
        ('USER_CREATE', 'Foydalanuvchi yaratish', 'Yangi foydalanuvchi', 0, false),
        ('USER_DELETE', 'Foydalanuvchi o''chirish', 'Foydalanuvchini o''chirish', 0, false),
        ('USER_UPDATE', 'Foydalanuvchi tahrir', 'Foydalanuvchini tahrirlash', 0, false),
        ('USER_VIEW', 'Foydalanuvchilar', 'Foydalanuvchilarni ko''rish', 0, false),
        ('USER_VIEW_DETAIL', 'Foydalanuvchi detail', 'Foydalanuvchini batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- ORG
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'ORG'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('BRANCH_CREATE', 'Filial yaratish', 'Yangi filial', 0, false),
        ('BRANCH_DELETE', 'Filial o''chirish', 'Filialni o''chirish', 0, false),
        ('BRANCH_UPDATE', 'Filial tahrirlash', 'Filialni tahrirlash', 0, false),
        ('BRANCH_VIEW', 'Filiallar', 'Filiallar ro''yxati', 0, false),
        ('BRANCH_VIEW_DETAIL', 'Filial detail', 'Filialni batafsil ko''rish', 0, false),
        ('DEPARTMENT_CREATE', 'Bo''lim yaratish', 'Yangi bo''lim', 0, false),
        ('DEPARTMENT_DELETE', 'Bo''lim o''chirish', 'Bo''limni o''chirish', 0, false),
        ('DEPARTMENT_UPDATE', 'Bo''lim tahrirlash', 'Bo''limni tahrirlash', 0, false),
        ('DEPARTMENT_VIEW', 'Bo''limlar', 'Bo''limlar ro''yxati', 0, false),
        ('DEPARTMENT_VIEW_DETAIL', 'Bo''lim detail', 'Bo''limni batafsil ko''rish', 0, false),
        ('ORGANIZATION_CREATE', 'Tashkilot yaratish', 'Yangi tashkilot', 0, false),
        ('ORGANIZATION_DELETE', 'Tashkilot o''chirish', 'Tashkilotni o''chirish', 0, false),
        ('ORGANIZATION_UPDATE', 'Tashkilot tahrirlash', 'Tashkilotni tahrirlash', 0, false),
        ('ORGANIZATION_VIEW', 'Tashkilotlar', 'Tashkilotlar ro''yxati', 0, false),
        ('ORGANIZATION_VIEW_DETAIL', 'Tashkilot detail', 'Tashkilotni batafsil ko''rish', 0, false),
        ('POSITION_CREATE', 'Lavozim yaratish', 'Yangi lavozim', 0, false),
        ('POSITION_DELETE', 'Lavozim o''chirish', 'Lavozimni o''chirish', 0, false),
        ('POSITION_UPDATE', 'Lavozim tahrirlash', 'Lavozimni tahrirlash', 0, false),
        ('POSITION_VIEW', 'Lavozimlar', 'Lavozimlar ro''yxati', 0, false),
        ('POSITION_VIEW_DETAIL', 'Lavozim detail', 'Lavozimni batafsil ko''rish', 0, false),
        ('SETUP_COMPLETE', 'Setup Complete', 'Setup Complete', 0, false),
        ('SETUP_GET', 'Setup Get', 'Setup Get', 0, false),
        ('SETUP_UPDATE_ACCOUNTING_POLICY', 'Setup Update Accounting Policy', 'Setup Update Accounting Policy', 0, false),
        ('SETUP_UPDATE_COMPANY_PROFILE', 'Setup Update Company Profile', 'Setup Update Company Profile', 0, false),
        ('SETUP_UPDATE_DEFAULTS', 'Setup Update Defaults', 'Setup Update Defaults', 0, false),
        ('SETUP_UPDATE_TAX_SETTINGS', 'Setup Update Tax Settings', 'Setup Update Tax Settings', 0, false),
        ('SETUP_UPDATE_USERS', 'Setup Update Users', 'Setup Update Users', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- COUNTERPARTY
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'COUNTERPARTY'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('COUNTERPARTY_BANK_ACCOUNT_CREATE', 'Kontragent bank hisobi yaratish', 'Yangi', 0, false),
        ('COUNTERPARTY_BANK_ACCOUNT_DELETE', 'Kontragent bank hisobi o''chirish', 'O''chirish', 0, false),
        ('COUNTERPARTY_BANK_ACCOUNT_UPDATE', 'Kontragent bank hisobi tahrirlash', 'Tahrirlash', 0, false),
        ('COUNTERPARTY_BANK_ACCOUNT_VIEW', 'Kontragent bank hisoblari', 'Ro''yxat', 0, false),
        ('COUNTERPARTY_BANK_ACCOUNT_VIEW_DETAIL', 'Kontragent bank hisobi detail', 'Batafsil', 0, false),
        ('COUNTERPARTY_CARD_CREATE', 'Kontragent yaratish', 'Yangi kontragent', 0, false),
        ('COUNTERPARTY_CARD_CREATE_MANY', 'Counterparty Card Create Many', 'Counterparty Card Create Many', 0, false),
        ('COUNTERPARTY_CARD_DELETE', 'Kontragent o''chirish', 'Kontragentni o''chirish', 0, false),
        ('COUNTERPARTY_CARD_UPDATE', 'Kontragent tahrirlash', 'Kontragentni tahrirlash', 0, false),
        ('COUNTERPARTY_CARD_VIEW', 'Kontragentlar', 'Kontragentlar ro''yxati', 0, false),
        ('COUNTERPARTY_CARD_VIEW_DETAIL', 'Kontragent detail', 'Kontragentni batafsil ko''rish', 0, false),
        ('COUNTERPARTY_CONTACT_CREATE', 'Kontragent kontakt yaratish', 'Yangi', 0, false),
        ('COUNTERPARTY_CONTACT_DELETE', 'Kontragent kontakt o''chirish', 'O''chirish', 0, false),
        ('COUNTERPARTY_CONTACT_UPDATE', 'Kontragent kontakt tahrirlash', 'Tahrirlash', 0, false),
        ('COUNTERPARTY_CONTACT_VIEW', 'Kontragent kontaktlari', 'Ro''yxat', 0, false),
        ('COUNTERPARTY_CONTACT_VIEW_DETAIL', 'Kontragent kontakt detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- INVENTORY
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'INVENTORY'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('PRODUCT_CREATE', 'Tovar yaratish', 'Yangi', 0, false),
        ('PRODUCT_CREATE_MANY', 'Product Create Many', 'Product Create Many', 0, false),
        ('PRODUCT_DELETE', 'Tovar o''chirish', 'O''chirish', 0, false),
        ('PRODUCT_GROUP_CREATE', 'Tovar guruhi yaratish', 'Yangi', 0, false),
        ('PRODUCT_GROUP_DELETE', 'Tovar guruhi o''chirish', 'O''chirish', 0, false),
        ('PRODUCT_GROUP_UPDATE', 'Tovar guruhi tahrirlash', 'Tahrirlash', 0, false),
        ('PRODUCT_GROUP_VIEW', 'Tovar guruhlari', 'Ro''yxat', 0, false),
        ('PRODUCT_GROUP_VIEW_DETAIL', 'Tovar guruhi detail', 'Batafsil', 0, false),
        ('PRODUCT_IMPORT_IMPORT', 'Product Import Import', 'Product Import Import', 0, false),
        ('PRODUCT_PRICE_CREATE', 'Tovar narxi yaratish', 'Yangi', 0, false),
        ('PRODUCT_PRICE_DELETE', 'Tovar narxi o''chirish', 'O''chirish', 0, false),
        ('PRODUCT_PRICE_GET_PRICE_DETAILS_BY_PRODUCT_ID', 'Product Price Get Price Details By Product Id', 'Product Price Get Price Details By Product Id', 0, false),
        ('PRODUCT_PRICE_UPDATE', 'Tovar narxi tahrirlash', 'Tahrirlash', 0, false),
        ('PRODUCT_PRICE_VIEW', 'Tovar narxlari', 'Ro''yxat', 0, false),
        ('PRODUCT_PRICE_VIEW_DETAIL', 'Tovar narxi detail', 'Batafsil', 0, false),
        ('PRODUCT_STOCK_GET_BY_MARKING_NUMBER', 'Product Stock Get By Marking Number', 'Product Stock Get By Marking Number', 0, false),
        ('PRODUCT_STOCK_GET_PRODUCT_GROUP_SUMMARY', 'Product Stock Get Product Group Summary', 'Product Stock Get Product Group Summary', 0, false),
        ('PRODUCT_STOCK_GET_PRODUCT_SUMMARY', 'Product Stock Get Product Summary', 'Product Stock Get Product Summary', 0, false),
        ('PRODUCT_STOCK_GET_PRODUCT_TABLE_SUMMARY', 'Product Stock Get Product Table Summary', 'Product Stock Get Product Table Summary', 0, false),
        ('PRODUCT_UPDATE', 'Tovar tahrirlash', 'Tahrirlash', 0, false),
        ('PRODUCT_VIEW', 'Tovarlar', 'Ro''yxat', 0, false),
        ('PRODUCT_VIEW_DETAIL', 'Tovar detail', 'Batafsil', 0, false),
        ('WAREHOUSE_CREATE', 'Ombor yaratish', 'Yangi', 0, false),
        ('WAREHOUSE_DELETE', 'Ombor o''chirish', 'O''chirish', 0, false),
        ('WAREHOUSE_UPDATE', 'Ombor tahrirlash', 'Tahrirlash', 0, false),
        ('WAREHOUSE_VIEW', 'Omborlar', 'Ro''yxat', 0, false),
        ('WAREHOUSE_VIEW_DETAIL', 'Ombor detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- BANK
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'BANK'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('BANK_CREATE', 'Bank yaratish', 'Yangi bank qo''shish', 0, false),
        ('BANK_DELETE', 'Bank o''chirish', 'Bankni o''chirish', 0, false),
        ('BANK_OPERATION_CREATE', 'Bank operatsiyasi yaratish', 'Yangi', 0, false),
        ('BANK_OPERATION_CREATE_MANY', 'Bank Operation Create Many', 'Bank Operation Create Many', 0, false),
        ('BANK_OPERATION_DELETE', 'Bank operatsiyasi o''chirish', 'O''chirish', 0, false),
        ('BANK_OPERATION_UPDATE', 'Bank operatsiyasi tahrirlash', 'Tahrirlash', 0, false),
        ('BANK_OPERATION_VIEW', 'Bank operatsiyalari', 'Ro''yxat', 0, false),
        ('BANK_OPERATION_VIEW_DETAIL', 'Bank operatsiyasi detail', 'Batafsil', 0, false),
        ('BANK_REPORT_EXPORT_OPERATIONS', 'Bank Report Export Operations', 'Bank Report Export Operations', 0, false),
        ('BANK_REPORT_OPERATION_BY_ID', 'Bank Report Operation By Id', 'Bank Report Operation By Id', 0, false),
        ('BANK_REPORT_OPERATIONS', 'Bank Report Operations', 'Bank Report Operations', 0, false),
        ('BANK_STATEMENT_PARSE', 'Bank statement import', 'Bank Excel statement faylini JSON qilib parse qilish', 0, false),
        ('BANK_UPDATE', 'Bank tahrirlash', 'Bankni tahrirlash', 0, false),
        ('BANK_VIEW', 'Banklar', 'Banklar ro''yxati', 0, false),
        ('BANK_VIEW_DETAIL', 'Bank detail', 'Bankni batafsil ko''rish', 0, false),
        ('CANCEL_BANK_OPERATION', 'Bank operatsiyasini bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_BANK_OPERATION', 'Bank operatsiyasini tasdiqlash', 'Tasdiqlash', 0, false),
        ('ORG_BANK_ACCOUNT_CREATE', 'Tashkilot bank hisobi yaratish', 'Yangi', 0, false),
        ('ORG_BANK_ACCOUNT_CREATE_MANY', 'Org Bank Account Create Many', 'Org Bank Account Create Many', 0, false),
        ('ORG_BANK_ACCOUNT_DELETE', 'Tashkilot bank hisobi o''chirish', 'O''chirish', 0, false),
        ('ORG_BANK_ACCOUNT_UPDATE', 'Tashkilot bank hisobi tahrirlash', 'Tahrirlash', 0, false),
        ('ORG_BANK_ACCOUNT_VIEW', 'Tashkilot bank hisoblari', 'Ro''yxat', 0, false),
        ('ORG_BANK_ACCOUNT_VIEW_DETAIL', 'Tashkilot bank hisobi detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- BANK TERMINAL
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'BANK_TERMINAL'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('BANK_TERMINAL_CREATE', 'Bank terminal yaratish', 'Yangi bank terminal qo''shish', 0, false),
        ('BANK_TERMINAL_DELETE', 'Bank terminal o''chirish', 'Bank terminalni o''chirish', 0, false),
        ('BANK_TERMINAL_UPDATE', 'Bank terminal tahrirlash', 'Bank terminalni tahrirlash', 0, false),
        ('BANK_TERMINAL_VIEW', 'Bank terminallari', 'Bank terminallari ro''yxati', 0, false),
        ('BANK_TERMINAL_VIEW_DETAIL', 'Bank terminal detail', 'Bank terminalni batafsil ko''rish', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- CASH
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'CASH'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_CASH_OPERATION', 'Kassa operatsiyasini bekor qilish', 'Bekor qilish', 0, false),
        ('CASH_BOX_CREATE', 'Kassa yaratish', 'Yangi', 0, false),
        ('CASH_BOX_DELETE', 'Kassa o''chirish', 'O''chirish', 0, false),
        ('CASH_BOX_UPDATE', 'Kassa tahrirlash', 'Tahrirlash', 0, false),
        ('CASH_BOX_VIEW', 'Kassalar', 'Ro''yxat', 0, false),
        ('CASH_BOX_VIEW_DETAIL', 'Kassa detail', 'Batafsil', 0, false),
        ('CASH_DOCUMENT_CANCEL_PAYMENT_ORDER', 'Cash Document Cancel Payment Order', 'Cash Document Cancel Payment Order', 0, false),
        ('CASH_DOCUMENT_CANCEL_RECEIPT_ORDER', 'Cash Document Cancel Receipt Order', 'Cash Document Cancel Receipt Order', 0, false),
        ('CASH_DOCUMENT_CONFIRM_PAYMENT_ORDER', 'Cash Document Confirm Payment Order', 'Cash Document Confirm Payment Order', 0, false),
        ('CASH_DOCUMENT_CONFIRM_RECEIPT_ORDER', 'Cash Document Confirm Receipt Order', 'Cash Document Confirm Receipt Order', 0, false),
        ('CASH_DOCUMENT_CREATE_PAYMENT_ORDER', 'Cash Document Create Payment Order', 'Cash Document Create Payment Order', 0, false),
        ('CASH_DOCUMENT_CREATE_RECEIPT_ORDER', 'Cash Document Create Receipt Order', 'Cash Document Create Receipt Order', 0, false),
        ('CASH_DOCUMENT_DELETE_PAYMENT_ORDER', 'Cash Document Delete Payment Order', 'Cash Document Delete Payment Order', 0, false),
        ('CASH_DOCUMENT_DELETE_RECEIPT_ORDER', 'Cash Document Delete Receipt Order', 'Cash Document Delete Receipt Order', 0, false),
        ('CASH_DOCUMENT_GET_PAYMENT_ORDER_BY_ID', 'Cash Document Get Payment Order By Id', 'Cash Document Get Payment Order By Id', 0, false),
        ('CASH_DOCUMENT_GET_PAYMENT_ORDERS', 'Cash Document Get Payment Orders', 'Cash Document Get Payment Orders', 0, false),
        ('CASH_DOCUMENT_GET_RECEIPT_ORDER_BY_ID', 'Cash Document Get Receipt Order By Id', 'Cash Document Get Receipt Order By Id', 0, false),
        ('CASH_DOCUMENT_GET_RECEIPT_ORDERS', 'Cash Document Get Receipt Orders', 'Cash Document Get Receipt Orders', 0, false),
        ('CASH_DOCUMENT_UPDATE_PAYMENT_ORDER', 'Cash Document Update Payment Order', 'Cash Document Update Payment Order', 0, false),
        ('CASH_DOCUMENT_UPDATE_RECEIPT_ORDER', 'Cash Document Update Receipt Order', 'Cash Document Update Receipt Order', 0, false),
        ('CASH_OPERATION_CREATE', 'Kassa operatsiyasi yaratish', 'Yangi', 0, false),
        ('CASH_OPERATION_DELETE', 'Kassa operatsiyasi o''chirish', 'O''chirish', 0, false),
        ('CASH_OPERATION_UPDATE', 'Kassa operatsiyasi tahrirlash', 'Tahrirlash', 0, false),
        ('CASH_OPERATION_VIEW', 'Kassa operatsiyalari', 'Ro''yxat', 0, false),
        ('CASH_OPERATION_VIEW_DETAIL', 'Kassa operatsiyasi detail', 'Batafsil', 0, false),
        ('CASH_REPORT_EXPORT_OPERATIONS', 'Cash Report Export Operations', 'Cash Report Export Operations', 0, false),
        ('CASH_REPORT_OPERATION_BY_ID', 'Cash Report Operation By Id', 'Cash Report Operation By Id', 0, false),
        ('CASH_REPORT_OPERATIONS', 'Cash Report Operations', 'Cash Report Operations', 0, false),
        ('CONFIRM_CASH_OPERATION', 'Kassa operatsiyasini tasdiqlash', 'Tasdiqlash', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- FISCAL CASH REGISTER
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'FISCAL_CASH_REGISTER'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('FISCAL_CASH_REGISTER_CREATE', 'Fiskal kassa yaratish', 'Yangi fiskal kassa registri qo''shish', 0, false),
        ('FISCAL_CASH_REGISTER_DELETE', 'Fiskal kassa o''chirish', 'Fiskal kassa registrini o''chirish', 0, false),
        ('FISCAL_CASH_REGISTER_UPDATE', 'Fiskal kassa tahrirlash', 'Fiskal kassa registrini tahrirlash', 0, false),
        ('FISCAL_CASH_REGISTER_VIEW', 'Fiskal kassalar', 'Fiskal kassa registrlari ro''yxati', 0, false),
        ('FISCAL_CASH_REGISTER_VIEW_DETAIL', 'Fiskal kassa detail', 'Fiskal kassa registrini batafsil ko''rish', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- PURCHASE
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'PURCHASE'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_PURCHASE', 'Xaridni bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_PURCHASE', 'Xaridni tasdiqlash', 'Tasdiqlash', 0, false),
        ('CONTRACT_CREATE', 'Shartnoma yaratish', 'Yangi shartnoma qo''shish', 0, false),
        ('CONTRACT_DELETE', 'Shartnoma o''chirish', 'Shartnomani o''chirish', 0, false),
        ('CONTRACT_UPDATE', 'Shartnoma tahrirlash', 'Shartnomani tahrirlash', 0, false),
        ('CONTRACT_VIEW', 'Shartnomalar', 'Shartnomalar ro''yxati', 0, false),
        ('CONTRACT_VIEW_DETAIL', 'Shartnoma detail', 'Shartnomani batafsil ko''rish', 0, false),
        ('PURCHASE_DOC_CREATE', 'Xarid hujjati yaratish', 'Yangi', 0, false),
        ('PURCHASE_DOC_DELETE', 'Xarid hujjati o''chirish', 'O''chirish', 0, false),
        ('PURCHASE_DOC_TABLE_CREATE', 'Xarid satri yaratish', 'Yangi', 0, false),
        ('PURCHASE_DOC_TABLE_DELETE', 'Xarid satri o''chirish', 'O''chirish', 0, false),
        ('PURCHASE_DOC_TABLE_UPDATE', 'Xarid satri tahrirlash', 'Tahrirlash', 0, false),
        ('PURCHASE_DOC_TABLE_VIEW', 'Xarid satrlari', 'Ro''yxat', 0, false),
        ('PURCHASE_DOC_TABLE_VIEW_DETAIL', 'Xarid satri detail', 'Batafsil', 0, false),
        ('PURCHASE_DOC_UPDATE', 'Xarid hujjati tahrirlash', 'Tahrirlash', 0, false),
        ('PURCHASE_DOC_VIEW', 'Xarid hujjatlari', 'Ro''yxat', 0, false),
        ('PURCHASE_DOC_VIEW_DETAIL', 'Xarid hujjati detail', 'Batafsil', 0, false),
        ('PURCHASE_REPORT_EXPORT', 'Purchase Report Export', 'Purchase Report Export', 0, false),
        ('PURCHASE_REPORT_GET_ALL', 'Purchase Report Get All', 'Purchase Report Get All', 0, false),
        ('PURCHASE_REPORT_GET_BY_ID', 'Purchase Report Get By Id', 'Purchase Report Get By Id', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- SALE
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'SALE'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_SALE', 'Sotuvni bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_SALE', 'Sotuvni tasdiqlash', 'Tasdiqlash', 0, false),
        ('SALE_DOC_CREATE', 'Sotuv hujjati yaratish', 'Yangi', 0, false),
        ('SALE_DOC_DELETE', 'Sotuv hujjati o''chirish', 'O''chirish', 0, false),
        ('SALE_DOC_TABLE_CREATE', 'Sotuv satri yaratish', 'Yangi', 0, false),
        ('SALE_DOC_TABLE_DELETE', 'Sotuv satri o''chirish', 'O''chirish', 0, false),
        ('SALE_DOC_TABLE_UPDATE', 'Sotuv satri tahrirlash', 'Tahrirlash', 0, false),
        ('SALE_DOC_TABLE_VIEW', 'Sotuv satrlari', 'Ro''yxat', 0, false),
        ('SALE_DOC_TABLE_VIEW_DETAIL', 'Sotuv satri detail', 'Batafsil', 0, false),
        ('SALE_DOC_UPDATE', 'Sotuv hujjati tahrirlash', 'Tahrirlash', 0, false),
        ('SALE_DOC_VIEW', 'Sotuv hujjatlari', 'Ro''yxat', 0, false),
        ('SALE_DOC_VIEW_DETAIL', 'Sotuv hujjati detail', 'Batafsil', 0, false),
        ('SALE_DOC_WAREHOUSE_CONFIRM', 'Sale Doc Warehouse Confirm', 'Sale Doc Warehouse Confirm', 0, false),
        ('SALES_REPORT_EXPORT', 'Sales Report Export', 'Sales Report Export', 0, false),
        ('SALES_REPORT_GET_ALL', 'Sales Report Get All', 'Sales Report Get All', 0, false),
        ('SALES_REPORT_GET_BY_ID', 'Sales Report Get By Id', 'Sales Report Get By Id', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- RETAIL_SALE
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'RETAIL_SALE'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_RETAIL_SALE', 'Chakana savdoni bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_RETAIL_SALE', 'Chakana savdoni tasdiqlash', 'Tasdiqlash', 0, false),
        ('RETAIL_SALE_DOC_CREATE', 'Chakana savdo yaratish', 'Yangi', 0, false),
        ('RETAIL_SALE_DOC_DELETE', 'Chakana savdo o''chirish', 'O''chirish', 0, false),
        ('RETAIL_SALE_DOC_UPDATE', 'Chakana savdo tahrirlash', 'Tahrirlash', 0, false),
        ('RETAIL_SALE_DOC_VIEW', 'Chakana savdo', 'Ro''yxat', 0, false),
        ('RETAIL_SALE_DOC_VIEW_DETAIL', 'Chakana savdo detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- ACCOUNTING
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'ACCOUNTING'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('ACCOUNTING_PERIOD_CLOSE', 'Accounting Period Close', 'Accounting Period Close', 0, false),
        ('ACCOUNTING_PERIOD_REOPEN', 'Accounting Period Reopen', 'Accounting Period Reopen', 0, false),
        ('ACCOUNTING_REPORT_GET_ACCOUNT_CARD', 'Accounting Report Get Account Card', 'Accounting Report Get Account Card', 0, false),
        ('ACCOUNTING_REPORT_GET_ACCOUNT_TURNOVER', 'Accounting Report Get Account Turnover', 'Accounting Report Get Account Turnover', 0, false),
        ('ACCOUNTING_REPORT_GET_BALANCE_SHEET', 'Accounting Report Get Balance Sheet', 'Accounting Report Get Balance Sheet', 0, false),
        ('ACCOUNTING_REPORT_GET_CASH_FLOW', 'Accounting Report Get Cash Flow', 'Accounting Report Get Cash Flow', 0, false),
        ('ACCOUNTING_REPORT_GET_INCOME_STATEMENT', 'Accounting Report Get Income Statement', 'Accounting Report Get Income Statement', 0, false),
        ('ACCOUNTING_REPORT_GET_JOURNAL', 'Accounting Report Get Journal', 'Accounting Report Get Journal', 0, false),
        ('CHART_ACCOUNT_CREATE', 'Hisob yaratish', 'Yangi', 0, false),
        ('CHART_ACCOUNT_DELETE', 'Hisob o''chirish', 'O''chirish', 0, false),
        ('CHART_ACCOUNT_UPDATE', 'Hisob tahrirlash', 'Tahrirlash', 0, false),
        ('CHART_ACCOUNT_VIEW', 'Hisoblar rejasi', 'Ro''yxat', 0, false),
        ('CHART_ACCOUNT_VIEW_DETAIL', 'Hisoblar rejasi detail', 'Batafsil', 0, false),
        ('FINANCIAL_REPORT_BALANCE_SHEET', 'Financial Report Balance Sheet', 'Financial Report Balance Sheet', 0, false),
        ('FINANCIAL_REPORT_CARD', 'Financial Report Card', 'Financial Report Card', 0, false),
        ('FINANCIAL_REPORT_CASH_FLOW', 'Financial Report Cash Flow', 'Financial Report Cash Flow', 0, false),
        ('FINANCIAL_REPORT_EXPORT_BALANCE_SHEET', 'Financial Report Export Balance Sheet', 'Financial Report Export Balance Sheet', 0, false),
        ('FINANCIAL_REPORT_EXPORT_CARD', 'Financial Report Export Card', 'Financial Report Export Card', 0, false),
        ('FINANCIAL_REPORT_EXPORT_CASH_FLOW', 'Financial Report Export Cash Flow', 'Financial Report Export Cash Flow', 0, false),
        ('FINANCIAL_REPORT_EXPORT_INCOME_STATEMENT', 'Financial Report Export Income Statement', 'Financial Report Export Income Statement', 0, false),
        ('FINANCIAL_REPORT_EXPORT_JOURNAL', 'Financial Report Export Journal', 'Financial Report Export Journal', 0, false),
        ('FINANCIAL_REPORT_EXPORT_TURNOVER', 'Financial Report Export Turnover', 'Financial Report Export Turnover', 0, false),
        ('FINANCIAL_REPORT_INCOME_STATEMENT', 'Financial Report Income Statement', 'Financial Report Income Statement', 0, false),
        ('FINANCIAL_REPORT_JOURNAL', 'Financial Report Journal', 'Financial Report Journal', 0, false),
        ('FINANCIAL_REPORT_TURNOVER', 'Financial Report Turnover', 'Financial Report Turnover', 0, false),
        ('PAYABLE_REPORT_BALANCE_BY_ID', 'Payable Report Balance By Id', 'Payable Report Balance By Id', 0, false),
        ('PAYABLE_REPORT_BALANCES', 'Payable Report Balances', 'Payable Report Balances', 0, false),
        ('PAYABLE_REPORT_EXPORT_BALANCES', 'Payable Report Export Balances', 'Payable Report Export Balances', 0, false),
        ('RECEIVABLE_REPORT_BALANCE_BY_ID', 'Receivable Report Balance By Id', 'Receivable Report Balance By Id', 0, false),
        ('RECEIVABLE_REPORT_BALANCES', 'Receivable Report Balances', 'Receivable Report Balances', 0, false),
        ('RECEIVABLE_REPORT_EXPORT_BALANCES', 'Receivable Report Export Balances', 'Receivable Report Export Balances', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- REGISTER
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'REGISTER'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('ACCOUNTING_REGISTER_ENTRY_GET_DAILY_POSTINGS', 'Accounting Register Entry Get Daily Postings', 'Accounting Register Entry Get Daily Postings', 0, false),
        ('ACCOUNTING_REGISTER_ENTRY_GET_POSTINGS', 'Accounting Register Entry Get Postings', 'Accounting Register Entry Get Postings', 0, false),
        ('CASH_BOOK_GET', 'Cash Book Get', 'Cash Book Get', 0, false),
        ('COUNTERPARTY_REG_BALANCE_CREATE', 'Kontragent qoldig''i yaratish', 'Yangi', 0, false),
        ('COUNTERPARTY_REG_BALANCE_DELETE', 'Kontragent qoldig''i o''chirish', 'O''chirish', 0, false),
        ('COUNTERPARTY_REG_BALANCE_UPDATE', 'Kontragent qoldig''i tahrirlash', 'Tahrirlash', 0, false),
        ('COUNTERPARTY_REG_BALANCE_VIEW', 'Kontragent qoldiqlari', 'Ro''yxat', 0, false),
        ('COUNTERPARTY_REG_BALANCE_VIEW_DETAIL', 'Kontragent qoldig''i detail', 'Batafsil', 0, false),
        ('LEDGER_GET', 'Ledger Get', 'Ledger Get', 0, false),
        ('MONEY_REG_BALANCE_CREATE', 'Pul qoldig''i yaratish', 'Yangi', 0, false),
        ('MONEY_REG_BALANCE_DELETE', 'Pul qoldig''i o''chirish', 'O''chirish', 0, false),
        ('MONEY_REG_BALANCE_UPDATE', 'Pul qoldig''i tahrirlash', 'Tahrirlash', 0, false),
        ('MONEY_REG_BALANCE_VIEW', 'Pul qoldiqlari', 'Ro''yxat', 0, false),
        ('MONEY_REG_BALANCE_VIEW_DETAIL', 'Pul qoldig''i detail', 'Batafsil', 0, false),
        ('REPOST_REPOST', 'Repost Repost', 'Repost Repost', 0, false),
        ('TRIAL_BALANCE_GET', 'Trial Balance Get', 'Trial Balance Get', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- MANUAL
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'MANUAL'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('BARCODE_GENERATE_CODE128', 'Barcode Generate Code128', 'Barcode Generate Code128', 0, false),
        ('BARCODE_GENERATE_EAN13', 'Barcode Generate Ean13', 'Barcode Generate Ean13', 0, false),
        ('BARCODE_GENERATE_QR', 'Barcode Generate Qr', 'Barcode Generate Qr', 0, false),
        ('CURRENCY_CREATE', 'Valyuta yaratish', 'Yangi valyuta qo''shish', 0, false),
        ('CURRENCY_DELETE', 'Valyuta o''chirish', 'Valyutani o''chirish', 0, false),
        ('CURRENCY_RATE_CREATE', 'Valyuta kursi yaratish', 'Yangi valyuta kursi qo''shish', 52, false),
        ('CURRENCY_RATE_DELETE', 'Valyuta kursi o''chirish', 'Valyuta kursini o''chirish', 54, false),
        ('CURRENCY_RATE_IMPORT', 'Valyuta kursini import qilish', 'Markaziy bankdan valyuta kursini import qilish', 0, false),
        ('CURRENCY_RATE_SYNC', 'Valyuta kursini sinxronlash', 'Markaziy bankdan valyuta kursini sanaga ko''ra sinxronlash', 0, false),
        ('CURRENCY_RATE_UPDATE', 'Valyuta kursi tahrirlash', 'Valyuta kursini tahrirlash', 53, false),
        ('CURRENCY_RATE_VIEW', 'Valyuta kurslari', 'Valyuta kurslari ro''yxati', 50, false),
        ('CURRENCY_RATE_VIEW_DETAIL', 'Valyuta kursi detail', 'Valyuta kursini batafsil ko''rish', 51, false),
        ('CURRENCY_RATES_GET_HISTORY', 'Currency Rates Get History', 'Currency Rates Get History', 0, false),
        ('CURRENCY_RATES_GET_IMPORT_STATUS', 'Currency Rates Get Import Status', 'Currency Rates Get Import Status', 0, false),
        ('CURRENCY_RATES_GET_LATEST', 'Currency Rates Get Latest', 'Currency Rates Get Latest', 0, false),
        ('CURRENCY_RATES_GET_PROVIDERS', 'Currency Rates Get Providers', 'Currency Rates Get Providers', 0, false),
        ('CURRENCY_REVALUATION_CANCEL', 'Valyuta qayta baholash bekor qilish', 'Valyuta qayta baholashni bekor qilish', 0, false),
        ('CURRENCY_REVALUATION_CONFIRM', 'Valyuta qayta baholash tasdiqlash', 'Valyuta qayta baholashni tasdiqlash', 0, false),
        ('CURRENCY_REVALUATION_CREATE', 'Valyuta qayta baholash yaratish', 'Valyuta qayta baholash preview va yaratish', 0, false),
        ('CURRENCY_REVALUATION_PREVIEW', 'Currency Revaluation Preview', 'Currency Revaluation Preview', 0, false),
        ('CURRENCY_REVALUATION_VIEW', 'Valyuta qayta baholash', 'Valyuta qayta baholash ro''yxati', 0, false),
        ('CURRENCY_REVALUATION_VIEW_DETAIL', 'Currency Revaluation View Detail', 'Currency Revaluation View Detail', 0, false),
        ('CURRENCY_UPDATE', 'Valyuta tahrirlash', 'Valyutani tahrirlash', 0, false),
        ('CURRENCY_VIEW', 'Valyutalar', 'Valyutalar ro''yxati', 0, false),
        ('CURRENCY_VIEW_DETAIL', 'Valyuta detail', 'Valyutani batafsil ko''rish', 0, false),
        ('MANUAL_GET_ACCOUNTING_POLICIES', 'Manual Get Accounting Policies', 'Manual Get Accounting Policies', 0, false),
        ('MANUAL_GET_BANKS', 'Manual Get Banks', 'Manual Get Banks', 0, false),
        ('MANUAL_GET_BANK_TERMINALS', 'Manual Get Bank Terminals', 'Manual Get Bank Terminals', 0, false),
        ('MANUAL_GET_BRANCHES', 'Manual Get Branches', 'Manual Get Branches', 0, false),
        ('MANUAL_GET_CASH_BOXES', 'Manual Get Cash Boxes', 'Manual Get Cash Boxes', 0, false),
        ('MANUAL_GET_CASH_OPERATIONS', 'Manual Get Cash Operations', 'Manual Get Cash Operations', 0, false),
        ('MANUAL_GET_CHART_ACCOUNTS', 'Manual Get Chart Accounts', 'Manual Get Chart Accounts', 0, false),
        ('MANUAL_GET_CLIENTS', 'Manual Get Clients', 'Manual Get Clients', 0, false),
        ('MANUAL_GET_CONTRACT_TYPES', 'Manual Get Contract Types', 'Manual Get Contract Types', 0, false),
        ('MANUAL_GET_CONTRACTS', 'Manual Get Contracts', 'Manual Get Contracts', 0, false),
        ('MANUAL_GET_COSTING_METHODS', 'Manual Get Costing Methods', 'Manual Get Costing Methods', 0, false),
        ('MANUAL_GET_COUNTERPARTIES', 'Manual Get Counterparties', 'Manual Get Counterparties', 0, false),
        ('MANUAL_GET_COUNTERPARTY_BANK_ACCOUNTS', 'Manual Get Counterparty Bank Accounts', 'Manual Get Counterparty Bank Accounts', 0, false),
        ('MANUAL_GET_COUNTERPARTY_TYPES', 'Manual Get Counterparty Types', 'Manual Get Counterparty Types', 0, false),
        ('MANUAL_GET_CURRENCIES', 'Manual Get Currencies', 'Manual Get Currencies', 0, false),
        ('MANUAL_GET_DEPARTMENTS', 'Manual Get Departments', 'Manual Get Departments', 0, false),
        ('MANUAL_GET_DISTRICTS', 'Manual Get Districts', 'Manual Get Districts', 0, false),
        ('MANUAL_GET_DOCUMENT_STATUSES', 'Manual Get Document Statuses', 'Manual Get Document Statuses', 0, false),
        ('MANUAL_GET_DOCUMENT_TYPES', 'Manual Get Document Types', 'Manual Get Document Types', 0, false),
        ('MANUAL_GET_FA_DEPRECIATION_METHODS', 'Manual Get FA Depreciation Methods', 'Manual Get FA Depreciation Methods', 0, false),
        ('MANUAL_GET_FA_GROUPS', 'Manual Get FA Groups', 'Manual Get FA Groups', 0, false),
        ('MANUAL_GET_FA_OKOFS', 'Manual Get FA Okofs', 'Manual Get FA Okofs', 0, false),
        ('MANUAL_GET_FISCAL_CASH_REGISTERS', 'Manual Get Fiscal Cash Registers', 'Manual Get Fiscal Cash Registers', 0, false),
        ('MANUAL_GET_FISCAL_CASH_REGISTER_TYPES', 'Manual Get Fiscal Cash Register Types', 'Manual Get Fiscal Cash Register Types', 0, false),
        ('MANUAL_GET_INVENTORY_ADJUSTMENT_TYPES', 'Manual Get Inventory Adjustment Types', 'Manual Get Inventory Adjustment Types', 0, false),
        ('MANUAL_GET_LANGUAGES', 'Manual Get Languages', 'Manual Get Languages', 0, false),
        ('MANUAL_GET_MODULE_SUB_GROUPS', 'Manual Get Module Sub Groups', 'Manual Get Module Sub Groups', 0, false),
        ('MANUAL_GET_OPERATION_TYPES', 'Manual Get Operation Types', 'Manual Get Operation Types', 0, false),
        ('MANUAL_GET_ORG_BANK_ACCOUNTS', 'Manual Get Org Bank Accounts', 'Manual Get Org Bank Accounts', 0, false),
        ('MANUAL_GET_ORGANIZATIONS', 'Manual Get Organizations', 'Manual Get Organizations', 0, false),
        ('MANUAL_GET_PAYMENT_TYPES', 'Manual Get Payment Types', 'Manual Get Payment Types', 0, false),
        ('MANUAL_GET_PAYMENT_METHODS', 'Manual Get Payment Methods', 'Manual Get Payment Methods', 0, false),
        ('MANUAL_GET_POSITIONS', 'Manual Get Positions', 'Manual Get Positions', 0, false),
        ('MANUAL_GET_PRICE_ROUNDING_METHODS', 'Manual Get Price Rounding Methods', 'Manual Get Price Rounding Methods', 0, false),
        ('MANUAL_GET_PRICING_METHODS', 'Manual Get Pricing Methods', 'Manual Get Pricing Methods', 0, false),
        ('MANUAL_GET_PRODUCT_GROUPS', 'Manual Get Product Groups', 'Manual Get Product Groups', 0, false),
        ('MANUAL_GET_PRODUCTS', 'Manual Get Products', 'Manual Get Products', 0, false),
        ('MANUAL_GET_REGIONS', 'Manual Get Regions', 'Manual Get Regions', 0, false),
        ('MANUAL_GET_ROLES', 'Manual Get Roles', 'Manual Get Roles', 0, false),
        ('MANUAL_GET_SOURCE_PRODUCT_TABLES', 'Manual Get Source Product Tables', 'Manual Get Source Product Tables', 0, false),
        ('MANUAL_GET_STATES', 'Manual Get States', 'Manual Get States', 0, false),
        ('MANUAL_GET_SUPPLIERS', 'Manual Get Suppliers', 'Manual Get Suppliers', 0, false),
        ('MANUAL_GET_TAX_TYPES', 'Manual Get Tax Types', 'Manual Get Tax Types', 0, false),
        ('MANUAL_GET_UNITS', 'Manual Get Units', 'Manual Get Units', 0, false),
        ('MANUAL_GET_USERS', 'Manual Get Users', 'Manual Get Users', 0, false),
        ('MANUAL_GET_VAT_RATES', 'Manual Get Vat Rates', 'Manual Get Vat Rates', 0, false),
        ('MANUAL_GET_WAREHOUSES', 'Manual Get Warehouses', 'Manual Get Warehouses', 0, false),
        ('TAX_CALCULATE', 'Tax Calculate', 'Tax Calculate', 0, false),
        ('TAX_CANCEL_E_FAKTURA', 'Tax Cancel E Faktura', 'Tax Cancel E Faktura', 0, false),
        ('TAX_CREATE', 'Soliq yaratish', 'Yangi soliq qo''shish', 0, false),
        ('TAX_DELETE', 'Soliq o''chirish', 'Soliqni o''chirish', 0, false),
        ('TAX_GET_E_FAKTURA_STATUS', 'Tax Get E Faktura Status', 'Tax Get E Faktura Status', 0, false),
        ('TAX_GET_MXIK_BY_CODE', 'Tax Get Mxik By Code', 'Tax Get Mxik By Code', 0, false),
        ('TAX_GET_PROVIDER_STATUS', 'Tax Get Provider Status', 'Tax Get Provider Status', 0, false),
        ('TAX_GET_PROVIDERS', 'Tax Get Providers', 'Tax Get Providers', 0, false),
        ('TAX_RESOLVE', 'Tax Resolve', 'Tax Resolve', 0, false),
        ('TAX_SEARCH_MXIK', 'Tax Search Mxik', 'Tax Search Mxik', 0, false),
        ('TAX_SEARCH_SOLIQ', 'Tax Search Soliq', 'Tax Search Soliq', 0, false),
        ('TAX_SUBMIT_E_FAKTURA', 'Tax Submit E Faktura', 'Tax Submit E Faktura', 0, false),
        ('TAX_UPDATE', 'Soliq tahrirlash', 'Soliqni tahrirlash', 0, false),
        ('TAX_VIEW', 'Soliqlar', 'Soliqlar ro''yxati', 0, false),
        ('TAX_VIEW_DETAIL', 'Soliq detail', 'Soliqni batafsil ko''rish', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- WAREHOUSE_TRANSFER
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'WAREHOUSE_TRANSFER'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_WAREHOUSE_TRANSFER', 'Ombor ko''chirishni bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_WAREHOUSE_TRANSFER', 'Ombor ko''chirishni tasdiqlash', 'Tasdiqlash', 0, false),
        ('WAREHOUSE_REPORT_COUNT_BY_ID', 'Warehouse Report Count By Id', 'Warehouse Report Count By Id', 0, false),
        ('WAREHOUSE_REPORT_COUNTS', 'Warehouse Report Counts', 'Warehouse Report Counts', 0, false),
        ('WAREHOUSE_REPORT_EXPORT_COUNTS', 'Warehouse Report Export Counts', 'Warehouse Report Export Counts', 0, false),
        ('WAREHOUSE_REPORT_EXPORT_TRANSFERS', 'Warehouse Report Export Transfers', 'Warehouse Report Export Transfers', 0, false),
        ('WAREHOUSE_REPORT_TRANSFER_BY_ID', 'Warehouse Report Transfer By Id', 'Warehouse Report Transfer By Id', 0, false),
        ('WAREHOUSE_REPORT_TRANSFERS', 'Warehouse Report Transfers', 'Warehouse Report Transfers', 0, false),
        ('WAREHOUSE_TRANSFER_CREATE', 'Ombor ko''chirish yaratish', 'Yangi', 0, false),
        ('WAREHOUSE_TRANSFER_DELETE', 'Ombor ko''chirish o''chirish', 'O''chirish', 0, false),
        ('WAREHOUSE_TRANSFER_GET_INVENTORY_MOVEMENTS', 'Warehouse Transfer Get Inventory Movements', 'Warehouse Transfer Get Inventory Movements', 0, false),
        ('WAREHOUSE_TRANSFER_GET_POSTING_BATCHES', 'Warehouse Transfer Get Posting Batches', 'Warehouse Transfer Get Posting Batches', 0, false),
        ('WAREHOUSE_TRANSFER_UPDATE', 'Ombor ko''chirish tahrirlash', 'Tahrirlash', 0, false),
        ('WAREHOUSE_TRANSFER_VIEW', 'Ombor ko''chirishlar', 'Ro''yxat', 0, false),
        ('WAREHOUSE_TRANSFER_VIEW_DETAIL', 'Ombor ko''chirish detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- INVENTORY_ADJUSTMENT
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'INVENTORY_ADJUSTMENT'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_INVENTORY_ADJUSTMENT', 'Inventar tuzatishni bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_INVENTORY_ADJUSTMENT', 'Inventar tuzatishni tasdiqlash', 'Tasdiqlash', 0, false),
        ('INVENTORY_ADJUSTMENT_CREATE', 'Inventar tuzatish yaratish', 'Yangi', 0, false),
        ('INVENTORY_ADJUSTMENT_DELETE', 'Inventar tuzatish o''chirish', 'O''chirish', 0, false),
        ('INVENTORY_ADJUSTMENT_GET_INVENTORY_MOVEMENTS', 'Inventory Adjustment Get Inventory Movements', 'Inventory Adjustment Get Inventory Movements', 0, false),
        ('INVENTORY_ADJUSTMENT_GET_POSTING_BATCHES', 'Inventory Adjustment Get Posting Batches', 'Inventory Adjustment Get Posting Batches', 0, false),
        ('INVENTORY_ADJUSTMENT_UPDATE', 'Inventar tuzatish tahrirlash', 'Tahrirlash', 0, false),
        ('INVENTORY_ADJUSTMENT_VIEW', 'Inventar tuzatishlar', 'Ro''yxat', 0, false),
        ('INVENTORY_ADJUSTMENT_VIEW_DETAIL', 'Inventar tuzatish detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- INVENTORY_COUNT
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'INVENTORY_COUNT'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('CANCEL_INVENTORY_COUNT', 'Inventar sanog''ini bekor qilish', 'Bekor qilish', 0, false),
        ('CONFIRM_INVENTORY_COUNT', 'Inventar sanog''ini tasdiqlash', 'Tasdiqlash', 0, false),
        ('INVENTORY_COUNT_CREATE', 'Inventar sanog''i yaratish', 'Yangi', 0, false),
        ('INVENTORY_COUNT_DELETE', 'Inventar sanog''i o''chirish', 'O''chirish', 0, false),
        ('INVENTORY_COUNT_GET_DIFFERENCES', 'Inventory Count Get Differences', 'Inventory Count Get Differences', 0, false),
        ('INVENTORY_COUNT_GET_INVENTORY_MOVEMENTS', 'Inventory Count Get Inventory Movements', 'Inventory Count Get Inventory Movements', 0, false),
        ('INVENTORY_COUNT_GET_POSTING_BATCHES', 'Inventory Count Get Posting Batches', 'Inventory Count Get Posting Batches', 0, false),
        ('INVENTORY_COUNT_UPDATE', 'Inventar sanog''i tahrirlash', 'Tahrirlash', 0, false),
        ('INVENTORY_COUNT_VIEW', 'Inventar sanog''i', 'Ro''yxat', 0, false),
        ('INVENTORY_COUNT_VIEW_DETAIL', 'Inventar sanog''i detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- DOCUMENT_ACCOUNT_SETTING
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'DOCUMENT_ACCOUNT_SETTING'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('DOCUMENT_ACCOUNT_SETTING_SAVE', 'Hujjat hisob sozlamasini saqlash', 'Saqlash', 0, false),
        ('DOCUMENT_ACCOUNT_SETTING_VIEW', 'Hujjat hisob sozlamalari', 'Ro''yxat', 0, false),
        ('DOCUMENT_ACCOUNT_SETTING_VIEW_DETAIL', 'Hujjat hisob sozlamasi detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- OPENING_BALANCE
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'OPENING_BALANCE'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('OPENING_BALANCE_CREATE', 'Boshlang''ich qoldiq yaratish', 'Yangi', 0, false),
        ('OPENING_BALANCE_DELETE', 'Boshlang''ich qoldiq o''chirish', 'O''chirish', 0, false),
        ('OPENING_BALANCE_UPDATE', 'Boshlang''ich qoldiq tahrirlash', 'Tahrirlash', 0, false),
        ('OPENING_BALANCE_VIEW', 'Boshlang''ich qoldiq', 'Ro''yxat', 0, false),
        ('OPENING_BALANCE_VIEW_DETAIL', 'Boshlang''ich qoldiq detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- OPENING_INVENTORY
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'OPENING_INVENTORY'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('OPENING_INVENTORY_CREATE', 'Boshlang''ich qoldiq yaratish', 'Boshlang''ich tovar qoldig''ini yaratish', 0, false),
        ('OPENING_INVENTORY_DELETE', 'Boshlang''ich qoldiq o''chirish', 'Boshlang''ich tovar qoldig''ini o''chirish', 0, false),
        ('OPENING_INVENTORY_UPDATE', 'Boshlang''ich qoldiq tahrirlash', 'Boshlang''ich tovar qoldig''ini tahrirlash', 0, false),
        ('OPENING_INVENTORY_VIEW', 'Boshlang''ich qoldiqlar', 'Boshlang''ich tovar qoldiqlarini ko''rish', 0, false),
        ('OPENING_INVENTORY_VIEW_DETAIL', 'Boshlang''ich qoldiq detail', 'Boshlang''ich tovar qoldig''ini ko''rish', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- HR
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'HR'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('HR_ABSENCE_CREATE', 'Yo''qlik yaratish', 'Ta''til, kasallik yoki boshqa yo''qlik yaratish', 8, false),
        ('HR_ABSENCE_DELETE', 'Yo''qlikni o''chirish', 'Yo''qlik hujjatini o''chirish', 10, false),
        ('HR_ABSENCE_UPDATE', 'Yo''qlikni tahrirlash', 'Yo''qlik va uning fayllarini tahrirlash', 9, false),
        ('HR_ABSENCE_VIEW', 'Ta''til va yo''qliklar', 'Ta''til, kasallik va boshqa yo''qliklarni ko''rish', 7, true),
        ('HR_CALENDAR_VIEW', 'Xodim kalendari', 'Ish, ta''til va kasallik kunlari kalendari', 11, true),
        ('HR_EMPLOYEE_CREATE', 'Xodim yaratish', 'Xodimni ishga qabul qilish', 2, false),
        ('HR_EMPLOYEE_DELETE', 'Xodimni bo''shatish', 'Xodimni faolsizlantirish', 4, false),
        ('HR_EMPLOYEE_UPDATE', 'Xodimni tahrirlash', 'Xodim va ishga qabul ma''lumotlarini tahrirlash', 3, false),
        ('HR_EMPLOYEE_VIEW', 'Xodimlar', 'Xodimlarni ko''rish', 1, true),
        ('HR_SCHEDULE_MANAGE', 'Ish grafiklarini boshqarish', 'Xodimlarning shaxsiy ish grafiklarini boshqarish', 6, false),
        ('HR_SCHEDULE_VIEW', 'Ish grafiklari', 'Xodimlarning shaxsiy ish grafiklarini ko''rish', 5, true),
        ('HR_VIEW', 'Kadrlar', 'Kadrlar moduli', 90, true)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- PAYROLL
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'PAYROLL'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('PAYROLL_COMPONENT_CREATE', 'Hisoblash turi yaratish', 'Hisoblash turi yaratish', 6, false),
        ('PAYROLL_COMPONENT_DELETE', 'Hisoblash turini ochirish', 'Hisoblash turini ochirish', 8, false),
        ('PAYROLL_COMPONENT_UPDATE', 'Hisoblash turini tahrirlash', 'Hisoblash turini tahrirlash', 7, false),
        ('PAYROLL_COMPONENT_VIEW', 'Hisoblash turlari', 'Hisoblash turlarini korish', 5, true),
        ('PAYROLL_DOCUMENT_CALCULATE', 'Oylikni hisoblash', 'Oylikni hisoblash', 17, false),
        ('PAYROLL_DOCUMENT_CANCEL', 'Oylikni bekor qilish', 'Oylikni storno qilish', 19, false),
        ('PAYROLL_DOCUMENT_CONFIRM', 'Oylikni tasdiqlash', 'Oylikni tasdiqlash va otkazish', 18, false),
        ('PAYROLL_DOCUMENT_DELETE', 'Oylik hujjatini ochirish', 'Draft oylik hujjatini ochirish', 20, false),
        ('PAYROLL_DOCUMENT_VIEW', 'Oylik hisoblash', 'Oylik hisoblash hujjatlarini korish', 16, true),
        ('PAYROLL_EMPLOYEE_CREATE', 'Xodim yaratish', 'Xodim yaratish', 2, false),
        ('PAYROLL_EMPLOYEE_DELETE', 'Xodimni ochirish', 'Xodimni ochirish', 4, false),
        ('PAYROLL_EMPLOYEE_UPDATE', 'Xodimni tahrirlash', 'Xodimni tahrirlash', 3, false),
        ('PAYROLL_EMPLOYEE_VIEW', 'Xodimlar', 'Xodimlarni korish', 1, true),
        ('PAYROLL_PAYMENT_CANCEL', 'Tolovni bekor qilish', 'Bank yoki kassa tolovini bekor qilish', 24, false),
        ('PAYROLL_PAYMENT_CONFIRM', 'Tolovni tasdiqlash', 'Bank yoki kassa tolovini tasdiqlash', 23, false),
        ('PAYROLL_PAYMENT_CREATE', 'Tolov yaratish', 'Avans yoki oylik tolovi yaratish', 22, false),
        ('PAYROLL_PAYMENT_VIEW', 'Oylik tolovlari', 'Oylik tolovlarini korish', 21, true),
        ('PAYROLL_PERIOD_MANAGE', 'Hisob davrini boshqarish', 'Hisob davrini ochish va yopish', 10, false),
        ('PAYROLL_PERIOD_VIEW', 'Hisob davrlari', 'Hisob davrlarini korish', 9, true),
        ('PAYROLL_REPORT_VIEW', 'Oylik hisobotlari', 'Vedomost va xodim hisob varaqasi', 25, true),
        ('PAYROLL_TIMESHEET_CANCEL', 'Tabelni bekor qilish', 'Tabelni bekor qilish', 15, false),
        ('PAYROLL_TIMESHEET_CONFIRM', 'Tabelni tasdiqlash', 'Tabelni tasdiqlash', 14, false),
        ('PAYROLL_TIMESHEET_CREATE', 'Tabel yaratish', 'Tabel yaratish', 12, false),
        ('PAYROLL_TIMESHEET_UPDATE', 'Tabelni tahrirlash', 'Tabelni tahrirlash', 13, false),
        ('PAYROLL_TIMESHEET_VIEW', 'Tabel', 'Tabellarni korish', 11, true),
        ('PAYROLL_VIEW', 'Oylik maosh', 'Oylik maosh moduli', 100, true)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- FA
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'FA'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('FA_ASSET_CREATE', 'Asosiy vosita yaratish', 'Yangi asosiy vosita qo''shish', 0, false),
        ('FA_ASSET_DELETE', 'Asosiy vosita o''chirish', 'Asosiy vositani o''chirish', 0, false),
        ('FA_ASSET_UPDATE', 'Asosiy vosita tahrirlash', 'Asosiy vositani tahrirlash', 0, false),
        ('FA_ASSET_VIEW', 'Asosiy vositalar', 'Asosiy vositalar ro''yxatini ko''rish', 0, false),
        ('FA_ASSET_VIEW_DETAIL', 'Asosiy vosita detail', 'Asosiy vositani batafsil ko''rish', 0, false),
        ('FA_DEPRECIATION_CANCEL', 'Fa Depreciation Cancel', 'Fa Depreciation Cancel', 0, false),
        ('FA_DEPRECIATION_RUN', 'Fa Depreciation Run', 'Fa Depreciation Run', 0, false),
        ('FA_DEPRECIATION_VIEW', 'Fa Depreciation View', 'Fa Depreciation View', 0, false),
        ('FA_DEPRECIATION_VIEW_DETAIL', 'Fa Depreciation View Detail', 'Fa Depreciation View Detail', 0, false),
        ('FA_DISPOSAL_CANCEL', 'Fa Disposal Cancel', 'Fa Disposal Cancel', 0, false),
        ('FA_DISPOSAL_CONFIRM', 'Fa Disposal Confirm', 'Fa Disposal Confirm', 0, false),
        ('FA_DISPOSAL_CREATE', 'Fa Disposal Create', 'Fa Disposal Create', 0, false),
        ('FA_DISPOSAL_UPDATE', 'Fa Disposal Update', 'Fa Disposal Update', 0, false),
        ('FA_DISPOSAL_VIEW', 'Fa Disposal View', 'Fa Disposal View', 0, false),
        ('FA_DISPOSAL_VIEW_DETAIL', 'Fa Disposal View Detail', 'Fa Disposal View Detail', 0, false),
        ('FA_MOVEMENT_CANCEL', 'Fa Movement Cancel', 'Fa Movement Cancel', 0, false),
        ('FA_MOVEMENT_CONFIRM', 'Fa Movement Confirm', 'Fa Movement Confirm', 0, false),
        ('FA_MOVEMENT_CREATE', 'Fa Movement Create', 'Fa Movement Create', 0, false),
        ('FA_MOVEMENT_UPDATE', 'Fa Movement Update', 'Fa Movement Update', 0, false),
        ('FA_MOVEMENT_VIEW', 'Fa Movement View', 'Fa Movement View', 0, false),
        ('FA_MOVEMENT_VIEW_DETAIL', 'Fa Movement View Detail', 'Fa Movement View Detail', 0, false),
        ('FA_RECEIPT_CANCEL', 'OS qabuli bekor qilish', 'Asosiy vosita qabul hujjatini bekor qilish', 0, false),
        ('FA_RECEIPT_CONFIRM', 'OS qabuli tasdiqlash', 'Asosiy vosita qabul hujjatini tasdiqlash', 0, false),
        ('FA_RECEIPT_CREATE', 'OS qabuli yaratish', 'Yangi asosiy vosita qabul hujjati yaratish', 0, false),
        ('FA_RECEIPT_DELETE', 'OS qabuli o''chirish', 'Asosiy vosita qabul hujjatini o''chirish', 0, false),
        ('FA_RECEIPT_UPDATE', 'OS qabuli tahrirlash', 'Asosiy vosita qabul hujjatini tahrirlash', 0, false),
        ('FA_RECEIPT_VIEW', 'OS qabuli', 'Asosiy vosita qabul hujjatlarini ko''rish', 0, false),
        ('FA_RECEIPT_VIEW_DETAIL', 'OS qabuli detail', 'Asosiy vosita qabul hujjatini batafsil ko''rish', 0, false),
        ('FA_REVALUATION_CANCEL', 'Fa Revaluation Cancel', 'Fa Revaluation Cancel', 0, false),
        ('FA_REVALUATION_CONFIRM', 'Fa Revaluation Confirm', 'Fa Revaluation Confirm', 0, false),
        ('FA_REVALUATION_CREATE', 'Fa Revaluation Create', 'Fa Revaluation Create', 0, false),
        ('FA_REVALUATION_UPDATE', 'Fa Revaluation Update', 'Fa Revaluation Update', 0, false),
        ('FA_REVALUATION_VIEW', 'Fa Revaluation View', 'Fa Revaluation View', 0, false),
        ('FA_REVALUATION_VIEW_DETAIL', 'Fa Revaluation View Detail', 'Fa Revaluation View Detail', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;
    
-- PRICING_CONDITION
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'PRICING_CONDITION'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('PRICING_CONDITION_CREATE', 'Narxlash qoidasi yaratish', 'Yangi', 0, false),
        ('PRICING_CONDITION_DELETE', 'Narxlash qoidasi o''chirish', 'O''chirish', 0, false),
        ('PRICING_CONDITION_GET_NOW', 'Pricing Condition Get Now', 'Pricing Condition Get Now', 0, false),
        ('PRICING_CONDITION_VIEW', 'Narxlash qoidasi', 'Ro''yxat', 0, false),
        ('PRICING_CONDITION_VIEW_DETAIL', 'Narxlash qoidasi detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

-- SALE_CONDITION
insert into sys_module (
    code,
    short_name,
    full_name,
    sub_group_id,
    state_id,
    created_date,
    sort_order,
    is_visible
)
select
    seed.code,
    seed.short_name,
    seed.full_name,
    (select id from sys_module_sub_group where code = 'SALE_CONDITION'),
    1,
    now(),
    seed.sort_order,
    seed.is_visible
from (
    values
        ('SALE_CONDITION_CREATE', 'Sotuv qoidasi yaratish', 'Yangi', 0, false),
        ('SALE_CONDITION_DELETE', 'Sotuv qoidasi o''chirish', 'O''chirish', 0, false),
        ('SALE_CONDITION_GET_NOW', 'Sale Condition Get Now', 'Sale Condition Get Now', 0, false),
        ('SALE_CONDITION_VIEW', 'Sotuv qoidasi', 'Ro''yxat', 0, false),
        ('SALE_CONDITION_VIEW_DETAIL', 'Sotuv qoidasi detail', 'Batafsil', 0, false)
) as seed (code, short_name, full_name, sort_order, is_visible)
on conflict (code) do update
set short_name = excluded.short_name,
    full_name = excluded.full_name,
    sub_group_id = excluded.sub_group_id,
    state_id = excluded.state_id,
    sort_order = excluded.sort_order,
    is_visible = excluded.is_visible;

commit;
