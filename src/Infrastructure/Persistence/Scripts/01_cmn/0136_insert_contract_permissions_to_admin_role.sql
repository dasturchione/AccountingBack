-- Admin rolga (role_id = 1) Contract permissionlarni biriktirish
INSERT INTO sys_role_module (role_id, module_id, created_date) VALUES
(1, 721, now()),
(1, 722, now()),
(1, 723, now()),
(1, 724, now()),
(1, 725, now())
ON CONFLICT (role_id, module_id) DO NOTHING;
