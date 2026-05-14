SET QUOTED_IDENTIFIER ON;
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'core.FileStorageItem', N'U') IS NULL
BEGIN
    CREATE TABLE core.FileStorageItem
    (
        FileStorageItemID int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_FileStorageItem PRIMARY KEY,
        FileCategory nvarchar(50) NOT NULL,
        OriginalFileName nvarchar(255) NOT NULL,
        StoredFileName nvarchar(255) NOT NULL,
        RelativePath nvarchar(500) NOT NULL,
        ContentType nvarchar(100) NULL,
        FileExtension nvarchar(20) NOT NULL,
        FileSizeBytes bigint NOT NULL,
        FileHashSHA256 char(64) NOT NULL,
        HashAlgorithm nvarchar(20) NOT NULL
            CONSTRAINT DF_FileStorageItem_HashAlgorithm DEFAULT (N'SHA-256'),
        UploadedByUserID int NULL,
        UploadedAt datetime2 NOT NULL
            CONSTRAINT DF_FileStorageItem_UploadedAt DEFAULT (SYSUTCDATETIME()),
        IsActive bit NOT NULL
            CONSTRAINT DF_FileStorageItem_IsActive DEFAULT (1),
        LastIntegrityCheckAt datetime2 NULL,
        CONSTRAINT CK_FileStorageItem_FileSizeBytes_Positive CHECK (FileSizeBytes > 0),
        CONSTRAINT CK_FileStorageItem_FileHashSHA256_Length CHECK (LEN(FileHashSHA256) = 64)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_FileStorageItem_RelativePath'
      AND object_id = OBJECT_ID(N'core.FileStorageItem'))
BEGIN
    CREATE UNIQUE INDEX UX_FileStorageItem_RelativePath
        ON core.FileStorageItem(RelativePath);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_FileStorageItem_Active_Category_Hash_Size'
      AND object_id = OBJECT_ID(N'core.FileStorageItem'))
BEGIN
    CREATE UNIQUE INDEX UX_FileStorageItem_Active_Category_Hash_Size
        ON core.FileStorageItem(FileCategory, FileHashSHA256, FileSizeBytes)
        WHERE IsActive = 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_FileStorageItem_User_UploadedByUserID')
BEGIN
    ALTER TABLE core.FileStorageItem
        ADD CONSTRAINT FK_FileStorageItem_User_UploadedByUserID
        FOREIGN KEY (UploadedByUserID)
        REFERENCES sec.[User](UserID);
END;

IF COL_LENGTH(N'leave.LeaveAttachment', N'FileStorageItemID') IS NULL
BEGIN
    ALTER TABLE leave.LeaveAttachment
        ADD FileStorageItemID int NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_LeaveAttachment_FileStorageItem_FileStorageItemID')
BEGIN
    ALTER TABLE leave.LeaveAttachment
        ADD CONSTRAINT FK_LeaveAttachment_FileStorageItem_FileStorageItemID
        FOREIGN KEY (FileStorageItemID)
        REFERENCES core.FileStorageItem(FileStorageItemID);
END;

IF COL_LENGTH(N'core.Person', N'PhotoFileStorageItemID') IS NULL
BEGIN
    ALTER TABLE core.Person
        ADD PhotoFileStorageItemID int NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Person_FileStorageItem_PhotoFileStorageItemID')
BEGIN
    ALTER TABLE core.Person
        ADD CONSTRAINT FK_Person_FileStorageItem_PhotoFileStorageItemID
        FOREIGN KEY (PhotoFileStorageItemID)
        REFERENCES core.FileStorageItem(FileStorageItemID);
END;

IF COL_LENGTH(N'emp.EmployeeDocuments', N'FileStorageItemID') IS NULL
BEGIN
    ALTER TABLE emp.EmployeeDocuments
        ADD FileStorageItemID int NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_EmployeeDocuments_FileStorageItem_FileStorageItemID')
BEGIN
    ALTER TABLE emp.EmployeeDocuments
        ADD CONSTRAINT FK_EmployeeDocuments_FileStorageItem_FileStorageItemID
        FOREIGN KEY (FileStorageItemID)
        REFERENCES core.FileStorageItem(FileStorageItemID);
END;

COMMIT TRANSACTION;
GO
