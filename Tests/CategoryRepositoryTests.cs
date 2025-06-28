using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly CategoryRepository _repository;
    private readonly SqliteConnection _connection;

    public CategoryRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetByVendorIdAsync_ShouldReturnCategoriesForVendor()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var category1 = new Category { Name = "Category 1", VendorId = vendor.Id, Vendor = vendor };
        var category2 = new Category { Name = "Category 2", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVendorIdAsync(vendor.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Category 1");
        result.Should().Contain(c => c.Name == "Category 2");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingCategories()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var category1 = new Category { Name = "Food Items", VendorId = vendor.Id, Vendor = vendor };
        var category2 = new Category { Name = "Food Drinks", VendorId = vendor.Id, Vendor = vendor };
        var category3 = new Category { Name = "Electronics", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Categories.AddRange(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Food");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Food Items");
        result.Should().Contain(c => c.Name == "Food Drinks");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnCategoryWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Vendor.Should().NotBeNull();
        result.Vendor.Name.Should().Be("Vendor");
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategory()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        _context.Users.Add(vendor);
        await _context.SaveChangesAsync();

        var category = new Category { Name = "New Category", VendorId = vendor.Id };

        // Act
        var result = await _repository.AddAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCategory()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var category = new Category { Name = "To Delete", VendorId = vendor.Id, Vendor = vendor };

        _context.Users.Add(vendor);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(category.Id);

        // Assert
        result.Should().BeTrue();
        var deletedCategory = await _repository.GetByIdAsync(category.Id);
        deletedCategory.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 