insert into cmn_purchase_item_type (id, code, name, state_id)
values
(1, 'product', 'Tovar', 1),
(2, 'service', 'Xizmat', 1)
on conflict do nothing;

select setval(
	pg_get_serial_sequence('cmn_purchase_item_type', 'id'),
	(select max(id) from cmn_purchase_item_type)
);

