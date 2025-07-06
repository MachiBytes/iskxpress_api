using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Orders;

public class AssignDeliveryPartnerRequest
{
    /// <summary>
    /// The ID of the delivery partner to assign
    /// </summary>
    [Required]
    public int DeliveryPartnerId { get; set; }
} 