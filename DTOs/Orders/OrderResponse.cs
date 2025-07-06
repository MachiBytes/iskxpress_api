using iskxpress_api.Models;

namespace iskxpress_api.DTOs.Orders;

public class OrderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int StallId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public FulfillmentMethod FulfillmentMethod { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public int? DeliveryPartnerId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalCommissionFee { get; set; }
    public decimal DeliveryFee { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string? ProductPictureUrl { get; set; }
    public int Quantity { get; set; }
    public decimal PriceEach { get; set; }
    public decimal CommissionFee { get; set; }
    public decimal TotalPrice { get; set; }
} 