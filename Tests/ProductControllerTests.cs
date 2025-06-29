using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using iskxpress_api.Controllers;
using iskxpress_api.Services;
using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;

namespace iskxpress_api.Tests;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _controller = new ProductController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetProduct_ExistingProduct_ReturnsOkWithProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new ProductResponse
        {
            Id = productId,
            Name = "Test Product",
            BasePrice = 12.99m,
            PriceWithMarkup = 15.00m,
            PriceWithDelivery = 18.00m,
            Availability = ProductAvailability.Available,
            AvailabilityText = "Available",
            CategoryName = "Test Category",
            SectionName = "Test Section",
            StallName = "Test Stall"
        };

        _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var product = okResult.Value.Should().BeOfType<ProductResponse>().Subject;
        product.Id.Should().Be(productId);
        product.Name.Should().Be("Test Product");
        product.BasePrice.Should().Be(12.99m);
        product.Availability.Should().Be(ProductAvailability.Available);
    }

    [Fact]
    public async Task GetProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = 1;
        _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
            .ReturnsAsync((ProductResponse?)null);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Product with ID {productId} not found");
    }

    [Fact]
    public async Task GetProductsByStall_ValidStall_ReturnsOkWithProducts()
    {
        // Arrange
        var stallId = 1;
        var expectedProducts = new List<ProductResponse>
        {
            new ProductResponse
            {
                Id = 1,
                Name = "Product 1",
                BasePrice = 10.00m,
                Availability = ProductAvailability.Available
            },
            new ProductResponse
            {
                Id = 2,
                Name = "Product 2",
                BasePrice = 15.00m,
                Availability = ProductAvailability.SoldOut
            }
        };

        _mockProductService.Setup(service => service.GetProductsByStallIdAsync(stallId))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller.GetProductsByStall(stallId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var products = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductResponse>>().Subject;
        products.Should().HaveCount(2);
        products.First().Id.Should().Be(1);
        products.Last().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetAvailableProductsByStall_ValidStall_ReturnsOkWithAvailableProducts()
    {
        // Arrange
        var stallId = 1;
        var expectedProducts = new List<ProductResponse>
        {
            new ProductResponse
            {
                Id = 1,
                Name = "Available Product",
                BasePrice = 10.00m,
                Availability = ProductAvailability.Available
            }
        };

        _mockProductService.Setup(service => service.GetAvailableProductsByStallIdAsync(stallId))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller.GetAvailableProductsByStall(stallId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var products = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductResponse>>().Subject;
        products.Should().HaveCount(1);
        products.First().Name.Should().Be("Available Product");
        products.First().Availability.Should().Be(ProductAvailability.Available);
    }

    [Fact]
    public async Task GetProductsBySection_ValidSection_ReturnsOkWithProducts()
    {
        // Arrange
        var sectionId = 1;
        var expectedProducts = new List<ProductResponse>
        {
            new ProductResponse
            {
                Id = 1,
                Name = "Product in Section",
                BasePrice = 12.00m,
                Availability = ProductAvailability.Available
            }
        };

        _mockProductService.Setup(service => service.GetProductsBySectionIdAsync(sectionId))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller.GetProductsBySection(sectionId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var products = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductResponse>>().Subject;
        products.Should().HaveCount(1);
        products.First().Name.Should().Be("Product in Section");
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var stallId = 1;
        var request = new CreateProductRequest
        {
            Name = "New Product",
            BasePrice = 12.99m,
            Availability = ProductAvailability.Available,
            CategoryId = 1,
            SectionId = 1
        };

        var createdProduct = new ProductResponse
        {
            Id = 1,
            Name = request.Name,
            BasePrice = request.BasePrice,
            Availability = request.Availability
        };

        _mockProductService.Setup(service => service.CreateProductAsync(stallId, request))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(stallId, request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetProduct));
        createdResult.RouteValues!["productId"].Should().Be(1);
        
        var product = createdResult.Value.Should().BeOfType<ProductResponse>().Subject;
        product.Name.Should().Be("New Product");
        product.BasePrice.Should().Be(12.99m);
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var stallId = 1;
        var request = new CreateProductRequest
        {
            Name = "New Product",
            BasePrice = 12.99m,
            CategoryId = 1,
            SectionId = 1
        };

        _mockProductService.Setup(service => service.CreateProductAsync(stallId, request))
            .ReturnsAsync((ProductResponse?)null);

        // Act
        var result = await _controller.CreateProduct(stallId, request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be($"Unable to create product. Check that stall {stallId} exists, and that the section and category belong to the stall.");
    }

    [Fact]
    public async Task UpdateProduct_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            BasePrice = 15.99m,
            Availability = ProductAvailability.Available,
            CategoryId = 1,
            SectionId = 1
        };

        var updatedProduct = new ProductResponse
        {
            Id = productId,
            Name = request.Name,
            BasePrice = request.BasePrice,
            Availability = request.Availability
        };

        _mockProductService.Setup(service => service.UpdateProductAsync(productId, request))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var product = okResult.Value.Should().BeOfType<ProductResponse>().Subject;
        product.Id.Should().Be(productId);
        product.Name.Should().Be("Updated Product");
        product.BasePrice.Should().Be(15.99m);
    }

    [Fact]
    public async Task UpdateProduct_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            BasePrice = 15.99m,
            CategoryId = 1,
            SectionId = 1
        };

        _mockProductService.Setup(service => service.UpdateProductAsync(productId, request))
            .ReturnsAsync((ProductResponse?)null);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Product with ID {productId} not found");
    }

    [Fact]
    public async Task UpdateProductBasics_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductBasicsRequest
        {
            Name = "Updated Product Name",
            BasePrice = 18.99m
        };

        var updatedProduct = new ProductResponse
        {
            Id = productId,
            Name = request.Name,
            BasePrice = request.BasePrice
        };

        _mockProductService.Setup(service => service.UpdateProductBasicsAsync(productId, request))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProductBasics(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var product = okResult.Value.Should().BeOfType<ProductResponse>().Subject;
        product.Name.Should().Be("Updated Product Name");
        product.BasePrice.Should().Be(18.99m);
    }

    [Fact]
    public async Task UpdateProductAvailability_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductAvailabilityRequest
        {
            Availability = ProductAvailability.SoldOut
        };

        var updatedProduct = new ProductResponse
        {
            Id = productId,
            Name = "Test Product",
            Availability = ProductAvailability.SoldOut
        };

        _mockProductService.Setup(service => service.UpdateProductAvailabilityAsync(productId, request))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProductAvailability(productId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var product = okResult.Value.Should().BeOfType<ProductResponse>().Subject;
        product.Availability.Should().Be(ProductAvailability.SoldOut);
    }

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var productId = 1;
        _mockProductService.Setup(service => service.DeleteProductAsync(productId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteProduct_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var productId = 1;
        _mockProductService.Setup(service => service.DeleteProductAsync(productId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Product with ID {productId} not found");
    }
} 