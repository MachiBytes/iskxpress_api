using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class FileRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly Mock<IS3Repository> _mockS3Repository;
    private readonly Mock<ILogger<FileRepository>> _mockLogger;
    private readonly FileRepository _repository;
    private readonly SqliteConnection _connection;

    public FileRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();

        _mockS3Repository = new Mock<IS3Repository>();
        _mockLogger = new Mock<ILogger<FileRepository>>();
        
        _repository = new FileRepository(_context, _mockS3Repository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAsync_ValidInput_ReturnsFileRecord()
    {
        // Arrange
        var fileType = FileType.UserAvatar;
        var entityId = 123;
        var contentType = "image/jpeg";
        var originalFileName = "avatar.jpg";
        var fileExtension = "jpg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var expectedUrl = "https://bucket.s3.region.amazonaws.com/user_avatars/123.jpg";

        _mockS3Repository.Setup(s3 => s3.UploadFileAsync(
            "user_avatars/123.jpg", 
            It.IsAny<Stream>(), 
            contentType))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _repository.UploadFileAsync(fileType, entityId, fileStream, contentType, originalFileName, fileExtension);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Type.Should().Be(FileType.UserAvatar);
        result.ObjectKey.Should().Be("user_avatars/123.jpg");
        result.ObjectUrl.Should().Be(expectedUrl);
        result.EntityId.Should().Be(entityId);
        result.OriginalFileName.Should().Be(originalFileName);
        result.ContentType.Should().Be(contentType);
        result.FileSizeBytes.Should().Be(5);

        // Verify S3 upload was called
        _mockS3Repository.Verify(s3 => s3.UploadFileAsync(
            "user_avatars/123.jpg", 
            It.IsAny<Stream>(), 
            contentType), Times.Once);

        // Verify record was saved to database
        var savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == result.Id);
        savedFile.Should().NotBeNull();
        savedFile!.ObjectKey.Should().Be("user_avatars/123.jpg");
    }

    [Fact]
    public async Task UploadFileAsync_ReplacesExistingFile_RemovesOldFileFirst()
    {
        // Arrange
        var fileType = FileType.UserAvatar;
        var entityId = 123;

        // Create existing file
        var existingFile = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = entityId,
            ObjectKey = "user_avatars/123.png",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.png",
            ContentType = "image/png"
        };
        _context.Files.Add(existingFile);
        await _context.SaveChangesAsync();

        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var expectedUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg";

        _mockS3Repository.Setup(s3 => s3.RemoveFileAsync("user_avatars/123.png"))
            .ReturnsAsync(true);
        _mockS3Repository.Setup(s3 => s3.UploadFileAsync(
            "user_avatars/123.jpg", 
            It.IsAny<Stream>(), 
            "image/jpeg"))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _repository.UploadFileAsync(fileType, entityId, fileStream, "image/jpeg", "avatar.jpg", "jpg");

        // Assert
        result.Should().NotBeNull();
        result.ObjectKey.Should().Be("user_avatars/123.jpg");

        // Verify old file was removed from S3
        _mockS3Repository.Verify(s3 => s3.RemoveFileAsync("user_avatars/123.png"), Times.Once);

        // Verify only new file exists in database
        var filesInDb = await _context.Files.Where(f => f.EntityId == entityId && f.Type == fileType).ToListAsync();
        filesInDb.Should().HaveCount(1);
        filesInDb[0].ObjectKey.Should().Be("user_avatars/123.jpg");
    }

    [Theory]
    [InlineData(FileType.UserAvatar, 123, "jpg", "user_avatars/123.jpg")]
    [InlineData(FileType.StallAvatar, 456, "png", "stall_avatars/456.png")]
    [InlineData(FileType.ProductImage, 789, "webp", "product_pictures/789.webp")]
    public async Task UploadFileAsync_GeneratesCorrectObjectKey(FileType fileType, int entityId, string extension, string expectedKey)
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var expectedUrl = $"https://s3.region.amazonaws.com/bucket/{expectedKey}";

        _mockS3Repository.Setup(s3 => s3.UploadFileAsync(expectedKey, It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _repository.UploadFileAsync(fileType, entityId, fileStream, "image/jpeg", "test.jpg", extension);

        // Assert
        result.ObjectKey.Should().Be(expectedKey);
        _mockS3Repository.Verify(s3 => s3.UploadFileAsync(expectedKey, It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFileAsync_ExistingFile_RemovesFromS3AndDatabase()
    {
        // Arrange
        var file = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg"
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        _mockS3Repository.Setup(s3 => s3.RemoveFileAsync("user_avatars/123.jpg"))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.RemoveFileAsync(file.Id);

        // Assert
        result.Should().BeTrue();

        // Verify S3 removal was called
        _mockS3Repository.Verify(s3 => s3.RemoveFileAsync("user_avatars/123.jpg"), Times.Once);

        // Verify file was removed from database
        var fileInDb = await _context.Files.FindAsync(file.Id);
        fileInDb.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFileAsync_NonExistentFile_ReturnsFalse()
    {
        // Act
        var result = await _repository.RemoveFileAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockS3Repository.Verify(s3 => s3.RemoveFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFileAsync_S3RemovalFails_StillRemovesFromDatabase()
    {
        // Arrange
        var file = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg"
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        _mockS3Repository.Setup(s3 => s3.RemoveFileAsync("user_avatars/123.jpg"))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.RemoveFileAsync(file.Id);

        // Assert
        result.Should().BeTrue(); // Should still return true as database removal succeeded

        // Verify file was removed from database even though S3 removal failed
        var fileInDb = await _context.Files.FindAsync(file.Id);
        fileInDb.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFileByEntityAsync_ExistingFiles_RemovesAllFiles()
    {
        // Arrange
        var file1 = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg"
        };
        _context.Files.Add(file1);
        await _context.SaveChangesAsync();

        // Note: Due to unique constraint (Type, EntityId), we can only have one file per entity per type
        // This test will verify removal of the single file

        _mockS3Repository.Setup(s3 => s3.RemoveFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.RemoveFileByEntityAsync(FileType.UserAvatar, 123);

        // Assert
        result.Should().BeTrue();

        // Verify file was removed from S3
        _mockS3Repository.Verify(s3 => s3.RemoveFileAsync("user_avatars/123.jpg"), Times.Once);

        // Verify file was removed from database
        var filesInDb = await _context.Files.Where(f => f.EntityId == 123 && f.Type == FileType.UserAvatar).ToListAsync();
        filesInDb.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveFileByEntityAsync_NoFiles_ReturnsTrue()
    {
        // Act
        var result = await _repository.RemoveFileByEntityAsync(FileType.UserAvatar, 999);

        // Assert
        result.Should().BeTrue();
        _mockS3Repository.Verify(s3 => s3.RemoveFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFileByIdAsync_ExistingFile_ReturnsFile()
    {
        // Arrange
        var file = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg"
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFileByIdAsync(file.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
        result.ObjectKey.Should().Be("user_avatars/123.jpg");
    }

    [Fact]
    public async Task GetFileByIdAsync_NonExistentFile_ReturnsNull()
    {
        // Act
        var result = await _repository.GetFileByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFilesByEntityAsync_ExistingFiles_ReturnsMatchingFiles()
    {
        // Arrange
        var file1 = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var file2 = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 456, // Different entity ID to avoid unique constraint violation
            ObjectKey = "user_avatars/456.png",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/456.png",
            CreatedAt = DateTime.UtcNow
        };
        var file3 = new FileRecord
        {
            Type = FileType.StallAvatar,
            EntityId = 123,
            ObjectKey = "stall_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/stall_avatars/123.jpg"
        };
        _context.Files.AddRange(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFilesByEntityAsync(FileType.UserAvatar, 123);

        // Assert
        result.Should().HaveCount(1); // Only one file for entity 123
        result.Should().OnlyContain(f => f.Type == FileType.UserAvatar && f.EntityId == 123);
        result[0].ObjectKey.Should().Be("user_avatars/123.jpg");
    }

    [Fact]
    public async Task GetFilesByTypeAsync_ExistingFiles_ReturnsMatchingFiles()
    {
        // Arrange
        var file1 = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg"
        };
        var file2 = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 456,
            ObjectKey = "user_avatars/456.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/456.jpg"
        };
        var file3 = new FileRecord
        {
            Type = FileType.StallAvatar,
            EntityId = 123,
            ObjectKey = "stall_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/stall_avatars/123.jpg"
        };
        _context.Files.AddRange(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFilesByTypeAsync(FileType.UserAvatar);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.Type == FileType.UserAvatar);
    }

    [Fact]
    public async Task UpdateFileAsync_ExistingFile_UpdatesFileAndTimestamp()
    {
        // Arrange
        var file = new FileRecord
        {
            Type = FileType.UserAvatar,
            EntityId = 123,
            ObjectKey = "user_avatars/123.jpg",
            ObjectUrl = "https://s3.region.amazonaws.com/bucket/user_avatars/123.jpg",
            OriginalFileName = "old.jpg",
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = file.UpdatedAt;

        // Act
        file.OriginalFileName = "new.jpg";
        var result = await _repository.UpdateFileAsync(file);

        // Assert
        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be("new.jpg");
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

        // Verify in database
        var updatedFile = await _context.Files.FindAsync(file.Id);
        updatedFile!.OriginalFileName.Should().Be("new.jpg");
        updatedFile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 