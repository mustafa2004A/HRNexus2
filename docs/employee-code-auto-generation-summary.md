# Employee Code Auto Generation Summary

## Why The Backend Generates EmployeeCode

`EmployeeCode` is a system identifier and must not be typed into Swagger, API requests, or frontend forms. Backend generation avoids duplicate codes, keeps the format consistent, and prevents normal update flows from changing employee identity.

## Format

Employee codes are generated as:

```text
HRN-EMP-{number:D4}
```

Examples:

- `HRN-EMP-0001`
- `HRN-EMP-0002`
- `HRN-EMP-10000`

## SQL Sequence

The numeric value comes from:

```sql
emp.EmployeeCodeSequence
```

The application reads the next value with:

```sql
SELECT NEXT VALUE FOR emp.EmployeeCodeSequence;
```

The DataAccess repository returns that value to the Business layer, where the final code string is formatted.

## Existing Employee Codes

The script `database/2026-05-09_employee_code_sequence.sql` preserves existing `emp.Employee.EmployeeCode` values. On first creation of the sequence, it finds the highest existing numeric code that matches `HRN-EMP-%` and starts the sequence at the next number.

If existing employees are coded through `HRN-EMP-0004`, the sequence starts at `5`, and the next created employee receives `HRN-EMP-0005`.

## Why MAX + 1 Is Avoided At Runtime

The runtime creation flow does not use `MAX(EmployeeID) + 1` or `MAX(EmployeeCode) + 1`. Those patterns are race-prone under concurrent requests. SQL Server sequence generation is concurrency-safe and works with the existing unique constraint on `EmployeeCode`.

## Create And Update DTO Changes

- `CreateEmployeeCoreRequest` no longer contains `EmployeeCode`.
- `UpdateEmployeeCoreRequest` no longer contains `EmployeeCode`.
- `EmployeeDetailsDto`, `EmployeeSummaryDto`, dashboard DTOs, leave DTOs, and security user views still return employee code as read-only metadata.

Normal employee update requests cannot change the employee code. A future special administrative workflow would be required if employee-code correction is ever needed.

## Generation Flow

1. Validate person, employee, and optional initial-job request data.
2. Read the next value from `emp.EmployeeCodeSequence`.
3. Format the code as `HRN-EMP-{number:D4}`.
4. Create `core.Person`.
5. Create `emp.Employee` with the generated code.
6. Optionally create the initial `emp.EmployeeJobHistory`.
7. Save through the existing unit-of-work style.

## Verification Results

- `dotnet build .\HRNexus.sln` succeeded with `0` warnings and `0` errors.
- The SQL sequence script was applied to the development database and `emp.EmployeeCodeSequence` was verified.
- The raw sequence statement was verified against SQL Server.
- Source request models no longer expose `EmployeeCode` on create/update requests.
- The repository sequence query was corrected to avoid a trailing semicolon, because EF Core composes scalar `SqlQueryRaw<T>` SQL.
- A full API create/update smoke test was completed after the repository query fix.
- Runtime Swagger for `POST /api/employees` advertises JSON content types only: `application/json`, `text/json`, and `application/*+json`.
- Runtime Swagger schemas for `CreateEmployeeRequest`, `CreateEmployeeCoreRequest`, and `UpdateEmployeeCoreRequest` do not include `employeeCode`.
- The smoke test created employees without sending `employeeCode` and received `HRN-EMP-0005` then `HRN-EMP-0006`.
- A test update request that included `employeeCode = CLIENT-SHOULD-NOT-WIN` did not change the stored employee code; both API response and database remained `HRN-EMP-0005`.
- Smoke-test employee rows were soft-deleted through the API after verification.

Note: SQL Server sequences can have gaps when values are consumed by failed transactions or diagnostics. Gaps do not violate the employee-code contract; uniqueness and backend-only generation are the important guarantees.
