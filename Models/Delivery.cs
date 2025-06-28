using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

/// <summary>
/// Represents a delivery in the system
/// </summary>
public class Delivery
{
    /// <summary>
    /// Unique identifier for the delivery
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID of the order being delivered
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the order
    /// </summary>
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// ID of the delivery partner (user)
    /// </summary>
    [Required]
    public int DeliveryPartnerId { get; set; }

    /// <summary>
    /// Navigation property to the delivery partner
    /// </summary>
    [ForeignKey("DeliveryPartnerId")]
    public virtual User DeliveryPartner { get; set; } = null!;

    /// <summary>
    /// Location where the order is picked up from
    /// </summary>
    [Required]
    public string PickupLocation { get; set; } = string.Empty;

    /// <summary>
    /// Location where the order is delivered to
    /// </summary>
    [Required]
    public string DropoffLocation { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the delivery
    /// </summary>
    [Required]
    public DeliveryStatus DeliveryStatus { get; set; }
} 