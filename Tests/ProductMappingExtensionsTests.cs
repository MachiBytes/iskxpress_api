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
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.BasePrice.Should().Be(12.99m);
        result.CalculatedMarkupPrice.Should().Be(15.00m); // 12.99 + (12.99 * 0.10) = 14.289 -> 15.00
        result.MarkupAmount.Should().Be(2.01m); // 15.00 - 12.99
        result.MarkupPercentage.Should().Be(10.0m);
        result.Availability.Should().Be(ProductAvailability.Available);
        result.AvailabilityText.Should().Be("Available");
        result.SectionName.Should().Be("Test Section");
        result.CategoryName.Should().Be("Test Category");
    }

    [Theory]
    [InlineData(10.00, 11.00, 1.00)] // 10.00 + (10.00 * 0.10) = 11.00
    [InlineData(10.50, 12.00, 1.50)] // 10.50 + (10.50 * 0.10) = 11.55 -> 12.00
    [InlineData(12.99, 15.00, 2.01)] // 12.99 + (12.99 * 0.10) = 14.289 -> 15.00
    [InlineData(18.99, 21.00, 2.01)] // 18.99 + (18.99 * 0.10) = 20.889 -> 21.00
    [InlineData(25.25, 28.00, 2.75)] // 25.25 + (25.25 * 0.10) = 27.775 -> 28.00
    [InlineData(9.09, 10.00, 0.91)]   // 9.09 + (9.09 * 0.10) = 9.999 -> 10.00
    [InlineData(100.00, 110.00, 10.00)] // 100.00 + (100.00 * 0.10) = 110.00
    public void ToVendorProductPricingResponse_VariousBasePrices_CalculatesMarkupCorrectly(
        decimal basePrice, 
        decimal expectedMarkupPrice, 
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
            PriceWithMarkup = basePrice * 1.2m, // Different markup for testing
            PriceWithDelivery = basePrice * 1.3m,
            Availability = ProductAvailability.Available,
            Category = category,
            Section = section
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.BasePrice.Should().Be(basePrice);
        result.CalculatedMarkupPrice.Should().Be(expectedMarkupPrice);
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
        result.CalculatedMarkupPrice.Should().Be(15.00m);
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
        result.CalculatedMarkupPrice.Should().Be(15.00m); // 12.99 + (12.99 * 0.10) = 14.289 -> 15.00
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
            PriceWithMarkup = 12.00m,
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
        result.CalculatedMarkupPrice.Should().Be(11.00m); // 10.00 + (10.00 * 0.10) = 11.00
    }

    [Theory]
    [InlineData(0.01, 1.00)]   // 0.01 + (0.01 * 0.10) = 0.011 -> 1.00
    [InlineData(0.91, 2.00)]   // 0.91 + (0.91 * 0.10) = 1.001 -> 2.00 (round up to next peso)
    [InlineData(999.99, 1100.00)] // 999.99 + (999.99 * 0.10) = 1099.989 -> 1100.00
    public void CalculatedMarkupPrice_EdgeCases_CalculatesCorrectly(decimal basePrice, decimal expectedMarkup)
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Edge Case Product",
            BasePrice = basePrice,
            Availability = ProductAvailability.Available
        };

        // Act
        var productResponse = product.ToVendorProductPricingResponse();

        // Assert
        productResponse.CalculatedMarkupPrice.Should().Be(expectedMarkup);
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
            Availability = ProductAvailability.Available,
            Category = new Category { Name = "Free Items" },
            Section = new StallSection { Name = "Promotions" }
        };

        // Act
        var result = product.ToVendorProductPricingResponse();

        // Assert
        result.BasePrice.Should().Be(0.00m);
        result.CalculatedMarkupPrice.Should().Be(0.00m); // 0.00 + (0.00 * 0.10) = 0.00
        result.MarkupAmount.Should().Be(0.00m);
        result.MarkupPercentage.Should().Be(0.0m);
    }
} 