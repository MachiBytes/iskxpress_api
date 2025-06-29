using Amazon.S3;
using Amazon.S3.Model;

namespace iskxpress_api.Repositories;

/// <summary>
/// AWS S3 repository implementation for file operations
/// </summary>
public class S3Repository : IS3Repository
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Repository> _logger;
    private readonly string _bucketName;

    public S3Repository(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Repository> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("AWS S3 bucket name not configured");
    }

    /// <summary>
    /// Uploads a file to S3 and returns the public URL
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) for the file</param>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <returns>The public URL of the uploaded file</returns>
    public async Task<string> UploadFileAsync(string objectKey, Stream fileStream, string contentType)
    {
        try
        {
            // Remove leading slash if present for consistency
            objectKey = objectKey.TrimStart('/');

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = fileStream,
                ContentType = contentType,
            };

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                // Construct the public URL
                var region = _configuration["AWS:Region"] ?? "us-east-1";
                var publicUrl = $"https://{_bucketName}.s3.{region}.amazonaws.com/{objectKey}";
                
                _logger.LogInformation("Successfully uploaded file to S3: {ObjectKey}", objectKey);
                return publicUrl;
            }
            else
            {
                _logger.LogError("Failed to upload file to S3. Status code: {StatusCode}", response.HttpStatusCode);
                throw new Exception($"Failed to upload file to S3. Status code: {response.HttpStatusCode}");
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 error occurred while uploading file: {ObjectKey}", objectKey);
            throw new Exception($"AWS S3 error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while uploading file: {ObjectKey}", objectKey);
            throw;
        }
    }

    /// <summary>
    /// Removes a file from S3
    /// </summary>
    /// <param name="objectKey">The S3 object key (path) of the file to remove</param>
    /// <returns>True if the file was successfully removed, false otherwise</returns>
    public async Task<bool> RemoveFileAsync(string objectKey)
    {
        try
        {
            // Remove leading slash if present for consistency
            objectKey = objectKey.TrimStart('/');

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            var response = await _s3Client.DeleteObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent || 
                response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully removed file from S3: {ObjectKey}", objectKey);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to remove file from S3. Status code: {StatusCode}", response.HttpStatusCode);
                return false;
            }
        }
        catch (AmazonS3Exception ex)
        {
            // If the object doesn't exist, consider it as successfully removed
            if (ex.ErrorCode == "NoSuchKey")
            {
                _logger.LogInformation("File does not exist in S3, considering as removed: {ObjectKey}", objectKey);
                return true;
            }

            _logger.LogError(ex, "AWS S3 error occurred while removing file: {ObjectKey}", objectKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while removing file: {ObjectKey}", objectKey);
            return false;
        }
    }
} 