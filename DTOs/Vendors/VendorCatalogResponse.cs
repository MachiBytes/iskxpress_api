namespace iskxpress_api.DTOs.Vendors;

public class VendorCatalogResponse
{
    public int VendorId { get; set; }
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public int TotalSections { get; set; }
    public int TotalProducts { get; set; }
    public List<VendorCatalogSectionResponse> Sections { get; set; } = new List<VendorCatalogSectionResponse>();
} 