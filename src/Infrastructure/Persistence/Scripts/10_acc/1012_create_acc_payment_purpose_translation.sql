-- Table: public.acc_payment_purpose_translation

CREATE TABLE public.acc_payment_purpose_translation (
    payment_purpose_id smallint NOT NULL,
    language_id smallint NOT NULL,
    name character varying(250) NOT NULL
);

insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (1, 1, 'Yetkazib beruvchiga to''lov');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (2, 1, 'Yetkazib beruvchiga avans');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (3, 1, 'Xaridordan to''lov');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (4, 1, 'Xaridordan avans');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (5, 1, 'Mehnat haqi');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (6, 1, 'Hisobdor summalar');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (7, 1, 'Dividendlar to''lovi');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (8, 1, 'Qarz berish');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (9, 1, 'Kreditni qaytarish');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (10, 1, 'Kredit olish');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (11, 1, 'QQS');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (12, 1, 'JShDS');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (13, 1, 'Foyda solig''i');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (14, 1, 'Mol-mulk solig''i');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (15, 1, 'Yer solig''i');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (16, 1, 'Ijtimoiy soliq (YaIJ)');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (17, 1, 'INPS');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (18, 1, 'Bank komissiyasi');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (1, 2, 'Оплата поставщику');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (2, 2, 'Аванс поставщику');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (3, 2, 'Оплата от клиента');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (4, 2, 'Аванс от клиента');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (5, 2, 'Заработная плата');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (6, 2, 'Подотчётные суммы');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (7, 2, 'Выплата дивидендов');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (8, 2, 'Выдача займа');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (9, 2, 'Погашение кредита');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (10, 2, 'Получение кредита');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (11, 2, 'НДС');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (12, 2, 'НДФЛ');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (13, 2, 'Налог на прибыль');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (14, 2, 'Налог на имущество');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (15, 2, 'Земельный налог');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (16, 2, 'Социальный налог (ЕСП)');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (17, 2, 'ИНПС');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (18, 2, 'Банковская комиссия');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (1, 3, 'Supplier payment');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (2, 3, 'Advance to supplier');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (3, 3, 'Payment from customer');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (4, 3, 'Advance from customer');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (5, 3, 'Salary');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (6, 3, 'Accountable amounts');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (7, 3, 'Dividend payment');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (8, 3, 'Loan given');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (9, 3, 'Loan repayment');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (10, 3, 'Loan received');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (11, 3, 'VAT');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (12, 3, 'Personal income tax');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (13, 3, 'Profit tax');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (14, 3, 'Property tax');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (15, 3, 'Land tax');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (16, 3, 'Social insurance tax');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (17, 3, 'Pension fund');
insert into public.acc_payment_purpose_translation (payment_purpose_id, language_id, name) values (18, 3, 'Bank fee');

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_pkey PRIMARY KEY (payment_purpose_id, language_id);

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_language_id_fkey FOREIGN KEY (language_id) REFERENCES public.cmn_language(id);

ALTER TABLE ONLY public.acc_payment_purpose_translation
    ADD CONSTRAINT acc_payment_purpose_translation_payment_purpose_id_fkey FOREIGN KEY (payment_purpose_id) REFERENCES public.acc_payment_purpose(id);
