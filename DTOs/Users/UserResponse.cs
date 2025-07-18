using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Data transfer object for user response data
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has premium status
    /// </summary>
    public bool Premium { get; set; }

    /// <summary>
    /// Authentication provider used by the user
    /// </summary>
    public AuthProvider AuthProvider { get; set; }

    /// <summary>
    /// String representation of the authentication provider
    /// </summary>
    public string AuthProviderString => AuthProvider.ToString();

    /// <summary>
    /// User's role in the system
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// String representation of the user's role
    /// </summary>
    public string RoleString => Role.ToString();

    /// <summary>
    /// Profile picture file ID reference (optional)
    /// </summary>
    public int? ProfilePictureId { get; set; }

    /// <summary>
    /// URL to user's profile picture (optional)
    /// </summary>
    public string? PictureUrl { get; set; }

    /// <summary>
    /// Date and time when the user was created (if available)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the user was last updated (if available)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
} 