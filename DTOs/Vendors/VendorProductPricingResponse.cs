using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Vendors;

public class VendorProductPricingResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal CalculatedMarkupPrice { get; set; }
    public decimal MarkupAmount { get; set; }
    public decimal MarkupPercentage { get; set; } = 10.0m;
    public ProductAvailability Availability { get; set; }
    public string AvailabilityText { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
} 