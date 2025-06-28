using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Interface for file operations combining S3 storage and database tracking
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Uploads a file to S3 and saves the record to database
    /// </summary>
    /// <param name="fileType">Type of file being uploaded</param>
    /// <param name="entityId">ID of the entity this file belongs to</param>
    /// <param name="fileStream">File stream to upload</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="originalFileName">Original filename</param>
    /// <param name="fileExtension">File extension (e.g., "jpg", "png")</param>
    /// <returns>The created file record with URL</returns>
    Task<FileRecord> UploadFileAsync(FileType fileType, int entityId, Stream fileStream, string contentType, string originalFileName, string fileExtension);

    /// <summary>
    /// Removes a file from S3 and deletes the record from database
    /// </summary>
    /// <param name="fileId">ID of the file to remove</param>
    /// <returns>True if successfully removed, false otherwise</returns>
    Task<bool> RemoveFileAsync(int fileId);

    /// <summary>
    /// Removes a file by entity ID and type
    /// </summary>
    /// <param name="fileType">Type of file to remove</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>True if successfully removed, false otherwise</returns>
    Task<bool> RemoveFileByEntityAsync(FileType fileType, int entityId);

    /// <summary>
    /// Gets a file record by ID
    /// </summary>
    /// <param name="fileId">ID of the file</param>
    /// <returns>File record or null if not found</returns>
    Task<FileRecord?> GetFileByIdAsync(int fileId);

    /// <summary>
    /// Gets files by entity ID and type
    /// </summary>
    /// <param name="fileType">Type of files to retrieve</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>List of file records</returns>
    Task<List<FileRecord>> GetFilesByEntityAsync(FileType fileType, int entityId);

    /// <summary>
    /// Gets all files of a specific type
    /// </summary>
    /// <param name="fileType">Type of files to retrieve</param>
    /// <returns>List of file records</returns>
    Task<List<FileRecord>> GetFilesByTypeAsync(FileType fileType);

    /// <summary>
    /// Updates an existing file record
    /// </summary>
    /// <param name="file">File record to update</param>
    /// <returns>Updated file record</returns>
    Task<FileRecord> UpdateFileAsync(FileRecord file);
} 