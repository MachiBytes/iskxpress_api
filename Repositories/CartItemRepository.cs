using Microsoft.EntityFrameworkCore;
using iskxpress_api.Data;
using iskxpress_api.Models;

namespace iskxpress_api.Repositories;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(IskExpressDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Category)
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Section)
            .Include(ci => ci.Stall)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CartItem>> GetByStallIdAsync(int stallId)
    {
        return await _dbSet
            .Include(ci => ci.Product)
            .Include(ci => ci.User)
            .Where(ci => ci.StallId == stallId)
            .ToListAsync();
    }

    public async Task<CartItem?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _dbSet
            .Include(ci => ci.Product)
            .Include(ci => ci.Stall)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
    }

    public async Task<bool> ClearCartByUserIdAsync(int userId)
    {
        var cartItems = await _dbSet.Where(ci => ci.UserId == userId).ToListAsync();
        if (cartItems.Any())
        {
            _dbSet.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> RemoveCartItemsByStallAsync(int userId, int stallId)
    {
        var cartItems = await _dbSet
            .Where(ci => ci.UserId == userId && ci.StallId == stallId)
            .ToListAsync();
        
        if (cartItems.Any())
        {
            _dbSet.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
        return true;
    }
} 