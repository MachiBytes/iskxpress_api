using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class StallRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly StallRepository _repository;
    private readonly SqliteConnection _connection;

    public StallRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new StallRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStall_WhenStallExists()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall 
        { 
            Name = "Test Stall", 
            ShortDescription = "A test stall",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(stall.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(stall.Id);
        result.Name.Should().Be("Test Stall");
    }

    [Fact]
    public async Task GetByVendorIdAsync_ShouldReturnStallForVendor()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall 
        { 
            Name = "Vendor's Stall", 
            ShortDescription = "The vendor's only stall",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVendorIdAsync(vendor.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Vendor's Stall");
        result.VendorId.Should().Be(vendor.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingStalls()
    {
        // Arrange
        var vendor1 = new User { Email = "vendor1@example.com", Name = "Vendor 1", Role = UserRole.Vendor };
        var vendor2 = new User { Email = "vendor2@example.com", Name = "Vendor 2", Role = UserRole.Vendor };
        var vendor3 = new User { Email = "vendor3@example.com", Name = "Vendor 3", Role = UserRole.Vendor };
        var stall1 = new Stall 
        { 
            Name = "Coffee Shop", 
            ShortDescription = "Best coffee in town",
            VendorId = vendor1.Id, 
            Vendor = vendor1 
        };
        var stall2 = new Stall 
        { 
            Name = "Coffee Corner", 
            ShortDescription = "Corner coffee stand",
            VendorId = vendor2.Id, 
            Vendor = vendor2 
        };
        var stall3 = new Stall 
        { 
            Name = "Tea House", 
            ShortDescription = "Traditional tea",
            VendorId = vendor3.Id, 
            Vendor = vendor3 
        };

        _context.Users.AddRange(vendor1, vendor2, vendor3);
        _context.Stalls.AddRange(stall1, stall2, stall3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Coffee");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Coffee Shop");
        result.Should().Contain(s => s.Name == "Coffee Corner");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnStallWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall 
        { 
            Name = "Test Stall", 
            ShortDescription = "A test stall",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };
        var section = new StallSection { Name = "Test Section", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(stall.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Vendor.Should().NotBeNull();
        result.StallSections.Should().HaveCount(1);
        result.Vendor.Name.Should().Be("Vendor");
        result.StallSections.First().Name.Should().Be("Test Section");
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_ShouldReturnAllStallsWithNavigationProperties()
    {
        // Arrange
        var vendor1 = new User { Email = "vendor1@example.com", Name = "Vendor 1", Role = UserRole.Vendor };
        var vendor2 = new User { Email = "vendor2@example.com", Name = "Vendor 2", Role = UserRole.Vendor };
        var stall1 = new Stall 
        { 
            Name = "Stall 1", 
            ShortDescription = "First stall",
            VendorId = vendor1.Id, 
            Vendor = vendor1 
        };
        var stall2 = new Stall 
        { 
            Name = "Stall 2", 
            ShortDescription = "Second stall",
            VendorId = vendor2.Id, 
            Vendor = vendor2 
        };

        _context.Users.AddRange(vendor1, vendor2);
        _context.Stalls.AddRange(stall1, stall2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.Vendor != null).Should().BeTrue();
        result.Should().Contain(s => s.Name == "Stall 1");
        result.Should().Contain(s => s.Name == "Stall 2");
    }

    [Fact]
    public async Task AddAsync_ShouldAddStall()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        _context.Users.Add(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall 
        { 
            Name = "New Stall", 
            ShortDescription = "A new stall",
            VendorId = vendor.Id
        };

        // Act
        var result = await _repository.AddAsync(stall);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Stall");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStall()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall 
        { 
            Name = "Original Name", 
            ShortDescription = "Original description",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        // Act
        stall.Name = "Updated Name";
        stall.ShortDescription = "Updated description";
        var result = await _repository.UpdateAsync(stall);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.ShortDescription.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveStall()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall 
        { 
            Name = "To Delete", 
            ShortDescription = "Will be deleted",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(stall.Id);

        // Assert
        result.Should().BeTrue();
        var deletedStall = await _repository.GetByIdAsync(stall.Id);
        deletedStall.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 