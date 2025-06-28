# Firebase Admin SDK Setup Guide

## Prerequisites
1. Firebase project with Authentication enabled
2. Users already registered via Firebase Auth (Google & Microsoft SSO)

## Setup Steps

### 1. Generate Service Account Key
1. Go to [Firebase Console](https://console.firebase.google.com)
2. Select your project
3. Navigate to **Project Settings** > **Service Accounts**
4. Click **Generate new private key**
5. Download the JSON file and rename it to `firebase-key.json`
6. Place `firebase-key.json` in your project root directory

### 2. Configure Authentication

**Required Setup (Using firebase-key.json file):**
1. Rename your downloaded service account file to `firebase-key.json`
2. Place it in the project root directory (same level as `Program.cs`)
3. The file is already added to `.gitignore` for security
4. The API will automatically use this file through the `FirebaseInitializer.InitializeFirebase()` method

**File structure should look like:**
```
iskxpress_api/
├── firebase-key.json          # ← Your Firebase service account key
├── Program.cs
├── appsettings.json
└── ...
```

**⚠️ Important Security Notes:**
- Never commit `firebase-key.json` to git
- The file is already in `.gitignore` 
- For production, use environment variables or Azure Key Vault

### 3. Test the Integration
1. Run your API
2. Call `GET /api/User/sync` endpoint
3. Check the response for sync statistics

## Sync Behavior
- **Microsoft SSO users** → `User` role
- **Google SSO users** → `Vendor` role
- **Existing users** → Updated with latest Firebase data
- **New users** → Created with appropriate role based on auth provider

## Flutter Integration
```dart
// Simple sync call from Flutter
final response = await http.get(
  Uri.parse('$apiUrl/api/User/sync'),
  headers: {'Accept': 'application/json'},
);

final syncResult = jsonDecode(response.body);
print('Synced: ${syncResult['newUsers']} new, ${syncResult['updatedUsers']} updated');
```

## Troubleshooting
- Ensure Firebase project has users registered
- Verify service account has Firebase Authentication Admin privileges
- Check API logs for specific error messages
- Test Firebase connectivity with a simple Firebase Admin SDK call 