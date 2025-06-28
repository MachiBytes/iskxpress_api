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
    /// </summary>
    [Required(ErrorMessage = "Auth provider is required")]
    public AuthProvider AuthProvider { get; set; }

    /// <summary>
    /// User's role in the system
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }

    /// <summary>
    /// URL to user's profile picture (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Picture URL cannot exceed 500 characters")]
    public string? PictureURL { get; set; }
} 