using iskxpress_api.DTOs.Orders;
using iskxpress_api.Models;

namespace iskxpress_api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<MultiOrderResponse> CreateMultiOrderAsync(int userId, CreateOrderRequest request);
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId, OrderStatus? status = null);
    Task<IEnumerable<OrderResponse>> GetStallOrdersAsync(int stallId, OrderStatus? status = null);
    Task<OrderResponse?> GetOrderAsync(int orderId);
    Task<OrderResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    Task<OrderConfirmationResponse> ConfirmOrderDeliveryAsync(int orderId);
    Task ProcessAutoConfirmationsAsync();
} 