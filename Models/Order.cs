using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Stall")]
    public int StallId { get; set; }

    [Required]
    public string VendorOrderId { get; set; } = string.Empty;

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    public FulfillmentMethod FulfillmentMethod { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? Notes { get; set; }

    [ForeignKey("DeliveryPartner")]
    public int? DeliveryPartnerId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Stall Stall { get; set; } = null!;
    public virtual User? DeliveryPartner { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending,
    ToPrepare,
    ToDeliver,
    ToReceive,
    Accomplished
}

public enum FulfillmentMethod
{
    Pickup,
    Delivery
} 