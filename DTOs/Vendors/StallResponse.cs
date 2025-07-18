namespace iskxpress_api.DTOs.Vendors;

public class StallResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StallNumber { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public int? PictureId { get; set; }
    public string? PictureUrl { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public bool DeliveryAvailable { get; set; }
    public decimal PendingFees { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
} 