ALTER TABLE inv_product_table
DROP COLUMN code; 

ALTER TABLE inv_product_table
DROP COLUMN barcode;

ALTER TABLE inv_product_table
DROP COLUMN name;

ALTER TABLE inv_product_table
ADD COLUMN serial_number VARCHAR(250) NULL;

ALTER TABLE inv_product_table
ADD COLUMN marking_number VARCHAR(250) NULL;

CREATE UNIQUE INDEX ux_inv_product_table_org_serial
ON inv_product_table (organization_id, serial_number)
WHERE serial_number IS NOT NULL;

CREATE UNIQUE INDEX ux_inv_product_table_org_marking
ON inv_product_table (organization_id, marking_number)
WHERE marking_number IS NOT NULL;
