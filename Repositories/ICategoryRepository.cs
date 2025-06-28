using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for category operations
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>
    /// Gets categories by vendor ID
    /// </summary>
    /// <param name="vendorId">The vendor ID</param>
    /// <returns>Collection of categories owned by the vendor</returns>
    Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId);

    /// <summary>
    /// Searches categories by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>Collection of categories matching the search</returns>
    Task<IEnumerable<Category>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Gets categories with their related information
    /// </summary>
    /// <returns>Collection of categories with navigation properties loaded</returns>
    Task<IEnumerable<Category>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a category by ID with related information
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <returns>The category with navigation properties loaded, or null if not found</returns>
    Task<Category?> GetByIdWithDetailsAsync(int id);
} 