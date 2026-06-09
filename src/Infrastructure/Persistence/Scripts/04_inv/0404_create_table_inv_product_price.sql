create table inv_product_price
(
	id bigserial primary key,
	organization_id int not null references org_organization(id),
	product_id int not null references inv_product(id),
	currency_id smallint not null references cmn_currency(id),
	price numeric(18,2) not null,
	start_date timestamp without time zone default now() not null,
	end_date timestamp without time zone null,
	state_id smallint not null references cmn_state(id),
	created_date timestamp without time zone default now() not null
);

create index idx_inv_product_price_organization_id on inv_product_price (organization_id);
create index idx_inv_product_price_product_id on inv_product_price (product_id);
create index idx_inv_product_price_currency_id on inv_product_price (currency_id);
create index idx_inv_product_price_dates on inv_product_price (start_date, end_date);
create index idx_inv_product_price_state_id on inv_product_price (state_id);
