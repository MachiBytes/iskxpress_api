using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for category operations
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId)
    {
        return await _context.Categories
            .Where(c => c.VendorId == vendorId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Categories
            .Where(c => c.Name.Contains(searchTerm))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetAllWithDetailsAsync()
    {
        return await _context.Categories
            .Include(c => c.Vendor)
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Vendor)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
} 