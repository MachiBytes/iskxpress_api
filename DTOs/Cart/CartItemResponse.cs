using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Cart;

public class CartItemResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int StallId { get; set; }
    
    // Product details
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductBasePrice { get; set; }
    public decimal ProductPriceWithMarkup { get; set; }
    public decimal ProductPremiumUserPrice { get; set; }
    public ProductAvailability ProductAvailability { get; set; }
    public string? ProductPictureUrl { get; set; }
    
    // Stall details
    public string StallName { get; set; } = string.Empty;
    public string StallShortDescription { get; set; } = string.Empty;
    public string? StallPictureUrl { get; set; }
    public string VendorName { get; set; } = string.Empty;
    
    // Calculated properties
    public decimal TotalPrice => ProductPriceWithMarkup * Quantity;
} 