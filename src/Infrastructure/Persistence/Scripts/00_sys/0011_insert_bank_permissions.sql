INSERT INTO sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date)
VALUES
(531, 'BANK_VIEW',        'Banklar',          'Banklar ro''yxati',        5, 1, now()),
(532, 'BANK_VIEW_DETAIL', 'Bank detail',      'Bankni batafsil ko''rish', 5, 1, now()),
(533, 'BANK_CREATE',      'Bank yaratish',    'Yangi bank qo''shish',     5, 1, now()),
(534, 'BANK_UPDATE',      'Bank tahrirlash',  'Bankni tahrirlash',        5, 1, now()),
(535, 'BANK_DELETE',      'Bank o''chirish',  'Bankni o''chirish',        5, 1, now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO sys_role_module (role_id, module_id, created_date)
SELECT r.id, m.id, now()
FROM sys_role r
CROSS JOIN sys_module m
WHERE m.code IN (
    'BANK_VIEW',
    'BANK_VIEW_DETAIL',
    'BANK_CREATE',
    'BANK_UPDATE',
    'BANK_DELETE'
)
  AND (r.id = 1 OR r.has_global_access = TRUE)
ON CONFLICT (role_id, module_id) DO NOTHING;
