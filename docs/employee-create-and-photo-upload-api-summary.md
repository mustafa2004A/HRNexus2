# Employee Create And Photo Upload API Summary

## Employee Creation

`POST /api/employees` is JSON-only. It accepts the normal `CreateEmployeeRequest` payload and creates:

- `core.Person`
- `emp.Employee`
- optional initial `emp.EmployeeJobHistory`

The endpoint does not accept `requestJson`, `multipart/form-data`, `PhotoUrl`, `PhotoFileStorageItemID`, `FilePath`, or a photo file. Photo values are backend-generated metadata and are not client input.

`employee.employeeCode` is not part of the request. The backend generates it during employee creation using `emp.EmployeeCodeSequence`.

## Dedicated Photo Upload

Person photos are uploaded only through:

```http
POST /api/people/{personId:int}/photo
```

That endpoint accepts `multipart/form-data` with an `IFormFile` named `photo`. It validates the person, authorization, file presence, size, and image extension before saving through the shared file storage service.

Allowed image extensions are:

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

## Why Manual Paths Are Not Allowed

Clients must not provide `PhotoUrl`, `FilePath`, physical local paths, arbitrary URLs, or `PhotoFileStorageItemID`. Manual path input can bypass validation, hashing, deduplication, and controlled storage rules. The backend owns storage naming and metadata.

## FileStorageItem Behavior

Photo upload stores file metadata in `core.FileStorageItem`:

- GUID-based physical stored filename
- safe relative path
- SHA-256 content hash
- file size
- normalized extension
- upload metadata

If an active person-photo file already exists with the same category, hash, and file size, the storage service reuses the existing `FileStorageItem`.

After a successful photo upload, the backend updates:

- `core.Person.PhotoFileStorageItemID`
- `core.Person.PhotoURL = FileStorageItem.RelativePath`

Responses may include `PhotoUrl` as a safe relative path and `PhotoFileStorageItemId`. They must not expose absolute paths such as `C:\HRNexusStorage\...`.

## Swagger Usage

1. Create the employee with `POST /api/employees` using `application/json`.
2. Read the returned `PersonId`.
3. Upload the photo with `POST /api/people/{personId:int}/photo`.
4. Use Swagger's file picker on the photo endpoint.

`POST /api/employees/with-photo` is not part of the API.
