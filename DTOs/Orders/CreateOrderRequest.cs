using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Orders;

public class CreateOrderRequest
{
    /// <summary>
    /// List of cart item IDs to checkout
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one cart item must be selected")]
    public List<int> CartItemIds { get; set; } = new List<int>();

    /// <summary>
    /// Fulfillment method for the order
    /// </summary>
    [Required]
    public FulfillmentMethod FulfillmentMethod { get; set; }

    /// <summary>
    /// Delivery address (required if fulfillment method is Delivery)
    /// </summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Additional notes for the order
    /// </summary>
    public string? Notes { get; set; }
} 