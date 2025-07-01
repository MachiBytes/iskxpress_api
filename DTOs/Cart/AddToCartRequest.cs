using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Cart;

public class AddToCartRequest
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; } = 1;
} 