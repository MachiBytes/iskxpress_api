using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    /// <summary>
    /// The new status for the order
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; }
} 