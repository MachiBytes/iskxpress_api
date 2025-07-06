namespace iskxpress_api.DTOs.Vendors;

public class PendingFeesResponse
{
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public decimal PendingFees { get; set; }
    public DateTime LastUpdated { get; set; }
} 