# Employee Create Request Cleanup Summary

## Why termination fields were removed

`POST /api/employees` creates a new active employee. Termination data is not a creation-time concern, and allowing it in the create request made Swagger advertise an unsafe lifecycle shortcut.

Employee termination is now kept out of the employee create contract so new employee records cannot be created with termination state supplied by the client.

## Removed fields

The following fields were removed from the employee create core request model:

- `terminationReasonId`
- `terminationDate`
- `isEligibleForRehire`

## Internal defaults during employee creation

When `EmployeeService.CreateAsync` creates the `Employee` entity, termination state is set internally:

- `TerminationReasonID = null`
- `TerminationDate = null`
- `IsEligibleForRehire = false`

`CurrentEmploymentStatusID` remains controlled by the create request and represents the initial active/employed status chosen for the new employee.

## Why termination belongs to a separate workflow

Termination is a lifecycle event with its own permissions, validation, audit trail, status transition, current job closure behavior, and notification step. It must be handled through the dedicated employee termination endpoint instead of general create/update payloads.

Keeping termination separate prevents HR users or clients from bypassing workflow rules by creating an employee directly in a terminated state.

## Entity and database columns remain

The `Employee` entity still contains:

- `TerminationReasonId`
- `TerminationDate`
- `IsEligibleForRehire`

The database still contains the corresponding `emp.Employee` columns. The schema script keeps these columns and aligns the employee-level `IsEligibleForRehire` default to `0` for new databases.

## Swagger verification notes

Swagger for `POST /api/employees` should show `CreateEmployeeRequest.employee` with only:

- `hireDate`
- `currentEmploymentStatusId`

It should not show:

- `terminationReasonId`
- `terminationDate`
- `isEligibleForRehire`

The dedicated termination endpoint still uses `TerminateEmployeeRequest`.

Verified on 2026-06-05 by loading `/swagger/v1/swagger.json` from a temporary local API instance:

- `POST /api/employees` request body references `CreateEmployeeRequest`
- `CreateEmployeeCoreRequest` properties: `hireDate`, `currentEmploymentStatusId`
- forbidden create fields present: none

Runtime smoke verification created employee `HRN-EMP-0007` through `POST /api/employees` and confirmed:

- `TerminationReasonID = NULL`
- `TerminationDate = NULL`
- `IsEligibleForRehire = 0`
- `CurrentEmploymentStatusID = 1`

## Future work

Continue building out the Employee Termination Workflow endpoint and related UX around termination initiation, approval/notification needs, and post-termination audit/reporting behavior.
