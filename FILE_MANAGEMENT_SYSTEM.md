# File Management System

## Overview
The ISK Express API now includes a comprehensive file management system that combines AWS S3 storage with database tracking. The system provides two main repositories:

1. **S3Repository**: Direct AWS S3 operations (upload/remove files)
2. **FileRepository**: High-level file management combining S3 operations with database tracking

## Architecture

```
Controller/Service
       ↓
  FileRepository ← (recommended for most use cases)
       ↓
  S3Repository + Database (FileRecord model)
       ↓
   AWS S3 Bucket
```

## Models

### FileRecord
The `FileRecord` model tracks all uploaded files:

```csharp
public class FileRecord
{
    public int Id { get; set; }
    public FileType Type { get; set; }        // UserAvatar, StallAvatar, ProductImage
    public string ObjectKey { get; set; }     // S3 object key/path
    public string ObjectUrl { get; set; }     // Public URL to access the file
    public int? EntityId { get; set; }        // ID of the related entity
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### FileType Enum
```csharp
public enum FileType
{
    UserAvatar,    // /user_avatars/user_id.ext
    StallAvatar,   // /stall_avatars/stall_id.ext
    ProductImage   // /product_pictures/product_id.ext
}
```

## Repositories

### FileRepository (Recommended)
High-level repository that combines S3 operations with database tracking.

**Key Features:**
- Automatic S3 object key generation based on file type and entity ID
- Database tracking of all file operations
- Automatic cleanup of existing files when uploading new ones
- Comprehensive error handling and logging

**Methods:**
- `UploadFileAsync()` - Upload file to S3 and save record
- `RemoveFileAsync(fileId)` - Remove file by database ID
- `RemoveFileByEntityAsync()` - Remove files by entity type and ID
- `GetFileByIdAsync()` - Get file record by ID
- `GetFilesByEntityAsync()` - Get files by entity type and ID
- `GetFilesByTypeAsync()` - Get all files of a specific type
- `UpdateFileAsync()` - Update file record metadata

### S3Repository (Low-level)
Direct AWS S3 operations for advanced use cases.

**Methods:**
- `UploadFileAsync(objectKey, stream, contentType)` - Direct S3 upload
- `RemoveFileAsync(objectKey)` - Direct S3 removal

## Usage Examples

### 1. Basic File Upload (Recommended)

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IFileRepository _fileRepository;

    public UserController(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    [HttpPost("{userId}/avatar")]
    public async Task<IActionResult> UploadUserAvatar(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        try
        {
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            
            using var stream = file.OpenReadStream();
            var fileRecord = await _fileRepository.UploadFileAsync(
                FileType.UserAvatar,
                userId,
                stream,
                file.ContentType,
                file.FileName,
                fileExtension
            );

            return Ok(new { 
                FileId = fileRecord.Id,
                PublicUrl = fileRecord.ObjectUrl 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading file: {ex.Message}");
        }
    }

    [HttpDelete("{userId}/avatar")]
    public async Task<IActionResult> RemoveUserAvatar(int userId)
    {
        try
        {
            var success = await _fileRepository.RemoveFileByEntityAsync(FileType.UserAvatar, userId);
            return success ? Ok("Avatar removed") : NotFound("Avatar not found");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error removing avatar: {ex.Message}");
        }
    }
}
```

### 2. Service Layer Implementation

```csharp
public class UserService : IUserService
{
    private readonly IFileRepository _fileRepository;
    private readonly IUserRepository _userRepository;

    public UserService(IFileRepository fileRepository, IUserRepository userRepository)
    {
        _fileRepository = fileRepository;
        _userRepository = userRepository;
    }

    public async Task<string> UpdateUserAvatarAsync(int userId, Stream imageStream, string contentType, string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName).TrimStart('.');
        
        var fileRecord = await _fileRepository.UploadFileAsync(
            FileType.UserAvatar,
            userId,
            imageStream,
            contentType,
            originalFileName,
            fileExtension
        );

        // Update user record with new avatar URL
        await _userRepository.UpdateUserAvatarAsync(userId, fileRecord.ObjectUrl);
        
        return fileRecord.ObjectUrl;
    }

    public async Task<List<FileRecord>> GetUserFilesAsync(int userId)
    {
        return await _fileRepository.GetFilesByEntityAsync(FileType.UserAvatar, userId);
    }
}
```

### 3. Direct S3Repository Usage (Advanced)

```csharp
public class CustomFileService
{
    private readonly IS3Repository _s3Repository;

    public async Task<string> UploadCustomFileAsync(string customObjectKey, Stream fileStream, string contentType)
    {
        // Direct S3 upload with custom object key
        return await _s3Repository.UploadFileAsync(customObjectKey, fileStream, contentType);
    }
}
```

## Object Key Patterns

The system automatically generates S3 object keys based on file type:

- **User Avatars**: `user_avatars/{userId}.{extension}`
- **Stall Avatars**: `stall_avatars/{stallId}.{extension}`
- **Product Images**: `product_pictures/{productId}.{extension}`

## Database Schema

The `Files` table includes:
- Unique constraint on `(Type, EntityId)` - ensures one file per entity per type
- Indexes for efficient querying
- Soft foreign key relationship via `EntityId`

## Configuration

Ensure your `appsettings.json` includes:

```json
{
  "AWS": {
    "Region": "ap-southeast-1",
    "S3": {
      "BucketName": "aki.iskxpress"
    }
  }
}
```

## Migration

To apply the database changes:

```bash
dotnet ef database update
```

## Best Practices

1. **Use FileRepository** for most file operations - it provides tracking and automatic cleanup
2. **Handle file extensions** properly - extract from original filename
3. **Validate file types** before upload (MIME type, file size, etc.)
4. **Use appropriate content types** for proper browser rendering
5. **Consider file size limits** for performance and storage costs
6. **Implement proper error handling** for upload failures

## Error Handling

The system includes comprehensive error handling:
- S3 upload failures are logged and thrown as exceptions
- Database failures are properly handled
- File conflicts are resolved by replacing existing files
- Missing files during deletion are handled gracefully

## Logging

All file operations are logged with appropriate levels:
- Info: Successful operations
- Warning: Non-critical issues (e.g., file not found during deletion)
- Error: Failed operations with full exception details 