using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class StallSectionRepository : GenericRepository<StallSection>, IStallSectionRepository
{
    public StallSectionRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StallSection>> GetByStallIdAsync(int stallId)
    {
        return await _dbSet.Where(ss => ss.StallId == stallId).ToListAsync();
    }
} 