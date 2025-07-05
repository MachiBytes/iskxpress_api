using iskxpress_api.DTOs.Delivery;
using iskxpress_api.DTOs.Orders;
using iskxpress_api.Models;

namespace iskxpress_api.Services;

public interface IDeliveryService
{
    /// <summary>
    /// Creates a delivery request for an order that needs delivery
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>The created delivery request</returns>
    Task<DeliveryRequestResponse> CreateDeliveryRequestAsync(int orderId);

    /// <summary>
    /// Assigns a delivery request to a delivery partner
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>The updated delivery request</returns>
    Task<DeliveryRequestResponse> AssignDeliveryRequestAsync(int requestId, int deliveryPartnerId);

    /// <summary>
    /// Gets all available delivery requests (pending assignment)
    /// </summary>
    /// <returns>Collection of pending delivery requests</returns>
    Task<IEnumerable<DeliveryRequestResponse>> GetAvailableDeliveryRequestsAsync();

    /// <summary>
    /// Gets delivery requests assigned to a specific delivery partner
    /// </summary>
    /// <param name="deliveryPartnerId">The delivery partner ID</param>
    /// <returns>Collection of delivery requests assigned to the partner</returns>
    Task<IEnumerable<DeliveryRequestResponse>> GetDeliveryPartnerRequestsAsync(int deliveryPartnerId);

    /// <summary>
    /// Gets a specific delivery request by ID
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <returns>The delivery request details</returns>
    Task<DeliveryRequestResponse?> GetDeliveryRequestAsync(int requestId);

    /// <summary>
    /// Updates the status of a delivery request
    /// </summary>
    /// <param name="requestId">The delivery request ID</param>
    /// <param name="status">The new status</param>
    /// <returns>The updated delivery request</returns>
    Task<DeliveryRequestResponse> UpdateDeliveryRequestStatusAsync(int requestId, DeliveryRequestStatus status);
} 