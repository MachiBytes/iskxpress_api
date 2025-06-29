using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class OrderItemRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly OrderItemRepository _repository;
    private readonly SqliteConnection _connection;

    public OrderItemRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new OrderItemRepository(_context);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnOrderItemsForOrder()
    {
        // Arrange
        var user = new User { Email = "user@example.com", Name = "User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category" };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.StallSections.AddAsync(section);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var orderItem = new OrderItem 
        { 
            OrderId = order.Id, 
            ProductId = product.Id, 
            Quantity = 2, 
            PriceEach = 15.00m
        };
        await _context.OrderItems.AddAsync(orderItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOrderIdAsync(order.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantity.Should().Be(2);
        result.First().PriceEach.Should().Be(15.00m);
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnOrderItemsForProduct()
    {
        // Arrange
        var user = new User { Email = "user@example.com", Name = "User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category" };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.StallSections.AddAsync(section);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var orderItem = new OrderItem 
        { 
            OrderId = order.Id, 
            ProductId = product.Id, 
            Quantity = 2, 
            PriceEach = 15.00m
        };
        await _context.OrderItems.AddAsync(orderItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProductIdAsync(product.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnOrderItemWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var user = new User { Email = "user@example.com", Name = "User", Role = UserRole.User };
        _context.Users.AddRange(vendor, user);
        await _context.SaveChangesAsync();
        
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();
        
        var category = new Category { Name = "Test Category" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();
        
        var product = new Product { Name = "Test Product", BasePrice = 10.00m, CategoryId = category.Id, SectionId = section.Id, StallId = stall.Id };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        var order = new Order { UserId = user.Id, StallId = stall.Id, Status = OrderStatus.Pending, DeliveryAddress = "Test Address" };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        var orderItem = new OrderItem { OrderId = order.Id, ProductId = product.Id, Quantity = 2, PriceEach = 10.00m };
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(orderItem.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderItem.Id);
        result.Order.Should().NotBeNull();
        result.Product.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddOrderItem()
    {
        // Arrange
        var user = new User { Email = "user@example.com", Name = "User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category" };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.StallSections.AddAsync(section);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var orderItem = new OrderItem 
        { 
            OrderId = order.Id, 
            ProductId = product.Id, 
            Quantity = 2, 
            PriceEach = 15.00m
        };

        // Act
        var result = await _repository.AddAsync(orderItem);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Quantity.Should().Be(2);
        result.PriceEach.Should().Be(15.00m);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 