INSERT INTO sys_module (id, code, short_name, full_name, sub_group_id, state_id, created_date)
VALUES
(521, 'BANK_STATEMENT_PARSE', 'Bank statement import', 'Bank Excel statement faylini JSON qilib parse qilish', 5, 1, now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO sys_role_module (role_id, module_id, created_date)
SELECT r.id, m.id, now()
FROM sys_role r
CROSS JOIN sys_module m
WHERE m.code = 'BANK_STATEMENT_PARSE'
  AND (r.id = 1 OR r.has_global_access = TRUE)
ON CONFLICT (role_id, module_id) DO NOTHING;
