# Demo Login Accounts Local Only Summary

## Purpose

This local-development setup keeps the HRNexus demo login accounts consistent for frontend integration and final demo testing. The accounts cover Admin, HR Manager, Department Manager, HR Clerk, and Employee self-service permission levels.

## Demo Usernames And Roles

| Username | Expected Role | Employee Link |
| --- | --- | --- |
| `Admin` | `Admin` | `HRN-ADMIN` |
| `sarah.haddad` | `HRManager` | `HRN-EMP-0001` |
| `omar.khalil` | `DepartmentManager` | `HRN-EMP-0002` |
| `lina.nasser` | `HRClerk` | `HRN-EMP-0003` |
| `nadia.saleh` | `Employee` | `HRN-EMP-0004` |

## Expected Permissions

- `Admin` receives full access through `PermissionMask = -1` role-module rows for active modules.
- `HRManager` receives broad HR, employee, leave, document, dashboard, holiday, and file-storage access, including employee termination through full Employee module access.
- `DepartmentManager` receives limited read/review access for employee and leave workflows and no security-admin access.
- `HRClerk` receives operational HR create/update/read access and no security-admin access.
- `Employee` receives own-scope employee, leave, leave balance, leave request, and document access only.

`sec.Permission.BitValue` remains positive-only. Full access uses `-1` only in permission mask tables such as `sec.RolePermissions` or `sec.UserPermissions`.

## Password Application

Plaintext passwords are not stored in SQL seed scripts, appsettings, or committed documentation. Local passwords are applied through the Development Password Bootstrap endpoint, which uses the existing Argon2 hashing service.

Use the local demo passwords shared out-of-band in the request body for:

```http
POST /api/dev/auth/reseed-demo-passwords
```

The endpoint is available only in Development when `DevelopmentAuth:PasswordBootstrapEnabled` is enabled and the request comes from localhost.

## Per-User Reseed Shape

The development bootstrap flow supports per-user password pairs:

```json
{
  "credentials": [
    { "username": "Admin", "password": "<local-demo-password>" },
    { "username": "sarah.haddad", "password": "<local-demo-password>" },
    { "username": "omar.khalil", "password": "<local-demo-password>" },
    { "username": "lina.nasser", "password": "<local-demo-password>" },
    { "username": "nadia.saleh", "password": "<local-demo-password>" }
  ]
}
```

The older shared-password request remains supported for compliant development passwords:

```json
{
  "usernames": ["Admin", "sarah.haddad"],
  "password": "<shared-local-demo-password>"
}
```

## Idempotency Rules

- User lookup is case-insensitive.
- `Admin`, `admin`, and `ADMIN` are treated as the same demo identity.
- The official stored Admin username is `Admin`.
- Existing lowercase `admin` rows are normalized to `Admin` when safe.
- Existing demo users are updated instead of duplicated.
- Duplicate role assignments are not inserted.
- Accidental duplicate username rows are deactivated rather than deleted when encountered by the development bootstrap flow.

## Admin Normalization

There must be only one official Admin demo login: `Admin`.

If a lowercase `admin` row exists from an older seed run, the seed/bootstrap path normalizes it to `Admin` instead of creating a separate official demo account. On case-insensitive SQL Server collations, lowercase input may still match the `Admin` row at login time; that is collation behavior, not a separate supported account.

## Current Admin Password Policy Note

The bootstrap flow keeps the existing development password strength validation. The requested short Admin-only local password remains blocked by this safe implementation. Use a compliant local Admin demo password unless a future change is explicitly approved to add a narrower Development-only exception.

## Rerun Steps

1. Run the normal seed scripts if the database is missing demo people/employees.
2. Start the API in Development with a valid `HRNEXUS_JWT_SIGNING_KEY`.
3. POST the local credential pairs to `/api/dev/auth/reseed-demo-passwords`.
4. Verify `POST /api/auth/login` for each account.
5. Verify `GET /api/auth/me` returns the expected username, role, and permission masks.

## Safety Notes

- Do not commit plaintext local demo passwords.
- Do not store passwords in appsettings.
- Do not manually insert password hashes from SQL.
- Do not expose `PasswordHash` through API responses.
- Keep the bootstrap endpoint Development-only and localhost-only.
