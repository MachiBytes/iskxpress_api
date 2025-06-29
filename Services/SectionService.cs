using iskxpress_api.DTOs.Vendors;
using iskxpress_api.Models;
using iskxpress_api.Repositories;
using iskxpress_api.Services.Mapping;

namespace iskxpress_api.Services;

public class SectionService : ISectionService
{
    private readonly IStallSectionRepository _sectionRepository;
    private readonly IStallRepository _stallRepository;

    public SectionService(IStallSectionRepository sectionRepository, IStallRepository stallRepository)
    {
        _sectionRepository = sectionRepository;
        _stallRepository = stallRepository;
    }

    public async Task<SectionResponse?> GetSectionByIdAsync(int sectionId)
    {
        var section = await _sectionRepository.GetByIdWithDetailsAsync(sectionId);
        return section?.ToSectionResponse();
    }

    public async Task<IEnumerable<SectionResponse>> GetSectionsByStallIdAsync(int stallId)
    {
        var sections = await _sectionRepository.GetByStallIdAsync(stallId);
        return sections.Select(s => s.ToSectionResponse());
    }

    public async Task<SectionResponse?> CreateSectionAsync(int stallId, CreateSectionRequest request)
    {
        // Verify that the stall exists
        var stall = await _stallRepository.GetByIdAsync(stallId);
        if (stall == null)
        {
            return null; // Stall doesn't exist
        }

        var newSection = new StallSection
        {
            Name = request.Name,
            StallId = stallId
        };

        var createdSection = await _sectionRepository.AddAsync(newSection);
        
        // Load the stall information for the response
        createdSection.Stall = stall;
        
        return createdSection.ToSectionResponse();
    }

    public async Task<SectionResponse?> UpdateSectionAsync(int sectionId, UpdateSectionRequest request)
    {
        var section = await _sectionRepository.GetByIdWithDetailsAsync(sectionId);
        if (section == null)
        {
            return null;
        }

        section.Name = request.Name;
        var updatedSection = await _sectionRepository.UpdateAsync(section);
        
        return updatedSection.ToSectionResponse();
    }

    public async Task<bool> DeleteSectionAsync(int sectionId)
    {
        var section = await _sectionRepository.GetByIdAsync(sectionId);
        if (section == null)
        {
            return false;
        }

        return await _sectionRepository.DeleteAsync(sectionId);
    }
} 