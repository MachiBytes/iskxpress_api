using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;

namespace iskxpress_api.Services.Mapping;

public static class ProductMappingExtensions
{
    public static ProductResponse ToProductResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            PictureId = product.PictureId,
            PictureUrl = product.Picture?.ObjectUrl,
            BasePrice = product.BasePrice,
            CalculatedMarkupPrice = Math.Ceiling(product.BasePrice * 1.1m),
            PriceWithMarkup = product.PriceWithMarkup,
            PriceWithDelivery = product.PriceWithDelivery,
            Availability = product.Availability,
            AvailabilityText = product.Availability.ToString(),
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            SectionId = product.SectionId,
            SectionName = product.Section?.Name ?? string.Empty,
            StallId = product.StallId,
            StallName = product.Stall?.Name ?? string.Empty,
            CreatedAt = DateTime.UtcNow, // TODO: Add CreatedAt to Product model if needed
            UpdatedAt = DateTime.UtcNow  // TODO: Add UpdatedAt to Product model if needed
        };
    }

    public static ProductResponse ToResponse(this Product product)
    {
        return product.ToProductResponse();
    }

    public static Product ToEntity(this CreateProductRequest request)
    {
        return new Product
        {
            Name = request.Name,
            PictureId = request.PictureId,
            BasePrice = request.BasePrice,
            PriceWithMarkup = CalculateMarkupPrice(request.BasePrice),
            PriceWithDelivery = CalculateDeliveryPrice(request.BasePrice),
            Availability = request.Availability,
            CategoryId = request.CategoryId,
            SectionId = request.SectionId,
            StallId = request.StallId
        };
    }

    public static VendorProductPricingResponse ToVendorProductPricingResponse(this Product product)
    {
        var calculatedMarkupPrice = Math.Ceiling(product.BasePrice * 1.1m);
        var markupAmount = calculatedMarkupPrice - product.BasePrice;

        return new VendorProductPricingResponse
        {
            Id = product.Id,
            Name = product.Name,
            BasePrice = product.BasePrice,
            CalculatedMarkupPrice = calculatedMarkupPrice,
            MarkupAmount = markupAmount,
            MarkupPercentage = 10.0m,
            Availability = product.Availability,
            AvailabilityText = product.Availability.ToString(),
            SectionName = product.Section?.Name ?? string.Empty,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }

    /// <summary>
    /// Calculates the markup price by adding 10% to the base price and rounding up using Math.Ceiling
    /// </summary>
    private static decimal CalculateMarkupPrice(decimal basePrice)
    {
        return Math.Ceiling(basePrice * 1.10m);
    }

    /// <summary>
    /// Calculates the delivery price by adding $3.00 to the markup price
    /// </summary>
    private static decimal CalculateDeliveryPrice(decimal basePrice)
    {
        return CalculateMarkupPrice(basePrice) + 3.00m;
    }
} 