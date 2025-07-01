using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

/// <summary>
/// Repository implementation for stall section operations
/// </summary>
public class StallSectionRepository : GenericRepository<StallSection>, IStallSectionRepository
{
    public StallSectionRepository(IskExpressDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<StallSection>> GetAllAsync()
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .OrderBy(ss => ss.Name)
            .ToListAsync();
    }

    public override async Task<StallSection?> GetByIdAsync(int id)
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(ss => ss.Id == id);
    }

    public async Task<IEnumerable<StallSection>> GetByStallIdAsync(int stallId)
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .Where(ss => ss.StallId == stallId)
            .OrderBy(ss => ss.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<StallSection>> SearchByNameAsync(string searchTerm)
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .Where(ss => ss.Name.Contains(searchTerm))
            .OrderBy(ss => ss.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<StallSection>> GetAllWithDetailsAsync()
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .OrderBy(ss => ss.Name)
            .ToListAsync();
    }

    public async Task<StallSection?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.StallSections
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Vendor)
            .Include(ss => ss.Stall)
                .ThenInclude(s => s.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Picture)
            .Include(ss => ss.Products)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(ss => ss.Id == id);
    }
} 