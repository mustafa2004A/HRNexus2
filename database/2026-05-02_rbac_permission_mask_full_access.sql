SET QUOTED_IDENTIFIER ON;
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM sec.Permission
    WHERE BitValue <= 0)
BEGIN
    THROW 51000, 'sec.Permission.BitValue must contain positive atomic bit values only.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM sec.RolePermissions
    WHERE PermissionMask < -1)
BEGIN
    THROW 51001, 'sec.RolePermissions.PermissionMask must be -1 or non-negative.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM sec.UserPermissions
    WHERE PermissionMask < -1)
BEGIN
    THROW 51002, 'sec.UserPermissions.PermissionMask must be -1 or non-negative.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_RolePermissions_PermissionMask_AllowedValues'
      AND parent_object_id = OBJECT_ID(N'sec.RolePermissions'))
BEGIN
    ALTER TABLE sec.RolePermissions
        DROP CONSTRAINT CK_RolePermissions_PermissionMask_AllowedValues;
END;

ALTER TABLE sec.RolePermissions
    ADD CONSTRAINT CK_RolePermissions_PermissionMask_AllowedValues
    CHECK (PermissionMask = -1 OR PermissionMask >= 0);

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_UserPermissions_PermissionMask_AllowedValues'
      AND parent_object_id = OBJECT_ID(N'sec.UserPermissions'))
BEGIN
    ALTER TABLE sec.UserPermissions
        DROP CONSTRAINT CK_UserPermissions_PermissionMask_AllowedValues;
END;

ALTER TABLE sec.UserPermissions
    ADD CONSTRAINT CK_UserPermissions_PermissionMask_AllowedValues
    CHECK (PermissionMask = -1 OR PermissionMask >= 0);

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Permission_BitValue_Positive'
      AND parent_object_id = OBJECT_ID(N'sec.Permission'))
BEGIN
    ALTER TABLE sec.Permission
        DROP CONSTRAINT CK_Permission_BitValue_Positive;
END;

ALTER TABLE sec.Permission
    ADD CONSTRAINT CK_Permission_BitValue_Positive
    CHECK (BitValue > 0);

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Permission_BitValue_PowerOfTwo'
      AND parent_object_id = OBJECT_ID(N'sec.Permission'))
BEGIN
    ALTER TABLE sec.Permission
        DROP CONSTRAINT CK_Permission_BitValue_PowerOfTwo;
END;

ALTER TABLE sec.Permission
    ADD CONSTRAINT CK_Permission_BitValue_PowerOfTwo
    CHECK ((BitValue & (BitValue - 1)) = 0);

DECLARE @FullPermissionMask int;

SELECT @FullPermissionMask = COALESCE(SUM(BitValue), 0)
FROM sec.Permission
WHERE IsActive = 1
  AND BitValue > 0;

IF @FullPermissionMask <= 0
BEGIN
    THROW 51003, 'Unable to calculate a positive full permission mask from active permissions.', 1;
END;

UPDATE sec.RolePermissions
SET PermissionMask = -1
WHERE PermissionMask = @FullPermissionMask;

UPDATE sec.UserPermissions
SET PermissionMask = -1
WHERE PermissionMask = @FullPermissionMask;

MERGE sec.RolePermissions AS target
USING
(
    SELECT role.RoleID, module.ModuleID
    FROM sec.[Role] AS role
    CROSS JOIN sec.[Module] AS module
    WHERE role.RoleName = N'Admin'
) AS source
ON target.RoleID = source.RoleID
   AND target.ModuleID = source.ModuleID
WHEN MATCHED AND target.PermissionMask <> -1 THEN
    UPDATE SET PermissionMask = -1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleID, ModuleID, PermissionMask)
    VALUES (source.RoleID, source.ModuleID, -1);

MERGE sec.RolePermissions AS target
USING
(
    SELECT role.RoleID, module.ModuleID
    FROM sec.[Role] AS role
    CROSS JOIN sec.[Module] AS module
    WHERE role.RoleName IN (N'HRManager', N'HRClerk', N'DepartmentManager', N'Employee')
) AS source
ON target.RoleID = source.RoleID
   AND target.ModuleID = source.ModuleID
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleID, ModuleID, PermissionMask)
    VALUES (source.RoleID, source.ModuleID, 0);

COMMIT TRANSACTION;
GO
