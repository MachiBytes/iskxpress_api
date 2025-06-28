using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class Stall
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string? Picture { get; set; }

    [Required]
    [ForeignKey("Vendor")]
    public int VendorId { get; set; }

    // Navigation properties
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<StallSection> StallSections { get; set; } = new List<StallSection>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
} 