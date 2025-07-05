using Xunit;
using FluentAssertions;
using iskxpress_api.Models;
using iskxpress_api.Services.Mapping;

namespace iskxpress_api.Tests;

public class ProductMappingExtensionsTests
{
    [Fact]
    public void ToVendorProductPricingResponse_ValidProduct_MapsCorrectly()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section" };
        var stall = new Stall { Id = 1, Name = "Test Stall" };
        
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            BasePrice = 12.99m,
            PriceWithMarkup = 15.99m, // Stored value (calculated as 12.99 + 10% = 14.289 -> 15.99)
            PriceWithDelivery = 17.99m,
            Availability = ProductAvailability.Available,
            CategoryId = category.Id,
            Category = category,
            SectionId = section.Id,
            Section = section,
            StallId = stall.Id,
            Stall = stall
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.BasePrice.Should().Be(12.99m);
        result.CalculatedMarkupPrice.Should().Be(15.99m); // Uses stored PriceWithMarkup value
        result.MarkupAmount.Should().Be(3.00m); // 15.99 - 12.99
        result.MarkupPercentage.Should().Be(10.0m);
        result.Availability.Should().Be(ProductAvailability.Available);
        result.AvailabilityText.Should().Be("Available");
        result.SectionName.Should().Be("Test Section");
        result.CategoryName.Should().Be("Test Category");
    }

    [Theory]
    [InlineData(10.00, 12.00, 2.00)] // Stored PriceWithMarkup = 12.00, MarkupAmount = 12.00 - 10.00
    [InlineData(10.50, 12.60, 2.10)] // Stored PriceWithMarkup = 12.60, MarkupAmount = 12.60 - 10.50
    [InlineData(12.99, 15.99, 3.00)] // Stored PriceWithMarkup = 15.99, MarkupAmount = 15.99 - 12.99
    [InlineData(18.99, 22.79, 3.80)] // Stored PriceWithMarkup = 22.79, MarkupAmount = 22.79 - 18.99
    [InlineData(25.25, 30.30, 5.05)] // Stored PriceWithMarkup = 30.30, MarkupAmount = 30.30 - 25.25
    [InlineData(9.09, 10.91, 1.82)]   // Stored PriceWithMarkup = 10.91, MarkupAmount = 10.91 - 9.09
    [InlineData(100.00, 120.00, 20.00)] // Stored PriceWithMarkup = 120.00, MarkupAmount = 120.00 - 100.00
    public void ToVendorProductPricingResponse_VariousBasePrices_UsesStoredValues(
        decimal basePrice, 
        decimal storedMarkupPrice, 
        decimal expectedMarkupAmount)
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section" };
        
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            BasePrice = basePrice,
            PriceWithMarkup = storedMarkupPrice, // Use stored values instead of calculating
            PriceWithDelivery = storedMarkupPrice + 10.00m,
            Availability = ProductAvailability.Available,
            Category = category,
            Section = section
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.BasePrice.Should().Be(basePrice);
        result.CalculatedMarkupPrice.Should().Be(storedMarkupPrice); // Uses stored value
        result.MarkupAmount.Should().Be(expectedMarkupAmount);
        result.MarkupPercentage.Should().Be(10.0m);
    }

    [Fact]
    public void ToVendorProductPricingResponse_SoldOutProduct_MapsAvailabilityCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Sold Out Product",
            BasePrice = 15.99m,
            PriceWithMarkup = 18.99m, // Stored value
            Availability = ProductAvailability.SoldOut,
            Category = new Category { Name = "Test Category" },
            Section = new StallSection { Name = "Test Section" }
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.Availability.Should().Be(ProductAvailability.SoldOut);
        result.AvailabilityText.Should().Be("SoldOut");
    }

    [Fact]
    public void ToVendorProductPricingResponse_NullNavigationProperties_HandlesGracefully()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            BasePrice = 12.99m,
            PriceWithMarkup = 15.99m, // Stored value
            Availability = ProductAvailability.Available,
            Category = null, // Null navigation property
            Section = null   // Null navigation property
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.Should().NotBeNull();
        result.CategoryName.Should().Be(string.Empty);
        result.SectionName.Should().Be(string.Empty);
        result.BasePrice.Should().Be(12.99m);
        result.CalculatedMarkupPrice.Should().Be(15.99m); // Uses stored value
    }

    [Fact]
    public void ToProductResponse_ValidProduct_IncludesCalculatedMarkupPrice()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test Category" };
        var section = new StallSection { Id = 1, Name = "Test Section" };
        var stall = new Stall { Id = 1, Name = "Test Stall" };
        var picture = new FileRecord { Id = 1, ObjectUrl = "https://example.com/image.jpg" };
        
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            PictureId = picture.Id,
            Picture = picture,
            BasePrice = 12.99m,
            PriceWithMarkup = 15.99m,
            PriceWithDelivery = 17.99m,
            Availability = ProductAvailability.Available,
            CategoryId = category.Id,
            Category = category,
            SectionId = section.Id,
            Section = section,
            StallId = stall.Id,
            Stall = stall
        };

        // Act
        var result = product.ToProductResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.PictureId.Should().Be(1);
        result.PictureUrl.Should().Be("https://example.com/image.jpg");
        result.BasePrice.Should().Be(12.99m);
        result.CalculatedMarkupPrice.Should().Be(15.99m); // Uses stored PriceWithMarkup value
        result.PriceWithMarkup.Should().Be(15.99m); // Original stored value
        result.PriceWithDelivery.Should().Be(17.99m);
        result.Availability.Should().Be(ProductAvailability.Available);
        result.AvailabilityText.Should().Be("Available");
        result.CategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Test Category");
        result.SectionId.Should().Be(1);
        result.SectionName.Should().Be("Test Section");
        result.StallId.Should().Be(1);
        result.StallName.Should().Be("Test Stall");
    }

    [Fact]
    public void ToProductResponse_NullPicture_HandlesGracefully()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            PictureId = null,
            Picture = null,
            BasePrice = 10.00m,
            PriceWithMarkup = 12.00m, // Stored value
            PriceWithDelivery = 14.00m,
            Availability = ProductAvailability.Available,
            Category = new Category { Name = "Test Category" },
            Section = new StallSection { Name = "Test Section" },
            Stall = new Stall { Name = "Test Stall" }
        };

        // Act
        var result = product.ToProductResponse();

        // Assert
        result.PictureId.Should().BeNull();
        result.PictureUrl.Should().BeNull();
        result.CalculatedMarkupPrice.Should().Be(12.00m); // Uses stored PriceWithMarkup value
    }

    [Theory]
    [InlineData(0.01, 1.00)]   // Stored PriceWithMarkup = 1.00
    [InlineData(0.91, 2.00)]   // Stored PriceWithMarkup = 2.00
    [InlineData(999.99, 1100.00)] // Stored PriceWithMarkup = 1100.00
    public void CalculatedMarkupPrice_EdgeCases_UsesStoredValues(decimal basePrice, decimal storedMarkupPrice)
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Edge Case Product",
            BasePrice = basePrice,
            PriceWithMarkup = storedMarkupPrice, // Use stored values
            Availability = ProductAvailability.Available
        };

        // Act
        var productResponse = product.ToVendorProductPricingResponse();

        // Assert
        productResponse.CalculatedMarkupPrice.Should().Be(storedMarkupPrice);
    }

    [Fact]
    public void ToVendorProductPricingResponse_ZeroBasePrice_HandlesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Free Product",
            BasePrice = 0.00m,
            PriceWithMarkup = 0.00m, // Stored value
            Availability = ProductAvailability.Available,
            Category = new Category { Name = "Free Items" },
            Section = new StallSection { Name = "Promotions" }
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.BasePrice.Should().Be(0.00m);
        result.CalculatedMarkupPrice.Should().Be(0.00m); // Uses stored value
        result.MarkupAmount.Should().Be(0.00m);
        result.MarkupPercentage.Should().Be(0.0m);
    }
} 