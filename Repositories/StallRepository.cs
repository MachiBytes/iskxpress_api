using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for stall operations
/// </summary>
public class StallRepository : GenericRepository<Stall>, IStallRepository
{
    public StallRepository(IskExpressDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Stall>> GetAllAsync()
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public override async Task<Stall?> GetByIdAsync(int id)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Stall?> GetByVendorIdAsync(int vendorId)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);
    }

    public async Task<IEnumerable<Stall>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Where(s => s.Name.Contains(searchTerm))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Stall>> GetAllWithDetailsAsync()
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.StallSections)
            .Include(s => s.Products)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Stall?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.StallSections)
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);
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