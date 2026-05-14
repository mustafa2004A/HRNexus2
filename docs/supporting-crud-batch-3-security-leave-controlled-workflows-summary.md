# Supporting CRUD Batch 3 - Security Assignment Workflows + Controlled Leave Admin CRUD

## 1. Tables / Workflows Reviewed

- Security assignment workflows: `sec.User`, `sec.UserRole`, `sec.RolePermissions`, `sec.UserPermissions`.
- Security read-only/admin views: `sec.UserActivityLog`, `sec.RefreshToken`, `sec.PermissionAudit`.
- Controlled leave workflows: `leave.LeaveBalance`, `leave.LeaveAttachment`, existing `leave.LeaveRequest` review/create/read flow.

## 2. Security Workflows Implemented

- Admin user management:
  - list users
  - get user by id
  - create user linked to an existing employee
  - update username, account status, and active flag
  - reset password through the existing Argon2 hashing service
  - lock and unlock users through account status transitions
- User role assignment:
  - list roles for a user
  - assign active roles only
  - remove role assignments
  - protect against removing the final active Admin user
- Role permission masks:
  - list role permission masks
  - set or update module permission masks
  - remove masks for non-built-in roles
  - write permission audit rows for role permission changes
- User permission overrides:
  - list per-user module overrides
  - set or update permission masks where `-1` means full access and `0` means no permissions
  - remove per-user overrides

## 3. Leave Workflows Implemented

- `LeaveBalance` was expanded with:
  - admin list endpoint with optional filters
  - admin get-by-id endpoint
  - existing employee self/HR balance read endpoint preserved
  - existing HR/Admin upsert endpoint preserved
- `LeaveAttachment` was expanded with:
  - get attachment metadata by id
  - soft deactivate attachment metadata with `IsActive = false`
  - existing create/list-by-leave-request endpoints preserved
- Existing `LeaveRequest` workflow was reviewed and left as controlled workflow logic:
  - employees/HR can create according to ownership rules
  - employee request list uses `SelfOrHr`
  - pending list and review/status updates use `CanReviewLeave`
  - no raw LeaveRequest CRUD was added

## 4. Read-only Admin Views Implemented

- `GET /api/security/activity-logs`
- `GET /api/security/activity-logs/{activityLogId}`
- `GET /api/security/users/{userId}/refresh-tokens`
- `POST /api/security/refresh-tokens/{refreshTokenId}/revoke`
- `GET /api/security/permission-audits`
- `GET /api/security/permission-audits/{auditId}`

Refresh token responses expose metadata only and intentionally do not expose token hashes.

## 5. Tables Intentionally System-managed

- `sec.UserActivityLog` remains read-only through public API.
- `sec.RefreshToken` remains system-managed by auth/refresh flow; admin API only exposes metadata and controlled revoke.
- `sec.PermissionAudit` remains append/read-only; no public update/delete endpoints were added.
- `leave.LeaveRequest` remains workflow-driven; no unsafe raw CRUD was added.

## 6. Controllers Added / Changed

- Added:
  - `HRNexus.API/Controllers/SecurityUsersController.cs`
  - `HRNexus.API/Controllers/SecurityRolePermissionsController.cs`
  - `HRNexus.API/Controllers/SecurityActivityLogsController.cs`
  - `HRNexus.API/Controllers/SecurityRefreshTokensController.cs`
  - `HRNexus.API/Controllers/SecurityPermissionAuditsController.cs`
- Changed:
  - `HRNexus.API/Controllers/LeaveBalancesController.cs`
  - `HRNexus.API/Controllers/LeaveAttachmentsController.cs`

## 7. Services Added / Changed

- Added/used:
  - `ISecurityAdminService`
  - `SecurityAdminService`
- Changed:
  - `ILeaveBalanceService`
  - `LeaveBalanceService`
  - `ILeaveAttachmentService`
  - `LeaveAttachmentService`
  - business DI registration for `ISecurityAdminService`

## 8. Repositories Added / Changed

- Added/used:
  - `ISecurityAdminRepository`
  - `SecurityAdminRepository`
  - security admin query result models
- Changed:
  - `ILeaveBalanceRepository`
  - `LeaveBalanceRepository`
  - `ILeaveAttachmentRepository`
  - `LeaveAttachmentRepository`
  - data access DI registration for `ISecurityAdminRepository`
- Added EF representation for:
  - `PermissionAudit`
  - `DbSet<PermissionAudit>`
  - `PermissionAuditConfiguration`

## 9. DTOs / Request Models Added

- Security admin DTOs and request/filter models:
  - `SecurityUserDto`
  - `CreateSecurityUserRequest`
  - `UpdateSecurityUserRequest`
  - `ResetSecurityUserPasswordRequest`
  - `SecurityUserRoleDto`
  - `AssignUserRoleRequest`
  - `RolePermissionMaskDto`
  - `UserPermissionOverrideDto`
  - `SetPermissionMaskRequest`
  - `SecurityActivityLogFilter`
  - `UserActivityLogDto`
  - `RefreshTokenMetadataDto`
  - `PermissionAuditDto`
  - `PermissionAuditFilter`

Existing leave DTOs were reused for leave balance and leave attachment responses.

## 10. Authorization Applied

- Security assignment and admin security view endpoints use `SecurityAdmin`.
- Leave balance list/get/upsert use `HrOrAdmin` where admin-level.
- Employee leave balance reads remain protected by `SelfOrHr`.
- Leave attachment create/list/get require `AuthenticatedUser` plus service-level ownership/reviewer checks.
- Leave attachment deactivate uses `CanReviewLeave`.
- Existing leave request pending/review flow remains protected by `CanReviewLeave`.

## 11. Validation Applied

- Route IDs use positive integer validation.
- Security user requests validate username length and password length.
- Security service validates password strength before hashing.
- Account status, employee, role, and module references are validated.
- Permission masks must be `-1` for full access or non-negative. `0` means no permissions, positive masks represent partial bit combinations, and `-1` applies to any role/module or user/module assignment that intentionally grants full access.
- `sec.Permission.BitValue` remains positive atomic bit values only and must never use `-1`.
- Activity/audit list filters validate pagination and date ranges.
- Leave balance filters validate positive IDs and reasonable years.

## 12. Sensitive Data Protection Decisions

- `PasswordHash` is never returned by admin user APIs.
- Security user create/reset accepts plaintext password only in request bodies and stores only Argon2 hashes through the existing hashing service.
- `TokenHash` is never returned by refresh token metadata APIs.
- User activity log APIs expose operational audit details only, not secrets.
- Leave attachment responses preserve the existing `FilePath` response contract for compatibility. This remains metadata-only and should be replaced by safer storage references or signed download URLs in a later hardening pass.

## 13. Audit / Logging Decisions

- Existing auth/security activity logging remains intact.
- Role permission mask changes write `PermissionAudit` rows with old and new mask values.
- User role and user permission changes are controlled through Admin-only APIs. Additional activity logging for every assignment change can be added later if needed.
- Runtime effective permission summaries ignore inactive modules, ignore inactive roles, and return `0` masks for disabled or locked users.

## 14. Smoke Tests Performed

- `dotnet build .\HRNexus.sln` succeeded with 0 warnings and 0 errors.
- API started locally in Development and Swagger returned HTTP 200.
- Auth sanity:
  - development demo password reseed returned 200
  - admin login returned 200
  - refresh token returned 200
  - employee login returned 200
  - `/api/auth/me` returned 200 with admin token
- Authorization sanity:
  - anonymous security user create returned 401
  - Employee role security user list returned 403
  - employee dashboard access returned 403
- Security workflows:
  - list users returned 200
  - create/update/reset/lock/unlock security user returned expected 201/200 responses
  - create smoke role returned 201
  - assign/list/remove user role returned 200
  - set/list/remove role permission mask returned 200
  - set/list/remove user permission override returned 200
  - list activity logs returned 200
  - list permission audits returned 200
  - list refresh token metadata returned 200 and did not expose `tokenHash`
  - admin refresh token revoke returned 200
- Leave workflows:
  - list leave balances returned 200
  - HR/Admin upsert of an existing leave balance returned 200
  - get leave balance by id returned 200
  - employee own leave balance read returned 200
  - employee reading another employee's balance returned 403
  - pending leave workflow returned 200
  - create/get/deactivate leave attachment metadata returned 200
- Existing system checks:
  - dashboard returned 200 with admin token
  - Batch 1 countries lookup returned 200
  - Batch 2 people and employees list returned 200
  - public leave reference types returned 200

Smoke-only security user, role, attachment, and employee records were deactivated/soft-deleted through API cleanup where supported.

## 15. Limitations and Future Work

- `UserActivityLog` remains read-only; richer audit filters and exports can be added later.
- Security assignment changes are protected by `SecurityAdmin`, currently Admin-only.
- Refresh token admin revoke marks tokens revoked but does not expose raw token values.
- `PermissionAudit` currently tracks role permission mask changes. User permission override auditing can be added later if required.
- Leave attachment metadata still includes the existing file path field. A future file storage abstraction should avoid exposing raw paths.
- Leave balance has no deactivate/delete endpoint because the current table design is operational balance state, not a soft-deletable record.
- Batch 3 did not implement raw CRUD for `LeaveRequest`, `RefreshToken`, `UserActivityLog`, or `PermissionAudit` by design.
