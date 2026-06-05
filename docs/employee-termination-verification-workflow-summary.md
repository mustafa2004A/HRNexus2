# Employee Termination Verification Workflow Summary

## Purpose

Employee termination is now modeled as a two-step sensitive action. An authorized HR/Admin user first requests a one-time verification code, then confirms termination by submitting that code with the termination details.

## Endpoints Added

- `POST /api/employees/{employeeId:int}/termination/verification-code`
- `POST /api/employees/{employeeId:int}/termination/confirm`

Both endpoints require `CanTerminateEmployee`, which is backed by the `Employees.Terminate` permission claim.

## Database Changes

Added `sec.ActionVerificationCode` for one-time action verification records.

Important columns:

- `ActionVerificationCodeID`
- `ActionType`
- `TargetEntityType`
- `TargetEntityID`
- `RequestedByUserID`
- `DeliveryMethod`
- `DestinationMasked`
- `CodeHash`
- `ExpiresAt`
- `UsedAt`
- `AttemptCount`
- `MaxAttempts`
- `CreatedAt`
- `CreatedByIp`
- `IsRevoked`
- `RevokedAt`

The table stores only a salted PBKDF2-SHA256 hash of the verification code. The plain code is not stored in the database and is not returned by the API.

## Services Added

- `IVerificationCodeService`
- `VerificationCodeService`
- `INotificationSender`
- `LoggingNotificationSender`
- `IActionVerificationCodeRepository`
- `ActionVerificationCodeRepository`

`LoggingNotificationSender` is a development/mock sender. It logs delivery metadata only and does not write the plain verification code to activity logs.

## Configuration

Added `TerminationVerification`:

- `Enabled`
- `CodeLength`
- `ExpiryMinutes`
- `MaxAttempts`
- `PreferredDeliveryMethod`
- `FallbackEmail`
- `FallbackPhoneNumber`

Defaults are 6 digits, 5 minutes, and 5 attempts.

## Activity Logging

Added activity code constants and seed rows for:

- `TerminationVerificationCodeRequested`
- `TerminationVerificationCodeSendFailed`
- `TerminationVerificationFailed`
- `TerminationVerificationSucceeded`
- `EmployeeTerminated`

Activity logs do not include the plain verification code.

## Termination Rules

After verification succeeds, the existing termination workflow still validates:

- employee exists
- employee is not soft-deleted
- employee is not already terminated
- termination reason exists
- terminated employment status exists
- termination date is not before hire date
- employee remains not deleted
- current employee/job lifecycle state moves to Terminated

## Swagger Notes

Swagger should show the new verification-code and confirm endpoints. The legacy direct endpoint remains in code for compatibility, but is marked obsolete and hidden from API explorer.

## Future Work

- Fully retire the legacy direct termination route once clients have migrated.
- Replace `LoggingNotificationSender` with SMTP, Twilio, Azure Communication Services, or another provider.
- Add a persistent HR remarks/audit-note store if termination remarks need long-term retention beyond activity metadata.
