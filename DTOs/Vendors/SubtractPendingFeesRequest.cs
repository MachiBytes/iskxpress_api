using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Vendors;

public class SubtractPendingFeesRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
} 