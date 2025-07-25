using iskxpress_api.DTOs.Orders;
using iskxpress_api.Models;

namespace iskxpress_api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<MultiOrderResponse> CreateMultiOrderAsync(int userId, CreateOrderRequest request);
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId, OrderStatus? status = null);
    Task<IEnumerable<OrderResponse>> GetStallOrdersAsync(int stallId, OrderStatus? status = null);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(bool? hasDeliveryPartner = null);
    Task<IEnumerable<OrderResponse>> GetStallOrdersWithDeliveryPartnerAsync(int stallId);
    Task<IEnumerable<OrderResponse>> GetDeliveryPartnerOrdersAsync(int deliveryPartnerId, bool? isFinished = null);
    Task<OrderResponse?> GetOrderAsync(int orderId);
    Task<OrderResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    Task<OrderResponse> AssignDeliveryPartnerAsync(int orderId, int deliveryPartnerId);
    Task<OrderResponse> RejectOrderAsync(int orderId, string rejectionReason);
} 