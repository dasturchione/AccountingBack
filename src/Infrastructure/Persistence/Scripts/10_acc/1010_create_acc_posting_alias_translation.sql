-- Table: public.acc_posting_alias_translation

CREATE TABLE public.acc_posting_alias_translation (
    posting_alias_id smallint NOT NULL,
    language_id smallint NOT NULL,
    name character varying(250) NOT NULL
);

insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (1, 1, 'Tovar ombor qoldig''i');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (2, 1, 'Xarajat (xizmat)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (3, 1, 'Yetkazib beruvchi — qarzni yopish');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (4, 1, 'Yetkazib beruvchiga avans');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (5, 1, 'Xaridor — qarzni yopish');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (6, 1, 'Xaridordan avans');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (7, 1, 'Tovarlar realizatsiyasidan daromad');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (8, 1, 'Xizmatlardan daromad');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (9, 1, 'Hisobga olinadigan QQS');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (10, 1, 'To''lanadigan QQS');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (11, 1, 'Sotilgan tovarlar tannarxi');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (12, 1, 'Ko''rsatilgan xizmatlar tannarxi');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (13, 1, 'Aktivni hisobdan chiqarish (tovar/OS)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (14, 1, 'Pul hisobvarag''i (bank/kassa)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (15, 1, 'Xodim — mehnat haqi');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (16, 1, 'Xodim — hisobdor summalar');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (17, 1, 'Asoschi — dividendlar');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (18, 1, 'Berilgan qarz');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (19, 1, 'Olingan kredit/qarz');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (20, 1, 'QQS (byudjet)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (21, 1, 'JShDS');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (22, 1, 'Foyda solig''i');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (23, 1, 'Aktsiz solig''i');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (24, 1, 'Mol-mulk solig''i');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (25, 1, 'Yer solig''i');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (26, 1, 'Boshqa soliq va yig''imlar');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (27, 1, 'Ijtimoiy soliq (YaIJ)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (28, 1, 'INPS');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (29, 1, 'Bank komissiyasi');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (1, 2, 'Товар на складе');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (2, 2, 'Расход (услуга)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (3, 2, 'Поставщик — погашение долга');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (4, 2, 'Поставщик — аванс');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (5, 2, 'Покупатель — погашение долга');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (6, 2, 'Покупатель — аванс');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (7, 2, 'Доход от реализации товаров');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (8, 2, 'Доход от услуг');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (9, 2, 'НДС к зачёту');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (10, 2, 'НДС к уплате');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (11, 2, 'Себестоимость реализованных товаров');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (12, 2, 'Себестоимость услуг');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (13, 2, 'Списание актива (товар/ОС)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (14, 2, 'Денежный счёт (банк/касса)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (15, 2, 'Сотрудник — оплата труда');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (16, 2, 'Сотрудник — подотчётные суммы');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (17, 2, 'Учредитель — дивиденды');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (18, 2, 'Заём выданный');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (19, 2, 'Кредит/заём полученный');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (20, 2, 'НДС (бюджет)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (21, 2, 'НДФЛ');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (22, 2, 'Налог на прибыль');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (23, 2, 'Акцизы');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (24, 2, 'Налог на имущество');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (25, 2, 'Земельный налог');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (26, 2, 'Прочие налоги и сборы');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (27, 2, 'Социальный налог (ЕСП)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (28, 2, 'ИНПС');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (29, 2, 'Банковская комиссия');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (1, 3, 'Inventory on stock');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (2, 3, 'Expense (service)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (3, 3, 'Supplier — debt settlement');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (4, 3, 'Advance to supplier');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (5, 3, 'Customer — debt settlement');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (6, 3, 'Advance from customer');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (7, 3, 'Revenue from goods sales');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (8, 3, 'Revenue from services');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (9, 3, 'Input VAT');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (10, 3, 'Output VAT');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (11, 3, 'Cost of goods sold');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (12, 3, 'Cost of services rendered');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (13, 3, 'Asset write-off (inventory/fixed asset)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (14, 3, 'Cash account (bank/cash)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (15, 3, 'Employee — payroll');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (16, 3, 'Employee — accountable amounts');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (17, 3, 'Founder — dividends');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (18, 3, 'Loan given');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (19, 3, 'Loan/credit received');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (20, 3, 'VAT (budget)');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (21, 3, 'Personal income tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (22, 3, 'Profit tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (23, 3, 'Excise tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (24, 3, 'Property tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (25, 3, 'Land tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (26, 3, 'Other taxes and fees');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (27, 3, 'Social insurance tax');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (28, 3, 'Pension fund');
insert into public.acc_posting_alias_translation (posting_alias_id, language_id, name) values (29, 3, 'Bank fee');

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_pkey PRIMARY KEY (posting_alias_id, language_id);

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);

ALTER TABLE ONLY public.acc_posting_alias_translation
    ADD CONSTRAINT acc_posting_alias_translation_posting_alias_id_fkey FOREIGN KEY (posting_alias_id) REFERENCES public.acc_posting_alias(id);
