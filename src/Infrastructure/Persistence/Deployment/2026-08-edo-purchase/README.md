# EDO → Purchase server deployment gate

This package deploys the historical EDOCS/DIDOX Draft Purchase import without running provider acceptance tests.

Read `AUDIT-2026-08-14.md` before deployment. The EDO-scoped release gate and the repository-wide 207-table parity gate are intentionally documented separately.

1. Take and verify a recoverable database backup.
2. Run `preflight.sql` against the exact target database.
3. If all EDO objects already exist, do not rerun their non-idempotent create scripts. Apply `0148_unify_document_number_generation.sql` when the centralized sequence is missing or the legacy Purchase generator still exists.
4. If the target is missing the whole feature, execute `migration-order.txt` from top to bottom. Stop on the first SQL error.
5. Run `postflight.sql`. Do not deploy binaries unless it completes without an exception.
6. Deploy the Release build and restart exactly one WebApi instance first.
7. Verify health, authentication and these read-only endpoints before scaling out:
   - `GET /api/purchase-docs/edo-imports/{jobId}`
   - `GET /api/purchase-docs/edo-imports/{jobId}/mapping-summary`
   - `GET /api/purchase-docs/edo-imports/{jobId}/import-plan`
8. Do not start a new bulk import during deployment. Existing durable `QUEUED`/`RUNNING` work resumes after restart.
9. Continue DIDOX and next-organization acceptance tests only as a separate operator-approved activity.

Never commit or deploy local `appsettings.json`, token-cache files, database backups or request-body scratch files.
