using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IOrderItemRepository : IGenericRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
} 