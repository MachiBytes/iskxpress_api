using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Data transfer object for updating an existing user
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User's full name
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's profile picture (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Picture URL cannot exceed 500 characters")]
    public string? PictureURL { get; set; }
} 