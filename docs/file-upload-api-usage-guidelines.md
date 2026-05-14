# File Upload API Usage Guidelines

## Why Manual FilePath Input Is Not Allowed

Files must not be created by pasting `FilePath`, physical local paths, arbitrary URLs, or copied absolute paths into Swagger request bodies. Client-supplied paths can expose local machine details, bypass file validation, skip hashing/deduplication, and create database rows that do not point to controlled storage.

HRNexus stores files through `core.FileStorageItem` and the local file storage service. The backend is responsible for naming, hashing, sizing, and storing files.

## Official Upload Endpoints

Use these upload APIs for file/image ingestion:

- `POST /api/people/{personId:int}/photo`
- `POST /api/leave-attachments/upload`
- `POST /api/employees/{employeeId:int}/documents/upload`

Each endpoint accepts `multipart/form-data` and an `IFormFile` field as displayed by Swagger.

Employee creation is intentionally not a file upload endpoint. Use `POST /api/employees` with `application/json` to create the employee, read the returned `PersonId`, then upload the photo through `POST /api/people/{personId:int}/photo`.

`POST /api/employees/with-photo` is not part of the API.

## What The Client Provides

The client provides:

- the selected file through Swagger/browser multipart upload.
- business metadata such as `personId`, `leaveRequestId`, `employeeId`, `documentTypeId`, `documentName`, dates, reference number, and remarks.

The client does not provide:

- physical local path.
- generated relative path.
- stored file name.
- file hash.
- file size.
- file extension for storage.
- `FileStorageItemID` for normal uploads.

## What The Backend Generates

The backend saves through `IFileStorageService` and generates or resolves:

- GUID-based physical stored file name.
- safe relative path.
- SHA-256 hash.
- file size.
- normalized file extension.
- `FileStorageItemID`.

If an active file already exists with the same category, hash, and size, the service reuses the existing `FileStorageItem` instead of creating a duplicate physical file.

## Response Path Rules

Responses may include safe relative paths such as:

```text
EmployeeDocuments/9f4a71a3a1df4572a7a76e37c29ef8c9.pdf
```

Responses must not expose physical absolute paths such as:

```text
C:\HRNexusStorage\EmployeeDocuments\9f4a71a3a1df4572a7a76e37c29ef8c9.pdf
```

Legacy absolute paths or URLs are not treated as safe public response paths.

## Current Cleanup Decisions

- Removed the legacy JSON leave attachment create endpoint that accepted `FilePath`.
- Removed the legacy JSON employee document create endpoint that accepted `FilePath` and `FileExtension`.
- Kept employee document update as metadata-only; it no longer accepts or changes file path/extension.
- Removed `PhotoUrl` from person create/update requests. Person photos must be changed with `POST /api/people/{personId:int}/photo`.
- Kept `POST /api/employees` as a JSON-only employee creation endpoint. It does not accept `requestJson`, `PhotoUrl`, `PhotoFileStorageItemID`, `FilePath`, or a photo file.
- Kept file path, extension, and `FileStorageItemId` in read DTOs as backend-generated metadata only.

## Swagger Behavior

The official upload endpoints use `[Consumes("multipart/form-data")]` and `IFormFile`, so Swagger should show a file picker. Users should choose a file instead of typing or pasting a path string.

`POST /api/employees` should show an `application/json` request body, not `requestJson` and not a photo file picker.

## Security Notes

- The backend validates file presence, size, extension, and category-specific rules.
- The backend does not trust client file names for physical storage names.
- Physical storage paths are not returned to clients.
- `FileStorageItemID` is read-only metadata for normal clients.
- File integrity verification is restricted to security/admin use.

## Future Secure Download And Preview

A later hardening pass should add controlled download/preview endpoints that authorize access and stream files by `FileStorageItemID` or owning business record. Public APIs should continue avoiding physical path disclosure and should prefer authorization-checked download URLs or streamed responses over direct storage paths.
