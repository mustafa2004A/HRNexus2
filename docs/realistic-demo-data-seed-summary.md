# Realistic Demo Data Seed Summary

## Purpose

The realistic demo seed prepares HRNexus for frontend integration and final demos with consistent people, employees, users, roles, permissions, documents, leave data, file metadata, and termination workflow test data.

Termination-related employee state is seeded only for dedicated demo employees. Normal employee creation remains separate from termination workflow behavior.

## Scripts Changed

- `database/05_seed/06_demo_operational_seed.sql`
  - Replaced demo photo URLs and file paths with safe relative paths.
- `database/05_seed/07_realistic_demo_seed.sql`
  - Adds the realistic, idempotent demo dataset.
- `database/run-all.sql`
  - Runs `07_realistic_demo_seed.sql` after the existing operational demo seed.

## Demo Personas

- `Admin`: stored full-access demo admin account. On the local case-insensitive database, this normalizes the previous lowercase admin row to the exact stored username `Admin`; lowercase login remains accepted by collation.
- `sarah.haddad`: HR Manager with broad HR, leave, documents, dashboard, and termination access.
- `omar.khalil`: Department Manager with limited employee visibility and leave review access.
- `lina.nasser`: HR Clerk with operational employee/document/leave access and no security-admin access.
- `nadia.saleh`: Employee self-service persona without termination/security access.
- Additional demo users: `yousef.mansour`, `reem.abbas`, `tariq.mahdi`, `huda.farouq`, `kareem.daher`, and inactive terminated user `salma.mahfouz`.

## Admin Account

The seed creates or updates a linked Admin employee/person (`HRN-ADMIN`) and ensures the `Admin` user is active, linked to that employee, assigned the built-in `Admin` role, and granted `PermissionMask = -1` user overrides on all active modules.

The development password bootstrap flow supports per-user local demo password pairs, so passwords can be applied by the Argon2 hashing service instead of SQL. Local demo passwords are not written into SQL or documentation.

Current caution: the bootstrap request model/service still enforce development password strength validation. The requested short Admin-only local password remains blocked unless a future explicitly approved Development-only exception is added.

## Roles And Permissions

- `Admin` role receives `PermissionMask = -1` for every active module.
- `HRManager` receives full access on HR-facing modules including `Employee`, `Leave`, `Documents`, `LeaveRequests`, and `LeaveBalances`.
- `HRClerk` receives partial create/update/read access for operational HR areas and no security-admin access.
- `DepartmentManager` receives limited employee and leave review access.
- `Employee` receives own-scope employee, leave, and document access only.

`sec.Permission.BitValue` remains positive-only. Full access uses `-1` only in `sec.RolePermissions.PermissionMask` and `sec.UserPermissions.PermissionMask`.

## Lookup Data

The seed expands core, organization, employee, document, leave, holiday, security module, permission, and activity type lookups. Existing conventions are preserved, including the backend `Employee` module that emits `Employees.*` permission claims.

## Employee Scenarios

Seeded scenarios include active employees, no-photo employees, photo metadata, multiple contacts, multiple addresses, multiple identifiers, family members, a safe-to-terminate active employee (`HRN-DEMO-0105`), and an already terminated employee (`HRN-DEMO-0106`).

## Leave And Documents

Leave data includes balances for current/next demo years, pending, approved, rejected, and cancelled requests, plus safe relative attachment metadata.

Document data includes verified, unverified, active, near-expiry, and expired documents with safe relative paths.

Where `core.FileStorageItem` exists, the seed adds safe relative file metadata and links it to person photos, employee documents, and leave attachments.

## Termination Readiness

Termination reasons and the `Terminated` employment status are present. `Admin` and `HRManager` have employee termination permission through full `Employee` module access; the `Employee` role does not include the `Terminate` bit.

## Idempotency

The script uses stable natural keys such as username, employee code, module name, permission name, department/position names, document names, and leave dates. It can be rerun without duplicate-key failures.

## Apply

From the `database` folder:

```bash
sqlcmd -S localhost -d HRNexus_CleanTest -E -b -i .\05_seed\07_realistic_demo_seed.sql
```

For a clean database, `run-all.sql` now includes the realistic demo seed automatically.
