using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class OrderRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly OrderRepository _repository;
    private readonly SqliteConnection _connection;

    public OrderRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new OrderRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.TotalPrice.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOrdersForUser()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order1 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };
        var order2 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER002",
            TotalPrice = 200.00m, 
            Status = OrderStatus.Accomplished,
            FulfillmentMethod = FulfillmentMethod.Pickup,
            CreatedAt = DateTime.Now.AddDays(-1),
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.TotalPrice == 100.00m);
        result.Should().Contain(o => o.TotalPrice == 200.00m);
    }

    [Fact]
    public async Task GetByStallIdAsync_ShouldReturnOrdersForStall()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order1 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };
        var order2 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER002",
            TotalPrice = 200.00m, 
            Status = OrderStatus.Accomplished,
            FulfillmentMethod = FulfillmentMethod.Pickup,
            CreatedAt = DateTime.Now.AddDays(-1),
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStallIdAsync(stall.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.TotalPrice == 100.00m);
        result.Should().Contain(o => o.TotalPrice == 200.00m);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOrdersWithStatus()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order1 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };
        var order2 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER002",
            TotalPrice = 200.00m, 
            Status = OrderStatus.Accomplished,
            FulfillmentMethod = FulfillmentMethod.Pickup,
            CreatedAt = DateTime.Now.AddDays(-1),
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(OrderStatus.Pending);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalPrice.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByDeliveryPartnerIdAsync_ShouldReturnOrdersForDeliveryPartner()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var deliveryPartner = new User { Email = "delivery@example.com", Name = "DeliveryPartner", Role = UserRole.User };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            DeliveryPartnerId = deliveryPartner.Id,
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall,
            DeliveryPartner = deliveryPartner
        };

        _context.Users.AddRange(user, vendor, deliveryPartner);
        _context.Stalls.Add(stall);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDeliveryPartnerIdAsync(deliveryPartner.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalPrice.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnOrdersInDateRange()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order1 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Today,
            User = user,
            Stall = stall
        };
        var order2 = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER002",
            TotalPrice = 200.00m, 
            Status = OrderStatus.Accomplished,
            FulfillmentMethod = FulfillmentMethod.Pickup,
            CreatedAt = DateTime.Today.AddDays(-10),
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(DateTime.Today.AddDays(-5), DateTime.Today.AddDays(1));

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalPrice.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnOrderWithNavigationProperties()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.Stall.Should().NotBeNull();
        result.User.Name.Should().Be("Test User");
        result.Stall.Name.Should().Be("Test Stall");
    }

    [Fact]
    public async Task AddAsync_ShouldAddOrder()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

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

        // Act
        var result = await _repository.AddAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.TotalPrice.Should().Be(100.00m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveOrder()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User", Role = UserRole.User };
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var order = new Order 
        { 
            UserId = user.Id, 
            StallId = stall.Id, 
            VendorOrderId = "ORDER001",
            TotalPrice = 100.00m, 
            Status = OrderStatus.Pending,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now,
            User = user,
            Stall = stall
        };

        _context.Users.AddRange(user, vendor);
        _context.Stalls.Add(stall);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(order.Id);

        // Assert
        result.Should().BeTrue();
        var deletedOrder = await _repository.GetByIdAsync(order.Id);
        deletedOrder.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 