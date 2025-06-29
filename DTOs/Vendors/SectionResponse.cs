namespace iskxpress_api.DTOs.Vendors;

public class SectionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 