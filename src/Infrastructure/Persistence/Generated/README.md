# Generated persistence model

This directory contains the historical database-scaffold model. It is retained
for reference and comparison only.

The application uses `Infrastructure.Persistence.AppDbContext`, registered by
the WebApi dependency-injection configuration. `Infrastructure.Persistence.Generated.AppDbContext`
is not registered and is not the runtime owner of persistence mappings.

Do not overwrite or regenerate this scaffold as part of ordinary schema or
feature work. Any differences from the public PostgreSQL schema must be
audited first. Changes to the active model belong in the canonical context or
domain model, and database changes require a separately reviewed migration.

The scaffold may remain behind the active schema when tables are intentionally
technical, legacy, or not required by the application. Missing scaffold
classes are not, by themselves, evidence that a table should be created or
that the active context should gain a new `DbSet`.

## Known active-schema drift

The following differences are intentionally tracked outside the generated
scaffold and do not authorize DDL changes:

- `inv_warehouse_product.available_quantity` is nullable in the database
  catalog because it is a stored computed value; the active domain contract
  treats the calculated quantity as non-null.
- `org_organization.tenant_id` is nullable in the observed database catalog,
  while the active domain contract treats tenant ownership as required. The
  hardening SQL is historical evidence of the intended invariant; reconcile
  the live schema separately before changing either contract.
- Date-only columns use explicit `date` mappings in the active domain model.
- `sys_audit_log.old_data` and `sys_audit_log.new_data` use explicit `jsonb`
  mappings and must not be changed to text merely to match the scaffold.
