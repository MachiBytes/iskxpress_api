# Google Users Endpoint

## Overview

The `GET /api/users/google` endpoint retrieves all Google accounts registered in Firebase Authentication.

## Endpoint Details

- **URL**: `GET /api/users/google`
- **Authentication**: None required (but should be protected in production)
- **Content-Type**: `application/json`

## Response Format

Returns an array of `GoogleUserResponse` objects:

```json
[
  {
    "uid": "firebase_uid_123",
    "email": "user@gmail.com",
    "displayName": "John Doe",
    "photoUrl": "https://lh3.googleusercontent.com/a/...",
    "emailVerified": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "lastSignInAt": "2024-12-20T14:45:00Z",
    "disabled": false
  }
]
```

## Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `uid` | string | Firebase UID (unique identifier) |
| `email` | string | User's email address |
| `displayName` | string? | User's display name (nullable) |
| `photoUrl` | string? | User's profile picture URL (nullable) |
| `emailVerified` | boolean | Whether the email is verified |
| `createdAt` | DateTime | When the user account was created |
| `lastSignInAt` | DateTime? | When the user last signed in (nullable) |
| `disabled` | boolean | Whether the user account is disabled |

## Implementation Details

### Firebase Integration

The endpoint uses Firebase Admin SDK to:
1. Initialize Firebase connection using `firebase-key.json`
2. List all users from Firebase Authentication
3. Filter users who have Google as their authentication provider
4. Map Firebase user data to `GoogleUserResponse` DTOs

### Error Handling

- If Firebase connection fails, the endpoint returns an empty array
- Errors are logged but don't break the API response
- In production, consider adding proper authentication and authorization

### Requirements

- `firebase-key.json` file must be present in the project root
- Firebase Admin SDK must be properly configured
- Internet connection required to access Firebase Auth

## Example Usage

### cURL
```bash
curl -X GET "http://localhost:5000/api/users/google" \
  -H "Content-Type: application/json"
```

### HTTP Request
```http
GET /api/users/google HTTP/1.1
Host: localhost:5000
Content-Type: application/json
```

## Security Considerations

1. **Authentication**: This endpoint should be protected with proper authentication in production
2. **Authorization**: Consider role-based access control (admin-only access)
3. **Rate Limiting**: Implement rate limiting to prevent abuse
4. **Data Privacy**: Ensure compliance with data protection regulations
5. **Firebase Key**: Keep the Firebase service account key secure

## Related Endpoints

- `GET /api/users` - Get all local users
- `GET /api/users/sync` - Sync Firebase users with local database
- `GET /api/users/{id}` - Get specific user by ID
- `GET /api/users/email/{email}` - Get user by email address 