using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public enum AuthProvider
{
    Google,
    Microsoft,
    Apple,
    Facebook
}

public enum UserRole
{
    User,
    Vendor,
    DeliveryPartner,
    Admin
}

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public bool Premium { get; set; } = false;

    [Required]
    public AuthProvider AuthProvider { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    [ForeignKey("ProfilePicture")]
    public int? ProfilePictureId { get; set; }

    // Navigation properties
    public virtual FileRecord? ProfilePicture { get; set; }
    public virtual Stall? Stall { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Order> DeliveryOrders { get; set; } = new List<Order>();
} 