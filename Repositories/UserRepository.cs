using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByAuthProviderAsync(AuthProvider authProvider)
    {
        return await _dbSet.Where(u => u.AuthProvider == authProvider).ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
} 