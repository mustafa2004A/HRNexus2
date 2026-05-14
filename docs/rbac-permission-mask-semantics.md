# RBAC Permission Mask Semantics

## Permission Mask Values

HRNexus uses integer permission masks for role and user module assignments.

- `PermissionMask = -1` means Full Access / All Permissions for that role or user override on that module.
- `PermissionMask = 0` means No Permissions.
- positive values are bitmask combinations of atomic permission values.

The `-1` sentinel is allowed only in:

- `sec.RolePermissions.PermissionMask`
- `sec.UserPermissions.PermissionMask`

It must never be stored in `sec.Permission.BitValue`.

## Atomic Permission Values

`sec.Permission.BitValue` stores atomic positive power-of-two values only, such as:

- `1`
- `2`
- `4`
- `8`

These values can be OR-combined into positive permission masks. `Permission.BitValue` remains the catalog of atomic permission bits, not assignment masks.

## Effective Permission Calculation

Runtime permission summaries use these rules:

- inactive, locked, disabled, suspended, or otherwise non-sign-in-eligible users receive `0` effective permissions for all active modules.
- inactive roles do not contribute permissions.
- inactive modules are not included in effective permission summaries.
- if any active role assignment has `PermissionMask = -1` for a module, the effective mask for that module is `-1`.
- if any user permission override has `PermissionMask = -1` for a module, the effective mask for that module is `-1`.
- otherwise, positive role and user masks are combined with bitwise OR.
- if a user has no active role permissions and no user override for a module, the effective mask is `0`.

The `/api/auth/me` permission summary returns one effective row per active module.

## Database Enforcement

The database update script `database/2026-05-02_rbac_permission_mask_full_access.sql`:

- enforces `PermissionMask = -1 OR PermissionMask >= 0` for role and user permission assignment tables.
- enforces positive power-of-two `Permission.BitValue` values.
- calculates the full active permission mask from active positive `Permission.BitValue` rows and normalizes matching role and user assignment masks to `-1`.
- sets the built-in `Admin` role to `PermissionMask = -1` for every module.
- inserts explicit `0` masks for missing built-in non-admin role/module rows.

## Full Access and Disabled Account Behavior

The built-in `Admin` role should have full access through `PermissionMask = -1`, but the full-access sentinel is not Admin-only. Any active role/module permission row or user/module override with `PermissionMask = -1` grants full effective access for that module.

Disabled or locked users must not inherit effective permissions from roles or overrides, even if they still have active role assignment rows in the database. Their effective module masks are `0`.
