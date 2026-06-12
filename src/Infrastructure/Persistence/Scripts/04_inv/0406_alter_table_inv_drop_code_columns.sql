-- inv_product_group: drop unique index and code column
DROP INDEX IF EXISTS idx_inv_product_group_org_code;
ALTER TABLE inv_product_group DROP COLUMN IF EXISTS code;

-- inv_product: drop unique index and code column
DROP INDEX IF EXISTS idx_inv_product_org_code;
ALTER TABLE inv_product DROP COLUMN IF EXISTS code;

-- inv_product_table: drop code column (no index)
ALTER TABLE inv_product_table DROP COLUMN IF EXISTS code;

-- inv_warehouse: drop unique index and code column
DROP INDEX IF EXISTS idx_inv_warehouse_org_code;
ALTER TABLE inv_warehouse DROP COLUMN IF EXISTS code;
