using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for cart item operations
/// </summary>
public interface ICartItemRepository : IGenericRepository<CartItem>
{
    /// <summary>
    /// Gets cart items by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of cart items for the user</returns>
    Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets cart items by product ID
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Collection of cart items for the product</returns>
    Task<IEnumerable<CartItem>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets cart items by stall ID
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of cart items from the stall</returns>
    Task<IEnumerable<CartItem>> GetByStallIdAsync(int stallId);

    /// <summary>
    /// Gets cart items by user and stall
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of cart items for the user from the specific stall</returns>
    Task<IEnumerable<CartItem>> GetByUserAndStallAsync(int userId, int stallId);

    /// <summary>
    /// Gets cart items with their related information
    /// </summary>
    /// <returns>Collection of cart items with navigation properties loaded</returns>
    Task<IEnumerable<CartItem>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a cart item by ID with related information
    /// </summary>
    /// <param name="id">The cart item ID</param>
    /// <returns>The cart item with navigation properties loaded, or null if not found</returns>
    Task<CartItem?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Clears all cart items for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if items were cleared, false otherwise</returns>
    Task<bool> ClearCartAsync(int userId);
} 