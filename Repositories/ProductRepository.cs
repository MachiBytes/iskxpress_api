using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByStallIdAsync(int stallId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Where(p => p.StallId == stallId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Include(p => p.Stall)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySectionIdAsync(int sectionId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Include(p => p.Stall)
            .Where(p => p.SectionId == sectionId)
            .ToListAsync();
    }

    public async Task<Product?> GetWithDetailsAsync(int productId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Include(p => p.Stall)
            .ThenInclude(s => s.Vendor)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
} 