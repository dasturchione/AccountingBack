-- Whether a document's prices already contain VAT.
--
-- Until now every document was treated as quoting prices before VAT and adding it on top. A
-- retail shelf price already contains it, so a 12 000 soum sale posted 12 000 to income plus
-- 1 440 of VAT while only 12 000 reached the till, leaving 1 440 stuck on the receivable.
--
-- The flag sits on the document, not on the VAT rate: the same 12% rate is quoted both ways,
-- and 1C likewise carries it per document («Сумма включает НДС»). It defaults to false so
-- every existing document keeps the meaning it was entered with.

alter table sale_doc
    add column if not exists price_includes_vat boolean default false not null;

alter table pur_doc
    add column if not exists price_includes_vat boolean default false not null;

alter table rtl_sale_doc
    add column if not exists price_includes_vat boolean default false not null;
