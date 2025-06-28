using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for stall section operations
/// </summary>
public interface IStallSectionRepository : IGenericRepository<StallSection>
{
    /// <summary>
    /// Gets stall sections by stall ID
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of sections for the stall</returns>
    Task<IEnumerable<StallSection>> GetByStallIdAsync(int stallId);

    /// <summary>
    /// Searches stall sections by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>Collection of stall sections matching the search</returns>
    Task<IEnumerable<StallSection>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Gets stall sections with their related information
    /// </summary>
    /// <returns>Collection of stall sections with navigation properties loaded</returns>
    Task<IEnumerable<StallSection>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a stall section by ID with related information
    /// </summary>
    /// <param name="id">The stall section ID</param>
    /// <returns>The stall section with navigation properties loaded, or null if not found</returns>
    Task<StallSection?> GetByIdWithDetailsAsync(int id);
} 