using iskxpress_api.DTOs.Cart;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Services;

public class CartService : ICartService
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public CartService(
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository,
        IUserRepository userRepository)
    {
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CartItemResponse>> GetUserCartAsync(int userId)
    {
        var cartItems = await _cartItemRepository.GetByUserIdAsync(userId);
        var user = await _userRepository.GetByIdAsync(userId);
        return cartItems.Select(ci => MapToCartItemResponse(ci, user));
    }

    public async Task<CartItemResponse> AddToCartAsync(int userId, AddToCartRequest request)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Validate product exists and is available
        var product = await _productRepository.GetByIdWithDetailsAsync(request.ProductId);
        if (product == null)
            throw new ArgumentException("Product not found", nameof(request.ProductId));

        if (product.Availability != ProductAvailability.Available)
            throw new InvalidOperationException("Product is not available");

        // Check if product already exists in user's cart
        var existingCartItem = await _cartItemRepository.GetByUserIdAsync(userId);
        var existingItem = existingCartItem.FirstOrDefault(ci => ci.ProductId == request.ProductId);

        CartItem cartItem;

        if (existingItem != null)
        {
            // Update existing cart item quantity
            existingItem.Quantity += request.Quantity;
            if (existingItem.Quantity > 100)
                existingItem.Quantity = 100;

            await _cartItemRepository.UpdateAsync(existingItem);
            cartItem = existingItem;
        }
        else
        {
            // Create new cart item
            cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                StallId = product.StallId
            };

            await _cartItemRepository.AddAsync(cartItem);
        }

        // Get the updated cart item with details
        var updatedCartItem = await _cartItemRepository.GetByIdWithDetailsAsync(cartItem.Id);
        return MapToCartItemResponse(updatedCartItem!, user);
    }

    public async Task<CartItemResponse?> UpdateCartItemQuantityAsync(int userId, int cartItemId, UpdateCartItemQuantityRequest request)
    {
        // Get the cart item and validate ownership
        var cartItem = await _cartItemRepository.GetByIdWithDetailsAsync(cartItemId);
        if (cartItem == null || cartItem.UserId != userId)
            throw new ArgumentException("Cart item not found or does not belong to user");

        if (request.Quantity == 0)
        {
            // Remove the item if quantity is 0
            await _cartItemRepository.DeleteAsync(cartItemId);
            return null;
        }

        // Update quantity
        cartItem.Quantity = request.Quantity;
        await _cartItemRepository.UpdateAsync(cartItem);

        // Get the updated cart item with details
        var updatedCartItem = await _cartItemRepository.GetByIdWithDetailsAsync(cartItemId);
        var user = await _userRepository.GetByIdAsync(userId);
        return MapToCartItemResponse(updatedCartItem!, user);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
    {
        // Get the cart item and validate ownership
        var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
        if (cartItem == null || cartItem.UserId != userId)
            return false;

        await _cartItemRepository.DeleteAsync(cartItemId);
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        return await _cartItemRepository.ClearCartAsync(userId);
    }

    private static CartItemResponse MapToCartItemResponse(CartItem cartItem, User? user)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id,
            UserId = cartItem.UserId,
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity,
            StallId = cartItem.StallId,
            
            // Product details
            ProductName = cartItem.Product.Name,
            ProductBasePrice = cartItem.Product.BasePrice,
            ProductPriceWithMarkup = cartItem.Product.PriceWithMarkup,
            ProductPremiumUserPrice = cartItem.Product.PremiumUserPrice,
            ProductAvailability = cartItem.Product.Availability,
            ProductPictureUrl = cartItem.Product.Picture?.ObjectUrl,
            
            // Stall details
            StallName = cartItem.Stall.Name,
            StallShortDescription = cartItem.Stall.ShortDescription,
            StallPictureUrl = cartItem.Stall.Picture?.ObjectUrl,
            VendorName = cartItem.Stall.Vendor.Name
        };
    }
} 