using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;

namespace iskxpress_api.Services.Mapping;

public static class SectionMappingExtensions
{
    public static SectionResponse ToSectionResponse(this StallSection section)
    {
        return new SectionResponse
        {
            Id = section.Id,
            Name = section.Name,
            StallId = section.StallId,
            StallName = section.Stall?.Name ?? string.Empty,
            ProductCount = section.Products?.Count ?? 0,
            CreatedAt = DateTime.UtcNow, // TODO: Add CreatedAt to StallSection model if needed
            UpdatedAt = DateTime.UtcNow  // TODO: Add UpdatedAt to StallSection model if needed
        };
    }
} 