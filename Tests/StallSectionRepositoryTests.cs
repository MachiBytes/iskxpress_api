using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class StallSectionRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly StallSectionRepository _repository;
    private readonly SqliteConnection _connection;

    public StallSectionRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new StallSectionRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStallSection_WhenStallSectionExists()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section = new StallSection { Name = "Test Section", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(section.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(section.Id);
        result.Name.Should().Be("Test Section");
    }

    [Fact]
    public async Task GetByStallIdAsync_ShouldReturnSectionsForStall()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section1 = new StallSection { Name = "Section 1", StallId = stall.Id, Stall = stall };
        var section2 = new StallSection { Name = "Section 2", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.AddRange(section1, section2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStallIdAsync(stall.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Section 1");
        result.Should().Contain(s => s.Name == "Section 2");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingSections()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section1 = new StallSection { Name = "Main Course", StallId = stall.Id, Stall = stall };
        var section2 = new StallSection { Name = "Main Drinks", StallId = stall.Id, Stall = stall };
        var section3 = new StallSection { Name = "Desserts", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.AddRange(section1, section2, section3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Main");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Main Course");
        result.Should().Contain(s => s.Name == "Main Drinks");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnSectionWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section = new StallSection { Name = "Test Section", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(section.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Stall.Should().NotBeNull();
        result.Stall.Name.Should().Be("Test Stall");
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_ShouldReturnAllSectionsWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section1 = new StallSection { Name = "Section 1", StallId = stall.Id, Stall = stall };
        var section2 = new StallSection { Name = "Section 2", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.AddRange(section1, section2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.Stall != null).Should().BeTrue();
        result.Should().Contain(s => s.Name == "Section 1");
        result.Should().Contain(s => s.Name == "Section 2");
    }

    [Fact]
    public async Task AddAsync_ShouldAddStallSection()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "New Section", StallId = stall.Id };

        // Act
        var result = await _repository.AddAsync(section);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Section");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStallSection()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section = new StallSection { Name = "Original Name", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();

        // Act
        section.Name = "Updated Name";
        var result = await _repository.UpdateAsync(section);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveStallSection()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var section = new StallSection { Name = "To Delete", StallId = stall.Id, Stall = stall };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(section.Id);

        // Assert
        result.Should().BeTrue();
        var deletedSection = await _repository.GetByIdAsync(section.Id);
        deletedSection.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 