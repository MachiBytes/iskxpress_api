using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class ProductRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly ProductRepository _repository;
    private readonly SqliteConnection _connection;

    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id, Vendor = vendor };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id, Vendor = vendor };
        var section = new StallSection { Name = "Test Section", StallId = stall.Id, Stall = stall };
        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id,
            Category = category,
            Section = section,
            Stall = stall
        };

        _context.Users.Add(vendor);
        _context.Stalls.Add(stall);
        _context.Categories.Add(category);
        _context.StallSections.Add(section);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ShouldReturnProductsForCategory()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product1 = new Product 
        { 
            Name = "Product 1", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        var product2 = new Product 
        { 
            Name = "Product 2", 
            BasePrice = 20.00m,
            PriceWithMarkup = 22.00m,
            PriceWithDelivery = 25.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryIdAsync(category.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Product 1");
        result.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task GetByStallIdAsync_ShouldReturnProductsForStall()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product1 = new Product 
        { 
            Name = "Product 1", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        var product2 = new Product 
        { 
            Name = "Product 2", 
            BasePrice = 20.00m,
            PriceWithMarkup = 22.00m,
            PriceWithDelivery = 25.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStallIdAsync(stall.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Product 1");
        result.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task GetBySectionIdAsync_ShouldReturnProductsForSection()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product1 = new Product 
        { 
            Name = "Product 1", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        await _context.Products.AddAsync(product1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySectionIdAsync(section.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingProducts()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product1 = new Product 
        { 
            Name = "Apple Pie", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        var product2 = new Product 
        { 
            Name = "Apple Juice", 
            BasePrice = 5.00m,
            PriceWithMarkup = 6.00m,
            PriceWithDelivery = 8.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        var product3 = new Product 
        { 
            Name = "Orange Juice", 
            BasePrice = 5.00m,
            PriceWithMarkup = 6.00m,
            PriceWithDelivery = 8.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Apple");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Apple Pie");
        result.Should().Contain(p => p.Name == "Apple Juice");
    }

    [Fact]
    public async Task GetByPriceRangeAsync_ShouldReturnProductsInPriceRange()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product1 = new Product 
        { 
            Name = "Cheap Product", 
            BasePrice = 5.00m,
            PriceWithMarkup = 6.00m,
            PriceWithDelivery = 8.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        var product2 = new Product 
        { 
            Name = "Expensive Product", 
            BasePrice = 20.00m,
            PriceWithMarkup = 22.00m,
            PriceWithDelivery = 25.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPriceRangeAsync(5.00m, 10.00m);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Cheap Product");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnProductWithNavigationProperties()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
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

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Category.Should().NotBeNull();
        result.Section.Should().NotBeNull();
        result.Stall.Should().NotBeNull();
        result.Category.Name.Should().Be("Test Category");
        result.Section.Name.Should().Be("Test Section");
        result.Stall.Name.Should().Be("Test Stall");
    }

    [Fact]
    public async Task AddAsync_ShouldAddProduct()
    {
        // Arrange
        var vendor = new User { Email = "vendor@example.com", Name = "Vendor", Role = UserRole.Vendor };
        await _context.Users.AddAsync(vendor);
        await _context.SaveChangesAsync();

        var stall = new Stall { Name = "Test Stall", VendorId = vendor.Id };
        var category = new Category { Name = "Test Category", VendorId = vendor.Id };
        await _context.Stalls.AddAsync(stall);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var section = new StallSection { Name = "Test Section", StallId = stall.Id };
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "New Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PriceWithDelivery = 15.00m,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Product");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 