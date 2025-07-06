using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Order")]
    public int OrderId { get; set; }

    [Required]
    [ForeignKey("Product")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceEach { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommissionFee { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
} 