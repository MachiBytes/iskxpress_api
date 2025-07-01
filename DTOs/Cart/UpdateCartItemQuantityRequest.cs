using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Cart;

public class UpdateCartItemQuantityRequest
{
    [Required]
    [Range(0, 100, ErrorMessage = "Quantity must be between 0 and 100")]
    public int Quantity { get; set; }
} 