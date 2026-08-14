alter table fa_asset
	drop column source_product_table_id;

alter table fa_asset
    alter column initial_cost type numeric(24,8),
    alter column salvage_value type numeric(24,8);
