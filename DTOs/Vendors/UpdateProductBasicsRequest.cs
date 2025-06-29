using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Vendors;

public class UpdateProductBasicsRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int? PictureId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
    public decimal BasePrice { get; set; }

    [Required]
    public ProductAvailability Availability { get; set; }
} 