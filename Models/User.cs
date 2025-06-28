using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool Verified { get; set; }

    [Required]
    public AuthProvider AuthProvider { get; set; }

    [Required]
    public UserRole Role { get; set; }

    public string? Picture { get; set; }

    // Navigation properties
    public virtual ICollection<Stall> Stalls { get; set; } = new List<Stall>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Order> DeliveryOrders { get; set; } = new List<Order>();
}

public enum AuthProvider
{
    Google,
    Microsoft
}

public enum UserRole
{
    User,
    Vendor
} 