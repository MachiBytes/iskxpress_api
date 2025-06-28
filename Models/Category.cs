using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [ForeignKey("Vendor")]
    public int VendorId { get; set; }

    // Navigation properties
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
} 