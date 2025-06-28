using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for delivery operations
/// </summary>
public interface IDeliveryRepository : IGenericRepository<Delivery>
{
    /// <summary>
    /// Gets deliveries by order ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Collection of deliveries for the order</returns>
    Task<IEnumerable<Delivery>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets deliveries by delivery partner ID
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of deliveries for the partner</returns>
    Task<IEnumerable<Delivery>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId);

    /// <summary>
    /// Gets deliveries by status
    /// </summary>
    /// <param name="status">The delivery status</param>
    /// <returns>Collection of deliveries with the specified status</returns>
    Task<IEnumerable<Delivery>> GetByStatusAsync(DeliveryStatus status);

    /// <summary>
    /// Gets deliveries with their related order and delivery partner information
    /// </summary>
    /// <returns>Collection of deliveries with navigation properties loaded</returns>
    Task<IEnumerable<Delivery>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a delivery by ID with related order and delivery partner information
    /// </summary>
    /// <param name="id">The delivery ID</param>
    /// <returns>The delivery with navigation properties loaded, or null if not found</returns>
    Task<Delivery?> GetByIdWithDetailsAsync(int id);
} 