using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId);
} 