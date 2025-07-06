using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Vendors;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? PictureId { get; set; }
    public string? PictureUrl { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CalculatedMarkupPrice { get; set; }
    public decimal PriceWithMarkup { get; set; }
    public ProductAvailability Availability { get; set; }
    public string AvailabilityText { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 