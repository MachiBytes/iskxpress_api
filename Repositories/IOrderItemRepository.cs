using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for order item operations
/// </summary>
public interface IOrderItemRepository : IGenericRepository<OrderItem>
{
    /// <summary>
    /// Gets order items by order ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Collection of order items for the order</returns>
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets order items by product ID
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Collection of order items for the product</returns>
    Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets order items with their related information
    /// </summary>
    /// <returns>Collection of order items with navigation properties loaded</returns>
    Task<IEnumerable<OrderItem>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets an order item by ID with related information
    /// </summary>
    /// <param name="id">The order item ID</param>
    /// <returns>The order item with navigation properties loaded, or null if not found</returns>
    Task<OrderItem?> GetByIdWithDetailsAsync(int id);
} 