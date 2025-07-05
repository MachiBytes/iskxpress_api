using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

/// <summary>
/// Represents a user confirmation for a delivered order
/// </summary>
public class OrderConfirmation
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID of the order that needs confirmation
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the order
    /// </summary>
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// When the confirmation request was created (when order status was set to ToReceive)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Deadline for user confirmation (5 minutes from creation)
    /// </summary>
    [Required]
    public DateTime ConfirmationDeadline { get; set; }

    /// <summary>
    /// Whether the user has confirmed the delivery
    /// </summary>
    public bool IsConfirmed { get; set; } = false;

    /// <summary>
    /// When the user confirmed the delivery
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Whether the confirmation was automatically approved after deadline
    /// </summary>
    public bool IsAutoConfirmed { get; set; } = false;

    /// <summary>
    /// When the confirmation was automatically approved
    /// </summary>
    public DateTime? AutoConfirmedAt { get; set; }
} 