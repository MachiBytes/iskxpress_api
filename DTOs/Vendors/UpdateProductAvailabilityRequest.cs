using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Vendors;

public class UpdateProductAvailabilityRequest
{
    [Required]
    public ProductAvailability Availability { get; set; }
} 