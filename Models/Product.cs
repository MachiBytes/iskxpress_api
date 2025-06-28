using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Picture { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceWithMarkup { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceWithDelivery { get; set; }

    [Required]
    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    [Required]
    [ForeignKey("Section")]
    public int SectionId { get; set; }

    [Required]
    [ForeignKey("Stall")]
    public int StallId { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual StallSection Section { get; set; } = null!;
    public virtual Stall Stall { get; set; } = null!;
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
} 