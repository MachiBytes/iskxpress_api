using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.DTOs.Orders;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services;
using Xunit;

namespace iskxpress_api.Tests;

public class OrderServiceTests
{
    private readonly IskExpressDbContext _context;
    private readonly OrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStallRepository _stallRepository;
    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IskExpressDbContext(options);
        _orderRepository = new OrderRepository(_context);
        _cartItemRepository = new CartItemRepository(_context);
        _productRepository = new ProductRepository(_context);
        _stallRepository = new StallRepository(_context);
        _orderService = new OrderService(_context, _orderRepository, _cartItemRepository, _productRepository, _stallRepository);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidCartItems_ShouldCreateOrder()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 2 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Debug: Verify stall was saved
        var savedStall = await _stallRepository.GetByIdAsync(1);
        Assert.NotNull(savedStall);
        Assert.Equal("Test Stall", savedStall.Name);

        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup,
            Notes = "Test order"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.StallId);
        Assert.Equal("Test Stall", result.StallName);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(FulfillmentMethod.Pickup, result.FulfillmentMethod);
        Assert.Equal(20.00m, result.TotalPrice); // 2 * 10.00
        Assert.Single(result.OrderItems);
        Assert.Equal("Test Product", result.OrderItems[0].ProductName);
        Assert.Equal(2, result.OrderItems[0].Quantity);
        Assert.Equal(10.00m, result.OrderItems[0].PriceEach);
        Assert.Equal(20.00m, result.OrderItems[0].TotalPrice);

        // Verify cart items were removed
        var remainingCartItems = await _cartItemRepository.GetByUserIdAsync(1);
        Assert.Empty(remainingCartItems);
    }

    [Fact]
    public async Task CreateOrderAsync_WithDeliveryMethod_ShouldRequireDeliveryAddress()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Delivery,
            Notes = "Test order"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(1, request));
        
        Assert.Contains("Delivery address is required", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidCartItemIds_ShouldThrowException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 999 }, // Non-existent cart item
            FulfillmentMethod = FulfillmentMethod.Pickup
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(1, request));
        
        Assert.Contains("do not belong to the user or do not exist", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_WithUnavailableProduct_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1,
            Availability = ProductAvailability.SoldOut // Unavailable
        };
        var cartItem = new CartItem 
        { 
            Id = 1, 
            UserId = 1, 
            ProductId = 1, 
            StallId = 1, 
            Quantity = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Debug: Verify stall was saved
        var savedStall = await _stallRepository.GetByIdAsync(1);
        Assert.NotNull(savedStall);
        Assert.Equal("Test Stall", savedStall.Name);

        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(1, request));
        
        Assert.Contains("no longer available", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_WithDeliveryMethod_ShouldUseDeliveryPricing()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 2 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Delivery,
            DeliveryAddress = "123 Test Street, Test City",
            Notes = "Test delivery order"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.StallId);
        Assert.Equal("Test Stall", result.StallName);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(FulfillmentMethod.Delivery, result.FulfillmentMethod);
        Assert.Equal("123 Test Street, Test City", result.DeliveryAddress);
        Assert.Equal(20.00m, result.TotalSellingPrice); // Total Selling Price (2 * 10.00 = 20.00)
        Assert.Equal(10.00m, result.DeliveryFee); // Delivery Fee (10.00)
        Assert.Equal(30.00m, result.TotalPrice); // Total Price (20.00 + 10.00 = 30.00)
        Assert.Single(result.OrderItems);
        Assert.Equal("Test Product", result.OrderItems[0].ProductName);
        Assert.Equal(2, result.OrderItems[0].Quantity);
        Assert.Equal(10.00m, result.OrderItems[0].PriceEach); // Should use markup price
        Assert.Equal(20.00m, result.OrderItems[0].TotalPrice); // 2 * 10.00 = 20.00 (selling price)

        // Verify cart items were removed
        var remainingCartItems = await _cartItemRepository.GetByUserIdAsync(1);
        Assert.Empty(remainingCartItems);
    }

    [Fact]
    public async Task CreateOrderAsync_WithPickupMethod_ShouldUseMarkupPricingWithNoDeliveryFee()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 2 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup,
            Notes = "Test pickup order"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.StallId);
        Assert.Equal("Test Stall", result.StallName);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(FulfillmentMethod.Pickup, result.FulfillmentMethod);
        Assert.Equal(20.00m, result.TotalSellingPrice); // Total Selling Price (2 * 10.00 = 20.00)
        Assert.Equal(0.00m, result.DeliveryFee); // Delivery Fee (0.00 for pickup)
        Assert.Equal(20.00m, result.TotalPrice); // Total Price (20.00 + 0.00 = 20.00)
        Assert.Single(result.OrderItems);
        Assert.Equal("Test Product", result.OrderItems[0].ProductName);
        Assert.Equal(2, result.OrderItems[0].Quantity);
        Assert.Equal(10.00m, result.OrderItems[0].PriceEach); // Should use markup price
        Assert.Equal(20.00m, result.OrderItems[0].TotalPrice); // 2 * 10.00 = 20.00 (selling price)

        // Verify cart items were removed
        var remainingCartItems = await _cartItemRepository.GetByUserIdAsync(1);
        Assert.Empty(remainingCartItems);
    }

    [Fact]
    public async Task AssignDeliveryPartnerAsync_WithValidOrder_ShouldAssignDeliveryPartner()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var deliveryPartner = new User { Id = 3, Name = "Test Delivery Partner", Email = "delivery@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Users.AddAsync(deliveryPartner);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Create order first
        var createRequest = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Delivery,
            DeliveryAddress = "123 Test Street",
            Notes = "Test order"
        };

        var createdOrder = await _orderService.CreateOrderAsync(1, createRequest);

        // Act
        var result = await _orderService.AssignDeliveryPartnerAsync(createdOrder.Id, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.DeliveryPartnerId);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task AssignDeliveryPartnerAsync_WithPickupOrder_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Create pickup order
        var createRequest = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup,
            Notes = "Test order"
        };

        var createdOrder = await _orderService.CreateOrderAsync(1, createRequest);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.AssignDeliveryPartnerAsync(createdOrder.Id, 3));
        
        Assert.Contains("Can only assign delivery partner to delivery orders", exception.Message);
    }

    [Fact]
    public async Task RejectOrderAsync_WithValidOrder_ShouldRejectOrder()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Create order first
        var createRequest = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup,
            Notes = "Test order"
        };

        var createdOrder = await _orderService.CreateOrderAsync(1, createRequest);

        // Act
        var result = await _orderService.RejectOrderAsync(createdOrder.Id, "Out of stock");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Rejected, result.Status);
        Assert.Equal("Out of stock", result.RejectionReason);
    }

    [Fact]
    public async Task RejectOrderAsync_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        var vendor = new User { Id = 2, Name = "Test Vendor", Email = "vendor@example.com" };
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = 1 };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            PriceWithMarkup = 10.00m,
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
            StallId = 1, 
            Quantity = 1 
        };

        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(vendor);
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.StallSections.AddAsync(section);
        await _context.Products.AddAsync(product);
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Create order first
        var createRequest = new CreateOrderRequest
        {
            CartItemIds = new List<int> { 1 },
            FulfillmentMethod = FulfillmentMethod.Pickup,
            Notes = "Test order"
        };

        var createdOrder = await _orderService.CreateOrderAsync(1, createRequest);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.RejectOrderAsync(createdOrder.Id, ""));
        
        Assert.Contains("Rejection reason is required", exception.Message);
    }
} 