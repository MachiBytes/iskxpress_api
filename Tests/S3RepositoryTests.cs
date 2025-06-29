using Xunit;
using Moq;
using FluentAssertions;
using Amazon.S3;
using Amazon.S3.Model;
using iskxpress_api.Repositories;
using System.Net;

namespace iskxpress_api.Tests;

public class S3RepositoryTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<S3Repository>> _mockLogger;
    private readonly S3Repository _repository;

    public S3RepositoryTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<S3Repository>>();

        // Setup configuration mocks
        _mockConfiguration.Setup(c => c["AWS:S3:BucketName"]).Returns("test-bucket");
        _mockConfiguration.Setup(c => c["AWS:Region"]).Returns("us-east-1");

        _repository = new S3Repository(_mockS3Client.Object, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullBucketName_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AWS:S3:BucketName"]).Returns((string?)null);

        // Act & Assert
        var action = () => new S3Repository(_mockS3Client.Object, mockConfig.Object, _mockLogger.Object);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("AWS S3 bucket name not configured");
    }

    [Fact]
    public async Task UploadFileAsync_ValidInput_ReturnsPublicUrl()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";
        var contentType = "image/jpeg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var putObjectResponse = new PutObjectResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        };

        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(putObjectResponse);

        // Act
        var result = await _repository.UploadFileAsync(objectKey, fileStream, contentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("https://s3.us-east-1.amazonaws.com/test-bucket/user_avatars/123.jpg");

        _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(req =>
            req.BucketName == "test-bucket" &&
            req.Key == "user_avatars/123.jpg" &&
            req.ContentType == contentType
        ), default), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithLeadingSlash_TrimsSlashAndUploads()
    {
        // Arrange
        var objectKey = "/user_avatars/123.jpg";
        var contentType = "image/jpeg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var putObjectResponse = new PutObjectResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        };

        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(putObjectResponse);

        // Act
        var result = await _repository.UploadFileAsync(objectKey, fileStream, contentType);

        // Assert
        result.Should().Be("https://s3.us-east-1.amazonaws.com/test-bucket/user_avatars/123.jpg");

        _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(req =>
            req.Key == "user_avatars/123.jpg"
        ), default), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_S3ReturnsError_ThrowsException()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";
        var contentType = "image/jpeg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var putObjectResponse = new PutObjectResponse
        {
            HttpStatusCode = HttpStatusCode.InternalServerError
        };

        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(putObjectResponse);

        // Act & Assert
        var action = async () => await _repository.UploadFileAsync(objectKey, fileStream, contentType);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to upload file to S3. Status code: InternalServerError");
    }

    [Fact]
    public async Task UploadFileAsync_AmazonS3Exception_ThrowsExceptionWithMessage()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";
        var contentType = "image/jpeg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ThrowsAsync(new AmazonS3Exception("Access denied"));

        // Act & Assert
        var action = async () => await _repository.UploadFileAsync(objectKey, fileStream, contentType);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("AWS S3 error: Access denied");
    }

    [Fact]
    public async Task RemoveFileAsync_ValidObjectKey_ReturnsTrue()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";

        var deleteObjectResponse = new DeleteObjectResponse
        {
            HttpStatusCode = HttpStatusCode.NoContent
        };

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ReturnsAsync(deleteObjectResponse);

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeTrue();

        _mockS3Client.Verify(s3 => s3.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req =>
            req.BucketName == "test-bucket" &&
            req.Key == "user_avatars/123.jpg"
        ), default), Times.Once);
    }

    [Fact]
    public async Task RemoveFileAsync_WithLeadingSlash_TrimsSlashAndDeletes()
    {
        // Arrange
        var objectKey = "/user_avatars/123.jpg";

        var deleteObjectResponse = new DeleteObjectResponse
        {
            HttpStatusCode = HttpStatusCode.NoContent
        };

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ReturnsAsync(deleteObjectResponse);

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeTrue();

        _mockS3Client.Verify(s3 => s3.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req =>
            req.Key == "user_avatars/123.jpg"
        ), default), Times.Once);
    }

    [Fact]
    public async Task RemoveFileAsync_FileNotExists_ReturnsTrue()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";

        var exception = new AmazonS3Exception("The specified key does not exist.");
        exception.ErrorCode = "NoSuchKey";

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ThrowsAsync(exception);

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeTrue(); // Should return true for non-existent files
    }

    [Fact]
    public async Task RemoveFileAsync_S3Error_ReturnsFalse()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";

        var deleteObjectResponse = new DeleteObjectResponse
        {
            HttpStatusCode = HttpStatusCode.InternalServerError
        };

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ReturnsAsync(deleteObjectResponse);

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFileAsync_AmazonS3Exception_ReturnsFalse()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ThrowsAsync(new AmazonS3Exception("Access denied"));

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFileAsync_UnexpectedException_ReturnsFalse()
    {
        // Arrange
        var objectKey = "user_avatars/123.jpg";

        _mockS3Client.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _repository.RemoveFileAsync(objectKey);

        // Assert
        result.Should().BeFalse();
    }
} 