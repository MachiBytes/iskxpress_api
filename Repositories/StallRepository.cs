using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class StallRepository : GenericRepository<Stall>, IStallRepository
{
    public StallRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Stall>> GetByVendorIdAsync(int vendorId)
    {
        return await _dbSet.Where(s => s.VendorId == vendorId).ToListAsync();
    }

    public async Task<Stall?> GetWithProductsAsync(int stallId)
    {
        return await _dbSet
            .Include(s => s.Products)
            .ThenInclude(p => p.Category)
            .Include(s => s.Products)
            .ThenInclude(p => p.Section)
            .FirstOrDefaultAsync(s => s.Id == stallId);
    }

    public async Task<Stall?> GetWithSectionsAsync(int stallId)
    {
        return await _dbSet
            .Include(s => s.StallSections)
            .FirstOrDefaultAsync(s => s.Id == stallId);
    }
} 