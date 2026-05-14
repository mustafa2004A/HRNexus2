# Leave Service Models

These types are currently **business service contracts shared with the API layer** for the MVP stage.

That means:

- request models in this folder are used as service inputs
- `Dto` records in this folder are used as service outputs
- the API currently reuses them directly instead of maintaining a second DTO layer

This is intentional for the current 3-project MVP structure. It keeps the solution simple while the backend surface is still small.

## ID Conventions

- `EmployeeId` means `emp.Employee.EmployeeID`
- `LeaveRequestId` means `[leave].LeaveRequest.LeaveRequestID`
- `LeaveTypeId` means `[leave].LeaveType.LeaveTypeID`
- `RequestStatusId` means `[leave].RequestStatus.RequestStatusID`
- `ReviewedByUserId` and `UploadedByUserId` mean `sec.[User].UserID`

## Current User Context

The service layer now supports a lightweight current-user abstraction for Stage 1 / Stage 2 preparation.

- callers may still pass reviewer/uploader IDs explicitly
- when appropriate, services can also fall back to the temporary current-user context
- the temporary API-side implementation currently reads `X-User-Id` and `X-Username` headers

When auth is introduced later, this shared service-model approach can be revisited without forcing a larger architecture rewrite now.
