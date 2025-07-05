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
            CalculatedMarkupPrice = product.PriceWithMarkup, // Use stored value instead of calculating
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
        // Calculate prices using the same logic as ProductService
        var markupPrice = Math.Ceiling(request.BasePrice + (request.BasePrice * 0.10m));
        var deliveryPrice = Math.Ceiling(markupPrice + 10.00m);
        
        return new Product
        {
            Name = request.Name,
            PictureId = null, // Picture will be set via upload endpoint
            BasePrice = request.BasePrice,
            PriceWithMarkup = markupPrice,
            PriceWithDelivery = deliveryPrice,
            Availability = ProductAvailability.Available, // Default to available
            CategoryId = request.CategoryId,
            SectionId = request.SectionId
            // StallId will be set by the service layer from the path parameter
        };
    }

    public static VendorProductPricingResponse ToVendorProductPricingResponse(this Product product)
    {
        var markupAmount = product.PriceWithMarkup - product.BasePrice;
        var markupPercentage = product.BasePrice == 0 ? 0.0m : 10.0m;

        return new VendorProductPricingResponse
        {
            Id = product.Id,
            Name = product.Name,
            BasePrice = product.BasePrice,
            CalculatedMarkupPrice = product.PriceWithMarkup, // Use stored value instead of calculating
            MarkupAmount = markupAmount,
            MarkupPercentage = markupPercentage,
            Availability = product.Availability,
            AvailabilityText = product.Availability.ToString(),
            SectionName = product.Section?.Name ?? string.Empty,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }


} 