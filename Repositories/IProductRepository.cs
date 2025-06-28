using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByStallIdAsync(int stallId);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySectionIdAsync(int sectionId);
    Task<Product?> GetWithDetailsAsync(int productId);
} 