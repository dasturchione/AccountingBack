create table acc_subkonto_type 
(
    id smallint   not null,
    code character varying(50) not null,
    name character varying(150) not null,
    source_table character varying(100) not null,
    state_id smallint not null,
    created_date timestamp without time zone default now() not null,
    constraint acc_subkonto_type_pkey primary key (id),
    constraint acc_subkonto_type_state_id_fkey foreign key (state_id) references cmn_state(id)
);

create unique index idx_acc_subkonto_type_code on acc_subkonto_type using btree (code);
create index idx_acc_subkonto_type_state_id on acc_subkonto_type using btree (state_id);

begin;

insert into acc_subkonto_type
    (id, code, name, source_table, state_id, created_date)
values
    (1, 'fixed_assets', 'Asosiy vositalar', 'acc_fixed_assets', 1, now()),
    (2, 'intangible_assets', 'Nomoddiy aktivlar', 'acc_intangible_assets', 1, now()),
    (3, 'counterparties', 'Kontragentlar', 'counterparty_card', 1, now()),
    (4, 'contracts', 'Shartnomalar', 'cmn_contract', 1, now()),
    (5, 'securities', 'Qimmatli qogʻozlar', 'acc_securities', 1, now()),
    (6, 'inventory_items', 'Nomenklatura', 'inv_product', 1, now()),
    (7, 'batches', 'Partiyalar', 'acc_posting_batch', 1, now()),
    (8, 'warehouses', 'Omborlar', 'inv_warehouse', 1, now()),
    (9, 'construction_objects', 'Qurilish obyektlari', 'acc_construction_objects', 1, now()),
    (10, 'cost_items_turnover', 'Xarajat moddalari (aylanma)', 'acc_cost_items_turnover', 1, now()),
    (11, 'construction_methods_turnover', 'Qurilish usullari (aylanma)', 'acc_construction_methods_turnover', 1, now()),
    (12, 'organization_employees', 'Tashkilot xodimlari', 'sys_user', 1, now()),
    (13, 'acquisition_cost_items', 'Sotib olish xarajatlari moddalari', 'acc_acquisition_cost_items', 1, now()),
    (14, 'batches_turnover', 'Partiyalar (aylanma)', 'acc_batches_turnover', 1, now()),
    (15, 'product_groups', 'Nomenklatura guruhlari', 'inv_product_group', 1, now()),
    (16, 'products_turnover', 'Mahsulotlar (aylanma)', 'acc_products_turnover', 1, now()),
    (17, 'value_types_turnover', 'Qiymat turlari (aylanma)', 'acc_value_types_turnover', 1, now()),
    (18, 'inventory_items_turnover', 'Nomenklatura (aylanma)', 'acc_inventory_items_turnover', 1, now()),
    (19, 'vat_rates', 'QQS stavkalari', 'acc_vat_rates', 1, now()),
    (20, 'deferred_expenses', 'Kelgusi davr xarajatlari', 'acc_deferred_expenses', 1, now()),
    (21, 'separate_divisions', 'Alohida boʻlinmalar', 'acc_separate_divisions', 1, now()),
    (22, 'counterparties_turnover', 'Kontragentlar (aylanma)', 'acc_counterparties_turnover', 1, now()),
    (23, 'received_invoices_turnover', 'Olingan hisob-fakturalar (aylanma)', 'acc_received_invoices_turnover', 1, now()),
    (24, 'vat_accounting_methods', 'QQS hisobga olish usullari', 'acc_vat_accounting_methods', 1, now()),
    (25, 'sales_documents_turnover', 'Realizatsiya hujjatlari (aylanma)', 'acc_sales_documents_turnover', 1, now()),
    (26, 'budget_fund_payment_types', 'Budjetga (jamgʻarmalarga) toʻlov turlari', 'acc_budget_fund_payment_types', 1, now()),
    (27, 'tax_authority_registrations', 'Soliq organida roʻyxatdan oʻtish', 'acc_tax_authority_registrations', 1, now()),
    (28, 'excisable_product_types', 'Aksiz mahsulotlari turlari', 'acc_excisable_product_types', 1, now()),
    (29, 'regulated_obligations', 'Tartibga solinadigan majburiyatlar', 'cmn_regulated_obligation', 1, now()),
    (30, 'counterparty_settlement_documents', 'Kontragent bilan hisob-kitob hujjatlari', 'acc_counterparty_settlement_documents', 1, now()),
    (31, 'cash_flow_items_turnover', 'Pul mablagʻlari harakati moddalari (aylanma)', 'acc_cash_flow_items_turnover', 1, now()),
    (32, 'organization_cash_desks', 'Tashkilot kassalari', 'cash_box', 1, now()),
    (33, 'bank_accounts', 'Bank hisobvaraqlari', 'org_bank_account', 1, now()),
    (34, 'monetary_documents', 'Pul hujjatlari', 'acc_monetary_documents', 1, now()),
    (35, 'deferred_income', 'Kelgusi davr daromadlari', 'acc_deferred_income', 1, now()),
    (36, 'asset_liability_types', 'Aktivlar va majburiyatlar turlari', 'acc_asset_liability_types', 1, now()),
    (37, 'tax_authority_registrations_turnover', 'Soliq organida roʻyxatdan oʻtish (aylanma)', 'acc_tax_authority_registrations_turnover', 1, now()),
    (38, 'organization_employees_turnover', 'Tashkilot xodimlari (aylanma)', 'acc_organization_employees_turnover', 1, now()),
    (39, 'profit_usage_directions', 'Foydadan foydalanish yoʻnalishlari', 'acc_profit_usage_directions', 1, now()),
    (40, 'earmarked_funds_purposes', 'Maqsadli mablagʻlarning maqsadi', 'acc_earmarked_funds_purposes', 1, now()),
    (41, 'earmarked_funds_movements_turnover', 'Maqsadli mablagʻlar harakati (aylanma)', 'acc_earmarked_funds_movements_turnover', 1, now()),
    (42, 'estimated_liabilities_and_reserves', 'Baholangan majburiyatlar va zaxiralar', 'acc_estimated_liabilities_and_reserves', 1, now()),
    (43, 'product_groups_turnover', 'Nomenklatura guruhlari (aylanma)', 'acc_product_groups_turnover', 1, now()),
    (44, 'vat_rates_turnover', 'QQS stavkalari (aylanma)', 'acc_vat_rates_turnover', 1, now()),
    (45, 'cost_elements_turnover', 'Xarajat elementlari (aylanma)', 'acc_cost_elements_turnover', 1, now()),
    (46, 'other_income_and_expenses_turnover', 'Boshqa daromadlar va xarajatlar (aylanma)', 'acc_other_income_and_expenses_turnover', 1, now()),
    (47, 'fixed_assets_turnover', 'Asosiy vositalar (aylanma)', 'acc_fixed_assets_turnover', 1, now()),
    (48, 'other_income_and_expenses', 'Boshqa daromadlar va xarajatlar', 'acc_other_income_and_expenses', 1, now()),
    (49, 'profits_and_losses_turnover', 'Foyda va zararlar (aylanma)', 'acc_profits_and_losses_turnover', 1, now()),
    (50, 'strict_reporting_forms', 'Qatʼiy hisobot blankalari', 'acc_strict_reporting_forms', 1, now()),
    (51, 'customs_declaration_numbers', 'Bojxona yuk deklaratsiyasi raqamlari', 'acc_customs_declaration_numbers', 1, now()),
    (52, 'materials_in_use_batches', 'Foydalanishdagi materiallar partiyalari', 'acc_materials_in_use_batches', 1, now()),
    (53, 'countries_of_origin', 'Kelib chiqish mamlakatlari', 'acc_countries_of_origin', 1, now()),
    (54, 'depreciation_bonus_documents', 'Amortizatsiya mukofoti hujjatlari', 'acc_depreciation_bonus_documents', 1, now()),
    (55, 'unused_insurance_contribution_types', '(ishlatilmaydi) Sugʻurta badallari turlari', 'acc_unused_insurance_contribution_types', 1, now()),
    (56, 'unused_estimated_liabilities', '(ishlatilmaydi) Baholangan majburiyatlar', 'acc_unused_estimated_liabilities', 1, now());

commit;
