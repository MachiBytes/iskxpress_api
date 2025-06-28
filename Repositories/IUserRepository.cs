using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetByAuthProviderAsync(AuthProvider authProvider);
    Task<bool> EmailExistsAsync(string email);
} 