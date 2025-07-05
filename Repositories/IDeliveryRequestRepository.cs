using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for delivery request operations
/// </summary>
public interface IDeliveryRequestRepository : IGenericRepository<DeliveryRequest>
{
    /// <summary>
    /// Gets delivery requests by status
    /// </summary>
    /// <param name="status">The delivery request status</param>
    /// <returns>Collection of delivery requests with the specified status</returns>
    Task<IEnumerable<DeliveryRequest>> GetByStatusAsync(DeliveryRequestStatus status);

    /// <summary>
    /// Gets delivery requests by order ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Delivery request for the order, or null if not found</returns>
    Task<DeliveryRequest?> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets delivery requests assigned to a specific delivery partner
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of delivery requests assigned to the partner</returns>
    Task<IEnumerable<DeliveryRequest>> GetByDeliveryPartnerIdAsync(int deliveryPartnerId);

    /// <summary>
    /// Gets all pending delivery requests (available for assignment)
    /// </summary>
    /// <returns>Collection of pending delivery requests</returns>
    Task<IEnumerable<DeliveryRequest>> GetPendingRequestsAsync();

    /// <summary>
    /// Gets delivery requests with their related order and delivery partner information
    /// </summary>
    /// <returns>Collection of delivery requests with navigation properties loaded</returns>
    Task<IEnumerable<DeliveryRequest>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a delivery request by ID with related order and delivery partner information
    /// </summary>
    /// <param name="id">The delivery request ID</param>
    /// <returns>The delivery request with navigation properties loaded, or null if not found</returns>
    Task<DeliveryRequest?> GetByIdWithDetailsAsync(int id);
} 