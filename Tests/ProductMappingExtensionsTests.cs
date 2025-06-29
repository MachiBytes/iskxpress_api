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
        result.CalculatedMarkupPrice.Should().Be(15.00m); // Math.Ceiling(12.99 * 1.1) = 15.00
        result.MarkupAmount.Should().Be(2.01m); // 15.00 - 12.99
        result.MarkupPercentage.Should().Be(10.0m);
        result.Availability.Should().Be(ProductAvailability.Available);
        result.AvailabilityText.Should().Be("Available");
        result.SectionName.Should().Be("Test Section");
        result.CategoryName.Should().Be("Test Category");
    }

    [Theory]
    [InlineData(10.00, 11.00, 1.00)] // 10.00 * 1.1 = 11.00 (exact)
    [InlineData(10.50, 12.00, 1.50)] // 10.50 * 1.1 = 11.55 -> rounded up to 12.00
    [InlineData(12.99, 15.00, 2.01)] // 12.99 * 1.1 = 14.289 -> rounded up to 15.00
    [InlineData(18.99, 21.00, 2.01)] // 18.99 * 1.1 = 20.889 -> rounded up to 21.00
    [InlineData(25.25, 28.00, 2.75)] // 25.25 * 1.1 = 27.775 -> rounded up to 28.00
    [InlineData(9.09, 10.00, 0.91)]  // 9.09 * 1.1 = 9.999 -> rounded up to 10.00
    [InlineData(100.00, 110.00, 10.00)] // 100.00 * 1.1 = 110.00 (exact)
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
        result.CalculatedMarkupPrice.Should().Be(15.00m); // Math.Ceiling(12.99 * 1.1)
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
        result.CalculatedMarkupPrice.Should().Be(11.00m); // Math.Ceiling(10.00 * 1.1)
    }

    [Theory]
    [InlineData(0.01, 1.00)]   // Very small price: 0.01 * 1.1 = 0.011 -> rounded up to 1.00
    [InlineData(0.91, 2.00)]   // 0.91 * 1.1 = 1.001 -> rounded up to 2.00
    [InlineData(999.99, 1100.00)] // Large price: 999.99 * 1.1 = 1099.989 -> rounded up to 1100.00
    public void CalculatedMarkupPrice_EdgeCases_CalculatesCorrectly(decimal basePrice, decimal expectedMarkup)
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            BasePrice = basePrice,
            Availability = ProductAvailability.Available,
            Category = new Category { Name = "Test Category" },
            Section = new StallSection { Name = "Test Section" },
            Stall = new Stall { Name = "Test Stall" }
        };

        // Act
        var productResponse = product.ToProductResponse();
        var pricingResponse = product.ToVendorProductPricingResponse();

        // Assert
        productResponse.CalculatedMarkupPrice.Should().Be(expectedMarkup);
        pricingResponse.CalculatedMarkupPrice.Should().Be(expectedMarkup);
        pricingResponse.MarkupAmount.Should().Be(expectedMarkup - basePrice);
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
        result.CalculatedMarkupPrice.Should().Be(0.00m); // Math.Ceiling(0.00 * 1.1) = 0.00
        result.MarkupAmount.Should().Be(0.00m);
        result.MarkupPercentage.Should().Be(10.0m);
    }
} 