using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;

namespace iskxpress_api.Services.Mapping;

public static class StallMappingExtensions
{
    public static StallResponse ToStallResponse(this Stall stall)
    {
        return new StallResponse
        {
            Id = stall.Id,
            Name = stall.Name,
            ShortDescription = stall.ShortDescription,
            PictureId = stall.PictureId,
            PictureUrl = stall.Picture?.ObjectUrl,
            VendorId = stall.VendorId,
            VendorName = stall.Vendor?.Name ?? string.Empty,
            DeliveryAvailable = stall.DeliveryAvailable,
            CreatedAt = DateTime.UtcNow, // TODO: Add CreatedAt to Stall model if needed
            UpdatedAt = DateTime.UtcNow, // TODO: Add UpdatedAt to Stall model if needed
            Categories = stall.Products
                .Where(p => p.Category != null)
                .Select(p => p.Category)
                .DistinctBy(c => c.Id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToList()
        };
    }
} 