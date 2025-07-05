using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for order confirmation operations
/// </summary>
public interface IOrderConfirmationRepository : IGenericRepository<OrderConfirmation>
{
    /// <summary>
    /// Gets order confirmation by order ID
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Order confirmation for the order, or null if not found</returns>
    Task<OrderConfirmation?> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets order confirmations that need auto-confirmation (past deadline)
    /// </summary>
    /// <returns>Collection of order confirmations that need auto-confirmation</returns>
    Task<IEnumerable<OrderConfirmation>> GetPendingAutoConfirmationsAsync();

    /// <summary>
    /// Gets order confirmations with their related order information
    /// </summary>
    /// <returns>Collection of order confirmations with navigation properties loaded</returns>
    Task<IEnumerable<OrderConfirmation>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets an order confirmation by ID with related order information
    /// </summary>
    /// <param name="id">The order confirmation ID</param>
    /// <returns>The order confirmation with navigation properties loaded, or null if not found</returns>
    Task<OrderConfirmation?> GetByIdWithDetailsAsync(int id);
} 