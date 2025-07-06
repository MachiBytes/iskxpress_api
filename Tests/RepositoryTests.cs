using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using iskxpress_api.Data;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using Xunit;

namespace iskxpress_api.Tests;

public class RepositoryTests : IDisposable
{
    private readonly IskExpressDbContext _context;
    private readonly ServiceProvider _serviceProvider;
    private readonly SqliteConnection _connection;
    private readonly IUserRepository _userRepository;
    private readonly IStallRepository _stallRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartItemRepository _cartItemRepository;

    public RepositoryTests()
    {
        var services = new ServiceCollection();
        
        // Create SQLite in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        services.AddDbContext<IskExpressDbContext>(options =>
            options.UseSqlite(_connection)
                   .EnableServiceProviderCaching(false));
        
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStallRepository, StallRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<IskExpressDbContext>();
        _context.Database.EnsureCreated();
        
        _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        _stallRepository = _serviceProvider.GetRequiredService<IStallRepository>();
        _productRepository = _serviceProvider.GetRequiredService<IProductRepository>();
        _orderRepository = _serviceProvider.GetRequiredService<IOrderRepository>();
        _cartItemRepository = _serviceProvider.GetRequiredService<ICartItemRepository>();
    }

    [Fact]
    public async Task UserRepository_Should_Find_User_By_Email()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.User
        };
        await _userRepository.AddAsync(user);

        // Act
        var foundUser = await _userRepository.GetByEmailAsync("test@example.com");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Name.Should().Be("Test User");
        foundUser.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task UserRepository_Should_Check_Email_Exists()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "exists@example.com",
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.User
        };
        await _userRepository.AddAsync(user);

        // Act
        var exists = await _userRepository.EmailExistsAsync("exists@example.com");
        var notExists = await _userRepository.EmailExistsAsync("notexists@example.com");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task UserRepository_Should_Filter_By_Role()
    {
        // Arrange
        var user1 = new User { Name = "User 1", Email = "user1@example.com", Verified = true, AuthProvider = AuthProvider.Google, Role = UserRole.User };
        var user2 = new User { Name = "User 2", Email = "user2@example.com", Verified = true, AuthProvider = AuthProvider.Google, Role = UserRole.User };
        var vendor1 = new User { Name = "Vendor 1", Email = "vendor1@example.com", Verified = true, AuthProvider = AuthProvider.Google, Role = UserRole.Vendor };
        
        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.AddAsync(vendor1);

        // Act
        var users = await _userRepository.GetByRoleAsync(UserRole.User);
        var vendors = await _userRepository.GetByRoleAsync(UserRole.Vendor);

        // Assert
        users.Should().HaveCount(2);
        vendors.Should().HaveCount(1);
        vendors.First().Name.Should().Be("Vendor 1");
    }

    [Fact]
    public async Task StallRepository_Should_Get_Stalls_By_Vendor()
    {
        // Arrange
        var vendor1 = await CreateTestVendor("vendor1@example.com");
        var vendor2 = await CreateTestVendor("vendor2@example.com");
        
        var stall1 = new Stall { Name = "Stall 1", ShortDescription = "Description 1", VendorId = vendor1.Id };
        var stall2 = new Stall { Name = "Stall 2", ShortDescription = "Description 2", VendorId = vendor2.Id };
        
        await _stallRepository.AddAsync(stall1);
        await _stallRepository.AddAsync(stall2);

        // Act
        var vendor1Stall = await _stallRepository.GetByVendorIdAsync(vendor1.Id);
        var vendor2Stall = await _stallRepository.GetByVendorIdAsync(vendor2.Id);

        // Assert
        vendor1Stall.Should().NotBeNull();
        vendor2Stall.Should().NotBeNull();
        vendor1Stall!.VendorId.Should().Be(vendor1.Id);
        vendor2Stall!.VendorId.Should().Be(vendor2.Id);
        vendor1Stall.Name.Should().Be("Stall 1");
        vendor2Stall.Name.Should().Be("Stall 2");
    }

    [Fact]
    public async Task ProductRepository_Should_Get_Products_By_Stall()
    {
        // Arrange
        var vendor1 = await CreateTestVendor("vendor1@example.com");
        var vendor2 = await CreateTestVendor("vendor2@example.com");
        var stall1 = await CreateTestStall("Stall 1", vendor1.Id);
        var stall2 = await CreateTestStall("Stall 2", vendor2.Id);
        var category1 = await CreateTestCategory("Category 1");
        var category2 = await CreateTestCategory("Category 2");
        var section1 = await CreateTestStallSection("Section 1", stall1.Id);
        var section2 = await CreateTestStallSection("Section 2", stall2.Id);
        
        var product1 = new Product { Name = "Product 1", BasePrice = 100, PriceWithMarkup = 110, CategoryId = category1.Id, SectionId = section1.Id, StallId = stall1.Id };
        var product2 = new Product { Name = "Product 2", BasePrice = 200, PriceWithMarkup = 220, CategoryId = category1.Id, SectionId = section1.Id, StallId = stall1.Id };
        var product3 = new Product { Name = "Product 3", BasePrice = 300, PriceWithMarkup = 330, CategoryId = category2.Id, SectionId = section2.Id, StallId = stall2.Id };
        
        await _productRepository.AddAsync(product1);
        await _productRepository.AddAsync(product2);
        await _productRepository.AddAsync(product3);

        // Act
        var stall1Products = await _productRepository.GetByStallIdAsync(stall1.Id);
        var stall2Products = await _productRepository.GetByStallIdAsync(stall2.Id);

        // Assert
        stall1Products.Should().HaveCount(2);
        stall2Products.Should().HaveCount(1);
        stall1Products.All(p => p.StallId == stall1.Id).Should().BeTrue();
    }

    [Fact]
    public async Task ProductRepository_Should_Include_Related_Data()
    {
        // Arrange
        var vendor = await CreateTestVendor("vendor@example.com");
        var stall = await CreateTestStall("Test Stall", vendor.Id);
        var category = await CreateTestCategory("Test Category");
        var section = await CreateTestStallSection("Test Section", stall.Id);
        
        var product = new Product
        {
            Name = "Test Product",
            BasePrice = 100,
            PriceWithMarkup = 110,
            CategoryId = category.Id,
            SectionId = section.Id,
            StallId = stall.Id
        };
        await _productRepository.AddAsync(product);

        // Act
        var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(product.Id);

        // Assert
        productWithDetails.Should().NotBeNull();
        productWithDetails!.Category.Should().NotBeNull();
        productWithDetails.Section.Should().NotBeNull();
        productWithDetails.Stall.Should().NotBeNull();
        productWithDetails.Stall.Vendor.Should().NotBeNull();
        productWithDetails.Stall.Vendor.Email.Should().Be("vendor@example.com");
    }

    [Fact]
    public async Task OrderRepository_Should_Get_Orders_By_User()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@example.com");
        var user2 = await CreateTestUser("user2@example.com");
        var vendor = await CreateTestVendor("vendor@example.com");
        var stall = await CreateTestStall("Test Stall", vendor.Id);
        
        var order1 = new Order { UserId = user1.Id, StallId = stall.Id, VendorOrderId = "VO001", Status = OrderStatus.Pending, FulfillmentMethod = FulfillmentMethod.Pickup, TotalPrice = 100 };
        var order2 = new Order { UserId = user1.Id, StallId = stall.Id, VendorOrderId = "VO002", Status = OrderStatus.Pending, FulfillmentMethod = FulfillmentMethod.Delivery, TotalPrice = 200 };
        var order3 = new Order { UserId = user2.Id, StallId = stall.Id, VendorOrderId = "VO003", Status = OrderStatus.Pending, FulfillmentMethod = FulfillmentMethod.Pickup, TotalPrice = 150 };
        
        await _orderRepository.AddAsync(order1);
        await _orderRepository.AddAsync(order2);
        await _orderRepository.AddAsync(order3);

        // Act
        var user1Orders = await _orderRepository.GetByUserIdAsync(user1.Id);
        var user2Orders = await _orderRepository.GetByUserIdAsync(user2.Id);

        // Assert
        user1Orders.Should().HaveCount(2);
        user2Orders.Should().HaveCount(1);
        user1Orders.Sum(o => o.TotalPrice).Should().Be(300);
    }

    [Fact]
    public async Task OrderRepository_Should_Get_Orders_By_Status()
    {
        // Arrange
        var user = await CreateTestUser("user@example.com");
        var vendor = await CreateTestVendor("vendor@example.com");
        var stall = await CreateTestStall("Test Stall", vendor.Id);
        
        var order1 = new Order { UserId = user.Id, StallId = stall.Id, VendorOrderId = "VO001", Status = OrderStatus.Pending, FulfillmentMethod = FulfillmentMethod.Pickup, TotalPrice = 100 };
        var order2 = new Order { UserId = user.Id, StallId = stall.Id, VendorOrderId = "VO002", Status = OrderStatus.Preparing, FulfillmentMethod = FulfillmentMethod.Delivery, TotalPrice = 200 };
        var order3 = new Order { UserId = user.Id, StallId = stall.Id, VendorOrderId = "VO003", Status = OrderStatus.Pending, FulfillmentMethod = FulfillmentMethod.Pickup, TotalPrice = 150 };
        
        await _orderRepository.AddAsync(order1);
        await _orderRepository.AddAsync(order2);
        await _orderRepository.AddAsync(order3);

        // Act
        var pendingOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Pending);
        var toPrepareOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Preparing);

        // Assert
        pendingOrders.Should().HaveCount(2);
        toPrepareOrders.Should().HaveCount(1);
        pendingOrders.All(o => o.Status == OrderStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task CartItemRepository_Should_Get_Cart_By_User()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@example.com");
        var user2 = await CreateTestUser("user2@example.com");
        var vendor = await CreateTestVendor("vendor@example.com");
        var stall = await CreateTestStall("Test Stall", vendor.Id);
        var category = await CreateTestCategory("Test Category");
        var section = await CreateTestStallSection("Test Section", stall.Id);
        var product1 = await CreateTestProduct("Product 1", category.Id, section.Id, stall.Id);
        var product2 = await CreateTestProduct("Product 2", category.Id, section.Id, stall.Id);
        
        var cartItem1 = new CartItem { UserId = user1.Id, ProductId = product1.Id, StallId = stall.Id, Quantity = 2 };
        var cartItem2 = new CartItem { UserId = user1.Id, ProductId = product2.Id, StallId = stall.Id, Quantity = 1 };
        var cartItem3 = new CartItem { UserId = user2.Id, ProductId = product1.Id, StallId = stall.Id, Quantity = 3 };
        
        await _cartItemRepository.AddAsync(cartItem1);
        await _cartItemRepository.AddAsync(cartItem2);
        await _cartItemRepository.AddAsync(cartItem3);

        // Act
        var user1Cart = await _cartItemRepository.GetByUserIdAsync(user1.Id);
        var user2Cart = await _cartItemRepository.GetByUserIdAsync(user2.Id);

        // Assert
        user1Cart.Should().HaveCount(2);
        user2Cart.Should().HaveCount(1);
        user1Cart.Sum(ci => ci.Quantity).Should().Be(3);
        user2Cart.Sum(ci => ci.Quantity).Should().Be(3);
    }

    [Fact]
    public async Task CartItemRepository_Should_Clear_User_Cart()
    {
        // Arrange
        var user = await CreateTestUser("user@example.com");
        var vendor = await CreateTestVendor("vendor@example.com");
        var stall = await CreateTestStall("Test Stall", vendor.Id);
        var category = await CreateTestCategory("Test Category");
        var section = await CreateTestStallSection("Test Section", stall.Id);
        var product1 = await CreateTestProduct("Product 1", category.Id, section.Id, stall.Id);
        
        var cartItem1 = new CartItem { UserId = user.Id, ProductId = product1.Id, StallId = stall.Id, Quantity = 2 };
        await _cartItemRepository.AddAsync(cartItem1);

        // Act
        var result = await _cartItemRepository.ClearCartAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var cartAfter = await _cartItemRepository.GetByUserIdAsync(user.Id);
        cartAfter.Should().BeEmpty();
    }

    // Helper methods
    private async Task<User> CreateTestUser(string email)
    {
        var user = new User
        {
            Name = "Test User",
            Email = email,
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.User
        };
        return await _userRepository.AddAsync(user);
    }

    private async Task<User> CreateTestVendor(string email)
    {
        var vendor = new User
        {
            Name = "Test Vendor",
            Email = email,
            Verified = true,
            AuthProvider = AuthProvider.Google,
            Role = UserRole.Vendor
        };
        return await _userRepository.AddAsync(vendor);
    }

    private async Task<Stall> CreateTestStall(string name, int vendorId)
    {
        var stall = new Stall
        {
            Name = name,
            ShortDescription = "Test Description",
            VendorId = vendorId
        };
        return await _stallRepository.AddAsync(stall);
    }

    private async Task<Category> CreateTestCategory(string name)
    {
        var category = new Category { Name = name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    private async Task<StallSection> CreateTestStallSection(string name, int stallId)
    {
        var section = new StallSection { Name = name, StallId = stallId };
        _context.StallSections.Add(section);
        await _context.SaveChangesAsync();
        return section;
    }

    private async Task<Product> CreateTestProduct(string name, int categoryId, int sectionId, int stallId)
    {
        var product = new Product
        {
            Name = name,
            BasePrice = 100,
            PriceWithMarkup = 110,
            CategoryId = categoryId,
            SectionId = sectionId,
            StallId = stallId
        };
        return await _productRepository.AddAsync(product);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
} 