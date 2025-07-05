using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

/// <summary>
/// Represents a delivery request that needs to be assigned to a delivery partner
/// </summary>
public class DeliveryRequest
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID of the order that needs delivery
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the order
    /// </summary>
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// ID of the delivery partner assigned to this request (nullable until assigned)
    /// </summary>
    public int? AssignedDeliveryPartnerId { get; set; }

    /// <summary>
    /// Navigation property to the assigned delivery partner
    /// </summary>
    [ForeignKey("AssignedDeliveryPartnerId")]
    public virtual User? AssignedDeliveryPartner { get; set; }

    /// <summary>
    /// Current status of the delivery request
    /// </summary>
    [Required]
    public DeliveryRequestStatus Status { get; set; } = DeliveryRequestStatus.Pending;

    /// <summary>
    /// When the delivery request was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the delivery request was assigned to a partner
    /// </summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>
    /// When the delivery request was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Enumeration for delivery request status
/// </summary>
public enum DeliveryRequestStatus
{
    /// <summary>
    /// Request is pending assignment to a delivery partner
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Request has been assigned to a delivery partner
    /// </summary>
    Assigned = 1,

    /// <summary>
    /// Request has been completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Request has been cancelled
    /// </summary>
    Cancelled = 3
} 