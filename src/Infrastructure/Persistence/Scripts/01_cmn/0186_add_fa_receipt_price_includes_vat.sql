-- Fixed-asset receipts carry the same "prices already contain VAT" flag as sale, purchase
-- and retail documents (see 0185_add_price_includes_vat.sql). A supplier that quotes a
-- VAT-inclusive price would otherwise have the VAT added on top, overstating both the asset's
-- capital investment account and the payable to the supplier.
--
-- Defaults to false so every existing receipt keeps the meaning it was entered with.

alter table fa_receipt_doc
    add column if not exists price_includes_vat boolean default false not null;
