using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for order operations
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>
    /// Gets orders by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of orders for the user</returns>
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets orders by stall ID
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of orders for the stall</returns>
    Task<IEnumerable<Order>> GetByStallIdAsync(int stallId);

    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <returns>Collection of orders with the specified status</returns>
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);

    /// <summary>
    /// Gets orders by delivery partner ID
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of orders for the delivery partner</returns>
    Task<IEnumerable<Order>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId);

    /// <summary>
    /// Gets orders with their related information
    /// </summary>
    /// <returns>Collection of orders with navigation properties loaded</returns>
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets an order by ID with related information
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <returns>The order with navigation properties loaded, or null if not found</returns>
    Task<Order?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Gets orders by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of orders within the date range</returns>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, OrderStatus? status = null);
    Task<IEnumerable<Order>> GetStallOrdersAsync(int stallId, OrderStatus? status = null);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
} 