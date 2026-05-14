# Soft Delete Trigger Safety Summary

## Why INSTEAD OF DELETE

`INSTEAD OF DELETE` triggers are used because they intercept a delete statement before SQL Server removes the row. This lets the database convert accidental hard deletes into soft-delete updates. `AFTER DELETE` triggers are not appropriate for this safety net because the target row has already been deleted by the time the trigger runs.

## Tables Covered

- `core.Person`
- `emp.Employee`

These tables have the true soft-delete columns:

- `IsDeleted`
- `DeletedBy`
- `DeletedDate`

They also have `ModifiedBy` and `ModifiedDate`, so the triggers update those audit columns at the same time.

## Tables Intentionally Not Covered

Lookup/admin/deactivation tables are intentionally not covered, including roles, departments, positions, leave types, holidays, request statuses, and file storage items. Those tables use `IsActive` deactivate behavior, not the `IsDeleted`/`DeletedBy`/`DeletedDate` soft-delete pattern.

## DeletedBy Resolution

The triggers resolve the current application user from SQL Server session context:

```sql
TRY_CAST(SESSION_CONTEXT(N'CurrentUserID') AS int)
```

If the session context value is unavailable, `DeletedBy` is set to `NULL`. The current application soft-delete services already set `DeletedBy` directly, so session context is mainly for direct SQL hard-delete safety. The application does not currently set `SESSION_CONTEXT(N'CurrentUserID')` globally.

## Multi-row Delete Handling

Both triggers join the target table to the trigger `deleted` pseudo-table, so a single multi-row `DELETE` statement soft-deletes every matching non-deleted row. Rows already marked `IsDeleted = 1` are ignored.

## Relationship With Application Soft Delete

Application-level soft delete remains the primary expected behavior. `PersonService.DeleteAsync` and `EmployeeService.DeleteAsync` update `IsDeleted`, `DeletedBy`, `DeletedDate`, `ModifiedBy`, and `ModifiedDate` directly. The triggers are a database-level safety net for accidental direct SQL or repository hard deletes.

## Verification Performed

- `dotnet build .\HRNexus.sln`
- Applied `database/2026-05-08_soft_delete_triggers.sql` to the local development database.
- Verified direct SQL delete on a test `core.Person` row preserved the row and set `IsDeleted = 1`, `DeletedDate`, `DeletedBy`, `ModifiedDate`, and `ModifiedBy`.
- Verified direct SQL delete on a test `emp.Employee` row preserved the row and set `IsDeleted = 1`, `DeletedDate`, `DeletedBy`, `ModifiedDate`, and `ModifiedBy`.
- Verified the existing application service code still performs soft delete through updates rather than relying on hard deletes.

## Cautions and Future Improvements

- Direct SQL deletes without `SESSION_CONTEXT(N'CurrentUserID')` will soft-delete the row but leave `DeletedBy` and `ModifiedBy` as `NULL`.
- A future hardening pass could add safe EF Core session-context support around write operations if the project needs database-generated audit user IDs for direct trigger paths.
- These triggers do not apply to `IsActive` deactivation tables and should not be copied there.
