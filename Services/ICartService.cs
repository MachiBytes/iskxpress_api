using iskxpress_api.DTOs.Cart;

namespace iskxpress_api.Services;

public interface ICartService
{
    /// <summary>
    /// Gets all cart items for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of cart item responses</returns>
    Task<IEnumerable<CartItemResponse>> GetUserCartAsync(int userId);
    
    /// <summary>
    /// Adds a product to the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The add to cart request</param>
    /// <returns>The updated cart item response</returns>
    Task<CartItemResponse> AddToCartAsync(int userId, AddToCartRequest request);
    
    /// <summary>
    /// Updates the quantity of a cart item
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cartItemId">The cart item ID</param>
    /// <param name="request">The update quantity request</param>
    /// <returns>The updated cart item response, or null if quantity is 0 (item removed)</returns>
    Task<CartItemResponse?> UpdateCartItemQuantityAsync(int userId, int cartItemId, UpdateCartItemQuantityRequest request);
    
    /// <summary>
    /// Removes a product from the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cartItemId">The cart item ID</param>
    /// <returns>True if the item was removed, false otherwise</returns>
    Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
    
    /// <summary>
    /// Clears all items from the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if the cart was cleared, false otherwise</returns>
    Task<bool> ClearCartAsync(int userId);
} 