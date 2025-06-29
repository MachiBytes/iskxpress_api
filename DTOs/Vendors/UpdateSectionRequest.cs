using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Vendors;

public class UpdateSectionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
} 