using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.Models;

/// <summary>
/// Represents a file stored in the system
/// </summary>
public class FileRecord
{
    /// <summary>
    /// Unique identifier for the file
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of file (UserAvatar, StallAvatar, ProductImage)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public FileType Type { get; set; }

    /// <summary>
    /// S3 object key/path for the file
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Public URL to access the file
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string ObjectUrl { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID to the entity this file belongs to (UserId, StallId, ProductId)
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Original filename when uploaded
    /// </summary>
    [MaxLength(255)]
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// MIME type of the file
    /// </summary>
    [MaxLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Date and time when the file was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the file was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of file types supported by the system
/// </summary>
public enum FileType
{
    UserAvatar,
    StallAvatar,
    ProductImage
} 