using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Tests;

public class CartItemRepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly CartItemRepository _repository;
    private readonly SqliteConnection _connection;

    public CartItemRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<IskExpressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IskExpressDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new CartItemRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnCartItemsForUser()
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
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PremiumUserPrice = Math.Round(12.00m * 0.90m, 0),
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cartItem = new CartItem 
        { 
            UserId = user.Id, 
            ProductId = product.Id, 
            StallId = stall.Id, 
            Quantity = 2
        };
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnCartItemsForProduct()
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
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PremiumUserPrice = Math.Round(12.00m * 0.90m, 0),
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cartItem = new CartItem 
        { 
            UserId = user.Id, 
            ProductId = product.Id, 
            StallId = stall.Id, 
            Quantity = 2
        };
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProductIdAsync(product.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetByStallIdAsync_ShouldReturnCartItemsForStall()
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
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PremiumUserPrice = Math.Round(12.00m * 0.90m, 0),
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cartItem = new CartItem 
        { 
            UserId = user.Id, 
            ProductId = product.Id, 
            StallId = stall.Id, 
            Quantity = 2
        };
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStallIdAsync(stall.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldRemoveAllCartItemsForUser()
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
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PremiumUserPrice = Math.Round(12.00m * 0.90m, 0),
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cartItem1 = new CartItem { UserId = user.Id, ProductId = product.Id, StallId = stall.Id, Quantity = 2 };
        var cartItem2 = new CartItem { UserId = user.Id, ProductId = product.Id, StallId = stall.Id, Quantity = 3 };
        await _context.CartItems.AddRangeAsync(cartItem1, cartItem2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ClearCartAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var remainingItems = await _repository.GetByUserIdAsync(user.Id);
        remainingItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddCartItem()
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
        await _context.StallSections.AddAsync(section);
        await _context.SaveChangesAsync();

        var product = new Product 
        { 
            Name = "Test Product", 
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m,
            PremiumUserPrice = Math.Round(12.00m * 0.90m, 0),
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var cartItem = new CartItem 
        { 
            UserId = user.Id, 
            ProductId = product.Id, 
            StallId = stall.Id, 
            Quantity = 2 
        };

        // Act
        var result = await _repository.AddAsync(cartItem);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnCartItemDetails()
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
        
        var cartItem = new CartItem { UserId = user.Id, ProductId = product.Id, StallId = stall.Id, Quantity = 2 };
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(cartItem.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cartItem.Id);
        result.User.Should().NotBeNull();
        result.Product.Should().NotBeNull();
        result.Stall.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 