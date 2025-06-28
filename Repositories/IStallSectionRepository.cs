using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IStallSectionRepository : IGenericRepository<StallSection>
{
    Task<IEnumerable<StallSection>> GetByStallIdAsync(int stallId);
} 