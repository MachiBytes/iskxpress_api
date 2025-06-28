using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for product operations
/// </summary>
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByStallIdAsync(int stallId)
    {
        return await _context.Products
            .Where(p => p.StallId == stallId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySectionIdAsync(int sectionId)
    {
        return await _context.Products
            .Where(p => p.SectionId == sectionId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Products
            .Where(p => p.Name.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await _context.Products
            .Where(p => p.PriceWithDelivery >= minPrice && p.PriceWithDelivery <= maxPrice)
            .ToListAsync();
        
        return products.OrderBy(p => p.PriceWithDelivery);
    }

    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Include(p => p.Stall)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Section)
            .Include(p => p.Stall)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
} 