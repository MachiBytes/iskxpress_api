using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Orders;

public class OrderConfirmationResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ConfirmationDeadline { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool IsAutoConfirmed { get; set; }
    public DateTime? AutoConfirmedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ConfirmationDeadline && !IsConfirmed && !IsAutoConfirmed;
    public TimeSpan TimeRemaining => IsExpired ? TimeSpan.Zero : ConfirmationDeadline - DateTime.UtcNow;
} 