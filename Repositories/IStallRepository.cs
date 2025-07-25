using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for stall operations
/// </summary>
public interface IStallRepository : IGenericRepository<Stall>
{
    /// <summary>
    /// Gets the stall owned by a vendor
    /// </summary>
    /// <param name="vendorId">The vendor ID</param>
    /// <returns>The stall owned by the vendor, or null if none exists</returns>
    Task<Stall?> GetByVendorIdAsync(int vendorId);

    /// <summary>
    /// Searches stalls by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>Collection of stalls matching the search</returns>
    Task<IEnumerable<Stall>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Gets stalls with their related information
    /// </summary>
    /// <returns>Collection of stalls with navigation properties loaded</returns>
    Task<IEnumerable<Stall>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a stall by ID with related information
    /// </summary>
    /// <param name="id">The stall ID</param>
    /// <returns>The stall with navigation properties loaded, or null if not found</returns>
    Task<Stall?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Gets stalls that have products matching the search term
    /// </summary>
    /// <param name="productSearchTerm">The product name search term</param>
    /// <returns>Collection of stalls that have products matching the search term</returns>
    Task<IEnumerable<Stall>> GetStallsByProductNameAsync(string productSearchTerm);

    /// <summary>
    /// Searches stalls by name or by their products' names
    /// </summary>
    /// <param name="searchTerm">The search term to match against stall names or product names</param>
    /// <returns>Collection of stalls matching the search term</returns>
    Task<IEnumerable<Stall>> SearchStallsAsync(string searchTerm);

    /// <summary>
    /// Updates the delivery availability for a stall
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <param name="deliveryAvailable">Whether delivery is currently available</param>
    /// <returns>The updated stall</returns>
    Task<Stall> UpdateDeliveryAvailabilityAsync(int stallId, bool deliveryAvailable);
} 