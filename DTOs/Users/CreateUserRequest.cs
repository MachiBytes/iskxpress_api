using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// User's full name
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Authentication provider used by the user
    /// Role will be automatically inferred: Microsoft = User, Google = Vendor
    /// </summary>
    [Required(ErrorMessage = "Auth provider is required")]
    public AuthProvider AuthProvider { get; set; }

    /// <summary>
    /// Profile picture file ID reference (optional)
    /// </summary>
    public int? ProfilePictureId { get; set; }
} 