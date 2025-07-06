using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services;
using iskxpress_api.DTOs.Cart;
using Xunit;

namespace iskxpress_api.Tests;

public class CartServiceTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly CartService _cartService;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public CartServiceTests()
    {
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IskExpressDbContext(options);
        _cartItemRepository = new CartItemRepository(_context);
        _productRepository = new ProductRepository(_context);
        _userRepository = new UserRepository(_context);
        _cartService = new CartService(_cartItemRepository, _productRepository, _userRepository);
    }

    [Fact]
    public async Task AddToCart_NewProduct_ShouldCreateNewCartItem()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com", AuthProvider = AuthProvider.Google, Role = UserRole.Vendor };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            BasePrice = 10.00m, 
            PriceWithMarkup = 10.50m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1,
            Availability = ProductAvailability.Available
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new AddToCartRequest { ProductId = 1, Quantity = 2 };

        // Act
        var result = await _cartService.AddToCartAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(1, result.StallId);
        Assert.Equal("Test Product", result.ProductName);
        Assert.Equal("Test Stall", result.StallName);
    }

    [Fact]
    public async Task AddToCart_ExistingProduct_ShouldIncreaseQuantity()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com", AuthProvider = AuthProvider.Google, Role = UserRole.Vendor };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            BasePrice = 10.00m, 
            PriceWithMarkup = 10.50m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1,
            Availability = ProductAvailability.Available
        };
        var existingCartItem = new CartItem 
        { 
            UserId = 1, 
            ProductId = 1, 
            Quantity = 3, 
            StallId = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(existingCartItem);
        await _context.SaveChangesAsync();

        var request = new AddToCartRequest { ProductId = 1, Quantity = 2 };

        // Act
        var result = await _cartService.AddToCartAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity); // 3 + 2
    }

    [Fact]
    public async Task UpdateCartItemQuantity_ValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com", AuthProvider = AuthProvider.Google, Role = UserRole.Vendor };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            BasePrice = 10.00m, 
            PriceWithMarkup = 10.50m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1,
            Availability = ProductAvailability.Available
        };
        var cartItem = new CartItem 
        { 
            Id = 1,
            UserId = 1, 
            ProductId = 1, 
            Quantity = 3, 
            StallId = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        var request = new UpdateCartItemQuantityRequest { Quantity = 5 };

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(1, 1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_ZeroQuantity_ShouldRemoveItem()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com", AuthProvider = AuthProvider.Google, Role = UserRole.Vendor };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            BasePrice = 10.00m, 
            PriceWithMarkup = 10.50m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1,
            Availability = ProductAvailability.Available
        };
        var cartItem = new CartItem 
        { 
            Id = 1,
            UserId = 1, 
            ProductId = 1, 
            Quantity = 3, 
            StallId = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        var request = new UpdateCartItemQuantityRequest { Quantity = 0 };

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(1, 1, request);

        // Assert
        Assert.Null(result);
        Assert.Empty(await _context.CartItems.ToListAsync());
    }

    [Fact]
    public async Task RemoveFromCart_ValidCartItem_ShouldRemoveItem()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var cartItem = new CartItem 
        { 
            Id = 1,
            UserId = 1, 
            ProductId = 1, 
            Quantity = 3, 
            StallId = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.RemoveFromCartAsync(1, 1);

        // Assert
        Assert.True(result);
        Assert.Empty(await _context.CartItems.ToListAsync());
    }

    [Fact]
    public async Task RemoveFromCart_InvalidCartItem_ShouldReturnFalse()
    {
        // Act
        var result = await _cartService.RemoveFromCartAsync(1, 999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ClearCart_ValidUser_ShouldClearAllItems()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", AuthProvider = AuthProvider.Google };
        var cartItems = new List<CartItem>
        {
            new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 3, StallId = 1 },
            new CartItem { Id = 2, UserId = 1, ProductId = 2, Quantity = 1, StallId = 1 },
            new CartItem { Id = 3, UserId = 2, ProductId = 1, Quantity = 2, StallId = 1 } // Different user
        };

        await _context.Users.AddAsync(user);
        await _context.CartItems.AddRangeAsync(cartItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.ClearCartAsync(1);

        // Assert
        Assert.True(result);
        var remainingItems = await _context.CartItems.Where(ci => ci.UserId == 1).ToListAsync();
        Assert.Empty(remainingItems);
        
        // Other user's items should remain
        var otherUserItems = await _context.CartItems.Where(ci => ci.UserId == 2).ToListAsync();
        Assert.Single(otherUserItems);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 