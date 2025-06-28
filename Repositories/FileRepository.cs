using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository for file operations combining S3 storage and database tracking
/// </summary>
public class FileRepository : IFileRepository
{
    private readonly IskExpressDbContext _context;
    private readonly IS3Repository _s3Repository;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(IskExpressDbContext context, IS3Repository s3Repository, ILogger<FileRepository> logger)
    {
        _context = context;
        _s3Repository = s3Repository;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file to S3 and saves the record to database
    /// </summary>
    public async Task<FileRecord> UploadFileAsync(FileType fileType, int entityId, Stream fileStream, string contentType, string originalFileName, string fileExtension)
    {
        try
        {
            // Remove any existing file for this entity and type
            await RemoveFileByEntityAsync(fileType, entityId);

            // Generate object key based on file type
            var objectKey = GenerateObjectKey(fileType, entityId, fileExtension);

            // Upload to S3
            var objectUrl = await _s3Repository.UploadFileAsync(objectKey, fileStream, contentType);

            // Create file record
            var file = new FileRecord
            {
                Type = fileType,
                ObjectKey = objectKey,
                ObjectUrl = objectUrl,
                EntityId = entityId,
                OriginalFileName = originalFileName,
                ContentType = contentType,
                FileSizeBytes = fileStream.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            _context.Files.Add(file);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully uploaded and tracked file: {ObjectKey} for entity {EntityId}", objectKey, entityId);
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for entity {EntityId} of type {FileType}", entityId, fileType);
            throw;
        }
    }

    /// <summary>
    /// Removes a file from S3 and deletes the record from database
    /// </summary>
    public async Task<bool> RemoveFileAsync(int fileId)
    {
        try
        {
            var file = await _context.Files.FindAsync(fileId);
            if (file == null)
            {
                _logger.LogWarning("File with ID {FileId} not found", fileId);
                return false;
            }

            // Remove from S3
            var s3Success = await _s3Repository.RemoveFileAsync(file.ObjectKey);

            // Remove from database (even if S3 removal failed)
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully removed file: {ObjectKey} (S3 Success: {S3Success})", file.ObjectKey, s3Success);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing file with ID {FileId}", fileId);
            return false;
        }
    }

    /// <summary>
    /// Removes a file by entity ID and type
    /// </summary>
    public async Task<bool> RemoveFileByEntityAsync(FileType fileType, int entityId)
    {
        try
        {
            var files = await _context.Files
                .Where(f => f.Type == fileType && f.EntityId == entityId)
                .ToListAsync();

            if (!files.Any())
            {
                return true; // No files to remove, consider it success
            }

            var success = true;
            foreach (var file in files)
            {
                // Remove from S3
                var s3Success = await _s3Repository.RemoveFileAsync(file.ObjectKey);
                if (!s3Success)
                {
                    success = false;
                    _logger.LogWarning("Failed to remove file from S3: {ObjectKey}", file.ObjectKey);
                }

                // Remove from database
                _context.Files.Remove(file);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed {FileCount} files for entity {EntityId} of type {FileType}", files.Count, entityId, fileType);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing files for entity {EntityId} of type {FileType}", entityId, fileType);
            return false;
        }
    }

    /// <summary>
    /// Gets a file record by ID
    /// </summary>
    public async Task<FileRecord?> GetFileByIdAsync(int fileId)
    {
        return await _context.Files.FindAsync(fileId);
    }

    /// <summary>
    /// Gets files by entity ID and type
    /// </summary>
    public async Task<List<FileRecord>> GetFilesByEntityAsync(FileType fileType, int entityId)
    {
        return await _context.Files
            .Where(f => f.Type == fileType && f.EntityId == entityId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all files of a specific type
    /// </summary>
    public async Task<List<FileRecord>> GetFilesByTypeAsync(FileType fileType)
    {
        return await _context.Files
            .Where(f => f.Type == fileType)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing file record
    /// </summary>
    public async Task<FileRecord> UpdateFileAsync(FileRecord file)
    {
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        await _context.SaveChangesAsync();
        return file;
    }

    /// <summary>
    /// Generates the S3 object key based on file type and entity ID
    /// </summary>
    private static string GenerateObjectKey(FileType fileType, int entityId, string fileExtension)
    {
        return fileType switch
        {
            FileType.UserAvatar => $"user_avatars/{entityId}.{fileExtension}",
            FileType.StallAvatar => $"stall_avatars/{entityId}.{fileExtension}",
            FileType.ProductImage => $"product_pictures/{entityId}.{fileExtension}",
            _ => throw new ArgumentException($"Unsupported file type: {fileType}")
        };
    }
} 