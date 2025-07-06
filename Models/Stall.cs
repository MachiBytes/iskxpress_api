using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class Stall
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical store number that serves as the stall's physical address
    /// </summary>
    [Required]
    public int StallNumber { get; set; }

    public string ShortDescription { get; set; } = string.Empty;

    [ForeignKey("Picture")]
    public int? PictureId { get; set; }

    [Required]
    [ForeignKey("Vendor")]
    public int VendorId { get; set; }

    /// <summary>
    /// Indicates if delivery service is currently available
    /// </summary>
    public bool DeliveryAvailable { get; set; } = false;

    /// <summary>
    /// Accumulated commission fees from completed orders
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendingFees { get; set; } = 0.00m;

    // Navigation properties
    public virtual FileRecord? Picture { get; set; }
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<StallSection> StallSections { get; set; } = new List<StallSection>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
} 