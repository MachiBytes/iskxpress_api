using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Vendors;

/// <summary>
/// Data transfer object for creating a new stall section
/// </summary>
public class CreateSectionRequest
{
    /// <summary>
    /// Section name (e.g., "Appetizers", "Main Courses", "Desserts")
    /// </summary>
    [Required(ErrorMessage = "Section name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Section name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
} 