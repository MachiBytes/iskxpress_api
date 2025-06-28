using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using Xunit;

namespace iskxpress_api.Tests;

public class DeliveryRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly DeliveryRepository _repository;
    private readonly SqliteConnection _connection;

    public DeliveryRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new DeliveryRepository(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    private async Task<(User user, User vendor, Stall stall)> CreateTestEntitiesAsync()
    {
        var user = new User
        {
            Name = "Test User",
            Email = "user@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.User,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        return (user, vendor, stall);
    }

    private async Task<Order> CreateTestOrderAsync(int userId, int stallId, string vendorOrderId = "ORDER123")
    {
        var order = new Order
        {
            UserId = userId,
            StallId = stallId,
            VendorOrderId = vendorOrderId,
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task AddAsync_ValidDelivery_ReturnsDeliveryWithId()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER123",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };

        // Act
        var result = await _repository.AddAsync(delivery);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.OrderId.Should().Be(order.Id);
        result.DeliveryPartnerId.Should().Be(user.Id);
        result.PickupLocation.Should().Be("Restaurant ABC");
        result.DropoffLocation.Should().Be("123 Main St");
        result.DeliveryStatus.Should().Be(DeliveryStatus.PickedUp);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDelivery_ReturnsDelivery()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER456",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.OutForDelivery
        };
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(delivery.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(delivery.Id);
        result.OrderId.Should().Be(order.Id);
        result.DeliveryPartnerId.Should().Be(user.Id);
        result.DeliveryStatus.Should().Be(DeliveryStatus.OutForDelivery);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingDelivery_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ExistingOrder_ReturnsDeliveries()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER789",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery1 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };

        var delivery2 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant XYZ",
            DropoffLocation = "456 Oak Ave",
            DeliveryStatus = DeliveryStatus.OutForDelivery
        };

        await _context.Deliveries.AddRangeAsync(delivery1, delivery2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOrderIdAsync(order.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Id == delivery1.Id);
        result.Should().Contain(d => d.Id == delivery2.Id);
    }

    [Fact]
    public async Task GetByDeliveryPartnerIdAsync_ExistingPartner_ReturnsDeliveries()
    {
        // Arrange
        var user1 = new User
        {
            Name = "Partner 1",
            Email = "partner1@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };

        var user2 = new User
        {
            Name = "Partner 2",
            Email = "partner2@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };

        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };

        await _context.Users.AddRangeAsync(user1, user2, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user1.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER101",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery1 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user1.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };

        var delivery2 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user2.Id,
            PickupLocation = "Restaurant XYZ",
            DropoffLocation = "456 Oak Ave",
            DeliveryStatus = DeliveryStatus.OutForDelivery
        };

        await _context.Deliveries.AddRangeAsync(delivery1, delivery2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDeliveryPartnerIdAsync(user1.Id);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(d => d.Id == delivery1.Id);
        result.Should().NotContain(d => d.Id == delivery2.Id);
    }

    [Fact]
    public async Task GetByStatusAsync_ExistingStatus_ReturnsDeliveries()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER202",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery1 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };

        var delivery2 = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant XYZ",
            DropoffLocation = "456 Oak Ave",
            DeliveryStatus = DeliveryStatus.OutForDelivery
        };

        await _context.Deliveries.AddRangeAsync(delivery1, delivery2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(DeliveryStatus.PickedUp);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(d => d.Id == delivery1.Id);
        result.Should().NotContain(d => d.Id == delivery2.Id);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_ReturnsDeliveriesWithNavigationProperties()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER303",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.Delivered
        };
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().HaveCount(1);
        var deliveryResult = result.First();
        deliveryResult.Order.Should().NotBeNull();
        deliveryResult.DeliveryPartner.Should().NotBeNull();
        deliveryResult.Order.Id.Should().Be(order.Id);
        deliveryResult.DeliveryPartner.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ExistingDelivery_ReturnsDeliveryWithNavigationProperties()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER404",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.Cancelled
        };
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(delivery.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Order.Should().NotBeNull();
        result.DeliveryPartner.Should().NotBeNull();
        result.Order.Id.Should().Be(order.Id);
        result.DeliveryPartner.Id.Should().Be(user.Id);
        result.DeliveryStatus.Should().Be(DeliveryStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateAsync_ValidDelivery_ReturnsUpdatedDelivery()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER505",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();

        // Act
        delivery.DeliveryStatus = DeliveryStatus.Delivered;
        delivery.DropoffLocation = "123 Main St (Updated)";
        var result = await _repository.UpdateAsync(delivery);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryStatus.Should().Be(DeliveryStatus.Delivered);
        result.DropoffLocation.Should().Be("123 Main St (Updated)");
    }

    [Fact]
    public async Task DeleteAsync_ExistingDelivery_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Name = "Delivery Partner",
            Email = "partner@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        var vendor = new User
        {
            Name = "Vendor",
            Email = "vendor@example.com",
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor,
            Verified = true
        };
        await _context.Users.AddRangeAsync(user, vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall
        {
            Name = "Test Stall",
            ShortDescription = "Test Description",
            VendorId = vendor.Id
        };
        await _context.Stalls.AddAsync(stall);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            StallId = stall.Id,
            VendorOrderId = "ORDER606",
            TotalPrice = 100.00m,
            Status = OrderStatus.ToPrepare,
            FulfillmentMethod = FulfillmentMethod.Delivery,
            CreatedAt = DateTime.Now
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var delivery = new Delivery
        {
            OrderId = order.Id,
            DeliveryPartnerId = user.Id,
            PickupLocation = "Restaurant ABC",
            DropoffLocation = "123 Main St",
            DeliveryStatus = DeliveryStatus.PickedUp
        };
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(delivery.Id);

        // Assert
        result.Should().BeTrue();
        var deletedDelivery = await _repository.GetByIdAsync(delivery.Id);
        deletedDelivery.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingDelivery_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }
} 