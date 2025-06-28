namespace iskxpress_api.Repositories;

/// <summary>
/// Interface for AWS S3 file operations
/// </summary>
public interface IS3Repository
{
    /// <summary>
    /// Uploads a file to S3 and returns the public URL
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) for the file</param>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <returns>The public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(string objectKey, Stream fileStream, string contentType);

    /// <summary>
    /// Removes a file from S3
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) of the file to remove</param>
    /// <returns>True if the file was successfully removed, false otherwise</returns>
    Task<bool> RemoveFileAsync(string objectKey);
} 