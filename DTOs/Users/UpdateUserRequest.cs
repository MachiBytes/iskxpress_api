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
    /// Profile picture file ID reference (optional)
    /// </summary>
    public int? ProfilePictureId { get; set; }
} 