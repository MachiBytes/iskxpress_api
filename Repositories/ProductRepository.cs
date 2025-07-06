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

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByStallIdAsync(int stallId)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .Where(p => p.StallId == stallId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySectionIdAsync(int sectionId)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .Where(p => p.SectionId == sectionId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .Where(p => p.Name.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .Where(p => p.PriceWithMarkup >= minPrice && p.PriceWithMarkup <= maxPrice)
            .ToListAsync();
        
        return products.OrderBy(p => p.PriceWithMarkup);
    }

    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Picture)
            .Include(p => p.Category)
            .Include(p => p.Section)
                .ThenInclude(s => s.Stall)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Stall)
                .ThenInclude(s => s.Picture)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
} 