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
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public override async Task<Stall?> GetByIdAsync(int id)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Stall?> GetByVendorIdAsync(int vendorId)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);
    }

    public async Task<IEnumerable<Stall>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
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
                .ThenInclude(p => p.Category)
            .Include(s => s.Products)
                .ThenInclude(p => p.Section)
            .Include(s => s.Products)
                .ThenInclude(p => p.Picture)
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
                .ThenInclude(p => p.Category)
            .Include(s => s.Products)
                .ThenInclude(p => p.Section)
            .Include(s => s.Products)
                .ThenInclude(p => p.Picture)
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

    public async Task<IEnumerable<Stall>> GetStallsByProductNameAsync(string productSearchTerm)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.StallSections)
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .Include(s => s.Products)
                .ThenInclude(p => p.Section)
            .Include(s => s.Products)
                .ThenInclude(p => p.Picture)
            .Where(s => s.Products.Any(p => p.Name.Contains(productSearchTerm)))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Stall>> SearchStallsAsync(string searchTerm)
    {
        return await _context.Stalls
            .Include(s => s.Vendor)
            .Include(s => s.Picture)
            .Include(s => s.StallSections)
            .Include(s => s.Products)
                .ThenInclude(p => p.Category)
            .Include(s => s.Products)
                .ThenInclude(p => p.Section)
            .Include(s => s.Products)
                .ThenInclude(p => p.Picture)
            .Where(s => s.Name.Contains(searchTerm) || 
                       s.Products.Any(p => p.Name.Contains(searchTerm)))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Stall> UpdateDeliveryAvailabilityAsync(int stallId, bool hasDelivery, bool deliveryAvailable)
    {
        var stall = await _context.Stalls.FindAsync(stallId);
        if (stall == null)
        {
            throw new ArgumentException($"Stall with ID {stallId} not found");
        }

        stall.hasDelivery = hasDelivery;
        stall.DeliveryAvailable = deliveryAvailable;

        await _context.SaveChangesAsync();
        return stall;
    }
} 