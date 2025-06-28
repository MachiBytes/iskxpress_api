using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Order>> GetByStallIdAsync(int stallId);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<IEnumerable<Order>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId);
    Task<Order?> GetWithItemsAsync(int orderId);
    Task<Order?> GetByVendorOrderIdAsync(string vendorOrderId);
} 