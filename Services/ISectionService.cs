using iskxpress_api.DTOs.Vendors;

namespace iskxpress_api.Services;

public interface ISectionService
{
    Task<SectionResponse?> GetSectionByIdAsync(int sectionId);
    Task<IEnumerable<SectionResponse>> GetSectionsByStallIdAsync(int stallId);
    Task<SectionResponse?> CreateSectionAsync(int stallId, CreateSectionRequest request);
    Task<SectionResponse?> UpdateSectionAsync(int sectionId, UpdateSectionRequest request);
    Task<bool> DeleteSectionAsync(int sectionId);
} 