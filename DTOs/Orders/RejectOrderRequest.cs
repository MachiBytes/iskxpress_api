using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Orders;

public class RejectOrderRequest
{
    /// <summary>
    /// The reason for rejecting the order
    /// </summary>
    [Required]
    public string RejectionReason { get; set; } = string.Empty;
} 