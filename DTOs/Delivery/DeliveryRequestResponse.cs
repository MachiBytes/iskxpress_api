using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Delivery;

public class DeliveryRequestResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderSummaryResponse Order { get; set; } = new OrderSummaryResponse();
    public int? AssignedDeliveryPartnerId { get; set; }
    public UserSummaryResponse? AssignedDeliveryPartner { get; set; }
    public DeliveryRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderSummaryResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
} 