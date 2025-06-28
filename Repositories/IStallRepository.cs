using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IStallRepository : IGenericRepository<Stall>
{
    Task<IEnumerable<Stall>> GetByVendorIdAsync(int vendorId);
    Task<Stall?> GetWithProductsAsync(int stallId);
    Task<Stall?> GetWithSectionsAsync(int stallId);
} 