using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId);
    Task<IEnumerable<CartItem>> GetByStallIdAsync(int stallId);
    Task<CartItem?> GetByUserAndProductAsync(int userId, int productId);
    Task<bool> ClearCartByUserIdAsync(int userId);
    Task<bool> RemoveCartItemsByStallAsync(int userId, int stallId);
} 