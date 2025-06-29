using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Data transfer object for syncing a user from Firebase
/// </summary>
public class FirebaseUserSyncRequest
{
    /// <summary>
    /// User's full name from Firebase
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address from Firebase
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Authentication provider used by the user (google, microsoft)
    /// </summary>
    [Required(ErrorMessage = "Auth provider is required")]
    [StringLength(50, ErrorMessage = "Auth provider cannot exceed 50 characters")]
    public string AuthProvider { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's profile picture from Firebase (optional)
    /// This will be used to create a FileRecord if provided
    /// </summary>
    [StringLength(500, ErrorMessage = "Picture URL cannot exceed 500 characters")]
    public string? PictureURL { get; set; }

    /// <summary>
    /// Whether the user's email is verified in Firebase
    /// </summary>
    public bool Verified { get; set; } = true;
} 