using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId)
    {
        return await _dbSet.Where(c => c.VendorId == vendorId).ToListAsync();
    }
} 