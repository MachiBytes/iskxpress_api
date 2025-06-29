using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using iskxpress_api.Data;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services;
using Moq;

namespace iskxpress_api.Tests;

public class StallServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IskExpressDbContext _context;
    private readonly StallRepository _stallRepository;
    private readonly StallService _stallService;

    public StallServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        
        _stallRepository = new StallRepository(_context);
        var mockFileRepository = new Mock<IFileRepository>();
        _stallService = new StallService(_stallRepository, mockFileRepository.Object);
    }

    [Fact]
    public async Task GetAllStallsAsync_ShouldReturnAllStalls()
    {
        // Arrange
        var vendor1 = new User 
        { 
            Email = "vendor1@example.com", 
            Name = "Vendor 1", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };
        var vendor2 = new User 
        { 
            Email = "vendor2@example.com", 
            Name = "Vendor 2", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };
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
        var result = await _stallService.GetAllStallsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        var stallList = result.ToList();
        stallList.Should().Contain(s => s.Name == "Stall 1");
        stallList.Should().Contain(s => s.Name == "Stall 2");
    }

    [Fact]
    public async Task UpdateStallAsync_WithValidData_ShouldUpdateStall()
    {
        // Arrange
        var vendor = new User 
        { 
            Email = "vendor@example.com", 
            Name = "Test Vendor", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };
        var stall = new Stall 
        { 
            Name = "Original Stall", 
            ShortDescription = "Original description",
            PictureId = null,
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        // Create a file record first
        var fileRecord = new FileRecord
        {
            Type = FileType.StallAvatar,
            ObjectKey = "stall-pictures/test.jpg",
            ObjectUrl = "https://example.com/stall-pictures/test.jpg",
            EntityId = stall.Id
        };
        _context.Files.Add(fileRecord);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateStallRequest
        {
            Name = "Updated Stall Name",
            ShortDescription = "Updated description",
            PictureId = fileRecord.Id
        };

        // Act
        var result = await _stallService.UpdateStallAsync(stall.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(stall.Id);
        result.Name.Should().Be("Updated Stall Name");
        result.ShortDescription.Should().Be("Updated description");
        result.PictureId.Should().Be(fileRecord.Id);
        result.VendorId.Should().Be(vendor.Id);
    }

    [Fact]
    public async Task UpdateStallAsync_WithNonExistentStall_ShouldReturnNull()
    {
        // Arrange
        var updateRequest = new UpdateStallRequest
        {
            Name = "Updated Name",
            ShortDescription = "Updated description"
        };

        // Act
        var result = await _stallService.UpdateStallAsync(999, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStallByVendorIdAsync_ShouldReturnVendorStall()
    {
        // Arrange
        var vendor = new User 
        { 
            Email = "vendor@example.com", 
            Name = "Test Vendor", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };
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
        var result = await _stallService.GetStallByVendorIdAsync(vendor.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Vendor's Stall");
        result.VendorId.Should().Be(vendor.Id);
    }

    [Fact]
    public async Task CreateStallAsync_WithValidData_ShouldCreateStall()
    {
        // Arrange
        var vendor = new User 
        { 
            Email = "vendor@example.com", 
            Name = "Test Vendor", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };

        _context.Users.Add(vendor);
        await _context.SaveChangesAsync();

        var createRequest = new CreateStallRequest
        {
            VendorId = vendor.Id,
            Name = "New Stall",
            ShortDescription = "A brand new stall",
            PictureId = null
        };

        // Act
        var result = await _stallService.CreateStallAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Stall");
        result.ShortDescription.Should().Be("A brand new stall");
        result.VendorId.Should().Be(vendor.Id);
    }

    [Fact]
    public async Task CreateStallAsync_WhenVendorAlreadyHasStall_ShouldReturnNull()
    {
        // Arrange
        var vendor = new User 
        { 
            Email = "vendor@example.com", 
            Name = "Test Vendor", 
            Role = UserRole.Vendor, 
            AuthProvider = AuthProvider.Google,
            Verified = true
        };
        var existingStall = new Stall 
        { 
            Name = "Existing Stall", 
            ShortDescription = "Already exists",
            VendorId = vendor.Id, 
            Vendor = vendor 
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(existingStall);
        await _context.SaveChangesAsync();

        var createRequest = new CreateStallRequest
        {
            VendorId = vendor.Id,
            Name = "Second Stall",
            ShortDescription = "Should not be allowed",
            PictureId = null
        };

        // Act
        var result = await _stallService.CreateStallAsync(createRequest);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
} 