CREATE TABLE org_organization_config
(
  organization_id INT NOT NULL PRIMARY KEY REFERENCES org_organization (id), 
  inventory_valuation_method VARCHAR(20) NOT NULL DEFAULT 'fifo' CHECK (inventory_valuation_method IN ('fifo', 'lifo', 'average'))
);