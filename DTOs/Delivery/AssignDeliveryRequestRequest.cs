using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Delivery;

public class AssignDeliveryRequestRequest
{
    /// <summary>
    /// ID of the delivery partner who will handle the delivery
    /// </summary>
    [Required]
    public int DeliveryPartnerId { get; set; }
} 