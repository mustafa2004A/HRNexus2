# IP Address and Validation Refactor Summary

## 1. IP Provider Added

Added `IClientIpAddressProvider` and `ClientIpAddressProvider` in the API security layer.

The provider uses `IHttpContextAccessor` and centralizes client IP resolution for request-time security flows.

## 2. Places Updated to Use the Provider

Updated:

- `AuthController`
- `DevelopmentAuthController`
- JWT authentication challenge / forbidden logging in `Program.cs`

The old local `AuthController.GetIpAddress()` implementation was removed.

## 3. IPv4 Normalization Behavior

The provider normalizes IP addresses as follows:

- returns `null` when no HTTP context or remote IP exists.
- maps IPv4-mapped IPv6 addresses to IPv4.
- normalizes all loopback requests to `127.0.0.1`.
- otherwise returns `IPAddress.ToString()`.

This keeps local auth, refresh-token, logout, and access-denied logging consistent.

## 4. LeaveValidation Refactor Decision

`LeaveValidation` contained generic text helpers that were used by auth, security admin, operational CRUD, user activity logging, and leave services.

Those generic helpers were moved out of `LeaveValidation`.

Kept in `LeaveValidation`:

- leave request date validation.
- leave balance days validation.
- leave attachment metadata validation.

## 5. New Validation Helper

Added `BusinessValidation` in `HRNexus.Business.Validation`.

Moved generic methods:

- `NormalizeOptionalText`
- `NormalizeRequiredText`

Updated non-leave and generic call sites to use `BusinessValidation`.

## 6. Files Changed

- `HRNexus.API/Security/IClientIpAddressProvider.cs`
- `HRNexus.API/Security/ClientIpAddressProvider.cs`
- `HRNexus.API/Program.cs`
- `HRNexus.API/Controllers/AuthController.cs`
- `HRNexus.API/Controllers/DevelopmentAuthController.cs`
- `HRNexus.Business/Validation/BusinessValidation.cs`
- `HRNexus.Business/Validation/LeaveValidation.cs`
- `HRNexus.Business/Services/AuthService.cs`
- `HRNexus.Business/Services/DevelopmentPasswordBootstrapService.cs`
- `HRNexus.Business/Services/LeaveAttachmentService.cs`
- `HRNexus.Business/Services/LeaveRequestService.cs`
- `HRNexus.Business/Services/OperationalServiceHelpers.cs`
- `HRNexus.Business/Services/RefreshTokenService.cs`
- `HRNexus.Business/Services/SecurityAdminService.cs`
- `HRNexus.Business/Services/UserActivityLogService.cs`

## 7. Build Result

`dotnet build .\HRNexus.sln` passed with:

- 0 errors
- 0 warnings

## 8. Smoke-Test Result

Verified locally with the API running over HTTPS:

- login returned tokens.
- refresh returned a rotated token.
- logout succeeded.
- `/api/auth/me` succeeded.
- anonymous dashboard request returned `401`.
- dashboard returned `200` with admin token.
- pending leave request endpoint returned `200`.
- leave attachment upload still worked.
- person photo upload still worked.
- employee document upload still worked.
- latest refresh-token `CreatedByIp` / `RevokedByIp` values were `127.0.0.1`.
- latest login / token refresh / logout / access denied activity log rows used `127.0.0.1`.

Older rows created before this refactor can still contain previous values such as `::1`.
