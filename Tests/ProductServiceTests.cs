using Xunit;
using Moq;
using FluentAssertions;
using iskxpress_api.Services;
using iskxpress_api.Repositories;
using iskxpress_api.Models;
using iskxpress_api.DTOs.Vendors;
using Microsoft.Extensions.Logging;

namespace iskxpress_api.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IStallRepository> _mockStallRepository;
    private readonly Mock<IStallSectionRepository> _mockSectionRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockStallRepository = new Mock<IStallRepository>();
        _mockSectionRepository = new Mock<IStallSectionRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockFileRepository = new Mock<IFileRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(
            _mockProductRepository.Object,
            _mockStallRepository.Object,
            _mockSectionRepository.Object,
            _mockCategoryRepository.Object,
            _mockFileRepository.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductResponse()
    {
        // Arrange
        var productId = 1;
        var stall = new Stall { Id = 1, Name = "Test Stall", VendorId = 1 };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = stall.Id };
        var category = new Category { Id = 1, Name = "Test Category" };

        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            BasePrice = 12.99m,
            PriceWithMarkup = 15.00m,
            PriceWithDelivery = 18.00m,
            Availability = ProductAvailability.Available,
            SectionId = section.Id,
            Section = section,
            CategoryId = category.Id,
            Category = category,
            StallId = stall.Id,
            Stall = stall
        };

        _mockProductRepository.Setup(repo => repo.GetByIdWithDetailsAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
        result.BasePrice.Should().Be(12.99m);
        result.Availability.Should().Be(ProductAvailability.Available);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = 1;
        _mockProductRepository.Setup(repo => repo.GetByIdWithDetailsAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsByStallIdAsync_ValidStall_ReturnsProducts()
    {
        // Arrange
        var stallId = 1;
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", BasePrice = 10.00m },
            new Product { Id = 2, Name = "Product 2", BasePrice = 15.00m }
        };

        _mockProductRepository.Setup(repo => repo.GetByStallIdAsync(stallId))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetProductsByStallIdAsync(stallId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        result.Last().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetProductsBySectionIdAsync_ValidSection_ReturnsProducts()
    {
        // Arrange
        var sectionId = 1;
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", BasePrice = 10.00m },
            new Product { Id = 2, Name = "Product 2", BasePrice = 15.00m }
        };

        _mockProductRepository.Setup(repo => repo.GetBySectionIdAsync(sectionId))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetProductsBySectionIdAsync(sectionId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        result.Last().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetAvailableProductsByStallIdAsync_ValidStall_ReturnsOnlyAvailableProducts()
    {
        // Arrange
        var stallId = 1;
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Available Product", BasePrice = 10.00m, Availability = ProductAvailability.Available },
            new Product { Id = 2, Name = "Sold Out Product", BasePrice = 15.00m, Availability = ProductAvailability.SoldOut }
        };

        _mockProductRepository.Setup(repo => repo.GetByStallIdAsync(stallId))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAvailableProductsByStallIdAsync(stallId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
        result.First().Name.Should().Be("Available Product");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        var stallId = 1;
        var stall = new Stall { Id = stallId, Name = "Test Stall", VendorId = 1 };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = stallId };
        var category = new Category { Id = 1, Name = "Test Category" };

        var request = new CreateProductRequest
        {
            Name = "New Product",
            BasePrice = 12.99m,
            Availability = ProductAvailability.Available,
            CategoryId = 1,
            SectionId = 1,
            StallId = stallId
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = request.Name,
            BasePrice = request.BasePrice,
            PriceWithMarkup = 15.00m,
            PriceWithDelivery = 18.00m,
            Availability = request.Availability,
            CategoryId = request.CategoryId,
            SectionId = request.SectionId,
            StallId = stallId
        };

        _mockStallRepository.Setup(repo => repo.GetByIdAsync(stallId))
            .ReturnsAsync(stall);
        _mockSectionRepository.Setup(repo => repo.GetByIdAsync(request.SectionId))
            .ReturnsAsync(section);
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(request.CategoryId))
            .ReturnsAsync(category);
        _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _productService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.BasePrice.Should().Be(12.99m);
        result.Availability.Should().Be(ProductAvailability.Available);
    }

    [Fact]
    public async Task CreateAsync_StallNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            BasePrice = 12.99m,
            CategoryId = 1,
            SectionId = 1,
            StallId = 1
        };

        _mockStallRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((Stall?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _productService.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var stallId = 1;
        var stall = new Stall { Id = stallId, Name = "Test Stall", VendorId = 1 };
        var section = new StallSection { Id = 1, Name = "Test Section", StallId = stallId };
        var category = new Category { Id = 1, Name = "Test Category" };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Existing Product",
            BasePrice = 10.00m,
            StallId = stallId
        };

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            BasePrice = 15.0m,
            PriceWithMarkup = 17.0m,
            PriceWithDelivery = 20.0m,
            CategoryId = 1,
            SectionId = 1,
            Availability = ProductAvailability.Available
        };

        _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(request.CategoryId))
            .ReturnsAsync(category);
        _mockSectionRepository.Setup(repo => repo.GetByIdAsync(request.SectionId))
            .ReturnsAsync(section);
        _mockProductRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _productService.UpdateAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Product");
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            BasePrice = 15.0m,
            PriceWithMarkup = 17.0m,
            PriceWithDelivery = 20.0m,
            CategoryId = 1,
            SectionId = 1,
            Availability = ProductAvailability.Available
        };

        _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateAsync(productId, request));
    }

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId };

        _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(repo => repo.DeleteAsync(productId))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_ProductNotFound_ReturnsFalse()
    {
        // Arrange
        var productId = 1;
        _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeFalse();
    }
} 