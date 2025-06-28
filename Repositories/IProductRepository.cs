using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository interface for product operations
/// </summary>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>
    /// Gets products by category ID
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>Collection of products in the category</returns>
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);

    /// <summary>
    /// Gets products by stall ID
    /// </summary>
    /// <param name="stallId">The stall ID</param>
    /// <returns>Collection of products from the stall</returns>
    Task<IEnumerable<Product>> GetByStallIdAsync(int stallId);

    /// <summary>
    /// Gets products by section ID
    /// </summary>
    /// <param name="sectionId">The section ID</param>
    /// <returns>Collection of products in the section</returns>
    Task<IEnumerable<Product>> GetBySectionIdAsync(int sectionId);

    /// <summary>
    /// Searches products by name
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>Collection of products matching the search</returns>
    Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Gets products by price range
    /// </summary>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <returns>Collection of products within the price range</returns>
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Gets products with their related information
    /// </summary>
    /// <returns>Collection of products with navigation properties loaded</returns>
    Task<IEnumerable<Product>> GetAllWithDetailsAsync();

    /// <summary>
    /// Gets a product by ID with related information
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <returns>The product with navigation properties loaded, or null if not found</returns>
    Task<Product?> GetByIdWithDetailsAsync(int id);
} 