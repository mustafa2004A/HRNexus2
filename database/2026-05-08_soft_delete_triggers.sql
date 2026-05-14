SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER TRIGGER core.TR_Person_SoftDelete
ON core.Person
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentUserID int =
        TRY_CAST(SESSION_CONTEXT(N'CurrentUserID') AS int);

    DECLARE @DeletedAt datetime2(7) = SYSUTCDATETIME();

    UPDATE person
    SET
        person.IsDeleted = 1,
        person.DeletedDate = @DeletedAt,
        person.DeletedBy = @CurrentUserID,
        person.ModifiedDate = @DeletedAt,
        person.ModifiedBy = @CurrentUserID
    FROM core.Person AS person
    INNER JOIN deleted AS deletedRows
        ON deletedRows.PersonID = person.PersonID
    WHERE person.IsDeleted = 0;
END;
GO

CREATE OR ALTER TRIGGER emp.TR_Employee_SoftDelete
ON emp.Employee
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentUserID int =
        TRY_CAST(SESSION_CONTEXT(N'CurrentUserID') AS int);

    DECLARE @DeletedAt datetime2(7) = SYSUTCDATETIME();

    UPDATE employee
    SET
        employee.IsDeleted = 1,
        employee.DeletedDate = @DeletedAt,
        employee.DeletedBy = @CurrentUserID,
        employee.ModifiedDate = @DeletedAt,
        employee.ModifiedBy = @CurrentUserID
    FROM emp.Employee AS employee
    INNER JOIN deleted AS deletedRows
        ON deletedRows.EmployeeID = employee.EmployeeID
    WHERE employee.IsDeleted = 0;
END;
GO
