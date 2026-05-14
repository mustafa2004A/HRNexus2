# File Storage Item and Local Upload Summary

## 1. FileStorageItem Purpose

`core.FileStorageItem` is the shared metadata table for files stored by HRNexus local storage. It keeps file identity, storage metadata, content hash, upload metadata, active state, and integrity-check metadata in one central place instead of scattering physical file details across business tables.

The table stores safe relative paths only. API responses must not expose the physical local root path.

## 2. Table Relationships

The file-storage relationship columns are nullable so existing rows continue to work:

- `core.Person.PhotoFileStorageItemID -> core.FileStorageItem.FileStorageItemID`
- `leave.LeaveAttachment.FileStorageItemID -> core.FileStorageItem.FileStorageItemID`
- `emp.EmployeeDocuments.FileStorageItemID -> core.FileStorageItem.FileStorageItemID`
- `core.FileStorageItem.UploadedByUserID -> sec.[User].UserID`

Delete behavior is restricted. Business records do not cascade-delete file metadata.

## 3. GUID + SHA-256 Strategy

Physical stored file names use a dashless GUID plus the normalized file extension, for example:

`9f4a71a3a1df4572a7a76e37c29ef8c9.pdf`

The GUID keeps stored names unguessable and decoupled from original names and file content. SHA-256 is computed from the uploaded content and stored as `FileHashSHA256` for deduplication and later integrity verification.

## 4. Deduplication Behavior

Before saving a new physical file, the storage service computes SHA-256 and file size, then checks for an active file with the same:

- `FileCategory`
- `FileHashSHA256`
- `FileSizeBytes`

If one exists, the service reuses the existing `FileStorageItem` and does not copy the file again. Otherwise, it writes a new GUID-named file and creates a new metadata row.

## 5. Integrity Verification Behavior

`IFileStorageService.VerifyIntegrityAsync(fileStorageItemId)` loads the metadata row, resolves the safe physical path under the configured root, recomputes SHA-256, compares it with `FileHashSHA256`, updates `LastIntegrityCheckAt`, and returns a success/failure result.

Integrity checks are explicit and do not make unrelated endpoints fail.

## 6. Local Storage Root

The local storage root is resolved in this order:

1. `HRNEXUS_STORAGE_ROOT`
2. `FileStorage:RootPath`
3. Development fallback: `C:\HRNexusStorage`

Configured folders:

- leave attachments: `LeaveAttachments`
- person photos: `PersonPhotos`
- employee documents: `EmployeeDocuments`

## 7. HRNEXUS_STORAGE_ROOT

Set this environment variable locally to control where files are written:

```powershell
$env:HRNEXUS_STORAGE_ROOT = 'C:\HRNexusStorage'
```

The API stores only relative paths such as `LeaveAttachments/{storedFileName}`.

## 8. HRNEXUS_JWT_SIGNING_KEY

JWT signing now requires:

```powershell
$env:HRNEXUS_JWT_SIGNING_KEY = '<local 32+ byte development secret>'
```

The API fails fast if the variable is missing or shorter than 32 bytes. A real secret is not committed to appsettings or documentation.

## 9. Leave Attachment Upload Behavior

`POST /api/leave-attachments/upload` accepts `multipart/form-data` with:

- `leaveRequestId`
- `file`
- optional `uploadedByUserId`

The endpoint uses the existing leave-attachment ownership/reviewer/HR authorization rules, validates size and extension, saves through `IFileStorageService`, creates a `leave.LeaveAttachment` row, and sets:

- `FileStorageItemID`
- `FileName`
- `FilePath` as safe relative path
- `FileExtension`
- `UploadedBy`
- `UploadedAt`
- `IsActive = true`

The legacy metadata-only leave attachment endpoint remains for backward compatibility.

## 10. Person Photo Upload Behavior

`POST /api/people/{personId}/photo` accepts `multipart/form-data` with a `file` field.

Allowed photo extensions are:

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

The endpoint saves through `IFileStorageService`, updates `core.Person.PhotoFileStorageItemID`, mirrors the safe relative path into `Person.PhotoURL`, and returns safe file metadata. The route remains protected by the existing HR/Admin people-controller policy.

## 11. Security Cautions

- Empty files are rejected.
- Unsupported extensions are rejected.
- Max file size is enforced by `FileStorage:MaxFileSizeBytes`.
- Client file names are normalized with `Path.GetFileName`.
- Stored physical file names are generated server-side.
- Resolved paths must remain under the configured storage root.
- Physical local paths are not returned by the API.
- File content and sensitive identifier values must not be logged.

## 12. Future Backup Strategy

For production-style operation, the storage root should be included in backup/restore planning alongside the database. Database rows and physical files must be backed up consistently so relative paths continue to resolve after restore.

## 13. Future HMAC / Cloud Storage Improvements

SHA-256 detects content changes but does not prove trusted authorship. A later hardening pass can add HMAC-based tamper protection and/or move the storage implementation behind the same `IFileStorageService` contract to cloud/object storage.
