# Employee Termination Verification Full-Stack Summary

## API Contract

Step 1 response:

```json
{
  "verificationRequestId": "guid-here",
  "deliveryMethod": "Email",
  "destinationMasked": "m***@example.com",
  "expiresAt": "2026-06-05T12:00:00Z",
  "message": "Verification code was sent."
}
```

Step 2 request:

```json
{
  "verificationRequestId": "guid-here",
  "verificationCode": "123456",
  "terminationReasonId": 1,
  "terminationDate": "2026-06-05",
  "isEligibleForRehire": true,
  "remarks": "Optional HR note"
}
```

## Backend

The backend stores one-time verification records in `sec.ActionVerificationCode`, hashes codes with salted PBKDF2-SHA256, enforces 5-minute expiry and max attempts, and logs verification activity without code disclosure.

## Frontend

The frontend dialog first requests a code, then confirms termination. Mock mode uses `123456` locally and routes all employee state mutation through `confirmEmployeeTermination`.

## Remaining Integration Work

- Apply the SQL table script to existing databases.
- Replace the mock notification sender with a real Email/SMS provider.
- Connect frontend `employeeService` to the real API client.
- Fully remove the legacy direct termination API route after client migration.
