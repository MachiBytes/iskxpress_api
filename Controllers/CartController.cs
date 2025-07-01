using Microsoft.AspNetCore.Mvc;
using iskxpress_api.DTOs.Cart;
using iskxpress_api.Services;

namespace iskxpress_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Gets all cart items for the current user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of cart items</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetUserCart(int userId)
    {
        try
        {
            var cartItems = await _cartService.GetUserCartAsync(userId);
            return Ok(cartItems);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds a product to the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The add to cart request</param>
    /// <returns>The updated cart item</returns>
    [HttpPost("user/{userId}/add")]
    public async Task<ActionResult<CartItemResponse>> AddToCart(int userId, [FromBody] AddToCartRequest request)
    {
        try
        {
            var cartItem = await _cartService.AddToCartAsync(userId, request);
            return Ok(cartItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates the quantity of a cart item
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cartItemId">The cart item ID</param>
    /// <param name="request">The update quantity request</param>
    /// <returns>The updated cart item, or null if quantity is 0 (item removed)</returns>
    [HttpPut("user/{userId}/items/{cartItemId}/quantity")]
    public async Task<ActionResult<CartItemResponse?>> UpdateCartItemQuantity(int userId, int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
    {
        try
        {
            var cartItem = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, request);
            
            if (cartItem == null)
            {
                // Item was removed (quantity set to 0)
                return Ok(new { message = "Cart item removed successfully" });
            }
            
            return Ok(cartItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Removes a product from the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cartItemId">The cart item ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("user/{userId}/items/{cartItemId}")]
    public async Task<ActionResult> RemoveFromCart(int userId, int cartItemId)
    {
        var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
        
        if (!success)
        {
            return NotFound("Cart item not found or does not belong to user");
        }
        
        return Ok(new { message = "Cart item removed successfully" });
    }

    /// <summary>
    /// Clears all items from the user's cart
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("user/{userId}/clear")]
    public async Task<ActionResult> ClearCart(int userId)
    {
        var success = await _cartService.ClearCartAsync(userId);
        
        if (!success)
        {
            return BadRequest("Failed to clear cart");
        }
        
        return Ok(new { message = "Cart cleared successfully" });
    }
} 