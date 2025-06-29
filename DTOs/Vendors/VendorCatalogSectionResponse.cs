namespace iskxpress_api.DTOs.Vendors;

public class VendorCatalogSectionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public List<ProductResponse> Products { get; set; } = new List<ProductResponse>();
} 