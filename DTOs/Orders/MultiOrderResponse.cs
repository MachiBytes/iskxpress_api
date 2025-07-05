namespace iskxpress_api.DTOs.Orders;

public class MultiOrderResponse
{
    public List<OrderResponse> Orders { get; set; } = new List<OrderResponse>();
} 