using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Vendors;

/// <summary>
/// Data transfer object for creating a new stall
/// </summary>
public class CreateStallRequest
{
    /// <summary>
    /// Stall name
    /// </summary>
    [Required(ErrorMessage = "Stall name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Stall name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the stall
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? ShortDescription { get; set; }
} 