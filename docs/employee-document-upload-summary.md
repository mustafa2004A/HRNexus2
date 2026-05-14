# Employee Document Upload Summary

## 1. Endpoint Added

`POST /api/employees/{employeeId:int}/documents/upload`

This pass wires employee document upload into the existing `core.FileStorageItem` local-storage foundation. It does not redesign file storage or replace the existing employee document metadata CRUD endpoints.

## 2. Request Format

The endpoint accepts `multipart/form-data`:

- `file` - required uploaded document file.
- `documentTypeId` - required positive integer.
- `documentName` - required, max 255 characters.
- `referenceNumber` - optional, max 50 characters.
- `issueDate` - optional.
- `expiryDate` - optional.
- `remarks` - optional, max 500 characters.

`employeeId` comes from the route. `UploadedBy` is resolved from the authenticated user context.

## 3. Authorization Behavior

The upload endpoint uses the existing `SelfOrHr` policy:

- Admin / HRManager / HRClerk can upload documents for any employee.
- A normal employee can upload only for their own `employeeId`.
- A normal employee cannot upload documents for another employee.

The file integrity endpoint uses the stricter `SecurityAdmin` policy.

## 4. Validation Rules

The business service validates:

- employee exists.
- document type exists.
- inactive document types are rejected.
- expiry date is required when `DocumentType.IsExpiryTracked = true`.
- `IssueDate <= ExpiryDate` when both are provided.
- uploaded file exists and is non-empty.
- file size and extension are enforced by `IFileStorageService`.

## 5. Storage Folder

Employee document files are stored under:

`{HRNEXUS_STORAGE_ROOT}\EmployeeDocuments`

When `HRNEXUS_STORAGE_ROOT` is not set, the current Development fallback is:

`C:\HRNexusStorage`

## 6. File Category Used

Employee document uploads use:

`EmployeeDocument`

This category is part of the deduplication key, so a document with the same content as a leave attachment is not treated as the same stored file.

## 7. Deduplication Behavior

The existing local file storage service computes SHA-256, compares active files by:

- `FileCategory`
- `FileHashSHA256`
- `FileSizeBytes`

If a match exists, the existing `FileStorageItem` row is reused and no duplicate physical file is written. Otherwise, a new GUID-based physical filename is created.

## 8. Database Fields Updated

For upload-created employee documents, the service sets:

- `EmployeeID`
- `DocumentTypeID`
- `FileStorageItemID`
- `DocumentName`
- `ReferenceNumber`
- `FilePath = FileStorageItem.RelativePath`
- `FileExtension = FileStorageItem.FileExtension`
- `IssueDate`
- `ExpiryDate`
- `IsVerified = false`
- `VerifiedBy = null`
- `VerifiedDate = null`
- `IsActive = true`
- `UploadedBy = current user id`
- `UploadedDate = UTC now`
- `Remarks`

The API returns safe relative paths only, never physical absolute paths.

## 9. Response Behavior

The upload endpoint returns `201 Created` with `EmployeeDocumentDto`, including:

- document identity and employee id.
- document type id/name.
- document name/reference.
- safe relative `FilePath`.
- `FileStorageItemId`.
- issue/expiry dates.
- verification state.
- active/upload metadata.

## 10. Integrity Endpoint

`POST /api/security/file-storage-items/{fileStorageItemId:int}/verify-integrity`

Authorization:

- `SecurityAdmin`

Behavior:

- returns `404` when the `FileStorageItem` does not exist.
- resolves the file using configured storage root plus safe relative path.
- returns `fileExists = false` if the physical file is missing.
- recomputes SHA-256 if the file exists.
- compares hash and file size.
- updates `LastIntegrityCheckAt`.
- returns `isIntegrityValid = false` when hash or size validation fails.

The response does not expose absolute physical paths or file content.

## 11. Future Work

- Add richer upload rules per document type if the UI needs stricter file-type mapping.
- Add a download endpoint with ownership-aware authorization.
- Add HMAC-based tamper protection if stronger proof than SHA-256 integrity checking is required.
- Add cloud/object storage implementation behind the existing `IFileStorageService` contract when moving beyond local demo storage.
