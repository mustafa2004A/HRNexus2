# HRNexus API Route Inventory

This inventory reflects the final backend demo-readiness pass. Authorization names match the ASP.NET Core policies configured in `Program.cs`.

## Auth

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `POST /api/auth/login` | Login and issue JWT + refresh token | Public, rate-limited | Public | Verified |
| `POST /api/auth/refresh` | Rotate refresh token and issue new access token | Public, rate-limited | Public | Verified |
| `POST /api/auth/logout` | Revoke supplied refresh token | Public, rate-limited | Public | Verified |
| `GET /api/auth/me` | Current authenticated user and permissions | `AuthenticatedUser` | Protected | Verified |
| `POST /api/dev/auth/reseed-demo-passwords` | Development demo password reseed | Development-only, localhost-only, config-enabled, rate-limited | Dev-only | Verified |

## Dashboard

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `GET /api/dashboard/summary` | HR dashboard KPIs and widgets | `HrOrAdmin` | Protected | Verified |

## Employee Context

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `GET /api/employees/{employeeId}` | Employee basic details | `SelfOrHr` | Protected | Verified |
| `GET /api/employees/{employeeId}/current-context` | Current assignment/context | `SelfOrHr` | Protected | Verified |
| `GET /api/employees/{employeeId}/job-history` | Employee job history summary | `SelfOrHr` | Protected | Verified |
| `GET /api/employees/{employeeId}/documents` | Employee document summary | `SelfOrHr` | Protected | Verified |

## Core / People Operational CRUD

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `GET /api/people` | List/search people | `HrOrAdmin` | Protected | Verified |
| `GET /api/people/{personId}` | Get person | `HrOrAdmin` | Protected | Ready |
| `POST /api/people` | Create person | `HrOrAdmin` | Protected | Verified |
| `PUT /api/people/{personId}` | Update person | `HrOrAdmin` | Protected | Verified |
| `DELETE /api/people/{personId}` | Soft-delete person | `HrOrAdmin` | Protected | Verified |
| `GET /api/people/{personId}/contacts` | List person contacts | `HrOrAdmin` | Protected | Ready |
| `GET /api/people/{personId}/contacts/{contactId}` | Get person contact | `HrOrAdmin` | Protected | Ready |
| `POST /api/people/{personId}/contacts` | Create person contact | `HrOrAdmin` | Protected | Ready |
| `PUT /api/people/{personId}/contacts/{contactId}` | Update person contact | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/people/{personId}/contacts/{contactId}` | Delete person contact | `HrOrAdmin` | Protected | Ready |
| `GET /api/people/{personId}/addresses` | List person addresses | `HrOrAdmin` | Protected | Ready |
| `GET /api/people/{personId}/addresses/{addressId}` | Get address | `HrOrAdmin` | Protected | Ready |
| `POST /api/people/{personId}/addresses` | Create address | `HrOrAdmin` | Protected | Ready |
| `PUT /api/people/{personId}/addresses/{addressId}` | Update address | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/people/{personId}/addresses/{addressId}` | Delete address | `HrOrAdmin` | Protected | Ready |
| `GET /api/people/{personId}/identifiers` | List masked identifiers | `HrOrAdmin` | Protected | Ready |
| `GET /api/people/{personId}/identifiers/{identifierId}` | Get masked identifier | `HrOrAdmin` | Protected | Ready |
| `POST /api/people/{personId}/identifiers` | Create identifier | `HrOrAdmin` | Protected | Ready |
| `PUT /api/people/{personId}/identifiers/{identifierId}` | Update identifier | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/people/{personId}/identifiers/{identifierId}` | Delete identifier | `HrOrAdmin` | Protected | Ready |

## Employee Operational CRUD

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `GET /api/employees` | List/search employees | `HrOrAdmin` | Protected | Verified |
| `POST /api/employees` | Create employee with person and optional initial job | `HrOrAdmin` | Protected | Verified |
| `PUT /api/employees/{employeeId}` | Update employee/person core fields | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/employees/{employeeId}` | Soft-delete employee | `HrOrAdmin` | Protected | Verified |
| `GET /api/employees/{employeeId}/job-history/{jobHistoryId}` | Get job history row | `SelfOrHr` | Protected | Ready |
| `POST /api/employees/{employeeId}/job-history` | Create job history row | `HrOrAdmin` | Protected | Ready |
| `PUT /api/employees/{employeeId}/job-history/{jobHistoryId}` | Update job history row | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/employees/{employeeId}/job-history/{jobHistoryId}` | Delete job history row | `HrOrAdmin` | Protected | Ready |
| `GET /api/employees/{employeeId}/documents/{documentId}` | Get employee document metadata | `SelfOrHr` | Protected | Ready |
| `POST /api/employees/{employeeId}/documents` | Create document metadata | `HrOrAdmin` | Protected | Ready |
| `PUT /api/employees/{employeeId}/documents/{documentId}` | Update document metadata | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/employees/{employeeId}/documents/{documentId}` | Deactivate document metadata | `HrOrAdmin` | Protected | Ready |
| `GET /api/employees/{employeeId}/family-members` | List family members | `SelfOrHr` | Protected | Ready |
| `GET /api/employees/{employeeId}/family-members/{familyMemberId}` | Get family member | `SelfOrHr` | Protected | Ready |
| `POST /api/employees/{employeeId}/family-members` | Create family member relationship and person | `HrOrAdmin` | Protected | Ready |
| `PUT /api/employees/{employeeId}/family-members/{familyMemberId}` | Update family member | `HrOrAdmin` | Protected | Ready |
| `DELETE /api/employees/{employeeId}/family-members/{familyMemberId}` | Delete family member relationship | `HrOrAdmin` | Protected | Ready |

## Leave

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `POST /api/leave-requests` | Create leave request with ownership checks | `AuthenticatedUser` | Protected | Ready |
| `GET /api/leave-requests/employees/{employeeId}` | Employee leave requests | `SelfOrHr` | Protected | Verified |
| `GET /api/leave-requests/pending` | Pending leave requests for reviewers | `CanReviewLeave` | Protected | Verified |
| `PATCH /api/leave-requests/{leaveRequestId}/status` | Review/update leave request status | `CanReviewLeave` | Protected | Protected; ready |
| `GET /api/leave-balances` | Admin list balances with filters | `HrOrAdmin` | Protected | Verified |
| `GET /api/leave-balances/{leaveBalanceId}` | Admin get leave balance | `HrOrAdmin` | Protected | Ready |
| `GET /api/leave-balances/employees/{employeeId}` | Employee balances | `SelfOrHr` | Protected | Verified |
| `PUT /api/leave-balances` | HR/Admin upsert balance | `HrOrAdmin` | Protected | Ready |
| `POST /api/leave-attachments` | Create attachment metadata | `AuthenticatedUser` plus service ownership/reviewer checks | Protected | Ready |
| `GET /api/leave-attachments/leave-requests/{leaveRequestId}` | List attachment metadata for request | `AuthenticatedUser` plus service ownership/reviewer checks | Protected | Verified |
| `GET /api/leave-attachments/{leaveAttachmentId}` | Get attachment metadata | `AuthenticatedUser` plus service ownership/reviewer checks | Protected | Ready |
| `DELETE /api/leave-attachments/{leaveAttachmentId}` | Deactivate attachment metadata | `CanReviewLeave` plus service checks | Protected | Ready |
| `GET /api/leave-reference/leave-types` | Public active leave types | Public | Public | Verified |
| `GET /api/leave-reference/request-statuses` | Public active request statuses | Public | Public | Verified |
| `GET /api/holidays/GetHolidays` | Public legacy holiday list | Public | Public | Verified |

## Lookup CRUD

Read endpoints use `AuthenticatedUser`; create/update/delete use `HrOrAdmin`, except security lookup/admin references use `SecurityAdmin`.

| Route group | Purpose | Authorization | Demo readiness |
| --- | --- | --- | --- |
| `/api/countries` | Country CRUD | Authenticated read, HR/Admin write | Verified |
| `/api/cities` | City CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/genders` | Gender CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/marital-statuses` | Marital status CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/contact-types` | Contact type CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/address-types` | Address type CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/identifier-types` | Identifier type CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/employment-types` | Employment type CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/employment-statuses` | Employment status CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/departments` | Department CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/positions` | Position CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/job-grades` | Job grade CRUD | Authenticated read, HR/Admin write | Verified read |
| `/api/termination-reasons` | Termination reason CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/relationship-types` | Relationship type CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/document-types` | Document type CRUD | Authenticated read, HR/Admin write | Ready |
| `/api/leave-types` | Leave type CRUD | Authenticated read, HR/Admin write | Public reference route also exists |
| `/api/holidays` | Holiday CRUD | Authenticated read, HR/Admin write | Public legacy route also exists |
| `/api/request-statuses` | Request status CRUD | Authenticated read, HR/Admin write | Public reference route also exists |

## Security Admin

| Route | Purpose | Authorization | Status | Demo readiness |
| --- | --- | --- | --- | --- |
| `GET /api/security/users` | List users without password hashes | `SecurityAdmin` | Protected | Verified |
| `GET /api/security/users/{userId}` | Get user | `SecurityAdmin` | Protected | Ready |
| `POST /api/security/users` | Create user linked to employee | `SecurityAdmin` | Protected | Ready |
| `PUT /api/security/users/{userId}` | Update username/status/active flag | `SecurityAdmin` | Protected | Ready |
| `POST /api/security/users/{userId}/reset-password` | Reset password through Argon2 hashing | `SecurityAdmin` | Protected | Ready |
| `POST /api/security/users/{userId}/lock` | Lock user | `SecurityAdmin` | Protected | Ready |
| `POST /api/security/users/{userId}/unlock` | Unlock user | `SecurityAdmin` | Protected | Ready |
| `GET /api/security/users/{userId}/roles` | List user roles | `SecurityAdmin` | Protected | Ready |
| `POST /api/security/users/{userId}/roles` | Assign role | `SecurityAdmin` | Protected | Ready |
| `DELETE /api/security/users/{userId}/roles/{roleId}` | Remove role with last-admin guard | `SecurityAdmin` | Protected | Ready |
| `GET /api/security/users/{userId}/permissions` | List user permission overrides | `SecurityAdmin` | Protected | Ready |
| `PUT /api/security/users/{userId}/permissions/{moduleId}` | Set user permission override | `SecurityAdmin` | Protected | Ready |
| `DELETE /api/security/users/{userId}/permissions/{moduleId}` | Remove user permission override | `SecurityAdmin` | Protected | Ready |
| `GET /api/security/users/{userId}/refresh-tokens` | Refresh token metadata only | `SecurityAdmin` | Protected | Verified |
| `POST /api/security/refresh-tokens/{refreshTokenId}/revoke` | Admin revoke refresh token | `SecurityAdmin` | Protected | Ready |
| `GET /api/security/roles/{roleId}/permissions` | List role permission masks | `SecurityAdmin` | Protected | Ready |
| `PUT /api/security/roles/{roleId}/permissions/{moduleId}` | Set role permission mask and audit | `SecurityAdmin` | Protected | Verified |
| `DELETE /api/security/roles/{roleId}/permissions/{moduleId}` | Remove non-built-in role mask and audit | `SecurityAdmin` | Protected | Verified |
| `GET /api/security/activity-logs` | Read activity logs | `SecurityAdmin` | Protected | Verified |
| `GET /api/security/activity-logs/{activityLogId}` | Read activity log by id | `SecurityAdmin` | Protected | Ready |
| `GET /api/security/permission-audits` | Read permission audit logs | `SecurityAdmin` | Protected | Verified |
| `GET /api/security/permission-audits/{auditId}` | Read permission audit by id | `SecurityAdmin` | Protected | Ready |
| `/api/security/roles` | Role lookup/admin CRUD with built-in role guards | `SecurityAdmin` | Protected | Verified representative workflow |
| `/api/security/modules` | Module lookup/admin CRUD | `SecurityAdmin` | Protected | Verified read |
| `/api/security/permissions` | Permission lookup/admin CRUD | `SecurityAdmin` | Protected | Ready |
| `/api/security/account-statuses` | Account status lookup/admin CRUD | `SecurityAdmin` | Protected | Ready |
| `/api/security/activity-types` | Activity type lookup/admin CRUD | `SecurityAdmin` | Protected | Ready |
